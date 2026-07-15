using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.AdvancedManufacturing.Domain;

/// <summary>
/// Operation Schedule 业务对象。
/// </summary>
public sealed class OperationSchedule : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Operation Schedule实例。
    /// </summary>
    private OperationSchedule()
    {
    }

    /// <summary>
    /// 初始化Operation Schedule实例。
    /// </summary>
    /// <param name="scheduleNo">schedule No 参数。</param>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="workOrderNo">work Order No 参数。</param>
    /// <param name="routingOperationId">routing Operation Id 参数。</param>
    /// <param name="operationCode">operation Code 参数。</param>
    /// <param name="operationName">operation Name 参数。</param>
    /// <param name="workCenterId">work Center Id 参数。</param>
    /// <param name="workCenterCode">work Center Code 参数。</param>
    /// <param name="workCenterName">work Center Name 参数。</param>
    /// <param name="plannedStartUtc">planned Start Utc 参数。</param>
    /// <param name="plannedEndUtc">planned End Utc 参数。</param>
    /// <param name="plannedQuantity">planned Quantity 参数。</param>
    /// <param name="scheduledBy">scheduled By 参数。</param>
    public OperationSchedule(
        string scheduleNo,
        Guid workOrderId,
        string workOrderNo,
        Guid routingOperationId,
        string operationCode,
        string operationName,
        Guid workCenterId,
        string workCenterCode,
        string workCenterName,
        DateTimeOffset plannedStartUtc,
        DateTimeOffset plannedEndUtc,
        decimal plannedQuantity,
        string scheduledBy)
    {
        ScheduleNo = scheduleNo;
        WorkOrderId = workOrderId;
        WorkOrderNo = workOrderNo;
        RoutingOperationId = routingOperationId;
        OperationCode = operationCode;
        OperationName = operationName;
        WorkCenterId = workCenterId;
        WorkCenterCode = workCenterCode;
        WorkCenterName = workCenterName;
        PlannedStartUtc = plannedStartUtc;
        PlannedEndUtc = plannedEndUtc;
        PlannedQuantity = plannedQuantity;
        ScheduledBy = scheduledBy;
    }

    /// <summary>
    /// Schedule No。
    /// </summary>
    public string ScheduleNo { get; private set; } = string.Empty;
    /// <summary>
    /// Work Order Id。
    /// </summary>
    public Guid WorkOrderId { get; private set; }
    /// <summary>
    /// Work Order No。
    /// </summary>
    public string WorkOrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Routing Operation Id。
    /// </summary>
    public Guid RoutingOperationId { get; private set; }
    /// <summary>
    /// Operation Code。
    /// </summary>
    public string OperationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Operation Name。
    /// </summary>
    public string OperationName { get; private set; } = string.Empty;
    /// <summary>
    /// Work Center Id。
    /// </summary>
    public Guid WorkCenterId { get; private set; }
    /// <summary>
    /// Work Center Code。
    /// </summary>
    public string WorkCenterCode { get; private set; } = string.Empty;
    /// <summary>
    /// Work Center Name。
    /// </summary>
    public string WorkCenterName { get; private set; } = string.Empty;
    /// <summary>
    /// Planned Start Utc。
    /// </summary>
    public DateTimeOffset PlannedStartUtc { get; private set; }
    /// <summary>
    /// Planned End Utc。
    /// </summary>
    public DateTimeOffset PlannedEndUtc { get; private set; }
    /// <summary>
    /// Planned Quantity。
    /// </summary>
    public decimal PlannedQuantity { get; private set; }
    /// <summary>
    /// Completed Quantity。
    /// </summary>
    public decimal CompletedQuantity { get; private set; }
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = AdvancedManufacturingStatus.Planned;
    /// <summary>
    /// Scheduled By。
    /// </summary>
    public string ScheduledBy { get; private set; } = string.Empty;

    /// <summary>
    /// Release。
    /// </summary>
    public void Release()
    {
        Status = AdvancedManufacturingStatus.Released;
        Touch();
    }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="completedQuantity">completed Quantity 参数。</param>
    public void Complete(decimal completedQuantity)
    {
        CompletedQuantity = completedQuantity;
        Status = AdvancedManufacturingStatus.Completed;
        Touch();
    }
}
