using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Wms.Domain;

/// <summary>
/// Warehouse Container 业务对象。
/// </summary>
public sealed class WarehouseContainer : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Warehouse Container实例。
    /// </summary>
    private WarehouseContainer()
    {
    }

    /// <summary>
    /// 初始化Warehouse Container实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="containerType">container Type 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="currentLocationId">current Location Id 参数。</param>
    /// <param name="currentLocationCode">current Location Code 参数。</param>
    /// <param name="currentLocationName">current Location Name 参数。</param>
    /// <param name="status">业务状态。</param>
    /// <param name="actor">操作人。</param>
    public WarehouseContainer(string code, string containerType, Guid warehouseId, string warehouseCode, string warehouseName, Guid? currentLocationId, string currentLocationCode, string currentLocationName, string status, string actor)
    {
        Code = code;
        ContainerType = containerType;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        CurrentLocationId = currentLocationId;
        CurrentLocationCode = currentLocationCode;
        CurrentLocationName = currentLocationName;
        Status = status;
        LastHandledBy = actor;
    }

    /// <summary>
    /// 业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// Container Type。
    /// </summary>
    public string ContainerType { get; private set; } = string.Empty;
    /// <summary>
    /// Warehouse Id。
    /// </summary>
    public Guid WarehouseId { get; private set; }
    /// <summary>
    /// Warehouse Code。
    /// </summary>
    public string WarehouseCode { get; private set; } = string.Empty;
    /// <summary>
    /// Warehouse Name。
    /// </summary>
    public string WarehouseName { get; private set; } = string.Empty;
    /// <summary>
    /// Current Location Id。
    /// </summary>
    public Guid? CurrentLocationId { get; private set; }
    /// <summary>
    /// Current Location Code。
    /// </summary>
    public string CurrentLocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Current Location Name。
    /// </summary>
    public string CurrentLocationName { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = string.Empty;
    /// <summary>
    /// Last Handled By。
    /// </summary>
    public string LastHandledBy { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="containerType">container Type 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="currentLocationId">current Location Id 参数。</param>
    /// <param name="currentLocationCode">current Location Code 参数。</param>
    /// <param name="currentLocationName">current Location Name 参数。</param>
    /// <param name="status">业务状态。</param>
    /// <param name="actor">操作人。</param>
    public void Update(string containerType, Guid warehouseId, string warehouseCode, string warehouseName, Guid? currentLocationId, string currentLocationCode, string currentLocationName, string status, string actor)
    {
        ContainerType = containerType;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        CurrentLocationId = currentLocationId;
        CurrentLocationCode = currentLocationCode;
        CurrentLocationName = currentLocationName;
        Status = status;
        LastHandledBy = actor;
        Touch();
    }
}
