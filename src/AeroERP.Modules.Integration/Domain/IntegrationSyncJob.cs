using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Integration.Domain;

/// <summary>
/// Integration Sync Job 业务对象。
/// </summary>
public sealed class IntegrationSyncJob : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Integration Sync Job实例。
    /// </summary>
    private IntegrationSyncJob()
    {
    }

    /// <summary>
    /// 初始化Integration Sync Job实例。
    /// </summary>
    /// <param name="jobNo">job No 参数。</param>
    /// <param name="connectorKey">connector Key 参数。</param>
    /// <param name="direction">业务方向。</param>
    /// <param name="payloadJson">payload Json 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public IntegrationSyncJob(string jobNo, string connectorKey, string direction, string payloadJson, string createdBy)
    {
        JobNo = jobNo;
        ConnectorKey = connectorKey;
        Direction = direction;
        PayloadJson = payloadJson;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Job No。
    /// </summary>
    public string JobNo { get; private set; } = string.Empty;
    /// <summary>
    /// Connector Key。
    /// </summary>
    public string ConnectorKey { get; private set; } = string.Empty;
    /// <summary>
    /// Direction。
    /// </summary>
    public string Direction { get; private set; } = string.Empty;
    /// <summary>
    /// Payload Json。
    /// </summary>
    public string PayloadJson { get; private set; } = "{}";
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = IntegrationStatus.Pending;
    /// <summary>
    /// Attempt Count。
    /// </summary>
    public int AttemptCount { get; private set; }
    /// <summary>
    /// Last Error。
    /// </summary>
    public string LastError { get; private set; } = string.Empty;
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
    /// Mark Running。
    /// </summary>
    public void MarkRunning()
    {
        AttemptCount++;
        Status = IntegrationStatus.Running;
        LastError = string.Empty;
        Touch();
    }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="actor">操作人。</param>
    public void Complete(string actor)
    {
        Status = IntegrationStatus.Completed;
        CompletedBy = actor;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Fail。
    /// </summary>
    /// <param name="error">错误信息。</param>
    public void Fail(string error)
    {
        Status = IntegrationStatus.Failed;
        LastError = error;
        Touch();
    }

    /// <summary>
    /// Retry。
    /// </summary>
    public void Retry()
    {
        Status = IntegrationStatus.Pending;
        LastError = string.Empty;
        Touch();
    }
}
