import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { Link } from "react-router-dom";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { Bom, Item, ProductionIssue, ProductionReceipt, Warehouse, WorkOrder } from "../types/api";

const loadEmptyBoms = () => Promise.resolve<Bom[]>([]);
const loadEmptyWorkOrders = () => Promise.resolve<WorkOrder[]>([]);
const loadEmptyProductionIssues = () => Promise.resolve<ProductionIssue[]>([]);
const loadEmptyProductionReceipts = () => Promise.resolve<ProductionReceipt[]>([]);
const loadEmptyItems = () => Promise.resolve<Item[]>([]);
const loadEmptyWarehouses = () => Promise.resolve<Warehouse[]>([]);

function formatDate(value: string) {
  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatMoney(value: number) {
  return new Intl.NumberFormat("zh-CN", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function workOrderStatusText(status: string) {
  switch (status) {
    case "Draft":
      return "草稿";
    case "Released":
      return "已下达";
    case "MaterialsIssued":
      return "已领料";
    case "PartiallyCompleted":
      return "部分完工";
    case "Completed":
      return "已完工";
    default:
      return status;
  }
}

function documentStatusText(status: string) {
  return status === "Completed" ? "已完成" : status;
}

/** 制造页面，串联 BOM、工单、生产领料和完工入库。 */
export function ManufacturingPage() {
  const { hasPermission, user } = useAuth();
  const canReadManufacturing = hasPermission(platformPermissions.manufacturingRead);
  const canManageBom = hasPermission(platformPermissions.manufacturingBomManage);
  const canManageWorkOrder = hasPermission(platformPermissions.manufacturingWorkOrderManage);
  const canManageExecution = hasPermission(platformPermissions.manufacturingExecutionManage);
  const canReadMasterData = hasPermission(platformPermissions.masterDataRead);
  const canReadQuality = hasPermission(platformPermissions.qualityRead);
  const hasQualityModule = user?.visibleModuleKeys.includes("quality") ?? false;
  const canEnterQuality = canReadQuality && hasQualityModule;

  const bomsQuery = useAsyncData(canReadManufacturing ? api.listBoms : loadEmptyBoms);
  const workOrdersQuery = useAsyncData(canReadManufacturing ? api.listWorkOrders : loadEmptyWorkOrders);
  const issuesQuery = useAsyncData(canReadManufacturing ? api.listProductionIssues : loadEmptyProductionIssues);
  const receiptsQuery = useAsyncData(canReadManufacturing ? api.listProductionReceipts : loadEmptyProductionReceipts);
  const itemsQuery = useAsyncData(canReadMasterData ? api.listItems : loadEmptyItems);
  const warehousesQuery = useAsyncData(canReadMasterData ? api.listWarehouses : loadEmptyWarehouses);

  const [bomForm, setBomForm] = useState({
    finishedItemId: "",
    version: "V1",
    baseQuantity: 1,
    componentItemId: "",
    componentQuantity: 1,
    isEnabled: true,
  });
  const [workOrderForm, setWorkOrderForm] = useState({
    bomId: "",
    plannedQuantity: 1,
  });
  const [issueWarehouses, setIssueWarehouses] = useState<Record<string, string>>({});
  const [receiptForms, setReceiptForms] = useState<Record<string, { warehouseId: string; quantity: number }>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const boms = bomsQuery.data ?? [];
  const workOrders = workOrdersQuery.data ?? [];
  const issues = issuesQuery.data ?? [];
  const receipts = receiptsQuery.data ?? [];
  const items = (itemsQuery.data ?? []).filter((entry) => entry.isEnabled);
  const warehouses = (warehousesQuery.data ?? []).filter((entry) => entry.isEnabled);
  const enabledBoms = boms.filter((entry) => entry.isEnabled);

  const releasedWorkOrders = useMemo(
    () => workOrders.filter((entry) => entry.status === "Released"),
    [workOrders],
  );
  const receivableWorkOrders = useMemo(
    () => workOrders.filter((entry) => entry.status === "MaterialsIssued" || entry.status === "PartiallyCompleted"),
    [workOrders],
  );
  const completedWorkOrders = workOrders.filter((entry) => entry.status === "Completed").length;
  const missingMasterData = items.length < 2 || warehouses.length === 0;

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

  async function reloadAll() {
    const tasks: Promise<unknown>[] = [];
    if (canReadManufacturing) {
      tasks.push(bomsQuery.reload(), workOrdersQuery.reload(), issuesQuery.reload(), receiptsQuery.reload());
    }
    if (canReadMasterData) {
      tasks.push(itemsQuery.reload(), warehousesQuery.reload());
    }
    await Promise.all(tasks);
  }

  function getReceiptForm(workOrder: WorkOrder) {
    return receiptForms[workOrder.id] ?? {
      warehouseId: "",
      quantity: Math.max(workOrder.plannedQuantity - workOrder.completedQuantity, 0),
    };
  }

  if (!canReadManufacturing && !canManageBom && !canManageWorkOrder && !canManageExecution) {
    return (
      <PageShell title="制造管理">
        <EmptyState title="无制造模块权限" description="当前账号不能查看或执行制造业务。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="制造管理"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "manufacturing-refresh"}
          onClick={async () => {
            await runAction("manufacturing-refresh", reloadAll, "制造数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      }
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <section className="stats-grid">
        <StatTile label="BOM" value={boms.length} tone={boms.length > 0 ? "success" : "default"} />
        <StatTile label="工单" value={workOrders.length} tone={workOrders.length > 0 ? "success" : "default"} />
        <StatTile label="待领料" value={releasedWorkOrders.length} tone={releasedWorkOrders.length > 0 ? "warning" : "default"} />
        <StatTile label="已完工" value={completedWorkOrders} tone={completedWorkOrders > 0 ? "success" : "default"} />
      </section>

      <div className="split-grid">
        <SectionBlock title="BOM 维护" hint="BOM 使用真实物料主数据，先建立成品和组件的最小结构。">
          {!canReadManufacturing ? (
            <EmptyState title="无 BOM 查看权限" description="当前账号不能读取 BOM。" />
          ) : bomsQuery.loading ? (
            <div className="section-note">正在加载 BOM...</div>
          ) : bomsQuery.error ? (
            <div className="section-note error">{bomsQuery.error}</div>
          ) : boms.length > 0 ? (
            <div className="table-shell">
              {boms.map((bom) => (
                <div key={bom.id} className="review-card">
                  <div>
                    <strong>{bom.bomNo} · {bom.finishedItemCode} · {bom.finishedItemName}</strong>
                    <p>{bom.version} · 基准 {bom.baseQuantity} {bom.unit} · {bom.isEnabled ? "启用" : "停用"}</p>
                    <small>更新：{formatDate(bom.updatedAtUtc)}</small>
                    <div className="inventory-lines">
                      {bom.lines.map((line) => (
                        <span key={line.id}>
                          {line.componentItemCode} · {line.componentItemName} x {line.quantity} {line.unit}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无 BOM" description="创建第一条 BOM 后，可以基于它创建工单。" />
          )}

          {canManageBom ? (
            !canReadMasterData ? (
              <EmptyState title="缺少主数据读取权限" description="创建 BOM 需要读取物料主数据。" />
            ) : items.length < 2 ? (
              <EmptyState
                title="物料不足"
                description="BOM 至少需要一个成品物料和一个组件物料。"
                action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
              />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!bomForm.finishedItemId || !bomForm.componentItemId || bomForm.finishedItemId === bomForm.componentItemId || bomForm.baseQuantity <= 0 || bomForm.componentQuantity <= 0) {
                    setError("请选择不同的成品和组件，并填写有效数量。");
                    return;
                  }

                  await runAction("bom-create", async () => {
                    await api.createBom({
                      finishedItemId: bomForm.finishedItemId,
                      version: bomForm.version.trim() || "V1",
                      baseQuantity: bomForm.baseQuantity,
                      isEnabled: bomForm.isEnabled,
                      lines: [{ componentItemId: bomForm.componentItemId, quantity: bomForm.componentQuantity }],
                    });
                    setBomForm({ finishedItemId: "", version: "V1", baseQuantity: 1, componentItemId: "", componentQuantity: 1, isEnabled: true });
                    await bomsQuery.reload();
                  }, "BOM 已创建。");
                }}
              >
                <select value={bomForm.finishedItemId} onChange={(event) => setBomForm({ ...bomForm, finishedItemId: event.target.value })}>
                  <option value="">选择成品物料</option>
                  {items.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <div className="inline-form">
                  <input placeholder="版本" value={bomForm.version} onChange={(event) => setBomForm({ ...bomForm, version: event.target.value })} />
                  <input type="number" min={0.0001} step="0.0001" value={bomForm.baseQuantity} onChange={(event) => setBomForm({ ...bomForm, baseQuantity: Number(event.target.value) })} />
                </div>
                <select value={bomForm.componentItemId} onChange={(event) => setBomForm({ ...bomForm, componentItemId: event.target.value })}>
                  <option value="">选择组件物料</option>
                  {items.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <input type="number" min={0.0001} step="0.0001" value={bomForm.componentQuantity} onChange={(event) => setBomForm({ ...bomForm, componentQuantity: Number(event.target.value) })} />
                <label className="checkbox-row">
                  <input type="checkbox" checked={bomForm.isEnabled} onChange={(event) => setBomForm({ ...bomForm, isEnabled: event.target.checked })} />
                  <span>启用 BOM</span>
                </label>
                <button
                  type="submit"
                  disabled={busyKey === "bom-create" || !bomForm.finishedItemId || !bomForm.componentItemId || bomForm.finishedItemId === bomForm.componentItemId || bomForm.baseQuantity <= 0 || bomForm.componentQuantity <= 0}
                >
                  创建 BOM
                </button>
              </form>
            )
          ) : canReadManufacturing ? (
            <div className="section-note">当前账号只能查看 BOM，不能创建。</div>
          ) : null}
        </SectionBlock>

        <SectionBlock title="工单管理" hint="工单从启用 BOM 创建，先下达，再进入领料与完工。">
          {!canReadManufacturing ? (
            <EmptyState title="无工单查看权限" description="当前账号不能读取工单。" />
          ) : workOrdersQuery.loading ? (
            <div className="section-note">正在加载工单...</div>
          ) : workOrdersQuery.error ? (
            <div className="section-note error">{workOrdersQuery.error}</div>
          ) : workOrders.length > 0 ? (
            <div className="table-shell">
              {workOrders.map((workOrder) => (
                <div key={workOrder.id} className="review-card">
                  <div>
                    <strong>{workOrder.workOrderNo} · {workOrder.finishedItemCode} · {workOrder.finishedItemName}</strong>
                    <p>
                      {workOrder.bomNo}/{workOrder.bomVersion} · {workOrderStatusText(workOrder.status)} ·
                      计划 {workOrder.plannedQuantity} {workOrder.unit} · 完工 {workOrder.completedQuantity} {workOrder.unit}
                    </p>
                    <small>更新：{formatDate(workOrder.updatedAtUtc)}</small>
                    <div className="inventory-lines">
                      {workOrder.materialLines.map((line) => (
                        <span key={line.id}>
                          {line.componentItemCode} · {line.componentItemName} 需 {line.requiredQuantity} / 已领 {line.issuedQuantity} {line.unit}
                        </span>
                      ))}
                    </div>
                    <div className="inventory-lines">
                      <span>成本来源：{workOrder.costSummary.costSource}</span>
                      <span>材料 {formatMoney(workOrder.costSummary.materialCost)}</span>
                      <span>人工 {formatMoney(workOrder.costSummary.laborCost)}</span>
                      <span>机时 {formatMoney(workOrder.costSummary.machineCost)}</span>
                      <span>制造费用 {formatMoney(workOrder.costSummary.overheadCost)}</span>
                      <span>总成本 {formatMoney(workOrder.costSummary.totalCost)}</span>
                      <span>单位成本 {formatMoney(workOrder.costSummary.unitCost)}</span>
                      <span>已入库成本 {formatMoney(workOrder.costSummary.receivedCost)}</span>
                      <span>待入库成本 {formatMoney(workOrder.costSummary.remainingCost)}</span>
                      {workOrder.costSummary.snapshotTotalCost > 0 ? (
                        <span>快照差异 {formatMoney(workOrder.costSummary.totalCostVariance)}</span>
                      ) : null}
                    </div>
                  </div>
                  <div className="inventory-actions">
                    {workOrder.status === "Draft" ? (
                      canManageWorkOrder ? (
                        <button
                          disabled={busyKey === `work-order-release-${workOrder.id}`}
                          onClick={async () => {
                            await runAction(`work-order-release-${workOrder.id}`, async () => {
                              await api.releaseWorkOrder(workOrder.id);
                              await workOrdersQuery.reload();
                            }, `${workOrder.workOrderNo} 已下达。`);
                          }}
                        >
                          下达工单
                        </button>
                      ) : (
                        <small>当前账号不能下达工单。</small>
                      )
                    ) : null}
                    {workOrder.status === "Released" ? <small>工单已下达，等待生产领料。</small> : null}
                    {workOrder.status === "MaterialsIssued" || workOrder.status === "PartiallyCompleted" ? <small>工单可执行完工入库。</small> : null}
                    {workOrder.status === "Completed" ? <small>工单已完工。</small> : null}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无工单" description="创建 BOM 后，可以在这里生成第一张制造工单。" />
          )}

          {canManageWorkOrder ? (
            enabledBoms.length === 0 ? (
              <EmptyState title="缺少启用 BOM" description="创建工单前需要先建立启用 BOM。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!workOrderForm.bomId || workOrderForm.plannedQuantity <= 0) {
                    setError("请选择 BOM 并填写有效计划数量。");
                    return;
                  }

                  await runAction("work-order-create", async () => {
                    await api.createWorkOrder({
                      bomId: workOrderForm.bomId,
                      plannedQuantity: workOrderForm.plannedQuantity,
                    });
                    setWorkOrderForm({ bomId: "", plannedQuantity: 1 });
                    await workOrdersQuery.reload();
                  }, "工单已创建。");
                }}
              >
                <select value={workOrderForm.bomId} onChange={(event) => setWorkOrderForm({ ...workOrderForm, bomId: event.target.value })}>
                  <option value="">选择 BOM</option>
                  {enabledBoms.map((bom) => (
                    <option key={bom.id} value={bom.id}>{bom.bomNo} · {bom.finishedItemCode} · {bom.finishedItemName}</option>
                  ))}
                </select>
                <input type="number" min={0.0001} step="0.0001" value={workOrderForm.plannedQuantity} onChange={(event) => setWorkOrderForm({ ...workOrderForm, plannedQuantity: Number(event.target.value) })} />
                <button type="submit" disabled={busyKey === "work-order-create" || !workOrderForm.bomId || workOrderForm.plannedQuantity <= 0}>创建工单</button>
              </form>
            )
          ) : canReadManufacturing ? (
            <div className="section-note">当前账号只能查看工单，不能创建或下达。</div>
          ) : null}
        </SectionBlock>
      </div>

      <div className="split-grid">
        <SectionBlock title="生产领料" hint="已下达工单会扣减原料库存，并写入库存流水。">
          {!canReadManufacturing ? (
            <EmptyState title="无生产领料查看权限" description="当前账号不能读取生产领料。" />
          ) : releasedWorkOrders.length > 0 ? (
            <div className="table-shell">
              {releasedWorkOrders.map((workOrder) => (
                <div key={workOrder.id} className="review-card">
                  <div>
                    <strong>{workOrder.workOrderNo}</strong>
                    <p>{workOrder.finishedItemCode} · {workOrder.finishedItemName} · 计划 {workOrder.plannedQuantity} {workOrder.unit}</p>
                    <div className="inventory-lines">
                      {workOrder.materialLines.map((line) => (
                        <span key={line.id}>
                          {line.componentItemCode} · 待领 {line.requiredQuantity - line.issuedQuantity} {line.unit}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div className="inventory-actions">
                    {canManageExecution ? (
                      !canReadMasterData || missingMasterData ? (
                        <EmptyState
                          title="缺少仓库或物料"
                          description="领料前需要启用仓库和物料，并确保原料有库存。"
                          action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
                        />
                      ) : (
                        <>
                          <select
                            value={issueWarehouses[workOrder.id] ?? ""}
                            onChange={(event) => setIssueWarehouses((current) => ({ ...current, [workOrder.id]: event.target.value }))}
                          >
                            <option value="">选择领料仓库</option>
                            {warehouses.map((warehouse) => (
                              <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                            ))}
                          </select>
                          <button
                            disabled={busyKey === `production-issue-${workOrder.id}` || !(issueWarehouses[workOrder.id] ?? "")}
                            onClick={async () => {
                              await runAction(`production-issue-${workOrder.id}`, async () => {
                                await api.executeProductionIssue(workOrder.id, { warehouseId: issueWarehouses[workOrder.id] ?? "" });
                                setIssueWarehouses((current) => {
                                  const next = { ...current };
                                  delete next[workOrder.id];
                                  return next;
                                });
                                await reloadAll();
                              }, `${workOrder.workOrderNo} 已完成生产领料。`);
                            }}
                          >
                            执行领料
                          </button>
                        </>
                      )
                    ) : (
                      <small>当前账号不能执行生产领料。</small>
                    )}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无待领料工单" description="工单下达后，会在这里等待扣减原料库存。" />
          )}

          {issues.length > 0 ? (
            <div className="inventory-record-list">
              {issues.map((issue) => (
                <div key={issue.id} className="inventory-record-row">
                  <div>
                    <strong>{issue.issueNo}</strong>
                    <p>{issue.workOrderNo} · {issue.warehouseName}</p>
                    <small>{issue.lines.map((line) => `${line.itemCode} x ${line.quantity} ${line.unit}`).join("，")}</small>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{documentStatusText(issue.status)}</small>
                    <small>{formatDate(issue.issuedAtUtc)}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无领料记录" description="执行生产领料后，这里会形成真实历史。" />
          )}
        </SectionBlock>

        <SectionBlock title="完工入库" hint="已领料工单会增加成品库存，并写入库存流水。">
          {!canReadManufacturing ? (
            <EmptyState title="无完工入库查看权限" description="当前账号不能读取完工入库。" />
          ) : receivableWorkOrders.length > 0 ? (
            <div className="table-shell">
              {receivableWorkOrders.map((workOrder) => {
                const receiptForm = getReceiptForm(workOrder);
                const remainingQuantity = workOrder.plannedQuantity - workOrder.completedQuantity;
                return (
                  <div key={workOrder.id} className="review-card">
                    <div>
                      <strong>{workOrder.workOrderNo}</strong>
                      <p>{workOrder.finishedItemCode} · {workOrder.finishedItemName}</p>
                      <small>剩余可入库：{remainingQuantity} {workOrder.unit}</small>
                      <div className="inventory-lines">
                        <span>成本来源：{workOrder.costSummary.costSource}</span>
                        <span>剩余成本 {formatMoney(workOrder.costSummary.remainingCost)}</span>
                        <span>预计单位成本 {formatMoney(workOrder.costSummary.unitCost)}</span>
                      </div>
                    </div>
                    <div className="inventory-actions">
                      {canManageExecution ? (
                        !canReadMasterData || warehouses.length === 0 ? (
                          <EmptyState
                            title="缺少仓库"
                            description="完工入库前需要先准备启用仓库。"
                            action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
                          />
                        ) : (
                          <>
                            <select
                              value={receiptForm.warehouseId}
                              onChange={(event) => setReceiptForms((current) => ({
                                ...current,
                                [workOrder.id]: { ...receiptForm, warehouseId: event.target.value },
                              }))}
                            >
                              <option value="">选择入库仓库</option>
                              {warehouses.map((warehouse) => (
                                <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                              ))}
                            </select>
                            <input
                              type="number"
                              min={0.0001}
                              max={remainingQuantity}
                              step="0.0001"
                              value={receiptForm.quantity}
                              onChange={(event) => setReceiptForms((current) => ({
                                ...current,
                                [workOrder.id]: { ...receiptForm, quantity: Number(event.target.value) },
                              }))}
                            />
                            <button
                              disabled={busyKey === `production-receipt-${workOrder.id}` || !receiptForm.warehouseId || receiptForm.quantity <= 0 || receiptForm.quantity > remainingQuantity}
                              onClick={async () => {
                                await runAction(`production-receipt-${workOrder.id}`, async () => {
                                  await api.completeProduction(workOrder.id, {
                                    warehouseId: receiptForm.warehouseId,
                                    quantity: receiptForm.quantity,
                                  });
                                  setReceiptForms((current) => {
                                    const next = { ...current };
                                    delete next[workOrder.id];
                                    return next;
                                  });
                                  await reloadAll();
                                }, `${workOrder.workOrderNo} 已完成入库。`);
                              }}
                            >
                              完工入库
                            </button>
                          </>
                        )
                      ) : (
                        <small>当前账号不能执行完工入库。</small>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无可入库工单" description="生产领料完成后，工单会在这里等待成品入库。" />
          )}

          {receipts.length > 0 ? (
            <div className="inventory-record-list">
              {receipts.map((receipt) => (
                <div key={receipt.id} className="inventory-record-row">
                  <div>
                    <strong>{receipt.receiptNo}</strong>
                    <p>{receipt.workOrderNo} · {receipt.warehouseName}</p>
                    <small>{receipt.finishedItemCode} · {receipt.finishedItemName} x {receipt.quantity} {receipt.unit}</small>
                    <div className="inventory-lines">
                      <span>单位成本 {formatMoney(receipt.unitCost)}</span>
                      <span>材料 {formatMoney(receipt.materialCost)}</span>
                      <span>人工 {formatMoney(receipt.laborCost)}</span>
                      <span>机时 {formatMoney(receipt.machineCost)}</span>
                      <span>制造费用 {formatMoney(receipt.overheadCost)}</span>
                      <span>入库成本 {formatMoney(receipt.costAmount)}</span>
                    </div>
                  </div>
                  <div className="inventory-record-meta">
                    <small>{documentStatusText(receipt.status)}</small>
                    <small>{formatDate(receipt.receivedAtUtc)}</small>
                    {canEnterQuality ? (
                      <Link to="/quality"><button type="button" className="secondary">去质量追溯</button></Link>
                    ) : null}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无完工入库记录" description="执行完工入库后，这里会形成真实历史。" />
          )}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
