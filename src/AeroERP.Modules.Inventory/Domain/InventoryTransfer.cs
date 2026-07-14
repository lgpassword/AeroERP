using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Transfer 业务对象。
/// </summary>
public sealed class InventoryTransfer : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Inventory Transfer实例。
    /// </summary>
    private InventoryTransfer()
    {
    }

    /// <summary>
    /// 初始化Inventory Transfer实例。
    /// </summary>
    /// <param name="transferNo">transfer No 参数。</param>
    /// <param name="fromWarehouseId">from Warehouse Id 参数。</param>
    /// <param name="fromWarehouseCode">from Warehouse Code 参数。</param>
    /// <param name="fromWarehouseName">from Warehouse Name 参数。</param>
    /// <param name="fromLocationId">from Location Id 参数。</param>
    /// <param name="fromLocationCode">from Location Code 参数。</param>
    /// <param name="fromLocationName">from Location Name 参数。</param>
    /// <param name="toWarehouseId">to Warehouse Id 参数。</param>
    /// <param name="toWarehouseCode">to Warehouse Code 参数。</param>
    /// <param name="toWarehouseName">to Warehouse Name 参数。</param>
    /// <param name="toLocationId">to Location Id 参数。</param>
    /// <param name="toLocationCode">to Location Code 参数。</param>
    /// <param name="toLocationName">to Location Name 参数。</param>
    /// <param name="reason">业务原因。</param>
    /// <param name="executedBy">executed By 参数。</param>
    /// <param name="lines">明细行集合。</param>
    public InventoryTransfer(
        string transferNo,
        Guid fromWarehouseId,
        string fromWarehouseCode,
        string fromWarehouseName,
        Guid? fromLocationId,
        string fromLocationCode,
        string fromLocationName,
        Guid toWarehouseId,
        string toWarehouseCode,
        string toWarehouseName,
        Guid? toLocationId,
        string toLocationCode,
        string toLocationName,
        string reason,
        string executedBy,
        IEnumerable<InventoryTransferLine> lines)
    {
        TransferNo = transferNo;
        FromWarehouseId = fromWarehouseId;
        FromWarehouseCode = fromWarehouseCode;
        FromWarehouseName = fromWarehouseName;
        FromLocationId = fromLocationId;
        FromLocationCode = fromLocationCode;
        FromLocationName = fromLocationName;
        ToWarehouseId = toWarehouseId;
        ToWarehouseCode = toWarehouseCode;
        ToWarehouseName = toWarehouseName;
        ToLocationId = toLocationId;
        ToLocationCode = toLocationCode;
        ToLocationName = toLocationName;
        Reason = reason;
        ExecutedBy = executedBy;
        Lines = lines.ToList();
    }

    /// <summary>
    /// Transfer No。
    /// </summary>
    public string TransferNo { get; private set; } = string.Empty;
    /// <summary>
    /// From Warehouse Id。
    /// </summary>
    public Guid FromWarehouseId { get; private set; }
    /// <summary>
    /// From Warehouse Code。
    /// </summary>
    public string FromWarehouseCode { get; private set; } = string.Empty;
    /// <summary>
    /// From Warehouse Name。
    /// </summary>
    public string FromWarehouseName { get; private set; } = string.Empty;
    /// <summary>
    /// From Location Id。
    /// </summary>
    public Guid? FromLocationId { get; private set; }
    /// <summary>
    /// From Location Code。
    /// </summary>
    public string FromLocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// From Location Name。
    /// </summary>
    public string FromLocationName { get; private set; } = string.Empty;
    /// <summary>
    /// To Warehouse Id。
    /// </summary>
    public Guid ToWarehouseId { get; private set; }
    /// <summary>
    /// To Warehouse Code。
    /// </summary>
    public string ToWarehouseCode { get; private set; } = string.Empty;
    /// <summary>
    /// To Warehouse Name。
    /// </summary>
    public string ToWarehouseName { get; private set; } = string.Empty;
    /// <summary>
    /// To Location Id。
    /// </summary>
    public Guid? ToLocationId { get; private set; }
    /// <summary>
    /// To Location Code。
    /// </summary>
    public string ToLocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// To Location Name。
    /// </summary>
    public string ToLocationName { get; private set; } = string.Empty;
    /// <summary>
    /// Reason。
    /// </summary>
    public string Reason { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = InventoryTransferStatus.Completed;
    /// <summary>
    /// Executed By。
    /// </summary>
    public string ExecutedBy { get; private set; } = string.Empty;
    /// <summary>
    /// 明细行集合。
    /// </summary>
    public List<InventoryTransferLine> Lines { get; private set; } = [];
}
