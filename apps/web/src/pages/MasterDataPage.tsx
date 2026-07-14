import { AnimatePresence, motion } from "framer-motion";
import { ArrowRight, Building2, Boxes, RefreshCcw, UsersRound, Warehouse as WarehouseIcon } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { Customer, Item, Supplier, Warehouse } from "../types/api";

type MasterPanelKey = "customers" | "suppliers" | "items" | "warehouses";

const loadEmptyCustomers = () => Promise.resolve<Customer[]>([]);
const loadEmptySuppliers = () => Promise.resolve<Supplier[]>([]);
const loadEmptyItems = () => Promise.resolve<Item[]>([]);
const loadEmptyWarehouses = () => Promise.resolve<Warehouse[]>([]);

const panelMeta: Record<MasterPanelKey, {
  title: string;
  shortTitle: string;
  emptyTitle: string;
  emptyDescription: string;
  readonlyDescription: string;
  createTitle: string;
}> = {
  customers: {
    title: "客户主数据",
    shortTitle: "客户",
    emptyTitle: "暂无客户",
    emptyDescription: "客户为空时，销售报价和销售订单都无法建立真实交易对象。",
    readonlyDescription: "当前账号只能查看客户结构分析，不能新增或维护客户。",
    createTitle: "新增客户",
  },
  suppliers: {
    title: "供应商主数据",
    shortTitle: "供应商",
    emptyTitle: "暂无供应商",
    emptyDescription: "供应商为空时，采购申请无法建立来源主体。",
    readonlyDescription: "当前账号只能查看供应商结构分析，不能新增或维护供应商。",
    createTitle: "新增供应商",
  },
  items: {
    title: "物料主数据",
    shortTitle: "物料",
    emptyTitle: "暂无物料",
    emptyDescription: "物料为空时，采购申请和库存记录都无法准确挂接。",
    readonlyDescription: "当前账号只能查看物料结构分析，不能新增或维护物料。",
    createTitle: "新增物料",
  },
  warehouses: {
    title: "仓库主数据",
    shortTitle: "仓库",
    emptyTitle: "暂无仓库",
    emptyDescription: "仓库为空时，采购订单已下达也无法完成真实入库。",
    readonlyDescription: "当前账号只能查看仓库结构分析，不能新增或维护仓库。",
    createTitle: "新增仓库",
  },
};

function clampPercent(value: number) {
  return Math.max(0, Math.min(100, Math.round(value)));
}

function enabledRate(enabledCount: number, totalCount: number) {
  if (totalCount <= 0) {
    return 0;
  }

  return clampPercent((enabledCount / totalCount) * 100);
}

function coverageRate(customerCount: number, supplierCount: number, itemCount: number, warehouseCount: number) {
  const readyCount = [customerCount, supplierCount, itemCount, warehouseCount].filter((value) => value > 0).length;
  return clampPercent((readyCount / 4) * 100);
}

function entityStatusText(count: number, enabledCount: number) {
  if (count === 0) {
    return "未建立";
  }

  if (enabledCount === 0) {
    return "全部停用";
  }

  if (enabledCount === count) {
    return "结构完整";
  }

  return "部分启用";
}

