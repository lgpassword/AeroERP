using AeroERP.Platform.Contracts;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.AppHost;

/// <summary>
/// Platform 模块 HTTP API 路由映射。
/// </summary>
public static class PlatformEndpoints
{
    /// <summary>
    /// 注册Platform Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapPlatformEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/platform");
        var securedGroup = group.RequireAuthorization();

        group.MapPost("/auth/login", async (LoginRequest request, IAuthService service, CancellationToken ct) =>
        {
            var result = await service.LoginAsync(request, ct);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous();

        securedGroup.MapGet("/auth/me", async (ICurrentUserAccessor currentUser, IAuthService service, CancellationToken ct) =>
        {
            if (currentUser.UserId is null)
            {
                return Results.Unauthorized();
            }

            var result = await service.GetCurrentUserAsync(currentUser.UserId.Value, ct);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        });

        securedGroup.MapGet("/organizations", async (IOrganizationService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)));

        securedGroup.MapPost("/organizations", async (CreateOrganizationRequest request, IOrganizationService service, CancellationToken ct) =>
            Results.Created("/api/platform/organizations", await service.CreateAsync(request, ct)))
            .RequireAuthorization(PlatformPermission(PlatformPermissions.OrganizationManage));

        securedGroup.MapGet("/roles", async (IUserManagementService service, CancellationToken ct) =>
            Results.Ok(await service.ListRolesAsync(ct)))
            .RequireAuthorization(PlatformPermission(PlatformPermissions.IdentityRoleManage));

        securedGroup.MapGet("/role-options", async (IUserManagementService service, CancellationToken ct) =>
            Results.Ok(await service.ListRolesAsync(ct)));

        securedGroup.MapGet("/users", async (IUserManagementService service, CancellationToken ct) =>
            Results.Ok(await service.ListUsersAsync(ct)))
            .RequireAuthorization(PlatformPermission(PlatformPermissions.IdentityUserRead));

        securedGroup.MapPost("/users", async (CreateUserRequest request, IUserManagementService service, CancellationToken ct) =>
        {
            try
            {
                return Results.Created("/api/platform/users", await service.CreateUserAsync(request, ct));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }).RequireAuthorization(PlatformPermission(PlatformPermissions.IdentityUserManage));

        securedGroup.MapPut("/users/{id:guid}/roles", async (Guid id, UpdateUserRolesRequest request, IUserManagementService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.UpdateUserRolesAsync(id, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }).RequireAuthorization(PlatformPermission(PlatformPermissions.IdentityUserManage));

        securedGroup.MapPut("/users/{id:guid}/status", async (Guid id, UpdateUserStatusRequest request, IUserManagementService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.UpdateUserStatusAsync(id, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }).RequireAuthorization(PlatformPermission(PlatformPermissions.IdentityUserManage));

        securedGroup.MapPost("/users/{id:guid}/reset-password", async (Guid id, ResetUserPasswordRequest request, IUserManagementService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.ResetUserPasswordAsync(id, request, ct);
                return result ? Results.NoContent() : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }).RequireAuthorization(PlatformPermission(PlatformPermissions.IdentityUserPasswordManage));

        securedGroup.MapPost("/auth/change-password", async (ChangePasswordRequest request, IUserManagementService service, CancellationToken ct) =>
        {
            try
            {
                await service.ChangeCurrentUserPasswordAsync(request, ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        securedGroup.MapPut("/roles/{id:guid}/modules", async (Guid id, UpdateModuleAccessRequest request, IUserManagementService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.UpdateRoleModulesAsync(id, request, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }).RequireAuthorization(PlatformPermission(PlatformPermissions.IdentityRoleManage));

        securedGroup.MapGet("/modules", async (IModuleVisibilityService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)))
            .RequireAuthorization(PlatformPermission(PlatformPermissions.PluginManage));

        securedGroup.MapGet("/visible-modules", async (IModuleVisibilityService service, CancellationToken ct) =>
            Results.Ok(await service.ListVisibleAsync(ct)));

        securedGroup.MapPut("/modules/{id:guid}/visibility", async (Guid id, ToggleModuleVisibilityRequest request, IModuleVisibilityService service, AeroErpDbContext db, CancellationToken ct) =>
        {
            var module = await db.PluginModules.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (module is null)
            {
                return Results.NotFound();
            }

            if (string.Equals(module.Key, "platform", StringComparison.OrdinalIgnoreCase) && !request.IsVisible)
            {
                return Results.BadRequest(new { message = "平台治理模块不能隐藏。" });
            }

            var result = await service.ToggleAsync(id, request.IsVisible, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization(PlatformPermission(PlatformPermissions.PluginManage));

        securedGroup.MapGet("/agent-reviews", async (IAgentReviewService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)));

        securedGroup.MapPost("/agent-reviews", async (SubmitAgentReviewRequest request, IAgentReviewService service, CancellationToken ct) =>
            Results.Created("/api/platform/agent-reviews", await service.SubmitAsync(request, ct)))
            .RequireAuthorization(PlatformPermission(PlatformPermissions.AgentReviewSubmit));

        securedGroup.MapPost("/agent-reviews/{id:guid}/decision", async (Guid id, DecideAgentReviewRequest request, IAgentReviewService service, CancellationToken ct) =>
        {
            var result = await service.DecideAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization(PlatformPermission(PlatformPermissions.AgentReviewDecide));

        securedGroup.MapGet("/audit-events", async (AeroErpDbContext db, CancellationToken ct) =>
        {
            var events = await db.AuditEvents
                .Select(x => new
                {
                    x.Id,
                    x.Category,
                    x.Action,
                    x.Actor,
                    x.Detail,
                    x.CreatedAtUtc
                })
                .ToListAsync(ct);

            return Results.Ok(events
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(50));
        });

        return group;
    }

    /// <summary>
    /// Platform Permission。
    /// </summary>
    /// <param name="policy">授权策略。</param>
    private static AuthorizeAttribute PlatformPermission(string policy)
    {
        return new AuthorizeAttribute { Policy = policy };
    }
}
