using AeroERP.Modules.Quality.Contracts;
using AeroERP.Modules.Quality.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Quality.Endpoints;

/// <summary>
/// Quality 模块 HTTP API 路由映射。
/// </summary>
public static class QualityEndpoints
{
    /// <summary>
    /// 注册Quality Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapQualityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quality").RequireAuthorization();

        group.MapGet("/source-candidates", async (IQualityService service, CancellationToken ct) =>
            Results.Ok(await service.ListSourceCandidatesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.QualityRead });

        group.MapGet("/inspections", async (IQualityService service, CancellationToken ct) =>
            Results.Ok(await service.ListInspectionsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.QualityRead });

        group.MapPost("/inspections", async (CreateQualityInspectionRequest request, IQualityService service, CancellationToken ct) =>
        {
            var result = await service.CreateInspectionAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/quality/inspections", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.QualityInspectionManage });

        group.MapGet("/lot-trace-events", async (IQualityService service, CancellationToken ct) =>
            Results.Ok(await service.ListLotTraceEventsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.QualityRead });

        group.MapPost("/lot-trace-events", async (CreateLotTraceEventRequest request, IQualityService service, CancellationToken ct) =>
        {
            var result = await service.CreateLotTraceEventAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/quality/lot-trace-events", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.QualityTraceabilityManage });

        group.MapGet("/lots/{lotNo}", async (string lotNo, IQualityService service, CancellationToken ct) =>
            Results.Ok(await service.GetLotTraceAsync(lotNo, ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.QualityRead });

        return group;
    }
}
