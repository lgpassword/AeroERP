using System.Text.Json;
using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Reporting.Contracts;
using AeroERP.Modules.Reporting.Domain;
using AeroERP.Modules.Reporting.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Reporting Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class ReportingService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IReportingService
{
    /// <summary>
    /// Allowed Query Models。
    /// </summary>
    private static readonly HashSet<string> AllowedQueryModels =
    [
        "operations-summary",
        "procurement-summary",
        "sales-summary",
        "inventory-summary",
        "finance-summary",
        "manufacturing-summary"
    ];

    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<ReportingOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var definitions = await dbContext.ReportDefinitions.AsNoTracking().OrderBy(x => x.Category).ThenBy(x => x.Key).ToListAsync(cancellationToken);
        var runs = (await dbContext.ReportRunRecords.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var exportTasks = (await dbContext.ReportExportTasks.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var metrics = await BuildOperationsSummaryAsync(cancellationToken);

        return new ReportingOverviewDto(
            definitions.Select(MapDefinition).ToList(),
            runs.Select(MapRun).ToList(),
            exportTasks.Select(MapExportTask).ToList(),
            metrics);
    }

    /// <summary>
    /// Upsert Definition Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ReportDefinitionDto>> UpsertDefinitionAsync(UpsertReportDefinitionRequest request, CancellationToken cancellationToken)
    {
        var key = NormalizeKey(request.Key);
        var displayName = NormalizeText(request.DisplayName);
        var category = NormalizeText(request.Category);
        var queryModel = NormalizeKey(request.QueryModel);
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(queryModel))
        {
            return OperationResult<ReportDefinitionDto>.Failure("报表编码、名称和查询模型不能为空。");
        }

        if (!AllowedQueryModels.Contains(queryModel))
        {
            return OperationResult<ReportDefinitionDto>.Failure("不支持的报表查询模型。");
        }

        string parametersJson;
        try
        {
            parametersJson = NormalizeJson(request.ParametersJson);
        }
        catch (JsonException)
        {
            return OperationResult<ReportDefinitionDto>.Failure("参数 JSON 格式无效。");
        }
        var actor = currentUser.GetActor();
        var definition = await dbContext.ReportDefinitions.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (definition is null)
        {
            definition = new ReportDefinition(key, displayName, category, queryModel, parametersJson, request.IsEnabled, actor);
            dbContext.ReportDefinitions.Add(definition);
        }
        else
        {
            definition.Update(displayName, category, queryModel, parametersJson, request.IsEnabled, actor);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Reporting", "DefinitionUpserted", actor, key, cancellationToken);
        return OperationResult<ReportDefinitionDto>.Success(MapDefinition(definition));
    }

    /// <summary>
    /// Run Report Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ReportRunRecordDto>> RunReportAsync(RunReportRequest request, CancellationToken cancellationToken)
    {
        var definition = await dbContext.ReportDefinitions.FirstOrDefaultAsync(x => x.Id == request.ReportDefinitionId, cancellationToken);
        if (definition is null || !definition.IsEnabled)
        {
            return OperationResult<ReportRunRecordDto>.Failure("报表定义不存在或已停用。");
        }

        var metrics = definition.QueryModel switch
        {
            "operations-summary" => await BuildOperationsSummaryAsync(cancellationToken),
            "procurement-summary" => await BuildProcurementSummaryAsync(cancellationToken),
            "sales-summary" => await BuildSalesSummaryAsync(cancellationToken),
            "inventory-summary" => await BuildInventorySummaryAsync(cancellationToken),
            "finance-summary" => await BuildFinanceSummaryAsync(cancellationToken),
            "manufacturing-summary" => await BuildManufacturingSummaryAsync(cancellationToken),
            _ => null
        };

        if (metrics is null)
        {
            return OperationResult<ReportRunRecordDto>.Failure("不支持的报表查询模型。");
        }

        string parametersJson;
        try
        {
            parametersJson = NormalizeJson(request.ParametersJson);
        }
        catch (JsonException)
        {
            return OperationResult<ReportRunRecordDto>.Failure("运行参数 JSON 格式无效。");
        }

        var actor = currentUser.GetActor();
        var run = new ReportRunRecord(NextNo("RR"), definition.Id, definition.Key, definition.DisplayName, parametersJson, actor);
        run.Complete(JsonSerializer.Serialize(metrics), metrics.Count);
        dbContext.ReportRunRecords.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Reporting", "ReportRun", actor, run.RunNo, cancellationToken);
        return OperationResult<ReportRunRecordDto>.Success(MapRun(run));
    }

    /// <summary>
    /// 创建Export Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ReportExportTaskDto>> CreateExportTaskAsync(CreateReportExportTaskRequest request, CancellationToken cancellationToken)
    {
        var run = await dbContext.ReportRunRecords.FirstOrDefaultAsync(x => x.Id == request.ReportRunRecordId, cancellationToken);
        if (run is null || run.Status != ReportingStatus.Completed)
        {
            return OperationResult<ReportExportTaskDto>.Failure("只能为已完成的报表运行记录创建导出任务。");
        }

        var format = NormalizeText(request.Format).ToUpperInvariant();
        if (format is not "CSV" and not "XLSX")
        {
            return OperationResult<ReportExportTaskDto>.Failure("导出格式仅支持 CSV 或 XLSX。");
        }

        var actor = currentUser.GetActor();
        var exportNo = NextNo("RE");
        var fileName = $"{run.ReportKey}-{exportNo}.{format.ToLowerInvariant()}";
        var task = new ReportExportTask(exportNo, run.Id, run.ReportName, format, fileName, actor);
        task.Complete();
        dbContext.ReportExportTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Reporting", "ExportTaskCreated", actor, exportNo, cancellationToken);
        return OperationResult<ReportExportTaskDto>.Success(MapExportTask(task));
    }

    /// <summary>
    /// Build Operations Summary Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<BusinessMetricDto>> BuildOperationsSummaryAsync(CancellationToken cancellationToken)
    {
        var procurement = await dbContext.ProcurementOrders.CountAsync(cancellationToken);
        var sales = await dbContext.SalesOrders.CountAsync(cancellationToken);
        var stockBalance = await dbContext.StockBalances.SumAsync(x => x.Quantity, cancellationToken);
        var workOrders = await dbContext.WorkOrders.CountAsync(cancellationToken);
        var payables = await dbContext.Payables.SumAsync(x => x.Amount - x.SettledAmount, cancellationToken);
        var receivables = await dbContext.Receivables.SumAsync(x => x.Amount - x.SettledAmount, cancellationToken);

        return
        [
            new("procurement-orders", "采购订单", procurement, "张"),
            new("sales-orders", "销售订单", sales, "张"),
            new("stock-balance", "库存结存", stockBalance, "数量"),
            new("work-orders", "制造工单", workOrders, "张"),
            new("open-payables", "未结应付", payables, "金额"),
            new("open-receivables", "未结应收", receivables, "金额")
        ];
    }

    /// <summary>
    /// Build Procurement Summary Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<BusinessMetricDto>> BuildProcurementSummaryAsync(CancellationToken cancellationToken) =>
    [
        new("requests", "采购申请", await dbContext.ProcurementRequests.CountAsync(cancellationToken), "张"),
        new("orders", "采购订单", await dbContext.ProcurementOrders.CountAsync(cancellationToken), "张"),
        new("receipts", "采购入库", await dbContext.InventoryReceipts.CountAsync(cancellationToken), "张")
    ];

    /// <summary>
    /// Build Sales Summary Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<BusinessMetricDto>> BuildSalesSummaryAsync(CancellationToken cancellationToken) =>
    [
        new("quotations", "销售报价", await dbContext.SalesQuotations.CountAsync(cancellationToken), "张"),
        new("orders", "销售订单", await dbContext.SalesOrders.CountAsync(cancellationToken), "张"),
        new("issues", "销售出库", await dbContext.InventoryIssues.CountAsync(cancellationToken), "张")
    ];

    /// <summary>
    /// Build Inventory Summary Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<BusinessMetricDto>> BuildInventorySummaryAsync(CancellationToken cancellationToken) =>
    [
        new("balances", "库存余额行", await dbContext.StockBalances.CountAsync(cancellationToken), "行"),
        new("quantity", "库存结存", await dbContext.StockBalances.SumAsync(x => x.Quantity, cancellationToken), "数量"),
        new("movements", "库存流水", await dbContext.InventoryMovements.CountAsync(cancellationToken), "条")
    ];

    /// <summary>
    /// Build Finance Summary Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<BusinessMetricDto>> BuildFinanceSummaryAsync(CancellationToken cancellationToken) =>
    [
        new("payables", "应付单", await dbContext.Payables.CountAsync(cancellationToken), "张"),
        new("receivables", "应收单", await dbContext.Receivables.CountAsync(cancellationToken), "张"),
        new("settlements", "结算记录", await dbContext.Settlements.CountAsync(cancellationToken), "条")
    ];

    /// <summary>
    /// Build Manufacturing Summary Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<BusinessMetricDto>> BuildManufacturingSummaryAsync(CancellationToken cancellationToken) =>
    [
        new("boms", "BOM", await dbContext.BillOfMaterials.CountAsync(cancellationToken), "份"),
        new("work-orders", "制造工单", await dbContext.WorkOrders.CountAsync(cancellationToken), "张"),
        new("production-issues", "生产领料", await dbContext.ProductionIssues.CountAsync(cancellationToken), "张"),
        new("production-receipts", "完工入库", await dbContext.ProductionReceipts.CountAsync(cancellationToken), "张")
    ];

    /// <summary>
    /// Next No。
    /// </summary>
    /// <param name="prefix">编号前缀。</param>
    private static string NextNo(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    /// <summary>
    /// Normalize Text。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeText(string value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Normalize Key。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeKey(string value) => NormalizeText(value).ToLowerInvariant();

    /// <summary>
    /// Normalize Json。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeJson(string value)
    {
        var text = NormalizeText(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return "{}";
        }

        JsonDocument.Parse(text);
        return text;
    }

    /// <summary>
    /// 注册Definition 路由。
    /// </summary>
    /// <param name="definition">定义对象。</param>
    private static ReportDefinitionDto MapDefinition(ReportDefinition definition) =>
        new(definition.Id, definition.Key, definition.DisplayName, definition.Category, definition.QueryModel, definition.ParametersJson, definition.IsEnabled, definition.UpdatedBy, definition.UpdatedAtUtc);

    /// <summary>
    /// 注册Run 路由。
    /// </summary>
    /// <param name="run">运行记录。</param>
    private static ReportRunRecordDto MapRun(ReportRunRecord run) =>
        new(run.Id, run.RunNo, run.ReportDefinitionId, run.ReportKey, run.ReportName, run.ParametersJson, run.ResultSummaryJson, run.RowCount, run.Status, run.RunBy, run.CompletedAtUtc, run.UpdatedAtUtc);

    /// <summary>
    /// 注册Export Task 路由。
    /// </summary>
    /// <param name="task">任务对象。</param>
    private static ReportExportTaskDto MapExportTask(ReportExportTask task) =>
        new(task.Id, task.ExportNo, task.ReportRunRecordId, task.ReportName, task.Format, task.FileName, task.Status, task.RequestedBy, task.CompletedAtUtc, task.UpdatedAtUtc);
}
