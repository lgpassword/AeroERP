using AeroERP.Modules.Workflow.Contracts;
using AeroERP.Modules.Workflow.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Workflow.Endpoints;

/// <summary>
/// Workflow 模块 HTTP API 路由映射。
/// </summary>
public static class WorkflowEndpoints
{
    /// <summary>
    /// 注册Workflow Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workflow").RequireAuthorization();

        group.MapGet("/definitions", async (IWorkflowService service, CancellationToken ct) =>
            Results.Ok(await service.ListDefinitionsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.WorkflowRead });

        group.MapGet("/instances", async (IWorkflowService service, CancellationToken ct) =>
            Results.Ok(await service.ListInstancesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.WorkflowRead });

        group.MapGet("/tasks", async (IWorkflowService service, CancellationToken ct) =>
            Results.Ok(await service.ListTasksAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.WorkflowRead });

        group.MapPost("/tasks/{id:guid}/decision", async (Guid id, DecideApprovalTaskRequest request, IWorkflowService service, CancellationToken ct) =>
        {
            var result = await service.DecideTaskAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.WorkflowTaskDecide });

        group.MapGet("/notifications", async (IWorkflowService service, CancellationToken ct) =>
            Results.Ok(await service.ListNotificationsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.NotificationRead });

        group.MapPut("/notifications/{id:guid}/read-state", async (Guid id, MarkNotificationReadRequest request, IWorkflowService service, CancellationToken ct) =>
        {
            var result = await service.MarkNotificationAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.NotificationRead });

        return group;
    }
}
