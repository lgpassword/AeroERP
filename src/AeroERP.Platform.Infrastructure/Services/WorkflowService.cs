using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Procurement.Domain;
using AeroERP.Modules.Workflow.Contracts;
using AeroERP.Modules.Workflow.Domain;
using AeroERP.Modules.Workflow.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Workflow.Services;

/// <summary>
/// Workflow Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class WorkflowService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IWorkflowService
{
    /// <summary>
    /// 查询Definitions。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<WorkflowDefinitionDto>> ListDefinitionsAsync(CancellationToken cancellationToken)
    {
        var definitions = await dbContext.WorkflowDefinitions.ToListAsync(cancellationToken);

        return definitions
            .OrderBy(x => x.ModuleKey)
            .ThenBy(x => x.DisplayName)
            .Select(MapDefinition)
            .ToList();
    }

    /// <summary>
    /// 查询Instances。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<WorkflowInstanceDto>> ListInstancesAsync(CancellationToken cancellationToken)
    {
        var instances = await dbContext.WorkflowInstances.ToListAsync(cancellationToken);

        return instances
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapInstance)
            .ToList();
    }

    /// <summary>
    /// 查询Tasks。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ApprovalTaskDto>> ListTasksAsync(CancellationToken cancellationToken)
    {
        var permissions = currentUser.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tasks = await dbContext.ApprovalTasks.ToListAsync(cancellationToken);

        return tasks
            .Where(x => permissions.Contains(x.RequiredPermission))
            .OrderBy(x => x.Status == ApprovalTaskStatus.Pending ? 0 : 1)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(MapTask)
            .ToList();
    }

    /// <summary>
    /// 查询Notifications。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<NotificationDto>> ListNotificationsAsync(CancellationToken cancellationToken)
    {
        var permissions = currentUser.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var notifications = await dbContext.Notifications.ToListAsync(cancellationToken);

        return notifications
            .Where(x => string.IsNullOrWhiteSpace(x.RecipientPermission) || permissions.Contains(x.RecipientPermission))
            .OrderBy(x => x.Status == NotificationStatus.Unread ? 0 : 1)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(MapNotification)
            .ToList();
    }

    /// <summary>
    /// Decide Task Async。
    /// </summary>
    /// <param name="taskId">task Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ApprovalTaskDto>> DecideTaskAsync(
        Guid taskId,
        DecideApprovalTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.ApprovalTasks.FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
        if (task is null)
        {
            return OperationResult<ApprovalTaskDto>.Failure("未找到审批任务。");
        }

        if (!currentUser.Permissions.Contains(task.RequiredPermission, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult<ApprovalTaskDto>.Failure("当前账号没有处理该审批任务的权限。");
        }

        var normalizedDecision = NormalizeDecision(request.Decision);
        if (normalizedDecision is null)
        {
            return OperationResult<ApprovalTaskDto>.Failure("审批结果必须是通过或驳回。");
        }

        var instance = await dbContext.WorkflowInstances.FirstOrDefaultAsync(x => x.Id == task.WorkflowInstanceId, cancellationToken);
        if (instance is null)
        {
            return OperationResult<ApprovalTaskDto>.Failure("审批实例不存在。");
        }

        var actor = currentUser.GetActor();

        try
        {
            task.Decide(normalizedDecision, actor, request.Comment.Trim());
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<ApprovalTaskDto>.Failure(ex.Message);
        }

        instance.Complete(normalizedDecision == "Approved" ? WorkflowStatus.Approved : WorkflowStatus.Rejected);

        if (string.Equals(task.DefinitionKey, WorkflowDefinitionCatalog.ProcurementRequestReview, StringComparison.Ordinal))
        {
            var procurementRequest = await dbContext.ProcurementRequests
                .FirstOrDefaultAsync(x => x.Id == task.DocumentId, cancellationToken);
            if (procurementRequest is null)
            {
                return OperationResult<ApprovalTaskDto>.Failure("采购申请不存在。");
            }

            procurementRequest.Decide(normalizedDecision, actor);
        }

        dbContext.Notifications.Add(new Notification(
            normalizedDecision == "Approved" ? "审批通过" : "审批驳回",
            $"{task.DocumentNo} 已由 {actor} {DecisionText(normalizedDecision)}。",
            "Workflow",
            task.DocumentType,
            task.DocumentId,
            task.DocumentNo,
            string.Empty));

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Workflow", "TaskDecided", actor, $"{task.DocumentNo}:{normalizedDecision}", cancellationToken);
        return OperationResult<ApprovalTaskDto>.Success(MapTask(task));
    }

    /// <summary>
    /// Mark Notification Async。
    /// </summary>
    /// <param name="notificationId">notification Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<NotificationDto>> MarkNotificationAsync(
        Guid notificationId,
        MarkNotificationReadRequest request,
        CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (notification is null)
        {
            return OperationResult<NotificationDto>.Failure("未找到通知。");
        }

        if (!string.IsNullOrWhiteSpace(notification.RecipientPermission)
            && !currentUser.Permissions.Contains(notification.RecipientPermission, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult<NotificationDto>.Failure("当前账号没有读取该通知的权限。");
        }

        if (request.IsRead)
        {
            notification.MarkRead();
        }
        else
        {
            notification.MarkUnread();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult<NotificationDto>.Success(MapNotification(notification));
    }

    /// <summary>
    /// Ensure Procurement Request Workflow Async。
    /// </summary>
    /// <param name="procurementRequestId">procurement Request Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task EnsureProcurementRequestWorkflowAsync(Guid procurementRequestId, CancellationToken cancellationToken)
    {
        var request = await dbContext.ProcurementRequests.FirstOrDefaultAsync(x => x.Id == procurementRequestId, cancellationToken);
        if (request is null || !string.Equals(request.Status, ProcurementRequestStatus.Submitted, StringComparison.Ordinal))
        {
            return;
        }

        var definition = await dbContext.WorkflowDefinitions
            .FirstOrDefaultAsync(x => x.Key == WorkflowDefinitionCatalog.ProcurementRequestReview && x.IsEnabled, cancellationToken);
        if (definition is null)
        {
            return;
        }

        var exists = await dbContext.WorkflowInstances.AnyAsync(
            x => x.DefinitionKey == definition.Key && x.DocumentId == request.Id,
            cancellationToken);
        if (exists)
        {
            return;
        }

        var actor = currentUser.GetActor();
        var title = $"采购申请审批：{request.Title}";
        var instance = new WorkflowInstance(
            definition.Id,
            definition.Key,
            definition.DisplayName,
            definition.DocumentType,
            request.Id,
            request.RequestNo,
            title,
            actor);

        dbContext.WorkflowInstances.Add(instance);
        dbContext.ApprovalTasks.Add(new ApprovalTask(
            instance.Id,
            definition.Key,
            definition.DisplayName,
            definition.DocumentType,
            request.Id,
            request.RequestNo,
            title,
            actor,
            definition.RequiredPermission));
        dbContext.Notifications.Add(new Notification(
            "新的采购审批待办",
            $"{request.RequestNo} 已提交，等待审批处理。",
            "Workflow",
            definition.DocumentType,
            request.Id,
            request.RequestNo,
            definition.RequiredPermission));

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Workflow", "TaskCreated", actor, request.RequestNo, cancellationToken);
    }

    /// <summary>
    /// Normalize Decision。
    /// </summary>
    /// <param name="decision">处理决策。</param>
    private static string? NormalizeDecision(string decision)
    {
        if (string.Equals(decision, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return "Approved";
        }

        if (string.Equals(decision, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return "Rejected";
        }

        return null;
    }

    /// <summary>
    /// Decision Text。
    /// </summary>
    /// <param name="decision">处理决策。</param>
    private static string DecisionText(string decision) =>
        decision == "Approved" ? "审批通过" : "审批驳回";

    /// <summary>
    /// 注册Definition 路由。
    /// </summary>
    /// <param name="definition">定义对象。</param>
    private static WorkflowDefinitionDto MapDefinition(WorkflowDefinition definition) =>
        new(
            definition.Id,
            definition.Key,
            definition.DisplayName,
            definition.ModuleKey,
            definition.DocumentType,
            definition.RequiredPermission,
            definition.IsEnabled,
            definition.CreatedAtUtc);

    /// <summary>
    /// 注册Instance 路由。
    /// </summary>
    /// <param name="instance">实例对象。</param>
    private static WorkflowInstanceDto MapInstance(WorkflowInstance instance) =>
        new(
            instance.Id,
            instance.DefinitionId,
            instance.DefinitionKey,
            instance.DefinitionName,
            instance.DocumentType,
            instance.DocumentId,
            instance.DocumentNo,
            instance.Title,
            instance.Status,
            instance.SubmittedBy,
            instance.CreatedAtUtc,
            instance.CompletedAtUtc);

    /// <summary>
    /// 注册Task 路由。
    /// </summary>
    /// <param name="task">任务对象。</param>
    private static ApprovalTaskDto MapTask(ApprovalTask task) =>
        new(
            task.Id,
            task.WorkflowInstanceId,
            task.DefinitionKey,
            task.DefinitionName,
            task.DocumentType,
            task.DocumentId,
            task.DocumentNo,
            task.Title,
            task.Status,
            task.SubmittedBy,
            task.RequiredPermission,
            task.DecidedBy,
            task.Decision,
            task.Comment,
            task.CreatedAtUtc,
            task.DecidedAtUtc);

    /// <summary>
    /// 注册Notification 路由。
    /// </summary>
    /// <param name="notification">通知对象。</param>
    private static NotificationDto MapNotification(Notification notification) =>
        new(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Category,
            notification.RelatedDocumentType,
            notification.RelatedDocumentId,
            notification.RelatedDocumentNo,
            notification.RecipientPermission,
            notification.Status,
            notification.CreatedAtUtc,
            notification.ReadAtUtc);
}
