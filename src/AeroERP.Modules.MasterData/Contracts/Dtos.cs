namespace AeroERP.Modules.MasterData.Contracts;

/// <summary>
/// Customer 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="ContactName">联系人姓名。</param>
/// <param name="Phone">联系电话。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="OrganizationId">所属组织标识。</param>
/// <param name="OrganizationName">所属组织名称。</param>
/// <param name="CurrencyCode">币种编码。</param>
/// <param name="TaxpayerId">纳税人识别号。</param>
/// <param name="InvoiceTitle">发票抬头。</param>
public sealed record CustomerDto(Guid Id, string Code, string Name, string ContactName, string Phone, bool IsEnabled, Guid? OrganizationId, string OrganizationName, string CurrencyCode, string TaxpayerId, string InvoiceTitle);
/// <summary>
/// Supplier 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="ContactName">联系人姓名。</param>
/// <param name="Phone">联系电话。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="OrganizationId">所属组织标识。</param>
/// <param name="OrganizationName">所属组织名称。</param>
/// <param name="CurrencyCode">币种编码。</param>
/// <param name="TaxpayerId">纳税人识别号。</param>
/// <param name="InvoiceTitle">发票抬头。</param>
public sealed record SupplierDto(Guid Id, string Code, string Name, string ContactName, string Phone, bool IsEnabled, Guid? OrganizationId, string OrganizationName, string CurrencyCode, string TaxpayerId, string InvoiceTitle);
/// <summary>
/// Item 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Specification">规格型号。</param>
/// <param name="Unit">计量单位。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record ItemDto(Guid Id, string Code, string Name, string Specification, string Unit, bool IsEnabled);
/// <summary>
/// Warehouse 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Location">位置说明。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="OrganizationId">所属组织标识。</param>
/// <param name="OrganizationName">所属组织名称。</param>
public sealed record WarehouseDto(Guid Id, string Code, string Name, string Location, bool IsEnabled, Guid? OrganizationId, string OrganizationName);

/// <summary>
/// Upsert Customer 请求参数。
/// </summary>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="ContactName">联系人姓名。</param>
/// <param name="Phone">联系电话。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="OrganizationId">所属组织标识。</param>
/// <param name="CurrencyCode">币种编码。</param>
/// <param name="TaxpayerId">纳税人识别号。</param>
/// <param name="InvoiceTitle">发票抬头。</param>
public sealed record UpsertCustomerRequest(string Code, string Name, string ContactName, string Phone, bool IsEnabled, Guid? OrganizationId, string CurrencyCode, string TaxpayerId, string InvoiceTitle);
/// <summary>
/// Upsert Supplier 请求参数。
/// </summary>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="ContactName">联系人姓名。</param>
/// <param name="Phone">联系电话。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="OrganizationId">所属组织标识。</param>
/// <param name="CurrencyCode">币种编码。</param>
/// <param name="TaxpayerId">纳税人识别号。</param>
/// <param name="InvoiceTitle">发票抬头。</param>
public sealed record UpsertSupplierRequest(string Code, string Name, string ContactName, string Phone, bool IsEnabled, Guid? OrganizationId, string CurrencyCode, string TaxpayerId, string InvoiceTitle);
/// <summary>
/// Upsert Item 请求参数。
/// </summary>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Specification">规格型号。</param>
/// <param name="Unit">计量单位。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record UpsertItemRequest(string Code, string Name, string Specification, string Unit, bool IsEnabled);
/// <summary>
/// Upsert Warehouse 请求参数。
/// </summary>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Location">位置说明。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="OrganizationId">所属组织标识。</param>
public sealed record UpsertWarehouseRequest(string Code, string Name, string Location, bool IsEnabled, Guid? OrganizationId);
