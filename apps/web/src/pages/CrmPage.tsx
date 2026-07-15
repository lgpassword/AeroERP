import { Link } from "react-router-dom";
import { ArrowRight, RefreshCcw, UsersRound } from "lucide-react";
import { EmptyState, PageShell, SectionBlock, StatTile } from "@aeroerp/ui-kit";
import { api } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { platformPermissions } from "../auth/permissions";
import { useAsyncData } from "../hooks/useAsyncData";
import { moduleRoutes } from "../modules/moduleNavigation";
import type { Customer, SalesOrder, SalesQuotation } from "../types/api";

const loadEmptyCustomers = () => Promise.resolve<Customer[]>([]);
const loadEmptyQuotations = () => Promise.resolve<SalesQuotation[]>([]);
const loadEmptyOrders = () => Promise.resolve<SalesOrder[]>([]);

/** CRM 插件页面，复用真实客户主数据和销售单据形成客户视图。 */
export function CrmPage() {
  const { user, hasPermission } = useAuth();
  const canReadMasterData = hasPermission(platformPermissions.masterDataRead);
  const canReadSales = hasPermission(platformPermissions.salesRead);
  const canOpenMasterData = user?.visibleModuleKeys.includes("master-data") ?? false;
  const canOpenSales = user?.visibleModuleKeys.includes("sales") ?? false;

  const customersQuery = useAsyncData(canReadMasterData ? api.listCustomers : loadEmptyCustomers, canReadMasterData ? "crm-customers" : "no-customers");
  const quotationsQuery = useAsyncData(canReadSales ? api.listSalesQuotations : loadEmptyQuotations, canReadSales ? "crm-quotations" : "no-quotations");
  const ordersQuery = useAsyncData(canReadSales ? api.listSalesOrders : loadEmptyOrders, canReadSales ? "crm-orders" : "no-orders");

  const customers = customersQuery.data ?? [];
  const quotations = quotationsQuery.data ?? [];
  const orders = ordersQuery.data ?? [];
  const enabledCustomers = customers.filter((customer) => customer.isEnabled);

  const customerPipeline = customers.map((customer) => {
    const customerQuotations = quotations.filter((quotation) => quotation.customerId === customer.id);
    const customerOrders = orders.filter((order) => order.customerId === customer.id);
    return {
      customer,
      quotationCount: customerQuotations.length,
      orderCount: customerOrders.length,
      openOrderCount: customerOrders.filter((order) => order.status !== "Completed").length,
    };
  });

  const orderCustomerCount = new Set(orders.map((order) => order.customerId)).size;
  const customerConversionRate = customers.length === 0 ? 0 : Math.round((orderCustomerCount / customers.length) * 100);

  async function reloadAll() {
    await Promise.all([
      customersQuery.reload(),
      quotationsQuery.reload(),
      ordersQuery.reload(),
    ]);
  }

  return (
    <PageShell
      title="客户CRM"
      actions={(
        <div className="button-row wrap">
          {canOpenMasterData ? (
            <Link className="button-link" to={moduleRoutes["master-data"]}>
              <UsersRound size={16} />
              <span>客户主数据</span>
            </Link>
          ) : null}
          {canOpenSales ? (
            <Link className="button-link" to={moduleRoutes.sales}>
              <ArrowRight size={16} />
              <span>销售管理</span>
            </Link>
          ) : null}
          <button type="button" className="secondary icon-button" onClick={reloadAll}>
            <RefreshCcw size={16} />
            <span>刷新数据</span>
          </button>
        </div>
      )}
    >
      <section className="stats-grid crm-summary-grid">
        <StatTile label="客户总数" value={customers.length} tone={customers.length > 0 ? "success" : "warning"} />
        <StatTile label="启用客户" value={enabledCustomers.length} tone={enabledCustomers.length > 0 ? "success" : "warning"} />
        <StatTile label="销售报价" value={quotations.length} tone={quotations.length > 0 ? "success" : "default"} />
        <StatTile label="成单覆盖率" value={`${customerConversionRate}%`} tone={customerConversionRate > 0 ? "success" : "warning"} />
      </section>

      <section className="crm-workspace-grid">
        <SectionBlock title="客户管道" hint="从客户主数据到报价、订单，展示真实销售链路覆盖情况。">
          {!canReadMasterData ? (
            <EmptyState title="缺少客户读取权限" description="当前账号无法读取客户主数据。" />
          ) : customersQuery.loading || quotationsQuery.loading || ordersQuery.loading ? (
            <div className="section-note">正在加载客户管道...</div>
          ) : customersQuery.error || quotationsQuery.error || ordersQuery.error ? (
            <div className="section-note error">{customersQuery.error ?? quotationsQuery.error ?? ordersQuery.error}</div>
          ) : customerPipeline.length > 0 ? (
            <div className="people-card-list crm-customer-list">
              {customerPipeline.map((entry) => (
                <div key={entry.customer.id} className="crm-customer-row">
                  <div className="crm-customer-main">
                    <span className="people-avatar"><UsersRound size={17} /></span>
                    <div>
                      <strong>{entry.customer.code} · {entry.customer.name}</strong>
                      <p>{entry.customer.contactName} · {entry.customer.phone}</p>
                      <small>{entry.customer.organizationName || "未绑定组织"} · {entry.customer.currencyCode}</small>
                    </div>
                  </div>
                  <div className="crm-pipeline-metrics">
                    <span>报价 {entry.quotationCount}</span>
                    <span>订单 {entry.orderCount}</span>
                    <span>未完 {entry.openOrderCount}</span>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无客户" description="请先在主数据中建立客户，再形成 CRM 客户池。" />
          )}
        </SectionBlock>

        <SectionBlock title="销售活动" hint="报价和订单来自销售管理模块，不复制销售单据。">
          {!canReadSales ? (
            <EmptyState title="缺少销售读取权限" description="当前账号无法读取销售报价和订单。" />
          ) : quotations.length > 0 || orders.length > 0 ? (
            <div className="people-card-list compact-list">
              {quotations.slice(0, 8).map((quotation) => (
                <div key={quotation.id} className="people-card-row">
                  <div>
                    <strong>{quotation.quotationNo} · {quotation.customerName}</strong>
                    <p>{quotation.title}</p>
                    <small>报价 · {quotation.status}</small>
                  </div>
                </div>
              ))}
              {orders.slice(0, 8).map((order) => (
                <div key={order.id} className="people-card-row">
                  <div>
                    <strong>{order.orderNo} · {order.customerName}</strong>
                    <p>{order.quotationNo}</p>
                    <small>订单 · {order.status}</small>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="暂无销售活动" description="客户报价或订单创建后，会在这里形成客户活动记录。" />
          )}
        </SectionBlock>
      </section>
    </PageShell>
  );
}
