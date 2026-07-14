using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Workflow.Domain;

/// <summary>
/// Notification 业务对象。
/// </summary>
public sealed class Notification : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Notification实例。
    /// </summary>
    private Notification()
    {
    }

    /// <summary>
    /// 初始化Notification实例。
    /// </summary>
    /// <param name="title">标题。</param>
    /// <param name="message">执行消息。</param>
    /// <param name="category">业务分类。</param>
    /// <param name="relatedDocumentType">related Document Type 参数。</param>
    /// <param name="relatedDocumentId">related Document Id 参数。</param>
    /// <param name="relatedDocumentNo">related Document No 参数。</param>
    /// <param name="recipientPermission">recipient Permission 参数。</param>
    public Notification(
        string title,
        string message,
        string category,
        string relatedDocumentType,
        Guid relatedDocumentId,
        string relatedDocumentNo,
        string recipientPermission)
    {
        Title = title;
        Message = message;
        Category = category;
        RelatedDocumentType = relatedDocumentType;
        RelatedDocumentId = relatedDocumentId;
        RelatedDocumentNo = relatedDocumentNo;
        RecipientPermission = recipientPermission;
    }

    /// <summary>
    /// Title。
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 执行消息。
    /// </summary>
    public string Message { get; private set; } = string.Empty;
    /// <summary>
    /// Category。
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    /// <summary>
    /// Related Document Type。
    /// </summary>
    public string RelatedDocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// Related Document Id。
    /// </summary>
    public Guid RelatedDocumentId { get; private set; }
    /// <summary>
    /// Related Document No。
    /// </summary>
    public string RelatedDocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// Recipient Permission。
    /// </summary>
    public string RecipientPermission { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = NotificationStatus.Unread;
    /// <summary>
    /// Read At Utc。
    /// </summary>
    public DateTimeOffset? ReadAtUtc { get; private set; }

    /// <summary>
    /// Mark Read。
    /// </summary>
    public void MarkRead()
    {
        Status = NotificationStatus.Read;
        ReadAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Mark Unread。
    /// </summary>
    public void MarkUnread()
    {
        Status = NotificationStatus.Unread;
        ReadAtUtc = null;
        Touch();
    }
}
