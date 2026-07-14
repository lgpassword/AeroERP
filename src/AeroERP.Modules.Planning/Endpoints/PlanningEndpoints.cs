using AeroERP.Modules.Planning.Contracts;
using AeroERP.Modules.Planning.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Planning.Endpoints;

/// <summary>
/// Planning 模块 HTTP API 路由映射。
/// </summary>
public static class PlanningEndpoints
{
    /// <summary>
    /// 注册Planning Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/planning").RequireAuthorization();

        group.MapGet("/suggestions", async (IPlanningService service, CancellationToken ct) =>
            Results.Ok(await service.ListSuggestionsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.PlanningRead });

        group.MapPost("/suggestions/generate", async (GeneratePlanningSuggestionRequest request, IPlanningService service, CancellationToken ct) =>
        {
            var result = await service.GenerateSuggestionAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/planning/suggestions", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.PlanningManage });

        group.MapPost("/suggestions/{id:guid}/decision", async (Guid id, PlanningSuggestionDecisionRequest request, IPlanningService service, CancellationToken ct) =>
        {
            var result = await service.DecideSuggestionAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.PlanningManage });

        group.MapGet("/outsourcing-orders", async (IPlanningService service, CancellationToken ct) =>
            Results.Ok(await service.ListOutsourcingOrdersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.PlanningRead });

        group.MapPost("/outsourcing-orders", async (CreateOutsourcingOrderRequest request, IPlanningService service, CancellationToken ct) =>
        {
            var result = await service.CreateOutsourcingOrderAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/planning/outsourcing-orders", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.OutsourcingManage });

        group.MapPost("/outsourcing-orders/{id:guid}/issue-materials", async (Guid id, IPlanningService service, CancellationToken ct) =>
        {
            var result = await service.IssueOutsourcingMaterialsAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.OutsourcingManage });

        group.MapPost("/outsourcing-orders/{id:guid}/receive", async (Guid id, ReceiveOutsourcingOrderRequest request, IPlanningService service, CancellationToken ct) =>
        {
            var result = await service.ReceiveOutsourcingOrderAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.OutsourcingManage });

        group.MapGet("/barcode-executions", async (IPlanningService service, CancellationToken ct) =>
            Results.Ok(await service.ListBarcodeExecutionsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.PlanningRead });

        group.MapPost("/barcode-executions", async (BarcodeExecutionRequest request, IPlanningService service, CancellationToken ct) =>
        {
            var result = await service.ExecuteBarcodeAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/planning/barcode-executions", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.BarcodeExecute });

        return group;
    }
}
