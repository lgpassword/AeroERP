using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Receipt 业务对象。
/// </summary>
public sealed class InventoryReceipt : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Inventory Receipt实例。
    /// </summary>
    private InventoryReceipt()
    {
    }

    /// <summary>
    /// 初始化Inventory Receipt实例。
    /// </summary>
    /// <param name="receiptNo">receipt No 参数。</param>
    /// <param name="procurementOrderId">procurement Order Id 参数。</param>
    /// <param name="procurementOrderNo">procurement Order No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="locationCode">location Code 参数。</param>
    /// <param name="locationName">location Name 参数。</param>
    /// <param name="supplierName">supplier Name 参数。</param>
    /// <param name="receivedBy">received By 参数。</param>
    /// <param name="lines">明细行集合。</param>
    public InventoryReceipt(
        string receiptNo,
        Guid procurementOrderId,
        string procurementOrderNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid? locationId,
        string locationCode,
        string locationName,
        string supplierName,
        string receivedBy,
        IEnumerable<InventoryReceiptLine> lines)
    {
        ReceiptNo = receiptNo;
        ProcurementOrderId = procurementOrderId;
        ProcurementOrderNo = procurementOrderNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        LocationId = locationId;
        LocationCode = locationCode;
        LocationName = locationName;
        SupplierName = supplierName;
        ReceivedBy = receivedBy;
        Lines = lines.ToList();
    }

    /// <summary>
    /// Receipt No。
    /// </summary>
    public string ReceiptNo { get; private set; } = string.Empty;
    /// <summary>
    /// Procurement Order Id。
    /// </summary>
    public Guid ProcurementOrderId { get; private set; }
    /// <summary>
    /// Procurement Order No。
    /// </summary>
    public string ProcurementOrderNo { get; private set; } = string.Empty;
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
    /// Supplier Name。
    /// </summary>
    public string SupplierName { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = InventoryReceiptStatus.Completed;
    /// <summary>
    /// Received By。
    /// </summary>
    public string ReceivedBy { get; private set; } = string.Empty;
    /// <summary>
    /// 明细行集合。
    /// </summary>
    public List<InventoryReceiptLine> Lines { get; private set; } = [];
}
