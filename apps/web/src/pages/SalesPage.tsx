import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { Customer, Item, SalesOrder, SalesQuotation } from "../types/api";

const loadEmptyCustomers = () => Promise.resolve<Customer[]>([]);
const loadEmptyItems = () => Promise.resolve<Item[]>([]);
const loadEmptyQuotations = () => Promise.resolve<SalesQuotation[]>([]);
const loadEmptyOrders = () => Promise.resolve<SalesOrder[]>([]);

function salesStatusText(status: string) {
  switch (status) {
    case "Created":
      return "已创建";
    case "Converted":
      return "已转订单";
    case "Confirmed":
      return "已确认";
    case "ReadyToShip":
      return "待出库";
    case "Shipped":
      return "已出库";
    default:
      return status;
  }
}

/** 销售页面，承接报价创建、转销售订单、订单确认和待发货状态流转。 */
export function SalesPage() {
  const { hasPermission, user } = useAuth();
  const navigate = useNavigate();
  const canReadMasterData = hasPermission(platformPermissions.masterDataRead);
  const canReadSales = hasPermission(platformPermissions.salesRead);
  const canCreateQuotation = hasPermission(platformPermissions.salesQuotationCreate);
  const canCreateOrder = hasPermission(platformPermissions.salesOrderCreate);
  const canManageOrder = hasPermission(platformPermissions.salesOrderManage);
  const canReadInventory = hasPermission(platformPermissions.inventoryRead);
  const canReadFinance = hasPermission(platformPermissions.financeRead);
  const hasInventoryModule = user?.visibleModuleKeys.includes("inventory") ?? false;
  const hasFinanceModule = user?.visibleModuleKeys.includes("finance") ?? false;

  const customersQuery = useAsyncData(canReadMasterData ? api.listCustomers : loadEmptyCustomers);
  const itemsQuery = useAsyncData(canReadMasterData ? api.listItems : loadEmptyItems);
  const quotationsQuery = useAsyncData(canReadSales ? api.listSalesQuotations : loadEmptyQuotations);
  const ordersQuery = useAsyncData(canReadSales ? api.listSalesOrders : loadEmptyOrders);

  const [form, setForm] = useState({ customerId: "", title: "", itemId: "", quantity: 1, unit: "PCS", currencyCode: "CNY", taxInvoiceType: "增值税普通发票", taxRate: 0.13 });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const customerCount = customersQuery.data?.filter((entry) => entry.isEnabled).length ?? 0;
  const itemCount = itemsQuery.data?.filter((entry) => entry.isEnabled).length ?? 0;
  const readyCount = ordersQuery.data?.filter((entry) => entry.status === "ReadyToShip").length ?? 0;
  const canCreateQuotationForm = canCreateQuotation && canReadMasterData && customerCount > 0 && itemCount > 0;

  const convertibleQuotations = useMemo(
    () => (quotationsQuery.data ?? []).filter((entry) => entry.status === "Created"),
    [quotationsQuery.data],
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
    if (canReadMasterData) {
      tasks.push(customersQuery.reload(), itemsQuery.reload());
    }
    if (canReadSales) {
      tasks.push(quotationsQuery.reload(), ordersQuery.reload());
    }

    await Promise.all(tasks);
  }

  return (
    <PageShell
      title="销售管理"
      actions={(canReadMasterData || canReadSales) ? (
        <button
          className="secondary icon-button"
          disabled={busyKey === "sales-refresh"}
          onClick={async () => {
            await runAction("sales-refresh", reloadAll, "销售数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      ) : undefined}
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      {!canReadSales && !canCreateQuotation ? (
        <EmptyState title="无销售模块权限" description="当前账号不能查看或创建销售业务。" />
      ) : (
        <>
          <section className="stats-grid">
            <StatTile label="启用客户" value={customerCount} tone={customerCount > 0 ? "success" : "warning"} />
            <StatTile label="销售报价" value={quotationsQuery.data?.length ?? 0} tone={(quotationsQuery.data?.length ?? 0) > 0 ? "success" : "default"} />
            <StatTile label="销售订单" value={ordersQuery.data?.length ?? 0} tone={(ordersQuery.data?.length ?? 0) > 0 ? "success" : "default"} />
            <StatTile label="待出库" value={readyCount} tone={readyCount > 0 ? "warning" : "default"} />
          </section>

          <div className="split-grid">
            <SectionBlock title="销售报价" hint="本阶段先完成报价到销售订单闭环，所有数据均来自真实主数据。">
              {!canReadSales ? (
                <EmptyState title="无销售查看权限" description="当前账号不能查看销售报价。" />
              ) : quotationsQuery.loading ? (
                <div className="section-note">正在加载销售报价...</div>
              ) : quotationsQuery.error ? (
                <div className="section-note error">{quotationsQuery.error}</div>
              ) : quotationsQuery.data && quotationsQuery.data.length > 0 ? (
                <div className="table-shell">
                  {quotationsQuery.data.map((quotation) => (
                    <div key={quotation.id} className="review-card sales-card">
                      <div>
                        <strong>{quotation.quotationNo} · {quotation.title}</strong>
                        <p>{quotation.customerName} · {quotation.lines.map((line) => `${line.itemName} x ${line.quantity}`).join("，")}</p>
                        <small>{salesStatusText(quotation.status)}</small>
                      </div>
                      <div className="button-row">
                        {quotation.status === "Created" ? (
                          canCreateOrder ? (
                            <button
                              disabled={busyKey === `quotation-convert-${quotation.id}`}
                              onClick={async () => {
                                await runAction(`quotation-convert-${quotation.id}`, async () => {
                                  await api.convertSalesOrder(quotation.id);
                                  await quotationsQuery.reload();
                                  await ordersQuery.reload();
                                }, `${quotation.quotationNo} 已生成销售订单。`);
                              }}
                            >
                              生成销售订单
                            </button>
                          ) : (
                            <small>当前账号不能将报价转为订单。</small>
                          )
                        ) : null}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  title="暂无销售报价"
                  description={canCreateQuotation
                    ? "准备好客户和物料后，可以在这里创建第一张销售报价。"
                    : "当前账号暂无销售报价可查看。"}
                />
              )}

              {canCreateQuotation ? (
                !canReadMasterData ? (
                  <EmptyState title="缺少主数据读取权限" description="当前账号可创建销售报价，但无法读取客户和物料。" />
                ) : customersQuery.loading || itemsQuery.loading ? (
                  <div className="section-note">正在加载客户和物料...</div>
                ) : customersQuery.error ? (
                  <div className="section-note error">{customersQuery.error}</div>
                ) : itemsQuery.error ? (
                  <div className="section-note error">{itemsQuery.error}</div>
                ) : canCreateQuotationForm ? (
                  <form
                    className="stack-form"
                    onSubmit={async (event) => {
                      event.preventDefault();
                      if (!form.customerId || !form.title.trim() || !form.itemId || form.quantity <= 0 || !form.unit.trim()) {
                        setError("请完整填写客户、主题、物料、数量和单位。");
                        return;
                      }

                      await runAction("sales-quotation-create", async () => {
                        await api.createSalesQuotation({
                          customerId: form.customerId,
                          title: form.title.trim(),
                          currencyCode: form.currencyCode.trim(),
                          taxInvoiceType: form.taxInvoiceType.trim(),
                          taxRate: form.taxRate,
                          lines: [{ itemId: form.itemId, quantity: form.quantity, unit: form.unit.trim() }],
                        });
                        setForm({ customerId: "", title: "", itemId: "", quantity: 1, unit: "PCS", currencyCode: "CNY", taxInvoiceType: "增值税普通发票", taxRate: 0.13 });
                        if (canReadSales) {
                          await quotationsQuery.reload();
                        }
                      }, "销售报价已创建。");
                    }}
                  >
                    <select value={form.customerId} onChange={(event) => setForm({ ...form, customerId: event.target.value })}>
                      <option value="">选择客户</option>
                      {customersQuery.data?.filter((entry) => entry.isEnabled).map((customer) => (
                        <option key={customer.id} value={customer.id}>{customer.code} · {customer.name}</option>
                      ))}
                    </select>
                    <input placeholder="报价主题" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
                    <select value={form.itemId} onChange={(event) => setForm({ ...form, itemId: event.target.value })}>
                      <option value="">选择物料</option>
                      {itemsQuery.data?.filter((entry) => entry.isEnabled).map((item) => (
                        <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                      ))}
                    </select>
                    <div className="inline-form">
                      <input type="number" min={1} value={form.quantity} onChange={(event) => setForm({ ...form, quantity: Number(event.target.value) })} />
                      <input value={form.unit} onChange={(event) => setForm({ ...form, unit: event.target.value })} />
                    </div>
                    <div className="inline-form">
                      <input placeholder="币种" value={form.currencyCode} onChange={(event) => setForm({ ...form, currencyCode: event.target.value.toUpperCase() })} />
                      <input placeholder="税票类型" value={form.taxInvoiceType} onChange={(event) => setForm({ ...form, taxInvoiceType: event.target.value })} />
                      <input type="number" min={0} max={1} step="0.01" value={form.taxRate} onChange={(event) => setForm({ ...form, taxRate: Number(event.target.value) })} />
                    </div>
                    <button
                      type="submit"
                      disabled={busyKey === "sales-quotation-create" || !form.customerId || !form.title.trim() || !form.itemId || form.quantity <= 0 || !form.unit.trim()}
                    >
                      创建报价
                    </button>
                  </form>
                ) : (
                  <EmptyState title="缺少客户或物料" description="请先在主数据中建立启用客户和启用物料，再创建销售报价。" />
                )
              ) : canReadSales ? (
                <div className="section-note">当前账号只能查看销售报价，不能新建。</div>
              ) : null}
            </SectionBlock>

            <SectionBlock title="销售订单" hint="报价转单后，可逐步推进到确认和待出库状态。">
              {!canReadSales ? (
                <EmptyState title="无销售查看权限" description="当前账号不能查看销售订单。" />
              ) : ordersQuery.loading ? (
                <div className="section-note">正在加载销售订单...</div>
              ) : ordersQuery.error ? (
                <div className="section-note error">{ordersQuery.error}</div>
              ) : ordersQuery.data && ordersQuery.data.length > 0 ? (
                <div className="table-shell">
                  {ordersQuery.data.map((order) => (
                    <div key={order.id} className="review-card sales-card">
                      <div>
                        <strong>{order.orderNo}</strong>
                        <p>{order.quotationNo} · {order.customerName}</p>
                        <small>{salesStatusText(order.status)}</small>
                        <div className="inventory-lines">
                          {order.lines.map((line) => (
                            <span key={`${order.id}-${line.itemId}`}>
                              {line.itemCode} · {line.itemName} x {line.quantity} {line.unit}
                            </span>
                          ))}
                        </div>
                      </div>
                      <div className="inventory-actions">
                        {order.status === "Created" ? (
                          canManageOrder ? (
                            <button
                              disabled={busyKey === `sales-order-confirm-${order.id}`}
                              onClick={async () => {
                                await runAction(`sales-order-confirm-${order.id}`, async () => {
                                  await api.confirmSalesOrder(order.id);
                                  await ordersQuery.reload();
                                }, `${order.orderNo} 已确认。`);
                              }}
                            >
                              确认订单
                            </button>
                          ) : (
                            <small>当前账号不能确认订单。</small>
                          )
                        ) : null}
                        {order.status === "Confirmed" ? (
                          canManageOrder ? (
                            <button
                              disabled={busyKey === `sales-order-ready-${order.id}`}
                              onClick={async () => {
                                await runAction(`sales-order-ready-${order.id}`, async () => {
                                  await api.markSalesOrderReadyToShip(order.id);
                                  await ordersQuery.reload();
                                }, `${order.orderNo} 已进入待出库。`);
                              }}
                            >
                              进入待出库
                            </button>
                          ) : (
                            <small>当前账号不能推进订单状态。</small>
                          )
                        ) : null}
                        {order.status === "ReadyToShip" ? (
                          canReadInventory && hasInventoryModule ? (
                            <button
                              className="secondary"
                              onClick={() => {
                                void navigate("/inventory?panel=issue");
                              }}
                            >
                              去库存出库
                            </button>
                          ) : (
                            <small>当前订单已进入待出库，但当前账号没有库存模块入口。</small>
                          )
                        ) : null}
                        {order.status === "Shipped" ? (
                          canReadFinance && hasFinanceModule ? (
                            <button
                              className="secondary"
                              onClick={() => {
                                void navigate("/finance");
                              }}
                            >
                              去财务结算
                            </button>
                          ) : (
                            <small>订单已出库，当前账号没有财务模块入口。</small>
                          )
                        ) : null}
                      </div>
                    </div>
                  ))}
                </div>
              ) : convertibleQuotations.length > 0 ? (
                <EmptyState
                  title="存在可转单报价"
                  description={canCreateOrder
                    ? "当前已有报价可直接转为销售订单。"
                    : "当前已有报价，但该账号没有转销售订单权限。"}
                />
              ) : (
                <EmptyState title="暂无销售订单" description="销售订单会在报价转单后显示在这里。" />
              )}
            </SectionBlock>
          </div>
        </>
      )}
    </PageShell>
  );
}
