import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { BarcodeExecution, Item, OutsourcingOrder, PlanningSuggestion, Warehouse } from "../types/api";

const loadEmptySuggestions = () => Promise.resolve<PlanningSuggestion[]>([]);
const loadEmptyOutsourcingOrders = () => Promise.resolve<OutsourcingOrder[]>([]);
const loadEmptyBarcodeExecutions = () => Promise.resolve<BarcodeExecution[]>([]);
const loadEmptyWarehouses = () => Promise.resolve<Warehouse[]>([]);
const loadEmptyItems = () => Promise.resolve<Item[]>([]);

function formatDate(value: string) {
  return new Intl.DateTimeFormat("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function suggestionStatusText(status: string) {
  switch (status) {
    case "Open":
      return "待处理";
    case "Accepted":
      return "已采纳";
    case "Ignored":
      return "已忽略";
    default:
      return status;
  }
}

function outsourcingStatusText(status: string) {
  switch (status) {
    case "Created":
      return "已创建";
    case "MaterialsIssued":
      return "已发料";
    case "PartiallyReceived":
      return "部分收料";
    case "Completed":
      return "已完成";
    default:
      return status;
  }
}

function barcodeActionText(action: string) {
  switch (action) {
    case "StockLookup":
      return "库存查询";
    case "OutsourcingIssue":
      return "外协发料";
    case "OutsourcingReceive":
      return "外协收料";
    default:
      return action;
  }
}

/** 计划执行页面，处理补货建议、外协订单材料发出/收回和条码执行。 */
export function PlanningPage() {
  const { hasPermission } = useAuth();
  const canReadPlanning = hasPermission(platformPermissions.planningRead);
  const canManagePlanning = hasPermission(platformPermissions.planningManage);
  const canManageOutsourcing = hasPermission(platformPermissions.outsourcingManage);
  const canExecuteBarcode = hasPermission(platformPermissions.barcodeExecute);
  const canReadMasterData = hasPermission(platformPermissions.masterDataRead);

  const suggestionsQuery = useAsyncData(canReadPlanning ? api.listPlanningSuggestions : loadEmptySuggestions);
  const outsourcingQuery = useAsyncData(canReadPlanning ? api.listOutsourcingOrders : loadEmptyOutsourcingOrders);
  const barcodeQuery = useAsyncData(canReadPlanning ? api.listBarcodeExecutions : loadEmptyBarcodeExecutions);
  const warehousesQuery = useAsyncData(canReadMasterData ? api.listWarehouses : loadEmptyWarehouses);
  const itemsQuery = useAsyncData(canReadMasterData ? api.listItems : loadEmptyItems);

  const [suggestionForm, setSuggestionForm] = useState({
    warehouseId: "",
    itemId: "",
    minimumQuantity: 1,
  });
  const [outsourcingForm, setOutsourcingForm] = useState({
    supplierName: "",
    warehouseId: "",
    finishedItemId: "",
    plannedQuantity: 1,
    materialItemId: "",
    materialQuantity: 1,
  });
  const [receiveQuantities, setReceiveQuantities] = useState<Record<string, number>>({});
  const [barcodeForm, setBarcodeForm] = useState({
    barcode: "",
    action: "StockLookup",
    documentId: "",
    note: "",
  });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const suggestions = suggestionsQuery.data ?? [];
  const outsourcingOrders = outsourcingQuery.data ?? [];
  const barcodeExecutions = barcodeQuery.data ?? [];
  const warehouses = (warehousesQuery.data ?? []).filter((entry) => entry.isEnabled);
  const items = (itemsQuery.data ?? []).filter((entry) => entry.isEnabled);
  const openSuggestions = useMemo(() => suggestions.filter((entry) => entry.status === "Open"), [suggestions]);
  const activeOutsourcing = useMemo(() => outsourcingOrders.filter((entry) => entry.status !== "Completed"), [outsourcingOrders]);

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
    if (canReadPlanning) {
      tasks.push(suggestionsQuery.reload(), outsourcingQuery.reload(), barcodeQuery.reload());
    }
    if (canReadMasterData) {
      tasks.push(warehousesQuery.reload(), itemsQuery.reload());
    }
    await Promise.all(tasks);
  }

  if (!canReadPlanning && !canManagePlanning && !canManageOutsourcing && !canExecuteBarcode) {
    return (
      <PageShell title="计划执行">
        <EmptyState title="无计划执行权限" description="当前账号不能查看或执行计划、外协和条码业务。" />
      </PageShell>
    );
  }

  return (
    <PageShell
      title="计划执行"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "planning-refresh"}
          onClick={async () => {
            await runAction("planning-refresh", reloadAll, "计划执行数据已刷新。");
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
        <StatTile label="待处理建议" value={openSuggestions.length} tone={openSuggestions.length > 0 ? "warning" : "success"} />
        <StatTile label="计划建议" value={suggestions.length} tone={suggestions.length > 0 ? "success" : "default"} />
        <StatTile label="在途外协" value={activeOutsourcing.length} tone={activeOutsourcing.length > 0 ? "warning" : "success"} />
        <StatTile label="扫码记录" value={barcodeExecutions.length} tone={barcodeExecutions.length > 0 ? "success" : "default"} />
      </section>

      <div className="split-grid">
        <SectionBlock title="补货建议" hint="基于真实仓库、物料和当前库存生成最小补货建议。">
          {!canReadPlanning ? (
            <EmptyState title="无计划查看权限" description="当前账号不能读取计划建议。" />
          ) : suggestionsQuery.loading ? (
            <div className="section-note">正在加载计划建议...</div>
          ) : suggestionsQuery.error ? (
            <div className="section-note error">{suggestionsQuery.error}</div>
          ) : suggestions.length > 0 ? (
            <div className="table-shell">
              {suggestions.map((suggestion) => (
                <div key={suggestion.id} className="review-card">
                  <div>
                    <strong>{suggestion.suggestionNo} · {suggestion.itemCode} · {suggestion.itemName}</strong>
                    <p>{suggestion.warehouseCode} · {suggestion.warehouseName} · {suggestionStatusText(suggestion.status)}</p>
                    <small>当前 {suggestion.currentQuantity} / 最低 {suggestion.minimumQuantity} / 建议 {suggestion.suggestedQuantity} {suggestion.unit}</small>
                    <small>{suggestion.createdBy} · {formatDate(suggestion.updatedAtUtc)}</small>
                  </div>
                  {canManagePlanning && suggestion.status === "Open" ? (
                    <div className="button-row">
                      <button
                        disabled={busyKey === `suggestion-accept-${suggestion.id}`}
                        onClick={async () => {
                          await runAction(`suggestion-accept-${suggestion.id}`, async () => {
                            await api.decidePlanningSuggestion(suggestion.id, { decision: "Accepted", note: "页面采纳" });
                            await suggestionsQuery.reload();
                          }, `${suggestion.suggestionNo} 已采纳。`);
                        }}
                      >
                        采纳建议
                      </button>
                      <button
                        className="secondary"
                        disabled={busyKey === `suggestion-ignore-${suggestion.id}`}
                        onClick={async () => {
                          await runAction(`suggestion-ignore-${suggestion.id}`, async () => {
                            await api.decidePlanningSuggestion(suggestion.id, { decision: "Ignored", note: "页面忽略" });
                            await suggestionsQuery.reload();
                          }, `${suggestion.suggestionNo} 已忽略。`);
                        }}
                      >
                        忽略建议
                      </button>
                    </div>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无计划建议" description="当库存低于设定最低值时，可以生成真实补货建议。" />
          )}

          {canManagePlanning ? (
            !canReadMasterData ? (
              <EmptyState title="缺少主数据读取权限" description="生成计划建议需要读取仓库和物料。" />
            ) : warehouses.length === 0 || items.length === 0 ? (
              <EmptyState title="缺少仓库或物料" description="请先准备启用仓库和物料，再生成补货建议。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!suggestionForm.warehouseId || !suggestionForm.itemId || suggestionForm.minimumQuantity <= 0) {
                    setError("请选择仓库、物料并填写有效最低库存。");
                    return;
                  }

                  await runAction("suggestion-generate", async () => {
                    await api.generatePlanningSuggestion(suggestionForm);
                    setSuggestionForm({ warehouseId: "", itemId: "", minimumQuantity: 1 });
                    await suggestionsQuery.reload();
                  }, "计划建议已生成。");
                }}
              >
                <select value={suggestionForm.warehouseId} onChange={(event) => setSuggestionForm({ ...suggestionForm, warehouseId: event.target.value })}>
                  <option value="">选择仓库</option>
                  {warehouses.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                  ))}
                </select>
                <select value={suggestionForm.itemId} onChange={(event) => setSuggestionForm({ ...suggestionForm, itemId: event.target.value })}>
                  <option value="">选择物料</option>
                  {items.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <input type="number" min={0.0001} step="0.0001" value={suggestionForm.minimumQuantity} onChange={(event) => setSuggestionForm({ ...suggestionForm, minimumQuantity: Number(event.target.value) })} />
                <button type="submit" disabled={busyKey === "suggestion-generate" || !suggestionForm.warehouseId || !suggestionForm.itemId || suggestionForm.minimumQuantity <= 0}>
                  生成补货建议
                </button>
              </form>
            )
          ) : canReadPlanning ? (
            <div className="section-note">当前账号只能查看计划建议，不能生成或决策。</div>
          ) : null}
        </SectionBlock>

        <SectionBlock title="外协加工" hint="外协单跟踪发料、收料和库存流水，不复制制造或库存规则。">
          {!canReadPlanning ? (
            <EmptyState title="无外协查看权限" description="当前账号不能读取外协单。" />
          ) : outsourcingQuery.loading ? (
            <div className="section-note">正在加载外协单...</div>
          ) : outsourcingQuery.error ? (
            <div className="section-note error">{outsourcingQuery.error}</div>
          ) : outsourcingOrders.length > 0 ? (
            <div className="table-shell">
              {outsourcingOrders.map((order) => {
                const remainingQuantity = order.plannedQuantity - order.receivedQuantity;
                const receiveQuantity = receiveQuantities[order.id] ?? remainingQuantity;
                return (
                  <div key={order.id} className="review-card">
                    <div>
                      <strong>{order.orderNo} · {order.finishedItemCode} · {order.finishedItemName}</strong>
                      <p>{order.supplierName} · {order.warehouseName} · {outsourcingStatusText(order.status)}</p>
                      <small>计划 {order.plannedQuantity} / 已收 {order.receivedQuantity} {order.unit}</small>
                      <div className="inventory-lines">
                        {order.materialLines.map((line) => (
                          <span key={line.id}>{line.itemCode} · {line.itemName} x {line.quantity} {line.unit}</span>
                        ))}
                      </div>
                    </div>
                    {canManageOutsourcing ? (
                      <div className="inventory-actions">
                        {order.status === "Created" ? (
                          <button
                            disabled={busyKey === `outsourcing-issue-${order.id}`}
                            onClick={async () => {
                              await runAction(`outsourcing-issue-${order.id}`, async () => {
                                await api.issueOutsourcingMaterials(order.id);
                                await Promise.all([outsourcingQuery.reload(), barcodeQuery.reload()]);
                              }, `${order.orderNo} 已完成外协发料。`);
                            }}
                          >
                            外协发料
                          </button>
                        ) : null}
                        {order.status === "MaterialsIssued" || order.status === "PartiallyReceived" ? (
                          <>
                            <input
                              type="number"
                              min={0.0001}
                              max={remainingQuantity}
                              step="0.0001"
                              value={receiveQuantity}
                              onChange={(event) => setReceiveQuantities({ ...receiveQuantities, [order.id]: Number(event.target.value) })}
                            />
                            <button
                              disabled={busyKey === `outsourcing-receive-${order.id}` || receiveQuantity <= 0 || receiveQuantity > remainingQuantity}
                              onClick={async () => {
                                await runAction(`outsourcing-receive-${order.id}`, async () => {
                                  await api.receiveOutsourcingOrder(order.id, { quantity: receiveQuantity });
                                  await outsourcingQuery.reload();
                                }, `${order.orderNo} 已完成外协收料。`);
                              }}
                            >
                              外协收料
                            </button>
                          </>
                        ) : null}
                        {order.status === "Completed" ? <small>外协单已完成。</small> : null}
                      </div>
                    ) : null}
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState title="暂无外协单" description="创建第一张外协单后，可在这里执行发料和收料。" />
          )}

          {canManageOutsourcing ? (
            !canReadMasterData ? (
              <EmptyState title="缺少主数据读取权限" description="创建外协单需要读取仓库和物料。" />
            ) : warehouses.length === 0 || items.length < 2 ? (
              <EmptyState title="缺少仓库或物料" description="外协单至少需要一个成品物料、一个发料物料和一个启用仓库。" />
            ) : (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!outsourcingForm.supplierName.trim() || !outsourcingForm.warehouseId || !outsourcingForm.finishedItemId || !outsourcingForm.materialItemId || outsourcingForm.plannedQuantity <= 0 || outsourcingForm.materialQuantity <= 0) {
                    setError("请完整填写外协供应商、仓库、成品、计划数量和发料物料。");
                    return;
                  }

                  await runAction("outsourcing-create", async () => {
                    await api.createOutsourcingOrder({
                      supplierName: outsourcingForm.supplierName.trim(),
                      warehouseId: outsourcingForm.warehouseId,
                      finishedItemId: outsourcingForm.finishedItemId,
                      plannedQuantity: outsourcingForm.plannedQuantity,
                      materialLines: [{ itemId: outsourcingForm.materialItemId, quantity: outsourcingForm.materialQuantity }],
                    });
                    setOutsourcingForm({ supplierName: "", warehouseId: "", finishedItemId: "", plannedQuantity: 1, materialItemId: "", materialQuantity: 1 });
                    await outsourcingQuery.reload();
                  }, "外协单已创建。");
                }}
              >
                <input placeholder="外协供应商" value={outsourcingForm.supplierName} onChange={(event) => setOutsourcingForm({ ...outsourcingForm, supplierName: event.target.value })} />
                <select value={outsourcingForm.warehouseId} onChange={(event) => setOutsourcingForm({ ...outsourcingForm, warehouseId: event.target.value })}>
                  <option value="">选择外协仓库</option>
                  {warehouses.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                  ))}
                </select>
                <select value={outsourcingForm.finishedItemId} onChange={(event) => setOutsourcingForm({ ...outsourcingForm, finishedItemId: event.target.value })}>
                  <option value="">选择收料成品</option>
                  {items.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <input type="number" min={0.0001} step="0.0001" value={outsourcingForm.plannedQuantity} onChange={(event) => setOutsourcingForm({ ...outsourcingForm, plannedQuantity: Number(event.target.value) })} />
                <select value={outsourcingForm.materialItemId} onChange={(event) => setOutsourcingForm({ ...outsourcingForm, materialItemId: event.target.value })}>
                  <option value="">选择发料物料</option>
                  {items.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <input type="number" min={0.0001} step="0.0001" value={outsourcingForm.materialQuantity} onChange={(event) => setOutsourcingForm({ ...outsourcingForm, materialQuantity: Number(event.target.value) })} />
                <button
                  type="submit"
                  disabled={busyKey === "outsourcing-create" || !outsourcingForm.supplierName.trim() || !outsourcingForm.warehouseId || !outsourcingForm.finishedItemId || !outsourcingForm.materialItemId || outsourcingForm.plannedQuantity <= 0 || outsourcingForm.materialQuantity <= 0}
                >
                  创建外协单
                </button>
              </form>
            )
          ) : canReadPlanning ? (
            <div className="section-note">当前账号只能查看外协单，不能创建或执行。</div>
          ) : null}
        </SectionBlock>
      </div>

      <SectionBlock title="PDA / 条码执行" hint="扫码接口记录真实执行结果，支持库存查询和外协发料/收料动作。">
        <div className="split-grid">
          <div>
            {canExecuteBarcode ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!barcodeForm.barcode.trim() || !barcodeForm.action) {
                    setError("请填写条码和扫码动作。");
                    return;
                  }

                  await runAction("barcode-execute", async () => {
                    await api.executeBarcode({
                      barcode: barcodeForm.barcode.trim(),
                      action: barcodeForm.action,
                      documentId: barcodeForm.documentId || null,
                      note: barcodeForm.note.trim(),
                    });
                    setBarcodeForm({ barcode: "", action: "StockLookup", documentId: "", note: "" });
                    await Promise.all([barcodeQuery.reload(), outsourcingQuery.reload()]);
                  }, "扫码动作已执行。");
                }}
              >
                <input placeholder="条码 / 外协单号 / 物料编码" value={barcodeForm.barcode} onChange={(event) => setBarcodeForm({ ...barcodeForm, barcode: event.target.value })} />
                <select value={barcodeForm.action} onChange={(event) => setBarcodeForm({ ...barcodeForm, action: event.target.value })}>
                  <option value="StockLookup">库存查询</option>
                  <option value="OutsourcingIssue">外协发料</option>
                  <option value="OutsourcingReceive">外协收料</option>
                </select>
                <select value={barcodeForm.documentId} onChange={(event) => setBarcodeForm({ ...barcodeForm, documentId: event.target.value })}>
                  <option value="">可选：选择外协单</option>
                  {outsourcingOrders.map((order) => (
                    <option key={order.id} value={order.id}>{order.orderNo} · {outsourcingStatusText(order.status)}</option>
                  ))}
                </select>
                <input placeholder="备注" value={barcodeForm.note} onChange={(event) => setBarcodeForm({ ...barcodeForm, note: event.target.value })} />
                <button type="submit" disabled={busyKey === "barcode-execute" || !barcodeForm.barcode.trim()}>
                  执行扫码
                </button>
              </form>
            ) : (
              <EmptyState title="无条码执行权限" description="当前账号不能提交 PDA 或条码动作。" />
            )}
          </div>
          <div>
            {!canReadPlanning ? (
              <EmptyState title="无扫码记录查看权限" description="当前账号不能读取扫码历史。" />
            ) : barcodeQuery.loading ? (
              <div className="section-note">正在加载扫码记录...</div>
            ) : barcodeQuery.error ? (
              <div className="section-note error">{barcodeQuery.error}</div>
            ) : barcodeExecutions.length > 0 ? (
              <div className="inventory-record-list">
                {barcodeExecutions.map((entry) => (
                  <div key={entry.id} className="inventory-record-row">
                    <div>
                      <strong>{entry.executionNo} · {barcodeActionText(entry.action)} · {entry.result === "Success" ? "成功" : "失败"}</strong>
                      <p>{entry.barcode} · {entry.documentNo}</p>
                      <small>{entry.message}</small>
                    </div>
                    <div className="inventory-record-meta">
                      <small>{entry.actor}</small>
                      <small>{formatDate(entry.createdAtUtc)}</small>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="暂无扫码记录" description="执行扫码动作后，这里会显示真实结果。" />
            )}
          </div>
        </div>
      </SectionBlock>
    </PageShell>
  );
}
