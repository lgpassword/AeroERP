import { useEffect, useMemo, useRef, useState } from "react";
import {
  BarChart3,
  Boxes,
  BriefcaseBusiness,
  CircleDollarSign,
  ClipboardCheck,
  ClipboardList,
  Cog,
  Factory,
  Globe2,
  HandCoins,
  Languages,
  LayoutDashboard,
  LogOut,
  Menu,
  MessageSquare,
  PackageCheck,
  Puzzle,
  ScanSearch,
  Search,
  Shield,
  ShoppingBag,
  ShoppingCart,
  Smartphone,
  UserRound,
  UsersRound,
  Warehouse,
  X,
} from "lucide-react";
import { Link, NavLink, Outlet } from "react-router-dom";
import { motion } from "framer-motion";
import { useAuth } from "../auth/AuthContext";
import { useLanguage } from "../i18n/LanguageContext";
import {
  getModuleRoute,
  isKnownModuleKey,
  moduleNavigationMeta,
  moduleRouteOrder,
  workspaceRoute,
  type KnownModuleKey,
  type ModuleNavigationGroup,
} from "../modules/moduleNavigation";
import type { CurrentUser, ModuleVisibility } from "../types/api";

const iconMap = {
  platform: Shield,
  "organization-collaboration": MessageSquare,
  "people-management": UsersRound,
  "plugin-center": Puzzle,
  "master-data": Boxes,
  crm: UsersRound,
  procurement: ShoppingCart,
  sales: CircleDollarSign,
  inventory: PackageCheck,
  wms: Warehouse,
  "mobile-work": Smartphone,
  integration: Globe2,
  "channel-integration": ShoppingBag,
  "document-exchange": ClipboardList,
  finance: HandCoins,
  workflow: ClipboardCheck,
  control: BarChart3,
  localization: Languages,
  "position-permissions": BriefcaseBusiness,
  manufacturing: Factory,
  "advanced-manufacturing": Cog,
  reporting: BarChart3,
  quality: ScanSearch,
  planning: ClipboardList,
} as const;

const groupLabels: Record<ModuleNavigationGroup, string> = {
  organization: "组织与人员",
  core: "业务主线",
  execution: "执行现场",
  finance: "财务与报表",
  integration: "集成插件",
  governance: "平台治理",
};

type NavItem = {
  key: KnownModuleKey;
  label: string;
  subtitle: string;
  group: ModuleNavigationGroup;
  to: string;
  icon: (typeof iconMap)[KnownModuleKey];
};

function getInitials(name: string) {
  return name.trim().slice(0, 2).toUpperCase() || "AE";
}

