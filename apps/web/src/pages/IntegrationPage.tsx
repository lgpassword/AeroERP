import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
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

function formatDate(value?: string | null) {
  if (!value) {
    return "未完成";
  }

  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function statusText(status: string) {
  switch (status) {
    case "Pending":
      return "待处理";
    case "Running":
      return "运行中";
    case "Completed":
      return "已完成";
    case "Failed":
      return "失败";
    default:
      return status || "未设置";
  }
}

function directionText(direction: string) {
  return direction === "Inbound" ? "入站" : direction === "Outbound" ? "出站" : direction || "未设置";
}

/** 集成页面，维护消息通道、外部连接器、同步任务、Webhook 和审计状态。 */
export function IntegrationPage() {
  const { hasPermission } = useAuth();
  const canRead = hasPermission(platformPermissions.integrationRead);
  const canManage = hasPermission(platformPermissions.integrationManage);
  const canExecute = hasPermission(platformPermissions.integrationExecute);
  const overviewQuery = useAsyncData(canRead ? api.getIntegrationOverview : loadEmptyOverview);
  const overview = overviewQuery.data ?? emptyOverview;

  const [channelForm, setChannelForm] = useState({
    channelKey: "",
    displayName: "",
    channelType: "站内通知",
    endpoint: "",
    isEnabled: true,
  });
  const [webhookForm, setWebhookForm] = useState({
    subscriptionKey: "",
    displayName: "",
    eventKey: "",
    targetUrl: "",
    secretName: "",
    isEnabled: true,
  });
  const [connectorForm, setConnectorForm] = useState({
    connectorKey: "",
    displayName: "",
    provider: "",
    baseUrl: "https://",
    authMode: "ApiKey",
    isEnabled: true,
  });
  const [jobForm, setJobForm] = useState({
    connectorKey: "",
    direction: "Outbound",
    payloadJson: "{}",
  });
  const [failReasons, setFailReasons] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const enabledConnectors = useMemo(() => overview.connectors.filter((connector) => connector.isEnabled), [overview.connectors]);
  const activeJobs = useMemo(() => overview.syncJobs.filter((job) => job.status !== "Completed"), [overview.syncJobs]);

  async function runAction(actionKey: string, action: () => Promise<void>, successText?: string) {
    setBusyKey(actionKey);
    setMessage(null);
    setError(null);
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

  async function reloadOverview() {
    if (canRead) {
      await overviewQuery.reload();
    }
  }

  if (!canRead) {
    return (
      <PageShell title="通知与集成">
        <EmptyState title="无通知与集成查看权限" description="当前账号不能读取消息通道、Webhook、连接器、同步任务和集成审计。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="通知与集成"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "integration-refresh"}
          onClick={async () => {
            await runAction("integration-refresh", reloadOverview, "通知与集成数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid inventory-kpi-grid">
        {(overview.metrics.length > 0 ? overview.metrics : [
          { key: "channels", label: "启用通道", value: overview.channels.filter((item) => item.isEnabled).length, unit: "个" },
          { key: "webhooks", label: "启用 Webhook", value: overview.webhooks.filter((item) => item.isEnabled).length, unit: "个" },
          { key: "connectors", label: "启用连接器", value: enabledConnectors.length, unit: "个" },
          { key: "active-jobs", label: "未完成任务", value: activeJobs.length, unit: "条" },
        ]).map((metric) => (
          <StatTile key={metric.key} label={`${metric.label}（${metric.unit}）`} value={metric.value} tone={metric.value > 0 ? "success" : "default"} />
        ))}
      </section>

      {overviewQuery.loading ? <div className="section-note">正在加载通知与集成...</div> : null}
      {overviewQuery.error ? <div className="section-note error">{overviewQuery.error}</div> : null}

      <SectionBlock title="消息通道" hint="通道保存通知出口配置。">
        <div className="inventory-surface-grid">
          <div className="inventory-surface">
            {overview.channels.length > 0 ? (
              <div className="inventory-record-list">
                {overview.channels.map((channel) => (
                  <div key={channel.id} className="inventory-record-row">
                    <div>
                      <strong>{channel.channelKey} · {channel.displayName}</strong>
                      <p>{channel.channelType} · {channel.isEnabled ? "已启用" : "已停用"}</p>
                      <small>{channel.endpoint || "未设置端点"}</small>
                    </div>
                    <div className="inventory-record-meta">
                      <small>{channel.updatedBy || "系统"}</small>
                      <small>{formatDate(channel.updatedAtUtc)}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无消息通道" description="保存消息通道后，外部通知出口会显示在这里。" />
            )}
          </div>
          <div className="inventory-surface">
            {canManage ? (
              <form
                className="stack-form inventory-form-panel"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!channelForm.channelKey.trim() || !channelForm.displayName.trim() || !channelForm.channelType.trim()) {
                    setError("请填写通道编码、名称和类型。");
                    return;
                  }

                  await runAction("channel-upsert", async () => {
                    await api.upsertIntegrationChannel({
                      channelKey: channelForm.channelKey.trim(),
                      displayName: channelForm.displayName.trim(),
                      channelType: channelForm.channelType.trim(),
                      endpoint: channelForm.endpoint.trim(),
                      isEnabled: channelForm.isEnabled,
                    });
                    setChannelForm({ channelKey: "", displayName: "", channelType: "站内通知", endpoint: "", isEnabled: true });
                    await reloadOverview();
                  }, "消息通道已保存。");
                }}
              >
                <input placeholder="通道编码" value={channelForm.channelKey} onChange={(event) => setChannelForm({ ...channelForm, channelKey: event.target.value })} />
                <input placeholder="通道名称" value={channelForm.displayName} onChange={(event) => setChannelForm({ ...channelForm, displayName: event.target.value })} />
                <input placeholder="通道类型" value={channelForm.channelType} onChange={(event) => setChannelForm({ ...channelForm, channelType: event.target.value })} />
                <input placeholder="端点" value={channelForm.endpoint} onChange={(event) => setChannelForm({ ...channelForm, endpoint: event.target.value })} />
                <label className="checkbox-row">
                  <input type="checkbox" checked={channelForm.isEnabled} onChange={(event) => setChannelForm({ ...channelForm, isEnabled: event.target.checked })} />
                  启用通道
                </label>
                <button type="submit" disabled={busyKey === "channel-upsert" || !channelForm.channelKey.trim() || !channelForm.displayName.trim()}>
                  保存通道
                </button>
              </form>
            ) : (
              <EmptyState title="无通道维护权限" description="当前账号只能查看消息通道。" />
            )}
          </div>
        </div>
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="外部连接器" hint="连接器保存第三方接口基础配置。">
          {overview.connectors.length > 0 ? (
            <div className="inventory-record-list">
              {overview.connectors.map((connector) => (
                <div key={connector.id} className="inventory-record-row">
                  <div>
                    <strong>{connector.connectorKey} · {connector.displayName}</strong>
                    <p>{connector.provider} · {connector.authMode} · {connector.isEnabled ? "已启用" : "已停用"}</p>
                    <small>{connector.baseUrl}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{connector.updatedBy || "系统"}</small>
                    <small>{formatDate(connector.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无连接器" description="创建连接器后，可以基于它创建同步任务。" />
          )}

          {canManage ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                if (!connectorForm.connectorKey.trim() || !connectorForm.displayName.trim() || !connectorForm.provider.trim() || !connectorForm.baseUrl.trim()) {
                  setError("请填写连接器编码、名称、供应方和基础地址。");
                  return;
                }

                await runAction("connector-upsert", async () => {
                  await api.upsertIntegrationConnector({
                    connectorKey: connectorForm.connectorKey.trim(),
                    displayName: connectorForm.displayName.trim(),
                    provider: connectorForm.provider.trim(),
                    baseUrl: connectorForm.baseUrl.trim(),
                    authMode: connectorForm.authMode.trim(),
                    isEnabled: connectorForm.isEnabled,
                  });
                  setConnectorForm({ connectorKey: "", displayName: "", provider: "", baseUrl: "https://", authMode: "ApiKey", isEnabled: true });
                  await reloadOverview();
                }, "外部连接器已保存。");
              }}
            >
              <input placeholder="连接器编码" value={connectorForm.connectorKey} onChange={(event) => setConnectorForm({ ...connectorForm, connectorKey: event.target.value })} />
              <input placeholder="连接器名称" value={connectorForm.displayName} onChange={(event) => setConnectorForm({ ...connectorForm, displayName: event.target.value })} />
              <input placeholder="供应方" value={connectorForm.provider} onChange={(event) => setConnectorForm({ ...connectorForm, provider: event.target.value })} />
              <input placeholder="基础地址" value={connectorForm.baseUrl} onChange={(event) => setConnectorForm({ ...connectorForm, baseUrl: event.target.value })} />
              <select value={connectorForm.authMode} onChange={(event) => setConnectorForm({ ...connectorForm, authMode: event.target.value })}>
                <option value="ApiKey">API Key</option>
                <option value="OAuth2">OAuth2</option>
                <option value="None">无认证</option>
              </select>
              <label className="checkbox-row">
                <input type="checkbox" checked={connectorForm.isEnabled} onChange={(event) => setConnectorForm({ ...connectorForm, isEnabled: event.target.checked })} />
                启用连接器
              </label>
              <button type="submit" disabled={busyKey === "connector-upsert" || !connectorForm.connectorKey.trim() || !connectorForm.displayName.trim() || !connectorForm.provider.trim() || !connectorForm.baseUrl.trim()}>
                保存连接器
              </button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="同步任务" hint="任务记录连接器、方向、载荷、状态和重试次数。">
          {overview.syncJobs.length > 0 ? (
            <div className="inventory-record-list">
              {overview.syncJobs.map((job) => {
                const failReason = failReasons[job.id] ?? job.lastError ?? "";
                return (
                  <div key={job.id} className="inventory-record-row">
                    <div>
                      <strong>{job.jobNo} · {job.connectorKey}</strong>
                      <p>{directionText(job.direction)} · {statusText(job.status)} · 尝试 {job.attemptCount} 次</p>
                      <small>{job.payloadJson}</small>
                      {job.lastError ? <small>失败原因：{job.lastError}</small> : null}
                    </div>
                    <div className="inventory-record-meta">
                      {canExecute && job.status !== "Completed" ? (
                        <>
                          {job.status !== "Running" ? (
                            <button
                              className="secondary"
                              disabled={busyKey === `job-start-${job.id}`}
                              onClick={async () => {
                                await runAction(`job-start-${job.id}`, async () => {
                                  await api.startIntegrationSyncJob(job.id);
                                  await reloadOverview();
                                }, `${job.jobNo} 已开始。`);
                              }}
                            >
                              开始
                            </button>
                          ) : null}
                          <button
                            disabled={busyKey === `job-complete-${job.id}`}
                            onClick={async () => {
                              await runAction(`job-complete-${job.id}`, async () => {
                                await api.completeIntegrationSyncJob(job.id);
                                await reloadOverview();
                              }, `${job.jobNo} 已完成。`);
                            }}
                          >
                            完成
                          </button>
                          <input placeholder="失败原因" value={failReason} onChange={(event) => setFailReasons({ ...failReasons, [job.id]: event.target.value })} />
                          <button
                            className="secondary"
                            disabled={busyKey === `job-fail-${job.id}` || !failReason.trim()}
                            onClick={async () => {
                              await runAction(`job-fail-${job.id}`, async () => {
                                await api.failIntegrationSyncJob(job.id, failReason.trim());
                                await reloadOverview();
                              }, `${job.jobNo} 已标记失败。`);
                            }}
                          >
                            标记失败
                          </button>
                          {job.status === "Failed" ? (
                            <button
                              className="secondary"
                              disabled={busyKey === `job-retry-${job.id}`}
                              onClick={async () => {
                                await runAction(`job-retry-${job.id}`, async () => {
                                  await api.retryIntegrationSyncJob(job.id);
                                  await reloadOverview();
                                }, `${job.jobNo} 已进入待重试。`);
                              }}
                            >
                              重试
                            </button>
                          ) : null}
                        </>
                      ) : (
                        <small>完成人：{job.completedBy || "未完成"}</small>
                      )}
                      <small>{formatDate(job.completedAtUtc ?? job.updatedAtUtc)}</small>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无同步任务" description="基于启用连接器创建任务后，这里会显示执行状态。" />
          )}

          {canManage ? (
            enabledConnectors.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!jobForm.connectorKey || !jobForm.direction) {
                    setError("请选择连接器和同步方向。");
                    return;
                  }

                  try {
                    JSON.parse(jobForm.payloadJson);
                  } catch {
                    setError("任务载荷必须是有效 JSON。");
                    return;
                  }

                  await runAction("job-create", async () => {
                    await api.createIntegrationSyncJob(jobForm);
                    setJobForm({ connectorKey: "", direction: "Outbound", payloadJson: "{}" });
                    await reloadOverview();
                  }, "同步任务已创建。");
                }}
              >
                <select value={jobForm.connectorKey} onChange={(event) => setJobForm({ ...jobForm, connectorKey: event.target.value })}>
                  <option value="">选择连接器</option>
                  {enabledConnectors.map((connector) => (
                    <option key={connector.id} value={connector.connectorKey}>{connector.connectorKey} · {connector.displayName}</option>
                  ))}
                </select>
                <select value={jobForm.direction} onChange={(event) => setJobForm({ ...jobForm, direction: event.target.value })}>
                  <option value="Outbound">出站</option>
                  <option value="Inbound">入站</option>
                </select>
                <textarea rows={4} placeholder="任务载荷 JSON" value={jobForm.payloadJson} onChange={(event) => setJobForm({ ...jobForm, payloadJson: event.target.value })} />
                <button type="submit" disabled={busyKey === "job-create" || !jobForm.connectorKey}>
                  创建同步任务
                </button>
              </form>
            ) : (
              <EmptyState title="没有启用连接器" description="先创建并启用外部连接器，再创建同步任务。" />
            )
          ) : null}
        </SectionBlock>
      </div>

      <div className="split-grid">
        <SectionBlock title="Webhook 订阅" hint="订阅保存事件键、目标地址和密钥名称。">
          {overview.webhooks.length > 0 ? (
            <div className="inventory-record-list">
              {overview.webhooks.map((webhook) => (
                <div key={webhook.id} className="inventory-record-row">
                  <div>
                    <strong>{webhook.subscriptionKey} · {webhook.displayName}</strong>
                    <p>{webhook.eventKey} · {webhook.isEnabled ? "已启用" : "已停用"}</p>
                    <small>{webhook.targetUrl}</small>
                    <small>密钥：{webhook.secretName || "未设置"}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{webhook.updatedBy || "系统"}</small>
                    <small>{formatDate(webhook.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无 Webhook" description="保存 Webhook 订阅后，外部回调目标会显示在这里。" />
          )}

          {canManage ? (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                if (!webhookForm.subscriptionKey.trim() || !webhookForm.displayName.trim() || !webhookForm.eventKey.trim() || !webhookForm.targetUrl.trim()) {
                  setError("请填写订阅编码、名称、事件键和目标地址。");
                  return;
                }

                await runAction("webhook-upsert", async () => {
                  await api.upsertIntegrationWebhook({
                    subscriptionKey: webhookForm.subscriptionKey.trim(),
                    displayName: webhookForm.displayName.trim(),
                    eventKey: webhookForm.eventKey.trim(),
                    targetUrl: webhookForm.targetUrl.trim(),
                    secretName: webhookForm.secretName.trim(),
                    isEnabled: webhookForm.isEnabled,
                  });
                  setWebhookForm({ subscriptionKey: "", displayName: "", eventKey: "", targetUrl: "", secretName: "", isEnabled: true });
                  await reloadOverview();
                }, "Webhook 订阅已保存。");
              }}
            >
              <input placeholder="订阅编码" value={webhookForm.subscriptionKey} onChange={(event) => setWebhookForm({ ...webhookForm, subscriptionKey: event.target.value })} />
              <input placeholder="订阅名称" value={webhookForm.displayName} onChange={(event) => setWebhookForm({ ...webhookForm, displayName: event.target.value })} />
              <input placeholder="事件键" value={webhookForm.eventKey} onChange={(event) => setWebhookForm({ ...webhookForm, eventKey: event.target.value })} />
              <input placeholder="目标地址" value={webhookForm.targetUrl} onChange={(event) => setWebhookForm({ ...webhookForm, targetUrl: event.target.value })} />
              <input placeholder="密钥名称" value={webhookForm.secretName} onChange={(event) => setWebhookForm({ ...webhookForm, secretName: event.target.value })} />
              <label className="checkbox-row">
                <input type="checkbox" checked={webhookForm.isEnabled} onChange={(event) => setWebhookForm({ ...webhookForm, isEnabled: event.target.checked })} />
                启用 Webhook
              </label>
              <button type="submit" disabled={busyKey === "webhook-upsert" || !webhookForm.subscriptionKey.trim() || !webhookForm.displayName.trim() || !webhookForm.eventKey.trim() || !webhookForm.targetUrl.trim()}>
                保存 Webhook
              </button>
            </form>
          ) : null}
        </SectionBlock>

        <SectionBlock title="集成审计" hint="审计记录由服务端在维护和执行动作中写入。">
          {overview.auditRecords.length > 0 ? (
            <div className="inventory-record-list">
              {overview.auditRecords.map((audit) => (
                <div key={audit.id} className="inventory-record-row">
                  <div>
                    <strong>{audit.auditNo} · {audit.action}</strong>
                    <p>{audit.category} · {audit.targetKey} · {audit.result}</p>
                    <small>{audit.message}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{audit.actor || "系统"}</small>
                    <small>{formatDate(audit.createdAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无集成审计" description="保存配置或推进同步任务后，审计记录会显示在这里。" />
          )}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
