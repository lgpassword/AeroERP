using AeroERP.Modules.PositionPermissions.Contracts;
using AeroERP.Modules.PositionPermissions.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.PositionPermissions.Endpoints;

/// <summary>
/// Position Permission 模块 HTTP API 路由映射。
/// </summary>
public static class PositionPermissionEndpoints
{
    /// <summary>
    /// 注册Position Permission Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapPositionPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/position-permissions").RequireAuthorization();

        group.MapGet("/overview", async (IPositionPermissionService service, CancellationToken ct) =>
            Results.Ok(await service.GetOverviewAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.PositionPermissionsRead));

        group.MapPost("/departments", async (UpsertDepartmentRequest request, IPositionPermissionService service, CancellationToken ct) =>
        {
            var result = await service.UpsertDepartmentAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.PositionPermissionsManage));

        group.MapPost("/positions", async (UpsertJobPositionRequest request, IPositionPermissionService service, CancellationToken ct) =>
        {
            var result = await service.UpsertPositionAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.PositionPermissionsManage));

        group.MapPost("/roles", async (UpsertCustomRoleRequest request, IPositionPermissionService service, CancellationToken ct) =>
        {
            var result = await service.UpsertCustomRoleAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.PositionPermissionsManage));

        group.MapPost("/permission-packages", async (UpsertPermissionPackageRequest request, IPositionPermissionService service, CancellationToken ct) =>
        {
            var result = await service.UpsertPermissionPackageAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.PositionPermissionsManage));

        group.MapPut("/positions/{id:guid}/role-bindings", async (Guid id, UpdatePositionRoleBindingsRequest request, IPositionPermissionService service, CancellationToken ct) =>
        {
            var result = await service.UpdatePositionRoleBindingsAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.PositionPermissionsManage));

        group.MapPut("/positions/{id:guid}/data-scope-rules", async (Guid id, UpdatePositionDataScopeRulesRequest request, IPositionPermissionService service, CancellationToken ct) =>
        {
            var result = await service.UpdatePositionDataScopeRulesAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.PositionPermissionsManage));

        return group;
    }

    /// <summary>
    /// Policy。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
