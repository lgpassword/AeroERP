namespace AeroERP.Modules.Integration.Contracts;

/// <summary>
/// Integration Metric 数据传输对象。
/// </summary>
/// <param name="Key">业务键。</param>
/// <param name="Label">界面显示标签。</param>
/// <param name="Value">数值或配置值。</param>
/// <param name="Unit">计量单位。</param>
public sealed record IntegrationMetricDto(string Key, string Label, decimal Value, string Unit);

/// <summary>
/// Message Channel 数据传输对象。
/// </summary>
public sealed record MessageChannelDto(
    Guid Id,
    string ChannelKey,
    string DisplayName,
    string ChannelType,
    string Endpoint,
    bool IsEnabled,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Webhook Subscription 数据传输对象。
/// </summary>
public sealed record WebhookSubscriptionDto(
    Guid Id,
    string SubscriptionKey,
    string DisplayName,
    string EventKey,
    string TargetUrl,
    string SecretName,
    bool IsEnabled,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// External Connector 数据传输对象。
/// </summary>
public sealed record ExternalConnectorDto(
    Guid Id,
    string ConnectorKey,
    string DisplayName,
    string Provider,
    string BaseUrl,
    string AuthMode,
    bool IsEnabled,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Integration Sync Job 数据传输对象。
/// </summary>
public sealed record IntegrationSyncJobDto(
    Guid Id,
    string JobNo,
    string ConnectorKey,
    string Direction,
    string PayloadJson,
    string Status,
    int AttemptCount,
    string LastError,
    string CreatedBy,
    string CompletedBy,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Integration Audit Record 数据传输对象。
/// </summary>
public sealed record IntegrationAuditRecordDto(
    Guid Id,
    string AuditNo,
    string Category,
    string Action,
    string TargetKey,
    string Result,
    string Message,
    string Actor,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Integration Overview 数据传输对象。
/// </summary>
public sealed record IntegrationOverviewDto(
    IReadOnlyList<MessageChannelDto> Channels,
    IReadOnlyList<WebhookSubscriptionDto> Webhooks,
    IReadOnlyList<ExternalConnectorDto> Connectors,
    IReadOnlyList<IntegrationSyncJobDto> SyncJobs,
    IReadOnlyList<IntegrationAuditRecordDto> AuditRecords,
    IReadOnlyList<IntegrationMetricDto> Metrics);

/// <summary>
/// Upsert Message Channel 请求参数。
/// </summary>
public sealed record UpsertMessageChannelRequest(
    string ChannelKey,
    string DisplayName,
    string ChannelType,
    string Endpoint,
    bool IsEnabled);

/// <summary>
/// Upsert Webhook Subscription 请求参数。
/// </summary>
public sealed record UpsertWebhookSubscriptionRequest(
    string SubscriptionKey,
    string DisplayName,
    string EventKey,
    string TargetUrl,
    string SecretName,
    bool IsEnabled);

/// <summary>
/// Upsert External Connector 请求参数。
/// </summary>
public sealed record UpsertExternalConnectorRequest(
    string ConnectorKey,
    string DisplayName,
    string Provider,
    string BaseUrl,
    string AuthMode,
    bool IsEnabled);

/// <summary>
/// Create Integration Sync Job 请求参数。
/// </summary>
public sealed record CreateIntegrationSyncJobRequest(
    string ConnectorKey,
    string Direction,
    string PayloadJson);

/// <summary>
/// Fail Integration Sync Job 请求参数。
/// </summary>
/// <param name="Error">错误信息。</param>
public sealed record FailIntegrationSyncJobRequest(string Error);
