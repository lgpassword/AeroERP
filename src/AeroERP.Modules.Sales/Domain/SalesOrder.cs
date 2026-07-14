using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Sales.Domain;

/// <summary>
/// Sales Order 业务对象。
/// </summary>
public sealed class SalesOrder : Entity, IAggregateRoot
{
    /// <summary>
    /// _lines。
    /// </summary>
    private readonly List<SalesOrderLine> _lines = [];

    /// <summary>
    /// 初始化Sales Order实例。
    /// </summary>
    private SalesOrder()
    {
    }

    /// <summary>
    /// 初始化Sales Order实例。
    /// </summary>
    /// <param name="orderNo">order No 参数。</param>
    /// <param name="quotationId">quotation Id 参数。</param>
    /// <param name="quotationNo">quotation No 参数。</param>
    /// <param name="customerId">客户标识。</param>
    /// <param name="customerName">customer Name 参数。</param>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="organizationName">所属组织名称。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxRate">税率。</param>
    /// <param name="createdBy">创建人。</param>
    /// <param name="lines">明细行集合。</param>
    public SalesOrder(
        string orderNo,
        Guid quotationId,
        string quotationNo,
        Guid customerId,
        string customerName,
        Guid? organizationId,
        string organizationName,
        string currencyCode,
        string taxInvoiceType,
        decimal taxRate,
        string createdBy,
        IEnumerable<SalesOrderLine> lines)
    {
        OrderNo = orderNo;
        QuotationId = quotationId;
        QuotationNo = quotationNo;
        CustomerId = customerId;
        CustomerName = customerName;
        OrganizationId = organizationId;
        OrganizationName = organizationName;
        CurrencyCode = currencyCode;
        TaxInvoiceType = taxInvoiceType;
        TaxRate = taxRate;
        CreatedBy = createdBy;
        _lines = lines.ToList();
    }

    /// <summary>
    /// Order No。
    /// </summary>
    public string OrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Quotation Id。
    /// </summary>
    public Guid QuotationId { get; private set; }
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
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = SalesOrderStatus.Created;
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
    public List<SalesOrderLine> Lines => _lines;

    /// <summary>
    /// Confirm。
    /// </summary>
    public void Confirm()
    {
        if (!string.Equals(Status, SalesOrderStatus.Created, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有新建状态的销售订单才能确认。");
        }

        Status = SalesOrderStatus.Confirmed;
        Touch();
    }

    /// <summary>
    /// Mark Ready To Ship。
    /// </summary>
    public void MarkReadyToShip()
    {
        if (!string.Equals(Status, SalesOrderStatus.Confirmed, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有已确认的销售订单才能进入待出库。");
        }

        Status = SalesOrderStatus.ReadyToShip;
        Touch();
    }

    /// <summary>
    /// Ship。
    /// </summary>
    public void Ship()
    {
        if (!string.Equals(Status, SalesOrderStatus.ReadyToShip, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有待出库的销售订单才能完成出库。");
        }

        Status = SalesOrderStatus.Shipped;
        Touch();
    }
}
