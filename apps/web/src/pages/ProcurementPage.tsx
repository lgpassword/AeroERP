import { RefreshCcw } from "lucide-react";
import { useMemo, useState } from "react";
import { EmptyState, PageShell, SectionBlock } from "@aeroerp/ui-kit";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import type { Item, ProcurementOrder, ProcurementRequest, Supplier } from "../types/api";

const loadEmptySuppliers = () => Promise.resolve<Supplier[]>([]);
const loadEmptyItems = () => Promise.resolve<Item[]>([]);
const loadEmptyRequests = () => Promise.resolve<ProcurementRequest[]>([]);
const loadEmptyOrders = () => Promise.resolve<ProcurementOrder[]>([]);

function statusText(status: string) {
  switch (status) {
    case "Submitted":
      return "已提交";
    case "Approved":
      return "已通过";
    case "Rejected":
      return "已驳回";
    case "Ordered":
      return "已转订单";
    case "Created":
      return "已创建";
    case "Released":
      return "已下达";
    case "Received":
      return "已入库";
    default:
      return status;
  }
}

/** 采购页面，承接采购申请创建、审核转换和采购订单发布。 */
export function ProcurementPage() {
  const { hasPermission, user } = useAuth();
  const navigate = useNavigate();
  const canReadMasterData = hasPermission(platformPermissions.masterDataRead);
  const canReadProcurement = hasPermission(platformPermissions.procurementRead);
  const canCreateRequests = hasPermission(platformPermissions.procurementRequestCreate);
  const canReviewRequests = hasPermission(platformPermissions.procurementRequestReview);
  const canCreateOrders = hasPermission(platformPermissions.procurementOrderCreate);
  const canReleaseOrders = hasPermission(platformPermissions.procurementOrderRelease);
  const canReadInventory = hasPermission(platformPermissions.inventoryRead);
  const canManageReceipts = hasPermission(platformPermissions.inventoryReceiptManage);
  const canReadFinance = hasPermission(platformPermissions.financeRead);
  const hasInventoryModule = user?.visibleModuleKeys.includes("inventory") ?? false;
  const hasFinanceModule = user?.visibleModuleKeys.includes("finance") ?? false;
  const hasWorkflowModule = user?.visibleModuleKeys.includes("workflow") ?? false;

  const suppliersQuery = useAsyncData(canReadMasterData ? api.listSuppliers : loadEmptySuppliers);
  const itemsQuery = useAsyncData(canReadMasterData ? api.listItems : loadEmptyItems);
  const requestsQuery = useAsyncData(canReadProcurement ? api.listRequests : loadEmptyRequests);
  const ordersQuery = useAsyncData(canReadProcurement ? api.listOrders : loadEmptyOrders);

  const [form, setForm] = useState({ supplierId: "", title: "", itemId: "", quantity: 1, unit: "PCS", currencyCode: "CNY", taxInvoiceType: "增值税普通发票", taxRate: 0.13 });
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);

  const approvedRequests = useMemo(
    () => (requestsQuery.data ?? []).filter((x) => x.status === "Approved"),
    [requestsQuery.data],
  );

  const supplierCount = suppliersQuery.data?.length ?? 0;
  const itemCount = itemsQuery.data?.length ?? 0;
  const canCreateRequestForm = canCreateRequests && canReadMasterData && supplierCount > 0 && itemCount > 0;

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
      tasks.push(suppliersQuery.reload(), itemsQuery.reload());
    }
    if (canReadProcurement) {
      tasks.push(requestsQuery.reload(), ordersQuery.reload());
    }

    await Promise.all(tasks);
  }

  return (
    <PageShell
      title="采购管理"
      actions={(canReadMasterData || canReadProcurement) ? (
        <button
          className="secondary icon-button"
          disabled={busyKey === "procurement-refresh"}
          onClick={async () => {
            await runAction("procurement-refresh", reloadAll, "采购数据已刷新。");
          }}
        >
          <RefreshCcw size={16} />
          <span>刷新数据</span>
        </button>
      ) : undefined}
    >
      {message ? <div className="form-message success">{message}</div> : null}
      {error ? <div className="form-message error">{error}</div> : null}

      <div className="split-grid">
        <SectionBlock title="采购申请" hint="申请提交后进入审核，通过后才能转采购订单。">
          {!canReadProcurement ? (
            <EmptyState title="无采购查看权限" description="当前账号不能查看采购申请。" />
          ) : requestsQuery.loading ? (
            <div className="section-note">正在加载采购申请...</div>
          ) : requestsQuery.error ? (
            <div className="section-note error">{requestsQuery.error}</div>
          ) : requestsQuery.data && requestsQuery.data.length > 0 ? (
            <div className="table-shell">
              {requestsQuery.data.map((request) => (
                <div key={request.id} className="review-card">
                  <div>
                    <strong>{request.requestNo} · {request.title}</strong>
                    <p>{request.supplierName} · {request.lines.map((line) => `${line.itemName} x ${line.quantity}`).join("，")}</p>
                    <small>{statusText(request.status)}</small>
                  </div>
                  <div className="button-row">
                    {request.status === "Submitted" ? (
                      canReviewRequests ? (
                        hasWorkflowModule ? (
                          <button
                            className="secondary"
                            onClick={() => {
                              void navigate("/workflow");
                            }}
                          >
                            去审批中心
                          </button>
                        ) : (
                          <small>当前账号可审核，但没有审批中心模块入口。</small>
                        )
                      ) : (
                        <small>当前账号不能审核该申请。</small>
                      )
                    ) : null}
                    {request.status === "Approved" ? (
                      canCreateOrders ? (
                        <button
                          disabled={busyKey === `request-convert-${request.id}`}
                          onClick={async () => {
                            await runAction(`request-convert-${request.id}`, async () => {
                              await api.convertOrder(request.id);
                              await requestsQuery.reload();
                              await ordersQuery.reload();
                            }, `${request.requestNo} 已生成采购订单。`);
                          }}
                        >
                          生成订单
                        </button>
                      ) : (
                        <small>当前账号不能将申请转为订单。</small>
                      )
                    ) : null}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              title="暂无采购申请"
              description={canCreateRequests
                ? "当供应商和物料准备完成后，可以从这里发起第一张采购申请。"
                : "当前账号只能查看采购申请，暂无可显示记录。"}
            />
          )}

          {canCreateRequests ? (
            !canReadMasterData ? (
              <EmptyState title="缺少主数据读取权限" description="当前账号可发起采购申请，但无法读取供应商与物料，暂时不能录入申请。" />
            ) : suppliersQuery.loading || itemsQuery.loading ? (
              <div className="section-note">正在加载供应商和物料...</div>
            ) : suppliersQuery.error ? (
              <div className="section-note error">{suppliersQuery.error}</div>
            ) : itemsQuery.error ? (
              <div className="section-note error">{itemsQuery.error}</div>
            ) : canCreateRequestForm ? (
              <form
                className="stack-form"
                onSubmit={async (event) => {
                  event.preventDefault();
                  if (!form.supplierId || !form.title.trim() || !form.itemId || form.quantity <= 0 || !form.unit.trim()) {
                    setError("请完整填写供应商、主题、物料、数量和单位。");
                    return;
                  }

                  await runAction("request-create", async () => {
                    await api.createRequest({
                      supplierId: form.supplierId,
                      title: form.title.trim(),
                      currencyCode: form.currencyCode.trim(),
                      taxInvoiceType: form.taxInvoiceType.trim(),
                      taxRate: form.taxRate,
                      lines: [{ itemId: form.itemId, quantity: form.quantity, unit: form.unit.trim() }],
                    });
                    setForm({ supplierId: "", title: "", itemId: "", quantity: 1, unit: "PCS", currencyCode: "CNY", taxInvoiceType: "增值税普通发票", taxRate: 0.13 });
                    if (canReadProcurement) {
                      await requestsQuery.reload();
                    }
                  }, "采购申请已提交。");
                }}
              >
                <select value={form.supplierId} onChange={(e) => setForm({ ...form, supplierId: e.target.value })}>
                  <option value="">选择供应商</option>
                  {suppliersQuery.data?.map((supplier) => (
                    <option key={supplier.id} value={supplier.id}>{supplier.code} · {supplier.name}</option>
                  ))}
                </select>
                <input placeholder="申请主题" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
                <select value={form.itemId} onChange={(e) => setForm({ ...form, itemId: e.target.value })}>
                  <option value="">选择物料</option>
                  {itemsQuery.data?.map((item) => (
                    <option key={item.id} value={item.id}>{item.code} · {item.name}</option>
                  ))}
                </select>
                <div className="inline-form">
                  <input type="number" min={1} value={form.quantity} onChange={(e) => setForm({ ...form, quantity: Number(e.target.value) })} />
                  <input value={form.unit} onChange={(e) => setForm({ ...form, unit: e.target.value })} />
                </div>
                <div className="inline-form">
                  <input placeholder="币种" value={form.currencyCode} onChange={(e) => setForm({ ...form, currencyCode: e.target.value.toUpperCase() })} />
                  <input placeholder="税票类型" value={form.taxInvoiceType} onChange={(e) => setForm({ ...form, taxInvoiceType: e.target.value })} />
                  <input type="number" min={0} max={1} step="0.01" value={form.taxRate} onChange={(e) => setForm({ ...form, taxRate: Number(e.target.value) })} />
                </div>
                <button
                  type="submit"
                  disabled={busyKey === "request-create" || !form.supplierId || !form.title.trim() || !form.itemId || form.quantity <= 0 || !form.unit.trim()}
                >
                  提交申请
                </button>
              </form>
            ) : (
              <EmptyState title="缺少主数据" description="请先创建供应商和物料，再开启采购申请。" />
            )
          ) : canReadProcurement ? (
            <div className="section-note">当前账号只能查看采购申请，不能新建。</div>
          ) : null}
        </SectionBlock>

        <SectionBlock title="采购订单" hint="只有审核通过的申请才能转单，订单创建后还需要下达。">
          {!canReadProcurement ? (
            <EmptyState title="无采购查看权限" description="当前账号不能查看采购订单。" />
          ) : ordersQuery.loading ? (
            <div className="section-note">正在加载采购订单...</div>
          ) : ordersQuery.error ? (
            <div className="section-note error">{ordersQuery.error}</div>
          ) : ordersQuery.data && ordersQuery.data.length > 0 ? (
            <div className="table-shell">
              {ordersQuery.data.map((order) => (
                <div key={order.id} className="review-card">
                  <div>
                    <strong>{order.orderNo}</strong>
                    <p>{order.requestNo} · {order.supplierName}</p>
                    <small>{statusText(order.status)}</small>
                  </div>
                  <div className="button-row">
                    {order.status === "Created" ? (
                      canReleaseOrders ? (
                        <button
                          disabled={busyKey === `order-release-${order.id}`}
                          onClick={async () => {
                            await runAction(`order-release-${order.id}`, async () => {
                              await api.releaseOrder(order.id);
                              await ordersQuery.reload();
                            }, `${order.orderNo} 已下达。`);
                          }}
                        >
                          下达订单
                        </button>
                      ) : (
                        <small>当前账号不能下达该订单。</small>
                      )
                    ) : null}
                    {order.status === "Released" ? (
                      canReadInventory && hasInventoryModule ? (
                        <button
                          className="secondary"
                          onClick={() => {
                            void navigate("/inventory?panel=receipt");
                          }}
                        >
                          {canManageReceipts ? "去入库" : "查看入库"}
                        </button>
                      ) : (
                        <small>当前账号没有库存模块入口。</small>
                      )
                    ) : null}
                    {order.status === "Received" ? (
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
                        <small>订单已入库，当前账号没有财务模块入口。</small>
                      )
                    ) : null}
                  </div>
                </div>
              ))}
            </div>
          ) : approvedRequests.length > 0 ? (
            <EmptyState
              title="存在可转单申请"
              description={canCreateOrders
                ? "当前已有审核通过的申请，可以直接生成第一张采购订单。"
                : "当前已有审核通过的申请，但该账号没有转采购订单权限。"}
            />
          ) : (
            <EmptyState title="暂无采购订单" description="采购订单会在申请通过并完成转单后显示在这里。" />
          )}
        </SectionBlock>
      </div>
    </PageShell>
  );
}
