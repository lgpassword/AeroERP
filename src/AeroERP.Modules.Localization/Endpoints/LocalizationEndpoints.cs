using AeroERP.Modules.Localization.Contracts;
using AeroERP.Modules.Localization.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Localization.Endpoints;

/// <summary>
/// Localization 模块 HTTP API 路由映射。
/// </summary>
public static class LocalizationEndpoints
{
    /// <summary>
    /// 注册Localization Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapLocalizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/localization").RequireAuthorization();

        group.MapGet("/currencies", async (ILocalizationService service, CancellationToken ct) =>
            Results.Ok(await service.ListCurrenciesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.LocalizationRead });

        group.MapPost("/currencies", async (UpsertCurrencyRequest request, ILocalizationService service, CancellationToken ct) =>
        {
            var result = await service.UpsertCurrencyAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.LocalizationManage });

        group.MapGet("/settings", async (ILocalizationService service, CancellationToken ct) =>
            Results.Ok(await service.GetSettingsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.LocalizationRead });

        group.MapPut("/settings", async (UpdateLocalizationSettingsRequest request, ILocalizationService service, CancellationToken ct) =>
        {
            var result = await service.UpdateSettingsAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.LocalizationManage });

        group.MapGet("/content", async (ILocalizationService service, CancellationToken ct) =>
            Results.Ok(await service.ListContentAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.LocalizationRead });

        group.MapPost("/content", async (UpsertLocalizationContentRequest request, ILocalizationService service, CancellationToken ct) =>
        {
            var result = await service.UpsertContentAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.LocalizationManage });

        return group;
    }
}
