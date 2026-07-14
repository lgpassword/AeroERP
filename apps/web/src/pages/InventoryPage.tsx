import { AnimatePresence, motion } from "framer-motion";
import {
  ArrowRightLeft,
  Boxes,
  ClipboardCheck,
  ListFilter,
  MapPin,
  PackageMinus,
  PackagePlus,
  RefreshCcw,
  ScrollText,
  Warehouse as WarehouseIcon,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { Link, useSearchParams } from "react-router-dom";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type {
  InventoryCountAdjustment,
  InventoryIssue,
  InventoryLedgerEntry,
  InventoryMovement,
  InventoryReceipt,
  InventoryTransfer,
  LocationStockBalance,
  Item,
  PendingInventoryIssue,
  PendingInventoryReceipt,
  StockBalance,
  Warehouse,
  WarehouseLocation,
} from "../types/api";

type InventoryPanelKey = "receipt" | "issue" | "transfer" | "count" | "location" | "movement" | "ledger" | "balance";

const loadEmptyPendingReceipts = () => Promise.resolve<PendingInventoryReceipt[]>([]);
const loadEmptyPendingIssues = () => Promise.resolve<PendingInventoryIssue[]>([]);
const loadEmptyReceipts = () => Promise.resolve<InventoryReceipt[]>([]);
const loadEmptyIssues = () => Promise.resolve<InventoryIssue[]>([]);
const loadEmptyTransfers = () => Promise.resolve<InventoryTransfer[]>([]);
const loadEmptyCounts = () => Promise.resolve<InventoryCountAdjustment[]>([]);
const loadEmptyMovements = () => Promise.resolve<InventoryMovement[]>([]);
const loadEmptyLedger = () => Promise.resolve<InventoryLedgerEntry[]>([]);
const loadEmptyBalances = () => Promise.resolve<StockBalance[]>([]);
const loadEmptyLocations = () => Promise.resolve<WarehouseLocation[]>([]);
const loadEmptyLocationBalances = () => Promise.resolve<LocationStockBalance[]>([]);
const loadEmptyWarehouses = () => Promise.resolve<Warehouse[]>([]);
const loadEmptyItems = () => Promise.resolve<Item[]>([]);

const panelMeta: Record<InventoryPanelKey, { title: string; hint: string }> = {
  receipt: { title: "采购入库", hint: "下达后的采购订单在这里落库，形成库存余额与流水。" },
  issue: { title: "销售出库", hint: "待出库销售订单在这里执行发货，避免销售链路断在状态推进。" },
  transfer: { title: "仓间调拨", hint: "在真实仓库之间移动库存，自动产生出入两段流水。" },
  count: { title: "库存盘点", hint: "按仓库执行盘点，保留盘前、盘后与差异结果。" },
  location: { title: "库位管理", hint: "维护仓库内库位，并查看库位级物料结存。" },
  movement: { title: "库存流水", hint: "查看所有库存动作的文档、方向、仓库和余额变化。" },
  ledger: { title: "存货明细账", hint: "按仓库和物料追踪入库、出库、结存数量与成本金额。" },
  balance: { title: "库存余额", hint: "按仓库、库位与物料查看当前结存，作为执行前校验依据。" },
};

const panelCards: Array<{ key: InventoryPanelKey; title: string; description: string; icon: typeof PackagePlus }> = [
  { key: "receipt", title: "采购入库", description: "处理已下达采购订单", icon: PackagePlus },
  { key: "issue", title: "销售出库", description: "处理待出库销售订单", icon: PackageMinus },
  { key: "transfer", title: "仓间调拨", description: "执行仓库间库存转移", icon: ArrowRightLeft },
  { key: "count", title: "库存盘点", description: "执行盘点与差异调整", icon: ClipboardCheck },
  { key: "location", title: "库位管理", description: "维护仓库库位与库位库存", icon: MapPin },
  { key: "movement", title: "库存流水", description: "查看全部库存变动记录", icon: ScrollText },
  { key: "ledger", title: "存货明细账", description: "查看数量与金额明细账", icon: ListFilter },
  { key: "balance", title: "库存余额", description: "查看当前库存结存", icon: Boxes },
];

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

function statusText(status: string) {
  switch (status) {
    case "Completed":
      return "已完成";
    default:
      return status;
  }
}

function movementTypeText(type: string) {
  switch (type) {
    case "Receipt":
      return "入库";
    case "Issue":
      return "出库";
    case "TransferOut":
      return "调出";
    case "TransferIn":
      return "调入";
    case "CountIncrease":
      return "盘盈";
    case "CountDecrease":
      return "盘亏";
    default:
      return type;
  }
}

function documentTypeText(type: string) {
  switch (type) {
    case "InventoryReceipt":
      return "采购入库";
    case "InventoryIssue":
      return "销售出库";
    case "InventoryTransfer":
      return "库存调拨";
    case "InventoryCountAdjustment":
      return "库存盘点";
    case "ProductionIssue":
      return "生产领料";
    case "ProductionReceipt":
      return "完工入库";
    default:
      return type;
  }
}

/** 库存页面，覆盖采购收货、销售出库、调拨、盘点、台账、余额和库位管理。 */
export function InventoryPage() {
  const { hasPermission, user } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const canReadInventory = hasPermission(platformPermissions.inventoryRead);
  const canReadMasterData = hasPermission(platformPermissions.masterDataRead);
  const canManageReceipts = hasPermission(platformPermissions.inventoryReceiptManage);
  const canManageIssues = hasPermission(platformPermissions.inventoryIssueManage);
  const canManageTransfers = hasPermission(platformPermissions.inventoryTransferManage);
  const canManageCounts = hasPermission(platformPermissions.inventoryCountManage);
  const canManageLocations = hasPermission(platformPermissions.inventoryLocationManage);
  const canReadQuality = hasPermission(platformPermissions.qualityRead);
  const hasQualityModule = user?.visibleModuleKeys.includes("quality") ?? false;
  const canEnterQuality = canReadQuality && hasQualityModule;
  const [ledgerWarehouseId, setLedgerWarehouseId] = useState("");
  const [ledgerItemId, setLedgerItemId] = useState("");
  const loadLedger = useCallback(
    () =>
      canReadInventory
        ? api.listInventoryLedger({
            warehouseId: ledgerWarehouseId || undefined,
            itemId: ledgerItemId || undefined,
          })
        : loadEmptyLedger(),
    [canReadInventory, ledgerItemId, ledgerWarehouseId],
  );

  const pendingReceiptsQuery = useAsyncData(canReadInventory ? api.listPendingProcurementOrders : loadEmptyPendingReceipts);
  const pendingIssuesQuery = useAsyncData(canReadInventory ? api.listPendingSalesOrders : loadEmptyPendingIssues);
  const receiptsQuery = useAsyncData(canReadInventory ? api.listInventoryReceipts : loadEmptyReceipts);
  const issuesQuery = useAsyncData(canReadInventory ? api.listInventoryIssues : loadEmptyIssues);
  const transfersQuery = useAsyncData(canReadInventory ? api.listInventoryTransfers : loadEmptyTransfers);
  const countsQuery = useAsyncData(canReadInventory ? api.listInventoryCountAdjustments : loadEmptyCounts);
  const movementsQuery = useAsyncData(canReadInventory ? api.listInventoryMovements : loadEmptyMovements);
  const ledgerQuery = useAsyncData(loadLedger, `${canReadInventory}|${ledgerWarehouseId}|${ledgerItemId}`);
  const balancesQuery = useAsyncData(canReadInventory ? api.listStockBalances : loadEmptyBalances);
  const locationsQuery = useAsyncData(canReadInventory ? api.listWarehouseLocations : loadEmptyLocations);
  const locationBalancesQuery = useAsyncData(canReadInventory ? api.listLocationStockBalances : loadEmptyLocationBalances);
  const warehousesQuery = useAsyncData(canReadMasterData ? api.listWarehouses : loadEmptyWarehouses);
  const itemsQuery = useAsyncData(canReadMasterData ? api.listItems : loadEmptyItems);

  const initialPanel = searchParams.get("panel");
  const [selectedPanel, setSelectedPanel] = useState<InventoryPanelKey>(
    initialPanel && initialPanel in panelMeta ? (initialPanel as InventoryPanelKey) : "receipt",
  );
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [receiptWarehouses, setReceiptWarehouses] = useState<Record<string, string>>({});
  const [receiptLocations, setReceiptLocations] = useState<Record<string, string>>({});
  const [receiptUnitCosts, setReceiptUnitCosts] = useState<Record<string, Record<string, number>>>({});
  const [issueWarehouses, setIssueWarehouses] = useState<Record<string, string>>({});
  const [issueLocations, setIssueLocations] = useState<Record<string, string>>({});
  const [transferForm, setTransferForm] = useState({
    fromWarehouseId: "",
    toWarehouseId: "",
    fromLocationId: "",
    toLocationId: "",
    reason: "",
    itemId: "",
    quantity: 1,
    unit: "PCS",
  });
  const [countForm, setCountForm] = useState({
    warehouseId: "",
    locationId: "",
    reason: "",
    itemId: "",
    countedQuantity: 0,
    unitCost: 0,
  });
  const [locationForm, setLocationForm] = useState({
    warehouseId: "",
    code: "",
    name: "",
    isEnabled: true,
  });

  const panelRef = useRef<HTMLElement | null>(null);
  const warehouses = (warehousesQuery.data ?? []).filter((entry) => entry.isEnabled);
  const items = (itemsQuery.data ?? []).filter((entry) => entry.isEnabled);
  const pendingReceipts = pendingReceiptsQuery.data ?? [];
  const pendingIssues = pendingIssuesQuery.data ?? [];
  const receipts = receiptsQuery.data ?? [];
  const issues = issuesQuery.data ?? [];
  const transfers = transfersQuery.data ?? [];
  const counts = countsQuery.data ?? [];
  const movements = movementsQuery.data ?? [];
  const ledgerEntries = ledgerQuery.data ?? [];
  const balances = balancesQuery.data ?? [];
  const locations = locationsQuery.data ?? [];
  const enabledLocations = locations.filter((entry) => entry.isEnabled);
  const locationBalances = locationBalancesQuery.data ?? [];

  useEffect(() => {
    const panel = searchParams.get("panel");
    if (panel && panel in panelMeta && panel !== selectedPanel) {
      setSelectedPanel(panel as InventoryPanelKey);
    }
  }, [searchParams, selectedPanel]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      panelRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    }, 80);
    return () => window.clearTimeout(timer);
  }, [selectedPanel]);

  const kpis = useMemo(
    () => ({
      pendingReceiptCount: pendingReceipts.length,
      pendingIssueCount: pendingIssues.length,
      balanceCount: balances.length,
      movementCount: movements.length,
    }),
    [balances.length, movements.length, pendingIssues.length, pendingReceipts.length],
  );

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
    if (canReadInventory) {
      tasks.push(
        pendingReceiptsQuery.reload(),
        pendingIssuesQuery.reload(),
        receiptsQuery.reload(),
        issuesQuery.reload(),
        transfersQuery.reload(),
        countsQuery.reload(),
        movementsQuery.reload(),
        ledgerQuery.reload(),
        balancesQuery.reload(),
        locationsQuery.reload(),
        locationBalancesQuery.reload(),
      );
    }
    if (canReadMasterData) {
      tasks.push(warehousesQuery.reload(), itemsQuery.reload());
    }
    await Promise.all(tasks);
  }

  function switchPanel(panel: InventoryPanelKey) {
    setSelectedPanel(panel);
    setSearchParams({ panel });
  }

  if (!canReadInventory) {
    return (
      <PageShell title="库存执行台">
        <EmptyState title="无库存查看权限" description="当前账号不能读取库存执行、流水和余额信息。" />
      </PageShell>
    );
  }

  const selectedMeta = panelMeta[selectedPanel];
  const missingMasterData = warehouses.length === 0 || items.length === 0;
  const locationsForWarehouse = (warehouseId: string) =>
    enabledLocations.filter((entry) => entry.warehouseId === warehouseId);

  return (
    <PageShell
      title="库存执行台"
      actions={
        <button
          className="secondary icon-button"
          disabled={busyKey === "inventory-refresh"}
          onClick={async () => {
            await runAction("inventory-refresh", reloadAll, "库存执行数据已刷新。");
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
        <StatTile label="待入库" value={kpis.pendingReceiptCount} tone={kpis.pendingReceiptCount > 0 ? "warning" : "success"} />
        <StatTile label="待出库" value={kpis.pendingIssueCount} tone={kpis.pendingIssueCount > 0 ? "warning" : "success"} />
        <StatTile label="库存余额" value={kpis.balanceCount} tone={kpis.balanceCount > 0 ? "success" : "default"} />
        <StatTile label="库存流水" value={kpis.movementCount} tone={kpis.movementCount > 0 ? "success" : "default"} />
      </section>

      <SectionBlock title="执行导航" hint="库存模块不是单一入库页，点击下方导航卡切换到对应执行面板。">
        <div className="inventory-nav-grid">
          {panelCards.map((card) => {
            const Icon = card.icon;
            return (
              <button
                key={card.key}
                type="button"
                className={`inventory-nav-card${selectedPanel === card.key ? " active" : ""}`}
                onClick={() => switchPanel(card.key)}
              >
                <div className="inventory-nav-card-head">
                  <span className="inventory-nav-icon"><Icon size={18} /></span>
                  <strong>{card.title}</strong>
                </div>
                <p>{card.description}</p>
              </button>
            );
          })}
        </div>
      </SectionBlock>

      <AnimatePresence mode="wait" initial={false}>
        <motion.section
          key={selectedPanel}
          ref={panelRef}
          initial={{ opacity: 0, y: 18 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -12 }}
          transition={{ duration: 0.22 }}
        >
          <SectionBlock title={selectedMeta.title} hint={selectedMeta.hint}>
            {selectedPanel === "receipt" ? (
              <div className="inventory-surface-grid">
                <div className="inventory-surface">
                  {pendingReceipts.length > 0 ? (
                    <div className="table-shell">
                      {pendingReceipts.map((entry) => (
                        <div key={entry.procurementOrderId} className="review-card inventory-card">
                          <div>
                            <strong>{entry.procurementOrderNo}</strong>
                            <p>{entry.requestNo} · {entry.supplierName}</p>
                            <small>下达时间：{formatDate(entry.releasedAtUtc)}</small>
                            <small>待入库行数：{entry.lines.length}</small>
                            <div className="inventory-lines">
                              {entry.lines.map((line) => (
                                <span key={`${entry.procurementOrderId}-${line.itemId}`}>
                                  {line.itemCode} · {line.itemName} x {line.quantity} {line.unit}
                                  <input
                                    type="number"
                                    min={0}
                                    step="0.01"
                                    placeholder="单位成本"
                                    value={receiptUnitCosts[entry.procurementOrderId]?.[line.itemId] ?? 0}
                                    onChange={(event) => {
                                      const nextValue = Math.max(0, Number(event.target.value));
                                      setReceiptUnitCosts((current) => ({
                                        ...current,
                                        [entry.procurementOrderId]: {
                                          ...(current[entry.procurementOrderId] ?? {}),
                                          [line.itemId]: nextValue,
                                        },
                                      }));
                                    }}
                                  />
                                </span>
                              ))}
                            </div>
                          </div>
                          <div className="inventory-actions">
                            {canManageReceipts ? (
                              missingMasterData ? (
                                <EmptyState
                                  title="缺少仓库或物料"
                                  description="入库前需要先准备启用仓库与物料。"
                                  action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
                                />
                              ) : (
                                <>
                                  <select
                                    value={receiptWarehouses[entry.procurementOrderId] ?? ""}
                                    onChange={(event) => {
                                      setReceiptWarehouses((current) => ({
                                        ...current,
                                        [entry.procurementOrderId]: event.target.value,
                                      }));
                                      setReceiptLocations((current) => ({
                                        ...current,
                                        [entry.procurementOrderId]: "",
                                      }));
                                    }}
                                  >
                                    <option value="">选择入库仓库</option>
                                    {warehouses.map((warehouse) => (
                                      <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                                    ))}
                                  </select>
                                  <select
                                    value={receiptLocations[entry.procurementOrderId] ?? ""}
                                    disabled={!(receiptWarehouses[entry.procurementOrderId] ?? "")}
                                    onChange={(event) => {
                                      setReceiptLocations((current) => ({
                                        ...current,
                                        [entry.procurementOrderId]: event.target.value,
                                      }));
                                    }}
                                  >
                                    <option value="">不指定库位</option>
                                    {locationsForWarehouse(receiptWarehouses[entry.procurementOrderId] ?? "").map((location) => (
                                      <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                                    ))}
                                  </select>
                                  <button
                                    disabled={busyKey === `receipt-${entry.procurementOrderId}` || !(receiptWarehouses[entry.procurementOrderId] ?? "")}
                                    onClick={async () => {
                                      await runAction(`receipt-${entry.procurementOrderId}`, async () => {
                                        await api.receiveProcurementOrder({
                                          procurementOrderId: entry.procurementOrderId,
                                          warehouseId: receiptWarehouses[entry.procurementOrderId] ?? "",
                                          locationId: receiptLocations[entry.procurementOrderId] || null,
                                          costs: entry.lines.map((line) => ({
                                            itemId: line.itemId,
                                            unitCost: receiptUnitCosts[entry.procurementOrderId]?.[line.itemId] ?? 0,
                                          })),
                                        });
                                        setReceiptWarehouses((current) => {
                                          const next = { ...current };
                                          delete next[entry.procurementOrderId];
                                          return next;
                                        });
                                        setReceiptLocations((current) => {
                                          const next = { ...current };
                                          delete next[entry.procurementOrderId];
                                          return next;
                                        });
                                        setReceiptUnitCosts((current) => {
                                          const next = { ...current };
                                          delete next[entry.procurementOrderId];
                                          return next;
                                        });
                                        await reloadAll();
                                      }, `${entry.procurementOrderNo} 已完成入库。`);
                                    }}
                                  >
                                    执行入库
                                  </button>
                                </>
                              )
                            ) : (
                              <small>当前账号只能查看待入库订单，不能执行入库。</small>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState
                      title="暂无待入库订单"
                      description="采购订单下达后，会在这里等待真实入库。"
                      action={<Link to="/procurement"><button type="button">去采购管理</button></Link>}
                    />
                  )}
                </div>
                <div className="inventory-surface">
                  {receipts.length > 0 ? (
                    <div className="inventory-record-list">
                      {receipts.map((entry) => (
                        <div key={entry.id} className="inventory-record-row">
                          <div>
                            <strong>{entry.receiptNo}</strong>
                            <p>{entry.procurementOrderNo} · {entry.warehouseName}{entry.locationCode ? ` · ${entry.locationCode}` : ""}</p>
                          </div>
                          <div className="inventory-record-meta">
                            <small>{statusText(entry.status)}</small>
                            <small>{formatDate(entry.receivedAtUtc)}</small>
                            {canEnterQuality ? (
                              <Link to="/quality"><button type="button" className="secondary">去质量追溯</button></Link>
                            ) : null}
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState title="暂无入库记录" description="成功执行采购入库后，这里会形成真实历史。" />
                  )}
                </div>
              </div>
            ) : null}

            {selectedPanel === "issue" ? (
              <div className="inventory-surface-grid">
                <div className="inventory-surface">
                  {pendingIssues.length > 0 ? (
                    <div className="table-shell">
                      {pendingIssues.map((entry) => (
                        <div key={entry.salesOrderId} className="review-card inventory-card">
                          <div>
                            <strong>{entry.salesOrderNo}</strong>
                            <p>{entry.quotationNo} · {entry.customerName}</p>
                            <small>待出库时间：{formatDate(entry.readyAtUtc)}</small>
                            <small>待出库行数：{entry.lines.length}</small>
                            <div className="inventory-lines">
                              {entry.lines.map((line) => (
                                <span key={`${entry.salesOrderId}-${line.itemId}`}>
                                  {line.itemCode} · {line.itemName} x {line.quantity} {line.unit}
                                </span>
                              ))}
                            </div>
                          </div>
                          <div className="inventory-actions">
                            {canManageIssues ? (
                              missingMasterData ? (
                                <EmptyState
                                  title="缺少仓库或物料"
                                  description="出库前需要先准备主数据。"
                                  action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
                                />
                              ) : (
                                <>
                                  <select
                                    value={issueWarehouses[entry.salesOrderId] ?? ""}
                                    onChange={(event) => {
                                      setIssueWarehouses((current) => ({
                                        ...current,
                                        [entry.salesOrderId]: event.target.value,
                                      }));
                                      setIssueLocations((current) => ({
                                        ...current,
                                        [entry.salesOrderId]: "",
                                      }));
                                    }}
                                  >
                                    <option value="">选择出库仓库</option>
                                    {warehouses.map((warehouse) => (
                                      <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                                    ))}
                                  </select>
                                  <select
                                    value={issueLocations[entry.salesOrderId] ?? ""}
                                    disabled={!(issueWarehouses[entry.salesOrderId] ?? "")}
                                    onChange={(event) => {
                                      setIssueLocations((current) => ({
                                        ...current,
                                        [entry.salesOrderId]: event.target.value,
                                      }));
                                    }}
                                  >
                                    <option value="">不指定库位</option>
                                    {locationsForWarehouse(issueWarehouses[entry.salesOrderId] ?? "").map((location) => (
                                      <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                                    ))}
                                  </select>
                                  <button
                                    disabled={busyKey === `issue-${entry.salesOrderId}` || !(issueWarehouses[entry.salesOrderId] ?? "")}
                                    onClick={async () => {
                                      await runAction(`issue-${entry.salesOrderId}`, async () => {
                                        await api.issueSalesOrder({
                                          salesOrderId: entry.salesOrderId,
                                          warehouseId: issueWarehouses[entry.salesOrderId] ?? "",
                                          locationId: issueLocations[entry.salesOrderId] || null,
                                        });
                                        setIssueWarehouses((current) => {
                                          const next = { ...current };
                                          delete next[entry.salesOrderId];
                                          return next;
                                        });
                                        setIssueLocations((current) => {
                                          const next = { ...current };
                                          delete next[entry.salesOrderId];
                                          return next;
                                        });
                                        await reloadAll();
                                      }, `${entry.salesOrderNo} 已完成出库。`);
                                    }}
                                  >
                                    执行出库
                                  </button>
                                </>
                              )
                            ) : (
                              <small>当前账号只能查看待出库订单，不能执行出库。</small>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState
                      title="暂无待出库订单"
                      description="销售订单推进到待出库后，会在这里等待真实发货。"
                      action={<Link to="/sales"><button type="button">去销售管理</button></Link>}
                    />
                  )}
                </div>
                <div className="inventory-surface">
                  {issues.length > 0 ? (
                    <div className="inventory-record-list">
                      {issues.map((entry) => (
                        <div key={entry.id} className="inventory-record-row">
                          <div>
                            <strong>{entry.issueNo}</strong>
                            <p>{entry.salesOrderNo} · {entry.warehouseName}{entry.locationCode ? ` · ${entry.locationCode}` : ""}</p>
                          </div>
                          <div className="inventory-record-meta">
                            <small>{statusText(entry.status)}</small>
                            <small>{formatDate(entry.issuedAtUtc)}</small>
                            {canEnterQuality ? (
                              <Link to="/quality"><button type="button" className="secondary">去质量追溯</button></Link>
                            ) : null}
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState title="暂无出库记录" description="成功执行销售出库后，这里会形成真实历史。" />
                  )}
                </div>
              </div>
            ) : null}

            {selectedPanel === "transfer" ? (
              <div className="inventory-surface-grid">
                <div className="inventory-surface">
                  {canManageTransfers ? (
                    missingMasterData ? (
                      <EmptyState
                        title="缺少仓库或物料"
                        description="调拨前需要先准备启用仓库与物料。"
                        action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
                      />
                    ) : (
                      <form
                        className="stack-form inventory-form-panel"
                        onSubmit={async (event) => {
                          event.preventDefault();
                          if (!transferForm.fromWarehouseId || !transferForm.toWarehouseId || !transferForm.reason.trim() || !transferForm.itemId || transferForm.quantity <= 0 || !transferForm.unit.trim()) {
                            setError("请完整填写调出仓库、调入仓库、原因、物料、数量和单位。");
                            return;
                          }

                          await runAction("transfer-create", async () => {
                            await api.createInventoryTransfer({
                              fromWarehouseId: transferForm.fromWarehouseId,
                              toWarehouseId: transferForm.toWarehouseId,
                              fromLocationId: transferForm.fromLocationId || null,
                              toLocationId: transferForm.toLocationId || null,
                              reason: transferForm.reason.trim(),
                              lines: [{ itemId: transferForm.itemId, quantity: transferForm.quantity, unit: transferForm.unit.trim() }],
                            });
                            setTransferForm({ fromWarehouseId: "", toWarehouseId: "", fromLocationId: "", toLocationId: "", reason: "", itemId: "", quantity: 1, unit: "PCS" });
                            await reloadAll();
                          }, "库存调拨已完成。");
                        }}
                      >
                        <select value={transferForm.fromWarehouseId} onChange={(event) => setTransferForm({ ...transferForm, fromWarehouseId: event.target.value, fromLocationId: "" })}>
                          <option value="">选择调出仓库</option>
                          {warehouses.map((warehouse) => (
                            <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                          ))}
                        </select>
                        <select value={transferForm.fromLocationId} disabled={!transferForm.fromWarehouseId} onChange={(event) => setTransferForm({ ...transferForm, fromLocationId: event.target.value })}>
                          <option value="">不指定调出库位</option>
                          {locationsForWarehouse(transferForm.fromWarehouseId).map((location) => (
                            <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                          ))}
                        </select>
                        <select value={transferForm.toWarehouseId} onChange={(event) => setTransferForm({ ...transferForm, toWarehouseId: event.target.value, toLocationId: "" })}>
                          <option value="">选择调入仓库</option>
                          {warehouses.map((warehouse) => (
                            <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                          ))}
                        </select>
                        <select value={transferForm.toLocationId} disabled={!transferForm.toWarehouseId} onChange={(event) => setTransferForm({ ...transferForm, toLocationId: event.target.value })}>
                          <option value="">不指定调入库位</option>
                          {locationsForWarehouse(transferForm.toWarehouseId).map((location) => (
                            <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                          ))}
                        </select>
                        <input placeholder="调拨原因" value={transferForm.reason} onChange={(event) => setTransferForm({ ...transferForm, reason: event.target.value })} />
                        <select value={transferForm.itemId} onChange={(event) => setTransferForm({ ...transferForm, itemId: event.target.value, unit: items.find((item) => item.id === event.target.value)?.unit ?? transferForm.unit })}>
                          <option value="">选择物料</option>
                          {items.map((item) => (
                            <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                          ))}
                        </select>
                        <div className="inline-form">
                          <input type="number" min={0.0001} step="0.0001" value={transferForm.quantity} onChange={(event) => setTransferForm({ ...transferForm, quantity: Number(event.target.value) })} />
                          <input value={transferForm.unit} onChange={(event) => setTransferForm({ ...transferForm, unit: event.target.value })} />
                        </div>
                        <button type="submit" disabled={busyKey === "transfer-create"}>执行调拨</button>
                      </form>
                    )
                  ) : (
                    <EmptyState title="无调拨执行权限" description="当前账号只能查看库存调拨结果，不能新建调拨。" />
                  )}
                </div>
                <div className="inventory-surface">
                  {transfers.length > 0 ? (
                    <div className="inventory-record-list">
                      {transfers.map((entry) => (
                        <div key={entry.id} className="inventory-record-row">
                          <div>
                            <strong>{entry.transferNo}</strong>
                            <p>
                              {entry.fromWarehouseName}{entry.fromLocationCode ? ` · ${entry.fromLocationCode}` : ""} → {entry.toWarehouseName}{entry.toLocationCode ? ` · ${entry.toLocationCode}` : ""}
                            </p>
                            <small>{entry.reason}</small>
                          </div>
                          <div className="inventory-record-meta">
                            <small>{statusText(entry.status)}</small>
                            <small>{formatDate(entry.executedAtUtc)}</small>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState title="暂无调拨记录" description="执行仓间调拨后，这里会形成真实历史。" />
                  )}
                </div>
              </div>
            ) : null}

            {selectedPanel === "count" ? (
              <div className="inventory-surface-grid">
                <div className="inventory-surface">
                  {canManageCounts ? (
                    missingMasterData ? (
                      <EmptyState
                        title="缺少仓库或物料"
                        description="盘点前需要先准备启用仓库与物料。"
                        action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
                      />
                    ) : (
                      <form
                        className="stack-form inventory-form-panel"
                        onSubmit={async (event) => {
                          event.preventDefault();
                          if (!countForm.warehouseId || !countForm.reason.trim() || !countForm.itemId || countForm.countedQuantity < 0 || countForm.unitCost < 0) {
                            setError("请完整填写仓库、原因、物料和盘点数量。");
                            return;
                          }

                          await runAction("count-create", async () => {
                            await api.createInventoryCountAdjustment({
                              warehouseId: countForm.warehouseId,
                              locationId: countForm.locationId || null,
                              reason: countForm.reason.trim(),
                              lines: [{ itemId: countForm.itemId, countedQuantity: countForm.countedQuantity, unitCost: countForm.unitCost }],
                            });
                            setCountForm({ warehouseId: "", locationId: "", reason: "", itemId: "", countedQuantity: 0, unitCost: 0 });
                            await reloadAll();
                          }, "库存盘点已完成。");
                        }}
                      >
                        <select value={countForm.warehouseId} onChange={(event) => setCountForm({ ...countForm, warehouseId: event.target.value, locationId: "" })}>
                          <option value="">选择盘点仓库</option>
                          {warehouses.map((warehouse) => (
                            <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                          ))}
                        </select>
                        <select value={countForm.locationId} disabled={!countForm.warehouseId} onChange={(event) => setCountForm({ ...countForm, locationId: event.target.value })}>
                          <option value="">不指定库位</option>
                          {locationsForWarehouse(countForm.warehouseId).map((location) => (
                            <option key={location.id} value={location.id}>{location.code} · {location.name}</option>
                          ))}
                        </select>
                        <input placeholder="盘点原因" value={countForm.reason} onChange={(event) => setCountForm({ ...countForm, reason: event.target.value })} />
                        <select value={countForm.itemId} onChange={(event) => setCountForm({ ...countForm, itemId: event.target.value })}>
                          <option value="">选择物料</option>
                          {items.map((item) => (
                            <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                          ))}
                        </select>
                        <input type="number" min={0} step="0.0001" value={countForm.countedQuantity} onChange={(event) => setCountForm({ ...countForm, countedQuantity: Number(event.target.value) })} />
                        <input type="number" min={0} step="0.01" placeholder="盘盈单位成本" value={countForm.unitCost} onChange={(event) => setCountForm({ ...countForm, unitCost: Math.max(0, Number(event.target.value)) })} />
                        <button type="submit" disabled={busyKey === "count-create"}>执行盘点</button>
                      </form>
                    )
                  ) : (
                    <EmptyState title="无盘点执行权限" description="当前账号只能查看盘点结果，不能提交盘点调整。" />
                  )}
                </div>
                <div className="inventory-surface">
                  {counts.length > 0 ? (
                    <div className="inventory-record-list">
                      {counts.map((entry) => (
                        <div key={entry.id} className="inventory-record-row">
                          <div>
                            <strong>{entry.countNo}</strong>
                            <p>{entry.warehouseName}{entry.locationCode ? ` · ${entry.locationCode}` : ""} · {entry.reason}</p>
                            <small>{entry.lines.map((line) => `${line.itemCode} ${line.beforeQuantity}→${line.countedQuantity}，金额 ${formatMoney(line.costAmount)}`).join("，")}</small>
                          </div>
                          <div className="inventory-record-meta">
                            <small>{statusText(entry.status)}</small>
                            <small>{formatDate(entry.countedAtUtc)}</small>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState title="暂无盘点记录" description="执行库存盘点后，这里会形成真实历史。" />
                  )}
                </div>
              </div>
            ) : null}

            {selectedPanel === "location" ? (
              <div className="inventory-surface-grid">
                <div className="inventory-surface">
                  {canManageLocations ? (
                    warehouses.length === 0 ? (
                      <EmptyState
                        title="缺少仓库"
                        description="维护库位前需要先准备启用仓库。"
                        action={<Link to="/master-data"><button type="button">去主数据</button></Link>}
                      />
                    ) : (
                      <form
                        className="stack-form inventory-form-panel"
                        onSubmit={async (event) => {
                          event.preventDefault();
                          if (!locationForm.warehouseId || !locationForm.code.trim() || !locationForm.name.trim()) {
                            setError("请完整填写仓库、库位编码和库位名称。");
                            return;
                          }

                          await runAction("location-create", async () => {
                            await api.createWarehouseLocation({
                              warehouseId: locationForm.warehouseId,
                              code: locationForm.code.trim(),
                              name: locationForm.name.trim(),
                              isEnabled: locationForm.isEnabled,
                            });
                            setLocationForm({ warehouseId: "", code: "", name: "", isEnabled: true });
                            await reloadAll();
                          }, "库位已创建。");
                        }}
                      >
                        <select value={locationForm.warehouseId} onChange={(event) => setLocationForm({ ...locationForm, warehouseId: event.target.value })}>
                          <option value="">选择仓库</option>
                          {warehouses.map((warehouse) => (
                            <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                          ))}
                        </select>
                        <input placeholder="库位编码" value={locationForm.code} onChange={(event) => setLocationForm({ ...locationForm, code: event.target.value })} />
                        <input placeholder="库位名称" value={locationForm.name} onChange={(event) => setLocationForm({ ...locationForm, name: event.target.value })} />
                        <label className="checkbox-row">
                          <input type="checkbox" checked={locationForm.isEnabled} onChange={(event) => setLocationForm({ ...locationForm, isEnabled: event.target.checked })} />
                          启用库位
                        </label>
                        <button type="submit" disabled={busyKey === "location-create"}>创建库位</button>
                      </form>
                    )
                  ) : (
                    <EmptyState title="无库位维护权限" description="当前账号只能查看库位和库位库存，不能创建库位。" />
                  )}
                </div>
                <div className="inventory-surface">
                  {locations.length > 0 ? (
                    <div className="inventory-record-list">
                      {locations.map((entry) => (
                        <div key={entry.id} className="inventory-record-row">
                          <div>
                            <strong>{entry.code} · {entry.name}</strong>
                            <p>{entry.warehouseCode} · {entry.warehouseName}</p>
                          </div>
                          <div className="inventory-record-meta">
                            <small>{entry.isEnabled ? "已启用" : "已停用"}</small>
                            <small>{formatDate(entry.updatedAtUtc)}</small>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState title="暂无库位" description="创建库位后，入库、出库、调拨和盘点可以选择具体库位。" />
                  )}
                </div>
                <div className="inventory-surface inventory-surface-wide">
                  {locationBalances.length > 0 ? (
                    <div className="inventory-record-list">
                      {locationBalances.map((entry) => (
                        <div key={entry.id} className="inventory-record-row">
                          <div>
                            <strong>{entry.itemCode} · {entry.itemName}</strong>
                            <p className="inventory-warehouse-line"><MapPin size={14} /> {entry.warehouseCode} · {entry.warehouseName} · {entry.locationCode} · {entry.locationName}</p>
                          </div>
                          <div className="inventory-balance">
                            <strong>{entry.quantity}</strong>
                            <small>{entry.unit}</small>
                            <small>单位成本 {formatMoney(entry.unitCost)}</small>
                            <small>结存金额 {formatMoney(entry.inventoryValue)}</small>
                            <small>{formatDate(entry.updatedAtUtc)}</small>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState title="暂无库位库存" description="选择库位执行入库、调拨或盘点后，这里会显示库位级结存。" />
                  )}
                </div>
              </div>
            ) : null}

            {selectedPanel === "movement" ? (
              movements.length > 0 ? (
                <div className="inventory-record-list">
                  {movements.map((entry) => (
                    <div key={entry.id} className="inventory-record-row">
                      <div>
                        <strong>{entry.documentNo}</strong>
                        <p>{documentTypeText(entry.documentType)} · {entry.warehouseName}{entry.locationCode ? ` · ${entry.locationCode}` : ""} · {entry.itemCode} · {entry.itemName}</p>
                      </div>
                      <div className="inventory-record-meta">
                        <span className={`inventory-movement-chip ${entry.changeQuantity >= 0 ? "movement-positive" : "movement-negative"}`}>
                          {movementTypeText(entry.movementType)} {entry.changeQuantity > 0 ? "+" : ""}{entry.changeQuantity} {entry.unit}
                        </span>
                        <small>余额 {entry.balanceAfter} {entry.unit}</small>
                        <small>单位成本 {formatMoney(entry.unitCost)}</small>
                        <small>发生金额 {formatMoney(entry.costAmount)}</small>
                        <small>结存金额 {formatMoney(entry.balanceCostAfter)}</small>
                        <small>{formatDate(entry.occurredAtUtc)}</small>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title="暂无库存流水" description="入库、出库、调拨或盘点发生后，这里会形成真实流水。" />
              )
            ) : null}

            {selectedPanel === "ledger" ? (
              <div className="inventory-surface inventory-surface-wide">
                <div className="inventory-ledger-toolbar">
                  <select value={ledgerWarehouseId} onChange={(event) => setLedgerWarehouseId(event.target.value)}>
                    <option value="">全部仓库</option>
                    {warehouses.map((warehouse) => (
                      <option key={warehouse.id} value={warehouse.id}>{warehouse.code} · {warehouse.name}</option>
                    ))}
                  </select>
                  <select value={ledgerItemId} onChange={(event) => setLedgerItemId(event.target.value)}>
                    <option value="">全部物料</option>
                    {items.map((item) => (
                      <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                    ))}
                  </select>
                </div>
                {ledgerEntries.length > 0 ? (
                  <div className="inventory-record-list">
                    {ledgerEntries.map((entry) => (
                      <div key={entry.id} className="inventory-record-row inventory-ledger-row">
                        <div>
                          <strong>{entry.documentNo}</strong>
                          <p>
                            {documentTypeText(entry.documentType)} · {movementTypeText(entry.movementType)} · {entry.warehouseName}
                            {entry.locationCode ? ` · ${entry.locationCode}` : ""} · {entry.itemCode} · {entry.itemName}
                          </p>
                          <small>经办人：{entry.actor}</small>
                        </div>
                        <div className="inventory-ledger-figures">
                          <span>入库 {entry.inQuantity} {entry.unit}</span>
                          <span>出库 {entry.outQuantity} {entry.unit}</span>
                          <span>结存 {entry.balanceAfter} {entry.unit}</span>
                          <span>单位成本 {formatMoney(entry.unitCost)}</span>
                          <span>入库金额 {formatMoney(entry.inAmount)}</span>
                          <span>出库金额 {formatMoney(entry.outAmount)}</span>
                          <span>结存金额 {formatMoney(entry.balanceCostAfter)}</span>
                          <span>{formatDate(entry.occurredAtUtc)}</span>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    title={ledgerWarehouseId || ledgerItemId ? "筛选条件下暂无明细" : "暂无存货明细账"}
                    description="入库、出库、调拨、盘点或生产领料发生后，这里会按数量与金额形成可追溯明细。"
                  />
                )}
              </div>
            ) : null}

            {selectedPanel === "balance" ? (
              balances.length > 0 || locationBalances.length > 0 ? (
                <div className="inventory-record-list">
                  {balances.map((entry) => (
                    <div key={entry.id} className="inventory-record-row">
                      <div>
                        <strong>{entry.itemCode} · {entry.itemName}</strong>
                        <p className="inventory-warehouse-line"><WarehouseIcon size={14} /> {entry.warehouseCode} · {entry.warehouseName}</p>
                      </div>
                      <div className="inventory-balance">
                        <strong>{entry.quantity}</strong>
                        <small>{entry.unit}</small>
                        <small>单位成本 {formatMoney(entry.unitCost)}</small>
                        <small>结存金额 {formatMoney(entry.inventoryValue)}</small>
                        <small>{formatDate(entry.updatedAtUtc)}</small>
                      </div>
                    </div>
                  ))}
                  {locationBalances.map((entry) => (
                    <div key={`location-${entry.id}`} className="inventory-record-row">
                      <div>
                        <strong>{entry.itemCode} · {entry.itemName}</strong>
                        <p className="inventory-warehouse-line"><MapPin size={14} /> {entry.warehouseCode} · {entry.warehouseName} · {entry.locationCode} · {entry.locationName}</p>
                      </div>
                      <div className="inventory-balance">
                        <strong>{entry.quantity}</strong>
                        <small>{entry.unit}</small>
                        <small>单位成本 {formatMoney(entry.unitCost)}</small>
                        <small>结存金额 {formatMoney(entry.inventoryValue)}</small>
                        <small>{formatDate(entry.updatedAtUtc)}</small>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title="暂无库存余额" description="完成首次入库后，这里会按仓库和物料显示真实结存。" />
              )
            ) : null}
          </SectionBlock>
        </motion.section>
      </AnimatePresence>
    </PageShell>
  );
}
