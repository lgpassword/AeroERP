namespace AeroERP.Modules.DocumentExchange.Contracts;

/// <summary>
/// Document Exchange Metric 数据传输对象。
/// </summary>
/// <param name="Key">业务键。</param>
/// <param name="Label">界面显示标签。</param>
/// <param name="Value">数值或配置值。</param>
/// <param name="Unit">计量单位。</param>
public sealed record DocumentExchangeMetricDto(string Key, string Label, decimal Value, string Unit);

/// <summary>
/// Import Template 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="TargetModule">Target Module 参数。</param>
/// <param name="FileType">File Type 参数。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="UpdatedBy">最后更新人。</param>
/// <param name="UpdatedAtUtc">最后更新时间，使用 UTC。</param>
public sealed record ImportTemplateDto(Guid Id, string TemplateKey, string DisplayName, string TargetModule, string FileType, bool IsEnabled, string UpdatedBy, DateTimeOffset UpdatedAtUtc);
/// <summary>
/// Import Field Mapping 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="SourceField">Source Field 参数。</param>
/// <param name="TargetField">Target Field 参数。</param>
/// <param name="IsRequired">Is Required 参数。</param>
/// <param name="TransformRule">Transform Rule 参数。</param>
/// <param name="UpdatedBy">最后更新人。</param>
/// <param name="UpdatedAtUtc">最后更新时间，使用 UTC。</param>
public sealed record ImportFieldMappingDto(Guid Id, string TemplateKey, string SourceField, string TargetField, bool IsRequired, string TransformRule, string UpdatedBy, DateTimeOffset UpdatedAtUtc);
/// <summary>
/// Import Batch 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="BatchNo">Batch No 参数。</param>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="FileName">File Name 参数。</param>
/// <param name="Status">业务状态。</param>
/// <param name="RowCount">Row Count 参数。</param>
/// <param name="ErrorCount">Error Count 参数。</param>
/// <param name="ErrorMessage">Error Message 参数。</param>
/// <param name="CreatedBy">创建人。</param>
/// <param name="CompletedBy">Completed By 参数。</param>
/// <param name="CompletedAtUtc">Completed At Utc 参数。</param>
/// <param name="UpdatedAtUtc">最后更新时间，使用 UTC。</param>
public sealed record ImportBatchDto(Guid Id, string BatchNo, string TemplateKey, string FileName, string Status, int RowCount, int ErrorCount, string ErrorMessage, string CreatedBy, string CompletedBy, DateTimeOffset? CompletedAtUtc, DateTimeOffset UpdatedAtUtc);
/// <summary>
/// Export File Task 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="ExportNo">Export No 参数。</param>
/// <param name="SourceModule">Source Module 参数。</param>
/// <param name="FileName">File Name 参数。</param>
/// <param name="Format">文件或报表格式。</param>
/// <param name="Status">业务状态。</param>
/// <param name="RequestedBy">Requested By 参数。</param>
/// <param name="CompletedBy">Completed By 参数。</param>
/// <param name="CompletedAtUtc">Completed At Utc 参数。</param>
/// <param name="UpdatedAtUtc">最后更新时间，使用 UTC。</param>
public sealed record ExportFileTaskDto(Guid Id, string ExportNo, string SourceModule, string FileName, string Format, string Status, string RequestedBy, string CompletedBy, DateTimeOffset? CompletedAtUtc, DateTimeOffset UpdatedAtUtc);
/// <summary>
/// Print Template 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="TargetModule">Target Module 参数。</param>
/// <param name="ContentType">Content Type 参数。</param>
/// <param name="TemplateBody">Template Body 参数。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="UpdatedBy">最后更新人。</param>
/// <param name="UpdatedAtUtc">最后更新时间，使用 UTC。</param>
public sealed record PrintTemplateDto(Guid Id, string TemplateKey, string DisplayName, string TargetModule, string ContentType, string TemplateBody, bool IsEnabled, string UpdatedBy, DateTimeOffset UpdatedAtUtc);
/// <summary>
/// Print Job 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="JobNo">Job No 参数。</param>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="DocumentNo">业务单据编号。</param>
/// <param name="Status">业务状态。</param>
/// <param name="RequestedBy">Requested By 参数。</param>
/// <param name="CompletedBy">Completed By 参数。</param>
/// <param name="CompletedAtUtc">Completed At Utc 参数。</param>
/// <param name="UpdatedAtUtc">最后更新时间，使用 UTC。</param>
public sealed record PrintJobDto(Guid Id, string JobNo, string TemplateKey, string DocumentNo, string Status, string RequestedBy, string CompletedBy, DateTimeOffset? CompletedAtUtc, DateTimeOffset UpdatedAtUtc);
/// <summary>
/// File Audit Record 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="AuditNo">Audit No 参数。</param>
/// <param name="Category">业务分类。</param>
/// <param name="Action">业务动作。</param>
/// <param name="TargetNo">Target No 参数。</param>
/// <param name="Result">执行结果。</param>
/// <param name="Message">执行消息。</param>
/// <param name="Actor">操作人。</param>
/// <param name="CreatedAtUtc">创建时间，使用 UTC。</param>
public sealed record FileAuditRecordDto(Guid Id, string AuditNo, string Category, string Action, string TargetNo, string Result, string Message, string Actor, DateTimeOffset CreatedAtUtc);

