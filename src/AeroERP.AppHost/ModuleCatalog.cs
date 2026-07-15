using AeroERP.BuildingBlocks.Abstractions;

namespace AeroERP.AppHost;

/// <summary>
/// Module Catalog 业务对象。
/// </summary>
public static class ModuleCatalog
{
    /// <summary>
    /// Modules。
    /// </summary>
    public static readonly AeroErpModuleDescriptor[] Modules =
    [
        new("platform", "平台治理", "平台"),
        new("organization-collaboration", "组织协同", "组织管理"),
        new("people-management", "人员管理", "组织管理"),
        new("plugin-center", "插件中心", "平台"),
        new("master-data", "主数据", "业务运营"),
        new("crm", "客户CRM", "业务运营"),
        new("procurement", "采购管理", "业务运营"),
        new("sales", "销售管理", "业务运营"),
        new("inventory", "库存管理", "业务运营"),
        new("wms", "WMS 执行", "业务运营"),
        new("mobile-work", "移动作业", "业务运营"),
        new("integration", "通知与集成", "平台"),
        new("channel-integration", "渠道集成", "集成插件"),
        new("document-exchange", "文档交换", "平台"),
        new("finance", "财务结算", "业务运营"),
        new("workflow", "审批中心", "平台"),
        new("control", "经营管控", "平台"),
        new("localization", "语言与本地化", "平台"),
        new("position-permissions", "岗位权限", "平台"),
        new("manufacturing", "制造管理", "业务运营"),
        new("advanced-manufacturing", "高级制造", "业务运营"),
        new("reporting", "报表中心", "业务运营"),
        new("quality", "质量追溯", "业务运营"),
        new("planning", "计划执行", "业务运营")
    ];
}
