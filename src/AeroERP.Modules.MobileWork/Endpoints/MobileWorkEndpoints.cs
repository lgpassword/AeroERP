using AeroERP.Modules.MobileWork.Contracts;
using AeroERP.Modules.MobileWork.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.MobileWork.Endpoints;

/// <summary>
/// Mobile Work 模块 HTTP API 路由映射。
/// </summary>
public static class MobileWorkEndpoints
{
    /// <summary>
    /// 注册Mobile Work Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapMobileWorkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/mobile-work").RequireAuthorization();

        group.MapGet("/overview", async (IMobileWorkService service, CancellationToken ct) =>
            Results.Ok(await service.GetOverviewAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.MobileWorkRead));

        group.MapPost("/devices", async (UpsertMobileDeviceRequest request, IMobileWorkService service, CancellationToken ct) =>
        {
            var result = await service.UpsertDeviceAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.MobileWorkManage));

        group.MapPost("/offline-tasks", async (CreateMobileOfflineTaskRequest request, IMobileWorkService service, CancellationToken ct) =>
        {
            var result = await service.CreateOfflineTaskAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.MobileWorkManage));

        group.MapPost("/offline-tasks/{id:guid}/sync", async (Guid id, IMobileWorkService service, CancellationToken ct) =>
        {
            var result = await service.MarkOfflineTaskSyncedAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.MobileWorkExecute));

        group.MapPost("/offline-tasks/{id:guid}/complete", async (Guid id, IMobileWorkService service, CancellationToken ct) =>
        {
            var result = await service.CompleteOfflineTaskAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.MobileWorkExecute));

        group.MapPost("/scan-events", async (RecordMobileScanEventRequest request, IMobileWorkService service, CancellationToken ct) =>
        {
            var result = await service.RecordScanEventAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.MobileWorkExecute));

        return group;
    }

    /// <summary>
    /// Policy。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
