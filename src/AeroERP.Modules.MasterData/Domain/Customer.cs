using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.MasterData.Domain;

/// <summary>
/// Customer 业务对象。
/// </summary>
public sealed class Customer : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Customer实例。
    /// </summary>
    private Customer()
    {
    }

    /// <summary>
    /// 初始化Customer实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="contactName">联系人姓名。</param>
    /// <param name="phone">联系电话。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="organizationName">所属组织名称。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="taxpayerId">纳税人识别号。</param>
    /// <param name="invoiceTitle">发票抬头。</param>
    public Customer(string code, string name, string contactName, string phone, bool isEnabled, Guid? organizationId, string organizationName, string currencyCode, string taxpayerId, string invoiceTitle)
    {
        Code = code;
        Name = name;
        ContactName = contactName;
        Phone = phone;
        IsEnabled = isEnabled;
        OrganizationId = organizationId;
        OrganizationName = organizationName;
        CurrencyCode = currencyCode;
        TaxpayerId = taxpayerId;
        InvoiceTitle = invoiceTitle;
    }

    /// <summary>
    /// 业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 联系人姓名。
    /// </summary>
    public string ContactName { get; private set; } = string.Empty;
    /// <summary>
    /// 联系电话。
    /// </summary>
    public string Phone { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }
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
    /// 纳税人识别号。
    /// </summary>
    public string TaxpayerId { get; private set; } = string.Empty;
    /// <summary>
    /// 发票抬头。
    /// </summary>
    public string InvoiceTitle { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="contactName">联系人姓名。</param>
    /// <param name="phone">联系电话。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="organizationName">所属组织名称。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="taxpayerId">纳税人识别号。</param>
    /// <param name="invoiceTitle">发票抬头。</param>
    public void Update(string code, string name, string contactName, string phone, bool isEnabled, Guid? organizationId, string organizationName, string currencyCode, string taxpayerId, string invoiceTitle)
    {
        Code = code;
        Name = name;
        ContactName = contactName;
        Phone = phone;
        IsEnabled = isEnabled;
        OrganizationId = organizationId;
        OrganizationName = organizationName;
        CurrencyCode = currencyCode;
        TaxpayerId = taxpayerId;
        InvoiceTitle = invoiceTitle;
        Touch();
    }
}
