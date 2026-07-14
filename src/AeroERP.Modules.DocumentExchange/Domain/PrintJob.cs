using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.DocumentExchange.Domain;

/// <summary>
/// Print Job 业务对象。
/// </summary>
public sealed class PrintJob : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Print Job实例。
    /// </summary>
    private PrintJob()
    {
    }

    /// <summary>
    /// 初始化Print Job实例。
    /// </summary>
    /// <param name="jobNo">job No 参数。</param>
    /// <param name="templateKey">template Key 参数。</param>
    /// <param name="documentNo">业务单据编号。</param>
    /// <param name="requestedBy">requested By 参数。</param>
    public PrintJob(string jobNo, string templateKey, string documentNo, string requestedBy)
    {
        JobNo = jobNo;
        TemplateKey = templateKey;
        DocumentNo = documentNo;
        RequestedBy = requestedBy;
    }

    /// <summary>
    /// Job No。
    /// </summary>
    public string JobNo { get; private set; } = string.Empty;
    /// <summary>
    /// Template Key。
    /// </summary>
    public string TemplateKey { get; private set; } = string.Empty;
    /// <summary>
    /// 业务单据编号。
    /// </summary>
    public string DocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = DocumentExchangeStatus.Pending;
    /// <summary>
    /// Requested By。
    /// </summary>
    public string RequestedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed By。
    /// </summary>
    public string CompletedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="actor">操作人。</param>
    public void Complete(string actor)
    {
        Status = DocumentExchangeStatus.Completed;
        CompletedBy = actor;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Fail。
    /// </summary>
    public void Fail()
    {
        Status = DocumentExchangeStatus.Failed;
        Touch();
    }
}
