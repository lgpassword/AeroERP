using AeroERP.Modules.Reporting.Contracts;
using AeroERP.Modules.Reporting.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Reporting.Endpoints;

/// <summary>
/// Reporting 模块 HTTP API 路由映射。
/// </summary>
public static class ReportingEndpoints
{
    /// <summary>
    /// 注册Reporting Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reporting").RequireAuthorization();

        group.MapGet("/overview", async (IReportingService service, CancellationToken ct) =>
            Results.Ok(await service.GetOverviewAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.ReportingRead));

        group.MapPost("/definitions", async (UpsertReportDefinitionRequest request, IReportingService service, CancellationToken ct) =>
        {
            var result = await service.UpsertDefinitionAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.ReportingManage));

        group.MapPost("/runs", async (RunReportRequest request, IReportingService service, CancellationToken ct) =>
        {
            var result = await service.RunReportAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.ReportingRead));

        group.MapPost("/export-tasks", async (CreateReportExportTaskRequest request, IReportingService service, CancellationToken ct) =>
        {
            var result = await service.CreateExportTaskAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.ReportingExport));

        return group;
    }

    /// <summary>
    /// Policy。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
