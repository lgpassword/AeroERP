// 工作台是登录后的聚合首页；业务模块路由仍然由后端模块可见性决定。
export const workspaceRoute = "/workspace";

// 模块路由的唯一来源，路由守卫和导航入口必须从这里取路径。
export const moduleRoutes = {
  platform: "/platform",
  "organization-collaboration": "/organization-collaboration",
  "people-management": "/people-management",
  "plugin-center": "/plugin-center",
  "master-data": "/master-data",
  crm: "/crm",
  procurement: "/procurement",
  sales: "/sales",
  inventory: "/inventory",
  wms: "/wms",
  "mobile-work": "/mobile-work",
  integration: "/integration",
  "channel-integration": "/channel-integration",
  "document-exchange": "/document-exchange",
  finance: "/finance",
  workflow: "/workflow",
  control: "/control",
  localization: "/localization",
  "position-permissions": "/position-permissions",
  manufacturing: "/manufacturing",
  "advanced-manufacturing": "/advanced-manufacturing",
  reporting: "/reporting",
  quality: "/quality",
  planning: "/planning",
} as const;

/** 当前前端已知且可路由的模块 key。 */
export type KnownModuleKey = keyof typeof moduleRoutes;

export type ModuleNavigationGroup = "organization" | "core" | "execution" | "finance" | "integration" | "governance";

export type ModuleNavigationMeta = {
  key: KnownModuleKey;
  subtitle: string;
  group: ModuleNavigationGroup;
  priority: number;
};

// 导航元数据只描述前端展示，不改变权限和模块可见性判断。
export const moduleNavigationMeta: Record<KnownModuleKey, ModuleNavigationMeta> = {
  platform: { key: "platform", subtitle: "组织、账号、模块与审查", group: "governance", priority: 70 },
  "organization-collaboration": { key: "organization-collaboration", subtitle: "组织、部门、个人联系", group: "organization", priority: 8 },
  "people-management": { key: "people-management", subtitle: "员工账号、入职、组织架构", group: "organization", priority: 9 },
  "plugin-center": { key: "plugin-center", subtitle: "插件模块、显隐、分组", group: "governance", priority: 75 },
  "master-data": { key: "master-data", subtitle: "客户、供应商、物料、仓库", group: "core", priority: 10 },
  crm: { key: "crm", subtitle: "客户、报价、订单管道", group: "core", priority: 15 },
  procurement: { key: "procurement", subtitle: "申请、审核、订单下达", group: "core", priority: 20 },
  sales: { key: "sales", subtitle: "报价、订单、待发货", group: "core", priority: 30 },
  inventory: { key: "inventory", subtitle: "入库、出库、调拨、盘点", group: "execution", priority: 40 },
  wms: { key: "wms", subtitle: "上架、拣货、波次、PDA", group: "execution", priority: 50 },
  "mobile-work": { key: "mobile-work", subtitle: "移动设备、离线任务、扫码", group: "execution", priority: 60 },
  integration: { key: "integration", subtitle: "消息通道、Webhook、同步", group: "integration", priority: 120 },
  "channel-integration": { key: "channel-integration", subtitle: "企微、电商、内容渠道", group: "integration", priority: 125 },
  "document-exchange": { key: "document-exchange", subtitle: "导入、导出、打印、审计", group: "integration", priority: 130 },
  finance: { key: "finance", subtitle: "总账、应收应付、结算", group: "finance", priority: 80 },
  workflow: { key: "workflow", subtitle: "审批待办、通知与实例", group: "governance", priority: 90 },
  control: { key: "control", subtitle: "经营指标、数据范围、编号", group: "governance", priority: 100 },
  localization: { key: "localization", subtitle: "语言、币种、税务基础", group: "governance", priority: 150 },
  "position-permissions": { key: "position-permissions", subtitle: "部门、岗位、权限包", group: "governance", priority: 110 },
  manufacturing: { key: "manufacturing", subtitle: "BOM、工单、领料、完工", group: "execution", priority: 55 },
  "advanced-manufacturing": { key: "advanced-manufacturing", subtitle: "工艺、排程、产能、成本", group: "execution", priority: 56 },
  reporting: { key: "reporting", subtitle: "报表定义、运行、导出", group: "finance", priority: 140 },
  quality: { key: "quality", subtitle: "质检、批次、追溯链", group: "execution", priority: 65 },
  planning: { key: "planning", subtitle: "补货、外协、条码执行", group: "execution", priority: 66 },
};

/** 模块展示顺序，决定导航排序。 */
export const moduleRouteOrder = (Object.keys(moduleNavigationMeta) as KnownModuleKey[])
  .sort((left, right) => moduleNavigationMeta[left].priority - moduleNavigationMeta[right].priority);

/** 判断后端返回的模块 key 是否已有前端页面承接。 */
export function isKnownModuleKey(moduleKey: string): moduleKey is KnownModuleKey {
  return moduleKey in moduleRoutes;
}

/** 将前端已知模块 key 映射到业务路由，未知模块必须先经过 isKnownModuleKey 判断。 */
export function getModuleRoute(moduleKey: KnownModuleKey) {
  return moduleRoutes[moduleKey];
}

/** 根据可见模块集合计算第一个业务模块入口，通常用于极端回退。 */
export function getFirstVisibleModuleRoute(moduleKeys: Set<string>) {
  const firstVisibleModule = moduleRouteOrder.find((moduleKey) => moduleKeys.has(moduleKey));
  return firstVisibleModule ? moduleRoutes[firstVisibleModule] : null;
}
