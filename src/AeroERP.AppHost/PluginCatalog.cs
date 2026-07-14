using AeroERP.BuildingBlocks.Abstractions;

namespace AeroERP.AppHost;

/// <summary>
/// Plugin Catalog 业务对象。
/// </summary>
public static class PluginCatalog
{
    /// <summary>
    /// Core Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] CoreModules =
        ModuleCatalog.Modules
            .Where(module => module.Key is not "position-permissions" and not "wms" and not "advanced-manufacturing" and not "reporting" and not "mobile-work" and not "integration" and not "document-exchange")
            .ToArray();

    /// <summary>
    /// Position Permission Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] PositionPermissionModules =
        ModuleCatalog.Modules
            .Where(module => module.Key == "position-permissions")
            .ToArray();

    /// <summary>
    /// Wms Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] WmsModules =
        ModuleCatalog.Modules
            .Where(module => module.Key == "wms")
            .ToArray();

    /// <summary>
    /// Advanced Manufacturing Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] AdvancedManufacturingModules =
        ModuleCatalog.Modules
            .Where(module => module.Key == "advanced-manufacturing")
            .ToArray();

    /// <summary>
    /// Reporting Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] ReportingModules =
        ModuleCatalog.Modules
            .Where(module => module.Key == "reporting")
            .ToArray();

    /// <summary>
    /// Mobile Work Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] MobileWorkModules =
        ModuleCatalog.Modules
            .Where(module => module.Key == "mobile-work")
            .ToArray();

    /// <summary>
    /// Integration Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] IntegrationModules =
        ModuleCatalog.Modules
            .Where(module => module.Key == "integration")
            .ToArray();

    /// <summary>
    /// Document Exchange Modules。
    /// </summary>
    private static readonly AeroErpModuleDescriptor[] DocumentExchangeModules =
        ModuleCatalog.Modules
            .Where(module => module.Key == "document-exchange")
            .ToArray();

    /// <summary>
    /// Plugins。
    /// </summary>
    public static readonly AeroErpPluginDescriptor[] Plugins =
    [
        new("aeroerp.core", "AeroERP 核心业务插件", CoreModules),
        new("aeroerp.position-permissions", "岗位权限插件", PositionPermissionModules),
        new("aeroerp.wms", "WMS 执行插件", WmsModules),
        new("aeroerp.advanced-manufacturing", "高级制造插件", AdvancedManufacturingModules),
        new("aeroerp.reporting", "报表中心插件", ReportingModules),
        new("aeroerp.mobile-work", "移动作业插件", MobileWorkModules),
        new("aeroerp.integration", "通知与集成插件", IntegrationModules),
        new("aeroerp.document-exchange", "文档交换插件", DocumentExchangeModules)
    ];
}
