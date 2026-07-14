import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { Link } from "react-router-dom";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { AdvancedManufacturingOverview } from "../types/api";

const emptyOverview: AdvancedManufacturingOverview = {
  workCenters: [],
  routings: [],
  operationSchedules: [],
  capacityLoads: [],
  costSnapshots: [],
  mrpSuggestions: [],
  warehouses: [],
  items: [],
  workOrders: [],
};

const loadEmptyOverview = () => Promise.resolve(emptyOverview);

function formatDate(value: string) {
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
    case "Draft":
      return "草稿";
    case "Active":
      return "已启用";
    case "Planned":
      return "计划中";
    case "Released":
      return "已释放";
    case "Completed":
      return "已完成";
    case "Open":
      return "待处理";
    case "Accepted":
      return "已采纳";
    case "Ignored":
      return "已忽略";
    default:
      return status || "未设置";
  }
}

function toIso(value: string) {
  return new Date(value).toISOString();
}

/** 高级制造页面，管理工作中心、工艺路线、工序排程、产能负载、成本快照和 MRP 建议。 */
export function AdvancedManufacturingPage() {
  const { hasPermission, user } = useAuth();
  const canRead = hasPermission(platformPermissions.advancedManufacturingRead);
  const canManage = hasPermission(platformPermissions.advancedManufacturingManage);
  const canSchedule = hasPermission(platformPermissions.advancedManufacturingSchedule);
  const canManageCost = hasPermission(platformPermissions.advancedManufacturingCostManage);
  const canManageMrp = hasPermission(platformPermissions.advancedManufacturingMrpManage);
  const hasMasterDataModule = user?.visibleModuleKeys.includes("master-data") ?? false;
  const hasManufacturingModule = user?.visibleModuleKeys.includes("manufacturing") ?? false;

  const overviewQuery = useAsyncData(canRead ? api.getAdvancedManufacturingOverview : loadEmptyOverview);
  const overview = overviewQuery.data ?? emptyOverview;

  const [workCenterForm, setWorkCenterForm] = useState({
    code: "",
    name: "",
    warehouseId: "",
    capacityMinutesPerDay: 480,
    hourlyCostRate: 0,
    isEnabled: true,
  });
  const [routingForm, setRoutingForm] = useState({
    finishedItemId: "",
    version: "V1",
    operations: [
      { sequence: 10, operationCode: "", operationName: "", workCenterId: "", standardMinutes: 30, laborCostRate: 0, machineCostRate: 0 },
    ],
  });
  const [scheduleForm, setScheduleForm] = useState({
    workOrderId: "",
    routingOperationId: "",
    plannedStartUtc: "",
    plannedEndUtc: "",
    plannedQuantity: 1,
  });
  const [completeQuantities, setCompleteQuantities] = useState<Record<string, number>>({});
  const [capacityForm, setCapacityForm] = useState({
    workCenterId: "",
    planDate: "",
    availableMinutes: 480,
    reservedMinutes: 0,
    sourceDocumentNo: "",
  });
  const [costForm, setCostForm] = useState({
    workOrderId: "",
    materialCost: 0,
    laborCost: 0,
    machineCost: 0,
    overheadCost: 0,
  });
  const [mrpForm, setMrpForm] = useState({
    warehouseId: "",
    itemId: "",
    demandQuantity: 1,
    supplyQuantity: 0,
    sourceType: "生产需求",
  });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const activeRoutings = useMemo(() => overview.routings.filter((routing) => routing.status === "Active"), [overview.routings]);
  const availableOperations = useMemo(
    () =>
      activeRoutings.flatMap((routing) =>
        routing.operations.map((operation) => ({
          ...operation,
          routingNo: routing.routingNo,
          finishedItemName: routing.finishedItemName,
        })),
      ),
    [activeRoutings],
  );
  const openSchedules = useMemo(
    () => overview.operationSchedules.filter((schedule) => schedule.status !== "Completed"),
    [overview.operationSchedules],
  );
  const openMrp = useMemo(
    () => overview.mrpSuggestions.filter((suggestion) => suggestion.status === "Open"),
    [overview.mrpSuggestions],
  );
  const missingMasterData = overview.warehouses.length === 0 || overview.items.length === 0;

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
      <PageShell title="高级制造">
        <EmptyState title="无高级制造查看权限" description="当前账号不能读取工艺路线、工序计划、产能、成本和 MRP 信息。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="高级制造"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "advanced-manufacturing-refresh"}
          onClick={async () => {
            await runAction("advanced-manufacturing-refresh", reloadOverview, "高级制造数据已刷新。");
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
        <StatTile label="工作中心" value={overview.workCenters.length} tone={overview.workCenters.length > 0 ? "success" : "default"} />
        <StatTile label="启用路线" value={activeRoutings.length} tone={activeRoutings.length > 0 ? "success" : "default"} />
        <StatTile label="待执行工序" value={openSchedules.length} tone={openSchedules.length > 0 ? "warning" : "success"} />
        <StatTile label="待处理 MRP" value={openMrp.length} tone={openMrp.length > 0 ? "warning" : "success"} />
      </section>

      {overviewQuery.loading ? <div className="section-note">正在加载高级制造概览...</div> : null}
      {overviewQuery.error ? <div className="section-note error">{overviewQuery.error}</div> : null}

      <div className="split-grid">
        <SectionBlock title="工作中心" hint="维护工作中心产能和成本参数，仓库仍来自主数据。">
          {overview.workCenters.length > 0 ? (
            <div className="inventory-record-list">
              {overview.workCenters.map((center) => (
                <div key={center.id} className="inventory-record-row">
                  <div>
                    <strong>{center.code} · {center.name}</strong>
                    <p>{center.warehouseName} · {center.isEnabled ? "启用" : "停用"}</p>
                    <small>日产能 {center.capacityMinutesPerDay} 分钟 · 小时成本 {center.hourlyCostRate}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{center.updatedBy || "系统"}</small>
                    <small>{formatDate(center.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无工作中心" description="创建工作中心后，可用于工艺路线和产能排程。" />
          )}

          {canManage ? (
            missingMasterData ? (
              <EmptyState
                title="缺少仓库或物料"
                description="维护高级制造前需要先准备启用仓库和物料。"
                action={hasMasterDataModule ? <Link to="/master-data"><button type="button">去主数据</button></Link> : undefined}
              />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!workCenterForm.code.trim() || !workCenterForm.name.trim() || !workCenterForm.warehouseId || workCenterForm.capacityMinutesPerDay <= 0 || workCenterForm.hourlyCostRate < 0) {
                    setError("请完整填写工作中心编码、名称、仓库、产能和成本。");
                    return;
                  }

                  await runAction("work-center-upsert", async () => {
                    await api.upsertWorkCenter(workCenterForm);
                    setWorkCenterForm({ code: "", name: "", warehouseId: "", capacityMinutesPerDay: 480, hourlyCostRate: 0, isEnabled: true });
                    await reloadOverview();
                  }, "工作中心已保存。");
                }}
              >
                <input placeholder="工作中心编码" value={workCenterForm.code} onChange={(event) => setWorkCenterForm({ ...workCenterForm, code: event.target.value })} />
                <input placeholder="工作中心名称" value={workCenterForm.name} onChange={(event) => setWorkCenterForm({ ...workCenterForm, name: event.target.value })} />
                <select value={workCenterForm.warehouseId} onChange={(event) => setWorkCenterForm({ ...workCenterForm, warehouseId: event.target.value })}>
                  <option value="">选择所属仓库</option>
                  {overview.warehouses.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                  ))}
                </select>
                <div className="inline-form">
                  <input type="number" min={1} step="1" value={workCenterForm.capacityMinutesPerDay} onChange={(event) => setWorkCenterForm({ ...workCenterForm, capacityMinutesPerDay: Number(event.target.value) })} />
                  <input type="number" min={0} step="0.01" value={workCenterForm.hourlyCostRate} onChange={(event) => setWorkCenterForm({ ...workCenterForm, hourlyCostRate: Number(event.target.value) })} />
                </div>
                <label className="checkbox-row">
                  <input type="checkbox" checked={workCenterForm.isEnabled} onChange={(event) => setWorkCenterForm({ ...workCenterForm, isEnabled: event.target.checked })} />
                  启用工作中心
                </label>
                <button type="submit" disabled={busyKey === "work-center-upsert"}>保存工作中心</button>
              </form>
            )
          ) : null}
        </SectionBlock>

        <SectionBlock title="工艺路线" hint="路线绑定成品物料和工作中心，不复制 BOM 或物料主数据。">
          {overview.routings.length > 0 ? (
            <div className="inventory-record-list">
              {overview.routings.map((routing) => (
                <div key={routing.id} className="inventory-record-row">
                  <div>
                    <strong>{routing.routingNo} · {routing.finishedItemCode} · {routing.finishedItemName}</strong>
                    <p>{routing.version} · {statusText(routing.status)}</p>
                    <div className="inventory-lines">
                      {routing.operations.map((operation) => (
                        <span key={operation.id}>{operation.sequence} · {operation.operationCode} · {operation.workCenterCode}</span>
                      ))}
                    </div>
                  </div>
                  <div className="inventory-record-meta">
                    {canManage && routing.status === "Draft" ? (
                      <button
                        disabled={busyKey === `routing-activate-${routing.id}`}
                        onClick={async () => {
                          await runAction(`routing-activate-${routing.id}`, async () => {
                            await api.activateManufacturingRouting(routing.id);
                            await reloadOverview();
                          }, `${routing.routingNo} 已启用。`);
                        }}
                      >
                        启用路线
                      </button>
                    ) : (
                      <small>{routing.createdBy}</small>
                    )}
                    <small>{formatDate(routing.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无工艺路线" description="创建并启用路线后，可用于工序计划排程。" />
          )}

          {canManage ? (
            overview.items.length === 0 || overview.workCenters.length === 0 ? (
              <EmptyState title="缺少物料或工作中心" description="创建路线需要启用物料和至少一个工作中心。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!routingForm.finishedItemId || !routingForm.version.trim() || routingForm.operations.some((operation) => !operation.operationCode.trim() || !operation.operationName.trim() || !operation.workCenterId || operation.sequence <= 0 || operation.standardMinutes <= 0)) {
                    setError("请完整填写路线物料、版本和工序。");
                    return;
                  }

                  await runAction("routing-create", async () => {
                    await api.createManufacturingRouting(routingForm);
                    setRoutingForm({
                      finishedItemId: "",
                      version: "V1",
                      operations: [{ sequence: 10, operationCode: "", operationName: "", workCenterId: "", standardMinutes: 30, laborCostRate: 0, machineCostRate: 0 }],
                    });
                    await reloadOverview();
                  }, "工艺路线已创建。");
                }}
              >
                <select value={routingForm.finishedItemId} onChange={(event) => setRoutingForm({ ...routingForm, finishedItemId: event.target.value })}>
                  <option value="">选择成品物料</option>
                  {overview.items.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <input placeholder="路线版本" value={routingForm.version} onChange={(event) => setRoutingForm({ ...routingForm, version: event.target.value })} />
                {routingForm.operations.map((operation, index) => (
                  <div key={index} className="stack-form">
                    <div className="inline-form">
                      <input type="number" min={1} step="1" value={operation.sequence} onChange={(event) => {
                        const operations = [...routingForm.operations];
                        operations[index] = { ...operation, sequence: Number(event.target.value) };
                        setRoutingForm({ ...routingForm, operations });
                      }} />
                      <input placeholder="工序编码" value={operation.operationCode} onChange={(event) => {
                        const operations = [...routingForm.operations];
                        operations[index] = { ...operation, operationCode: event.target.value };
                        setRoutingForm({ ...routingForm, operations });
                      }} />
                    </div>
                    <input placeholder="工序名称" value={operation.operationName} onChange={(event) => {
                      const operations = [...routingForm.operations];
                      operations[index] = { ...operation, operationName: event.target.value };
                      setRoutingForm({ ...routingForm, operations });
                    }} />
                    <select value={operation.workCenterId} onChange={(event) => {
                      const operations = [...routingForm.operations];
                      operations[index] = { ...operation, workCenterId: event.target.value };
                      setRoutingForm({ ...routingForm, operations });
                    }}>
                      <option value="">选择工作中心</option>
                      {overview.workCenters.filter((center) => center.isEnabled).map((center) => (
                        <option key={center.id} value={center.id}>{center.code} · {center.name}</option>
                      ))}
                    </select>
                    <div className="inline-form">
                      <input type="number" min={0.0001} step="0.0001" value={operation.standardMinutes} onChange={(event) => {
                        const operations = [...routingForm.operations];
                        operations[index] = { ...operation, standardMinutes: Number(event.target.value) };
                        setRoutingForm({ ...routingForm, operations });
                      }} />
                      <input type="number" min={0} step="0.01" value={operation.laborCostRate} onChange={(event) => {
                        const operations = [...routingForm.operations];
                        operations[index] = { ...operation, laborCostRate: Number(event.target.value) };
                        setRoutingForm({ ...routingForm, operations });
                      }} />
                      <input type="number" min={0} step="0.01" value={operation.machineCostRate} onChange={(event) => {
                        const operations = [...routingForm.operations];
                        operations[index] = { ...operation, machineCostRate: Number(event.target.value) };
                        setRoutingForm({ ...routingForm, operations });
                      }} />
                    </div>
                  </div>
                ))}
                <div className="button-row">
                  <button
                    type="button"
                    className="secondary"
                    onClick={() => setRoutingForm({
                      ...routingForm,
                      operations: [...routingForm.operations, { sequence: (routingForm.operations.length + 1) * 10, operationCode: "", operationName: "", workCenterId: "", standardMinutes: 30, laborCostRate: 0, machineCostRate: 0 }],
                    })}
                  >
                    添加工序
                  </button>
                  {routingForm.operations.length > 1 ? (
                    <button type="button" className="secondary" onClick={() => setRoutingForm({ ...routingForm, operations: routingForm.operations.slice(0, -1) })}>
                      移除末尾工序
                    </button>
                  ) : null}
                </div>
                <button type="submit" disabled={busyKey === "routing-create"}>创建工艺路线</button>
              </form>
            )
          ) : null}
        </SectionBlock>
      </div>

      <SectionBlock title="工序排程与产能负荷" hint="工序排程会写入真实工序计划，并同步形成工作中心日负荷。">
        <div className="inventory-surface-grid">
          <div className="inventory-surface">
            {overview.operationSchedules.length > 0 ? (
              <div className="inventory-record-list">
                {overview.operationSchedules.map((schedule) => {
                  const completedQuantity = completeQuantities[schedule.id] ?? schedule.plannedQuantity;
                  return (
                    <div key={schedule.id} className="inventory-record-row">
                      <div>
                        <strong>{schedule.scheduleNo} · {schedule.operationCode} · {schedule.operationName}</strong>
                        <p>{schedule.workOrderNo} · {schedule.workCenterName} · {statusText(schedule.status)}</p>
                        <small>{formatDate(schedule.plannedStartUtc)} → {formatDate(schedule.plannedEndUtc)} · 计划 {schedule.plannedQuantity} / 完成 {schedule.completedQuantity}</small>
                      </div>
                      <div className="inventory-record-meta">
                        {canSchedule && schedule.status === "Planned" ? (
                          <button
                            disabled={busyKey === `schedule-release-${schedule.id}`}
                            onClick={async () => {
                              await runAction(`schedule-release-${schedule.id}`, async () => {
                                await api.releaseOperationSchedule(schedule.id);
                                await reloadOverview();
                              }, `${schedule.scheduleNo} 已释放。`);
                            }}
                          >
                            释放工序
                          </button>
                        ) : null}
                        {canSchedule && schedule.status !== "Completed" ? (
                          <>
                            <input type="number" min={0.0001} max={schedule.plannedQuantity} step="0.0001" value={completedQuantity} onChange={(event) => setCompleteQuantities({ ...completeQuantities, [schedule.id]: Number(event.target.value) })} />
                            <button
                              disabled={busyKey === `schedule-complete-${schedule.id}` || completedQuantity <= 0 || completedQuantity > schedule.plannedQuantity}
                              onClick={async () => {
                                await runAction(`schedule-complete-${schedule.id}`, async () => {
                                  await api.completeOperationSchedule(schedule.id, { completedQuantity });
                                  await reloadOverview();
                                }, `${schedule.scheduleNo} 已完工。`);
                              }}
                            >
                              工序完工
                            </button>
                          </>
                        ) : null}
                        {schedule.status === "Completed" ? <small>已完成</small> : null}
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <EmptyState title="暂无工序计划" description="创建工序计划后，会自动占用工作中心产能。" />
            )}
          </div>
          <div className="inventory-surface">
            {canSchedule ? (
              overview.workOrders.length === 0 || availableOperations.length === 0 ? (
                <EmptyState
                  title="缺少工单或启用路线"
                  description="排程需要已有制造工单和启用后的工艺路线。"
                  action={hasManufacturingModule ? <Link to="/manufacturing"><button type="button">去制造管理</button></Link> : undefined}
                />
              ) : (
                <form
                  className="stack-form inventory-form-panel"
                  onSubmit={async (event) => {
                    event.preventDefault();
                    if (!scheduleForm.workOrderId || !scheduleForm.routingOperationId || !scheduleForm.plannedStartUtc || !scheduleForm.plannedEndUtc || scheduleForm.plannedQuantity <= 0) {
                      setError("请完整填写工单、工序、计划时间和数量。");
                      return;
                    }

                    await runAction("schedule-create", async () => {
                      await api.createOperationSchedule({
                        workOrderId: scheduleForm.workOrderId,
                        routingOperationId: scheduleForm.routingOperationId,
                        plannedStartUtc: toIso(scheduleForm.plannedStartUtc),
                        plannedEndUtc: toIso(scheduleForm.plannedEndUtc),
                        plannedQuantity: scheduleForm.plannedQuantity,
                      });
                      setScheduleForm({ workOrderId: "", routingOperationId: "", plannedStartUtc: "", plannedEndUtc: "", plannedQuantity: 1 });
                      await reloadOverview();
                    }, "工序计划已创建。");
                  }}
                >
                  <select value={scheduleForm.workOrderId} onChange={(event) => setScheduleForm({ ...scheduleForm, workOrderId: event.target.value })}>
                    <option value="">选择制造工单</option>
                    {overview.workOrders.map((order) => (
                      <option key={order.id} value={order.id}>{order.workOrderNo} · {order.finishedItemName}</option>
                    ))}
                  </select>
                  <select value={scheduleForm.routingOperationId} onChange={(event) => setScheduleForm({ ...scheduleForm, routingOperationId: event.target.value })}>
                    <option value="">选择启用路线工序</option>
                    {availableOperations.map((operation) => (
                      <option key={operation.id} value={operation.id}>{operation.routingNo} · {operation.operationCode} · {operation.workCenterName}</option>
                    ))}
                  </select>
                  <input type="datetime-local" value={scheduleForm.plannedStartUtc} onChange={(event) => setScheduleForm({ ...scheduleForm, plannedStartUtc: event.target.value })} />
                  <input type="datetime-local" value={scheduleForm.plannedEndUtc} onChange={(event) => setScheduleForm({ ...scheduleForm, plannedEndUtc: event.target.value })} />
                  <input type="number" min={0.0001} step="0.0001" value={scheduleForm.plannedQuantity} onChange={(event) => setScheduleForm({ ...scheduleForm, plannedQuantity: Number(event.target.value) })} />
                  <button type="submit" disabled={busyKey === "schedule-create"}>创建工序计划</button>
                </form>
              )
            ) : (
              <EmptyState title="无排程权限" description="当前账号不能创建或执行工序计划。" />
            )}
          </div>
          <div className="inventory-surface inventory-surface-wide">
            {overview.capacityLoads.length > 0 ? (
              <div className="inventory-record-list">
                {overview.capacityLoads.map((load) => (
                  <div key={load.id} className="inventory-record-row">
                    <div>
                      <strong>{load.workCenterCode} · {load.workCenterName}</strong>
                      <p>{load.planDate} · 来源：{load.sourceDocumentNo || "手工维护"}</p>
                    </div>
                    <div className="inventory-balance">
                      <strong>{load.remainingMinutes}</strong>
                      <small>剩余分钟</small>
                      <small>可用 {load.availableMinutes} / 已占 {load.reservedMinutes}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无产能负荷" description="创建工序计划或手工维护产能后，这里会显示工作中心日负荷。" />
            )}

            {canSchedule && overview.workCenters.length > 0 ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!capacityForm.workCenterId || !capacityForm.planDate || capacityForm.availableMinutes <= 0 || capacityForm.reservedMinutes < 0) {
                    setError("请完整填写工作中心、日期和有效产能。");
                    return;
                  }

                  await runAction("capacity-upsert", async () => {
                    await api.upsertCapacityLoad(capacityForm);
                    setCapacityForm({ workCenterId: "", planDate: "", availableMinutes: 480, reservedMinutes: 0, sourceDocumentNo: "" });
                    await reloadOverview();
                  }, "产能负荷已保存。");
                }}
              >
                <select value={capacityForm.workCenterId} onChange={(event) => setCapacityForm({ ...capacityForm, workCenterId: event.target.value })}>
                  <option value="">选择工作中心</option>
                  {overview.workCenters.filter((center) => center.isEnabled).map((center) => (
                    <option key={center.id} value={center.id}>{center.code} · {center.name}</option>
                  ))}
                </select>
                <input type="date" value={capacityForm.planDate} onChange={(event) => setCapacityForm({ ...capacityForm, planDate: event.target.value })} />
                <div className="inline-form">
                  <input type="number" min={1} step="1" value={capacityForm.availableMinutes} onChange={(event) => setCapacityForm({ ...capacityForm, availableMinutes: Number(event.target.value) })} />
                  <input type="number" min={0} step="1" value={capacityForm.reservedMinutes} onChange={(event) => setCapacityForm({ ...capacityForm, reservedMinutes: Number(event.target.value) })} />
                </div>
                <input placeholder="来源单据号" value={capacityForm.sourceDocumentNo} onChange={(event) => setCapacityForm({ ...capacityForm, sourceDocumentNo: event.target.value })} />
                <button type="submit" disabled={busyKey === "capacity-upsert"}>保存产能负荷</button>
              </form>
            ) : null}
          </div>
        </div>
      </SectionBlock>

      <div className="split-grid">
        <SectionBlock title="制造成本" hint="成本快照绑定现有制造工单，保留材料、人工、机时和制造费用。">
          {overview.costSnapshots.length > 0 ? (
            <div className="inventory-record-list">
              {overview.costSnapshots.map((snapshot) => (
                <div key={snapshot.id} className="inventory-record-row">
                  <div>
                    <strong>{snapshot.snapshotNo} · {snapshot.workOrderNo}</strong>
                    <p>{snapshot.finishedItemCode} · {snapshot.finishedItemName}</p>
                    <small>材料 {snapshot.materialCost} / 人工 {snapshot.laborCost} / 机时 {snapshot.machineCost} / 费用 {snapshot.overheadCost}</small>
                  </div>
                  <div className="inventory-balance">
                    <strong>{snapshot.totalCost}</strong>
                    <small>总成本</small>
                    <small>{formatDate(snapshot.createdAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无成本快照" description="对真实工单生成成本快照后，这里会显示历史记录。" />
          )}

          {canManageCost ? (
            overview.workOrders.length === 0 ? (
              <EmptyState title="缺少制造工单" description="生成成本快照前需要先创建制造工单。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!costForm.workOrderId || costForm.materialCost < 0 || costForm.laborCost < 0 || costForm.machineCost < 0 || costForm.overheadCost < 0) {
                    setError("请选择工单并填写有效成本金额。");
                    return;
                  }

                  await runAction("cost-create", async () => {
                    await api.createManufacturingCostSnapshot(costForm);
                    setCostForm({ workOrderId: "", materialCost: 0, laborCost: 0, machineCost: 0, overheadCost: 0 });
                    await reloadOverview();
                  }, "制造成本快照已生成。");
                }}
              >
                <select value={costForm.workOrderId} onChange={(event) => setCostForm({ ...costForm, workOrderId: event.target.value })}>
                  <option value="">选择制造工单</option>
                  {overview.workOrders.map((order) => (
                    <option key={order.id} value={order.id}>{order.workOrderNo} · {order.finishedItemName}</option>
                  ))}
                </select>
                <div className="inline-form">
                  <input type="number" min={0} step="0.01" value={costForm.materialCost} onChange={(event) => setCostForm({ ...costForm, materialCost: Number(event.target.value) })} />
                  <input type="number" min={0} step="0.01" value={costForm.laborCost} onChange={(event) => setCostForm({ ...costForm, laborCost: Number(event.target.value) })} />
                </div>
                <div className="inline-form">
                  <input type="number" min={0} step="0.01" value={costForm.machineCost} onChange={(event) => setCostForm({ ...costForm, machineCost: Number(event.target.value) })} />
                  <input type="number" min={0} step="0.01" value={costForm.overheadCost} onChange={(event) => setCostForm({ ...costForm, overheadCost: Number(event.target.value) })} />
                </div>
                <button type="submit" disabled={busyKey === "cost-create"}>生成成本快照</button>
              </form>
            )
          ) : null}
        </SectionBlock>

        <SectionBlock title="MRP 建议" hint="根据仓库库存余额、需求量和已知供给量生成净需求建议。">
          {overview.mrpSuggestions.length > 0 ? (
            <div className="inventory-record-list">
              {overview.mrpSuggestions.map((suggestion) => (
                <div key={suggestion.id} className="inventory-record-row">
                  <div>
                    <strong>{suggestion.suggestionNo} · {suggestion.itemCode} · {suggestion.itemName}</strong>
                    <p>{suggestion.warehouseName} · {statusText(suggestion.status)} · {suggestion.sourceType || "未分类"}</p>
                    <small>库存 {suggestion.currentQuantity} / 需求 {suggestion.demandQuantity} / 供给 {suggestion.supplyQuantity} / 建议 {suggestion.suggestedQuantity}</small>
                  </div>
                  <div className="inventory-record-meta">
                    {canManageMrp && suggestion.status === "Open" ? (
                      <>
                        <button
                          disabled={busyKey === `mrp-accept-${suggestion.id}`}
                          onClick={async () => {
                            await runAction(`mrp-accept-${suggestion.id}`, async () => {
                              await api.decideMrpSuggestion(suggestion.id, { decision: "Accepted", note: "页面采纳" });
                              await reloadOverview();
                            }, `${suggestion.suggestionNo} 已采纳。`);
                          }}
                        >
                          采纳
                        </button>
                        <button
                          className="secondary"
                          disabled={busyKey === `mrp-ignore-${suggestion.id}`}
                          onClick={async () => {
                            await runAction(`mrp-ignore-${suggestion.id}`, async () => {
                              await api.decideMrpSuggestion(suggestion.id, { decision: "Ignored", note: "页面忽略" });
                              await reloadOverview();
                            }, `${suggestion.suggestionNo} 已忽略。`);
                          }}
                        >
                          忽略
                        </button>
                      </>
                    ) : (
                      <small>{suggestion.decidedBy || suggestion.createdBy}</small>
                    )}
                    <small>{formatDate(suggestion.updatedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无 MRP 建议" description="生成净需求建议后，这里会显示待处理和历史决策。" />
          )}

          {canManageMrp ? (
            missingMasterData ? (
              <EmptyState title="缺少仓库或物料" description="生成 MRP 建议需要读取仓库和物料。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!mrpForm.warehouseId || !mrpForm.itemId || mrpForm.demandQuantity <= 0 || mrpForm.supplyQuantity < 0) {
                    setError("请选择仓库、物料并填写有效需求和供给数量。");
                    return;
                  }

                  await runAction("mrp-generate", async () => {
                    await api.generateMrpSuggestion(mrpForm);
                    setMrpForm({ warehouseId: "", itemId: "", demandQuantity: 1, supplyQuantity: 0, sourceType: "生产需求" });
                    await reloadOverview();
                  }, "MRP 建议已生成。");
                }}
              >
                <select value={mrpForm.warehouseId} onChange={(event) => setMrpForm({ ...mrpForm, warehouseId: event.target.value })}>
                  <option value="">选择仓库</option>
                  {overview.warehouses.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                  ))}
                </select>
                <select value={mrpForm.itemId} onChange={(event) => setMrpForm({ ...mrpForm, itemId: event.target.value })}>
                  <option value="">选择物料</option>
                  {overview.items.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <div className="inline-form">
                  <input type="number" min={0.0001} step="0.0001" value={mrpForm.demandQuantity} onChange={(event) => setMrpForm({ ...mrpForm, demandQuantity: Number(event.target.value) })} />
                  <input type="number" min={0} step="0.0001" value={mrpForm.supplyQuantity} onChange={(event) => setMrpForm({ ...mrpForm, supplyQuantity: Number(event.target.value) })} />
                </div>
                <input placeholder="需求来源" value={mrpForm.sourceType} onChange={(event) => setMrpForm({ ...mrpForm, sourceType: event.target.value })} />
                <button type="submit" disabled={busyKey === "mrp-generate"}>生成 MRP 建议</button>
              </form>
            )
          ) : null}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
