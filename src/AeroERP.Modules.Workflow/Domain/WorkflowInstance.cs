using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Workflow.Domain;

/// <summary>
/// Workflow Instance 业务对象。
/// </summary>
public sealed class WorkflowInstance : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Workflow Instance实例。
    /// </summary>
    private WorkflowInstance()
    {
    }

    /// <summary>
    /// 初始化Workflow Instance实例。
    /// </summary>
    /// <param name="definitionId">definition Id 参数。</param>
    /// <param name="definitionKey">definition Key 参数。</param>
    /// <param name="definitionName">definition Name 参数。</param>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="documentId">业务单据标识。</param>
    /// <param name="documentNo">业务单据编号。</param>
    /// <param name="title">标题。</param>
    /// <param name="submittedBy">submitted By 参数。</param>
    public WorkflowInstance(
        Guid definitionId,
        string definitionKey,
        string definitionName,
        string documentType,
        Guid documentId,
        string documentNo,
        string title,
        string submittedBy)
    {
        DefinitionId = definitionId;
        DefinitionKey = definitionKey;
        DefinitionName = definitionName;
        DocumentType = documentType;
        DocumentId = documentId;
        DocumentNo = documentNo;
        Title = title;
        SubmittedBy = submittedBy;
    }

    /// <summary>
    /// Definition Id。
    /// </summary>
    public Guid DefinitionId { get; private set; }
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
    public string Status { get; private set; } = WorkflowStatus.Pending;
    /// <summary>
    /// Submitted By。
    /// </summary>
    public string SubmittedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="status">业务状态。</param>
    public void Complete(string status)
    {
        Status = status;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
