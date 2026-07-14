using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.MobileWork.Domain;

/// <summary>
/// Mobile Device 业务对象。
/// </summary>
public sealed class MobileDevice : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Mobile Device实例。
    /// </summary>
    private MobileDevice()
    {
    }

    /// <summary>
    /// 初始化Mobile Device实例。
    /// </summary>
    /// <param name="deviceCode">device Code 参数。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="assignedTo">assigne DTO 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public MobileDevice(string deviceCode, string displayName, string assignedTo, bool isEnabled, string updatedBy)
    {
        DeviceCode = deviceCode;
        DisplayName = displayName;
        AssignedTo = assignedTo;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        LastSeenAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Device Code。
    /// </summary>
    public string DeviceCode { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    /// <summary>
    /// Assigned To。
    /// </summary>
    public string AssignedTo { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Last Seen At Utc。
    /// </summary>
    public DateTimeOffset LastSeenAtUtc { get; private set; }

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="assignedTo">assigne DTO 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string displayName, string assignedTo, bool isEnabled, string updatedBy)
    {
        DisplayName = displayName;
        AssignedTo = assignedTo;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        LastSeenAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
