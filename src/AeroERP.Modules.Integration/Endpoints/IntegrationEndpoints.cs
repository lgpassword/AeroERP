using AeroERP.Modules.Integration.Contracts;
using AeroERP.Modules.Integration.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Integration.Endpoints;

/// <summary>
/// Integration 模块 HTTP API 路由映射。
/// </summary>
public static class IntegrationEndpoints
{
    /// <summary>
    /// 注册Integration Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/integration").RequireAuthorization();

        group.MapGet("/overview", async (IIntegrationService service, CancellationToken ct) =>
            Results.Ok(await service.GetOverviewAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.IntegrationRead));

        group.MapPost("/channels", async (UpsertMessageChannelRequest request, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.UpsertChannelAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationManage));

        group.MapPost("/webhooks", async (UpsertWebhookSubscriptionRequest request, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.UpsertWebhookAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationManage));

        group.MapPost("/connectors", async (UpsertExternalConnectorRequest request, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.UpsertConnectorAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationManage));

        group.MapPost("/sync-jobs", async (CreateIntegrationSyncJobRequest request, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.CreateSyncJobAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationManage));

        group.MapPost("/sync-jobs/{id:guid}/start", async (Guid id, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.StartSyncJobAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationExecute));

        group.MapPost("/sync-jobs/{id:guid}/complete", async (Guid id, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.CompleteSyncJobAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationExecute));

        group.MapPost("/sync-jobs/{id:guid}/fail", async (Guid id, FailIntegrationSyncJobRequest request, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.FailSyncJobAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationExecute));

        group.MapPost("/sync-jobs/{id:guid}/retry", async (Guid id, IIntegrationService service, CancellationToken ct) =>
        {
            var result = await service.RetrySyncJobAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.IntegrationExecute));

        return group;
    }

    /// <summary>
    /// Policy。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
