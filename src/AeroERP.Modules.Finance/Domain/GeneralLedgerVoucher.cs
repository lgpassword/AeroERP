using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// General Ledger Voucher 业务对象。
/// </summary>
public sealed class GeneralLedgerVoucher : Entity, IAggregateRoot
{
    /// <summary>
    /// _lines。
    /// </summary>
    private readonly List<GeneralLedgerVoucherLine> _lines = [];

    /// <summary>
    /// 初始化General Ledger Voucher实例。
    /// </summary>
    private GeneralLedgerVoucher()
    {
    }

    /// <summary>
    /// 初始化General Ledger Voucher实例。
    /// </summary>
    /// <param name="voucherNo">voucher No 参数。</param>
    /// <param name="accountingPeriodId">accounting Period Id 参数。</param>
    /// <param name="accountingPeriodName">accounting Period Name 参数。</param>
    /// <param name="documentDate">document Date 参数。</param>
    /// <param name="summary">摘要。</param>
    /// <param name="sourceType">来源单据类型。</param>
    /// <param name="sourceId">来源单据标识。</param>
    /// <param name="sourceNo">来源单据编号。</param>
    /// <param name="createdBy">创建人。</param>
    /// <param name="lines">明细行集合。</param>
    public GeneralLedgerVoucher(
        string voucherNo,
        Guid accountingPeriodId,
        string accountingPeriodName,
        DateOnly documentDate,
        string summary,
        string sourceType,
        Guid? sourceId,
        string sourceNo,
        string createdBy,
        IEnumerable<GeneralLedgerVoucherLine> lines)
    {
        VoucherNo = voucherNo;
        AccountingPeriodId = accountingPeriodId;
        AccountingPeriodName = accountingPeriodName;
        DocumentDate = documentDate;
        Summary = summary;
        SourceType = sourceType;
        SourceId = sourceId;
        SourceNo = sourceNo;
        CreatedBy = createdBy;
        _lines = lines.ToList();
    }

    /// <summary>
    /// Voucher No。
    /// </summary>
    public string VoucherNo { get; private set; } = string.Empty;
    /// <summary>
    /// Accounting Period Id。
    /// </summary>
    public Guid AccountingPeriodId { get; private set; }
    /// <summary>
    /// Accounting Period Name。
    /// </summary>
    public string AccountingPeriodName { get; private set; } = string.Empty;
    /// <summary>
    /// Document Date。
    /// </summary>
    public DateOnly DocumentDate { get; private set; }
    /// <summary>
    /// Summary。
    /// </summary>
    public string Summary { get; private set; } = string.Empty;
    /// <summary>
    /// 来源单据类型。
    /// </summary>
    public string SourceType { get; private set; } = GeneralLedgerVoucherSourceType.Manual;
    /// <summary>
    /// 来源单据标识。
    /// </summary>
    public Guid? SourceId { get; private set; }
    /// <summary>
    /// 来源单据编号。
    /// </summary>
    public string SourceNo { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = GeneralLedgerVoucherStatus.Draft;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Submitted By。
    /// </summary>
    public string SubmittedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Submitted At Utc。
    /// </summary>
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    /// <summary>
    /// Reviewed By。
    /// </summary>
    public string ReviewedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Reviewed At Utc。
    /// </summary>
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    /// <summary>
    /// Review Note。
    /// </summary>
    public string ReviewNote { get; private set; } = string.Empty;
    public IReadOnlyCollection<GeneralLedgerVoucherLine> Lines => _lines;
    public decimal TotalDebit => _lines.Sum(x => x.DebitAmount);
    public decimal TotalCredit => _lines.Sum(x => x.CreditAmount);

    /// <summary>
    /// Submit。
    /// </summary>
    /// <param name="actor">操作人。</param>
    public void Submit(string actor)
    {
        if (!string.Equals(Status, GeneralLedgerVoucherStatus.Draft, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有草稿凭证可以提交。");
        }

        Status = GeneralLedgerVoucherStatus.Submitted;
        SubmittedBy = actor;
        SubmittedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Approve。
    /// </summary>
    /// <param name="actor">操作人。</param>
    /// <param name="note">备注。</param>
    public void Approve(string actor, string note)
    {
        if (!string.Equals(Status, GeneralLedgerVoucherStatus.Submitted, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有已提交凭证可以审核通过。");
        }

        Status = GeneralLedgerVoucherStatus.Approved;
        ReviewedBy = actor;
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        ReviewNote = note;
        Touch();
    }

    /// <summary>
    /// Reject。
    /// </summary>
    /// <param name="actor">操作人。</param>
    /// <param name="note">备注。</param>
    public void Reject(string actor, string note)
    {
        if (!string.Equals(Status, GeneralLedgerVoucherStatus.Submitted, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有已提交凭证可以驳回。");
        }

        Status = GeneralLedgerVoucherStatus.Rejected;
        ReviewedBy = actor;
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        ReviewNote = note;
        Touch();
    }
}
