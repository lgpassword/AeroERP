using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.OrganizationCollaboration.Contracts;

namespace AeroERP.Modules.OrganizationCollaboration.Services;

/// <summary>
/// 组织协同服务契约。
/// </summary>
public interface IOrganizationCollaborationService
{
    /// <summary>
    /// 查询当前用户可见会话。
    /// </summary>
    Task<IReadOnlyList<CollaborationConversationDto>> ListConversationsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 确保当前用户与目标用户之间的直接会话存在。
    /// </summary>
    Task<OperationResult<CollaborationConversationDto>> EnsureDirectConversationAsync(EnsureDirectConversationRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// 查询会话消息。
    /// </summary>
    Task<OperationResult<IReadOnlyList<CollaborationMessageDto>>> ListMessagesAsync(Guid conversationId, CancellationToken cancellationToken);

    /// <summary>
    /// 发送文本消息。
    /// </summary>
    Task<OperationResult<CollaborationMessageDto>> SendMessageAsync(Guid conversationId, SendCollaborationMessageRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// 标记会话为已读。
    /// </summary>
    Task<OperationResult<CollaborationConversationDto>> MarkConversationReadAsync(Guid conversationId, MarkCollaborationConversationReadRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// 下载会话附件。
    /// </summary>
    Task<OperationResult<CollaborationAttachmentDownloadDto>> DownloadAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken);

    /// <summary>
    /// 获取当前用户协同事件游标。
    /// </summary>
    Task<CollaborationEventDto> GetEventAsync(long previousCursor, CancellationToken cancellationToken);
}
