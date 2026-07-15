namespace AeroERP.Modules.OrganizationCollaboration.Contracts;

/// <summary>
/// 协同会话参与者 DTO。
/// </summary>
public sealed record CollaborationParticipantDto(
    Guid UserId,
    string UserName,
    string DisplayName);

/// <summary>
/// 协同会话 DTO。
/// </summary>
public sealed record CollaborationConversationDto(
    Guid Id,
    string ConversationKey,
    string ScopeType,
    string Title,
    IReadOnlyList<CollaborationParticipantDto> Participants,
    string LastMessagePreview,
    int UnreadCount,
    DateTimeOffset? LastReadAtUtc,
    DateTimeOffset? LastMessageAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// 协同消息附件 DTO。
/// </summary>
public sealed record CollaborationAttachmentDto(
    Guid Id,
    Guid MessageId,
    string FileName,
    string ContentType,
    long SizeBytes,
    bool IsImage,
    string DownloadUrl,
    string PreviewUrl,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// 协同消息 DTO。
/// </summary>
public sealed record CollaborationMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderUserId,
    string SenderUserName,
    string SenderDisplayName,
    string Content,
    IReadOnlyList<CollaborationAttachmentDto> Attachments,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// 协同附件下载 DTO。
/// </summary>
public sealed record CollaborationAttachmentDownloadDto(
    string FileName,
    string ContentType,
    byte[] Content);

/// <summary>
/// 确保直接会话存在的请求。
/// </summary>
public sealed record EnsureDirectConversationRequest(Guid TargetUserId);

/// <summary>
/// 创建协同消息附件请求。
/// </summary>
public sealed record CreateCollaborationAttachmentRequest(
    string FileName,
    string ContentType,
    string ContentBase64);

/// <summary>
/// 发送协同消息请求。
/// </summary>
public sealed record SendCollaborationMessageRequest(
    string Content,
    IReadOnlyList<CreateCollaborationAttachmentRequest>? Attachments);

/// <summary>
/// 标记会话已读请求。
/// </summary>
public sealed record MarkCollaborationConversationReadRequest(Guid? LastReadMessageId);

/// <summary>
/// 协同实时事件 DTO。
/// </summary>
public sealed record CollaborationEventDto(
    string EventKey,
    DateTimeOffset ServerTimeUtc,
    long Cursor);
