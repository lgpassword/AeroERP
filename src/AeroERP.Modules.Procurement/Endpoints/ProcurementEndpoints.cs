using AeroERP.Modules.Procurement.Contracts;
using AeroERP.Modules.Procurement.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Procurement.Endpoints;

/// <summary>
/// Procurement 模块 HTTP API 路由映射。
/// </summary>
public static class ProcurementEndpoints
{
    /// <summary>
    /// 注册Procurement Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapProcurementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/procurement").RequireAuthorization();

        group.MapGet("/requests", async (IProcurementService service, CancellationToken ct) =>
            Results.Ok(await service.ListRequestsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ProcurementRead });

        group.MapPost("/requests", async (CreateProcurementRequestRequest request, IProcurementService service, CancellationToken ct) =>
        {
            var result = await service.CreateRequestAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/procurement/requests", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ProcurementRequestCreate });

        group.MapPost("/requests/{id:guid}/decision", async (Guid id, DecideProcurementRequestRequest request, IProcurementService service, CancellationToken ct) =>
        {
            var result = await service.DecideRequestAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ProcurementRequestReview });

        group.MapPost("/requests/{id:guid}/convert-order", async (Guid id, IProcurementService service, CancellationToken ct) =>
        {
            var result = await service.ConvertToOrderAsync(id, ct);
            return result.IsSuccess ? Results.Created("/api/procurement/orders", result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ProcurementOrderCreate });

        group.MapGet("/orders", async (IProcurementService service, CancellationToken ct) =>
            Results.Ok(await service.ListOrdersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ProcurementRead });

        group.MapPost("/orders/{id:guid}/release", async (Guid id, IProcurementService service, CancellationToken ct) =>
        {
            var result = await service.ReleaseOrderAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ProcurementOrderRelease });

        return group;
    }
}
