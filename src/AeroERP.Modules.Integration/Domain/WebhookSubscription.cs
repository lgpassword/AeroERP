using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Integration.Domain;

/// <summary>
/// Webhook Subscription 业务对象。
/// </summary>
public sealed class WebhookSubscription : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Webhook Subscription实例。
    /// </summary>
    private WebhookSubscription()
    {
    }

    /// <summary>
    /// 初始化Webhook Subscription实例。
    /// </summary>
    /// <param name="subscriptionKey">subscription Key 参数。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="eventKey">event Key 参数。</param>
    /// <param name="targetUrl">target Url 参数。</param>
    /// <param name="secretName">secret Name 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public WebhookSubscription(string subscriptionKey, string displayName, string eventKey, string targetUrl, string secretName, bool isEnabled, string updatedBy)
    {
        SubscriptionKey = subscriptionKey;
        DisplayName = displayName;
        EventKey = eventKey;
        TargetUrl = targetUrl;
        SecretName = secretName;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Subscription Key。
    /// </summary>
    public string SubscriptionKey { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    /// <summary>
    /// Event Key。
    /// </summary>
    public string EventKey { get; private set; } = string.Empty;
    /// <summary>
    /// Target Url。
    /// </summary>
    public string TargetUrl { get; private set; } = string.Empty;
    /// <summary>
    /// Secret Name。
    /// </summary>
    public string SecretName { get; private set; } = string.Empty;
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
    /// <param name="eventKey">event Key 参数。</param>
    /// <param name="targetUrl">target Url 参数。</param>
    /// <param name="secretName">secret Name 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string displayName, string eventKey, string targetUrl, string secretName, bool isEnabled, string updatedBy)
    {
        DisplayName = displayName;
        EventKey = eventKey;
        TargetUrl = targetUrl;
        SecretName = secretName;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
