using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Payable 业务对象。
/// </summary>
public sealed class Payable : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Payable实例。
    /// </summary>
    private Payable()
    {
    }

    /// <summary>
    /// 初始化Payable实例。
    /// </summary>
    /// <param name="payableNo">payable No 参数。</param>
    /// <param name="procurementOrderId">procurement Order Id 参数。</param>
    /// <param name="procurementOrderNo">procurement Order No 参数。</param>
    /// <param name="inventoryReceiptId">inventory Receipt Id 参数。</param>
    /// <param name="inventoryReceiptNo">inventory Receipt No 参数。</param>
    /// <param name="supplierName">supplier Name 参数。</param>
    /// <param name="amount">金额。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="dueDate">due Date 参数。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxRate">税率。</param>
    /// <param name="sourceType">来源单据类型。</param>
    /// <param name="createdBy">创建人。</param>
    public Payable(
        string payableNo,
        Guid procurementOrderId,
        string procurementOrderNo,
        Guid? inventoryReceiptId,
        string inventoryReceiptNo,
        string supplierName,
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
            throw new ArgumentOutOfRangeException(nameof(amount), "应付金额必须大于 0。");
        }

        PayableNo = payableNo;
        ProcurementOrderId = procurementOrderId;
        ProcurementOrderNo = procurementOrderNo;
        InventoryReceiptId = inventoryReceiptId;
        InventoryReceiptNo = inventoryReceiptNo;
        SupplierName = supplierName;
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
    /// Payable No。
    /// </summary>
    public string PayableNo { get; private set; } = string.Empty;
    /// <summary>
    /// Procurement Order Id。
    /// </summary>
    public Guid ProcurementOrderId { get; private set; }
    /// <summary>
    /// Procurement Order No。
    /// </summary>
    public string ProcurementOrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Inventory Receipt Id。
    /// </summary>
    public Guid? InventoryReceiptId { get; private set; }
    /// <summary>
    /// Inventory Receipt No。
    /// </summary>
    public string InventoryReceiptNo { get; private set; } = string.Empty;
    /// <summary>
    /// Supplier Name。
    /// </summary>
    public string SupplierName { get; private set; } = string.Empty;
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
    public string SourceType { get; private set; } = FinanceSourceType.InventoryReceipt;
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
