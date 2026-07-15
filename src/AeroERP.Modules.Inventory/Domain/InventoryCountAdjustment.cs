using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Count Adjustment 业务对象。
/// </summary>
public sealed class InventoryCountAdjustment : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Inventory Count Adjustment实例。
    /// </summary>
    private InventoryCountAdjustment()
    {
    }

    /// <summary>
    /// 初始化Inventory Count Adjustment实例。
    /// </summary>
    /// <param name="countNo">count No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="locationCode">location Code 参数。</param>
    /// <param name="locationName">location Name 参数。</param>
    /// <param name="reason">业务原因。</param>
    /// <param name="countedBy">counted By 参数。</param>
    /// <param name="lines">明细行集合。</param>
    public InventoryCountAdjustment(
        string countNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid? locationId,
        string locationCode,
        string locationName,
        string reason,
        string countedBy,
        IEnumerable<InventoryCountAdjustmentLine> lines)
    {
        CountNo = countNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        LocationId = locationId;
        LocationCode = locationCode;
        LocationName = locationName;
        Reason = reason;
        CountedBy = countedBy;
        Lines = lines.ToList();
    }

    /// <summary>
    /// Count No。
    /// </summary>
    public string CountNo { get; private set; } = string.Empty;
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
    /// Location Id。
    /// </summary>
    public Guid? LocationId { get; private set; }
    /// <summary>
    /// Location Code。
    /// </summary>
    public string LocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Location Name。
    /// </summary>
    public string LocationName { get; private set; } = string.Empty;
    /// <summary>
    /// Reason。
    /// </summary>
    public string Reason { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = InventoryCountAdjustmentStatus.Completed;
    /// <summary>
    /// Counted By。
    /// </summary>
    public string CountedBy { get; private set; } = string.Empty;
    /// <summary>
    /// 明细行集合。
    /// </summary>
    public List<InventoryCountAdjustmentLine> Lines { get; private set; } = [];
}
