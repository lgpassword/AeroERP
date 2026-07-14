using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Sales.Domain;

/// <summary>
/// Sales Quotation 业务对象。
/// </summary>
public sealed class SalesQuotation : Entity, IAggregateRoot
{
    /// <summary>
    /// _lines。
    /// </summary>
    private readonly List<SalesLine> _lines = [];

    /// <summary>
    /// 初始化Sales Quotation实例。
    /// </summary>
    private SalesQuotation()
    {
    }

    /// <summary>
    /// 初始化Sales Quotation实例。
    /// </summary>
    /// <param name="quotationNo">quotation No 参数。</param>
    /// <param name="customerId">客户标识。</param>
    /// <param name="customerName">customer Name 参数。</param>
    /// <param name="title">标题。</param>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="organizationName">所属组织名称。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxRate">税率。</param>
    /// <param name="createdBy">创建人。</param>
    /// <param name="lines">明细行集合。</param>
    public SalesQuotation(
        string quotationNo,
        Guid customerId,
        string customerName,
        string title,
        Guid? organizationId,
        string organizationName,
        string currencyCode,
        string taxInvoiceType,
        decimal taxRate,
        string createdBy,
        IEnumerable<SalesLine> lines)
    {
        QuotationNo = quotationNo;
        CustomerId = customerId;
        CustomerName = customerName;
        Title = title;
        OrganizationId = organizationId;
        OrganizationName = organizationName;
        CurrencyCode = currencyCode;
        TaxInvoiceType = taxInvoiceType;
        TaxRate = taxRate;
        CreatedBy = createdBy;
        _lines = lines.ToList();
    }

    /// <summary>
    /// Quotation No。
    /// </summary>
    public string QuotationNo { get; private set; } = string.Empty;
    /// <summary>
    /// Customer Id。
    /// </summary>
    public Guid CustomerId { get; private set; }
    /// <summary>
    /// Customer Name。
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;
    /// <summary>
    /// Title。
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = SalesQuotationStatus.Created;
    /// <summary>
    /// 所属组织标识。
    /// </summary>
    public Guid? OrganizationId { get; private set; }
    /// <summary>
    /// 所属组织名称。
    /// </summary>
    public string OrganizationName { get; private set; } = string.Empty;
    /// <summary>
    /// 币种编码。
    /// </summary>
    public string CurrencyCode { get; private set; } = "CNY";
    /// <summary>
    /// Tax Invoice Type。
    /// </summary>
    public string TaxInvoiceType { get; private set; } = "增值税普通发票";
    /// <summary>
    /// 税率。
    /// </summary>
    public decimal TaxRate { get; private set; } = 0.13m;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Sales Order Id。
    /// </summary>
    public Guid? SalesOrderId { get; private set; }
    public List<SalesLine> Lines => _lines;

    /// <summary>
    /// Link Order。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    public void LinkOrder(Guid orderId)
    {
        SalesOrderId = orderId;
        Status = SalesQuotationStatus.Converted;
        Touch();
    }
}
