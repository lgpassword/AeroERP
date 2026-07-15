using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.OrganizationCollaboration.Domain;

/// <summary>
/// 协同消息附件，内容由后端持久化并通过权限校验接口下载。
/// </summary>
public sealed class CollaborationAttachment : Entity
{
    private CollaborationAttachment()
    {
    }

    public CollaborationAttachment(
        Guid conversationId,
        Guid messageId,
        string fileName,
        string contentType,
        long sizeBytes,
        byte[] content,
        Guid uploadedByUserId,
        string uploadedBy)
    {
        ConversationId = conversationId;
        MessageId = messageId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        Content = content;
        UploadedByUserId = uploadedByUserId;
        UploadedBy = uploadedBy;
    }

    /// <summary>
    /// 所属会话标识。
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// 所属消息标识。
    /// </summary>
    public Guid MessageId { get; private set; }

    /// <summary>
    /// 原始文件名。
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// 文件内容类型。
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// 文件大小。
    /// </summary>
    public long SizeBytes { get; private set; }

    /// <summary>
    /// 文件内容。
    /// </summary>
    public byte[] Content { get; private set; } = [];

    /// <summary>
    /// 上传用户标识。
    /// </summary>
    public Guid UploadedByUserId { get; private set; }

    /// <summary>
    /// 上传人显示名称。
    /// </summary>
    public string UploadedBy { get; private set; } = string.Empty;
}
