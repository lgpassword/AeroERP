using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Issue 业务对象。
/// </summary>
public sealed class InventoryIssue : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Inventory Issue实例。
    /// </summary>
    private InventoryIssue()
    {
    }

    /// <summary>
    /// 初始化Inventory Issue实例。
    /// </summary>
    /// <param name="issueNo">issue No 参数。</param>
    /// <param name="salesOrderId">sales Order Id 参数。</param>
    /// <param name="salesOrderNo">sales Order No 参数。</param>
    /// <param name="quotationNo">quotation No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="locationCode">location Code 参数。</param>
    /// <param name="locationName">location Name 参数。</param>
    /// <param name="customerName">customer Name 参数。</param>
    /// <param name="issuedBy">issued By 参数。</param>
    /// <param name="lines">明细行集合。</param>
    public InventoryIssue(
        string issueNo,
        Guid salesOrderId,
        string salesOrderNo,
        string quotationNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid? locationId,
        string locationCode,
        string locationName,
        string customerName,
        string issuedBy,
        IEnumerable<InventoryIssueLine> lines)
    {
        IssueNo = issueNo;
        SalesOrderId = salesOrderId;
        SalesOrderNo = salesOrderNo;
        QuotationNo = quotationNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        LocationId = locationId;
        LocationCode = locationCode;
        LocationName = locationName;
        CustomerName = customerName;
        IssuedBy = issuedBy;
        Lines = lines.ToList();
    }

    /// <summary>
    /// Issue No。
    /// </summary>
    public string IssueNo { get; private set; } = string.Empty;
    /// <summary>
    /// Sales Order Id。
    /// </summary>
    public Guid SalesOrderId { get; private set; }
    /// <summary>
    /// Sales Order No。
    /// </summary>
    public string SalesOrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Quotation No。
    /// </summary>
    public string QuotationNo { get; private set; } = string.Empty;
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
    /// Customer Name。
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = InventoryIssueStatus.Completed;
    /// <summary>
    /// Issued By。
    /// </summary>
    public string IssuedBy { get; private set; } = string.Empty;
    /// <summary>
    /// 明细行集合。
    /// </summary>
    public List<InventoryIssueLine> Lines { get; private set; } = [];
}
