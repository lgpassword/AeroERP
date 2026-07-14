using AeroERP.Modules.AdvancedManufacturing.Contracts;
using AeroERP.Modules.AdvancedManufacturing.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.AdvancedManufacturing.Endpoints;

/// <summary>
/// Advanced Manufacturing 模块 HTTP API 路由映射。
/// </summary>
public static class AdvancedManufacturingEndpoints
{
    /// <summary>
    /// 注册Advanced Manufacturing Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapAdvancedManufacturingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/advanced-manufacturing").RequireAuthorization();

        group.MapGet("/overview", async (IAdvancedManufacturingService service, CancellationToken ct) =>
            Results.Ok(await service.GetOverviewAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingRead));

        group.MapPost("/work-centers", async (UpsertWorkCenterRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.UpsertWorkCenterAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingManage));

        group.MapPost("/routings", async (CreateManufacturingRoutingRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.CreateRoutingAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingManage));

        group.MapPost("/routings/{id:guid}/activate", async (Guid id, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.ActivateRoutingAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingManage));

        group.MapPost("/operation-schedules", async (CreateOperationScheduleRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.CreateOperationScheduleAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingSchedule));

        group.MapPost("/operation-schedules/{id:guid}/release", async (Guid id, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.ReleaseOperationScheduleAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingSchedule));

        group.MapPost("/operation-schedules/{id:guid}/complete", async (Guid id, CompleteOperationScheduleRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.CompleteOperationScheduleAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingSchedule));

        group.MapPost("/capacity-loads", async (UpsertCapacityLoadRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.UpsertCapacityLoadAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingSchedule));

        group.MapPost("/cost-snapshots", async (CreateCostSnapshotRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.CreateCostSnapshotAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingCostManage));

        group.MapPost("/mrp-suggestions/generate", async (GenerateMrpSuggestionRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.GenerateMrpSuggestionAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingMrpManage));

        group.MapPost("/mrp-suggestions/{id:guid}/decision", async (Guid id, DecideMrpSuggestionRequest request, IAdvancedManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.DecideMrpSuggestionAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.AdvancedManufacturingMrpManage));

        return group;
    }

    /// <summary>
    /// Policy。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
