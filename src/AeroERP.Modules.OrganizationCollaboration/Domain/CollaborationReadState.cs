using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.OrganizationCollaboration.Domain;

/// <summary>
/// 当前用户对协同会话的读取状态。
/// </summary>
public sealed class CollaborationReadState : Entity
{
    private CollaborationReadState()
    {
    }

    public CollaborationReadState(Guid conversationId, Guid userId)
    {
        ConversationId = conversationId;
        UserId = userId;
    }

    /// <summary>
    /// 所属会话标识。
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// 用户标识。
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// 最后已读消息标识。
    /// </summary>
    public Guid? LastReadMessageId { get; private set; }

    /// <summary>
    /// 最后已读时间。
    /// </summary>
    public DateTimeOffset? LastReadAtUtc { get; private set; }

    /// <summary>
    /// 标记已读。
    /// </summary>
    public void MarkRead(Guid? lastReadMessageId)
    {
        LastReadMessageId = lastReadMessageId;
        LastReadAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
