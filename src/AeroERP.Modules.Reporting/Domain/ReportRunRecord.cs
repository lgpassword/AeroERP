using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Reporting.Domain;

/// <summary>
/// Report Run Record 业务对象。
/// </summary>
public sealed class ReportRunRecord : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Report Run Record实例。
    /// </summary>
    private ReportRunRecord()
    {
    }

    /// <summary>
    /// 初始化Report Run Record实例。
    /// </summary>
    /// <param name="runNo">run No 参数。</param>
    /// <param name="reportDefinitionId">report Definition Id 参数。</param>
    /// <param name="reportKey">report Key 参数。</param>
    /// <param name="reportName">report Name 参数。</param>
    /// <param name="parametersJson">parameters Json 参数。</param>
    /// <param name="runBy">run By 参数。</param>
    public ReportRunRecord(string runNo, Guid reportDefinitionId, string reportKey, string reportName, string parametersJson, string runBy)
    {
        RunNo = runNo;
        ReportDefinitionId = reportDefinitionId;
        ReportKey = reportKey;
        ReportName = reportName;
        ParametersJson = parametersJson;
        RunBy = runBy;
    }

    /// <summary>
    /// Run No。
    /// </summary>
    public string RunNo { get; private set; } = string.Empty;
    /// <summary>
    /// Report Definition Id。
    /// </summary>
    public Guid ReportDefinitionId { get; private set; }
    /// <summary>
    /// Report Key。
    /// </summary>
    public string ReportKey { get; private set; } = string.Empty;
    /// <summary>
    /// Report Name。
    /// </summary>
    public string ReportName { get; private set; } = string.Empty;
    /// <summary>
    /// Parameters Json。
    /// </summary>
    public string ParametersJson { get; private set; } = "{}";
    /// <summary>
    /// Result Summary Json。
    /// </summary>
    public string ResultSummaryJson { get; private set; } = "{}";
    /// <summary>
    /// Row Count。
    /// </summary>
    public int RowCount { get; private set; }
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = ReportingStatus.Pending;
    /// <summary>
    /// Run By。
    /// </summary>
    public string RunBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="resultSummaryJson">result Summary Json 参数。</param>
    /// <param name="rowCount">row Count 参数。</param>
    public void Complete(string resultSummaryJson, int rowCount)
    {
        ResultSummaryJson = resultSummaryJson;
        RowCount = rowCount;
        Status = ReportingStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Fail。
    /// </summary>
    /// <param name="error">错误信息。</param>
    public void Fail(string error)
    {
        ResultSummaryJson = error;
        RowCount = 0;
        Status = ReportingStatus.Failed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
