using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Control.Domain;
using AeroERP.Modules.Control.Services;
using AeroERP.Modules.Procurement.Contracts;
using AeroERP.Modules.Procurement.Domain;
using AeroERP.Modules.Procurement.Services;
using AeroERP.Modules.Workflow.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Procurement.Services;

/// <summary>
/// Procurement Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class ProcurementService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser,
    IWorkflowService workflowService,
    INumberingService numberingService) : IProcurementService
{
    /// <summary>
    /// 查询Requests。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ProcurementRequestDto>> ListRequestsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.ProcurementRequests
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// 创建Request。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ProcurementRequestDto>> CreateRequestAsync(CreateProcurementRequestRequest request, CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return OperationResult<ProcurementRequestDto>.Failure("未找到供应商。");
        }

        var itemIds = request.Lines.Select(x => x.ItemId).Distinct().ToList();
        var items = await dbContext.Items.Where(x => itemIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        if (items.Count != itemIds.Count)
        {
            return OperationResult<ProcurementRequestDto>.Failure("存在不存在的物料。");
        }

        var lines = request.Lines.Select(x => new ProcurementRequestLine(x.ItemId, items[x.ItemId].Name, x.Quantity, x.Unit));
        var requestNo = await numberingService.NextAsync(DocumentTypeKeys.ProcurementRequest, "PR", cancellationToken);
        var settings = await dbContext.LocalizationSettings.FirstOrDefaultAsync(cancellationToken);
        var currencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? supplier.CurrencyCode : request.CurrencyCode.Trim().ToUpperInvariant();
        var taxInvoiceType = string.IsNullOrWhiteSpace(request.TaxInvoiceType) ? settings?.TaxInvoiceType ?? "增值税普通发票" : request.TaxInvoiceType.Trim();
        var taxRate = request.TaxRate ?? settings?.DefaultTaxRate ?? 0.13m;
        var entity = new ProcurementRequest(
            requestNo,
            supplier.Id,
            supplier.Name,
            request.Title,
            supplier.OrganizationId,
            supplier.OrganizationName,
            currencyCode,
            taxInvoiceType,
            taxRate,
            lines);
        entity.Submit();
        dbContext.ProcurementRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await workflowService.EnsureProcurementRequestWorkflowAsync(entity.Id, cancellationToken);
        await auditWriter.WriteAsync("Procurement", "RequestSubmitted", currentUser.GetActor(), entity.RequestNo, cancellationToken);
        return OperationResult<ProcurementRequestDto>.Success(Map(entity));
    }

    /// <summary>
    /// Decide Request Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ProcurementRequestDto>> DecideRequestAsync(Guid id, DecideProcurementRequestRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProcurementRequests.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return OperationResult<ProcurementRequestDto>.Failure("未找到采购申请。");
        }

        entity.Decide(request.Decision, currentUser.GetActor());
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Procurement", "RequestReviewed", currentUser.GetActor(), $"{entity.RequestNo}:{request.Decision}", cancellationToken);
        return OperationResult<ProcurementRequestDto>.Success(Map(entity));
    }

    /// <summary>
    /// Convert To Order Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ProcurementOrderDto>> ConvertToOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProcurementRequests.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return OperationResult<ProcurementOrderDto>.Failure("未找到采购申请。");
        }

        if (!string.Equals(entity.Status, ProcurementRequestStatus.Approved, StringComparison.Ordinal))
        {
            return OperationResult<ProcurementOrderDto>.Failure("只有已审核通过的申请才能转为订单。");
        }

        var order = new ProcurementOrder($"PO-{DateTime.UtcNow:yyyyMMddHHmmss}", entity.Id, entity.RequestNo, entity.SupplierId, entity.SupplierName, currentUser.GetActor());
        dbContext.ProcurementOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        entity.LinkOrder(order.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Procurement", "OrderCreated", currentUser.GetActor(), $"{entity.RequestNo}:{order.OrderNo}", cancellationToken);
        return OperationResult<ProcurementOrderDto>.Success(Map(order));
    }

    /// <summary>
    /// 查询Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ProcurementOrderDto>> ListOrdersAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.ProcurementOrders
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// Release Order Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ProcurementOrderDto>> ReleaseOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProcurementOrders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return OperationResult<ProcurementOrderDto>.Failure("未找到采购订单。");
        }

        entity.Release();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Procurement", "OrderReleased", currentUser.GetActor(), entity.OrderNo, cancellationToken);
        return OperationResult<ProcurementOrderDto>.Success(Map(entity));
    }

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static ProcurementRequestDto Map(ProcurementRequest entity) =>
        new(
            entity.Id,
            entity.RequestNo,
            entity.SupplierId,
            entity.SupplierName,
            entity.Title,
            entity.Status,
            entity.OrganizationId,
            entity.OrganizationName,
            entity.CurrencyCode,
            entity.TaxInvoiceType,
            entity.TaxRate,
            entity.Lines.Select(line => new ProcurementRequestLineDto(line.ItemId, line.ItemName, line.Quantity, line.Unit)).ToList(),
            entity.CreatedAtUtc);

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static ProcurementOrderDto Map(ProcurementOrder entity) =>
        new(entity.Id, entity.OrderNo, entity.RequestId, entity.RequestNo, entity.SupplierName, entity.Status, entity.CreatedAtUtc);
}
