import { EmptyState, StyleRegistry } from "@aeroerp/ui-kit";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider, useAuth } from "./auth/AuthContext";
import { Shell } from "./components/Shell";
import { api } from "./api/client";
import { useAsyncData } from "./hooks/useAsyncData";
import { LanguageProvider } from "./i18n/LanguageContext";
import { getFirstVisibleModuleRoute, moduleRoutes, type KnownModuleKey } from "./modules/moduleNavigation";
import { ControlPage } from "./pages/ControlPage";
import { DocumentExchangePage } from "./pages/DocumentExchangePage";
import { LoginPage } from "./pages/LoginPage";
import { FinancePage } from "./pages/FinancePage";
import { AdvancedManufacturingPage } from "./pages/AdvancedManufacturingPage";
import { InventoryPage } from "./pages/InventoryPage";
import { IntegrationPage } from "./pages/IntegrationPage";
import { LocalizationPage } from "./pages/LocalizationPage";
import { ManufacturingPage } from "./pages/ManufacturingPage";
import { MasterDataPage } from "./pages/MasterDataPage";
import { MobileWorkPage } from "./pages/MobileWorkPage";
import { PlatformPage } from "./pages/PlatformPage";
import { PositionPermissionsPage } from "./pages/PositionPermissionsPage";
import { PlanningPage } from "./pages/PlanningPage";
import { ProcurementPage } from "./pages/ProcurementPage";
import { QualityPage } from "./pages/QualityPage";
import { ReportingPage } from "./pages/ReportingPage";
import { SalesPage } from "./pages/SalesPage";
import { WmsPage } from "./pages/WmsPage";
import { WorkflowPage } from "./pages/WorkflowPage";
import type { ModuleVisibility } from "./types/api";

const loadEmptyModules = () => Promise.resolve<ModuleVisibility[]>([]);

/**
 * 登录后的受保护应用区域。
 * 先同步可见模块，再按模块权限挂载路由，避免隐藏模块从导航或地址栏被访问。
 */
function ProtectedApp() {
  const { user, loading } = useAuth();
  const visibleModules = useAsyncData(
    user ? api.listVisibleModules : loadEmptyModules,
    `${user?.id ?? ""}|${user?.permissions.join("|") ?? ""}|${user?.visibleModuleKeys.join("|") ?? ""}`,
  );

  if (loading) {
    return <div className="app-loading">正在加载 AeroERP 工作台...</div>;
  }

  if (!user) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    );
  }

  if (visibleModules.loading) {
    return <div className="app-loading">正在同步模块权限...</div>;
  }

  if (visibleModules.error || !visibleModules.data) {
    return (
      <div className="app-shell centered">
        <EmptyState
          title="后端服务不可用"
          description={visibleModules.error ?? "平台接口没有响应，请先启动 AppHost。"}
        />
      </div>
    );
  }

  const visibleModuleKeys = new Set(visibleModules.data.map((module) => module.key));
  const canAccess = (moduleKey: KnownModuleKey) => visibleModuleKeys.has(moduleKey);
  const defaultRoute = getFirstVisibleModuleRoute(visibleModuleKeys);

  if (!defaultRoute) {
    return (
      <div className="app-shell centered">
        <EmptyState
          title="当前账号暂无可访问模块"
          description="请让平台管理员为该账号分配角色与模块权限。"
        />
      </div>
    );
  }

  return (
    <Routes>
      <Route element={<Shell modules={visibleModules.data} user={user} />}>
        {canAccess("platform") ? <Route path={moduleRoutes.platform} element={<PlatformPage />} /> : null}
        {canAccess("master-data") ? <Route path={moduleRoutes["master-data"]} element={<MasterDataPage />} /> : null}
        {canAccess("procurement") ? <Route path={moduleRoutes.procurement} element={<ProcurementPage />} /> : null}
        {canAccess("sales") ? <Route path={moduleRoutes.sales} element={<SalesPage />} /> : null}
        {canAccess("inventory") ? <Route path={moduleRoutes.inventory} element={<InventoryPage />} /> : null}
        {canAccess("wms") ? <Route path={moduleRoutes.wms} element={<WmsPage />} /> : null}
        {canAccess("mobile-work") ? <Route path={moduleRoutes["mobile-work"]} element={<MobileWorkPage />} /> : null}
        {canAccess("integration") ? <Route path={moduleRoutes.integration} element={<IntegrationPage />} /> : null}
        {canAccess("document-exchange") ? <Route path={moduleRoutes["document-exchange"]} element={<DocumentExchangePage />} /> : null}
        {canAccess("finance") ? <Route path={moduleRoutes.finance} element={<FinancePage />} /> : null}
        {canAccess("workflow") ? <Route path={moduleRoutes.workflow} element={<WorkflowPage />} /> : null}
        {canAccess("control") ? <Route path={moduleRoutes.control} element={<ControlPage />} /> : null}
        {canAccess("localization") ? <Route path={moduleRoutes.localization} element={<LocalizationPage />} /> : null}
        {canAccess("position-permissions") ? <Route path={moduleRoutes["position-permissions"]} element={<PositionPermissionsPage />} /> : null}
        {canAccess("manufacturing") ? <Route path={moduleRoutes.manufacturing} element={<ManufacturingPage />} /> : null}
        {canAccess("advanced-manufacturing") ? <Route path={moduleRoutes["advanced-manufacturing"]} element={<AdvancedManufacturingPage />} /> : null}
        {canAccess("reporting") ? <Route path={moduleRoutes.reporting} element={<ReportingPage />} /> : null}
        {canAccess("quality") ? <Route path={moduleRoutes.quality} element={<QualityPage />} /> : null}
        {canAccess("planning") ? <Route path={moduleRoutes.planning} element={<PlanningPage />} /> : null}
        <Route path="/" element={<Navigate to={defaultRoute} replace />} />
      </Route>
      <Route path="/login" element={<Navigate to={defaultRoute} replace />} />
    </Routes>
  );
}

/** Web 应用根组件，装配全局样式、路由、认证上下文和多语言上下文。 */
function App() {
  return (
    <>
      <StyleRegistry />
      <BrowserRouter>
        <AuthProvider>
          <LanguageProvider>
            <ProtectedApp />
          </LanguageProvider>
        </AuthProvider>
      </BrowserRouter>
    </>
  );
}

export default App;
