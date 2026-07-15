using AeroERP.Modules.Wms.Contracts;
using AeroERP.Modules.Wms.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Wms.Endpoints;

/// <summary>
/// Wms 模块 HTTP API 路由映射。
/// </summary>
public static class WmsEndpoints
{
    /// <summary>
    /// 注册Wms Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapWmsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wms").RequireAuthorization();

        group.MapGet("/overview", async (IWmsService service, CancellationToken ct) =>
            Results.Ok(await service.GetOverviewAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.WmsRead));

        group.MapPost("/containers", async (UpsertWarehouseContainerRequest request, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.UpsertContainerAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsManage));

        group.MapPost("/routes", async (UpsertWarehouseRouteRequest request, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.UpsertRouteAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsManage));

        group.MapPost("/put-away-tasks", async (CreatePutAwayTaskRequest request, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.CreatePutAwayTaskAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsManage));

        group.MapPost("/put-away-tasks/{id:guid}/complete", async (Guid id, CompletePutAwayTaskRequest request, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.CompletePutAwayTaskAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsExecute));

        group.MapPost("/picking-tasks", async (CreatePickingTaskRequest request, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.CreatePickingTaskAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsManage));

        group.MapPost("/picking-tasks/{id:guid}/complete", async (Guid id, CompletePickingTaskRequest request, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.CompletePickingTaskAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsExecute));

        group.MapPost("/waves", async (CreatePickingWaveRequest request, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.CreateWaveAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsManage));

        group.MapPost("/waves/{id:guid}/release", async (Guid id, IWmsService service, CancellationToken ct) =>
        {
            var result = await service.ReleaseWaveAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.WmsExecute));

        return group;
    }

    /// <summary>
    /// Policy。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
