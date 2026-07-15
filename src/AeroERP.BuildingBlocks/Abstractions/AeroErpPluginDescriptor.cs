namespace AeroERP.BuildingBlocks.Abstractions;

/// <summary>
/// Aero Erp Plugin Descriptor 数据记录。
/// </summary>
public sealed record AeroErpPluginDescriptor(
    string Key,
    string DisplayName,
    IReadOnlyList<AeroErpModuleDescriptor> Modules);
