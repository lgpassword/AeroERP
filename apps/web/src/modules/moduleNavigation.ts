// 模块路由的唯一来源，路由守卫和顶部导航必须从这里取路径。
export const moduleRoutes = {
  platform: "/platform",
  "master-data": "/master-data",
  procurement: "/procurement",
  sales: "/sales",
  inventory: "/inventory",
  wms: "/wms",
  "mobile-work": "/mobile-work",
  integration: "/integration",
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

/** 模块展示顺序，决定默认入口和导航排序。 */
export const moduleRouteOrder = Object.keys(moduleRoutes) as KnownModuleKey[];

/** 将后端返回的模块 key 映射到前端路由，未知模块回退到采购页。 */
export function getModuleRoute(moduleKey: string) {
  return moduleRoutes[moduleKey as KnownModuleKey] ?? moduleRoutes.procurement;
}

/** 根据可见模块集合计算登录后的第一个可访问页面。 */
export function getFirstVisibleModuleRoute(moduleKeys: Set<string>) {
  const firstVisibleModule = moduleRouteOrder.find((moduleKey) => moduleKeys.has(moduleKey));
  return firstVisibleModule ? moduleRoutes[firstVisibleModule] : null;
}
