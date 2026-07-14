using AeroERP.Modules.Finance.Contracts;
using AeroERP.Modules.Finance.Services;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AeroERP.Modules.Finance.Endpoints;

/// <summary>
/// Finance 模块 HTTP API 路由映射。
/// </summary>
public static class FinanceEndpoints
{
    /// <summary>
    /// 注册Finance Endpoints 路由。
    /// </summary>
    /// <param name="app">端点路由构建器。</param>
    public static RouteGroupBuilder MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/finance").RequireAuthorization();

        group.MapGet("/accounting-accounts", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListAccountingAccountsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/accounting-accounts", async (UpsertAccountingAccountRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.UpsertAccountingAccountAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceAccountingManage });

        group.MapGet("/accounting-periods", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListAccountingPeriodsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/accounting-periods", async (CreateAccountingPeriodRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateAccountingPeriodAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/accounting-periods", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceAccountingManage });

        group.MapPost("/accounting-periods/{id:guid}/close", async (Guid id, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CloseAccountingPeriodAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceAccountingManage });

        group.MapPost("/accounting-periods/{id:guid}/reopen", async (Guid id, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.ReopenAccountingPeriodAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceAccountingManage });

        group.MapGet("/vouchers", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListGeneralLedgerVouchersAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/vouchers/manual", async (CreateManualVoucherRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateManualVoucherAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/vouchers", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceVoucherManage });

        group.MapPost("/vouchers/from-business-document", async (CreateBusinessVoucherRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateBusinessVoucherAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/vouchers", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceVoucherManage });

        group.MapPost("/vouchers/{id:guid}/submit", async (Guid id, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.SubmitVoucherAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceVoucherManage });

        group.MapPost("/vouchers/{id:guid}/approve", async (Guid id, ReviewVoucherRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.ApproveVoucherAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceVoucherReview });

        group.MapPost("/vouchers/{id:guid}/reject", async (Guid id, ReviewVoucherRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.RejectVoucherAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceVoucherReview });

        group.MapGet("/reports", async (Guid? accountingPeriodId, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.GetFinanceReportSnapshotAsync(accountingPeriodId, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapGet("/payables", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListPayablesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/payables/from-receipt", async (CreatePayableFromReceiptRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreatePayableFromReceiptAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/payables", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinancePayableManage });

        group.MapPost("/payables/from-order", async (CreatePayableFromOrderRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreatePayableFromOrderAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/payables", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinancePayableManage });

        group.MapGet("/receivables", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListReceivablesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapGet("/aging", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.GetAgingSnapshotAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapGet("/invoices", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListFinanceInvoicesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/invoices", async (CreateFinanceInvoiceRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateFinanceInvoiceAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/invoices", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceSettlementManage });

        group.MapGet("/bank-accounts", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListBankAccountsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/bank-accounts", async (UpsertBankAccountRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.UpsertBankAccountAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceSettlementManage });

        group.MapGet("/bank-statement-lines", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListBankStatementLinesAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/bank-statement-lines", async (CreateBankStatementLineRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateBankStatementLineAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/bank-statement-lines", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceSettlementManage });

        group.MapPost("/bank-statement-lines/reconcile", async (ReconcileBankStatementRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.ReconcileBankStatementAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceSettlementManage });

        group.MapPost("/receivables/from-issue", async (CreateReceivableFromIssueRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateReceivableFromIssueAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/receivables", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceReceivableManage });

        group.MapPost("/receivables/from-order", async (CreateReceivableFromOrderRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateReceivableFromOrderAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/receivables", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceReceivableManage });

        group.MapGet("/settlements", async (IFinanceService service, CancellationToken ct) =>
            Results.Ok(await service.ListSettlementsAsync(ct)))
            .RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceRead });

        group.MapPost("/settlements", async (CreateSettlementRequest request, IFinanceService service, CancellationToken ct) =>
        {
            var result = await service.CreateSettlementAsync(request, ct);
            return result.IsSuccess
                ? Results.Created("/api/finance/settlements", result.Value)
                : Results.BadRequest(new { message = result.Error });
        }).RequireAuthorization(new AuthorizeAttribute { Policy = PlatformPermissions.FinanceSettlementManage });

        return group;
    }
}
