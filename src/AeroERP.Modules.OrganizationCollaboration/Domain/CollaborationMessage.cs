using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.OrganizationCollaboration.Domain;

/// <summary>
/// 协同消息记录，第一阶段仅承载文本消息。
/// </summary>
public sealed class CollaborationMessage : Entity, IAggregateRoot
{
    private CollaborationMessage()
    {
    }

    public CollaborationMessage(Guid conversationId, Guid senderUserId, string senderUserName, string senderDisplayName, string content)
    {
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        SenderUserName = senderUserName;
        SenderDisplayName = senderDisplayName;
        Content = content;
    }

    /// <summary>
    /// 所属会话标识。
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// 发送人用户标识。
    /// </summary>
    public Guid SenderUserId { get; private set; }

    /// <summary>
    /// 发送人登录账号。
    /// </summary>
    public string SenderUserName { get; private set; } = string.Empty;

    /// <summary>
    /// 发送人显示名称。
    /// </summary>
    public string SenderDisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// 文本消息内容。
    /// </summary>
    public string Content { get; private set; } = string.Empty;
}
