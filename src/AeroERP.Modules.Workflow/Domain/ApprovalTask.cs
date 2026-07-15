using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Workflow.Domain;

/// <summary>
/// Approval Task 业务对象。
/// </summary>
public sealed class ApprovalTask : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Approval Task实例。
    /// </summary>
    private ApprovalTask()
    {
    }

    /// <summary>
    /// 初始化Approval Task实例。
    /// </summary>
    /// <param name="workflowInstanceId">workflow Instance Id 参数。</param>
    /// <param name="definitionKey">definition Key 参数。</param>
    /// <param name="definitionName">definition Name 参数。</param>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="documentId">业务单据标识。</param>
    /// <param name="documentNo">业务单据编号。</param>
    /// <param name="title">标题。</param>
    /// <param name="submittedBy">submitted By 参数。</param>
    /// <param name="requiredPermission">required Permission 参数。</param>
    public ApprovalTask(
        Guid workflowInstanceId,
        string definitionKey,
        string definitionName,
        string documentType,
        Guid documentId,
        string documentNo,
        string title,
        string submittedBy,
        string requiredPermission)
    {
        WorkflowInstanceId = workflowInstanceId;
        DefinitionKey = definitionKey;
        DefinitionName = definitionName;
        DocumentType = documentType;
        DocumentId = documentId;
        DocumentNo = documentNo;
        Title = title;
        SubmittedBy = submittedBy;
        RequiredPermission = requiredPermission;
    }

    /// <summary>
    /// Workflow Instance Id。
    /// </summary>
    public Guid WorkflowInstanceId { get; private set; }
    /// <summary>
    /// Definition Key。
    /// </summary>
    public string DefinitionKey { get; private set; } = string.Empty;
    /// <summary>
    /// Definition Name。
    /// </summary>
    public string DefinitionName { get; private set; } = string.Empty;
    /// <summary>
    /// 业务单据类型。
    /// </summary>
    public string DocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// Document Id。
    /// </summary>
    public Guid DocumentId { get; private set; }
    /// <summary>
    /// 业务单据编号。
    /// </summary>
    public string DocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// Title。
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = ApprovalTaskStatus.Pending;
    /// <summary>
    /// Submitted By。
    /// </summary>
    public string SubmittedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Required Permission。
    /// </summary>
    public string RequiredPermission { get; private set; } = string.Empty;
    /// <summary>
    /// Decided By。
    /// </summary>
    public string? DecidedBy { get; private set; }
    /// <summary>
    /// Decision。
    /// </summary>
    public string? Decision { get; private set; }
    /// <summary>
    /// Comment。
    /// </summary>
    public string? Comment { get; private set; }
    /// <summary>
    /// Decided At Utc。
    /// </summary>
    public DateTimeOffset? DecidedAtUtc { get; private set; }

    /// <summary>
    /// Decide。
    /// </summary>
    /// <param name="decision">处理决策。</param>
    /// <param name="decidedBy">decided By 参数。</param>
    /// <param name="comment">处理意见。</param>
    public void Decide(string decision, string decidedBy, string comment)
    {
        if (!string.Equals(Status, ApprovalTaskStatus.Pending, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("该审批任务已经处理。");
        }

        Status = ApprovalTaskStatus.Completed;
        Decision = decision;
        DecidedBy = decidedBy;
        Comment = comment;
        DecidedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
