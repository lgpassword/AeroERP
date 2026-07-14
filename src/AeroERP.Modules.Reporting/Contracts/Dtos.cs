namespace AeroERP.Modules.Reporting.Contracts;

/// <summary>
/// Business Metric 数据传输对象。
/// </summary>
/// <param name="Key">业务键。</param>
/// <param name="Label">界面显示标签。</param>
/// <param name="Value">数值或配置值。</param>
/// <param name="Unit">计量单位。</param>
public sealed record BusinessMetricDto(string Key, string Label, decimal Value, string Unit);

/// <summary>
/// Report Definition 数据传输对象。
/// </summary>
public sealed record ReportDefinitionDto(
    Guid Id,
    string Key,
    string DisplayName,
    string Category,
    string QueryModel,
    string ParametersJson,
    bool IsEnabled,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Report Run Record 数据传输对象。
/// </summary>
public sealed record ReportRunRecordDto(
    Guid Id,
    string RunNo,
    Guid ReportDefinitionId,
    string ReportKey,
    string ReportName,
    string ParametersJson,
    string ResultSummaryJson,
    int RowCount,
    string Status,
    string RunBy,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Report Export Task 数据传输对象。
/// </summary>
public sealed record ReportExportTaskDto(
    Guid Id,
    string ExportNo,
    Guid ReportRunRecordId,
    string ReportName,
    string Format,
    string FileName,
    string Status,
    string RequestedBy,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Reporting Overview 数据传输对象。
/// </summary>
public sealed record ReportingOverviewDto(
    IReadOnlyList<ReportDefinitionDto> Definitions,
    IReadOnlyList<ReportRunRecordDto> Runs,
    IReadOnlyList<ReportExportTaskDto> ExportTasks,
    IReadOnlyList<BusinessMetricDto> LiveMetrics);

/// <summary>
/// Upsert Report Definition 请求参数。
/// </summary>
public sealed record UpsertReportDefinitionRequest(
    string Key,
    string DisplayName,
    string Category,
    string QueryModel,
    string ParametersJson,
    bool IsEnabled);

/// <summary>
/// Run Report 请求参数。
/// </summary>
/// <param name="ReportDefinitionId">Report Definition Id 参数。</param>
/// <param name="ParametersJson">Parameters Json 参数。</param>
public sealed record RunReportRequest(Guid ReportDefinitionId, string ParametersJson);

/// <summary>
/// Create Report Export Task 请求参数。
/// </summary>
/// <param name="ReportRunRecordId">Report Run Record Id 参数。</param>
/// <param name="Format">文件或报表格式。</param>
public sealed record CreateReportExportTaskRequest(Guid ReportRunRecordId, string Format);
