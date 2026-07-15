using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Settlement 业务对象。
/// </summary>
public sealed class Settlement : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Settlement实例。
    /// </summary>
    private Settlement()
    {
    }

    /// <summary>
    /// 初始化Settlement实例。
    /// </summary>
    /// <param name="settlementNo">settlement No 参数。</param>
    /// <param name="targetType">target Type 参数。</param>
    /// <param name="targetId">target Id 参数。</param>
    /// <param name="targetNo">target No 参数。</param>
    /// <param name="counterpartyName">counterparty Name 参数。</param>
    /// <param name="amount">金额。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="bankAccountId">bank Account Id 参数。</param>
    /// <param name="bankAccountNo">bank Account No 参数。</param>
    /// <param name="bankAccountName">bank Account Name 参数。</param>
    /// <param name="method">HTTP 方法或业务处理方式。</param>
    /// <param name="note">备注。</param>
    /// <param name="settledBy">settled By 参数。</param>
    public Settlement(
        string settlementNo,
        string targetType,
        Guid targetId,
        string targetNo,
        string counterpartyName,
        decimal amount,
        string currencyCode,
        Guid bankAccountId,
        string bankAccountNo,
        string bankAccountName,
        string method,
        string note,
        string settledBy)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "结算金额必须大于 0。");
        }

        SettlementNo = settlementNo;
        TargetType = targetType;
        TargetId = targetId;
        TargetNo = targetNo;
        CounterpartyName = counterpartyName;
        Amount = amount;
        CurrencyCode = currencyCode;
        BankAccountId = bankAccountId;
        BankAccountNo = bankAccountNo;
        BankAccountName = bankAccountName;
        Method = method;
        Note = note;
        SettledBy = settledBy;
    }

    /// <summary>
    /// Settlement No。
    /// </summary>
    public string SettlementNo { get; private set; } = string.Empty;
    /// <summary>
    /// Target Type。
    /// </summary>
    public string TargetType { get; private set; } = string.Empty;
    /// <summary>
    /// Target Id。
    /// </summary>
    public Guid TargetId { get; private set; }
    /// <summary>
    /// Target No。
    /// </summary>
    public string TargetNo { get; private set; } = string.Empty;
    /// <summary>
    /// Counterparty Name。
    /// </summary>
    public string CounterpartyName { get; private set; } = string.Empty;
    /// <summary>
    /// 金额。
    /// </summary>
    public decimal Amount { get; private set; }
    /// <summary>
    /// 币种编码。
    /// </summary>
    public string CurrencyCode { get; private set; } = "CNY";
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
    /// Method。
    /// </summary>
    public string Method { get; private set; } = string.Empty;
    /// <summary>
    /// 备注。
    /// </summary>
    public string Note { get; private set; } = string.Empty;
    /// <summary>
    /// Reconciliation Status。
    /// </summary>
    public string ReconciliationStatus { get; private set; } = BankReconciliationStatus.Unmatched;
    /// <summary>
    /// Bank Statement Line Id。
    /// </summary>
    public Guid? BankStatementLineId { get; private set; }
    /// <summary>
    /// Bank Statement No。
    /// </summary>
    public string BankStatementNo { get; private set; } = string.Empty;
    /// <summary>
    /// Reconciled By。
    /// </summary>
    public string ReconciledBy { get; private set; } = string.Empty;
    /// <summary>
    /// Reconciled At Utc。
    /// </summary>
    public DateTimeOffset? ReconciledAtUtc { get; private set; }
    /// <summary>
    /// Settled By。
    /// </summary>
    public string SettledBy { get; private set; } = string.Empty;

    /// <summary>
    /// Reconcile。
    /// </summary>
    /// <param name="bankStatementLineId">bank Statement Line Id 参数。</param>
    /// <param name="bankStatementNo">bank Statement No 参数。</param>
    /// <param name="reconciledBy">reconciled By 参数。</param>
    public void Reconcile(Guid bankStatementLineId, string bankStatementNo, string reconciledBy)
    {
        if (ReconciliationStatus == BankReconciliationStatus.Matched)
        {
            throw new InvalidOperationException("该结算记录已经完成对账。");
        }

        BankStatementLineId = bankStatementLineId;
        BankStatementNo = bankStatementNo;
        ReconciliationStatus = BankReconciliationStatus.Matched;
        ReconciledBy = reconciledBy;
        ReconciledAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
