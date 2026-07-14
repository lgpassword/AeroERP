using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.MobileWork.Domain;

/// <summary>
/// Mobile Offline Task 业务对象。
/// </summary>
public sealed class MobileOfflineTask : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Mobile Offline Task实例。
    /// </summary>
    private MobileOfflineTask()
    {
    }

    /// <summary>
    /// 初始化Mobile Offline Task实例。
    /// </summary>
    /// <param name="taskNo">task No 参数。</param>
    /// <param name="sourceModule">source Module 参数。</param>
    /// <param name="sourceTaskType">source Task Type 参数。</param>
    /// <param name="sourceTaskNo">source Task No 参数。</param>
    /// <param name="payloadJson">payload Json 参数。</param>
    /// <param name="assignedTo">assigne DTO 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public MobileOfflineTask(string taskNo, string sourceModule, string sourceTaskType, string sourceTaskNo, string payloadJson, string assignedTo, string createdBy)
    {
        TaskNo = taskNo;
        SourceModule = sourceModule;
        SourceTaskType = sourceTaskType;
        SourceTaskNo = sourceTaskNo;
        PayloadJson = payloadJson;
        AssignedTo = assignedTo;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Task No。
    /// </summary>
    public string TaskNo { get; private set; } = string.Empty;
    /// <summary>
    /// Source Module。
    /// </summary>
    public string SourceModule { get; private set; } = string.Empty;
    /// <summary>
    /// Source Task Type。
    /// </summary>
    public string SourceTaskType { get; private set; } = string.Empty;
    /// <summary>
    /// Source Task No。
    /// </summary>
    public string SourceTaskNo { get; private set; } = string.Empty;
    /// <summary>
    /// Payload Json。
    /// </summary>
    public string PayloadJson { get; private set; } = "{}";
    /// <summary>
    /// Assigned To。
    /// </summary>
    public string AssignedTo { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = MobileWorkStatus.Pending;
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
    /// Mark Synced。
    /// </summary>
    public void MarkSynced()
    {
        Status = MobileWorkStatus.Synced;
        Touch();
    }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="actor">操作人。</param>
    public void Complete(string actor)
    {
        Status = MobileWorkStatus.Completed;
        CompletedBy = actor;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
