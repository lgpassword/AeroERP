import {
  Activity,
  ArrowRight,
  Boxes,
  ClipboardCheck,
  Factory,
  Gauge,
  HandCoins,
  LayoutDashboard,
  LockKeyhole,
  MessageSquare,
  Puzzle,
  ShieldCheck,
  ShoppingBag,
  UsersRound,
} from "lucide-react";
import { Link } from "react-router-dom";
import { PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { useLanguage } from "../i18n/LanguageContext";
import {
  getModuleRoute,
  isKnownModuleKey,
  moduleNavigationMeta,
  moduleRouteOrder,
  type KnownModuleKey,
  type ModuleNavigationGroup,
} from "../modules/moduleNavigation";
import type { CurrentUser, ModuleVisibility } from "../types/api";

const groupLabels: Record<ModuleNavigationGroup, string> = {
  organization: "组织与人员",
  core: "业务主线",
  execution: "执行现场",
  finance: "财务与报表",
  integration: "集成插件",
  governance: "平台治理",
};

const groupDescriptions: Record<ModuleNavigationGroup, string> = {
  organization: "组织协同、人员账号、部门和岗位入口。",
  core: "主数据、采购、销售这些高频业务入口。",
  execution: "库存、仓储、制造、质量和移动执行入口。",
  finance: "财务闭环、报表运行和导出入口。",
  integration: "消息通道、渠道连接、文档交换和外部系统入口。",
  governance: "平台、审批、权限、集成和本地化配置入口。",
};

const flowSteps: { key: KnownModuleKey; label: string; detail: string }[] = [
  { key: "master-data", label: "基础资料", detail: "客户、供应商、物料、仓库" },
  { key: "procurement", label: "采购来源", detail: "申请、审核、订单" },
  { key: "inventory", label: "库存执行", detail: "入库、出库、调拨、盘点" },
  { key: "finance", label: "财务结算", detail: "应收应付、凭证、银行对账" },
  { key: "reporting", label: "经营报表", detail: "报表运行、导出和审计" },
];

function getModuleIcon(moduleKey: KnownModuleKey) {
  switch (moduleKey) {
    case "organization-collaboration":
      return MessageSquare;
    case "people-management":
      return UsersRound;
    case "plugin-center":
      return Puzzle;
    case "crm":
      return UsersRound;
    case "channel-integration":
      return ShoppingBag;
    case "master-data":
      return Boxes;
    case "procurement":
    case "sales":
      return ClipboardCheck;
    case "inventory":
    case "wms":
    case "mobile-work":
      return Activity;
    case "finance":
    case "reporting":
      return HandCoins;
    case "manufacturing":
    case "advanced-manufacturing":
      return Factory;
    case "platform":
    case "position-permissions":
      return ShieldCheck;
    default:
      return LayoutDashboard;
  }
}

/** 登录后的总览工作台，聚合真实可见模块和当前账号能力，不伪造业务数据。 */
export function WorkspacePage({ modules, user }: { modules: ModuleVisibility[]; user: CurrentUser }) {
  const { t } = useLanguage();
  const visibleKnownModules = moduleRouteOrder
    .filter((moduleKey) => modules.some((module) => module.key === moduleKey));
  const unknownModules = modules.filter((module) => !isKnownModuleKey(module.key));
  const groupedModules = visibleKnownModules.reduce<Record<ModuleNavigationGroup, KnownModuleKey[]>>((groups, moduleKey) => {
    groups[moduleNavigationMeta[moduleKey].group].push(moduleKey);
    return groups;
  }, {
    organization: [],
    core: [],
    execution: [],
    finance: [],
    integration: [],
    governance: [],
  });
  const flowCoverage = flowSteps.filter((step) => visibleKnownModules.includes(step.key)).length;
  const maxGroupCount = Math.max(1, ...Object.values(groupedModules).map((items) => items.length));

  return (
    <PageShell
      title="工作台"
      actions={visibleKnownModules[0] ? (
        <Link className="button-link primary-link" to={getModuleRoute(visibleKnownModules[0])}>
          <LayoutDashboard size={16} />
          <span>进入首个模块</span>
        </Link>
      ) : undefined}
    >
      <section className="workspace-hero">
        <div>
          <span className="eyebrow">AeroERP</span>
          <h2>{user.displayName}，当前工作区已按权限加载</h2>
          <p>这里展示当前账号真实可见的模块、权限和闭环覆盖情况。所有入口都会跳转到现有业务页面，不替代原有功能。</p>
        </div>
        <div className="workspace-hero-panel">
          <Gauge size={28} />
          <strong>{Math.round((flowCoverage / flowSteps.length) * 100)}%</strong>
          <span>核心链路覆盖</span>
        </div>
      </section>

      <div className="stats-grid workspace-stat-grid">
        <StatTile label="可见模块" value={visibleKnownModules.length} tone={visibleKnownModules.length > 0 ? "success" : "warning"} />
        <StatTile label="权限数量" value={user.permissions.length} tone={user.permissions.length > 0 ? "success" : "warning"} />
        <StatTile label="角色数量" value={user.roles.length} tone={user.roles.length > 0 ? "success" : "warning"} />
        <StatTile label="未接入路由" value={unknownModules.length} tone={unknownModules.length > 0 ? "warning" : "success"} />
      </div>

      <SectionBlock title="模块分布" hint="按业务主线、执行现场、财务报表和平台治理分组，减少顶栏拥挤但保留所有可见模块入口。">
        <div className="workspace-chart-grid">
          {Object.entries(groupedModules).map(([group, items]) => {
            const typedGroup = group as ModuleNavigationGroup;
            return (
              <div className="workspace-bar-card" key={group}>
                <div className="workspace-bar-card-head">
                  <strong>{groupLabels[typedGroup]}</strong>
                  <span>{items.length} 个模块</span>
                </div>
                <div className="workspace-bar-track" aria-label={`${groupLabels[typedGroup]}模块数量`}>
                  <span style={{ width: `${Math.max(8, (items.length / maxGroupCount) * 100)}%` }} />
                </div>
                <p>{groupDescriptions[typedGroup]}</p>
              </div>
            );
          })}
        </div>
      </SectionBlock>

      <SectionBlock title="闭环链路" hint="核心业务流按真实可见模块点亮；没有权限或模块被隐藏时只显示状态，不制造不可用按钮。">
        <div className="workspace-flow">
          {flowSteps.map((step, index) => {
            const isAvailable = visibleKnownModules.includes(step.key);
            const Icon = getModuleIcon(step.key);
            return (
              <div className={`workspace-flow-step${isAvailable ? " available" : ""}`} key={step.key}>
                {index > 0 ? <ArrowRight className="workspace-flow-arrow" size={18} /> : null}
                {isAvailable ? (
                  <Link to={getModuleRoute(step.key)}>
                    <Icon size={18} />
                    <strong>{step.label}</strong>
                    <span>{step.detail}</span>
                  </Link>
                ) : (
                  <div>
                    <LockKeyhole size={18} />
                    <strong>{step.label}</strong>
                    <span>未授权或模块已隐藏</span>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </SectionBlock>

      <SectionBlock title="模块入口" hint="下方卡片来自后端可见模块列表，点击后进入对应业务页面。">
        <div className="workspace-module-grid">
          {visibleKnownModules.map((moduleKey) => {
            const Icon = getModuleIcon(moduleKey);
            const meta = moduleNavigationMeta[moduleKey];
            return (
              <Link className="workspace-module-card" to={getModuleRoute(moduleKey)} key={moduleKey}>
                <span className="workspace-module-icon"><Icon size={18} /></span>
                <strong>{t(`module.${moduleKey}`, modules.find((module) => module.key === moduleKey)?.displayName ?? moduleKey)}</strong>
                <p>{meta.subtitle}</p>
                <small>{groupLabels[meta.group]}</small>
              </Link>
            );
          })}
        </div>
      </SectionBlock>

      {unknownModules.length > 0 ? (
        <SectionBlock title="路由接入提醒" hint="这些模块已由后端返回为可见，但当前 Web 前端还没有对应路由页面。">
          <div className="compact-tag-list">
            {unknownModules.map((module) => (
              <span className="compact-tag" key={module.key}>{module.displayName}（{module.key}）</span>
            ))}
          </div>
        </SectionBlock>
      ) : null}
    </PageShell>
  );
}
