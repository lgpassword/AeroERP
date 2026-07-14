using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Finance Invoice 业务对象。
/// </summary>
public sealed class FinanceInvoice : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Finance Invoice实例。
    /// </summary>
    private FinanceInvoice()
    {
    }

    /// <summary>
    /// 初始化Finance Invoice实例。
    /// </summary>
    /// <param name="invoiceNo">invoice No 参数。</param>
    /// <param name="direction">业务方向。</param>
    /// <param name="sourceId">来源单据标识。</param>
    /// <param name="sourceNo">来源单据编号。</param>
    /// <param name="counterpartyName">counterparty Name 参数。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxRate">税率。</param>
    /// <param name="grossAmount">gross Amount 参数。</param>
    /// <param name="netAmount">未税金额。</param>
    /// <param name="taxAmount">税额。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="invoiceDate">invoice Date 参数。</param>
    /// <param name="note">备注。</param>
    /// <param name="createdBy">创建人。</param>
    public FinanceInvoice(
        string invoiceNo,
        string direction,
        Guid sourceId,
        string sourceNo,
        string counterpartyName,
        string taxInvoiceType,
        decimal taxRate,
        decimal grossAmount,
        decimal netAmount,
        decimal taxAmount,
        string currencyCode,
        DateOnly invoiceDate,
        string note,
        string createdBy)
    {
        if (grossAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(grossAmount), "发票金额必须大于 0。");
        }

        InvoiceNo = invoiceNo;
        Direction = direction;
        SourceId = sourceId;
        SourceNo = sourceNo;
        CounterpartyName = counterpartyName;
        TaxInvoiceType = taxInvoiceType;
        TaxRate = taxRate;
        GrossAmount = grossAmount;
        NetAmount = netAmount;
        TaxAmount = taxAmount;
        CurrencyCode = currencyCode;
        InvoiceDate = invoiceDate;
        Note = note;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Invoice No。
    /// </summary>
    public string InvoiceNo { get; private set; } = string.Empty;
    /// <summary>
    /// Direction。
    /// </summary>
    public string Direction { get; private set; } = string.Empty;
    /// <summary>
    /// 来源单据标识。
    /// </summary>
    public Guid SourceId { get; private set; }
    /// <summary>
    /// 来源单据编号。
    /// </summary>
    public string SourceNo { get; private set; } = string.Empty;
    /// <summary>
    /// Counterparty Name。
    /// </summary>
    public string CounterpartyName { get; private set; } = string.Empty;
    /// <summary>
    /// Tax Invoice Type。
    /// </summary>
    public string TaxInvoiceType { get; private set; } = string.Empty;
    /// <summary>
    /// 税率。
    /// </summary>
    public decimal TaxRate { get; private set; }
    /// <summary>
    /// Gross Amount。
    /// </summary>
    public decimal GrossAmount { get; private set; }
    /// <summary>
    /// 未税金额。
    /// </summary>
    public decimal NetAmount { get; private set; }
    /// <summary>
    /// 税额。
    /// </summary>
    public decimal TaxAmount { get; private set; }
    /// <summary>
    /// 币种编码。
    /// </summary>
    public string CurrencyCode { get; private set; } = "CNY";
    /// <summary>
    /// Invoice Date。
    /// </summary>
    public DateOnly InvoiceDate { get; private set; }
    /// <summary>
    /// 备注。
    /// </summary>
    public string Note { get; private set; } = string.Empty;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
}
