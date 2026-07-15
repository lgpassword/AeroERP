using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.OrganizationCollaboration.Contracts;
using AeroERP.Modules.OrganizationCollaboration.Domain;
using AeroERP.Modules.OrganizationCollaboration.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// 组织协同服务实现，负责会话创建、参与者校验和文本消息持久化。
/// </summary>
public sealed class OrganizationCollaborationService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IOrganizationCollaborationService
{
    private const int MaxAttachmentCount = 5;
    private const int MaxAttachmentBytes = 2 * 1024 * 1024;
    private const int MaxTotalAttachmentBytes = 8 * 1024 * 1024;

    public async Task<IReadOnlyList<CollaborationConversationDto>> ListConversationsAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            return [];
        }

        var conversations = await dbContext.CollaborationConversations
            .AsNoTracking()
            .Include(x => x.Participants)
            .Where(x => x.Participants.Any(participant => participant.UserId == userId.Value))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        var conversationIds = conversations.Select(x => x.Id).ToArray();
        var messages = await LoadMessagesForConversationsAsync(conversationIds, cancellationToken);
        var readStates = await LoadReadStatesAsync(conversationIds, userId.Value, cancellationToken);
        var lastMessages = messages
            .GroupBy(x => x.ConversationId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(x => x.CreatedAtUtc).First());
        var unreadCounts = messages
            .Where(message => message.SenderUserId != userId.Value)
            .Where(message =>
            {
                var readState = readStates.GetValueOrDefault(message.ConversationId);
                return readState?.LastReadAtUtc is null || message.CreatedAtUtc > readState.LastReadAtUtc;
            })
            .GroupBy(x => x.ConversationId)
            .ToDictionary(group => group.Key, group => group.Count());

        return conversations
            .Select(conversation => MapConversation(
                conversation,
                lastMessages.GetValueOrDefault(conversation.Id),
                readStates.GetValueOrDefault(conversation.Id),
                unreadCounts.GetValueOrDefault(conversation.Id)))
            .ToList();
    }

    public async Task<OperationResult<CollaborationConversationDto>> EnsureDirectConversationAsync(EnsureDirectConversationRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.UserId;
        if (currentUserId is null)
        {
            return OperationResult<CollaborationConversationDto>.Failure("当前用户未登录。");
        }

        if (request.TargetUserId == currentUserId.Value)
        {
            return OperationResult<CollaborationConversationDto>.Failure("不能与自己创建直接会话。");
        }

        var users = await dbContext.Users
            .Where(x => x.Id == currentUserId.Value || x.Id == request.TargetUserId)
            .ToListAsync(cancellationToken);
        var self = users.FirstOrDefault(x => x.Id == currentUserId.Value);
        var target = users.FirstOrDefault(x => x.Id == request.TargetUserId);
        if (self is null || target is null || !target.IsEnabled)
        {
            return OperationResult<CollaborationConversationDto>.Failure("目标联系人不存在或已停用。");
        }

        var conversationKey = BuildDirectKey(currentUserId.Value, request.TargetUserId);
        var conversation = await dbContext.CollaborationConversations
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.ConversationKey == conversationKey, cancellationToken);

        if (conversation is null)
        {
            conversation = new CollaborationConversation(conversationKey, "Direct", "直接会话");
            conversation.AddParticipant(self.Id, self.UserName, self.DisplayName);
            conversation.AddParticipant(target.Id, target.UserName, target.DisplayName);
            dbContext.CollaborationConversations.Add(conversation);
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditWriter.WriteAsync("OrganizationCollaboration", "DirectConversationCreated", currentUser.GetActor(), conversationKey, cancellationToken);
        }

        var lastMessage = await dbContext.CollaborationMessages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversation.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var readState = await dbContext.CollaborationReadStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConversationId == conversation.Id && x.UserId == currentUserId.Value, cancellationToken);

        return OperationResult<CollaborationConversationDto>.Success(MapConversation(conversation, lastMessage, readState, 0));
    }

    public async Task<OperationResult<IReadOnlyList<CollaborationMessageDto>>> ListMessagesAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var hasAccess = await CurrentUserCanAccessConversationAsync(conversationId, cancellationToken);
        if (!hasAccess)
        {
            return OperationResult<IReadOnlyList<CollaborationMessageDto>>.Failure("会话不存在或当前用户无权查看。");
        }

        var messages = await dbContext.CollaborationMessages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(120)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var attachments = await LoadAttachmentsAsync(messages.Select(x => x.Id).ToArray(), cancellationToken);
        return OperationResult<IReadOnlyList<CollaborationMessageDto>>.Success(messages.Select(message => MapMessage(message, attachments.GetValueOrDefault(message.Id) ?? [])).ToList());
    }

    public async Task<OperationResult<CollaborationMessageDto>> SendMessageAsync(Guid conversationId, SendCollaborationMessageRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.UserId;
        if (currentUserId is null)
        {
            return OperationResult<CollaborationMessageDto>.Failure("当前用户未登录。");
        }

        var conversation = await dbContext.CollaborationConversations
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);
        if (conversation is null || conversation.Participants.All(x => x.UserId != currentUserId.Value))
        {
            return OperationResult<CollaborationMessageDto>.Failure("会话不存在或当前用户无权发送。");
        }

        var content = NormalizeMessage(request.Content);
        var attachmentsResult = PrepareAttachments(request.Attachments);
        if (!attachmentsResult.IsSuccess)
        {
            return OperationResult<CollaborationMessageDto>.Failure(attachmentsResult.Error ?? "附件无效。");
        }

        var preparedAttachments = attachmentsResult.Value ?? [];
        if (string.IsNullOrWhiteSpace(content) && preparedAttachments.Count == 0)
        {
            return OperationResult<CollaborationMessageDto>.Failure("消息内容或附件不能同时为空。");
        }

        if (content.Length > 2000)
        {
            return OperationResult<CollaborationMessageDto>.Failure("消息内容不能超过 2000 个字符。");
        }

        var message = new CollaborationMessage(conversation.Id, currentUserId.Value, currentUser.UserName, currentUser.DisplayName, content);
        dbContext.CollaborationMessages.Add(message);
        var attachments = preparedAttachments
            .Select(attachment => new CollaborationAttachment(
                conversation.Id,
                message.Id,
                attachment.FileName,
                attachment.ContentType,
                attachment.Content.LongLength,
                attachment.Content,
                currentUserId.Value,
                currentUser.DisplayName))
            .ToList();
        dbContext.CollaborationAttachments.AddRange(attachments);
        conversation.MarkMessageAppended();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(
            "OrganizationCollaboration",
            attachments.Count > 0 ? "MessageWithAttachmentSent" : "MessageSent",
            currentUser.GetActor(),
            conversation.ConversationKey,
            cancellationToken);
        return OperationResult<CollaborationMessageDto>.Success(MapMessage(message, attachments));
    }

    public async Task<OperationResult<CollaborationConversationDto>> MarkConversationReadAsync(
        Guid conversationId,
        MarkCollaborationConversationReadRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            return OperationResult<CollaborationConversationDto>.Failure("当前用户未登录。");
        }

        var conversation = await dbContext.CollaborationConversations
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);
        if (conversation is null || conversation.Participants.All(x => x.UserId != userId.Value))
        {
            return OperationResult<CollaborationConversationDto>.Failure("会话不存在或当前用户无权标记已读。");
        }

        if (request.LastReadMessageId is not null)
        {
            var messageExists = await dbContext.CollaborationMessages
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.LastReadMessageId && x.ConversationId == conversationId, cancellationToken);
            if (!messageExists)
            {
                return OperationResult<CollaborationConversationDto>.Failure("已读消息不属于当前会话。");
            }
        }

        var readState = await dbContext.CollaborationReadStates
            .FirstOrDefaultAsync(x => x.ConversationId == conversationId && x.UserId == userId.Value, cancellationToken);
        if (readState is null)
        {
            readState = new CollaborationReadState(conversationId, userId.Value);
            dbContext.CollaborationReadStates.Add(readState);
        }

        readState.MarkRead(request.LastReadMessageId);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lastMessage = await dbContext.CollaborationMessages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        return OperationResult<CollaborationConversationDto>.Success(MapConversation(conversation, lastMessage, readState, 0));
    }

    public async Task<OperationResult<CollaborationAttachmentDownloadDto>> DownloadAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.CollaborationAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attachmentId, cancellationToken);
        if (attachment is null)
        {
            return OperationResult<CollaborationAttachmentDownloadDto>.Failure("附件不存在。");
        }

        var hasAccess = await CurrentUserCanAccessConversationAsync(attachment.ConversationId, cancellationToken);
        if (!hasAccess)
        {
            return OperationResult<CollaborationAttachmentDownloadDto>.Failure("当前用户无权下载该附件。");
        }

        return OperationResult<CollaborationAttachmentDownloadDto>.Success(new CollaborationAttachmentDownloadDto(
            attachment.FileName,
            attachment.ContentType,
            attachment.Content));
    }

    public async Task<CollaborationEventDto> GetEventAsync(long previousCursor, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            return new CollaborationEventDto("heartbeat", DateTimeOffset.UtcNow, previousCursor);
        }

        var conversationIds = await dbContext.CollaborationParticipants
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value)
            .Select(x => x.ConversationId)
            .ToListAsync(cancellationToken);
        if (conversationIds.Count == 0)
        {
            return new CollaborationEventDto("heartbeat", DateTimeOffset.UtcNow, 0);
        }

        var updatedAtValues = await dbContext.CollaborationConversations
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.Id))
            .Select(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
        updatedAtValues.AddRange(await dbContext.CollaborationMessages
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId))
            .Select(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken));
        updatedAtValues.AddRange(await dbContext.CollaborationReadStates
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId) && x.UserId == userId.Value)
            .Select(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken));

        var cursor = updatedAtValues.Count == 0 ? 0 : updatedAtValues.Max(x => x.UtcTicks);
        return new CollaborationEventDto(cursor > previousCursor ? "changed" : "heartbeat", DateTimeOffset.UtcNow, cursor);
    }

    private async Task<bool> CurrentUserCanAccessConversationAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            return false;
        }

        return await dbContext.CollaborationParticipants
            .AsNoTracking()
            .AnyAsync(x => x.ConversationId == conversationId && x.UserId == userId.Value, cancellationToken);
    }

    private async Task<IReadOnlyList<CollaborationMessage>> LoadMessagesForConversationsAsync(Guid[] conversationIds, CancellationToken cancellationToken)
    {
        if (conversationIds.Length == 0)
        {
            return [];
        }

        return await dbContext.CollaborationMessages
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyDictionary<Guid, CollaborationReadState>> LoadReadStatesAsync(Guid[] conversationIds, Guid userId, CancellationToken cancellationToken)
    {
        if (conversationIds.Length == 0)
        {
            return new Dictionary<Guid, CollaborationReadState>();
        }

        return await dbContext.CollaborationReadStates
            .AsNoTracking()
            .Where(x => x.UserId == userId && conversationIds.Contains(x.ConversationId))
            .ToDictionaryAsync(x => x.ConversationId, cancellationToken);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<CollaborationAttachment>>> LoadAttachmentsAsync(Guid[] messageIds, CancellationToken cancellationToken)
    {
        if (messageIds.Length == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<CollaborationAttachment>>();
        }

        var attachments = await dbContext.CollaborationAttachments
            .AsNoTracking()
            .Where(x => messageIds.Contains(x.MessageId))
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        return attachments
            .GroupBy(x => x.MessageId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<CollaborationAttachment>)group.ToList());
    }

    private static string BuildDirectKey(Guid left, Guid right)
    {
        var ordered = new[] { left, right }
            .OrderBy(x => x)
            .Select(x => x.ToString("N"))
            .ToArray();
        return $"direct:{ordered[0]}:{ordered[1]}";
    }

    private static string NormalizeMessage(string value) => value?.Trim() ?? string.Empty;

    private static OperationResult<IReadOnlyList<PreparedAttachment>> PrepareAttachments(IReadOnlyList<CreateCollaborationAttachmentRequest>? attachments)
    {
        if (attachments is null || attachments.Count == 0)
        {
            return OperationResult<IReadOnlyList<PreparedAttachment>>.Success([]);
        }

        if (attachments.Count > MaxAttachmentCount)
        {
            return OperationResult<IReadOnlyList<PreparedAttachment>>.Failure($"单条消息最多上传 {MaxAttachmentCount} 个附件。");
        }

        var totalBytes = 0;
        var prepared = new List<PreparedAttachment>();
        foreach (var attachment in attachments)
        {
            var fileName = NormalizeFileName(attachment.FileName);
            var contentType = NormalizeContentType(attachment.ContentType);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return OperationResult<IReadOnlyList<PreparedAttachment>>.Failure("附件文件名不能为空。");
            }

            byte[] content;
            try
            {
                content = Convert.FromBase64String(attachment.ContentBase64?.Trim() ?? string.Empty);
            }
            catch (FormatException)
            {
                return OperationResult<IReadOnlyList<PreparedAttachment>>.Failure($"{fileName} 的文件内容不是有效 Base64。");
            }

            if (content.Length == 0)
            {
                return OperationResult<IReadOnlyList<PreparedAttachment>>.Failure($"{fileName} 内容不能为空。");
            }

            if (content.Length > MaxAttachmentBytes)
            {
                return OperationResult<IReadOnlyList<PreparedAttachment>>.Failure($"{fileName} 超过 2 MB 限制。");
            }

            totalBytes += content.Length;
            if (totalBytes > MaxTotalAttachmentBytes)
            {
                return OperationResult<IReadOnlyList<PreparedAttachment>>.Failure("单条消息附件总大小不能超过 8 MB。");
            }

            prepared.Add(new PreparedAttachment(fileName, contentType, content));
        }

        return OperationResult<IReadOnlyList<PreparedAttachment>>.Success(prepared);
    }

    private static string NormalizeFileName(string value)
    {
        var fileName = Path.GetFileName(value?.Trim() ?? string.Empty);
        return fileName.Length > 192 ? fileName[..192] : fileName;
    }

    private static string NormalizeContentType(string value)
    {
        var contentType = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return "application/octet-stream";
        }

        return contentType.Length > 96 ? contentType[..96] : contentType;
    }

    private static CollaborationConversationDto MapConversation(
        CollaborationConversation conversation,
        CollaborationMessage? lastMessage,
        CollaborationReadState? readState,
        int unreadCount)
    {
        var preview = lastMessage?.Content ?? string.Empty;
        if (string.IsNullOrWhiteSpace(preview) && lastMessage is not null)
        {
            preview = "[附件消息]";
        }

        if (preview.Length > 80)
        {
            preview = $"{preview[..80]}...";
        }

        return new CollaborationConversationDto(
            conversation.Id,
            conversation.ConversationKey,
            conversation.ScopeType,
            conversation.Title,
            conversation.Participants
                .OrderBy(x => x.DisplayName)
                .Select(x => new CollaborationParticipantDto(x.UserId, x.UserName, x.DisplayName))
                .ToList(),
            preview,
            unreadCount,
            readState?.LastReadAtUtc,
            lastMessage?.CreatedAtUtc,
            conversation.UpdatedAtUtc);
    }

    private static CollaborationMessageDto MapMessage(CollaborationMessage message, IReadOnlyList<CollaborationAttachment> attachments) =>
        new(
            message.Id,
            message.ConversationId,
            message.SenderUserId,
            message.SenderUserName,
            message.SenderDisplayName,
            message.Content,
            attachments.Select(MapAttachment).ToList(),
            message.CreatedAtUtc);

    private static CollaborationAttachmentDto MapAttachment(CollaborationAttachment attachment)
    {
        var downloadUrl = $"/api/organization-collaboration/attachments/{attachment.Id}/download";
        var isImage = attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        return new CollaborationAttachmentDto(
            attachment.Id,
            attachment.MessageId,
            attachment.FileName,
            attachment.ContentType,
            attachment.SizeBytes,
            isImage,
            downloadUrl,
            isImage ? downloadUrl : string.Empty,
            attachment.CreatedAtUtc);
    }

    private sealed record PreparedAttachment(string FileName, string ContentType, byte[] Content);
}