/// <summary>
/// Document Exchange Overview 数据传输对象。
/// </summary>
public sealed record DocumentExchangeOverviewDto(
    IReadOnlyList<ImportTemplateDto> ImportTemplates,
    IReadOnlyList<ImportFieldMappingDto> FieldMappings,
    IReadOnlyList<ImportBatchDto> ImportBatches,
    IReadOnlyList<ExportFileTaskDto> ExportTasks,
    IReadOnlyList<PrintTemplateDto> PrintTemplates,
    IReadOnlyList<PrintJobDto> PrintJobs,
    IReadOnlyList<FileAuditRecordDto> AuditRecords,
    IReadOnlyList<DocumentExchangeMetricDto> Metrics);

/// <summary>
/// Upsert Import Template 请求参数。
/// </summary>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="TargetModule">Target Module 参数。</param>
/// <param name="FileType">File Type 参数。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record UpsertImportTemplateRequest(string TemplateKey, string DisplayName, string TargetModule, string FileType, bool IsEnabled);
/// <summary>
/// Upsert Import Field Mapping 请求参数。
/// </summary>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="SourceField">Source Field 参数。</param>
/// <param name="TargetField">Target Field 参数。</param>
/// <param name="IsRequired">Is Required 参数。</param>
/// <param name="TransformRule">Transform Rule 参数。</param>
public sealed record UpsertImportFieldMappingRequest(string TemplateKey, string SourceField, string TargetField, bool IsRequired, string TransformRule);
/// <summary>
/// Create Import Batch 请求参数。
/// </summary>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="FileName">File Name 参数。</param>
public sealed record CreateImportBatchRequest(string TemplateKey, string FileName);
/// <summary>
/// Complete Import Batch 请求参数。
/// </summary>
/// <param name="RowCount">Row Count 参数。</param>
/// <param name="ErrorCount">Error Count 参数。</param>
public sealed record CompleteImportBatchRequest(int RowCount, int ErrorCount);
/// <summary>
/// Fail File Task 请求参数。
/// </summary>
/// <param name="Error">错误信息。</param>
public sealed record FailFileTaskRequest(string Error);
/// <summary>
/// Create Export File Task 请求参数。
/// </summary>
/// <param name="SourceModule">Source Module 参数。</param>
/// <param name="FileName">File Name 参数。</param>
/// <param name="Format">文件或报表格式。</param>
public sealed record CreateExportFileTaskRequest(string SourceModule, string FileName, string Format);
/// <summary>
/// Upsert Print Template 请求参数。
/// </summary>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="TargetModule">Target Module 参数。</param>
/// <param name="ContentType">Content Type 参数。</param>
/// <param name="TemplateBody">Template Body 参数。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record UpsertPrintTemplateRequest(string TemplateKey, string DisplayName, string TargetModule, string ContentType, string TemplateBody, bool IsEnabled);
/// <summary>
/// Create Print Job 请求参数。
/// </summary>
/// <param name="TemplateKey">Template Key 参数。</param>
/// <param name="DocumentNo">业务单据编号。</param>
public sealed record CreatePrintJobRequest(string TemplateKey, string DocumentNo);
