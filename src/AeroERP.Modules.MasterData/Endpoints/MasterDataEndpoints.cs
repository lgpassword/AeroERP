using AeroERP.Modules.MasterData.Contracts;
using AeroERP.Modules.MasterData.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.MasterData.Endpoints;

/// <summary>
/// Master Data 模块 HTTP API 路由映射。
/// </summary>
public static class MasterDataEndpoints
{
    /// <summary>
    /// 注册Master Data Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapMasterDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/master-data").RequireAuthorization();

        group.MapGet("/customers", async (IMasterDataService service, CancellationToken ct) =>
            Results.Ok(await service.ListCustomersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataRead });

        group.MapPost("/customers", async (UpsertCustomerRequest request, IMasterDataService service, CancellationToken ct) =>
            Results.Created("/api/master-data/customers", await service.CreateCustomerAsync(request, ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        group.MapPut("/customers/{id:guid}", async (Guid id, UpsertCustomerRequest request, IMasterDataService service, CancellationToken ct) =>
        {
            var result = await service.UpdateCustomerAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        group.MapGet("/suppliers", async (IMasterDataService service, CancellationToken ct) =>
            Results.Ok(await service.ListSuppliersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataRead });

        group.MapPost("/suppliers", async (UpsertSupplierRequest request, IMasterDataService service, CancellationToken ct) =>
            Results.Created("/api/master-data/suppliers", await service.CreateSupplierAsync(request, ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        group.MapPut("/suppliers/{id:guid}", async (Guid id, UpsertSupplierRequest request, IMasterDataService service, CancellationToken ct) =>
        {
            var result = await service.UpdateSupplierAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        group.MapGet("/items", async (IMasterDataService service, CancellationToken ct) =>
            Results.Ok(await service.ListItemsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataRead });

        group.MapPost("/items", async (UpsertItemRequest request, IMasterDataService service, CancellationToken ct) =>
            Results.Created("/api/master-data/items", await service.CreateItemAsync(request, ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        group.MapPut("/items/{id:guid}", async (Guid id, UpsertItemRequest request, IMasterDataService service, CancellationToken ct) =>
        {
            var result = await service.UpdateItemAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        group.MapGet("/warehouses", async (IMasterDataService service, CancellationToken ct) =>
            Results.Ok(await service.ListWarehousesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataRead });

        group.MapPost("/warehouses", async (UpsertWarehouseRequest request, IMasterDataService service, CancellationToken ct) =>
            Results.Created("/api/master-data/warehouses", await service.CreateWarehouseAsync(request, ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        group.MapPut("/warehouses/{id:guid}", async (Guid id, UpsertWarehouseRequest request, IMasterDataService service, CancellationToken ct) =>
        {
            var result = await service.UpdateWarehouseAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.MasterDataManage });

        return group;
    }
}
