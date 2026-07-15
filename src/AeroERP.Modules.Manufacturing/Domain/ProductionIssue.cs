using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Manufacturing.Domain;

/// <summary>
/// Production Issue 业务对象。
/// </summary>
public sealed class ProductionIssue : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Production Issue实例。
    /// </summary>
    private ProductionIssue()
    {
    }

    /// <summary>
    /// 初始化Production Issue实例。
    /// </summary>
    /// <param name="issueNo">issue No 参数。</param>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="workOrderNo">work Order No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="issuedBy">issued By 参数。</param>
    /// <param name="lines">明细行集合。</param>
    public ProductionIssue(
        string issueNo,
        Guid workOrderId,
        string workOrderNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        string issuedBy,
        IEnumerable<ProductionIssueLine> lines)
    {
        IssueNo = issueNo;
        WorkOrderId = workOrderId;
        WorkOrderNo = workOrderNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        IssuedBy = issuedBy;
        Lines = lines.ToList();
    }

    /// <summary>
    /// Issue No。
    /// </summary>
    public string IssueNo { get; private set; } = string.Empty;
    /// <summary>
    /// Work Order Id。
    /// </summary>
    public Guid WorkOrderId { get; private set; }
    /// <summary>
    /// Work Order No。
    /// </summary>
    public string WorkOrderNo { get; private set; } = string.Empty;
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
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = ProductionDocumentStatus.Completed;
    /// <summary>
    /// Issued By。
    /// </summary>
    public string IssuedBy { get; private set; } = string.Empty;
    /// <summary>
    /// 明细行集合。
    /// </summary>
    public List<ProductionIssueLine> Lines { get; private set; } = [];
}
