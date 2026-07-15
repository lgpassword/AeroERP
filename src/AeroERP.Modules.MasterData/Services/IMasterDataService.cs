using AeroERP.Modules.MasterData.Contracts;

namespace AeroERP.Modules.MasterData.Services;

/// <summary>
/// Master Data Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IMasterDataService
{
    /// <summary>
    /// 查询Customers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<CustomerDto>> ListCustomersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Customer。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<CustomerDto> CreateCustomerAsync(UpsertCustomerRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新Customer。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpsertCustomerRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Suppliers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<SupplierDto>> ListSuppliersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Supplier。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<SupplierDto> CreateSupplierAsync(UpsertSupplierRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新Supplier。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<SupplierDto?> UpdateSupplierAsync(Guid id, UpsertSupplierRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Items。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ItemDto>> ListItemsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Item。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<ItemDto> CreateItemAsync(UpsertItemRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新Item。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<ItemDto?> UpdateItemAsync(Guid id, UpsertItemRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Warehouses。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<WarehouseDto>> ListWarehousesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Warehouse。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<WarehouseDto> CreateWarehouseAsync(UpsertWarehouseRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新Warehouse。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<WarehouseDto?> UpdateWarehouseAsync(Guid id, UpsertWarehouseRequest request, CancellationToken cancellationToken);
}
