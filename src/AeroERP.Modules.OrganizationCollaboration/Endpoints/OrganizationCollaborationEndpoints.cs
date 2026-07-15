using AeroERP.Modules.OrganizationCollaboration.Contracts;
using AeroERP.Modules.OrganizationCollaboration.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;

namespace AeroERP.Modules.OrganizationCollaboration.Endpoints;

/// <summary>
/// 组织协同模块 HTTP API 路由映射。
/// </summary>
public static class OrganizationCollaborationEndpoints
{
    /// <summary>
    /// 注册组织协同路由。
    /// </summary>
    public static RouteGroupBuilder MapOrganizationCollaborationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organization-collaboration").RequireAuthorization();

        group.MapGet("/conversations", async (IOrganizationCollaborationService service, CancellationToken ct) =>
            Results.Ok(await service.ListConversationsAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.OrganizationCollaborationRead));

        group.MapPost("/direct-conversations", async (EnsureDirectConversationRequest request, IOrganizationCollaborationService service, CancellationToken ct) =>
        {
            var result = await service.EnsureDirectConversationAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.OrganizationCollaborationMessage));

        group.MapGet("/conversations/{conversationId:guid}/messages", async (Guid conversationId, IOrganizationCollaborationService service, CancellationToken ct) =>
        {
            var result = await service.ListMessagesAsync(conversationId, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.OrganizationCollaborationRead));

        group.MapPost("/conversations/{conversationId:guid}/messages", async (Guid conversationId, SendCollaborationMessageRequest request, IOrganizationCollaborationService service, CancellationToken ct) =>
        {
            var result = await service.SendMessageAsync(conversationId, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.OrganizationCollaborationMessage));

        group.MapPut("/conversations/{conversationId:guid}/read-state", async (Guid conversationId, MarkCollaborationConversationReadRequest request, IOrganizationCollaborationService service, CancellationToken ct) =>
        {
            var result = await service.MarkConversationReadAsync(conversationId, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.OrganizationCollaborationRead));

        group.MapGet("/attachments/{attachmentId:guid}/download", async (Guid attachmentId, IOrganizationCollaborationService service, CancellationToken ct) =>
        {
            var result = await service.DownloadAttachmentAsync(attachmentId, ct);
            return result is { IsSuccess: true, Value: not null }
                ? Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
                : Results.NotFound(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.OrganizationCollaborationRead));

        group.MapGet("/events", async (HttpContext httpContext, IOrganizationCollaborationService service, CancellationToken ct) =>
        {
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers.Connection = "keep-alive";
            httpContext.Response.ContentType = "text/event-stream";

            var cursor = 0L;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var eventDto = await service.GetEventAsync(cursor, ct);
                    cursor = Math.Max(cursor, eventDto.Cursor);
                    await httpContext.Response.WriteAsync($"event: {eventDto.EventKey}\n", ct);
                    await httpContext.Response.WriteAsync($"data: {JsonSerializer.Serialize(eventDto)}\n\n", ct);
                    await httpContext.Response.Body.FlushAsync(ct);
                    await Task.Delay(TimeSpan.FromSeconds(3), ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
        }).RequireAuthorization(Policy(PlatformPermissions.OrganizationCollaborationRead));

        return group;
    }

    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
