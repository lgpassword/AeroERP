namespace AeroERP.Modules.Localization.Contracts;

/// <summary>
/// Currency 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Symbol">币种符号。</param>
/// <param name="ExchangeRateToBase">Exchange Rate To Base 参数。</param>
/// <param name="IsBase">Is Base 参数。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record CurrencyDto(Guid Id, string Code, string Name, string Symbol, decimal ExchangeRateToBase, bool IsBase, bool IsEnabled);

/// <summary>
/// Upsert Currency 请求参数。
/// </summary>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Symbol">币种符号。</param>
/// <param name="ExchangeRateToBase">Exchange Rate To Base 参数。</param>
/// <param name="IsBase">Is Base 参数。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record UpsertCurrencyRequest(string Code, string Name, string Symbol, decimal ExchangeRateToBase, bool IsBase, bool IsEnabled);

/// <summary>
/// Localization Settings 数据传输对象。
/// </summary>
public sealed record LocalizationSettingsDto(
    Guid Id,
    string DefaultCurrencyCode,
    string TaxInvoiceType,
    string TaxpayerId,
    string InvoiceTitle,
    decimal DefaultTaxRate);

/// <summary>
/// Update Localization Settings 请求参数。
/// </summary>
public sealed record UpdateLocalizationSettingsRequest(
    string DefaultCurrencyCode,
    string TaxInvoiceType,
    string TaxpayerId,
    string InvoiceTitle,
    decimal DefaultTaxRate);

/// <summary>
/// Localization Content 数据传输对象。
/// </summary>
public sealed record LocalizationContentDto(
    Guid Id,
    string Key,
    string Category,
    string ChineseText,
    string EnglishText,
    bool IsEnabled,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Upsert Localization Content 请求参数。
/// </summary>
public sealed record UpsertLocalizationContentRequest(
    string Key,
    string Category,
    string ChineseText,
    string EnglishText,
    bool IsEnabled);
