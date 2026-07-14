using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Wms.Domain;

/// <summary>
/// Warehouse Route 业务对象。
/// </summary>
public sealed class WarehouseRoute : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Warehouse Route实例。
    /// </summary>
    private WarehouseRoute()
    {
    }

    /// <summary>
    /// 初始化Warehouse Route实例。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="fromLocationId">from Location Id 参数。</param>
    /// <param name="fromLocationCode">from Location Code 参数。</param>
    /// <param name="fromLocationName">from Location Name 参数。</param>
    /// <param name="toLocationId">to Location Id 参数。</param>
    /// <param name="toLocationCode">to Location Code 参数。</param>
    /// <param name="toLocationName">to Location Name 参数。</param>
    /// <param name="distanceMeters">distance Meters 参数。</param>
    /// <param name="priority">优先级。</param>
    /// <param name="isEnabled">是否启用。</param>
    public WarehouseRoute(Guid warehouseId, string warehouseCode, string warehouseName, Guid fromLocationId, string fromLocationCode, string fromLocationName, Guid toLocationId, string toLocationCode, string toLocationName, decimal distanceMeters, int priority, bool isEnabled)
    {
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        FromLocationId = fromLocationId;
        FromLocationCode = fromLocationCode;
        FromLocationName = fromLocationName;
        ToLocationId = toLocationId;
        ToLocationCode = toLocationCode;
        ToLocationName = toLocationName;
        DistanceMeters = distanceMeters;
        Priority = priority;
        IsEnabled = isEnabled;
    }

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
    /// From Location Id。
    /// </summary>
    public Guid FromLocationId { get; private set; }
    /// <summary>
    /// From Location Code。
    /// </summary>
    public string FromLocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// From Location Name。
    /// </summary>
    public string FromLocationName { get; private set; } = string.Empty;
    /// <summary>
    /// To Location Id。
    /// </summary>
    public Guid ToLocationId { get; private set; }
    /// <summary>
    /// To Location Code。
    /// </summary>
    public string ToLocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// To Location Name。
    /// </summary>
    public string ToLocationName { get; private set; } = string.Empty;
    /// <summary>
    /// Distance Meters。
    /// </summary>
    public decimal DistanceMeters { get; private set; }
    /// <summary>
    /// Priority。
    /// </summary>
    public int Priority { get; private set; }
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="distanceMeters">distance Meters 参数。</param>
    /// <param name="priority">优先级。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(decimal distanceMeters, int priority, bool isEnabled)
    {
        DistanceMeters = distanceMeters;
        Priority = priority;
        IsEnabled = isEnabled;
        Touch();
    }
}
