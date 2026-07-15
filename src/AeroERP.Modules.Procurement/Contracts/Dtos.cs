namespace AeroERP.Modules.Procurement.Contracts;

/// <summary>
/// Procurement Request Line 数据传输对象。
/// </summary>
/// <param name="ItemId">物料标识。</param>
/// <param name="ItemName">Item Name 参数。</param>
/// <param name="Quantity">数量。</param>
/// <param name="Unit">计量单位。</param>
public sealed record ProcurementRequestLineDto(Guid ItemId, string ItemName, decimal Quantity, string Unit);
/// <summary>
/// Procurement Request 数据传输对象。
/// </summary>
public sealed record ProcurementRequestDto(
    Guid Id,
    string RequestNo,
    Guid SupplierId,
    string SupplierName,
    string Title,
    string Status,
    Guid? OrganizationId,
    string OrganizationName,
    string CurrencyCode,
    string TaxInvoiceType,
    decimal TaxRate,
    IReadOnlyList<ProcurementRequestLineDto> Lines,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Procurement Order 数据传输对象。
/// </summary>
public sealed record ProcurementOrderDto(
    Guid Id,
    string OrderNo,
    Guid RequestId,
    string RequestNo,
    string SupplierName,
    string Status,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Create Procurement Request Line 请求参数。
/// </summary>
/// <param name="ItemId">物料标识。</param>
/// <param name="Quantity">数量。</param>
/// <param name="Unit">计量单位。</param>
public sealed record CreateProcurementRequestLineRequest(Guid ItemId, decimal Quantity, string Unit);
/// <summary>
/// Create Procurement Request 请求参数。
/// </summary>
/// <param name="SupplierId">供应商标识。</param>
/// <param name="Title">标题。</param>
/// <param name="CurrencyCode">币种编码。</param>
/// <param name="TaxInvoiceType">Tax Invoice Type 参数。</param>
/// <param name="TaxRate">税率。</param>
/// <param name="Lines">明细行集合。</param>
public sealed record CreateProcurementRequestRequest(Guid SupplierId, string Title, string CurrencyCode, string TaxInvoiceType, decimal? TaxRate, IReadOnlyList<CreateProcurementRequestLineRequest> Lines);
/// <summary>
/// Decide Procurement Request 请求参数。
/// </summary>
/// <param name="Decision">处理决策。</param>
public sealed record DecideProcurementRequestRequest(string Decision);
