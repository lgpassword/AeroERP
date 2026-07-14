namespace AeroERP.Modules.Sales.Contracts;

/// <summary>
/// Sales Line 数据传输对象。
/// </summary>
/// <param name="ItemId">物料标识。</param>
/// <param name="ItemCode">Item Code 参数。</param>
/// <param name="ItemName">Item Name 参数。</param>
/// <param name="Quantity">数量。</param>
/// <param name="Unit">计量单位。</param>
public sealed record SalesLineDto(Guid ItemId, string ItemCode, string ItemName, decimal Quantity, string Unit);

/// <summary>
/// Sales Quotation 数据传输对象。
/// </summary>
public sealed record SalesQuotationDto(
    Guid Id,
    string QuotationNo,
    Guid CustomerId,
    string CustomerName,
    string Title,
    string Status,
    Guid? OrganizationId,
    string OrganizationName,
    string CurrencyCode,
    string TaxInvoiceType,
    decimal TaxRate,
    IReadOnlyList<SalesLineDto> Lines,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Sales Order 数据传输对象。
/// </summary>
public sealed record SalesOrderDto(
    Guid Id,
    string OrderNo,
    Guid QuotationId,
    string QuotationNo,
    Guid CustomerId,
    string CustomerName,
    string Status,
    Guid? OrganizationId,
    string OrganizationName,
    string CurrencyCode,
    string TaxInvoiceType,
    decimal TaxRate,
    IReadOnlyList<SalesLineDto> Lines,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Create Sales Quotation Line 请求参数。
/// </summary>
/// <param name="ItemId">物料标识。</param>
/// <param name="Quantity">数量。</param>
/// <param name="Unit">计量单位。</param>
public sealed record CreateSalesQuotationLineRequest(Guid ItemId, decimal Quantity, string Unit);

/// <summary>
/// Create Sales Quotation 请求参数。
/// </summary>
public sealed record CreateSalesQuotationRequest(
    Guid CustomerId,
    string Title,
    string CurrencyCode,
    string TaxInvoiceType,
    decimal? TaxRate,
    IReadOnlyList<CreateSalesQuotationLineRequest> Lines);
