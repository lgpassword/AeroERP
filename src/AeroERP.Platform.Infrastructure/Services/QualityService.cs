using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.Manufacturing.Domain;
using AeroERP.Modules.Quality.Contracts;
using AeroERP.Modules.Quality.Domain;
using AeroERP.Modules.Quality.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Quality Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class QualityService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IQualityService
{
    /// <summary>
    /// 查询Source Candidates。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<QualitySourceCandidateDto>> ListSourceCandidatesAsync(CancellationToken cancellationToken)
    {
        var receiptCandidates = await BuildInventoryReceiptCandidatesAsync(cancellationToken);
        var productionCandidates = await BuildProductionReceiptCandidatesAsync(cancellationToken);
        var issueCandidates = await BuildInventoryIssueCandidatesAsync(cancellationToken);

        return receiptCandidates
            .Concat(productionCandidates)
            .Concat(issueCandidates)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToList();
    }

    /// <summary>
    /// 查询Inspections。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<QualityInspectionDto>> ListInspectionsAsync(CancellationToken cancellationToken)
    {
        var inspections = await dbContext.QualityInspections.ToListAsync(cancellationToken);
        return inspections
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapInspection)
            .ToList();
    }

    /// <summary>
    /// 创建Inspection。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<QualityInspectionDto>> CreateInspectionAsync(
        CreateQualityInspectionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.InspectedQuantity <= 0)
        {
            return OperationResult<QualityInspectionDto>.Failure("质检数量必须大于零。");
        }

        if (request.AcceptedQuantity < 0 || request.RejectedQuantity < 0)
        {
            return OperationResult<QualityInspectionDto>.Failure("合格数量和不合格数量不能为负数。");
        }

        if (request.AcceptedQuantity + request.RejectedQuantity != request.InspectedQuantity)
        {
            return OperationResult<QualityInspectionDto>.Failure("合格数量与不合格数量之和必须等于质检数量。");
        }

        var source = await ResolveSourceAsync(
            request.SourceDocumentType,
            request.SourceDocumentId,
            request.ItemId,
            cancellationToken);
        if (source is null)
        {
            return OperationResult<QualityInspectionDto>.Failure("未找到可质检的业务来源或物料行。");
        }

        if (request.InspectedQuantity > source.Quantity)
        {
            return OperationResult<QualityInspectionDto>.Failure("质检数量不能超过来源单据数量。");
        }

        var result = request.RejectedQuantity == 0
            ? QualityInspectionResult.Accepted
            : request.AcceptedQuantity == 0
                ? QualityInspectionResult.Rejected
                : QualityInspectionResult.PartiallyAccepted;

        var actor = currentUser.GetActor();
        var inspection = new QualityInspection(
            $"QI-{DateTime.UtcNow:yyyyMMddHHmmss}",
            source.SourceDocumentType,
            source.SourceDocumentId,
            source.SourceDocumentNo,
            source.ItemId,
            source.ItemCode,
            source.ItemName,
            request.InspectedQuantity,
            request.AcceptedQuantity,
            request.RejectedQuantity,
            result,
            string.IsNullOrWhiteSpace(request.Disposition) ? "放行" : request.Disposition.Trim(),
            actor,
            request.Note.Trim());

        dbContext.QualityInspections.Add(inspection);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Quality", "InspectionCreated", actor, $"{inspection.InspectionNo}:{inspection.SourceDocumentNo}", cancellationToken);
        return OperationResult<QualityInspectionDto>.Success(MapInspection(inspection));
    }

    /// <summary>
    /// 查询Lot Trace Events。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<LotTraceEventDto>> ListLotTraceEventsAsync(CancellationToken cancellationToken)
    {
        var events = await dbContext.LotTraceEvents.ToListAsync(cancellationToken);
        return events
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapLotTraceEvent)
            .ToList();
    }

    /// <summary>
    /// 创建Lot Trace Event。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<LotTraceEventDto>> CreateLotTraceEventAsync(
        CreateLotTraceEventRequest request,
        CancellationToken cancellationToken)
    {
        var lotNo = request.LotNo.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(lotNo))
        {
            return OperationResult<LotTraceEventDto>.Failure("批次号不能为空。");
        }

        if (request.Quantity <= 0)
        {
            return OperationResult<LotTraceEventDto>.Failure("批次数量必须大于零。");
        }

        var source = await ResolveSourceAsync(
            request.SourceDocumentType,
            request.SourceDocumentId,
            request.ItemId,
            cancellationToken);
        if (source is null)
        {
            return OperationResult<LotTraceEventDto>.Failure("未找到可追溯的业务来源或物料行。");
        }

        if (request.Quantity > source.Quantity)
        {
            return OperationResult<LotTraceEventDto>.Failure("批次数量不能超过来源单据数量。");
        }

        var eventType = NormalizeEventType(request.EventType, source.SourceDocumentType);
        var actor = currentUser.GetActor();
        var traceEvent = new LotTraceEvent(
            lotNo,
            eventType,
            source.SourceDocumentType,
            source.SourceDocumentId,
            source.SourceDocumentNo,
            request.TargetDocumentType.Trim(),
            request.TargetDocumentId,
            request.TargetDocumentNo.Trim(),
            source.ItemId,
            source.ItemCode,
            source.ItemName,
            request.Quantity,
            source.Unit,
            actor,
            request.Note.Trim());

        dbContext.LotTraceEvents.Add(traceEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Quality", "LotTraceEventCreated", actor, $"{traceEvent.LotNo}:{traceEvent.SourceDocumentNo}", cancellationToken);
        return OperationResult<LotTraceEventDto>.Success(MapLotTraceEvent(traceEvent));
    }

    /// <summary>
    /// 获取Lot Trace。
    /// </summary>
    /// <param name="lotNo">lot No 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<LotTraceDto> GetLotTraceAsync(string lotNo, CancellationToken cancellationToken)
    {
        var normalized = lotNo.Trim().ToUpperInvariant();
        var events = await dbContext.LotTraceEvents
            .Where(x => x.LotNo == normalized)
            .ToListAsync(cancellationToken);

        return new LotTraceDto(
            normalized,
            events
                .OrderBy(x => x.CreatedAtUtc)
                .Select(MapLotTraceEvent)
                .ToList());
    }

    /// <summary>
    /// Build Inventory Receipt Candidates Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<QualitySourceCandidateDto>> BuildInventoryReceiptCandidatesAsync(CancellationToken cancellationToken)
    {
        var receipts = await dbContext.InventoryReceipts
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return receipts
            .SelectMany(receipt => receipt.Lines.Select(line => new QualitySourceCandidateDto(
                QualityDocumentTypes.InventoryReceipt,
                receipt.Id,
                receipt.ReceiptNo,
                receipt.SupplierName,
                line.ItemId,
                line.ItemCode,
                line.ItemName,
                line.Quantity,
                line.Unit,
                receipt.CreatedAtUtc)))
            .ToList();
    }

    /// <summary>
    /// Build Production Receipt Candidates Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<QualitySourceCandidateDto>> BuildProductionReceiptCandidatesAsync(CancellationToken cancellationToken)
    {
        var receipts = await dbContext.ProductionReceipts.ToListAsync(cancellationToken);
        return receipts
            .Select(receipt => new QualitySourceCandidateDto(
                QualityDocumentTypes.ProductionReceipt,
                receipt.Id,
                receipt.ReceiptNo,
                receipt.WorkOrderNo,
                receipt.FinishedItemId,
                receipt.FinishedItemCode,
                receipt.FinishedItemName,
                receipt.Quantity,
                receipt.Unit,
                receipt.CreatedAtUtc))
            .ToList();
    }

    /// <summary>
    /// Build Inventory Issue Candidates Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<QualitySourceCandidateDto>> BuildInventoryIssueCandidatesAsync(CancellationToken cancellationToken)
    {
        var issues = await dbContext.InventoryIssues
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return issues
            .SelectMany(issue => issue.Lines.Select(line => new QualitySourceCandidateDto(
                QualityDocumentTypes.InventoryIssue,
                issue.Id,
                issue.IssueNo,
                issue.CustomerName,
                line.ItemId,
                line.ItemCode,
                line.ItemName,
                line.Quantity,
                line.Unit,
                issue.CreatedAtUtc)))
            .ToList();
    }

    /// <summary>
    /// Resolve Source Async。
    /// </summary>
    /// <param name="sourceDocumentType">source Document Type 参数。</param>
    /// <param name="sourceDocumentId">source Document Id 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<QualitySourceCandidateDto?> ResolveSourceAsync(
        string sourceDocumentType,
        Guid sourceDocumentId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var normalizedType = sourceDocumentType.Trim();
        if (normalizedType == QualityDocumentTypes.InventoryReceipt)
        {
            var receipt = await dbContext.InventoryReceipts
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == sourceDocumentId, cancellationToken);
            var line = receipt?.Lines.FirstOrDefault(x => x.ItemId == itemId);
            return receipt is null || line is null
                ? null
                : new QualitySourceCandidateDto(
                    QualityDocumentTypes.InventoryReceipt,
                    receipt.Id,
                    receipt.ReceiptNo,
                    receipt.SupplierName,
                    line.ItemId,
                    line.ItemCode,
                    line.ItemName,
                    line.Quantity,
                    line.Unit,
                    receipt.CreatedAtUtc);
        }

        if (normalizedType == QualityDocumentTypes.ProductionReceipt)
        {
            var receipt = await dbContext.ProductionReceipts
                .FirstOrDefaultAsync(x => x.Id == sourceDocumentId && x.FinishedItemId == itemId, cancellationToken);
            return receipt is null
                ? null
                : new QualitySourceCandidateDto(
                    QualityDocumentTypes.ProductionReceipt,
                    receipt.Id,
                    receipt.ReceiptNo,
                    receipt.WorkOrderNo,
                    receipt.FinishedItemId,
                    receipt.FinishedItemCode,
                    receipt.FinishedItemName,
                    receipt.Quantity,
                    receipt.Unit,
                    receipt.CreatedAtUtc);
        }

        if (normalizedType == QualityDocumentTypes.InventoryIssue)
        {
            var issue = await dbContext.InventoryIssues
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == sourceDocumentId, cancellationToken);
            var line = issue?.Lines.FirstOrDefault(x => x.ItemId == itemId);
            return issue is null || line is null
                ? null
                : new QualitySourceCandidateDto(
                    QualityDocumentTypes.InventoryIssue,
                    issue.Id,
                    issue.IssueNo,
                    issue.CustomerName,
                    line.ItemId,
                    line.ItemCode,
                    line.ItemName,
                    line.Quantity,
                    line.Unit,
                    issue.CreatedAtUtc);
        }

        return null;
    }

    /// <summary>
    /// Normalize Event Type。
    /// </summary>
    /// <param name="requested">请求值。</param>
    /// <param name="sourceDocumentType">source Document Type 参数。</param>
    private static string NormalizeEventType(string requested, string sourceDocumentType)
    {
        var trimmed = requested.Trim();
        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            return trimmed;
        }

        return sourceDocumentType switch
        {
            QualityDocumentTypes.InventoryReceipt => LotTraceEventType.Incoming,
            QualityDocumentTypes.ProductionReceipt => LotTraceEventType.ProductionCompletion,
            QualityDocumentTypes.InventoryIssue => LotTraceEventType.Shipment,
            _ => LotTraceEventType.Inspection
        };
    }

    /// <summary>
    /// 注册Inspection 路由。
    /// </summary>
    /// <param name="inspection">质检记录。</param>
    private static QualityInspectionDto MapInspection(QualityInspection inspection) =>
        new(
            inspection.Id,
            inspection.InspectionNo,
            inspection.SourceDocumentType,
            inspection.SourceDocumentId,
            inspection.SourceDocumentNo,
            inspection.ItemId,
            inspection.ItemCode,
            inspection.ItemName,
            inspection.InspectedQuantity,
            inspection.AcceptedQuantity,
            inspection.RejectedQuantity,
            inspection.Result,
            inspection.Disposition,
            inspection.Inspector,
            inspection.Note,
            inspection.CreatedAtUtc);

    /// <summary>
    /// 注册Lot Trace Event 路由。
    /// </summary>
    /// <param name="traceEvent">trace Event 参数。</param>
    private static LotTraceEventDto MapLotTraceEvent(LotTraceEvent traceEvent) =>
        new(
            traceEvent.Id,
            traceEvent.LotNo,
            traceEvent.EventType,
            traceEvent.SourceDocumentType,
            traceEvent.SourceDocumentId,
            traceEvent.SourceDocumentNo,
            traceEvent.TargetDocumentType,
            traceEvent.TargetDocumentId,
            traceEvent.TargetDocumentNo,
            traceEvent.ItemId,
            traceEvent.ItemCode,
            traceEvent.ItemName,
            traceEvent.Quantity,
            traceEvent.Unit,
            traceEvent.Actor,
            traceEvent.Note,
            traceEvent.CreatedAtUtc);
}
