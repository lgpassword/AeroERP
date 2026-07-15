using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Reporting.Domain;

/// <summary>
/// Report Export Task 业务对象。
/// </summary>
public sealed class ReportExportTask : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Report Export Task实例。
    /// </summary>
    private ReportExportTask()
    {
    }

    /// <summary>
    /// 初始化Report Export Task实例。
    /// </summary>
    /// <param name="exportNo">export No 参数。</param>
    /// <param name="reportRunRecordId">report Run Record Id 参数。</param>
    /// <param name="reportName">report Name 参数。</param>
    /// <param name="format">文件或报表格式。</param>
    /// <param name="fileName">file Name 参数。</param>
    /// <param name="requestedBy">requested By 参数。</param>
    public ReportExportTask(string exportNo, Guid reportRunRecordId, string reportName, string format, string fileName, string requestedBy)
    {
        ExportNo = exportNo;
        ReportRunRecordId = reportRunRecordId;
        ReportName = reportName;
        Format = format;
        FileName = fileName;
        RequestedBy = requestedBy;
    }

    /// <summary>
    /// Export No。
    /// </summary>
    public string ExportNo { get; private set; } = string.Empty;
    /// <summary>
    /// Report Run Record Id。
    /// </summary>
    public Guid ReportRunRecordId { get; private set; }
    /// <summary>
    /// Report Name。
    /// </summary>
    public string ReportName { get; private set; } = string.Empty;
    /// <summary>
    /// Format。
    /// </summary>
    public string Format { get; private set; } = string.Empty;
    /// <summary>
    /// File Name。
    /// </summary>
    public string FileName { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = ReportingStatus.Pending;
    /// <summary>
    /// Requested By。
    /// </summary>
    public string RequestedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Complete。
    /// </summary>
    public void Complete()
    {
        Status = ReportingStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
