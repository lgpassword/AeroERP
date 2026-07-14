using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Wms.Domain;

/// <summary>
/// Put Away Task 业务对象。
/// </summary>
public sealed class PutAwayTask : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Put Away Task实例。
    /// </summary>
    private PutAwayTask()
    {
    }

    /// <summary>
    /// 初始化Put Away Task实例。
    /// </summary>
    /// <param name="taskNo">task No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="suggestedLocationId">suggested Location Id 参数。</param>
    /// <param name="suggestedLocationCode">suggested Location Code 参数。</param>
    /// <param name="suggestedLocationName">suggested Location Name 参数。</param>
    /// <param name="containerCode">container Code 参数。</param>
    /// <param name="sourceDocumentNo">source Document No 参数。</param>
    /// <param name="assignedTo">assigne DTO 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public PutAwayTask(
        string taskNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal quantity,
        string unit,
        Guid? suggestedLocationId,
        string suggestedLocationCode,
        string suggestedLocationName,
        string containerCode,
        string sourceDocumentNo,
        string assignedTo,
        string createdBy)
    {
        TaskNo = taskNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        Quantity = quantity;
        Unit = unit;
        SuggestedLocationId = suggestedLocationId;
        SuggestedLocationCode = suggestedLocationCode;
        SuggestedLocationName = suggestedLocationName;
        ContainerCode = containerCode;
        SourceDocumentNo = sourceDocumentNo;
        AssignedTo = assignedTo;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Task No。
    /// </summary>
    public string TaskNo { get; private set; } = string.Empty;
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
    /// Item Id。
    /// </summary>
    public Guid ItemId { get; private set; }
    /// <summary>
    /// Item Code。
    /// </summary>
    public string ItemCode { get; private set; } = string.Empty;
    /// <summary>
    /// Item Name。
    /// </summary>
    public string ItemName { get; private set; } = string.Empty;
    /// <summary>
    /// 数量。
    /// </summary>
    public decimal Quantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// Suggested Location Id。
    /// </summary>
    public Guid? SuggestedLocationId { get; private set; }
    /// <summary>
    /// Suggested Location Code。
    /// </summary>
    public string SuggestedLocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Suggested Location Name。
    /// </summary>
    public string SuggestedLocationName { get; private set; } = string.Empty;
    /// <summary>
    /// Container Code。
    /// </summary>
    public string ContainerCode { get; private set; } = string.Empty;
    /// <summary>
    /// Source Document No。
    /// </summary>
    public string SourceDocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = WmsTaskStatus.Planned;
    /// <summary>
    /// Assigned To。
    /// </summary>
    public string AssignedTo { get; private set; } = string.Empty;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed By。
    /// </summary>
    public string CompletedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="targetLocationId">target Location Id 参数。</param>
    /// <param name="targetLocationCode">target Location Code 参数。</param>
    /// <param name="targetLocationName">target Location Name 参数。</param>
    /// <param name="actor">操作人。</param>
    public void Complete(Guid targetLocationId, string targetLocationCode, string targetLocationName, string actor)
    {
        SuggestedLocationId = targetLocationId;
        SuggestedLocationCode = targetLocationCode;
        SuggestedLocationName = targetLocationName;
        Status = WmsTaskStatus.Completed;
        CompletedBy = actor;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
