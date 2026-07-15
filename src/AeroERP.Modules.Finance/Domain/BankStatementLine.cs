using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Bank Statement 明细行实体。
/// </summary>
public sealed class BankStatementLine : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Bank Statement Line实例。
    /// </summary>
    private BankStatementLine()
    {
    }

    /// <summary>
    /// 初始化Bank Statement Line实例。
    /// </summary>
    /// <param name="statementNo">statement No 参数。</param>
    /// <param name="bankAccountId">bank Account Id 参数。</param>
    /// <param name="bankAccountNo">bank Account No 参数。</param>
    /// <param name="bankAccountName">bank Account Name 参数。</param>
    /// <param name="transactionDate">transaction Date 参数。</param>
    /// <param name="direction">业务方向。</param>
    /// <param name="amount">金额。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="counterpartyName">counterparty Name 参数。</param>
    /// <param name="bankReferenceNo">bank Reference No 参数。</param>
    /// <param name="summary">摘要。</param>
    /// <param name="createdBy">创建人。</param>
    public BankStatementLine(
        string statementNo,
        Guid bankAccountId,
        string bankAccountNo,
        string bankAccountName,
        DateOnly transactionDate,
        string direction,
        decimal amount,
        string currencyCode,
        string counterpartyName,
        string bankReferenceNo,
        string summary,
        string createdBy)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "银行流水金额必须大于 0。");
        }

        StatementNo = statementNo;
        BankAccountId = bankAccountId;
        BankAccountNo = bankAccountNo;
        BankAccountName = bankAccountName;
        TransactionDate = transactionDate;
        Direction = direction;
        Amount = amount;
        CurrencyCode = currencyCode;
        CounterpartyName = counterpartyName;
        BankReferenceNo = bankReferenceNo;
        Summary = summary;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Statement No。
    /// </summary>
    public string StatementNo { get; private set; } = string.Empty;
    /// <summary>
    /// Bank Account Id。
    /// </summary>
    public Guid BankAccountId { get; private set; }
    /// <summary>
    /// Bank Account No。
    /// </summary>
    public string BankAccountNo { get; private set; } = string.Empty;
    /// <summary>
    /// Bank Account Name。
    /// </summary>
    public string BankAccountName { get; private set; } = string.Empty;
    /// <summary>
    /// Transaction Date。
    /// </summary>
    public DateOnly TransactionDate { get; private set; }
    /// <summary>
    /// Direction。
    /// </summary>
    public string Direction { get; private set; } = BankStatementDirection.Outflow;
    /// <summary>
    /// 金额。
    /// </summary>
    public decimal Amount { get; private set; }
    /// <summary>
    /// 币种编码。
    /// </summary>
    public string CurrencyCode { get; private set; } = "CNY";
    /// <summary>
    /// Counterparty Name。
    /// </summary>
    public string CounterpartyName { get; private set; } = string.Empty;
    /// <summary>
    /// Bank Reference No。
    /// </summary>
    public string BankReferenceNo { get; private set; } = string.Empty;
    /// <summary>
    /// Summary。
    /// </summary>
    public string Summary { get; private set; } = string.Empty;
    /// <summary>
    /// Reconciliation Status。
    /// </summary>
    public string ReconciliationStatus { get; private set; } = BankReconciliationStatus.Unmatched;
    /// <summary>
    /// Settlement Id。
    /// </summary>
    public Guid? SettlementId { get; private set; }
    /// <summary>
    /// Settlement No。
    /// </summary>
    public string SettlementNo { get; private set; } = string.Empty;
    /// <summary>
    /// Reconciled By。
    /// </summary>
    public string ReconciledBy { get; private set; } = string.Empty;
    /// <summary>
    /// Reconciled At Utc。
    /// </summary>
    public DateTimeOffset? ReconciledAtUtc { get; private set; }
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Reconcile。
    /// </summary>
    /// <param name="settlementId">settlement Id 参数。</param>
    /// <param name="settlementNo">settlement No 参数。</param>
    /// <param name="reconciledBy">reconciled By 参数。</param>
    public void Reconcile(Guid settlementId, string settlementNo, string reconciledBy)
    {
        if (ReconciliationStatus == BankReconciliationStatus.Matched)
        {
            throw new InvalidOperationException("该银行流水已经完成对账。");
        }

        SettlementId = settlementId;
        SettlementNo = settlementNo;
        ReconciliationStatus = BankReconciliationStatus.Matched;
        ReconciledBy = reconciledBy;
        ReconciledAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
