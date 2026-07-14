using System.Text.Json;
using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Integration.Contracts;
using AeroERP.Modules.Integration.Domain;
using AeroERP.Modules.Integration.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Integration Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class IntegrationService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IIntegrationService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IntegrationOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var channels = await dbContext.MessageChannels.AsNoTracking().OrderBy(x => x.ChannelKey).ToListAsync(cancellationToken);
        var webhooks = await dbContext.WebhookSubscriptions.AsNoTracking().OrderBy(x => x.EventKey).ThenBy(x => x.SubscriptionKey).ToListAsync(cancellationToken);
        var connectors = await dbContext.ExternalConnectors.AsNoTracking().OrderBy(x => x.ConnectorKey).ToListAsync(cancellationToken);
        var syncJobs = (await dbContext.IntegrationSyncJobs.AsNoTracking().ToListAsync(cancellationToken))
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var auditRecords = (await dbContext.IntegrationAuditRecords.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(80)
            .ToList();

        var metrics = new List<IntegrationMetricDto>
        {
            new("enabled-channels", "启用通道", channels.Count(x => x.IsEnabled), "个"),
            new("enabled-webhooks", "启用 Webhook", webhooks.Count(x => x.IsEnabled), "个"),
            new("enabled-connectors", "启用连接器", connectors.Count(x => x.IsEnabled), "个"),
            new("failed-jobs", "失败任务", syncJobs.Count(x => x.Status == IntegrationStatus.Failed), "条")
        };

        return new IntegrationOverviewDto(
            channels.Select(MapChannel).ToList(),
            webhooks.Select(MapWebhook).ToList(),
            connectors.Select(MapConnector).ToList(),
            syncJobs.Select(MapSyncJob).ToList(),
            auditRecords.Select(MapAudit).ToList(),
            metrics);
    }

    /// <summary>
    /// Upsert Channel Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MessageChannelDto>> UpsertChannelAsync(UpsertMessageChannelRequest request, CancellationToken cancellationToken)
    {
        var key = NormalizeKey(request.ChannelKey);
        var displayName = NormalizeText(request.DisplayName);
        var channelType = NormalizeText(request.ChannelType);
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(channelType))
        {
            return OperationResult<MessageChannelDto>.Failure("通道编码、名称和类型不能为空。");
        }

        var actor = currentUser.GetActor();
        var channel = await dbContext.MessageChannels.FirstOrDefaultAsync(x => x.ChannelKey == key, cancellationToken);
        if (channel is null)
        {
            channel = new MessageChannel(key, displayName, channelType, NormalizeText(request.Endpoint), request.IsEnabled, actor);
            dbContext.MessageChannels.Add(channel);
        }
        else
        {
            channel.Update(displayName, channelType, NormalizeText(request.Endpoint), request.IsEnabled, actor);
        }

        AddIntegrationAudit("Channel", "ChannelUpserted", key, "Success", "消息通道已保存。", actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "ChannelUpserted", actor, key, cancellationToken);
        return OperationResult<MessageChannelDto>.Success(MapChannel(channel));
    }

    /// <summary>
    /// Upsert Webhook Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<WebhookSubscriptionDto>> UpsertWebhookAsync(UpsertWebhookSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var key = NormalizeKey(request.SubscriptionKey);
        var displayName = NormalizeText(request.DisplayName);
        var eventKey = NormalizeKey(request.EventKey);
        var targetUrl = NormalizeText(request.TargetUrl);
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(eventKey) || string.IsNullOrWhiteSpace(targetUrl))
        {
            return OperationResult<WebhookSubscriptionDto>.Failure("Webhook 编码、名称、事件键和目标地址不能为空。");
        }

        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out _))
        {
            return OperationResult<WebhookSubscriptionDto>.Failure("Webhook 目标地址必须是完整 URL。");
        }

        var actor = currentUser.GetActor();
        var webhook = await dbContext.WebhookSubscriptions.FirstOrDefaultAsync(x => x.SubscriptionKey == key, cancellationToken);
        if (webhook is null)
        {
            webhook = new WebhookSubscription(key, displayName, eventKey, targetUrl, NormalizeText(request.SecretName), request.IsEnabled, actor);
            dbContext.WebhookSubscriptions.Add(webhook);
        }
        else
        {
            webhook.Update(displayName, eventKey, targetUrl, NormalizeText(request.SecretName), request.IsEnabled, actor);
        }

        AddIntegrationAudit("Webhook", "WebhookUpserted", key, "Success", "Webhook 订阅已保存。", actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "WebhookUpserted", actor, key, cancellationToken);
        return OperationResult<WebhookSubscriptionDto>.Success(MapWebhook(webhook));
    }

    /// <summary>
    /// Upsert Connector Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ExternalConnectorDto>> UpsertConnectorAsync(UpsertExternalConnectorRequest request, CancellationToken cancellationToken)
    {
        var key = NormalizeKey(request.ConnectorKey);
        var displayName = NormalizeText(request.DisplayName);
        var provider = NormalizeText(request.Provider);
        var baseUrl = NormalizeText(request.BaseUrl);
        var authMode = NormalizeText(request.AuthMode);
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(authMode))
        {
            return OperationResult<ExternalConnectorDto>.Failure("连接器编码、名称、供应方、基础地址和认证方式不能为空。");
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            return OperationResult<ExternalConnectorDto>.Failure("连接器基础地址必须是完整 URL。");
        }

        var actor = currentUser.GetActor();
        var connector = await dbContext.ExternalConnectors.FirstOrDefaultAsync(x => x.ConnectorKey == key, cancellationToken);
        if (connector is null)
        {
            connector = new ExternalConnector(key, displayName, provider, baseUrl, authMode, request.IsEnabled, actor);
            dbContext.ExternalConnectors.Add(connector);
        }
        else
        {
            connector.Update(displayName, provider, baseUrl, authMode, request.IsEnabled, actor);
        }

        AddIntegrationAudit("Connector", "ConnectorUpserted", key, "Success", "外部连接器已保存。", actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "ConnectorUpserted", actor, key, cancellationToken);
        return OperationResult<ExternalConnectorDto>.Success(MapConnector(connector));
    }

    /// <summary>
    /// 创建Sync Job。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<IntegrationSyncJobDto>> CreateSyncJobAsync(CreateIntegrationSyncJobRequest request, CancellationToken cancellationToken)
    {
        var connectorKey = NormalizeKey(request.ConnectorKey);
        var direction = NormalizeText(request.Direction);
        if (direction is not "Inbound" and not "Outbound")
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("同步方向仅支持 Inbound 或 Outbound。");
        }

        var connector = await dbContext.ExternalConnectors.AsNoTracking().FirstOrDefaultAsync(x => x.ConnectorKey == connectorKey && x.IsEnabled, cancellationToken);
        if (connector is null)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("连接器不存在或已停用。");
        }

        string payloadJson;
        try
        {
            payloadJson = NormalizeJson(request.PayloadJson);
        }
        catch (JsonException)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("同步任务载荷 JSON 格式无效。");
        }

        var actor = currentUser.GetActor();
        var job = new IntegrationSyncJob(NextNo("IS"), connectorKey, direction, payloadJson, actor);
        dbContext.IntegrationSyncJobs.Add(job);
        AddIntegrationAudit("SyncJob", "SyncJobCreated", job.JobNo, "Success", "同步任务已创建。", actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "SyncJobCreated", actor, job.JobNo, cancellationToken);
        return OperationResult<IntegrationSyncJobDto>.Success(MapSyncJob(job));
    }

    /// <summary>
    /// Start Sync Job Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<IntegrationSyncJobDto>> StartSyncJobAsync(Guid id, CancellationToken cancellationToken)
    {
        var job = await dbContext.IntegrationSyncJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job is null)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("同步任务不存在。");
        }

        if (job.Status == IntegrationStatus.Completed || job.Status == IntegrationStatus.Running)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("已完成或运行中的同步任务不能再次开始。");
        }

        var actor = currentUser.GetActor();
        job.MarkRunning();
        AddIntegrationAudit("SyncJob", "SyncJobStarted", job.JobNo, "Success", "同步任务已开始。", actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "SyncJobStarted", actor, job.JobNo, cancellationToken);
        return OperationResult<IntegrationSyncJobDto>.Success(MapSyncJob(job));
    }

    /// <summary>
    /// Complete Sync Job Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<IntegrationSyncJobDto>> CompleteSyncJobAsync(Guid id, CancellationToken cancellationToken)
    {
        var job = await dbContext.IntegrationSyncJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job is null)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("同步任务不存在。");
        }

        if (job.Status == IntegrationStatus.Completed)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("同步任务已完成。");
        }

        var actor = currentUser.GetActor();
        job.Complete(actor);
        AddIntegrationAudit("SyncJob", "SyncJobCompleted", job.JobNo, "Success", "同步任务已完成。", actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "SyncJobCompleted", actor, job.JobNo, cancellationToken);
        return OperationResult<IntegrationSyncJobDto>.Success(MapSyncJob(job));
    }

    /// <summary>
    /// Fail Sync Job Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<IntegrationSyncJobDto>> FailSyncJobAsync(Guid id, FailIntegrationSyncJobRequest request, CancellationToken cancellationToken)
    {
        var job = await dbContext.IntegrationSyncJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job is null)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("同步任务不存在。");
        }

        if (job.Status == IntegrationStatus.Completed)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("已完成的同步任务不能标记失败。");
        }

        var error = NormalizeText(request.Error);
        if (string.IsNullOrWhiteSpace(error))
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("失败原因不能为空。");
        }

        var actor = currentUser.GetActor();
        job.Fail(error);
        AddIntegrationAudit("SyncJob", "SyncJobFailed", job.JobNo, "Failed", error, actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "SyncJobFailed", actor, job.JobNo, cancellationToken);
        return OperationResult<IntegrationSyncJobDto>.Success(MapSyncJob(job));
    }

    /// <summary>
    /// Retry Sync Job Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<IntegrationSyncJobDto>> RetrySyncJobAsync(Guid id, CancellationToken cancellationToken)
    {
        var job = await dbContext.IntegrationSyncJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job is null)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("同步任务不存在。");
        }

        if (job.Status != IntegrationStatus.Failed)
        {
            return OperationResult<IntegrationSyncJobDto>.Failure("只有失败的同步任务可以重试。");
        }

        var actor = currentUser.GetActor();
        job.Retry();
        AddIntegrationAudit("SyncJob", "SyncJobRetryRequested", job.JobNo, "Success", "同步任务已进入待重试。", actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Integration", "SyncJobRetryRequested", actor, job.JobNo, cancellationToken);
        return OperationResult<IntegrationSyncJobDto>.Success(MapSyncJob(job));
    }

    /// <summary>
    /// Add Integration Audit。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="targetKey">target Key 参数。</param>
    /// <param name="result">执行结果。</param>
    /// <param name="message">执行消息。</param>
    /// <param name="actor">操作人。</param>
    private void AddIntegrationAudit(string category, string action, string targetKey, string result, string message, string actor)
    {
        dbContext.IntegrationAuditRecords.Add(new IntegrationAuditRecord(NextNo("IA"), category, action, targetKey, result, message, actor));
    }

    /// <summary>
    /// Next No。
    /// </summary>
    /// <param name="prefix">编号前缀。</param>
    private static string NextNo(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    /// <summary>
    /// Normalize Text。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeText(string value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Normalize Key。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeKey(string value) => NormalizeText(value).ToLowerInvariant();

    /// <summary>
    /// Normalize Json。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeJson(string value)
    {
        var text = NormalizeText(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return "{}";
        }

        using var document = JsonDocument.Parse(text);
        return document.RootElement.GetRawText();
    }

    /// <summary>
    /// 注册Channel 路由。
    /// </summary>
    /// <param name="channel">消息通道。</param>
    private static MessageChannelDto MapChannel(MessageChannel channel) =>
        new(channel.Id, channel.ChannelKey, channel.DisplayName, channel.ChannelType, channel.Endpoint, channel.IsEnabled, channel.UpdatedBy, channel.UpdatedAtUtc);

    /// <summary>
    /// 注册Webhook 路由。
    /// </summary>
    /// <param name="webhook">Webhook 订阅。</param>
    private static WebhookSubscriptionDto MapWebhook(WebhookSubscription webhook) =>
        new(webhook.Id, webhook.SubscriptionKey, webhook.DisplayName, webhook.EventKey, webhook.TargetUrl, webhook.SecretName, webhook.IsEnabled, webhook.UpdatedBy, webhook.UpdatedAtUtc);

    /// <summary>
    /// 注册Connector 路由。
    /// </summary>
    /// <param name="connector">外部连接器。</param>
    private static ExternalConnectorDto MapConnector(ExternalConnector connector) =>
        new(connector.Id, connector.ConnectorKey, connector.DisplayName, connector.Provider, connector.BaseUrl, connector.AuthMode, connector.IsEnabled, connector.UpdatedBy, connector.UpdatedAtUtc);

    /// <summary>
    /// 注册Sync Job 路由。
    /// </summary>
    /// <param name="job">任务对象。</param>
    private static IntegrationSyncJobDto MapSyncJob(IntegrationSyncJob job) =>
        new(job.Id, job.JobNo, job.ConnectorKey, job.Direction, job.PayloadJson, job.Status, job.AttemptCount, job.LastError, job.CreatedBy, job.CompletedBy, job.CompletedAtUtc, job.UpdatedAtUtc);

    /// <summary>
    /// 注册Audit 路由。
    /// </summary>
    /// <param name="audit">审计记录。</param>
    private static IntegrationAuditRecordDto MapAudit(IntegrationAuditRecord audit) =>
        new(audit.Id, audit.AuditNo, audit.Category, audit.Action, audit.TargetKey, audit.Result, audit.Message, audit.Actor, audit.CreatedAtUtc);
}
