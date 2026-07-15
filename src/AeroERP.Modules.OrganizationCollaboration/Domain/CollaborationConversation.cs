using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.OrganizationCollaboration.Domain;

/// <summary>
/// 组织协同会话，承载个人、部门或组织范围内的消息沟通。
/// </summary>
public sealed class CollaborationConversation : Entity, IAggregateRoot
{
    private readonly List<CollaborationParticipant> _participants = [];

    private CollaborationConversation()
    {
    }

    public CollaborationConversation(string conversationKey, string scopeType, string title)
    {
        ConversationKey = conversationKey;
        ScopeType = scopeType;
        Title = title;
    }

    /// <summary>
    /// 会话业务键，直接会话使用双方用户标识生成稳定键。
    /// </summary>
    public string ConversationKey { get; private set; } = string.Empty;

    /// <summary>
    /// 会话范围，当前第一阶段支持 Direct。
    /// </summary>
    public string ScopeType { get; private set; } = string.Empty;

    /// <summary>
    /// 会话标题。
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// 会话参与者。
    /// </summary>
    public IReadOnlyCollection<CollaborationParticipant> Participants => _participants;

    /// <summary>
    /// 添加会话参与者。
    /// </summary>
    public void AddParticipant(Guid userId, string userName, string displayName)
    {
        if (_participants.Any(x => x.UserId == userId))
        {
            return;
        }

        _participants.Add(new CollaborationParticipant(Id, userId, userName, displayName));
        Touch();
    }

    /// <summary>
    /// 刷新会话更新时间。
    /// </summary>
    public void MarkMessageAppended()
    {
        Touch();
    }
}
