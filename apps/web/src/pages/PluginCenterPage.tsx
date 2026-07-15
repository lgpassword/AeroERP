import { Eye, EyeOff, PackageCheck, RefreshCcw } from "lucide-react";
import { useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { ModuleVisibility } from "../types/api";

const loadEmptyModules = () => Promise.resolve<ModuleVisibility[]>([]);

/** 插件中心页面，基于真实模块可见性接口管理当前已安装插件模块。 */
export function PluginCenterPage() {
  const { hasPermission, refresh } = useAuth();
  const canManagePlugins = hasPermission(platformPermissions.pluginManage);
  const modulesQuery = useAsyncData(canManagePlugins ? api.listModules : loadEmptyModules, canManagePlugins ? "plugin-center" : "no-plugin-center");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const modules = modulesQuery.data ?? [];
  const groupedModules = modules.reduce<Record<string, ModuleVisibility[]>>((groups, module) => {
    const key = module.category || "未分组";
    groups[key] = [...(groups[key] ?? []), module];
    return groups;
  }, {});

  async function runAction(actionKey: string, action: () => Promise<void>, successText?: string) {
    setBusyKey(actionKey);
    setError(null);
    setMessage(null);
    try {
      await action();
      if (successText) {
        setMessage(successText);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "操作失败");
    } finally {
      setBusyKey(null);
    }
  }

  async function toggleModule(module: ModuleVisibility) {
    await runAction(`plugin-${module.id}`, async () => {
      await api.toggleModule(module.id, !module.isVisible);
      await modulesQuery.reload();
      await refresh();
    }, `${module.displayName} 已${module.isVisible ? "隐藏" : "显示"}。`);
  }

  return (
    <PageShell
      title="插件中心"
      actions={canManagePlugins ? (
        <button
          type="button"
          className="secondary icon-button"
          disabled={busyKey === "plugin-refresh"}
          onClick={() => void runAction("plugin-refresh", modulesQuery.reload, "插件模块已刷新。")}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      ) : undefined}
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      {!canManagePlugins ? (
        <EmptyState title="无插件中心权限" description="当前账号不能查看或调整插件模块。" />
      ) : (
        <>
          <section className="stats-grid plugin-summary-grid">
            <StatTile label="安装模块" value={modules.length} tone={modules.length > 0 ? "success" : "warning"} />
            <StatTile label="显示模块" value={modules.filter((module) => module.isVisible).length} tone="success" />
            <StatTile label="隐藏模块" value={modules.filter((module) => !module.isVisible).length} tone="warning" />
            <StatTile label="插件分组" value={Object.keys(groupedModules).length} tone="success" />
          </section>

          {modulesQuery.loading ? <div className="section-note">正在加载插件模块...</div> : null}
          {modulesQuery.error ? <div className="section-note error">{modulesQuery.error}</div> : null}

          <SectionBlock title="插件模块" hint="模块显隐会影响导航入口，业务数据不会因为隐藏入口被删除。">
            {Object.entries(groupedModules).length > 0 ? (
              <div className="plugin-group-grid">
                {Object.entries(groupedModules).map(([category, items]) => (
                  <div key={category} className="plugin-group-card">
                    <div className="plugin-group-head">
                      <PackageCheck size={18} />
                      <div>
                        <strong>{category}</strong>
                        <p>{items.length} 个模块</p>
                      </div>
                    </div>
                    <div className="people-card-list compact-list">
                      {items.map((module) => (
                        <div key={module.id} className="people-card-row">
                          <div>
                            <strong>{module.displayName}</strong>
                            <p>{module.key}</p>
                          </div>
                          <button
                            type="button"
                            className={`toggle-btn${module.isVisible ? " on" : ""}`}
                            disabled={busyKey === `plugin-${module.id}`}
                            onClick={() => void toggleModule(module)}
                          >
                            {module.isVisible ? <Eye size={16} /> : <EyeOff size={16} />}
                            <span>{module.isVisible ? "已显示" : "已隐藏"}</span>
                          </button>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无插件模块" description="后端模块目录为空，请检查 ModuleCatalog 初始化。" />
            )}
          </SectionBlock>
        </>
      )}
    </PageShell>
  );
}
