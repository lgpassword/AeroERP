import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { Link } from "react-router-dom";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { WmsOverview } from "../types/api";

const emptyOverview: WmsOverview = {
  putAwayTasks: [],
  pickingTasks: [],
  waves: [],
  containers: [],
  routes: [],
  pdaQueue: [],
  warehouses: [],
  locations: [],
  items: [],
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
    case "Planned":
      return "计划中";
    case "Released":
      return "已释放";
    case "Completed":
      return "已完成";
    case "Cancelled":
      return "已取消";
    case "Available":
      return "可用";
    case "InUse":
      return "使用中";
    case "Locked":
      return "锁定";
    default:
      return status || "未设置";
  }
}

/** WMS 页面，管理上架、拣货、波次、容器和库内路线执行。 */
export function WmsPage() {
  const { hasPermission, user } = useAuth();
  const canReadWms = hasPermission(platformPermissions.wmsRead);
  const canManageWms = hasPermission(platformPermissions.wmsManage);
  const canExecuteWms = hasPermission(platformPermissions.wmsExecute);
  const hasMasterDataModule = user?.visibleModuleKeys.includes("master-data") ?? false;
  const hasInventoryModule = user?.visibleModuleKeys.includes("inventory") ?? false;

  const overviewQuery = useAsyncData(canReadWms ? api.getWmsOverview : loadEmptyOverview);
  const overview = overviewQuery.data ?? emptyOverview;

  const [putAwayForm, setPutAwayForm] = useState({
    warehouseId: "",
    itemId: "",
    quantity: 1,
    suggestedLocationId: "",
    containerCode: "",
    sourceDocumentNo: "",
    assignedTo: "",
  });
  const [pickingForm, setPickingForm] = useState({
    warehouseId: "",
    itemId: "",
    quantity: 1,
    sourceLocationId: "",
    assignedTo: "",
  });
  const [waveForm, setWaveForm] = useState({
    warehouseId: "",
    pickingTaskIds: [] as string[],
  });
  const [containerForm, setContainerForm] = useState({
    code: "",
    containerType: "托盘",
    warehouseId: "",
    currentLocationId: "",
    status: "Available",
  });
  const [routeForm, setRouteForm] = useState({
    warehouseId: "",
    fromLocationId: "",
    toLocationId: "",
    distanceMeters: 10,
    priority: 10,
    isEnabled: true,
  });
  const [putAwayTargets, setPutAwayTargets] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const openPutAway = useMemo(
    () => overview.putAwayTasks.filter((task) => task.status !== "Completed"),
    [overview.putAwayTasks],
  );
  const openPicking = useMemo(
    () => overview.pickingTasks.filter((task) => task.status !== "Completed"),
    [overview.pickingTasks],
  );
  const activeQueue = useMemo(
    () => overview.pdaQueue.filter((entry) => entry.status !== "Completed"),
    [overview.pdaQueue],
  );
  const waveCandidates = useMemo(
    () =>
      overview.pickingTasks.filter(
        (task) =>
          task.warehouseId === waveForm.warehouseId &&
          task.status !== "Completed" &&
          !task.waveId,
      ),
    [overview.pickingTasks, waveForm.warehouseId],
  );

  const missingMasterData = overview.warehouses.length === 0 || overview.items.length === 0;
  const locationsForWarehouse = (warehouseId: string) =>
    overview.locations.filter((location) => location.warehouseId === warehouseId);

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
    if (canReadWms) {
      await overviewQuery.reload();
    }
  }

  if (!canReadWms) {
    return (
      <PageShell title="WMS 执行">
        <EmptyState title="无 WMS 查看权限" description="当前账号不能读取 WMS 执行任务、波次、容器、路径和 PDA 队列。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="WMS 执行"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "wms-refresh"}
          onClick={async () => {
            await runAction("wms-refresh", reloadOverview, "WMS 数据已刷新。");
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
        <StatTile label="待上架" value={openPutAway.length} tone={openPutAway.length > 0 ? "warning" : "success"} />
        <StatTile label="待拣货" value={openPicking.length} tone={openPicking.length > 0 ? "warning" : "success"} />
        <StatTile label="波次数" value={overview.waves.length} tone={overview.waves.length > 0 ? "success" : "default"} />
        <StatTile label="PDA 待办" value={activeQueue.length} tone={activeQueue.length > 0 ? "warning" : "success"} />
      </section>

      {overviewQuery.loading ? <div className="section-note">正在加载 WMS 概览...</div> : null}
      {overviewQuery.error ? <div className="section-note error">{overviewQuery.error}</div> : null}

      <SectionBlock title="上架任务" hint="上架只管理仓储执行任务，不复制库存余额；库存数量仍由库存模块维护。">
        <div className="inventory-surface-grid">
          <div className="inventory-surface">
            {overview.putAwayTasks.length > 0 ? (
              <div className="table-shell">
                {overview.putAwayTasks.map((task) => {
                  const targetLocations = locationsForWarehouse(task.warehouseId);
                  const selectedTarget = putAwayTargets[task.id] ?? task.suggestedLocationId ?? "";
                  return (
                    <div key={task.id} className="review-card inventory-card">
                      <div>
                        <strong>{task.taskNo} · {task.itemCode} · {task.itemName}</strong>
                        <p>{task.warehouseName} · {statusText(task.status)}</p>
                        <small>数量：{task.quantity} {task.unit} · 指派：{task.assignedTo || "未指派"}</small>
                        <small>建议库位：{task.suggestedLocationName || "未指定"} · 容器：{task.containerCode || "未绑定"}</small>
                        <small>来源单据：{task.sourceDocumentNo || "未填写"} · {formatDate(task.updatedAtUtc)}</small>
                      </div>
                      {canExecuteWms && task.status !== "Completed" ? (
                        targetLocations.length > 0 ? (
                          <div className="inventory-actions">
                            <select
                              value={selectedTarget}
                              onChange={(event) => setPutAwayTargets({ ...putAwayTargets, [task.id]: event.target.value })}
                            >
                              <option value="">选择目标库位</option>
                              {targetLocations.map((location) => (
                                <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                              ))}
                            </select>
                            <button
                              disabled={busyKey === `put-away-complete-${task.id}` || !selectedTarget}
                              onClick={async () => {
                                await runAction(`put-away-complete-${task.id}`, async () => {
                                  await api.completeWmsPutAwayTask(task.id, { targetLocationId: selectedTarget });
                                  setPutAwayTargets((current) => {
                                    const next = { ...current };
                                    delete next[task.id];
                                    return next;
                                  });
                                  await reloadOverview();
                                }, `${task.taskNo} 已完成上架。`);
                              }}
                            >
                              完成上架
                            </button>
                          </div>
                        ) : (
                          <small>该仓库暂无可用库位，不能完成上架。</small>
                        )
                      ) : task.status === "Completed" ? (
                        <small>完成人：{task.completedBy || "系统"} · {formatDate(task.completedAtUtc)}</small>
                      ) : null}
                    </div>
                  );
                })}
              </div>
            ) : (
              <EmptyState title="暂无上架任务" description="创建上架任务后，这里会显示待执行和已完成记录。" />
            )}
          </div>
          <div className="inventory-surface">
            {canManageWms ? (
              missingMasterData ? (
                <EmptyState
                  title="缺少仓库或物料"
                  description="创建上架任务需要启用仓库和物料。"
                  action={hasMasterDataModule ? <Link to="/master-data"><button type="button">去主数据</button></Link> : undefined}
                />
              ) : (
                <form
                  className="stack-form inventory-form-panel"
                  onSubmit={async (event) => {
                    event.preventDefault();
                    if (!putAwayForm.warehouseId || !putAwayForm.itemId || putAwayForm.quantity <= 0) {
                      setError("请选择仓库、物料并填写有效上架数量。");
                      return;
                    }

                    await runAction("put-away-create", async () => {
                      await api.createWmsPutAwayTask({
                        warehouseId: putAwayForm.warehouseId,
                        itemId: putAwayForm.itemId,
                        quantity: putAwayForm.quantity,
                        suggestedLocationId: putAwayForm.suggestedLocationId || null,
                        containerCode: putAwayForm.containerCode.trim(),
                        sourceDocumentNo: putAwayForm.sourceDocumentNo.trim(),
                        assignedTo: putAwayForm.assignedTo.trim(),
                      });
                      setPutAwayForm({ warehouseId: "", itemId: "", quantity: 1, suggestedLocationId: "", containerCode: "", sourceDocumentNo: "", assignedTo: "" });
                      await reloadOverview();
                    }, "上架任务已创建。");
                  }}
                >
                  <select value={putAwayForm.warehouseId} onChange={(event) => setPutAwayForm({ ...putAwayForm, warehouseId: event.target.value, suggestedLocationId: "" })}>
                    <option value="">选择仓库</option>
                    {overview.warehouses.map((warehouse) => (
                      <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                    ))}
                  </select>
                  <select value={putAwayForm.itemId} onChange={(event) => setPutAwayForm({ ...putAwayForm, itemId: event.target.value })}>
                    <option value="">选择物料</option>
                    {overview.items.map((item) => (
                      <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                    ))}
                  </select>
                  <input type="number" min={0.0001} step="0.0001" value={putAwayForm.quantity} onChange={(event) => setPutAwayForm({ ...putAwayForm, quantity: Number(event.target.value) })} />
                  <select value={putAwayForm.suggestedLocationId} disabled={!putAwayForm.warehouseId} onChange={(event) => setPutAwayForm({ ...putAwayForm, suggestedLocationId: event.target.value })}>
                    <option value="">可选：建议上架库位</option>
                    {locationsForWarehouse(putAwayForm.warehouseId).map((location) => (
                      <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                    ))}
                  </select>
                  <input placeholder="容器编码" value={putAwayForm.containerCode} onChange={(event) => setPutAwayForm({ ...putAwayForm, containerCode: event.target.value })} />
                  <input placeholder="来源单据号" value={putAwayForm.sourceDocumentNo} onChange={(event) => setPutAwayForm({ ...putAwayForm, sourceDocumentNo: event.target.value })} />
                  <input placeholder="指派给" value={putAwayForm.assignedTo} onChange={(event) => setPutAwayForm({ ...putAwayForm, assignedTo: event.target.value })} />
                  <button type="submit" disabled={busyKey === "put-away-create" || !putAwayForm.warehouseId || !putAwayForm.itemId || putAwayForm.quantity <= 0}>
                    创建上架任务
                  </button>
                </form>
              )
            ) : (
              <EmptyState title="无上架维护权限" description="当前账号只能查看 WMS 上架任务，不能创建任务。" />
            )}
          </div>
        </div>
      </SectionBlock>

      <SectionBlock title="拣货任务与波次" hint="拣货任务可以先单独创建，再按同仓库规则组成波次并释放到 PDA 队列。">
        <div className="inventory-surface-grid">
          <div className="inventory-surface">
            {overview.pickingTasks.length > 0 ? (
              <div className="table-shell">
                {overview.pickingTasks.map((task) => (
                  <div key={task.id} className="review-card inventory-card">
                    <div>
                      <strong>{task.taskNo} · {task.itemCode} · {task.itemName}</strong>
                      <p>{task.warehouseName} · {statusText(task.status)} · {task.waveNo || "未组波"}</p>
                      <small>数量：{task.quantity} {task.unit} · 来源库位：{task.sourceLocationName || "未指定"}</small>
                      <small>指派：{task.assignedTo || "未指派"} · {formatDate(task.updatedAtUtc)}</small>
                    </div>
                    {canExecuteWms && task.status !== "Completed" ? (
                      <div className="inventory-actions">
                        <button
                          disabled={busyKey === `picking-complete-${task.id}`}
                          onClick={async () => {
                            await runAction(`picking-complete-${task.id}`, async () => {
                              await api.completeWmsPickingTask(task.id, { note: "页面完成拣货" });
                              await reloadOverview();
                            }, `${task.taskNo} 已完成拣货。`);
                          }}
                        >
                          完成拣货
                        </button>
                      </div>
                    ) : task.status === "Completed" ? (
                      <small>完成人：{task.completedBy || "系统"} · {formatDate(task.completedAtUtc)}</small>
                    ) : null}
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无拣货任务" description="创建拣货任务后，可组波并释放到 PDA 队列。" />
            )}
          </div>
          <div className="inventory-surface">
            {canManageWms ? (
              missingMasterData ? (
                <EmptyState
                  title="缺少仓库或物料"
                  description="创建拣货任务需要启用仓库和物料。"
                  action={hasMasterDataModule ? <Link to="/master-data"><button type="button">去主数据</button></Link> : undefined}
                />
              ) : (
                <form
                  className="stack-form inventory-form-panel"
                  onSubmit={async (event) => {
                    event.preventDefault();
                    if (!pickingForm.warehouseId || !pickingForm.itemId || pickingForm.quantity <= 0) {
                      setError("请选择仓库、物料并填写有效拣货数量。");
                      return;
                    }

                    await runAction("picking-create", async () => {
                      await api.createWmsPickingTask({
                        warehouseId: pickingForm.warehouseId,
                        itemId: pickingForm.itemId,
                        quantity: pickingForm.quantity,
                        sourceLocationId: pickingForm.sourceLocationId || null,
                        assignedTo: pickingForm.assignedTo.trim(),
                      });
                      setPickingForm({ warehouseId: "", itemId: "", quantity: 1, sourceLocationId: "", assignedTo: "" });
                      await reloadOverview();
                    }, "拣货任务已创建。");
                  }}
                >
                  <select value={pickingForm.warehouseId} onChange={(event) => setPickingForm({ ...pickingForm, warehouseId: event.target.value, sourceLocationId: "" })}>
                    <option value="">选择仓库</option>
                    {overview.warehouses.map((warehouse) => (
                      <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                    ))}
                  </select>
                  <select value={pickingForm.itemId} onChange={(event) => setPickingForm({ ...pickingForm, itemId: event.target.value })}>
                    <option value="">选择物料</option>
                    {overview.items.map((item) => (
                      <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                    ))}
                  </select>
                  <input type="number" min={0.0001} step="0.0001" value={pickingForm.quantity} onChange={(event) => setPickingForm({ ...pickingForm, quantity: Number(event.target.value) })} />
                  <select value={pickingForm.sourceLocationId} disabled={!pickingForm.warehouseId} onChange={(event) => setPickingForm({ ...pickingForm, sourceLocationId: event.target.value })}>
                    <option value="">可选：来源拣货库位</option>
                    {locationsForWarehouse(pickingForm.warehouseId).map((location) => (
                      <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                    ))}
                  </select>
                  <input placeholder="指派给" value={pickingForm.assignedTo} onChange={(event) => setPickingForm({ ...pickingForm, assignedTo: event.target.value })} />
                  <button type="submit" disabled={busyKey === "picking-create" || !pickingForm.warehouseId || !pickingForm.itemId || pickingForm.quantity <= 0}>
                    创建拣货任务
                  </button>
                </form>
              )
            ) : (
              <EmptyState title="无拣货维护权限" description="当前账号只能查看 WMS 拣货任务，不能创建任务。" />
            )}
          </div>
        </div>
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="波次管理" hint="同一仓库的未完成、未组波拣货任务可以合并为一个波次。">
          {overview.waves.length > 0 ? (
            <div className="inventory-record-list">
              {overview.waves.map((wave) => {
                const waveTasks = overview.pickingTasks.filter((task) => task.waveId === wave.id);
                return (
                  <div key={wave.id} className="inventory-record-row">
                    <div>
                      <strong>{wave.waveNo}</strong>
                      <p>{wave.warehouseName} · {statusText(wave.status)}</p>
                      <small>{waveTasks.map((task) => task.taskNo).join("，") || "暂无任务"} · {formatDate(wave.updatedAtUtc)}</small>
                    </div>
                    <div className="inventory-record-meta">
                      {canExecuteWms && wave.status === "Planned" ? (
                        <button
                          disabled={busyKey === `wave-release-${wave.id}`}
                          onClick={async () => {
                            await runAction(`wave-release-${wave.id}`, async () => {
                              await api.releaseWmsWave(wave.id);
                              await reloadOverview();
                            }, `${wave.waveNo} 已释放。`);
                          }}
                        >
                          释放波次
                        </button>
                      ) : (
                        <small>释放人：{wave.releasedBy || "未释放"}</small>
                      )}
                      <small>{formatDate(wave.releasedAtUtc)}</small>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无波次" description="将拣货任务组波后，这里会显示波次状态。" />
          )}

          {canManageWms ? (
            overview.warehouses.length === 0 ? (
              <EmptyState title="缺少仓库" description="创建波次需要启用仓库。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!waveForm.warehouseId || waveForm.pickingTaskIds.length === 0) {
                    setError("请选择仓库和至少一个未组波拣货任务。");
                    return;
                  }

                  await runAction("wave-create", async () => {
                    await api.createWmsWave(waveForm);
                    setWaveForm({ warehouseId: "", pickingTaskIds: [] });
                    await reloadOverview();
                  }, "拣货波次已创建。");
                }}
              >
                <select value={waveForm.warehouseId} onChange={(event) => setWaveForm({ warehouseId: event.target.value, pickingTaskIds: [] })}>
                  <option value="">选择波次仓库</option>
                  {overview.warehouses.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                  ))}
                </select>
                {waveForm.warehouseId && waveCandidates.length > 0 ? (
                  <div className="inventory-lines">
                    {waveCandidates.map((task) => (
                      <label key={task.id} className="checkbox-row compact">
                        <input
                          type="checkbox"
                          checked={waveForm.pickingTaskIds.includes(task.id)}
                          onChange={(event) => {
                            setWaveForm((current) => ({
                              ...current,
                              pickingTaskIds: event.target.checked
                                ? [...current.pickingTaskIds, task.id]
                                : current.pickingTaskIds.filter((id) => id !== task.id),
                            }));
                          }}
                        />
                        <span>{task.taskNo} · {task.itemCode} x {task.quantity}</span>
                      </label>
                    ))}
                  </div>
                ) : (
                  <div className="section-note">选择仓库后，可勾选该仓库未组波的拣货任务。</div>
                )}
                <button type="submit" disabled={busyKey === "wave-create" || !waveForm.warehouseId || waveForm.pickingTaskIds.length === 0}>
                  创建波次
                </button>
              </form>
            )
          ) : null}
        </SectionBlock>

        <SectionBlock title="PDA 作业队列" hint="队列由上架、拣货和波次释放自动驱动，不在前端手工伪造任务。">
          {overview.pdaQueue.length > 0 ? (
            <div className="inventory-record-list">
              {overview.pdaQueue.map((entry) => (
                <div key={entry.id} className="inventory-record-row">
                  <div>
                    <strong>{entry.taskNo} · {entry.taskType}</strong>
                    <p>{entry.warehouseName}{entry.locationCode ? ` · ${entry.locationCode}` : ""}</p>
                    <small>指派：{entry.assignedTo || "未指派"} · 优先级：{entry.priority}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{statusText(entry.status)}</small>
                    <small>{formatDate(entry.completedAtUtc ?? entry.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无 PDA 队列" description="创建上架或拣货任务后，系统会生成真实 PDA 作业队列。" />
          )}
        </SectionBlock>
      </div>

      <SectionBlock title="容器与库内路径" hint="容器和路径独立维护，供仓储执行调度使用；库存结存仍由库存模块负责。">
        <div className="inventory-surface-grid">
          <div className="inventory-surface">
            {overview.containers.length > 0 ? (
              <div className="inventory-record-list">
                {overview.containers.map((container) => (
                  <div key={container.id} className="inventory-record-row">
                    <div>
                      <strong>{container.code} · {container.containerType || "未分类"}</strong>
                      <p>{container.warehouseName} · {container.currentLocationName || "未定位"}</p>
                    </div>
                    <div className="inventory-record-meta">
                      <small>{statusText(container.status)}</small>
                      <small>{container.lastHandledBy || "未处理"} · {formatDate(container.updatedAtUtc)}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无容器" description="维护托盘、周转箱等容器后，这里会显示当前位置和状态。" />
            )}

            {canManageWms ? (
              missingMasterData ? (
                <EmptyState
                  title="缺少仓库"
                  description="维护容器需要启用仓库。"
                  action={hasMasterDataModule ? <Link to="/master-data"><button type="button">去主数据</button></Link> : undefined}
                />
              ) : (
                <form
                  className="stack-form inventory-form-panel"
                  onSubmit={async (event) => {
                    event.preventDefault();
                    if (!containerForm.code.trim() || !containerForm.containerType.trim() || !containerForm.warehouseId) {
                      setError("请填写容器编码、类型并选择仓库。");
                      return;
                    }

                    await runAction("container-upsert", async () => {
                      await api.upsertWmsContainer({
                        code: containerForm.code.trim(),
                        containerType: containerForm.containerType.trim(),
                        warehouseId: containerForm.warehouseId,
                        currentLocationId: containerForm.currentLocationId || null,
                        status: containerForm.status,
                      });
                      setContainerForm({ code: "", containerType: "托盘", warehouseId: "", currentLocationId: "", status: "Available" });
                      await reloadOverview();
                    }, "容器已保存。");
                  }}
                >
                  <input placeholder="容器编码" value={containerForm.code} onChange={(event) => setContainerForm({ ...containerForm, code: event.target.value })} />
                  <input placeholder="容器类型" value={containerForm.containerType} onChange={(event) => setContainerForm({ ...containerForm, containerType: event.target.value })} />
                  <select value={containerForm.warehouseId} onChange={(event) => setContainerForm({ ...containerForm, warehouseId: event.target.value, currentLocationId: "" })}>
                    <option value="">选择仓库</option>
                    {overview.warehouses.map((warehouse) => (
                      <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                    ))}
                  </select>
                  <select value={containerForm.currentLocationId} disabled={!containerForm.warehouseId} onChange={(event) => setContainerForm({ ...containerForm, currentLocationId: event.target.value })}>
                    <option value="">可选：当前库位</option>
                    {locationsForWarehouse(containerForm.warehouseId).map((location) => (
                      <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                    ))}
                  </select>
                  <select value={containerForm.status} onChange={(event) => setContainerForm({ ...containerForm, status: event.target.value })}>
                    <option value="Available">可用</option>
                    <option value="InUse">使用中</option>
                    <option value="Locked">锁定</option>
                  </select>
                  <button type="submit" disabled={busyKey === "container-upsert" || !containerForm.code.trim() || !containerForm.containerType.trim() || !containerForm.warehouseId}>
                    保存容器
                  </button>
                </form>
              )
            ) : null}
          </div>

          <div className="inventory-surface">
            {overview.routes.length > 0 ? (
              <div className="inventory-record-list">
                {overview.routes.map((route) => (
                  <div key={route.id} className="inventory-record-row">
                    <div>
                      <strong>{route.fromLocationName} → {route.toLocationName}</strong>
                      <p>{route.warehouseName} · {route.distanceMeters} 米</p>
                    </div>
                    <div className="inventory-record-meta">
                      <small>{route.isEnabled ? "已启用" : "已停用"}</small>
                      <small>优先级：{route.priority}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无库内路径" description="维护库位间路径后，可用于后续拣货路径优化。" />
            )}

            {canManageWms ? (
              overview.locations.length < 2 ? (
                <EmptyState
                  title="缺少库位"
                  description="维护路径至少需要同一仓库下两个启用库位。"
                  action={hasInventoryModule ? <Link to="/inventory?panel=location"><button type="button">去库位管理</button></Link> : undefined}
                />
              ) : (
                <form
                  className="stack-form inventory-form-panel"
                  onSubmit={async (event) => {
                    event.preventDefault();
                    if (!routeForm.warehouseId || !routeForm.fromLocationId || !routeForm.toLocationId || routeForm.fromLocationId === routeForm.toLocationId || routeForm.distanceMeters <= 0) {
                      setError("请选择仓库、起止库位，并填写大于 0 的路径距离。");
                      return;
                    }

                    await runAction("route-upsert", async () => {
                      await api.upsertWmsRoute(routeForm);
                      setRouteForm({ warehouseId: "", fromLocationId: "", toLocationId: "", distanceMeters: 10, priority: 10, isEnabled: true });
                      await reloadOverview();
                    }, "库内路径已保存。");
                  }}
                >
                  <select value={routeForm.warehouseId} onChange={(event) => setRouteForm({ ...routeForm, warehouseId: event.target.value, fromLocationId: "", toLocationId: "" })}>
                    <option value="">选择仓库</option>
                    {overview.warehouses.map((warehouse) => (
                      <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                    ))}
                  </select>
                  <select value={routeForm.fromLocationId} disabled={!routeForm.warehouseId} onChange={(event) => setRouteForm({ ...routeForm, fromLocationId: event.target.value })}>
                    <option value="">选择起点库位</option>
                    {locationsForWarehouse(routeForm.warehouseId).map((location) => (
                      <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                    ))}
                  </select>
                  <select value={routeForm.toLocationId} disabled={!routeForm.warehouseId} onChange={(event) => setRouteForm({ ...routeForm, toLocationId: event.target.value })}>
                    <option value="">选择终点库位</option>
                    {locationsForWarehouse(routeForm.warehouseId).map((location) => (
                      <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                    ))}
                  </select>
                  <div className="inline-form">
                    <input type="number" min={0.01} step="0.01" value={routeForm.distanceMeters} onChange={(event) => setRouteForm({ ...routeForm, distanceMeters: Number(event.target.value) })} />
                    <input type="number" min={0} step={1} value={routeForm.priority} onChange={(event) => setRouteForm({ ...routeForm, priority: Number(event.target.value) })} />
                  </div>
                  <label className="checkbox-row">
                    <input type="checkbox" checked={routeForm.isEnabled} onChange={(event) => setRouteForm({ ...routeForm, isEnabled: event.target.checked })} />
                    启用路径
                  </label>
                  <button type="submit" disabled={busyKey === "route-upsert" || !routeForm.warehouseId || !routeForm.fromLocationId || !routeForm.toLocationId || routeForm.fromLocationId === routeForm.toLocationId || routeForm.distanceMeters <= 0}>
                    保存路径
                  </button>
                </form>
              )
            ) : null}
          </div>
        </div>
      </SectionBlock>
    </PageShell>
  );
}
