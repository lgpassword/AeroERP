using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Integration.Contracts;

namespace AeroERP.Modules.Integration.Services;

/// <summary>
/// Integration Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IIntegrationService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IntegrationOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Channel。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MessageChannelDto>> UpsertChannelAsync(UpsertMessageChannelRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Webhook。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<WebhookSubscriptionDto>> UpsertWebhookAsync(UpsertWebhookSubscriptionRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Connector。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ExternalConnectorDto>> UpsertConnectorAsync(UpsertExternalConnectorRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Sync Job。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<IntegrationSyncJobDto>> CreateSyncJobAsync(CreateIntegrationSyncJobRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Start Sync Job。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<IntegrationSyncJobDto>> StartSyncJobAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Sync Job。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<IntegrationSyncJobDto>> CompleteSyncJobAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Fail Sync Job。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<IntegrationSyncJobDto>> FailSyncJobAsync(Guid id, FailIntegrationSyncJobRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Retry Sync Job。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<IntegrationSyncJobDto>> RetrySyncJobAsync(Guid id, CancellationToken cancellationToken);
}
