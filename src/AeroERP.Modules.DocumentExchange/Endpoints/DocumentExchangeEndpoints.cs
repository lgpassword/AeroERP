using AeroERP.Modules.DocumentExchange.Contracts;
using AeroERP.Modules.DocumentExchange.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.DocumentExchange.Endpoints;

/// <summary>
/// Document Exchange 模块 HTTP API 路由映射。
/// </summary>
public static class DocumentExchangeEndpoints
{
    /// <summary>
    /// 注册Document Exchange Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapDocumentExchangeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/document-exchange").RequireAuthorization();

        group.MapGet("/overview", async (IDocumentExchangeService service, CancellationToken ct) =>
            Results.Ok(await service.GetOverviewAsync(ct)))
            .RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeRead));

        group.MapPost("/import-templates", async (UpsertImportTemplateRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.UpsertImportTemplateAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeManage));

        group.MapPost("/field-mappings", async (UpsertImportFieldMappingRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.UpsertFieldMappingAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeManage));

        group.MapPost("/import-batches", async (CreateImportBatchRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.CreateImportBatchAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeManage));

        group.MapPost("/import-batches/{id:guid}/complete", async (Guid id, CompleteImportBatchRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.CompleteImportBatchAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeExecute));

        group.MapPost("/import-batches/{id:guid}/fail", async (Guid id, FailFileTaskRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.FailImportBatchAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeExecute));

        group.MapPost("/export-tasks", async (CreateExportFileTaskRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.CreateExportTaskAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeManage));

        group.MapPost("/export-tasks/{id:guid}/complete", async (Guid id, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.CompleteExportTaskAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeExecute));

        group.MapPost("/export-tasks/{id:guid}/fail", async (Guid id, FailFileTaskRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.FailExportTaskAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeExecute));

        group.MapPost("/print-templates", async (UpsertPrintTemplateRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.UpsertPrintTemplateAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeManage));

        group.MapPost("/print-jobs", async (CreatePrintJobRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.CreatePrintJobAsync(request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeManage));

        group.MapPost("/print-jobs/{id:guid}/complete", async (Guid id, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.CompletePrintJobAsync(id, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeExecute));

        group.MapPost("/print-jobs/{id:guid}/fail", async (Guid id, FailFileTaskRequest request, IDocumentExchangeService service, CancellationToken ct) =>
        {
            var result = await service.FailPrintJobAsync(id, request, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(Policy(PlatformPermissions.DocumentExchangeExecute));

        return group;
    }

    /// <summary>
    /// Policy。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    private static AuthorizeAttribute Policy(string permission) => new() { Policy = permission };
}
