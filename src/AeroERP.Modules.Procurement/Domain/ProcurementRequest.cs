using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Procurement.Domain;

/// <summary>
/// Procurement 请求参数。
/// </summary>
public sealed class ProcurementRequest : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Procurement Request实例。
    /// </summary>
    private ProcurementRequest()
    {
    }

    /// <summary>
    /// 初始化Procurement Request实例。
    /// </summary>
    /// <param name="requestNo">request No 参数。</param>
    /// <param name="supplierId">供应商标识。</param>
    /// <param name="supplierName">supplier Name 参数。</param>
    /// <param name="title">标题。</param>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="organizationName">所属组织名称。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxRate">税率。</param>
    /// <param name="lines">明细行集合。</param>
    public ProcurementRequest(
        string requestNo,
        Guid supplierId,
        string supplierName,
        string title,
        Guid? organizationId,
        string organizationName,
        string currencyCode,
        string taxInvoiceType,
        decimal taxRate,
        IEnumerable<ProcurementRequestLine> lines)
    {
        RequestNo = requestNo;
        SupplierId = supplierId;
        SupplierName = supplierName;
        Title = title;
        OrganizationId = organizationId;
        OrganizationName = organizationName;
        CurrencyCode = currencyCode;
        TaxInvoiceType = taxInvoiceType;
        TaxRate = taxRate;
        Lines = lines.ToList();
    }

    /// <summary>
    /// Request No。
    /// </summary>
    public string RequestNo { get; private set; } = string.Empty;
    /// <summary>
    /// Supplier Id。
    /// </summary>
    public Guid SupplierId { get; private set; }
    /// <summary>
    /// Supplier Name。
    /// </summary>
    public string SupplierName { get; private set; } = string.Empty;
    /// <summary>
    /// Title。
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = ProcurementRequestStatus.Draft;
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
    /// Reviewed By。
    /// </summary>
    public string? ReviewedBy { get; private set; }
    /// <summary>
    /// 明细行集合。
    /// </summary>
    public List<ProcurementRequestLine> Lines { get; private set; } = [];
    /// <summary>
    /// Procurement Order Id。
    /// </summary>
    public Guid? ProcurementOrderId { get; private set; }

    /// <summary>
    /// Submit。
    /// </summary>
    public void Submit()
    {
        Status = ProcurementRequestStatus.Submitted;
        Touch();
    }

    /// <summary>
    /// Decide。
    /// </summary>
    /// <param name="decision">处理决策。</param>
    /// <param name="reviewedBy">reviewed By 参数。</param>
    public void Decide(string decision, string reviewedBy)
    {
        Status = decision;
        ReviewedBy = reviewedBy;
        Touch();
    }

    /// <summary>
    /// Link Order。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    public void LinkOrder(Guid orderId)
    {
        ProcurementOrderId = orderId;
        Status = ProcurementRequestStatus.Ordered;
        Touch();
    }
}
