namespace AeroERP.Modules.Finance.Contracts;

/// <summary>
/// Accounting Account 数据传输对象。
/// </summary>
public sealed record AccountingAccountDto(
    Guid Id,
    string Code,
    string Name,
    string Type,
    Guid? ParentAccountId,
    string ParentAccountCode,
    string ParentAccountName,
    bool IsActive,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Accounting Period 数据传输对象。
/// </summary>
public sealed record AccountingPeriodDto(
    Guid Id,
    int Year,
    int Month,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string CreatedBy,
    string ClosedBy,
    DateTimeOffset? ClosedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// General Ledger Voucher Line 数据传输对象。
/// </summary>
public sealed record GeneralLedgerVoucherLineDto(
    Guid Id,
    Guid AccountingAccountId,
    string AccountCode,
    string AccountName,
    string Summary,
    decimal DebitAmount,
    decimal CreditAmount);

/// <summary>
/// General Ledger Voucher 数据传输对象。
/// </summary>
public sealed record GeneralLedgerVoucherDto(
    Guid Id,
    string VoucherNo,
    Guid AccountingPeriodId,
    string AccountingPeriodName,
    DateOnly DocumentDate,
    string Summary,
    string SourceType,
    Guid? SourceId,
    string SourceNo,
    string Status,
    decimal TotalDebit,
    decimal TotalCredit,
    string CreatedBy,
    string SubmittedBy,
    DateTimeOffset? SubmittedAtUtc,
    string ReviewedBy,
    DateTimeOffset? ReviewedAtUtc,
    string ReviewNote,
    IReadOnlyList<GeneralLedgerVoucherLineDto> Lines,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Trial Balance Line 数据传输对象。
/// </summary>
public sealed record TrialBalanceLineDto(
    Guid AccountingAccountId,
    string AccountCode,
    string AccountName,
    string AccountType,
    decimal DebitAmount,
    decimal CreditAmount,
    decimal EndingDebit,
    decimal EndingCredit);

/// <summary>
/// Income Statement 数据传输对象。
/// </summary>
public sealed record IncomeStatementDto(
    decimal Revenue,
    decimal Cost,
    decimal Expense,
    decimal Profit);

/// <summary>
/// Balance Sheet 数据传输对象。
/// </summary>
public sealed record BalanceSheetDto(
    decimal Assets,
    decimal Liabilities,
    decimal Equity,
    decimal RetainedEarnings,
    decimal TotalLiabilitiesAndEquity,
    decimal Difference);

/// <summary>
/// Finance Report Snapshot 数据传输对象。
/// </summary>
public sealed record FinanceReportSnapshotDto(
    Guid? AccountingPeriodId,
    string AccountingPeriodName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int ApprovedVoucherCount,
    decimal TotalDebit,
    decimal TotalCredit,
    bool IsBalanced,
    IReadOnlyList<TrialBalanceLineDto> TrialBalance,
    IncomeStatementDto IncomeStatement,
    BalanceSheetDto BalanceSheet);

/// <summary>
/// Payable 数据传输对象。
/// </summary>
public sealed record PayableDto(
    Guid Id,
    string PayableNo,
    Guid ProcurementOrderId,
    string ProcurementOrderNo,
    Guid? InventoryReceiptId,
    string InventoryReceiptNo,
    string SupplierName,
    decimal Amount,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TaxRate,
    string TaxInvoiceType,
    decimal SettledAmount,
    decimal RemainingAmount,
    string CurrencyCode,
    DateOnly DueDate,
    int OverdueDays,
    string Status,
    string SourceType,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Receivable 数据传输对象。
/// </summary>
public sealed record ReceivableDto(
    Guid Id,
    string ReceivableNo,
    Guid SalesOrderId,
    string SalesOrderNo,
    Guid? InventoryIssueId,
    string InventoryIssueNo,
    string CustomerName,
    decimal Amount,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TaxRate,
    string TaxInvoiceType,
    decimal SettledAmount,
    decimal RemainingAmount,
    string CurrencyCode,
    DateOnly DueDate,
    int OverdueDays,
    string Status,
    string SourceType,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Aging Bucket 数据传输对象。
/// </summary>
public sealed record AgingBucketDto(
    string Bucket,
    string BucketName,
    int Count,
    decimal Amount);

/// <summary>
/// Aging Entry 数据传输对象。
/// </summary>
public sealed record AgingEntryDto(
    Guid Id,
    string DocumentNo,
    string CounterpartyName,
    string SourceNo,
    decimal Amount,
    decimal SettledAmount,
    decimal RemainingAmount,
    string CurrencyCode,
    DateOnly DueDate,
    int OverdueDays,
    string Bucket,
    string Status);

/// <summary>
/// Aging Side 数据传输对象。
/// </summary>
public sealed record AgingSideDto(
    decimal TotalOpenAmount,
    decimal TotalOverdueAmount,
    int OpenCount,
    int OverdueCount,
    IReadOnlyList<AgingBucketDto> Buckets,
    IReadOnlyList<AgingEntryDto> Entries);

/// <summary>
/// Finance Aging Snapshot 数据传输对象。
/// </summary>
public sealed record FinanceAgingSnapshotDto(
    DateOnly AsOfDate,
    AgingSideDto Payables,
    AgingSideDto Receivables);

/// <summary>
/// Finance Invoice 数据传输对象。
/// </summary>
public sealed record FinanceInvoiceDto(
    Guid Id,
    string InvoiceNo,
    string Direction,
    Guid SourceId,
    string SourceNo,
    string CounterpartyName,
    string TaxInvoiceType,
    decimal TaxRate,
    decimal GrossAmount,
    decimal NetAmount,
    decimal TaxAmount,
    string CurrencyCode,
    DateOnly InvoiceDate,
    string Note,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Bank Account 数据传输对象。
/// </summary>
public sealed record BankAccountDto(
    Guid Id,
    string AccountNo,
    string AccountName,
    string BankName,
    string CurrencyCode,
    bool IsEnabled,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Bank Statement Line 数据传输对象。
/// </summary>
public sealed record BankStatementLineDto(
    Guid Id,
    string StatementNo,
    Guid BankAccountId,
    string BankAccountNo,
    string BankAccountName,
    DateOnly TransactionDate,
    string Direction,
    decimal Amount,
    string CurrencyCode,
    string CounterpartyName,
    string BankReferenceNo,
    string Summary,
    string ReconciliationStatus,
    Guid? SettlementId,
    string SettlementNo,
    string ReconciledBy,
    DateTimeOffset? ReconciledAtUtc,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Settlement 数据传输对象。
/// </summary>
public sealed record SettlementDto(
    Guid Id,
    string SettlementNo,
    string TargetType,
    Guid TargetId,
    string TargetNo,
    string CounterpartyName,
    decimal Amount,
    string CurrencyCode,
    Guid BankAccountId,
    string BankAccountNo,
    string BankAccountName,
    string Method,
    string Note,
    string ReconciliationStatus,
    Guid? BankStatementLineId,
    string BankStatementNo,
    string ReconciledBy,
    DateTimeOffset? ReconciledAtUtc,
    string SettledBy,
    DateTimeOffset SettledAtUtc);

/// <summary>
/// Create Payable From Receipt 请求参数。
/// </summary>
/// <param name="InventoryReceiptId">Inventory Receipt Id 参数。</param>
/// <param name="Amount">金额。</param>
public sealed record CreatePayableFromReceiptRequest(Guid InventoryReceiptId, decimal Amount);

/// <summary>
/// Create Payable From Order 请求参数。
/// </summary>
/// <param name="ProcurementOrderId">Procurement Order Id 参数。</param>
/// <param name="Amount">金额。</param>
public sealed record CreatePayableFromOrderRequest(Guid ProcurementOrderId, decimal Amount);

/// <summary>
/// Create Receivable From Issue 请求参数。
/// </summary>
/// <param name="InventoryIssueId">Inventory Issue Id 参数。</param>
/// <param name="Amount">金额。</param>
public sealed record CreateReceivableFromIssueRequest(Guid InventoryIssueId, decimal Amount);

/// <summary>
/// Create Receivable From Order 请求参数。
/// </summary>
/// <param name="SalesOrderId">Sales Order Id 参数。</param>
/// <param name="Amount">金额。</param>
public sealed record CreateReceivableFromOrderRequest(Guid SalesOrderId, decimal Amount);

/// <summary>
/// Create Settlement 请求参数。
/// </summary>
/// <param name="TargetType">Target Type 参数。</param>
/// <param name="TargetId">Target Id 参数。</param>
/// <param name="Amount">金额。</param>
/// <param name="BankAccountId">Bank Account Id 参数。</param>
/// <param name="Method">HTTP 方法或业务处理方式。</param>
/// <param name="Note">备注。</param>
public sealed record CreateSettlementRequest(string TargetType, Guid TargetId, decimal Amount, Guid BankAccountId, string Method, string Note);

/// <summary>
/// Create Finance Invoice 请求参数。
/// </summary>
public sealed record CreateFinanceInvoiceRequest(
    string Direction,
    Guid SourceId,
    DateOnly InvoiceDate,
    string Note);

/// <summary>
/// Upsert Bank Account 请求参数。
/// </summary>
public sealed record UpsertBankAccountRequest(
    Guid? Id,
    string AccountNo,
    string AccountName,
    string BankName,
    string CurrencyCode,
    bool IsEnabled);

/// <summary>
/// Create Bank Statement Line 请求参数。
/// </summary>
public sealed record CreateBankStatementLineRequest(
    Guid BankAccountId,
    DateOnly TransactionDate,
    string Direction,
    decimal Amount,
    string CounterpartyName,
    string BankReferenceNo,
    string Summary);

/// <summary>
/// Reconcile Bank Statement 请求参数。
/// </summary>
/// <param name="BankStatementLineId">Bank Statement Line Id 参数。</param>
/// <param name="SettlementId">Settlement Id 参数。</param>
public sealed record ReconcileBankStatementRequest(Guid BankStatementLineId, Guid SettlementId);

/// <summary>
/// Upsert Accounting Account 请求参数。
/// </summary>
public sealed record UpsertAccountingAccountRequest(
    Guid? Id,
    string Code,
    string Name,
    string Type,
    Guid? ParentAccountId,
    bool IsActive);

/// <summary>
/// Create Accounting Period 请求参数。
/// </summary>
/// <param name="Year">会计年度。</param>
/// <param name="Month">会计月份。</param>
public sealed record CreateAccountingPeriodRequest(int Year, int Month);

/// <summary>
/// Create Manual Voucher Line 请求参数。
/// </summary>
public sealed record CreateManualVoucherLineRequest(
    Guid AccountingAccountId,
    string Summary,
    decimal DebitAmount,
    decimal CreditAmount);

/// <summary>
/// Create Manual Voucher 请求参数。
/// </summary>
public sealed record CreateManualVoucherRequest(
    Guid AccountingPeriodId,
    DateOnly DocumentDate,
    string Summary,
    IReadOnlyList<CreateManualVoucherLineRequest> Lines);

/// <summary>
/// Create Business Voucher 请求参数。
/// </summary>
public sealed record CreateBusinessVoucherRequest(
    Guid AccountingPeriodId,
    DateOnly DocumentDate,
    string SourceType,
    Guid SourceId,
    Guid DebitAccountId,
    Guid CreditAccountId,
    string Summary);

/// <summary>
/// Review Voucher 请求参数。
/// </summary>
/// <param name="Note">备注。</param>
public sealed record ReviewVoucherRequest(string Note);
