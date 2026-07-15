using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Control.Contracts;
using AeroERP.Modules.Control.Domain;
using AeroERP.Modules.Control.Services;
using AeroERP.Modules.Finance.Domain;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.Procurement.Domain;
using AeroERP.Modules.Sales.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Control.Services;

/// <summary>
/// Control Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
/// <param name="auditWriter">audit Writer 参数。</param>
/// <param name="currentUser">current User 参数。</param>
public sealed class ControlService(AeroErpDbContext dbContext, IAuditWriter auditWriter, ICurrentUserAccessor currentUser) : IControlService
{
    /// <summary>
    /// 获取Analytics。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<AnalyticsSnapshotDto> GetAnalyticsAsync(CancellationToken cancellationToken)
    {
        var procurementRequests = await dbContext.ProcurementRequests.ToListAsync(cancellationToken);
        var procurementOrders = await dbContext.ProcurementOrders.ToListAsync(cancellationToken);
        var salesOrders = await dbContext.SalesOrders.ToListAsync(cancellationToken);
        var stockBalances = await dbContext.StockBalances.ToListAsync(cancellationToken);
        var payables = await dbContext.Payables.ToListAsync(cancellationToken);
        var receivables = await dbContext.Receivables.ToListAsync(cancellationToken);

        return new AnalyticsSnapshotDto(
            [
                new("procurement.requests", "采购申请", procurementRequests.Count, "张"),
                new("procurement.pending", "待审批采购申请", procurementRequests.Count(x => x.Status == ProcurementRequestStatus.Submitted), "张"),
                new("procurement.orders", "采购订单", procurementOrders.Count, "张")
            ],
            [
                new("sales.orders", "销售订单", salesOrders.Count, "张"),
                new("sales.readyToShip", "待出库销售订单", salesOrders.Count(x => x.Status == SalesOrderStatus.ReadyToShip), "张"),
                new("sales.shipped", "已出库销售订单", salesOrders.Count(x => x.Status == SalesOrderStatus.Shipped), "张")
            ],
            [
                new("inventory.balanceItems", "库存余额项", stockBalances.Count, "项"),
                new("inventory.totalQuantity", "库存总数量", stockBalances.Sum(x => x.Quantity), "件"),
                new("inventory.lowOrZero", "零库存项", stockBalances.Count(x => x.Quantity <= 0), "项")
            ],
            [
                new("finance.openPayables", "未结应付", payables.Count(x => x.Status != FinanceRecordStatus.Settled), "笔"),
                new("finance.openReceivables", "未结应收", receivables.Count(x => x.Status != FinanceRecordStatus.Settled), "笔"),
                new("finance.remaining", "待结金额", payables.Sum(x => x.RemainingAmount) + receivables.Sum(x => x.RemainingAmount), "元")
            ],
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 查询Data Scope Rules。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<DataScopeRuleDto>> ListDataScopeRulesAsync(CancellationToken cancellationToken)
    {
        var rules = await dbContext.DataScopeRules.ToListAsync(cancellationToken);

        return rules
            .OrderBy(x => x.RoleKey)
            .ThenBy(x => x.ScopeType)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// Upsert Data Scope Rule Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<DataScopeRuleDto>> UpsertDataScopeRuleAsync(
        UpsertDataScopeRuleRequest request,
        CancellationToken cancellationToken)
    {
        var roleKey = request.RoleKey.Trim();
        var scopeType = request.ScopeType.Trim();
        var matchValue = request.MatchValue.Trim();
        if (string.IsNullOrWhiteSpace(roleKey) || string.IsNullOrWhiteSpace(scopeType))
        {
            return OperationResult<DataScopeRuleDto>.Failure("角色和范围类型不能为空。");
        }

        if (scopeType != DataScopeType.SalesCustomerName)
        {
            return OperationResult<DataScopeRuleDto>.Failure("当前仅支持销售客户名称数据范围。");
        }

        var rule = await dbContext.DataScopeRules
            .FirstOrDefaultAsync(x => x.RoleKey == roleKey && x.ScopeType == scopeType, cancellationToken);
        if (rule is null)
        {
            rule = new DataScopeRule(roleKey, scopeType, matchValue, request.Description.Trim(), request.IsEnabled);
            dbContext.DataScopeRules.Add(rule);
        }
        else
        {
            rule.Update(matchValue, request.Description.Trim(), request.IsEnabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Control", "DataScopeRuleUpserted", currentUser.GetActor(), $"{roleKey}:{scopeType}", cancellationToken);
        return OperationResult<DataScopeRuleDto>.Success(Map(rule));
    }

    /// <summary>
    /// 查询Numbering Rules。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<NumberingRuleDto>> ListNumberingRulesAsync(CancellationToken cancellationToken)
    {
        var rules = await dbContext.NumberingRules.ToListAsync(cancellationToken);

        return rules
            .OrderBy(x => x.DocumentType)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// Upsert Numbering Rule Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<NumberingRuleDto>> UpsertNumberingRuleAsync(
        UpsertNumberingRuleRequest request,
        CancellationToken cancellationToken)
    {
        var documentType = request.DocumentType.Trim();
        var prefix = request.Prefix.Trim();
        if (documentType is not DocumentTypeKeys.ProcurementRequest and not DocumentTypeKeys.SalesQuotation)
        {
            return OperationResult<NumberingRuleDto>.Failure("当前仅支持采购申请和销售报价编号规则。");
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return OperationResult<NumberingRuleDto>.Failure("编号前缀不能为空。");
        }

        if (request.Padding is < 2 or > 8)
        {
            return OperationResult<NumberingRuleDto>.Failure("流水号位数必须在 2 到 8 之间。");
        }

        var rule = await dbContext.NumberingRules.FirstOrDefaultAsync(x => x.DocumentType == documentType, cancellationToken);
        if (rule is null)
        {
            rule = new NumberingRule(documentType, prefix, request.UseDateSegment, request.Padding, request.IsEnabled);
            dbContext.NumberingRules.Add(rule);
        }
        else
        {
            rule.Update(prefix, request.UseDateSegment, request.Padding, request.IsEnabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Control", "NumberingRuleUpserted", currentUser.GetActor(), documentType, cancellationToken);
        return OperationResult<NumberingRuleDto>.Success(Map(rule));
    }

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="rule">规则对象。</param>
    private static DataScopeRuleDto Map(DataScopeRule rule) =>
        new(rule.Id, rule.RoleKey, rule.ScopeType, rule.MatchValue, rule.Description, rule.IsEnabled, rule.CreatedAtUtc);

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="rule">规则对象。</param>
    private static NumberingRuleDto Map(NumberingRule rule) =>
        new(rule.Id, rule.DocumentType, rule.Prefix, rule.UseDateSegment, rule.NextSequence, rule.Padding, rule.IsEnabled, rule.CreatedAtUtc);
}
