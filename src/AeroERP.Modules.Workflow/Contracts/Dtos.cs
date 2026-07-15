namespace AeroERP.Modules.Workflow.Contracts;

/// <summary>
/// Workflow Definition 数据传输对象。
/// </summary>
public sealed record WorkflowDefinitionDto(
    Guid Id,
    string Key,
    string DisplayName,
    string ModuleKey,
    string DocumentType,
    string RequiredPermission,
    bool IsEnabled,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Workflow Instance 数据传输对象。
/// </summary>
public sealed record WorkflowInstanceDto(
    Guid Id,
    Guid DefinitionId,
    string DefinitionKey,
    string DefinitionName,
    string DocumentType,
    Guid DocumentId,
    string DocumentNo,
    string Title,
    string Status,
    string SubmittedBy,
    DateTimeOffset SubmittedAtUtc,
    DateTimeOffset? CompletedAtUtc);

/// <summary>
/// Approval Task 数据传输对象。
/// </summary>
public sealed record ApprovalTaskDto(
    Guid Id,
    Guid WorkflowInstanceId,
    string DefinitionKey,
    string DefinitionName,
    string DocumentType,
    Guid DocumentId,
    string DocumentNo,
    string Title,
    string Status,
    string SubmittedBy,
    string RequiredPermission,
    string? DecidedBy,
    string? Decision,
    string? Comment,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DecidedAtUtc);

/// <summary>
/// Notification 数据传输对象。
/// </summary>
public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Category,
    string RelatedDocumentType,
    Guid RelatedDocumentId,
    string RelatedDocumentNo,
    string RecipientPermission,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc);

/// <summary>
/// Decide Approval Task 请求参数。
/// </summary>
/// <param name="Decision">处理决策。</param>
/// <param name="Comment">处理意见。</param>
public sealed record DecideApprovalTaskRequest(string Decision, string Comment);

/// <summary>
/// Mark Notification Read 请求参数。
/// </summary>
/// <param name="IsRead">Is Read 参数。</param>
public sealed record MarkNotificationReadRequest(bool IsRead);
