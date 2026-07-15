using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Receivable 业务对象。
/// </summary>
public sealed class Receivable : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Receivable实例。
    /// </summary>
    private Receivable()
    {
    }

    /// <summary>
    /// 初始化Receivable实例。
    /// </summary>
    /// <param name="receivableNo">receivable No 参数。</param>
    /// <param name="salesOrderId">sales Order Id 参数。</param>
    /// <param name="salesOrderNo">sales Order No 参数。</param>
    /// <param name="inventoryIssueId">inventory Issue Id 参数。</param>
    /// <param name="inventoryIssueNo">inventory Issue No 参数。</param>
    /// <param name="customerName">customer Name 参数。</param>
    /// <param name="amount">金额。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="dueDate">due Date 参数。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxRate">税率。</param>
    /// <param name="sourceType">来源单据类型。</param>
    /// <param name="createdBy">创建人。</param>
    public Receivable(
        string receivableNo,
        Guid salesOrderId,
        string salesOrderNo,
        Guid? inventoryIssueId,
        string inventoryIssueNo,
        string customerName,
        decimal amount,
        string currencyCode,
        DateOnly dueDate,
        string taxInvoiceType,
        decimal taxRate,
        string sourceType,
        string createdBy)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "应收金额必须大于 0。");
        }

        ReceivableNo = receivableNo;
        SalesOrderId = salesOrderId;
        SalesOrderNo = salesOrderNo;
        InventoryIssueId = inventoryIssueId;
        InventoryIssueNo = inventoryIssueNo;
        CustomerName = customerName;
        Amount = amount;
        CurrencyCode = currencyCode;
        DueDate = dueDate;
        TaxInvoiceType = taxInvoiceType;
        TaxRate = taxRate;
        NetAmount = CalculateNetAmount(amount, taxRate);
        TaxAmount = amount - NetAmount;
        SourceType = sourceType;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Receivable No。
    /// </summary>
    public string ReceivableNo { get; private set; } = string.Empty;
    /// <summary>
    /// Sales Order Id。
    /// </summary>
    public Guid SalesOrderId { get; private set; }
    /// <summary>
    /// Sales Order No。
    /// </summary>
    public string SalesOrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Inventory Issue Id。
    /// </summary>
    public Guid? InventoryIssueId { get; private set; }
    /// <summary>
    /// Inventory Issue No。
    /// </summary>
    public string InventoryIssueNo { get; private set; } = string.Empty;
    /// <summary>
    /// Customer Name。
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;
    /// <summary>
    /// 金额。
    /// </summary>
    public decimal Amount { get; private set; }
    /// <summary>
    /// Settled Amount。
    /// </summary>
    public decimal SettledAmount { get; private set; }
    /// <summary>
    /// 币种编码。
    /// </summary>
    public string CurrencyCode { get; private set; } = "CNY";
    /// <summary>
    /// Due Date。
    /// </summary>
    public DateOnly? DueDate { get; private set; }
    /// <summary>
    /// Tax Invoice Type。
    /// </summary>
    public string TaxInvoiceType { get; private set; } = "增值税普通发票";
    /// <summary>
    /// 税率。
    /// </summary>
    public decimal TaxRate { get; private set; } = 0.13m;
    /// <summary>
    /// 未税金额。
    /// </summary>
    public decimal NetAmount { get; private set; }
    /// <summary>
    /// 税额。
    /// </summary>
    public decimal TaxAmount { get; private set; }
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = FinanceRecordStatus.Open;
    /// <summary>
    /// 来源单据类型。
    /// </summary>
    public string SourceType { get; private set; } = FinanceSourceType.InventoryIssue;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    public decimal RemainingAmount => Amount - SettledAmount;

    /// <summary>
    /// Settle。
    /// </summary>
    /// <param name="amount">金额。</param>
    public void Settle(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("结算金额必须大于 0。");
        }

        if (amount > RemainingAmount)
        {
            throw new InvalidOperationException("结算金额不能大于未结金额。");
        }

        SettledAmount += amount;
        Status = RemainingAmount == 0 ? FinanceRecordStatus.Settled : FinanceRecordStatus.Partial;
        Touch();
    }

    /// <summary>
    /// Calculate Net Amount。
    /// </summary>
    /// <param name="grossAmount">gross Amount 参数。</param>
    /// <param name="taxRate">税率。</param>
    private static decimal CalculateNetAmount(decimal grossAmount, decimal taxRate) =>
        taxRate <= 0 ? grossAmount : Math.Round(grossAmount / (1 + taxRate), 2, MidpointRounding.AwayFromZero);
}
