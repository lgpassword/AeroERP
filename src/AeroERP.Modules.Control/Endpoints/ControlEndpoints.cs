using AeroERP.Modules.Control.Contracts;
using AeroERP.Modules.Control.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Control.Endpoints;

/// <summary>
/// Control 模块 HTTP API 路由映射。
/// </summary>
public static class ControlEndpoints
{
    /// <summary>
    /// 注册Control Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapControlEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/control").RequireAuthorization();

        group.MapGet("/analytics", async (IControlService service, CancellationToken ct) =>
            Results.Ok(await service.GetAnalyticsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ControlAnalyticsRead });

        group.MapGet("/data-scope-rules", async (IControlService service, CancellationToken ct) =>
            Results.Ok(await service.ListDataScopeRulesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ControlDataScopeManage });

        group.MapPost("/data-scope-rules", async (UpsertDataScopeRuleRequest request, IControlService service, CancellationToken ct) =>
        {
            var result = await service.UpsertDataScopeRuleAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ControlDataScopeManage });

        group.MapGet("/numbering-rules", async (IControlService service, CancellationToken ct) =>
            Results.Ok(await service.ListNumberingRulesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ControlNumberingManage });

        group.MapPost("/numbering-rules", async (UpsertNumberingRuleRequest request, IControlService service, CancellationToken ct) =>
        {
            var result = await service.UpsertNumberingRuleAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ControlNumberingManage });

        return group;
    }
}
