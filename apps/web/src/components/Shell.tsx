import { AnimatePresence, motion } from "framer-motion";
import { BarChart3, Boxes, BriefcaseBusiness, CircleDollarSign, ClipboardCheck, ClipboardList, Cog, Factory, Globe2, HandCoins, Languages, LogOut, PackageCheck, ScanSearch, Shield, ShoppingCart, Smartphone, UserRound, Warehouse } from "lucide-react";
import { Link, NavLink, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useLanguage } from "../i18n/LanguageContext";
import { getModuleRoute } from "../modules/moduleNavigation";
import type { ModuleVisibility } from "../types/api";
import type { CurrentUser } from "../types/api";

// 导航图标按模块 key 固定映射，新增模块时应同时补充图标和 moduleNavigation 路由。
const iconMap = {
  platform: Shield,
  "master-data": Boxes,
  procurement: ShoppingCart,
  sales: CircleDollarSign,
  inventory: PackageCheck,
  wms: Warehouse,
  "mobile-work": Smartphone,
  integration: Globe2,
  "document-exchange": ClipboardList,
  finance: HandCoins,
  workflow: ClipboardCheck,
  control: BarChart3,
  localization: Globe2,
  "position-permissions": BriefcaseBusiness,
  manufacturing: Factory,
  "advanced-manufacturing": Cog,
  reporting: BarChart3,
  quality: ScanSearch,
  planning: ClipboardList,
} as const;

/** 登录后的工作台外壳，负责顶部导航、账号信息、语言切换和路由内容出口。 */
export function Shell({ modules, user }: { modules: ModuleVisibility[]; user: CurrentUser }) {
  const location = useLocation();
  const { logout } = useAuth();
  const { language, setLanguage, t } = useLanguage();
  const nav = modules.map((module) => ({
    key: module.key,
    label: t(`module.${module.key}`, module.displayName),
    to: getModuleRoute(module.key),
    icon: iconMap[module.key as keyof typeof iconMap] ?? Shield,
  }));
  const roleLabels = user.roles.map((role, index) => {
    const displayName = user.roleDisplayNames[index] ?? role;
    return t(`role.${role}`, displayName);
  });

  return (
    <div className="workspace-shell">
      <header className="topbar">
        <div className="topbar-left">
          <Link to="/" className="brand topbar-brand">
            <div className="brand-mark">AE</div>
            <div className="brand-copy">
              <small>{t("app.workspace", "AeroERP 工作台")}</small>
              <strong>AeroERP</strong>
              <span>{t("app.tagline", "模块化企业运营平台")}</span>
            </div>
          </Link>
          <nav className="top-nav" aria-label="主导航">
            {nav.map((item) => {
              const Icon = item.icon;
              return (
                <NavLink
                  key={item.key}
                  to={item.to}
                  className={({ isActive }) => `top-nav-item${isActive ? " active" : ""}`}
                >
                  <Icon size={16} />
                  <span>{item.label}</span>
                </NavLink>
              );
            })}
          </nav>
        </div>
        <div className="topbar-actions">
          <div className="language-switch" aria-label={t("language.current", "界面语言")}>
            <Languages size={16} />
            <button
              type="button"
              className={language === "zh-CN" ? "active" : ""}
              onClick={() => setLanguage("zh-CN")}
            >
              {t("language.zh", "中文")}
            </button>
            <button
              type="button"
              className={language === "en-US" ? "active" : ""}
              onClick={() => setLanguage("en-US")}
            >
              {t("language.en", "英文")}
            </button>
          </div>
          <div className="topbar-account">
            <div className="user-chip topbar-user-chip">
              <UserRound size={16} />
              <div>
                <strong>{user.displayName}</strong>
                <span>{roleLabels.join(" / ") || "未分配角色"}</span>
              </div>
            </div>
          </div>
          <button className="secondary icon-button topbar-logout-button" onClick={logout}>
            <LogOut size={16} />
            <span>{t("action.logout", "退出登录")}</span>
          </button>
        </div>
      </header>

      <main className="content-area">
        <AnimatePresence initial={false}>
          <motion.div
            key={location.pathname}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.14 }}
            className="route-stage"
          >
            <Outlet />
          </motion.div>
        </AnimatePresence>
      </main>
    </div>
  );
}
