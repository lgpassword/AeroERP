using AeroERP.Modules.MasterData.Contracts;
using AeroERP.Modules.MasterData.Domain;
using AeroERP.Modules.MasterData.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.MasterData.Services;

/// <summary>
/// Master Data Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
/// <param name="auditWriter">audit Writer 参数。</param>
/// <param name="currentUser">current User 参数。</param>
public sealed class MasterDataService(AeroErpDbContext dbContext, IAuditWriter auditWriter, ICurrentUserAccessor currentUser) : IMasterDataService
{
    /// <summary>
    /// 查询Customers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<CustomerDto>> ListCustomersAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Customers.OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return entities.Select(Map).ToList();
    }

    /// <summary>
    /// 创建Customer。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<CustomerDto> CreateCustomerAsync(UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        var organization = await ResolveOrganizationAsync(request.OrganizationId, cancellationToken);
        var currencyCode = await ResolveCurrencyCodeAsync(request.CurrencyCode, cancellationToken);
        var entity = new Customer(request.Code, request.Name, request.ContactName, request.Phone, request.IsEnabled, organization?.Id, organization?.Name ?? string.Empty, currencyCode, request.TaxpayerId, request.InvoiceTitle);
        dbContext.Customers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "CustomerCreated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 更新Customer。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var organization = await ResolveOrganizationAsync(request.OrganizationId, cancellationToken);
        var currencyCode = await ResolveCurrencyCodeAsync(request.CurrencyCode, cancellationToken);
        entity.Update(request.Code, request.Name, request.ContactName, request.Phone, request.IsEnabled, organization?.Id, organization?.Name ?? string.Empty, currencyCode, request.TaxpayerId, request.InvoiceTitle);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "CustomerUpdated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 查询Suppliers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<SupplierDto>> ListSuppliersAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Suppliers.OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return entities.Select(Map).ToList();
    }

    /// <summary>
    /// 创建Supplier。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<SupplierDto> CreateSupplierAsync(UpsertSupplierRequest request, CancellationToken cancellationToken)
    {
        var organization = await ResolveOrganizationAsync(request.OrganizationId, cancellationToken);
        var currencyCode = await ResolveCurrencyCodeAsync(request.CurrencyCode, cancellationToken);
        var entity = new Supplier(request.Code, request.Name, request.ContactName, request.Phone, request.IsEnabled, organization?.Id, organization?.Name ?? string.Empty, currencyCode, request.TaxpayerId, request.InvoiceTitle);
        dbContext.Suppliers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "SupplierCreated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 更新Supplier。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<SupplierDto?> UpdateSupplierAsync(Guid id, UpsertSupplierRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var organization = await ResolveOrganizationAsync(request.OrganizationId, cancellationToken);
        var currencyCode = await ResolveCurrencyCodeAsync(request.CurrencyCode, cancellationToken);
        entity.Update(request.Code, request.Name, request.ContactName, request.Phone, request.IsEnabled, organization?.Id, organization?.Name ?? string.Empty, currencyCode, request.TaxpayerId, request.InvoiceTitle);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "SupplierUpdated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 查询Items。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ItemDto>> ListItemsAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Items.OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return entities.Select(Map).ToList();
    }

    /// <summary>
    /// 创建Item。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<ItemDto> CreateItemAsync(UpsertItemRequest request, CancellationToken cancellationToken)
    {
        var entity = new Item(request.Code, request.Name, request.Specification, request.Unit, request.IsEnabled);
        dbContext.Items.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "ItemCreated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 更新Item。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<ItemDto?> UpdateItemAsync(Guid id, UpsertItemRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Items.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        entity.Update(request.Code, request.Name, request.Specification, request.Unit, request.IsEnabled);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "ItemUpdated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 查询Warehouses。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<WarehouseDto>> ListWarehousesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Warehouses.OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return entities.Select(Map).ToList();
    }

    /// <summary>
    /// 创建Warehouse。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<WarehouseDto> CreateWarehouseAsync(UpsertWarehouseRequest request, CancellationToken cancellationToken)
    {
        var organization = await ResolveOrganizationAsync(request.OrganizationId, cancellationToken);
        var entity = new Warehouse(request.Code, request.Name, request.Location, request.IsEnabled, organization?.Id, organization?.Name ?? string.Empty);
        dbContext.Warehouses.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "WarehouseCreated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 更新Warehouse。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<WarehouseDto?> UpdateWarehouseAsync(Guid id, UpsertWarehouseRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Warehouses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var organization = await ResolveOrganizationAsync(request.OrganizationId, cancellationToken);
        entity.Update(request.Code, request.Name, request.Location, request.IsEnabled, organization?.Id, organization?.Name ?? string.Empty);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MasterData", "WarehouseUpdated", currentUser.GetActor(), entity.Code, cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// Resolve Organization Async。
    /// </summary>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<AeroERP.Platform.Domain.Organization?> ResolveOrganizationAsync(Guid? organizationId, CancellationToken cancellationToken)
    {
        if (organizationId is null || organizationId == Guid.Empty)
        {
            return null;
        }

        return await dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == organizationId, cancellationToken);
    }

    /// <summary>
    /// Resolve Currency Code Async。
    /// </summary>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<string> ResolveCurrencyCodeAsync(string currencyCode, CancellationToken cancellationToken)
    {
        var normalized = string.IsNullOrWhiteSpace(currencyCode) ? "CNY" : currencyCode.Trim().ToUpperInvariant();
        var exists = await dbContext.Currencies.AnyAsync(x => x.Code == normalized && x.IsEnabled, cancellationToken);
        return exists ? normalized : "CNY";
    }

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static CustomerDto Map(Customer entity) => new(entity.Id, entity.Code, entity.Name, entity.ContactName, entity.Phone, entity.IsEnabled, entity.OrganizationId, entity.OrganizationName, entity.CurrencyCode, entity.TaxpayerId, entity.InvoiceTitle);
    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static SupplierDto Map(Supplier entity) => new(entity.Id, entity.Code, entity.Name, entity.ContactName, entity.Phone, entity.IsEnabled, entity.OrganizationId, entity.OrganizationName, entity.CurrencyCode, entity.TaxpayerId, entity.InvoiceTitle);
    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static ItemDto Map(Item entity) => new(entity.Id, entity.Code, entity.Name, entity.Specification, entity.Unit, entity.IsEnabled);
    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static WarehouseDto Map(Warehouse entity) => new(entity.Id, entity.Code, entity.Name, entity.Location, entity.IsEnabled, entity.OrganizationId, entity.OrganizationName);
}
