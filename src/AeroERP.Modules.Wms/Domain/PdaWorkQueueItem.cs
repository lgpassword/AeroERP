using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Wms.Domain;

/// <summary>
/// Pda Work Queue Item 业务对象。
/// </summary>
public sealed class PdaWorkQueueItem : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Pda Work Queue Item实例。
    /// </summary>
    private PdaWorkQueueItem()
    {
    }

    /// <summary>
    /// 初始化Pda Work Queue Item实例。
    /// </summary>
    /// <param name="taskType">task Type 参数。</param>
    /// <param name="taskId">task Id 参数。</param>
    /// <param name="taskNo">task No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="locationCode">location Code 参数。</param>
    /// <param name="assignedTo">assigne DTO 参数。</param>
    /// <param name="priority">优先级。</param>
    public PdaWorkQueueItem(string taskType, Guid taskId, string taskNo, Guid warehouseId, string warehouseName, string locationCode, string assignedTo, int priority)
    {
        TaskType = taskType;
        TaskId = taskId;
        TaskNo = taskNo;
        WarehouseId = warehouseId;
        WarehouseName = warehouseName;
        LocationCode = locationCode;
        AssignedTo = assignedTo;
        Priority = priority;
    }

    /// <summary>
    /// Task Type。
    /// </summary>
    public string TaskType { get; private set; } = string.Empty;
    /// <summary>
    /// Task Id。
    /// </summary>
    public Guid TaskId { get; private set; }
    /// <summary>
    /// Task No。
    /// </summary>
    public string TaskNo { get; private set; } = string.Empty;
    /// <summary>
    /// Warehouse Id。
    /// </summary>
    public Guid WarehouseId { get; private set; }
    /// <summary>
    /// Warehouse Name。
    /// </summary>
    public string WarehouseName { get; private set; } = string.Empty;
    /// <summary>
    /// Location Code。
    /// </summary>
    public string LocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Assigned To。
    /// </summary>
    public string AssignedTo { get; private set; } = string.Empty;
    /// <summary>
    /// Priority。
    /// </summary>
    public int Priority { get; private set; }
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = WmsTaskStatus.Planned;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Release。
    /// </summary>
    public void Release()
    {
        Status = WmsTaskStatus.Released;
        Touch();
    }

    /// <summary>
    /// Complete。
    /// </summary>
    public void Complete()
    {
        Status = WmsTaskStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
