using AeroERP.Modules.Sales.Contracts;
using AeroERP.Modules.Sales.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Sales.Endpoints;

/// <summary>
/// Sales 模块 HTTP API 路由映射。
/// </summary>
public static class SalesEndpoints
{
    /// <summary>
    /// 注册Sales Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales").RequireAuthorization();

        group.MapGet("/quotations", async (ISalesService service, CancellationToken ct) =>
            Results.Ok(await service.ListQuotationsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.SalesRead });

        group.MapPost("/quotations", async (CreateSalesQuotationRequest request, ISalesService service, CancellationToken ct) =>
        {
            var result = await service.CreateQuotationAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/sales/quotations", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.SalesQuotationCreate });

        group.MapPost("/quotations/{id:guid}/convert-order", async (Guid id, ISalesService service, CancellationToken ct) =>
        {
            var result = await service.ConvertToOrderAsync(id, ct);
            return result.IsSuccess
                ? Results.Created("/api/sales/orders", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.SalesOrderCreate });

        group.MapGet("/orders", async (ISalesService service, CancellationToken ct) =>
            Results.Ok(await service.ListOrdersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.SalesRead });

        group.MapPost("/orders/{id:guid}/confirm", async (Guid id, ISalesService service, CancellationToken ct) =>
        {
            var result = await service.ConfirmOrderAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.SalesOrderManage });

        group.MapPost("/orders/{id:guid}/ready-to-ship", async (Guid id, ISalesService service, CancellationToken ct) =>
        {
            var result = await service.MarkOrderReadyToShipAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.SalesOrderManage });

        return group;
    }
}
