using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.OrganizationCollaboration.Domain;

/// <summary>
/// 协同会话参与者。
/// </summary>
public sealed class CollaborationParticipant : Entity
{
    private CollaborationParticipant()
    {
    }

    public CollaborationParticipant(Guid conversationId, Guid userId, string userName, string displayName)
    {
        ConversationId = conversationId;
        UserId = userId;
        UserName = userName;
        DisplayName = displayName;
    }

    /// <summary>
    /// 所属会话标识。
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// 平台用户标识。
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string UserName { get; private set; } = string.Empty;

    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
}
