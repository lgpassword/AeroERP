using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Integration.Domain;

/// <summary>
/// External Connector 业务对象。
/// </summary>
public sealed class ExternalConnector : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化External Connector实例。
    /// </summary>
    private ExternalConnector()
    {
    }

    /// <summary>
    /// 初始化External Connector实例。
    /// </summary>
    /// <param name="connectorKey">connector Key 参数。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="provider">外部提供方。</param>
    /// <param name="baseUrl">base Url 参数。</param>
    /// <param name="authMode">auth Mode 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public ExternalConnector(string connectorKey, string displayName, string provider, string baseUrl, string authMode, bool isEnabled, string updatedBy)
    {
        ConnectorKey = connectorKey;
        DisplayName = displayName;
        Provider = provider;
        BaseUrl = baseUrl;
        AuthMode = authMode;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Connector Key。
    /// </summary>
    public string ConnectorKey { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    /// <summary>
    /// Provider。
    /// </summary>
    public string Provider { get; private set; } = string.Empty;
    /// <summary>
    /// Base Url。
    /// </summary>
    public string BaseUrl { get; private set; } = string.Empty;
    /// <summary>
    /// Auth Mode。
    /// </summary>
    public string AuthMode { get; private set; } = string.Empty;
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
    /// <param name="provider">外部提供方。</param>
    /// <param name="baseUrl">base Url 参数。</param>
    /// <param name="authMode">auth Mode 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string displayName, string provider, string baseUrl, string authMode, bool isEnabled, string updatedBy)
    {
        DisplayName = displayName;
        Provider = provider;
        BaseUrl = baseUrl;
        AuthMode = authMode;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
