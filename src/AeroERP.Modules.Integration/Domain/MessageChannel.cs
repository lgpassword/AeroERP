using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Integration.Domain;

/// <summary>
/// Message Channel 业务对象。
/// </summary>
public sealed class MessageChannel : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Message Channel实例。
    /// </summary>
    private MessageChannel()
    {
    }

    /// <summary>
    /// 初始化Message Channel实例。
    /// </summary>
    /// <param name="channelKey">channel Key 参数。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="channelType">channel Type 参数。</param>
    /// <param name="endpoint">外部端点地址。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public MessageChannel(string channelKey, string displayName, string channelType, string endpoint, bool isEnabled, string updatedBy)
    {
        ChannelKey = channelKey;
        DisplayName = displayName;
        ChannelType = channelType;
        Endpoint = endpoint;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Channel Key。
    /// </summary>
    public string ChannelKey { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    /// <summary>
    /// Channel Type。
    /// </summary>
    public string ChannelType { get; private set; } = string.Empty;
    /// <summary>
    /// Endpoint。
    /// </summary>
    public string Endpoint { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="channelType">channel Type 参数。</param>
    /// <param name="endpoint">外部端点地址。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string displayName, string channelType, string endpoint, bool isEnabled, string updatedBy)
    {
        DisplayName = displayName;
        ChannelType = channelType;
        Endpoint = endpoint;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
