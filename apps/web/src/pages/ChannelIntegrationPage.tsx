import { Globe2, RefreshCcw, ShoppingBag } from "lucide-react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { IntegrationOverview } from "../types/api";

const emptyOverview: IntegrationOverview = {
  channels: [],
  webhooks: [],
  connectors: [],
  syncJobs: [],
  auditRecords: [],
  metrics: [],
};

const loadEmptyOverview = () => Promise.resolve(emptyOverview);

const providerPresets = [
  { key: "wecom", label: "企业微信", provider: "WeCom", baseUrl: "https://qyapi.weixin.qq.com", authMode: "OAuth2" },
  { key: "douyin", label: "抖音", provider: "Douyin", baseUrl: "https://open.douyin.com", authMode: "OAuth2" },
  { key: "taobao-tmall", label: "淘宝/天猫", provider: "TaobaoTmall", baseUrl: "https://eco.taobao.com", authMode: "OAuth2" },
  { key: "jd", label: "京东", provider: "JD", baseUrl: "https://api.jd.com", authMode: "OAuth2" },
  { key: "xiaohongshu", label: "小红书", provider: "Xiaohongshu", baseUrl: "https://ark.xiaohongshu.com", authMode: "OAuth2" },
] as const;

/** 渠道集成插件页面，按垂直行业渠道维护真实外部连接器。 */
export function ChannelIntegrationPage() {
  const { hasPermission } = useAuth();
  const canRead = hasPermission(platformPermissions.integrationRead);
  const canManage = hasPermission(platformPermissions.integrationManage);
  const overviewQuery = useAsyncData(canRead ? api.getIntegrationOverview : loadEmptyOverview, canRead ? "channel-integration" : "no-channel-integration");
  const overview = overviewQuery.data ?? emptyOverview;
  const connectors = overview.connectors;

  async function saveConnector(preset: (typeof providerPresets)[number]) {
    await api.upsertIntegrationConnector({
      connectorKey: preset.key,
      displayName: preset.label,
      provider: preset.provider,
      baseUrl: preset.baseUrl,
      authMode: preset.authMode,
      isEnabled: true,
    });
    await overviewQuery.reload();
  }

  const enabledConnectorCount = connectors.filter((connector) => connector.isEnabled).length;
  const coveredPresetCount = providerPresets.filter((preset) => connectors.some((connector) => connector.connectorKey === preset.key)).length;

  return (
    <PageShell
      title="渠道集成"
      actions={canRead ? (
        <button type="button" className="secondary icon-button" onClick={overviewQuery.reload}>
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      ) : undefined}
    >
      {!canRead ? (
        <EmptyState title="无渠道集成查看权限" description="当前账号不能读取外部渠道连接器。" />
      ) : (
        <>
          <section className="stats-grid channel-summary-grid">
            <StatTile label="渠道模板" value={providerPresets.length} tone="success" />
            <StatTile label="已建连接器" value={connectors.length} tone={connectors.length > 0 ? "success" : "warning"} />
            <StatTile label="启用连接器" value={enabledConnectorCount} tone={enabledConnectorCount > 0 ? "success" : "warning"} />
            <StatTile label="模板覆盖" value={`${Math.round((coveredPresetCount / providerPresets.length) * 100)}%`} tone={coveredPresetCount > 0 ? "success" : "warning"} />
          </section>

          {overviewQuery.loading ? <div className="section-note">正在加载渠道连接器...</div> : null}
          {overviewQuery.error ? <div className="section-note error">{overviewQuery.error}</div> : null}

          <SectionBlock title="垂直渠道连接器" hint="这些连接器保存真实第三方接口基础配置，密钥仍由安全配置管理。">
            <div className="channel-provider-grid">
              {providerPresets.map((preset) => {
                const connector = connectors.find((item) => item.connectorKey === preset.key);
                return (
                  <div key={preset.key} className={`channel-provider-card${connector?.isEnabled ? " ready" : ""}`}>
                    <div className="channel-provider-head">
                      <span className="people-avatar"><ShoppingBag size={17} /></span>
                      <div>
                        <strong>{preset.label}</strong>
                        <p>{preset.provider}</p>
                      </div>
                    </div>
                    <small>{connector ? connector.baseUrl : preset.baseUrl}</small>
                    <div className="channel-provider-meta">
                      <span>{connector ? (connector.isEnabled ? "已启用" : "已停用") : "未创建"}</span>
                      <span>{connector?.authMode ?? preset.authMode}</span>
                    </div>
                    {canManage ? (
                      <button type="button" onClick={() => void saveConnector(preset)}>
                        <Globe2 size={16} />
                        <span>{connector ? "更新连接器" : "创建连接器"}</span>
                      </button>
                    ) : null}
                  </div>
                );
              })}
            </div>
          </SectionBlock>

          <section className="org-contact-grid">
            <SectionBlock title="现有连接器" hint="连接器统一存放在通知与集成模块，渠道页面只做垂直入口聚合。">
              {connectors.length > 0 ? (
                <div className="people-card-list compact-list">
                  {connectors.map((connector) => (
                    <div key={connector.id} className="people-card-row">
                      <div>
                        <strong>{connector.connectorKey} · {connector.displayName}</strong>
                        <p>{connector.provider} · {connector.authMode}</p>
                        <small>{connector.baseUrl}</small>
                      </div>
                      <span className={connector.isEnabled ? "status-pill success" : "status-pill warning"}>
                        {connector.isEnabled ? "启用" : "停用"}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title="暂无连接器" description="创建渠道连接器后，会在这里统一查看。" />
              )}
            </SectionBlock>

            <SectionBlock title="同步任务" hint="订单、库存、商品等同步任务继续复用集成模块执行状态。">
              {overview.syncJobs.length > 0 ? (
                <div className="people-card-list compact-list">
                  {overview.syncJobs.map((job) => (
                    <div key={job.id} className="people-card-row">
                      <div>
                        <strong>{job.jobNo} · {job.connectorKey}</strong>
                        <p>{job.direction} · {job.status}</p>
                        <small>尝试 {job.attemptCount} 次</small>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title="暂无同步任务" description="在通知与集成模块创建同步任务后，这里会按渠道显示状态。" />
              )}
            </SectionBlock>
          </section>
        </>
      )}
    </PageShell>
  );
}