/** 登录后的工作台外壳，负责导航分组、账号菜单、语言切换和路由出口。 */
export function Shell({ modules, user }: { modules: ModuleVisibility[]; user: CurrentUser }) {
  const { logout } = useAuth();
  const { language, setLanguage, t } = useLanguage();
  const [moduleFilter, setModuleFilter] = useState("");
  const [accountMenuOpen, setAccountMenuOpen] = useState(false);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement | null>(null);

  const roleLabels = user.roles.map((role, index) => {
    const displayName = user.roleDisplayNames[index] ?? role;
    return t(`role.${role}`, displayName);
  });

  const visibleModuleMap = useMemo(() => {
    return new Map(modules.map((module) => [module.key, module]));
  }, [modules]);

  const navItems = useMemo<NavItem[]>(() => {
    return moduleRouteOrder
      .filter((moduleKey) => visibleModuleMap.has(moduleKey))
      .map((moduleKey) => {
        const meta = moduleNavigationMeta[moduleKey];
        const module = visibleModuleMap.get(moduleKey);
        return {
          key: moduleKey,
          label: t(`module.${moduleKey}`, module?.displayName ?? moduleKey),
          subtitle: meta.subtitle,
          group: meta.group,
          to: getModuleRoute(moduleKey),
          icon: iconMap[moduleKey],
        };
      });
  }, [t, visibleModuleMap]);

  const filteredNavItems = useMemo(() => {
    const keyword = moduleFilter.trim().toLowerCase();
    if (!keyword) {
      return navItems;
    }

    return navItems.filter((item) => {
      return `${item.label} ${item.subtitle} ${item.key}`.toLowerCase().includes(keyword);
    });
  }, [moduleFilter, navItems]);

  const groupedNavItems = useMemo(() => {
    return filteredNavItems.reduce<Record<ModuleNavigationGroup, NavItem[]>>((groups, item) => {
      groups[item.group].push(item);
      return groups;
    }, {
      organization: [],
      core: [],
      execution: [],
      finance: [],
      integration: [],
      governance: [],
    });
  }, [filteredNavItems]);

  const quickLinks = [
    navItems.find((item) => item.key === "platform"),
    navItems.find((item) => item.key === "control"),
    navItems.find((item) => item.key === "workflow"),
    navItems.find((item) => item.key === "reporting"),
  ].filter(Boolean) as NavItem[];

  useEffect(() => {
    if (!accountMenuOpen) {
      return undefined;
    }

    function closeOnOutsideClick(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setAccountMenuOpen(false);
      }
    }

    document.addEventListener("mousedown", closeOnOutsideClick);
    return () => document.removeEventListener("mousedown", closeOnOutsideClick);
  }, [accountMenuOpen]);

  function closeMobileSidebar() {
    setSidebarOpen(false);
  }

  return (
    <div className={`workspace-shell${sidebarOpen ? " sidebar-open" : ""}`}>
      <aside className="workspace-sidebar" aria-label="模块导航">
        <Link to={workspaceRoute} className="brand sidebar-brand" onClick={closeMobileSidebar}>
          <div className="brand-mark">AE</div>
          <div className="brand-copy">
            <small>{t("app.workspace", "AeroERP 工作台")}</small>
            <strong>AeroERP</strong>
            <span>{t("app.tagline", "模块化企业运营平台")}</span>
          </div>
        </Link>

        <NavLink to={workspaceRoute} end className={({ isActive }) => `workspace-home-link${isActive ? " active" : ""}`} onClick={closeMobileSidebar}>
          <LayoutDashboard size={18} />
          <span>
            <strong>工作台</strong>
            <small>总览、链路与入口</small>
          </span>
        </NavLink>

        <label className="sidebar-search">
          <Search size={16} />
          <input
            value={moduleFilter}
            onChange={(event) => setModuleFilter(event.target.value)}
            placeholder="搜索模块"
            aria-label="搜索模块"
          />
        </label>

        <nav className="side-nav">
          {Object.entries(groupedNavItems).map(([group, items]) => {
            if (items.length === 0) {
              return null;
            }

            return (
              <section className="side-nav-group" key={group}>
                <h2>{groupLabels[group as ModuleNavigationGroup]}</h2>
                {items.map((item) => {
                  const Icon = item.icon;
                  return (
                    <NavLink
                      key={item.key}
                      to={item.to}
                      className={({ isActive }) => `side-nav-item${isActive ? " active" : ""}`}
                      onClick={closeMobileSidebar}
                    >
                      <span className="side-nav-icon"><Icon size={18} /></span>
                      <span>
                        <strong>{item.label}</strong>
                        <small>{item.subtitle}</small>
                      </span>
                    </NavLink>
                  );
                })}
              </section>
            );
          })}
          {filteredNavItems.length === 0 ? (
            <div className="sidebar-empty">没有匹配的模块</div>
          ) : null}
        </nav>
      </aside>

      <div className="workspace-main">
        <header className="topbar">
          <button
            type="button"
            className="secondary icon-button mobile-menu-button"
            onClick={() => setSidebarOpen((value) => !value)}
            aria-label={sidebarOpen ? "关闭导航" : "打开导航"}
          >
            {sidebarOpen ? <X size={18} /> : <Menu size={18} />}
          </button>

          <div className="topbar-status">
            <strong>企业运营工作区</strong>
            <span>已加载 {navItems.length} 个模块 · {user.permissions.length} 项权限</span>
          </div>

          <div className="topbar-actions">
            <div className="language-switch" aria-label={t("language.current", "界面语言")}>
              <Languages size={16} />
              <button type="button" className={language === "zh-CN" ? "active" : ""} onClick={() => setLanguage("zh-CN")}>
                {t("language.zh", "中文")}
              </button>
              <button type="button" className={language === "en-US" ? "active" : ""} onClick={() => setLanguage("en-US")}>
                {t("language.en", "英文")}
              </button>
            </div>

            <div className="account-menu" ref={menuRef}>
              <button
                type="button"
                className="secondary account-trigger"
                onClick={() => setAccountMenuOpen((value) => !value)}
                aria-expanded={accountMenuOpen}
                aria-haspopup="menu"
              >
                <span className="account-avatar">{getInitials(user.displayName)}</span>
                <span>
                  <strong>{user.displayName}</strong>
                  <small>{roleLabels.join(" / ") || "未分配角色"}</small>
                </span>
              </button>

              {accountMenuOpen ? (
                <motion.div
                  className="account-dropdown"
                  role="menu"
                  initial={{ opacity: 0, y: -8 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.16 }}
                >
                  <div className="account-dropdown-head">
                    <UserRound size={18} />
                    <div>
                      <strong>{user.displayName}</strong>
                      <span>{user.userName}</span>
                    </div>
                  </div>
                  <div className="account-dropdown-section">
                    <small>常用入口</small>
                    <Link to={workspaceRoute} onClick={() => setAccountMenuOpen(false)}>工作台</Link>
                    {quickLinks.map((item) => (
                      <Link to={item.to} key={item.key} onClick={() => setAccountMenuOpen(false)}>
                        {item.label}
                      </Link>
                    ))}
                  </div>
                  <div className="account-dropdown-section">
                    <small>账号能力</small>
                    <span>{user.roles.length} 个角色</span>
                    <span>{user.permissions.length} 项权限</span>
                    <span>{modules.filter((module) => isKnownModuleKey(module.key)).length} 个可用模块</span>
                  </div>
                  <button type="button" className="secondary icon-button logout-menu-item" onClick={logout}>
                    <LogOut size={16} />
                    <span>{t("action.logout", "退出登录")}</span>
                  </button>
                </motion.div>
              ) : null}
            </div>
          </div>
        </header>

        {sidebarOpen ? <button type="button" className="sidebar-backdrop" aria-label="关闭导航" onClick={closeMobileSidebar} /> : null}

        <main className="content-area">
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.18 }}
            className="route-stage"
          >
            <Outlet />
          </motion.div>
        </main>
      </div>
    </div>
  );
}
