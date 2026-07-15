using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Workflow.Contracts;

namespace AeroERP.Modules.Workflow.Services;

/// <summary>
/// Workflow Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// 查询Definitions。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<WorkflowDefinitionDto>> ListDefinitionsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Instances。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<WorkflowInstanceDto>> ListInstancesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Tasks。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ApprovalTaskDto>> ListTasksAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Notifications。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<NotificationDto>> ListNotificationsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Decide Task。
    /// </summary>
    /// <param name="taskId">task Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ApprovalTaskDto>> DecideTaskAsync(Guid taskId, DecideApprovalTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Mark Notification。
    /// </summary>
    /// <param name="notificationId">notification Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<NotificationDto>> MarkNotificationAsync(Guid notificationId, MarkNotificationReadRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Ensure Procurement Request Workflow。
    /// </summary>
    /// <param name="procurementRequestId">procurement Request Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task EnsureProcurementRequestWorkflowAsync(Guid procurementRequestId, CancellationToken cancellationToken);
}
