using AeroERP.Modules.Manufacturing.Contracts;
using AeroERP.Modules.Manufacturing.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Manufacturing.Endpoints;

/// <summary>
/// Manufacturing 模块 HTTP API 路由映射。
/// </summary>
public static class ManufacturingEndpoints
{
    /// <summary>
    /// 注册Manufacturing Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapManufacturingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/manufacturing").RequireAuthorization();

        group.MapGet("/boms", async (IManufacturingService service, CancellationToken ct) =>
            Results.Ok(await service.ListBomsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingRead });

        group.MapPost("/boms", async (CreateBomRequest request, IManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.CreateBomAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/manufacturing/boms", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingBomManage });

        group.MapGet("/work-orders", async (IManufacturingService service, CancellationToken ct) =>
            Results.Ok(await service.ListWorkOrdersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingRead });

        group.MapPost("/work-orders", async (CreateWorkOrderRequest request, IManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.CreateWorkOrderAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/manufacturing/work-orders", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingWorkOrderManage });

        group.MapPost("/work-orders/{workOrderId:guid}/release", async (Guid workOrderId, IManufacturingService service, CancellationToken ct) =>
        {
            var result = await service.ReleaseWorkOrderAsync(workOrderId, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingWorkOrderManage });

        group.MapGet("/production-issues", async (IManufacturingService service, CancellationToken ct) =>
            Results.Ok(await service.ListProductionIssuesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingRead });

        group.MapPost("/work-orders/{workOrderId:guid}/issue", async (
            Guid workOrderId,
            ExecuteProductionIssueRequest request,
            IManufacturingService service,
            CancellationToken ct) =>
        {
            var result = await service.ExecuteProductionIssueAsync(workOrderId, request, ct);
            return result.IsSuccess
                ? Results.Created("/api/manufacturing/production-issues", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingExecutionManage });

        group.MapGet("/production-receipts", async (IManufacturingService service, CancellationToken ct) =>
            Results.Ok(await service.ListProductionReceiptsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingRead });

        group.MapPost("/work-orders/{workOrderId:guid}/complete", async (
            Guid workOrderId,
            CompleteProductionRequest request,
            IManufacturingService service,
            CancellationToken ct) =>
        {
            var result = await service.CompleteProductionAsync(workOrderId, request, ct);
            return result.IsSuccess
                ? Results.Created("/api/manufacturing/production-receipts", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.ManufacturingExecutionManage });

        return group;
    }
}
