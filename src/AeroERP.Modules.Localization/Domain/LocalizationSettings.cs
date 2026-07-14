using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Localization.Domain;

/// <summary>
/// Localization Settings 业务对象。
/// </summary>
public sealed class LocalizationSettings : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Localization Settings实例。
    /// </summary>
    private LocalizationSettings()
    {
    }

    /// <summary>
    /// 初始化Localization Settings实例。
    /// </summary>
    /// <param name="defaultCurrencyCode">default Currency Code 参数。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxpayerId">纳税人识别号。</param>
    /// <param name="invoiceTitle">发票抬头。</param>
    /// <param name="defaultTaxRate">default Tax Rate 参数。</param>
    public LocalizationSettings(string defaultCurrencyCode, string taxInvoiceType, string taxpayerId, string invoiceTitle, decimal defaultTaxRate)
    {
        DefaultCurrencyCode = defaultCurrencyCode;
        TaxInvoiceType = taxInvoiceType;
        TaxpayerId = taxpayerId;
        InvoiceTitle = invoiceTitle;
        DefaultTaxRate = defaultTaxRate;
    }

    /// <summary>
    /// Default Currency Code。
    /// </summary>
    public string DefaultCurrencyCode { get; private set; } = "CNY";
    /// <summary>
    /// Tax Invoice Type。
    /// </summary>
    public string TaxInvoiceType { get; private set; } = "增值税普通发票";
    /// <summary>
    /// 纳税人识别号。
    /// </summary>
    public string TaxpayerId { get; private set; } = string.Empty;
    /// <summary>
    /// 发票抬头。
    /// </summary>
    public string InvoiceTitle { get; private set; } = string.Empty;
    /// <summary>
    /// Default Tax Rate。
    /// </summary>
    public decimal DefaultTaxRate { get; private set; } = 0.13m;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="defaultCurrencyCode">default Currency Code 参数。</param>
    /// <param name="taxInvoiceType">tax Invoice Type 参数。</param>
    /// <param name="taxpayerId">纳税人识别号。</param>
    /// <param name="invoiceTitle">发票抬头。</param>
    /// <param name="defaultTaxRate">default Tax Rate 参数。</param>
    public void Update(string defaultCurrencyCode, string taxInvoiceType, string taxpayerId, string invoiceTitle, decimal defaultTaxRate)
    {
        DefaultCurrencyCode = defaultCurrencyCode;
        TaxInvoiceType = taxInvoiceType;
        TaxpayerId = taxpayerId;
        InvoiceTitle = invoiceTitle;
        DefaultTaxRate = defaultTaxRate;
        Touch();
    }
}
