using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.AdvancedManufacturing.Domain;

/// <summary>
/// Capacity Load 业务对象。
/// </summary>
public sealed class CapacityLoad : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Capacity Load实例。
    /// </summary>
    private CapacityLoad()
    {
    }

    /// <summary>
    /// 初始化Capacity Load实例。
    /// </summary>
    /// <param name="workCenterId">work Center Id 参数。</param>
    /// <param name="workCenterCode">work Center Code 参数。</param>
    /// <param name="workCenterName">work Center Name 参数。</param>
    /// <param name="planDate">plan Date 参数。</param>
    /// <param name="availableMinutes">available Minutes 参数。</param>
    /// <param name="reservedMinutes">reserved Minutes 参数。</param>
    /// <param name="sourceDocumentNo">source Document No 参数。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public CapacityLoad(
        Guid workCenterId,
        string workCenterCode,
        string workCenterName,
        DateOnly planDate,
        decimal availableMinutes,
        decimal reservedMinutes,
        string sourceDocumentNo,
        string updatedBy)
    {
        WorkCenterId = workCenterId;
        WorkCenterCode = workCenterCode;
        WorkCenterName = workCenterName;
        PlanDate = planDate;
        AvailableMinutes = availableMinutes;
        ReservedMinutes = reservedMinutes;
        SourceDocumentNo = sourceDocumentNo;
        UpdatedBy = updatedBy;
    }

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
    /// Plan Date。
    /// </summary>
    public DateOnly PlanDate { get; private set; }
    /// <summary>
    /// Available Minutes。
    /// </summary>
    public decimal AvailableMinutes { get; private set; }
    /// <summary>
    /// Reserved Minutes。
    /// </summary>
    public decimal ReservedMinutes { get; private set; }
    /// <summary>
    /// Source Document No。
    /// </summary>
    public string SourceDocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;
    public decimal RemainingMinutes => AvailableMinutes - ReservedMinutes;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="availableMinutes">available Minutes 参数。</param>
    /// <param name="reservedMinutes">reserved Minutes 参数。</param>
    /// <param name="sourceDocumentNo">source Document No 参数。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(decimal availableMinutes, decimal reservedMinutes, string sourceDocumentNo, string updatedBy)
    {
        AvailableMinutes = availableMinutes;
        ReservedMinutes = reservedMinutes;
        SourceDocumentNo = sourceDocumentNo;
        UpdatedBy = updatedBy;
        Touch();
    }
}
