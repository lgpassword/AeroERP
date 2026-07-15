namespace AeroERP.BuildingBlocks.Abstractions;

/// <summary>
/// Aero Erp Module Descriptor 数据记录。
/// </summary>
/// <param name="Key">业务键。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="Category">业务分类。</param>
public sealed record AeroErpModuleDescriptor(string Key, string DisplayName, string Category);
