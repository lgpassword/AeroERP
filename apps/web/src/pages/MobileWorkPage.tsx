import { RefreshCcw } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { MobileWorkOverview } from "../types/api";

const emptyOverview: MobileWorkOverview = {
  devices: [],
  offlineTasks: [],
  scanEvents: [],
  workQueue: [],
  metrics: [],
};

const moduleOptions = [
  { key: "wms", label: "WMS 执行" },
  { key: "inventory", label: "库存管理" },
  { key: "manufacturing", label: "制造管理" },
  { key: "quality", label: "质量追溯" },
  { key: "planning", label: "计划执行" },
];

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

function moduleText(key: string) {
  return moduleOptions.find((item) => item.key === key)?.label ?? key;
}

function statusText(status: string) {
  switch (status) {
    case "Pending":
      return "待同步";
    case "Synced":
      return "已同步";
    case "Completed":
      return "已完成";
    case "Failed":
      return "失败";
    case "Planned":
      return "计划中";
    case "Released":
      return "已释放";
    case "Cancelled":
      return "已取消";
    default:
      return status || "未设置";
  }
}

/** 移动作业页面，管理移动终端、离线任务、扫码事件和现场执行队列。 */
export function MobileWorkPage() {
  const { hasPermission, user } = useAuth();
  const canRead = hasPermission(platformPermissions.mobileWorkRead);
  const canManage = hasPermission(platformPermissions.mobileWorkManage);
  const canExecute = hasPermission(platformPermissions.mobileWorkExecute);
  const overviewQuery = useAsyncData(canRead ? api.getMobileWorkOverview : loadEmptyOverview);
  const overview = overviewQuery.data ?? emptyOverview;

  const [deviceForm, setDeviceForm] = useState({
    deviceCode: "",
    displayName: "",
    assignedTo: user?.displayName ?? "",
    isEnabled: true,
  });
  const [offlineForm, setOfflineForm] = useState({
    sourceModule: "wms",
    sourceTaskType: "PDA 队列",
    sourceTaskNo: "",
    payloadJson: "{}",
    assignedTo: user?.displayName ?? "",
  });
  const [scanForm, setScanForm] = useState({
    deviceCode: "",
    barcode: "",
    targetModule: "wms",
    action: "扫码记录",
    documentNo: "",
    result: "成功",
    message: "",
  });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const visibleModuleKey = user?.visibleModuleKeys.join("|") ?? "";
  const availableModuleOptions = useMemo(
    () => moduleOptions.filter((module) => user?.visibleModuleKeys.includes(module.key)),
    [visibleModuleKey],
  );
  const enabledDevices = useMemo(() => overview.devices.filter((device) => device.isEnabled), [overview.devices]);
  const openOfflineTasks = useMemo(() => overview.offlineTasks.filter((task) => task.status !== "Completed"), [overview.offlineTasks]);
  const openQueue = useMemo(() => overview.workQueue.filter((entry) => entry.status !== "Completed"), [overview.workQueue]);

  useEffect(() => {
    const firstModule = availableModuleOptions[0]?.key;
    if (!firstModule) {
      return;
    }

    if (!availableModuleOptions.some((module) => module.key === offlineForm.sourceModule)) {
      setOfflineForm((current) => ({ ...current, sourceModule: firstModule }));
    }

    if (!availableModuleOptions.some((module) => module.key === scanForm.targetModule)) {
      setScanForm((current) => ({ ...current, targetModule: firstModule }));
    }
  }, [availableModuleOptions, offlineForm.sourceModule, scanForm.targetModule]);

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
      <PageShell title="移动作业">
        <EmptyState title="无移动作业查看权限" description="当前账号不能读取移动设备、离线任务、扫码记录和移动队列。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="移动作业"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "mobile-work-refresh"}
          onClick={async () => {
            await runAction("mobile-work-refresh", reloadOverview, "移动作业数据已刷新。");
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
          { key: "enabled-devices", label: "启用设备", value: enabledDevices.length, unit: "台" },
          { key: "offline-open", label: "待同步任务", value: openOfflineTasks.length, unit: "条" },
          { key: "queue-open", label: "移动队列", value: openQueue.length, unit: "条" },
          { key: "scan-events", label: "扫码记录", value: overview.scanEvents.length, unit: "次" },
        ]).map((metric) => (
          <StatTile key={metric.key} label={`${metric.label}（${metric.unit}）`} value={metric.value} tone={metric.value > 0 ? "success" : "default"} />
        ))}
      </section>

      {overviewQuery.loading ? <div className="section-note">正在加载移动作业...</div> : null}
      {overviewQuery.error ? <div className="section-note error">{overviewQuery.error}</div> : null}

      <SectionBlock title="移动设备" hint="设备用于限定扫码和离线缓存入口。">
        <div className="inventory-surface-grid">
          <div className="inventory-surface">
            {overview.devices.length > 0 ? (
              <div className="inventory-record-list">
                {overview.devices.map((device) => (
                  <div key={device.id} className="inventory-record-row">
                    <div>
                      <strong>{device.deviceCode} · {device.displayName}</strong>
                      <p>{device.isEnabled ? "已启用" : "已停用"} · 指派：{device.assignedTo || "未指派"}</p>
                    </div>
                    <div className="inventory-record-meta">
                      <small>{device.updatedBy || "系统"}</small>
                      <small>{formatDate(device.lastSeenAtUtc)}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无移动设备" description="登记设备后，扫码记录会校验设备是否启用。" />
            )}
          </div>
          <div className="inventory-surface">
            {canManage ? (
              <form
                className="stack-form inventory-form-panel"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!deviceForm.deviceCode.trim() || !deviceForm.displayName.trim()) {
                    setError("请填写设备编码和设备名称。");
                    return;
                  }

                  await runAction("device-upsert", async () => {
                    await api.upsertMobileDevice({
                      deviceCode: deviceForm.deviceCode.trim(),
                      displayName: deviceForm.displayName.trim(),
                      assignedTo: deviceForm.assignedTo.trim(),
                      isEnabled: deviceForm.isEnabled,
                    });
                    setDeviceForm({ deviceCode: "", displayName: "", assignedTo: user?.displayName ?? "", isEnabled: true });
                    await reloadOverview();
                  }, "移动设备已保存。");
                }}
              >
                <input placeholder="设备编码" value={deviceForm.deviceCode} onChange={(event) => setDeviceForm({ ...deviceForm, deviceCode: event.target.value })} />
                <input placeholder="设备名称" value={deviceForm.displayName} onChange={(event) => setDeviceForm({ ...deviceForm, displayName: event.target.value })} />
                <input placeholder="指派给" value={deviceForm.assignedTo} onChange={(event) => setDeviceForm({ ...deviceForm, assignedTo: event.target.value })} />
                <label className="checkbox-row">
                  <input type="checkbox" checked={deviceForm.isEnabled} onChange={(event) => setDeviceForm({ ...deviceForm, isEnabled: event.target.checked })} />
                  启用设备
                </label>
                <button type="submit" disabled={busyKey === "device-upsert" || !deviceForm.deviceCode.trim() || !deviceForm.displayName.trim()}>
                  保存设备
                </button>
              </form>
            ) : (
              <EmptyState title="无设备维护权限" description="当前账号只能查看移动设备，不能登记或停用设备。" />
            )}
          </div>
        </div>
      </SectionBlock>

      <SectionBlock title="移动队列" hint="队列聚合 WMS/PDA 待办和移动离线缓存。">
        {overview.workQueue.length > 0 ? (
          <div className="inventory-record-list">
            {overview.workQueue.map((entry) => (
              <div key={`${entry.sourceModule}-${entry.id}`} className="inventory-record-row">
                <div>
                  <strong>{entry.taskNo} · {entry.taskType}</strong>
                  <p>{moduleText(entry.sourceModule)}{entry.warehouseName ? ` · ${entry.warehouseName}` : ""}</p>
                  <small>库位：{entry.locationCode || "未指定"} · 指派：{entry.assignedTo || "未指派"} · 优先级：{entry.priority}</small>
                </div>
                <div className="inventory-record-meta">
                  <small>{statusText(entry.status)}</small>
                  <small>{formatDate(entry.updatedAtUtc)}</small>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState title="暂无移动队列" description="WMS/PDA 任务或离线缓存创建后，会进入移动队列。" />
        )}
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="离线任务缓存" hint="缓存只保存移动端执行载荷和来源索引。">
          {overview.offlineTasks.length > 0 ? (
            <div className="inventory-record-list">
              {overview.offlineTasks.map((task) => (
                <div key={task.id} className="inventory-record-row">
                  <div>
                    <strong>{task.taskNo} · {task.sourceTaskType}</strong>
                    <p>{moduleText(task.sourceModule)} · 来源：{task.sourceTaskNo}</p>
                    <small>{statusText(task.status)} · 指派：{task.assignedTo || "未指派"} · 创建：{task.createdBy || "系统"}</small>
                    <div className="inventory-lines">
                      <span>{task.payloadJson}</span>
                    </div>
                  </div>
                  <div className="inventory-record-meta">
                    {canExecute && task.status !== "Completed" ? (
                      <>
                        <button
                          className="secondary"
                          disabled={busyKey === `offline-sync-${task.id}`}
                          onClick={async () => {
                            await runAction(`offline-sync-${task.id}`, async () => {
                              await api.syncMobileOfflineTask(task.id);
                              await reloadOverview();
                            }, `${task.taskNo} 已标记同步。`);
                          }}
                        >
                          标记同步
                        </button>
                        <button
                          disabled={busyKey === `offline-complete-${task.id}`}
                          onClick={async () => {
                            await runAction(`offline-complete-${task.id}`, async () => {
                              await api.completeMobileOfflineTask(task.id);
                              await reloadOverview();
                            }, `${task.taskNo} 已完成。`);
                          }}
                        >
                          完成任务
                        </button>
                      </>
                    ) : (
                      <small>完成人：{task.completedBy || "未完成"}</small>
                    )}
                    <small>{formatDate(task.completedAtUtc ?? task.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无离线任务" description="创建离线缓存后，任务会保存到移动作业插件表。" />
          )}

          {canManage ? (
            availableModuleOptions.length === 0 ? (
              <EmptyState title="暂无可选来源模块" description="当前账号没有 WMS、库存、制造、质量或计划模块访问权。" />
            ) : (
            <form
              className="stack-form"
              onSubmit={async (event) => {
                event.preventDefault();
                if (!offlineForm.sourceModule || !offlineForm.sourceTaskType.trim() || !offlineForm.sourceTaskNo.trim()) {
                  setError("请选择来源模块，并填写任务类型和来源任务号。");
                  return;
                }

                try {
                  JSON.parse(offlineForm.payloadJson);
                } catch {
                  setError("任务载荷必须是有效 JSON。");
                  return;
                }

                await runAction("offline-create", async () => {
                  await api.createMobileOfflineTask({
                    sourceModule: offlineForm.sourceModule,
                    sourceTaskType: offlineForm.sourceTaskType.trim(),
                    sourceTaskNo: offlineForm.sourceTaskNo.trim(),
                    payloadJson: offlineForm.payloadJson.trim(),
                    assignedTo: offlineForm.assignedTo.trim(),
                  });
                  setOfflineForm({ sourceModule: "wms", sourceTaskType: "PDA 队列", sourceTaskNo: "", payloadJson: "{}", assignedTo: user?.displayName ?? "" });
                  await reloadOverview();
                }, "离线任务已创建。");
              }}
            >
              <select value={offlineForm.sourceModule} onChange={(event) => setOfflineForm({ ...offlineForm, sourceModule: event.target.value })}>
                {availableModuleOptions.map((module) => (
                  <option key={module.key} value={module.key}>{module.label}</option>
                ))}
              </select>
              <input placeholder="任务类型" value={offlineForm.sourceTaskType} onChange={(event) => setOfflineForm({ ...offlineForm, sourceTaskType: event.target.value })} />
              <input placeholder="来源任务号" value={offlineForm.sourceTaskNo} onChange={(event) => setOfflineForm({ ...offlineForm, sourceTaskNo: event.target.value })} />
              <textarea rows={4} placeholder="任务载荷 JSON" value={offlineForm.payloadJson} onChange={(event) => setOfflineForm({ ...offlineForm, payloadJson: event.target.value })} />
              <input placeholder="指派给" value={offlineForm.assignedTo} onChange={(event) => setOfflineForm({ ...offlineForm, assignedTo: event.target.value })} />
              <button type="submit" disabled={busyKey === "offline-create" || !offlineForm.sourceTaskType.trim() || !offlineForm.sourceTaskNo.trim()}>
                创建离线任务
              </button>
            </form>
            )
          ) : null}
        </SectionBlock>

        <SectionBlock title="扫码记录" hint="扫码动作会保存设备、目标模块、单据号和结果。">
          {overview.scanEvents.length > 0 ? (
            <div className="inventory-record-list">
              {overview.scanEvents.map((scan) => (
                <div key={scan.id} className="inventory-record-row">
                  <div>
                    <strong>{scan.scanNo} · {scan.barcode}</strong>
                    <p>{moduleText(scan.targetModule)} · {scan.action} · {scan.result || "未设置"}</p>
                    <small>设备：{scan.deviceCode} · 单据：{scan.documentNo || "未绑定"}</small>
                    <small>{scan.message || "无备注"}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{scan.actor || "系统"}</small>
                    <small>{formatDate(scan.createdAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无扫码记录" description="登记启用设备后，可写入扫码记录。" />
          )}

          {canExecute ? (
            enabledDevices.length > 0 && availableModuleOptions.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!scanForm.deviceCode || !scanForm.barcode.trim() || !scanForm.targetModule || !scanForm.action.trim()) {
                    setError("请选择设备，并填写条码、目标模块和动作。");
                    return;
                  }

                  await runAction("scan-record", async () => {
                    await api.recordMobileScanEvent({
                      deviceCode: scanForm.deviceCode,
                      barcode: scanForm.barcode.trim(),
                      targetModule: scanForm.targetModule,
                      action: scanForm.action.trim(),
                      documentNo: scanForm.documentNo.trim(),
                      result: scanForm.result.trim(),
                      message: scanForm.message.trim(),
                    });
                    setScanForm({ deviceCode: scanForm.deviceCode, barcode: "", targetModule: "wms", action: "扫码记录", documentNo: "", result: "成功", message: "" });
                    await reloadOverview();
                  }, "扫码记录已写入。");
                }}
              >
                <select value={scanForm.deviceCode} onChange={(event) => setScanForm({ ...scanForm, deviceCode: event.target.value })}>
                  <option value="">选择启用设备</option>
                  {enabledDevices.map((device) => (
                    <option key={device.id} value={device.deviceCode}>{device.deviceCode} · {device.displayName}</option>
                  ))}
                </select>
                <input placeholder="条码" value={scanForm.barcode} onChange={(event) => setScanForm({ ...scanForm, barcode: event.target.value })} />
                <select value={scanForm.targetModule} onChange={(event) => setScanForm({ ...scanForm, targetModule: event.target.value })}>
                  {availableModuleOptions.map((module) => (
                    <option key={module.key} value={module.key}>{module.label}</option>
                  ))}
                </select>
                <input placeholder="动作" value={scanForm.action} onChange={(event) => setScanForm({ ...scanForm, action: event.target.value })} />
                <input placeholder="单据号" value={scanForm.documentNo} onChange={(event) => setScanForm({ ...scanForm, documentNo: event.target.value })} />
                <select value={scanForm.result} onChange={(event) => setScanForm({ ...scanForm, result: event.target.value })}>
                  <option value="成功">成功</option>
                  <option value="失败">失败</option>
                  <option value="待处理">待处理</option>
                </select>
                <input placeholder="备注" value={scanForm.message} onChange={(event) => setScanForm({ ...scanForm, message: event.target.value })} />
                <button type="submit" disabled={busyKey === "scan-record" || !scanForm.deviceCode || !scanForm.barcode.trim() || !scanForm.action.trim()}>
                  写入扫码记录
                </button>
              </form>
            ) : availableModuleOptions.length === 0 ? (
              <EmptyState title="暂无可选目标模块" description="当前账号没有 WMS、库存、制造、质量或计划模块访问权。" />
            ) : (
              <EmptyState title="没有启用设备" description="先登记并启用移动设备，再写入扫码记录。" />
            )
          ) : null}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
