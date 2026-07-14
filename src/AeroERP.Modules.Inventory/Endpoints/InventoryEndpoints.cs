using AeroERP.Modules.Inventory.Contracts;
using AeroERP.Modules.Inventory.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Inventory.Endpoints;

/// <summary>
/// Inventory 模块 HTTP API 路由映射。
/// </summary>
public static class InventoryEndpoints
{
    /// <summary>
    /// 注册Inventory Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").RequireAuthorization();

        group.MapGet("/pending-procurement-orders", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListPendingProcurementOrdersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapGet("/pending-sales-orders", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListPendingSalesOrdersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapGet("/receipts", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListReceiptsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapPost("/receipts", async (ReceiveProcurementOrderRequest request, IInventoryService service, CancellationToken ct) =>
        {
            var result = await service.ReceiveProcurementOrderAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/inventory/receipts", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryReceiptManage });

        group.MapGet("/issues", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListIssuesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapPost("/issues", async (IssueSalesOrderRequest request, IInventoryService service, CancellationToken ct) =>
        {
            var result = await service.IssueSalesOrderAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/inventory/issues", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryIssueManage });

        group.MapGet("/transfers", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListTransfersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapPost("/transfers", async (CreateInventoryTransferRequest request, IInventoryService service, CancellationToken ct) =>
        {
            var result = await service.CreateTransferAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/inventory/transfers", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryTransferManage });

        group.MapGet("/counts", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListCountAdjustmentsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapPost("/counts", async (CreateInventoryCountAdjustmentRequest request, IInventoryService service, CancellationToken ct) =>
        {
            var result = await service.CreateCountAdjustmentAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/inventory/counts", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryCountManage });

        group.MapGet("/movements", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListMovementsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapGet("/ledger", async (Guid? warehouseId, Guid? itemId, IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListInventoryLedgerAsync(warehouseId, itemId, ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapGet("/balances", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListStockBalancesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapGet("/locations", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListWarehouseLocationsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        group.MapPost("/locations", async (CreateWarehouseLocationRequest request, IInventoryService service, CancellationToken ct) =>
        {
            var result = await service.CreateWarehouseLocationAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/inventory/locations", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryLocationManage });

        group.MapGet("/location-balances", async (IInventoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListLocationStockBalancesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.InventoryRead });

        return group;
    }
}
