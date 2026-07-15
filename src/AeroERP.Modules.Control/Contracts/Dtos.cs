namespace AeroERP.Modules.Control.Contracts;

/// <summary>
/// Analytics Metric 数据传输对象。
/// </summary>
/// <param name="Key">业务键。</param>
/// <param name="Label">界面显示标签。</param>
/// <param name="Value">数值或配置值。</param>
/// <param name="Unit">计量单位。</param>
public sealed record AnalyticsMetricDto(string Key, string Label, decimal Value, string Unit);

/// <summary>
/// Analytics Snapshot 数据传输对象。
/// </summary>
public sealed record AnalyticsSnapshotDto(
    IReadOnlyList<AnalyticsMetricDto> Procurement,
    IReadOnlyList<AnalyticsMetricDto> Sales,
    IReadOnlyList<AnalyticsMetricDto> Inventory,
    IReadOnlyList<AnalyticsMetricDto> Finance,
    DateTimeOffset GeneratedAtUtc);

/// <summary>
/// Data Scope Rule 数据传输对象。
/// </summary>
public sealed record DataScopeRuleDto(
    Guid Id,
    string RoleKey,
    string ScopeType,
    string MatchValue,
    string Description,
    bool IsEnabled,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Upsert Data Scope Rule 请求参数。
/// </summary>
public sealed record UpsertDataScopeRuleRequest(
    string RoleKey,
    string ScopeType,
    string MatchValue,
    string Description,
    bool IsEnabled);

/// <summary>
/// Numbering Rule 数据传输对象。
/// </summary>
public sealed record NumberingRuleDto(
    Guid Id,
    string DocumentType,
    string Prefix,
    bool UseDateSegment,
    int NextSequence,
    int Padding,
    bool IsEnabled,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Upsert Numbering Rule 请求参数。
/// </summary>
public sealed record UpsertNumberingRuleRequest(
    string DocumentType,
    string Prefix,
    bool UseDateSegment,
    int Padding,
    bool IsEnabled);
