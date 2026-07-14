using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.DocumentExchange.Domain;

/// <summary>
/// Export File Task 业务对象。
/// </summary>
public sealed class ExportFileTask : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Export File Task实例。
    /// </summary>
    private ExportFileTask()
    {
    }

    /// <summary>
    /// 初始化Export File Task实例。
    /// </summary>
    /// <param name="exportNo">export No 参数。</param>
    /// <param name="sourceModule">source Module 参数。</param>
    /// <param name="fileName">file Name 参数。</param>
    /// <param name="format">文件或报表格式。</param>
    /// <param name="requestedBy">requested By 参数。</param>
    public ExportFileTask(string exportNo, string sourceModule, string fileName, string format, string requestedBy)
    {
        ExportNo = exportNo;
        SourceModule = sourceModule;
        FileName = fileName;
        Format = format;
        RequestedBy = requestedBy;
    }

    /// <summary>
    /// Export No。
    /// </summary>
    public string ExportNo { get; private set; } = string.Empty;
    /// <summary>
    /// Source Module。
    /// </summary>
    public string SourceModule { get; private set; } = string.Empty;
    /// <summary>
    /// File Name。
    /// </summary>
    public string FileName { get; private set; } = string.Empty;
    /// <summary>
    /// Format。
    /// </summary>
    public string Format { get; private set; } = string.Empty;
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
