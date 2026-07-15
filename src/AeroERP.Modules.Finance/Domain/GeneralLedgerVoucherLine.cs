using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// General Ledger Voucher 明细行实体。
/// </summary>
public sealed class GeneralLedgerVoucherLine : Entity
{
    /// <summary>
    /// 初始化General Ledger Voucher Line实例。
    /// </summary>
    private GeneralLedgerVoucherLine()
    {
    }

    /// <summary>
    /// 初始化General Ledger Voucher Line实例。
    /// </summary>
    /// <param name="accountingAccountId">accounting Account Id 参数。</param>
    /// <param name="accountCode">account Code 参数。</param>
    /// <param name="accountName">account Name 参数。</param>
    /// <param name="summary">摘要。</param>
    /// <param name="debitAmount">debit Amount 参数。</param>
    /// <param name="creditAmount">credit Amount 参数。</param>
    public GeneralLedgerVoucherLine(
        Guid accountingAccountId,
        string accountCode,
        string accountName,
        string summary,
        decimal debitAmount,
        decimal creditAmount)
    {
        AccountingAccountId = accountingAccountId;
        AccountCode = accountCode;
        AccountName = accountName;
        Summary = summary;
        DebitAmount = debitAmount;
        CreditAmount = creditAmount;
    }

    /// <summary>
    /// General Ledger Voucher Id。
    /// </summary>
    public Guid GeneralLedgerVoucherId { get; private set; }
    /// <summary>
    /// Accounting Account Id。
    /// </summary>
    public Guid AccountingAccountId { get; private set; }
    /// <summary>
    /// Account Code。
    /// </summary>
    public string AccountCode { get; private set; } = string.Empty;
    /// <summary>
    /// Account Name。
    /// </summary>
    public string AccountName { get; private set; } = string.Empty;
    /// <summary>
    /// Summary。
    /// </summary>
    public string Summary { get; private set; } = string.Empty;
    /// <summary>
    /// Debit Amount。
    /// </summary>
    public decimal DebitAmount { get; private set; }
    /// <summary>
    /// Credit Amount。
    /// </summary>
    public decimal CreditAmount { get; private set; }
}