/** 主数据页面，维护客户、供应商、物料和仓库，作为业务单据的公共基础。 */
export function MasterDataPage() {
  const { hasPermission } = useAuth();
  const canReadMasterData = hasPermission(platformPermissions.masterDataRead);
  const canManageMasterData = hasPermission(platformPermissions.masterDataManage);

  const customersQuery = useAsyncData(canReadMasterData ? api.listCustomers : loadEmptyCustomers);
  const suppliersQuery = useAsyncData(canReadMasterData ? api.listSuppliers : loadEmptySuppliers);
  const itemsQuery = useAsyncData(canReadMasterData ? api.listItems : loadEmptyItems);
  const warehousesQuery = useAsyncData(canReadMasterData ? api.listWarehouses : loadEmptyWarehouses);

  const [selectedPanel, setSelectedPanel] = useState<MasterPanelKey | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [customerForm, setCustomerForm] = useState({
    code: "",
    name: "",
    contactName: "",
    phone: "",
    isEnabled: true,
  });
  const [supplierForm, setSupplierForm] = useState({
    code: "",
    name: "",
    contactName: "",
    phone: "",
    isEnabled: true,
  });
  const [itemForm, setItemForm] = useState({
    code: "",
    name: "",
    specification: "",
    unit: "",
    isEnabled: true,
  });
  const [warehouseForm, setWarehouseForm] = useState({
    code: "",
    name: "",
    location: "",
    isEnabled: true,
  });

  const maintenanceRef = useRef<HTMLElement | null>(null);

  const customers = customersQuery.data ?? [];
  const suppliers = suppliersQuery.data ?? [];
  const items = itemsQuery.data ?? [];
  const warehouses = warehousesQuery.data ?? [];

  const customerCount = customers.length;
  const supplierCount = suppliers.length;
  const itemCount = items.length;
  const warehouseCount = warehouses.length;
  const enabledCustomerCount = customers.filter((entry) => entry.isEnabled).length;
  const enabledSupplierCount = suppliers.filter((entry) => entry.isEnabled).length;
  const enabledItemCount = items.filter((entry) => entry.isEnabled).length;
  const enabledWarehouseCount = warehouses.filter((entry) => entry.isEnabled).length;
  const enabledTotal = enabledCustomerCount + enabledSupplierCount + enabledItemCount + enabledWarehouseCount;
  const totalEntities = customerCount + supplierCount + itemCount + warehouseCount;
  const overallEnabledRate = enabledRate(enabledTotal, totalEntities);
  const dependencyCoverage = coverageRate(customerCount, supplierCount, itemCount, warehouseCount);
  const maxEntityCount = Math.max(customerCount, supplierCount, itemCount, warehouseCount, 1);

  const readinessNotes = useMemo(() => {
    const notes: string[] = [];

    if (customerCount === 0) {
      notes.push("未建立客户，销售报价和销售订单无法指定交易对象。");
    }
    if (supplierCount === 0) {
      notes.push("未建立供应商，采购申请无法指定交易对象。");
    }
    if (itemCount === 0) {
      notes.push("未建立物料，采购申请与库存记录没有可挂接的编码。");
    }
    if (warehouseCount === 0) {
      notes.push("未建立仓库，采购订单下达后也不能执行入库。");
    }
    if (supplierCount > 0 && enabledSupplierCount === 0) {
      notes.push("供应商已存在但全部停用，采购入口会被结构性阻断。");
    }
    if (itemCount > 0 && enabledItemCount === 0) {
      notes.push("物料已存在但全部停用，申请和库存无法形成有效对象。");
    }
    if (warehouseCount > 0 && enabledWarehouseCount === 0) {
      notes.push("仓库已存在但全部停用，入库动作没有可落账位置。");
    }
    if (customerCount > 0 && enabledCustomerCount === 0) {
      notes.push("客户已存在但全部停用，销售链路没有可用交易对象。");
    }

    if (notes.length === 0) {
      notes.push("主数据结构已具备基础完备度，可以继续进行采购、销售与库存业务。");
    }

    return notes;
  }, [customerCount, enabledCustomerCount, enabledItemCount, enabledSupplierCount, enabledWarehouseCount, itemCount, supplierCount, warehouseCount]);

  const chartCards = useMemo(() => ([
    {
      key: "customers" as const,
      title: "客户结构",
      label: "客户",
      icon: UsersRound,
      count: customerCount,
      enabledCount: enabledCustomerCount,
      entityHint: "销售主体来源",
    },
    {
      key: "suppliers" as const,
      title: "供应商结构",
      label: "供应商",
      icon: Building2,
      count: supplierCount,
      enabledCount: enabledSupplierCount,
      entityHint: "采购主体来源",
    },
    {
      key: "items" as const,
      title: "物料结构",
      label: "物料",
      icon: Boxes,
      count: itemCount,
      enabledCount: enabledItemCount,
      entityHint: "采购与库存对象",
    },
    {
      key: "warehouses" as const,
      title: "仓库结构",
      label: "仓库",
      icon: WarehouseIcon,
      count: warehouseCount,
      enabledCount: enabledWarehouseCount,
      entityHint: "入库落账位置",
    },
  ]), [customerCount, enabledCustomerCount, enabledItemCount, enabledSupplierCount, enabledWarehouseCount, itemCount, supplierCount, warehouseCount]);

  const selectedMeta = selectedPanel ? panelMeta[selectedPanel] : null;

  useEffect(() => {
    if (!selectedPanel) {
      return;
    }

    const timer = window.setTimeout(() => {
      maintenanceRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    }, 120);

    return () => window.clearTimeout(timer);
  }, [selectedPanel]);

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
    if (!canReadMasterData) {
      return;
    }

    await Promise.all([
      customersQuery.reload(),
      suppliersQuery.reload(),
      itemsQuery.reload(),
      warehousesQuery.reload(),
    ]);
  }

  function openPanel(panel: MasterPanelKey) {
    setSelectedPanel(panel);
  }

  async function createCustomer() {
    const payload = {
      code: customerForm.code.trim(),
      name: customerForm.name.trim(),
      contactName: customerForm.contactName.trim(),
      phone: customerForm.phone.trim(),
      isEnabled: customerForm.isEnabled,
    };

    if (!payload.code || !payload.name || !payload.contactName || !payload.phone) {
      setError("请完整填写客户编码、名称、联系人和联系电话。");
      return;
    }

    await runAction("create-customer", async () => {
      await api.createCustomer(payload);
      setCustomerForm({ code: "", name: "", contactName: "", phone: "", isEnabled: true });
      await customersQuery.reload();
    }, "客户已创建并进入销售主数据结构。");
  }

  async function createSupplier() {
    const payload = {
      code: supplierForm.code.trim(),
      name: supplierForm.name.trim(),
      contactName: supplierForm.contactName.trim(),
      phone: supplierForm.phone.trim(),
      isEnabled: supplierForm.isEnabled,
    };

    if (!payload.code || !payload.name || !payload.contactName || !payload.phone) {
      setError("请完整填写供应商编码、名称、联系人和联系电话。");
      return;
    }

    await runAction("create-supplier", async () => {
      await api.createSupplier(payload);
      setSupplierForm({ code: "", name: "", contactName: "", phone: "", isEnabled: true });
      await suppliersQuery.reload();
    }, "供应商已创建并进入主数据结构。");
  }

  async function createItem() {
    const payload = {
      code: itemForm.code.trim(),
      name: itemForm.name.trim(),
      specification: itemForm.specification.trim(),
      unit: itemForm.unit.trim(),
      isEnabled: itemForm.isEnabled,
    };

    if (!payload.code || !payload.name || !payload.specification || !payload.unit) {
      setError("请完整填写物料编码、名称、规格和单位。");
      return;
    }

    await runAction("create-item", async () => {
      await api.createItem(payload);
      setItemForm({ code: "", name: "", specification: "", unit: "", isEnabled: true });
      await itemsQuery.reload();
    }, "物料已创建并进入采购与库存结构。");
  }

  async function createWarehouse() {
    const payload = {
      code: warehouseForm.code.trim(),
      name: warehouseForm.name.trim(),
      location: warehouseForm.location.trim(),
      isEnabled: warehouseForm.isEnabled,
    };

    if (!payload.code || !payload.name || !payload.location) {
      setError("请完整填写仓库编码、名称和库位说明。");
      return;
    }

    await runAction("create-warehouse", async () => {
      await api.createWarehouse(payload);
      setWarehouseForm({ code: "", name: "", location: "", isEnabled: true });
      await warehousesQuery.reload();
    }, "仓库已创建，后续可直接被入库流程引用。");
  }

  return (
    <PageShell
      title="主数据分析"
      actions={canReadMasterData ? (
        <button
          className="secondary icon-button"
          disabled={busyKey === "master-data-refresh"}
          onClick={async () => {
            await runAction("master-data-refresh", reloadAll, "主数据分析已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      ) : undefined}
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      {!canReadMasterData ? (
        <EmptyState title="无主数据查看权限" description="当前账号不能读取客户、供应商、物料和仓库分析信息。" />
      ) : (
        <>
          <section className="stats-grid">
            <StatTile label="客户" value={customerCount} tone={customerCount > 0 ? "success" : "warning"} />
            <StatTile label="供应商" value={supplierCount} tone={supplierCount > 0 ? "success" : "warning"} />
            <StatTile label="物料" value={itemCount} tone={itemCount > 0 ? "success" : "warning"} />
            <StatTile label="仓库" value={warehouseCount} tone={warehouseCount > 0 ? "success" : "warning"} />
          </section>

          <section className="stats-grid master-summary-grid">
            <StatTile label="链路完备度" value={`${dependencyCoverage}%`} tone={dependencyCoverage === 100 ? "success" : "warning"} />
            <StatTile label="启用覆盖率" value={`${overallEnabledRate}%`} tone={overallEnabledRate >= 70 ? "success" : "warning"} />
            <StatTile label="启用对象" value={enabledTotal} tone={enabledTotal > 0 ? "success" : "warning"} />
            <StatTile label="停用对象" value={Math.max(totalEntities - enabledTotal, 0)} tone={totalEntities === enabledTotal ? "success" : "warning"} />
          </section>

          <SectionBlock title="结构分析图" hint="先看结构分布，再点击图卡进入对应维护区。">
            <div className="master-dashboard-grid">
              {chartCards.map((card) => {
                const Icon = card.icon;
                const volumeRate = clampPercent((card.count / maxEntityCount) * 100);
                const activeRate = enabledRate(card.enabledCount, card.count);

                return (
                  <button
                    key={card.key}
                    type="button"
                    className={`master-chart-card${selectedPanel === card.key ? " active" : ""}`}
                    onClick={() => openPanel(card.key)}
                    aria-pressed={selectedPanel === card.key}
                  >
                    <div className="master-chart-card-head">
                      <div className="master-chart-card-title">
                        <span className="master-chart-icon"><Icon size={18} /></span>
                        <div>
                          <strong>{card.title}</strong>
                          <small>{card.entityHint}</small>
                        </div>
                      </div>
                      <ArrowRight size={16} />
                    </div>

                    <div className="master-chart-bars">
                      <div className="master-chart-bar">
                        <div className="master-chart-bar-label">
                          <span>总量</span>
                          <strong>{card.count}</strong>
                        </div>
                        <div className="master-chart-track">
                          <span className="master-chart-fill" style={{ width: `${volumeRate}%` }} />
                        </div>
                      </div>
                      <div className="master-chart-bar">
                        <div className="master-chart-bar-label">
                          <span>启用</span>
                          <strong>{card.enabledCount}</strong>
                        </div>
                        <div className="master-chart-track">
                          <span className="master-chart-fill accent" style={{ width: `${activeRate}%` }} />
                        </div>
                      </div>
                    </div>

                    <div className="master-chart-meta">
                      <span>{entityStatusText(card.count, card.enabledCount)}</span>
                      <span>{card.label}启用率 {activeRate}%</span>
                    </div>
                  </button>
                );
              })}
            </div>
          </SectionBlock>

          <SectionBlock title="链路关联" hint="主数据不是孤立录入项，它们直接决定采购、销售和库存链路是否可闭环。">
            <div className="master-flow-map">
              <button type="button" className="master-flow-node" onClick={() => openPanel("customers")}>
                <div className="master-flow-node-head">
                  <UsersRound size={18} />
                  <strong>客户</strong>
                </div>
                <p>{customerCount > 0 ? `${customerCount} 个客户已建档` : "未建立客户"}</p>
                <small>{enabledCustomerCount > 0 ? "可作为销售报价来源" : "无可用销售主体"}</small>
              </button>

              <div className="master-flow-link">
                <ArrowRight size={18} />
                <span>销售报价</span>
              </div>

              <button type="button" className="master-flow-node" onClick={() => openPanel("items")}>
                <div className="master-flow-node-head">
                  <Boxes size={18} />
                  <strong>物料</strong>
                </div>
                <p>{itemCount > 0 ? `${itemCount} 个物料已编码` : "未建立物料"}</p>
                <small>{enabledItemCount > 0 ? "可挂接销售、采购和库存行" : "无可用业务对象"}</small>
              </button>

              <div className="master-flow-link">
                <ArrowRight size={18} />
                <span>销售订单</span>
              </div>

              <button type="button" className="master-flow-node" onClick={() => openPanel("suppliers")}>
                <div className="master-flow-node-head">
                  <Building2 size={18} />
                  <strong>供应商</strong>
                </div>
                <p>{supplierCount > 0 ? `${supplierCount} 个主体已建档` : "未建立供应商"}</p>
                <small>{enabledSupplierCount > 0 ? "可作为采购申请来源" : "无可用采购主体"}</small>
              </button>

              <div className="master-flow-link">
                <ArrowRight size={18} />
                <span>采购申请</span>
              </div>

              <div className="master-flow-link">
                <ArrowRight size={18} />
                <span>入库落账</span>
              </div>

              <button type="button" className="master-flow-node" onClick={() => openPanel("warehouses")}>
                <div className="master-flow-node-head">
                  <WarehouseIcon size={18} />
                  <strong>仓库</strong>
                </div>
                <p>{warehouseCount > 0 ? `${warehouseCount} 个仓库已配置` : "未建立仓库"}</p>
                <small>{enabledWarehouseCount > 0 ? "采购入库可落到真实位置" : "无可用入库位置"}</small>
              </button>
            </div>

            <div className="master-insight-strip">
              {readinessNotes.map((note) => (
                <div key={note} className="section-note">{note}</div>
              ))}
            </div>
          </SectionBlock>

          <AnimatePresence mode="wait" initial={false}>
            <motion.section
              key={selectedPanel ?? "empty"}
              ref={maintenanceRef}
              initial={{ opacity: 0, y: 18 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -12 }}
              transition={{ duration: 0.22 }}
            >
              <SectionBlock
                title="维护工作台"
                hint={selectedMeta
                  ? `当前焦点：${selectedMeta.title}。点击上方图卡切换维护对象。`
                  : "点击上方图卡后，在这里查看结构清单并执行新增。"}
              >
                {!selectedPanel ? (
                  <EmptyState
                    title="先选择分析对象"
                    description="当前页面默认展示结构分析，不直接弹出录入表单。请点击上方图卡或链路节点进入维护区。"
                  />
                ) : (
                  <div className="master-maintenance-panel">
                    <div className="master-panel-header">
                      <div>
                        <h3>{selectedMeta?.title}</h3>
                        <p>当前维护区只展示真实数据，不注入任何演示记录。</p>
                      </div>
                    </div>

                    {!canManageMasterData ? (
                      <div className="section-note">{selectedMeta?.readonlyDescription}</div>
                    ) : null}

                    {selectedPanel === "customers" ? (
                      <div className="master-maintenance-grid">
                        <div className="master-entity-list">
                          {customersQuery.loading ? (
                            <div className="section-note">正在加载客户结构...</div>
                          ) : customersQuery.error ? (
                            <div className="section-note error">{customersQuery.error}</div>
                          ) : customers.length > 0 ? (
                            <div className="table-shell">
                              {customers.map((customer) => (
                                <div key={customer.id} className="master-entity-row">
                                  <div>
                                    <strong>{customer.code} · {customer.name}</strong>
                                    <p>{customer.contactName} · {customer.phone}</p>
                                  </div>
                                  <small>{customer.isEnabled ? "启用中" : "已停用"}</small>
                                </div>
                              ))}
                            </div>
                          ) : (
                            <EmptyState title={panelMeta.customers.emptyTitle} description={panelMeta.customers.emptyDescription} />
                          )}
                        </div>

                        {canManageMasterData ? (
                          <form
                            className="stack-form master-entry-form"
                            onSubmit={async (event) => {
                              event.preventDefault();
                              await createCustomer();
                            }}
                          >
                            <h4>{panelMeta.customers.createTitle}</h4>
                            <input
                              placeholder="客户编码"
                              value={customerForm.code}
                              onChange={(event) => setCustomerForm({ ...customerForm, code: event.target.value })}
                            />
                            <input
                              placeholder="客户名称"
                              value={customerForm.name}
                              onChange={(event) => setCustomerForm({ ...customerForm, name: event.target.value })}
                            />
                            <input
                              placeholder="联系人"
                              value={customerForm.contactName}
                              onChange={(event) => setCustomerForm({ ...customerForm, contactName: event.target.value })}
                            />
                            <input
                              placeholder="联系电话"
                              value={customerForm.phone}
                              onChange={(event) => setCustomerForm({ ...customerForm, phone: event.target.value })}
                            />
                            <label className="checkbox-row">
                              <input
                                type="checkbox"
                                checked={customerForm.isEnabled}
                                onChange={(event) => setCustomerForm({ ...customerForm, isEnabled: event.target.checked })}
                              />
                              <span>创建后立即启用</span>
                            </label>
                            <button type="submit" disabled={busyKey === "create-customer"}>保存客户</button>
                          </form>
                        ) : null}
                      </div>
                    ) : null}

                    {selectedPanel === "suppliers" ? (
                      <div className="master-maintenance-grid">
                        <div className="master-entity-list">
                          {suppliersQuery.loading ? (
                            <div className="section-note">正在加载供应商结构...</div>
                          ) : suppliersQuery.error ? (
                            <div className="section-note error">{suppliersQuery.error}</div>
                          ) : suppliers.length > 0 ? (
                            <div className="table-shell">
                              {suppliers.map((supplier) => (
                                <div key={supplier.id} className="master-entity-row">
                                  <div>
                                    <strong>{supplier.code} · {supplier.name}</strong>
                                    <p>{supplier.contactName} · {supplier.phone}</p>
                                  </div>
                                  <small>{supplier.isEnabled ? "启用中" : "已停用"}</small>
                                </div>
                              ))}
                            </div>
                          ) : (
                            <EmptyState title={panelMeta.suppliers.emptyTitle} description={panelMeta.suppliers.emptyDescription} />
                          )}
                        </div>

                        {canManageMasterData ? (
                          <form
                            className="stack-form master-entry-form"
                            onSubmit={async (event) => {
                              event.preventDefault();
                              await createSupplier();
                            }}
                          >
                            <h4>{panelMeta.suppliers.createTitle}</h4>
                            <input
                              placeholder="供应商编码"
                              value={supplierForm.code}
                              onChange={(event) => setSupplierForm({ ...supplierForm, code: event.target.value })}
                            />
                            <input
                              placeholder="供应商名称"
                              value={supplierForm.name}
                              onChange={(event) => setSupplierForm({ ...supplierForm, name: event.target.value })}
                            />
                            <input
                              placeholder="联系人"
                              value={supplierForm.contactName}
                              onChange={(event) => setSupplierForm({ ...supplierForm, contactName: event.target.value })}
                            />
                            <input
                              placeholder="联系电话"
                              value={supplierForm.phone}
                              onChange={(event) => setSupplierForm({ ...supplierForm, phone: event.target.value })}
                            />
                            <label className="checkbox-row">
                              <input
                                type="checkbox"
                                checked={supplierForm.isEnabled}
                                onChange={(event) => setSupplierForm({ ...supplierForm, isEnabled: event.target.checked })}
                              />
                              <span>创建后立即启用</span>
                            </label>
                            <button type="submit" disabled={busyKey === "create-supplier"}>保存供应商</button>
                          </form>
                        ) : null}
                      </div>
                    ) : null}

                    {selectedPanel === "items" ? (
                      <div className="master-maintenance-grid">
                        <div className="master-entity-list">
                          {itemsQuery.loading ? (
                            <div className="section-note">正在加载物料结构...</div>
                          ) : itemsQuery.error ? (
                            <div className="section-note error">{itemsQuery.error}</div>
                          ) : items.length > 0 ? (
                            <div className="table-shell">
                              {items.map((item) => (
                                <div key={item.id} className="master-entity-row">
                                  <div>
                                    <strong>{item.code} · {item.name}</strong>
                                    <p>{item.specification} · {item.unit}</p>
                                  </div>
                                  <small>{item.isEnabled ? "启用中" : "已停用"}</small>
                                </div>
                              ))}
                            </div>
                          ) : (
                            <EmptyState title={panelMeta.items.emptyTitle} description={panelMeta.items.emptyDescription} />
                          )}
                        </div>

                        {canManageMasterData ? (
                          <form
                            className="stack-form master-entry-form"
                            onSubmit={async (event) => {
                              event.preventDefault();
                              await createItem();
                            }}
                          >
                            <h4>{panelMeta.items.createTitle}</h4>
                            <input
                              placeholder="物料编码"
                              value={itemForm.code}
                              onChange={(event) => setItemForm({ ...itemForm, code: event.target.value })}
                            />
                            <input
                              placeholder="物料名称"
                              value={itemForm.name}
                              onChange={(event) => setItemForm({ ...itemForm, name: event.target.value })}
                            />
                            <input
                              placeholder="规格型号"
                              value={itemForm.specification}
                              onChange={(event) => setItemForm({ ...itemForm, specification: event.target.value })}
                            />
                            <input
                              placeholder="计量单位"
                              value={itemForm.unit}
                              onChange={(event) => setItemForm({ ...itemForm, unit: event.target.value })}
                            />
                            <label className="checkbox-row">
                              <input
                                type="checkbox"
                                checked={itemForm.isEnabled}
                                onChange={(event) => setItemForm({ ...itemForm, isEnabled: event.target.checked })}
                              />
                              <span>创建后立即启用</span>
                            </label>
                            <button type="submit" disabled={busyKey === "create-item"}>保存物料</button>
                          </form>
                        ) : null}
                      </div>
                    ) : null}

                    {selectedPanel === "warehouses" ? (
                      <div className="master-maintenance-grid">
                        <div className="master-entity-list">
                          {warehousesQuery.loading ? (
                            <div className="section-note">正在加载仓库结构...</div>
                          ) : warehousesQuery.error ? (
                            <div className="section-note error">{warehousesQuery.error}</div>
                          ) : warehouses.length > 0 ? (
                            <div className="table-shell">
                              {warehouses.map((warehouse) => (
                                <div key={warehouse.id} className="master-entity-row">
                                  <div>
                                    <strong>{warehouse.code} · {warehouse.name}</strong>
                                    <p>{warehouse.location}</p>
                                  </div>
                                  <small>{warehouse.isEnabled ? "启用中" : "已停用"}</small>
                                </div>
                              ))}
                            </div>
                          ) : (
                            <EmptyState title={panelMeta.warehouses.emptyTitle} description={panelMeta.warehouses.emptyDescription} />
                          )}
                        </div>

                        {canManageMasterData ? (
                          <form
                            className="stack-form master-entry-form"
                            onSubmit={async (event) => {
                              event.preventDefault();
                              await createWarehouse();
                            }}
                          >
                            <h4>{panelMeta.warehouses.createTitle}</h4>
                            <input
                              placeholder="仓库编码"
                              value={warehouseForm.code}
                              onChange={(event) => setWarehouseForm({ ...warehouseForm, code: event.target.value })}
                            />
                            <input
                              placeholder="仓库名称"
                              value={warehouseForm.name}
                              onChange={(event) => setWarehouseForm({ ...warehouseForm, name: event.target.value })}
                            />
                            <input
                              placeholder="库位说明"
                              value={warehouseForm.location}
                              onChange={(event) => setWarehouseForm({ ...warehouseForm, location: event.target.value })}
                            />
                            <label className="checkbox-row">
                              <input
                                type="checkbox"
                                checked={warehouseForm.isEnabled}
                                onChange={(event) => setWarehouseForm({ ...warehouseForm, isEnabled: event.target.checked })}
                              />
                              <span>创建后立即启用</span>
                            </label>
                            <button type="submit" disabled={busyKey === "create-warehouse"}>保存仓库</button>
                          </form>
                        ) : null}
                      </div>
                    ) : null}
                  </div>
                )}
              </SectionBlock>
            </motion.section>
          </AnimatePresence>
        </>
      )}
    </PageShell>
  );
}
