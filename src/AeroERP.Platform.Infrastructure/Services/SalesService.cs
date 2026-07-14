using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Control.Domain;
using AeroERP.Modules.Control.Services;
using AeroERP.Modules.Sales.Contracts;
using AeroERP.Modules.Sales.Domain;
using AeroERP.Modules.Sales.Services;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Sales.Services;

/// <summary>
/// Sales Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class SalesService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser,
    INumberingService numberingService) : ISalesService
{
    /// <summary>
    /// 查询Quotations。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<SalesQuotationDto>> ListQuotationsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.SalesQuotations
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// 创建Quotation。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<SalesQuotationDto>> CreateQuotationAsync(CreateSalesQuotationRequest request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId, cancellationToken);
        if (customer is null || !customer.IsEnabled)
        {
            return OperationResult<SalesQuotationDto>.Failure("未找到可用客户。");
        }

        if (request.Lines.Count == 0)
        {
            return OperationResult<SalesQuotationDto>.Failure("销售报价至少需要一条物料行。");
        }

        var itemIds = request.Lines.Select(x => x.ItemId).Distinct().ToList();
        var items = await dbContext.Items
            .Where(x => itemIds.Contains(x.Id) && x.IsEnabled)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (items.Count != itemIds.Count)
        {
            return OperationResult<SalesQuotationDto>.Failure("存在不存在或已停用的物料。");
        }

        var lines = request.Lines.Select(line =>
        {
            var item = items[line.ItemId];
            return new SalesLine(item.Id, item.Code, item.Name, line.Quantity, line.Unit);
        });

        var settings = await dbContext.LocalizationSettings.FirstOrDefaultAsync(cancellationToken);
        var entity = new SalesQuotation(
            await numberingService.NextAsync(DocumentTypeKeys.SalesQuotation, "SQ", cancellationToken),
            customer.Id,
            customer.Name,
            request.Title.Trim(),
            customer.OrganizationId,
            customer.OrganizationName,
            string.IsNullOrWhiteSpace(request.CurrencyCode) ? customer.CurrencyCode : request.CurrencyCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(request.TaxInvoiceType) ? settings?.TaxInvoiceType ?? "增值税普通发票" : request.TaxInvoiceType.Trim(),
            request.TaxRate ?? settings?.DefaultTaxRate ?? 0.13m,
            currentUser.GetActor(),
            lines);

        dbContext.SalesQuotations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Sales", "QuotationCreated", currentUser.GetActor(), entity.QuotationNo, cancellationToken);
        return OperationResult<SalesQuotationDto>.Success(Map(entity));
    }

    /// <summary>
    /// Convert To Order Async。
    /// </summary>
    /// <param name="quotationId">quotation Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<SalesOrderDto>> ConvertToOrderAsync(Guid quotationId, CancellationToken cancellationToken)
    {
        var quotation = await dbContext.SalesQuotations
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == quotationId, cancellationToken);
        if (quotation is null)
        {
            return OperationResult<SalesOrderDto>.Failure("未找到销售报价。");
        }

        if (quotation.SalesOrderId.HasValue || string.Equals(quotation.Status, SalesQuotationStatus.Converted, StringComparison.Ordinal))
        {
            return OperationResult<SalesOrderDto>.Failure("该销售报价已转为销售订单。");
        }

        if (quotation.Lines.Count == 0)
        {
            return OperationResult<SalesOrderDto>.Failure("销售报价没有有效行项目。");
        }

        var order = new SalesOrder(
            $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            quotation.Id,
            quotation.QuotationNo,
            quotation.CustomerId,
            quotation.CustomerName,
            quotation.OrganizationId,
            quotation.OrganizationName,
            quotation.CurrencyCode,
            quotation.TaxInvoiceType,
            quotation.TaxRate,
            currentUser.GetActor(),
            quotation.Lines.Select(line => new SalesOrderLine(line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit)));

        dbContext.SalesOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        quotation.LinkOrder(order.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Sales", "OrderCreated", currentUser.GetActor(), $"{quotation.QuotationNo}:{order.OrderNo}", cancellationToken);
        return OperationResult<SalesOrderDto>.Success(Map(order));
    }

    /// <summary>
    /// 查询Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<SalesOrderDto>> ListOrdersAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.SalesOrders
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        entities = await ApplySalesDataScopeAsync(entities, cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// Confirm Order Async。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<SalesOrderDto>> ConfirmOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<SalesOrderDto>.Failure("未找到销售订单。");
        }

        try
        {
            order.Confirm();
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<SalesOrderDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Sales", "OrderConfirmed", currentUser.GetActor(), order.OrderNo, cancellationToken);
        return OperationResult<SalesOrderDto>.Success(Map(order));
    }

    /// <summary>
    /// Mark Order Ready To Ship Async。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<SalesOrderDto>> MarkOrderReadyToShipAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<SalesOrderDto>.Failure("未找到销售订单。");
        }

        try
        {
            order.MarkReadyToShip();
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<SalesOrderDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Sales", "OrderReadyToShip", currentUser.GetActor(), order.OrderNo, cancellationToken);
        return OperationResult<SalesOrderDto>.Success(Map(order));
    }

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static SalesQuotationDto Map(SalesQuotation entity) =>
        new(
            entity.Id,
            entity.QuotationNo,
            entity.CustomerId,
            entity.CustomerName,
            entity.Title,
            entity.Status,
            entity.OrganizationId,
            entity.OrganizationName,
            entity.CurrencyCode,
            entity.TaxInvoiceType,
            entity.TaxRate,
            entity.Lines.Select(line => new SalesLineDto(line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit)).ToList(),
            entity.CreatedAtUtc);

    /// <summary>
    /// Apply Sales Data Scope Async。
    /// </summary>
    /// <param name="orders">订单集合。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<List<SalesOrder>> ApplySalesDataScopeAsync(List<SalesOrder> orders, CancellationToken cancellationToken)
    {
        if (currentUser.HasRole(PlatformRoleCatalog.PlatformAdmin))
        {
            return orders;
        }

        var roleKeys = currentUser.Roles.ToList();
        if (roleKeys.Count == 0)
        {
            return orders;
        }

        var rules = await dbContext.DataScopeRules
            .Where(x =>
                x.IsEnabled &&
                x.ScopeType == DataScopeType.SalesCustomerName &&
                roleKeys.Contains(x.RoleKey))
            .ToListAsync(cancellationToken);
        if (rules.Count == 0)
        {
            return orders;
        }

        return orders
            .Where(order => rules.Any(rule =>
                order.CustomerName.Contains(rule.MatchValue, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static SalesOrderDto Map(SalesOrder entity) =>
        new(
            entity.Id,
            entity.OrderNo,
            entity.QuotationId,
            entity.QuotationNo,
            entity.CustomerId,
            entity.CustomerName,
            entity.Status,
            entity.OrganizationId,
            entity.OrganizationName,
            entity.CurrencyCode,
            entity.TaxInvoiceType,
            entity.TaxRate,
            entity.Lines.Select(line => new SalesLineDto(line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit)).ToList(),
            entity.CreatedAtUtc);
}
