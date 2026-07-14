using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Finance.Contracts;

namespace AeroERP.Modules.Finance.Services;

/// <summary>
/// Finance Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IFinanceService
{
    /// <summary>
    /// 查询Accounting Accounts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<AccountingAccountDto>> ListAccountingAccountsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Accounting Account。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<AccountingAccountDto>> UpsertAccountingAccountAsync(UpsertAccountingAccountRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Accounting Periods。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<AccountingPeriodDto>> ListAccountingPeriodsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Accounting Period。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<AccountingPeriodDto>> CreateAccountingPeriodAsync(CreateAccountingPeriodRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Close Accounting Period。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<AccountingPeriodDto>> CloseAccountingPeriodAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Reopen Accounting Period。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<AccountingPeriodDto>> ReopenAccountingPeriodAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 查询General Ledger Vouchers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<GeneralLedgerVoucherDto>> ListGeneralLedgerVouchersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Manual Voucher。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<GeneralLedgerVoucherDto>> CreateManualVoucherAsync(CreateManualVoucherRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Business Voucher。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<GeneralLedgerVoucherDto>> CreateBusinessVoucherAsync(CreateBusinessVoucherRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 提交Voucher。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<GeneralLedgerVoucherDto>> SubmitVoucherAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 审批通过Voucher。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<GeneralLedgerVoucherDto>> ApproveVoucherAsync(Guid id, ReviewVoucherRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 驳回Voucher。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<GeneralLedgerVoucherDto>> RejectVoucherAsync(Guid id, ReviewVoucherRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 获取Finance Report Snapshot。
    /// </summary>
    /// <param name="accountingPeriodId">accounting Period Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<FinanceReportSnapshotDto>> GetFinanceReportSnapshotAsync(Guid? accountingPeriodId, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Payables。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<PayableDto>> ListPayablesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Receivables。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ReceivableDto>> ListReceivablesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 获取Aging Snapshot。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<FinanceAgingSnapshotDto> GetAgingSnapshotAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Finance Invoices。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<FinanceInvoiceDto>> ListFinanceInvoicesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Finance Invoice。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<FinanceInvoiceDto>> CreateFinanceInvoiceAsync(CreateFinanceInvoiceRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Bank Accounts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<BankAccountDto>> ListBankAccountsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Bank Account。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<BankAccountDto>> UpsertBankAccountAsync(UpsertBankAccountRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Bank Statement Lines。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<BankStatementLineDto>> ListBankStatementLinesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Bank Statement Line。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<BankStatementLineDto>> CreateBankStatementLineAsync(CreateBankStatementLineRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Reconcile Bank Statement。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<BankStatementLineDto>> ReconcileBankStatementAsync(ReconcileBankStatementRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Settlements。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<SettlementDto>> ListSettlementsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Payable From Receipt。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PayableDto>> CreatePayableFromReceiptAsync(CreatePayableFromReceiptRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Payable From Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PayableDto>> CreatePayableFromOrderAsync(CreatePayableFromOrderRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Receivable From Issue。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ReceivableDto>> CreateReceivableFromIssueAsync(CreateReceivableFromIssueRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Receivable From Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ReceivableDto>> CreateReceivableFromOrderAsync(CreateReceivableFromOrderRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Settlement。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<SettlementDto>> CreateSettlementAsync(CreateSettlementRequest request, CancellationToken cancellationToken);
}
