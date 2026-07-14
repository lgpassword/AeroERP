import type {
  AccountingAccount,
  AccountingPeriod,
  AgentReview,
  AdvancedManufacturingOverview,
  AnalyticsSnapshot,
  ApprovalTask,
  BankAccount,
  BankStatementLine,
  BarcodeExecution,
  Bom,
  Customer,
  CurrentUser,
  Currency,
  DocumentExchangeOverview,
  ExternalConnector,
  FinanceAgingSnapshot,
  FinanceInvoice,
  FinanceReportSnapshot,
  GeneralLedgerVoucher,
  LocalizationSettings,
  LocationStockBalance,
  LocalizationContent,
  InventoryReceipt,
  InventoryIssue,
  InventoryTransfer,
  InventoryCountAdjustment,
  InventoryLedgerEntry,
  InventoryMovement,
  Item,
  IntegrationOverview,
  IntegrationSyncJob,
  LotTrace,
  LotTraceEvent,
  LoginResponse,
  ModuleVisibility,
  MobileWorkDevice,
  MobileWorkOfflineTask,
  MobileWorkOverview,
  MobileWorkScanEvent,
  MessageChannel,
  NumberingRule,
  PendingInventoryIssue,
  OrganizationSummary,
  OutsourcingOrder,
  PendingInventoryReceipt,
  Payable,
  PickingTask,
  PickingWave,
  PositionDataScopeRule,
  PositionDepartment,
  JobPosition,
  PositionPermissionOverview,
  PositionPermissionPackage,
  PositionRole,
  PositionRoleBinding,
  PlanningSuggestion,
  ProductionIssue,
  ProductionReceipt,
  ProcurementOrder,
  ProcurementRequest,
  PutAwayTask,
  QualityInspection,
  QualitySourceCandidate,
  Receivable,
  ReportingOverview,
  RoleSummary,
  SalesOrder,
  SalesQuotation,
  Settlement,
  StockBalance,
  Supplier,
  UserSummary,
  Warehouse,
  WarehouseContainer,
  WarehouseLocation,
  WarehouseRoute,
  WebhookSubscription,
  WmsOverview,
  WorkOrder,
  WorkflowDefinition,
  WorkflowInstance,
  WorkflowNotification,
  DataScopeRule,
} from "../types/api";

// 当前 web app 的 API 边界集中在本文件，页面不直接拼接 fetch 细节。
const baseUrl = "http://localhost:5099";

let accessToken: string | null = null;

/** 更新 API 客户端持有的访问令牌，供登录、登出和会话恢复流程共享。 */
export function setAccessToken(token: string | null) {
  accessToken = token;
}

/** 统一处理 JSON 请求、Bearer Token、HTTP 错误和 204 空响应。 */
async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  if (!headers.has("Content-Type") && init?.body) {
    headers.set("Content-Type", "application/json");
  }

  if (accessToken) {
    headers.set("Authorization", `Bearer ${accessToken}`);
  }

  const response = await fetch(`${baseUrl}${path}`, { ...init, headers });
  if (!response.ok) {
    const maybeJson = await response.json().catch(() => null);
    throw new Error(maybeJson?.message ?? `请求失败：${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

/** AeroERP Web 使用的后端 API 门面，按平台、业务模块和闭环动作组织。 */
export const api = {
  // 平台认证、模块可见性、组织、角色和用户治理。
  login: (payload: { userName: string; password: string }) =>
    request<LoginResponse>("/api/platform/auth/login", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  me: () => request<CurrentUser>("/api/platform/auth/me"),
  listVisibleModules: () => request<ModuleVisibility[]>("/api/platform/visible-modules"),
  listModules: () => request<ModuleVisibility[]>("/api/platform/modules"),
  toggleModule: (id: string, isVisible: boolean) =>
    request<ModuleVisibility>(`/api/platform/modules/${id}/visibility`, {
      method: "PUT",
      body: JSON.stringify({ isVisible }),
    }),
  listOrganizations: () => request<OrganizationSummary[]>("/api/platform/organizations"),
  createOrganization: (payload: { name: string; defaultRole: string; regionCode: string }) =>
    request<OrganizationSummary>("/api/platform/organizations", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listRoles: () => request<RoleSummary[]>("/api/platform/roles"),
  listRoleOptions: () => request<RoleSummary[]>("/api/platform/role-options"),
  listUsers: () => request<UserSummary[]>("/api/platform/users"),
  createUser: (payload: { userName: string; displayName: string; password: string; isEnabled: boolean; roleIds: string[] }) =>
    request<UserSummary>("/api/platform/users", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  updateUserRoles: (id: string, roleIds: string[]) =>
    request<UserSummary>(`/api/platform/users/${id}/roles`, {
      method: "PUT",
      body: JSON.stringify({ roleIds }),
    }),
  updateUserStatus: (id: string, isEnabled: boolean) =>
    request<UserSummary>(`/api/platform/users/${id}/status`, {
      method: "PUT",
      body: JSON.stringify({ isEnabled }),
    }),
  resetUserPassword: (id: string, newPassword: string) =>
    request<void>(`/api/platform/users/${id}/reset-password`, {
      method: "POST",
      body: JSON.stringify({ newPassword }),
    }),
  changePassword: (payload: { currentPassword: string; newPassword: string }) =>
    request<void>("/api/platform/auth/change-password", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  updateRoleModules: (id: string, moduleKeys: string[]) =>
    request<RoleSummary>(`/api/platform/roles/${id}/modules`, {
      method: "PUT",
      body: JSON.stringify({ moduleKeys }),
    }),
  listAgentReviews: () => request<AgentReview[]>("/api/platform/agent-reviews"),
  submitAgentReview: (payload: { agentName: string; actionName: string; payload: string }) =>
    request<AgentReview>("/api/platform/agent-reviews", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  decideAgentReview: (id: string, decision: "Approved" | "Rejected", reviewerComment: string) =>
    request<AgentReview>(`/api/platform/agent-reviews/${id}/decision`, {
      method: "POST",
      body: JSON.stringify({ decision, reviewerComment }),
    }),
  // 主数据是采购、销售、库存、制造等模块的公共业务基础。
  listCustomers: () => request<Customer[]>("/api/master-data/customers"),
  createCustomer: (payload: {
    code: string;
    name: string;
    contactName: string;
    phone: string;
    isEnabled: boolean;
    organizationId?: string | null;
    currencyCode?: string;
    taxpayerId?: string;
    invoiceTitle?: string;
  }) =>
    request<Customer>("/api/master-data/customers", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listSuppliers: () => request<Supplier[]>("/api/master-data/suppliers"),
  createSupplier: (payload: {
    code: string;
    name: string;
    contactName: string;
    phone: string;
    isEnabled: boolean;
    organizationId?: string | null;
    currencyCode?: string;
    taxpayerId?: string;
    invoiceTitle?: string;
  }) =>
    request<Supplier>("/api/master-data/suppliers", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listItems: () => request<Item[]>("/api/master-data/items"),
  createItem: (payload: Omit<Item, "id">) =>
    request<Item>("/api/master-data/items", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listWarehouses: () => request<Warehouse[]>("/api/master-data/warehouses"),
  createWarehouse: (payload: {
    code: string;
    name: string;
    location: string;
    isEnabled: boolean;
    organizationId?: string | null;
  }) =>
    request<Warehouse>("/api/master-data/warehouses", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  // 采购从申请、审批转换到订单发布，后续衔接库存收货。
  listRequests: () => request<ProcurementRequest[]>("/api/procurement/requests"),
  createRequest: (payload: { supplierId: string; title: string; currencyCode?: string; taxInvoiceType?: string; taxRate?: number; lines: { itemId: string; quantity: number; unit: string }[] }) =>
    request<ProcurementRequest>("/api/procurement/requests", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  reviewRequest: (id: string, decision: "Approved" | "Rejected") =>
    request<ProcurementRequest>(`/api/procurement/requests/${id}/decision`, {
      method: "POST",
      body: JSON.stringify({ decision }),
    }),
  convertOrder: (id: string) =>
    request<ProcurementOrder>(`/api/procurement/requests/${id}/convert-order`, {
      method: "POST",
    }),
  listOrders: () => request<ProcurementOrder[]>("/api/procurement/orders"),
  releaseOrder: (id: string) =>
    request<ProcurementOrder>(`/api/procurement/orders/${id}/release`, {
      method: "POST",
    }),
  // 销售从报价转换为订单，并推动确认与待发货状态。
  listSalesQuotations: () => request<SalesQuotation[]>("/api/sales/quotations"),
  createSalesQuotation: (payload: { customerId: string; title: string; currencyCode?: string; taxInvoiceType?: string; taxRate?: number; lines: { itemId: string; quantity: number; unit: string }[] }) =>
    request<SalesQuotation>("/api/sales/quotations", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  convertSalesOrder: (id: string) =>
    request<SalesOrder>(`/api/sales/quotations/${id}/convert-order`, {
      method: "POST",
    }),
  listSalesOrders: () => request<SalesOrder[]>("/api/sales/orders"),
  confirmSalesOrder: (id: string) =>
    request<SalesOrder>(`/api/sales/orders/${id}/confirm`, {
      method: "POST",
    }),
  markSalesOrderReadyToShip: (id: string) =>
    request<SalesOrder>(`/api/sales/orders/${id}/ready-to-ship`, {
      method: "POST",
    }),
  // 库存闭环覆盖采购入库、销售出库、调拨、盘点、流水和库位余额。
  listPendingProcurementOrders: () =>
    request<PendingInventoryReceipt[]>("/api/inventory/pending-procurement-orders"),
  listPendingSalesOrders: () =>
    request<PendingInventoryIssue[]>("/api/inventory/pending-sales-orders"),
  listInventoryReceipts: () => request<InventoryReceipt[]>("/api/inventory/receipts"),
  receiveProcurementOrder: (payload: {
    procurementOrderId: string;
    warehouseId: string;
    locationId?: string | null;
    costs?: { itemId: string; unitCost: number }[] | null;
  }) =>
    request<InventoryReceipt>("/api/inventory/receipts", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listInventoryIssues: () => request<InventoryIssue[]>("/api/inventory/issues"),
  issueSalesOrder: (payload: { salesOrderId: string; warehouseId: string; locationId?: string | null }) =>
    request<InventoryIssue>("/api/inventory/issues", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listInventoryTransfers: () => request<InventoryTransfer[]>("/api/inventory/transfers"),
  createInventoryTransfer: (payload: {
    fromWarehouseId: string;
    toWarehouseId: string;
    fromLocationId?: string | null;
    toLocationId?: string | null;
    reason: string;
    lines: { itemId: string; quantity: number; unit: string }[];
  }) =>
    request<InventoryTransfer>("/api/inventory/transfers", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listInventoryCountAdjustments: () => request<InventoryCountAdjustment[]>("/api/inventory/counts"),
  createInventoryCountAdjustment: (payload: {
    warehouseId: string;
    locationId?: string | null;
    reason: string;
    lines: { itemId: string; countedQuantity: number; unitCost?: number | null }[];
  }) =>
    request<InventoryCountAdjustment>("/api/inventory/counts", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listInventoryMovements: () => request<InventoryMovement[]>("/api/inventory/movements"),
  listInventoryLedger: (filters?: { warehouseId?: string; itemId?: string }) => {
    const params = new URLSearchParams();
    if (filters?.warehouseId) {
      params.set("warehouseId", filters.warehouseId);
    }
    if (filters?.itemId) {
      params.set("itemId", filters.itemId);
    }

    const queryString = params.toString();
    return request<InventoryLedgerEntry[]>(queryString ? `/api/inventory/ledger?${queryString}` : "/api/inventory/ledger");
  },
  listStockBalances: () => request<StockBalance[]>("/api/inventory/balances"),
  listWarehouseLocations: () => request<WarehouseLocation[]>("/api/inventory/locations"),
  createWarehouseLocation: (payload: { warehouseId: string; code: string; name: string; isEnabled: boolean }) =>
    request<WarehouseLocation>("/api/inventory/locations", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listLocationStockBalances: () => request<LocationStockBalance[]>("/api/inventory/location-balances"),
  // 财务接口覆盖账簿、期间、凭证、报表、往来、发票、银行和结算。
  listAccountingAccounts: () => request<AccountingAccount[]>("/api/finance/accounting-accounts"),
  upsertAccountingAccount: (payload: {
    id?: string | null;
    code: string;
    name: string;
    type: string;
    parentAccountId?: string | null;
    isActive: boolean;
  }) =>
    request<AccountingAccount>("/api/finance/accounting-accounts", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listAccountingPeriods: () => request<AccountingPeriod[]>("/api/finance/accounting-periods"),
  createAccountingPeriod: (payload: { year: number; month: number }) =>
    request<AccountingPeriod>("/api/finance/accounting-periods", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  closeAccountingPeriod: (id: string) =>
    request<AccountingPeriod>(`/api/finance/accounting-periods/${id}/close`, {
      method: "POST",
    }),
  reopenAccountingPeriod: (id: string) =>
    request<AccountingPeriod>(`/api/finance/accounting-periods/${id}/reopen`, {
      method: "POST",
    }),
  listGeneralLedgerVouchers: () => request<GeneralLedgerVoucher[]>("/api/finance/vouchers"),
  createManualVoucher: (payload: {
    accountingPeriodId: string;
    documentDate: string;
    summary: string;
    lines: { accountingAccountId: string; summary: string; debitAmount: number; creditAmount: number }[];
  }) =>
    request<GeneralLedgerVoucher>("/api/finance/vouchers/manual", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createBusinessVoucher: (payload: {
    accountingPeriodId: string;
    documentDate: string;
    sourceType: "Payable" | "Receivable" | "Settlement";
    sourceId: string;
    debitAccountId: string;
    creditAccountId: string;
    summary: string;
  }) =>
    request<GeneralLedgerVoucher>("/api/finance/vouchers/from-business-document", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  submitGeneralLedgerVoucher: (id: string) =>
    request<GeneralLedgerVoucher>(`/api/finance/vouchers/${id}/submit`, {
      method: "POST",
    }),
  approveGeneralLedgerVoucher: (id: string, note: string) =>
    request<GeneralLedgerVoucher>(`/api/finance/vouchers/${id}/approve`, {
      method: "POST",
      body: JSON.stringify({ note }),
    }),
  rejectGeneralLedgerVoucher: (id: string, note: string) =>
    request<GeneralLedgerVoucher>(`/api/finance/vouchers/${id}/reject`, {
      method: "POST",
      body: JSON.stringify({ note }),
    }),
  getFinanceReportSnapshot: (accountingPeriodId?: string) =>
    request<FinanceReportSnapshot>(
      accountingPeriodId ? `/api/finance/reports?accountingPeriodId=${encodeURIComponent(accountingPeriodId)}` : "/api/finance/reports",
    ),
  getFinanceAging: () => request<FinanceAgingSnapshot>("/api/finance/aging"),
  listFinanceInvoices: () => request<FinanceInvoice[]>("/api/finance/invoices"),
  createFinanceInvoice: (payload: { direction: "Payable" | "Receivable"; sourceId: string; invoiceDate: string; note: string }) =>
    request<FinanceInvoice>("/api/finance/invoices", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listBankAccounts: () => request<BankAccount[]>("/api/finance/bank-accounts"),
  upsertBankAccount: (payload: {
    id?: string | null;
    accountNo: string;
    accountName: string;
    bankName: string;
    currencyCode: string;
    isEnabled: boolean;
  }) =>
    request<BankAccount>("/api/finance/bank-accounts", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listBankStatementLines: () => request<BankStatementLine[]>("/api/finance/bank-statement-lines"),
  createBankStatementLine: (payload: {
    bankAccountId: string;
    transactionDate: string;
    direction: "Inflow" | "Outflow";
    amount: number;
    counterpartyName: string;
    bankReferenceNo: string;
    summary: string;
  }) =>
    request<BankStatementLine>("/api/finance/bank-statement-lines", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  reconcileBankStatement: (payload: { bankStatementLineId: string; settlementId: string }) =>
    request<BankStatementLine>("/api/finance/bank-statement-lines/reconcile", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listPayables: () => request<Payable[]>("/api/finance/payables"),
  createPayableFromReceipt: (payload: { inventoryReceiptId: string; amount: number }) =>
    request<Payable>("/api/finance/payables/from-receipt", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createPayableFromOrder: (payload: { procurementOrderId: string; amount: number }) =>
    request<Payable>("/api/finance/payables/from-order", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listReceivables: () => request<Receivable[]>("/api/finance/receivables"),
  createReceivableFromIssue: (payload: { inventoryIssueId: string; amount: number }) =>
    request<Receivable>("/api/finance/receivables/from-issue", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createReceivableFromOrder: (payload: { salesOrderId: string; amount: number }) =>
    request<Receivable>("/api/finance/receivables/from-order", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listSettlements: () => request<Settlement[]>("/api/finance/settlements"),
  createSettlement: (payload: { targetType: "Payable" | "Receivable"; targetId: string; amount: number; bankAccountId: string; method: string; note: string }) =>
    request<Settlement>("/api/finance/settlements", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  // 工作流接口提供定义、实例、审批任务和通知状态处理。
  listWorkflowDefinitions: () => request<WorkflowDefinition[]>("/api/workflow/definitions"),
  listWorkflowInstances: () => request<WorkflowInstance[]>("/api/workflow/instances"),
  listApprovalTasks: () => request<ApprovalTask[]>("/api/workflow/tasks"),
  decideApprovalTask: (id: string, decision: "Approved" | "Rejected", comment: string) =>
    request<ApprovalTask>(`/api/workflow/tasks/${id}/decision`, {
      method: "POST",
      body: JSON.stringify({ decision, comment }),
    }),
  listWorkflowNotifications: () => request<WorkflowNotification[]>("/api/workflow/notifications"),
  markWorkflowNotification: (id: string, isRead: boolean) =>
    request<WorkflowNotification>(`/api/workflow/notifications/${id}/read-state`, {
      method: "PUT",
      body: JSON.stringify({ isRead }),
    }),
  // 经营管控和本地化配置影响权限数据范围、编号规则、币种和多语言内容。
  getAnalytics: () => request<AnalyticsSnapshot>("/api/control/analytics"),
  listDataScopeRules: () => request<DataScopeRule[]>("/api/control/data-scope-rules"),
  upsertDataScopeRule: (payload: { roleKey: string; scopeType: string; matchValue: string; description: string; isEnabled: boolean }) =>
    request<DataScopeRule>("/api/control/data-scope-rules", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listNumberingRules: () => request<NumberingRule[]>("/api/control/numbering-rules"),
  upsertNumberingRule: (payload: { documentType: string; prefix: string; useDateSegment: boolean; padding: number; isEnabled: boolean }) =>
    request<NumberingRule>("/api/control/numbering-rules", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listCurrencies: () => request<Currency[]>("/api/localization/currencies"),
  upsertCurrency: (payload: { code: string; name: string; symbol: string; exchangeRateToBase: number; isBase: boolean; isEnabled: boolean }) =>
    request<Currency>("/api/localization/currencies", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  getLocalizationSettings: () => request<LocalizationSettings>("/api/localization/settings"),
  updateLocalizationSettings: (payload: { defaultCurrencyCode: string; taxInvoiceType: string; taxpayerId: string; invoiceTitle: string; defaultTaxRate: number }) =>
    request<LocalizationSettings>("/api/localization/settings", {
      method: "PUT",
      body: JSON.stringify(payload),
    }),
  listLocalizationContent: () => request<LocalizationContent[]>("/api/localization/content"),
  upsertLocalizationContent: (payload: { key: string; category: string; chineseText: string; englishText: string; isEnabled: boolean }) =>
    request<LocalizationContent>("/api/localization/content", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  getPositionPermissionOverview: () => request<PositionPermissionOverview>("/api/position-permissions/overview"),
  upsertPositionDepartment: (payload: { id?: string | null; code: string; name: string; parentDepartmentId?: string | null; isEnabled: boolean }) =>
    request<PositionDepartment>("/api/position-permissions/departments", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertJobPosition: (payload: { id?: string | null; code: string; name: string; departmentId: string; description: string; isEnabled: boolean }) =>
    request<JobPosition>("/api/position-permissions/positions", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertPositionPermissionPackage: (payload: { id?: string | null; displayName: string; description: string; moduleKeys: string[]; permissions: string[]; isEnabled: boolean }) =>
    request<PositionPermissionPackage>("/api/position-permissions/permission-packages", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertCustomPositionRole: (payload: { id?: string | null; displayName: string; moduleKeys: string[]; permissions: string[] }) =>
    request<PositionRole>("/api/position-permissions/roles", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  updatePositionRoleBindings: (positionId: string, roleIds: string[]) =>
    request<PositionRoleBinding[]>(`/api/position-permissions/positions/${positionId}/role-bindings`, {
      method: "PUT",
      body: JSON.stringify({ roleIds }),
    }),
  updatePositionDataScopeRules: (positionId: string, rules: { scopeType: string; matchValue: string; description: string; isEnabled: boolean }[]) =>
    request<PositionDataScopeRule[]>(`/api/position-permissions/positions/${positionId}/data-scope-rules`, {
      method: "PUT",
      body: JSON.stringify({ rules }),
    }),
  // 制造主线覆盖 BOM、工单、领料和完工入库。
  listBoms: () => request<Bom[]>("/api/manufacturing/boms"),
  createBom: (payload: {
    finishedItemId: string;
    version: string;
    baseQuantity: number;
    isEnabled: boolean;
    lines: { componentItemId: string; quantity: number }[];
  }) =>
    request<Bom>("/api/manufacturing/boms", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listWorkOrders: () => request<WorkOrder[]>("/api/manufacturing/work-orders"),
  createWorkOrder: (payload: { bomId: string; plannedQuantity: number }) =>
    request<WorkOrder>("/api/manufacturing/work-orders", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  releaseWorkOrder: (id: string) =>
    request<WorkOrder>(`/api/manufacturing/work-orders/${id}/release`, {
      method: "POST",
    }),
  listProductionIssues: () => request<ProductionIssue[]>("/api/manufacturing/production-issues"),
  executeProductionIssue: (workOrderId: string, payload: { warehouseId: string }) =>
    request<ProductionIssue>(`/api/manufacturing/work-orders/${workOrderId}/issue`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listProductionReceipts: () => request<ProductionReceipt[]>("/api/manufacturing/production-receipts"),
  completeProduction: (workOrderId: string, payload: { warehouseId: string; quantity: number }) =>
    request<ProductionReceipt>(`/api/manufacturing/work-orders/${workOrderId}/complete`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  // 高级制造提供工艺路线、排程、产能、成本快照和 MRP 建议。
  getAdvancedManufacturingOverview: () => request<AdvancedManufacturingOverview>("/api/advanced-manufacturing/overview"),
  upsertWorkCenter: (payload: { code: string; name: string; warehouseId: string; capacityMinutesPerDay: number; hourlyCostRate: number; isEnabled: boolean }) =>
    request<AdvancedManufacturingOverview["workCenters"][number]>("/api/advanced-manufacturing/work-centers", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createManufacturingRouting: (payload: {
    finishedItemId: string;
    version: string;
    operations: { sequence: number; operationCode: string; operationName: string; workCenterId: string; standardMinutes: number; laborCostRate: number; machineCostRate: number }[];
  }) =>
    request<AdvancedManufacturingOverview["routings"][number]>("/api/advanced-manufacturing/routings", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  activateManufacturingRouting: (id: string) =>
    request<AdvancedManufacturingOverview["routings"][number]>(`/api/advanced-manufacturing/routings/${id}/activate`, {
      method: "POST",
    }),
  createOperationSchedule: (payload: { workOrderId: string; routingOperationId: string; plannedStartUtc: string; plannedEndUtc: string; plannedQuantity: number }) =>
    request<AdvancedManufacturingOverview["operationSchedules"][number]>("/api/advanced-manufacturing/operation-schedules", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  releaseOperationSchedule: (id: string) =>
    request<AdvancedManufacturingOverview["operationSchedules"][number]>(`/api/advanced-manufacturing/operation-schedules/${id}/release`, {
      method: "POST",
    }),
  completeOperationSchedule: (id: string, payload: { completedQuantity: number }) =>
    request<AdvancedManufacturingOverview["operationSchedules"][number]>(`/api/advanced-manufacturing/operation-schedules/${id}/complete`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertCapacityLoad: (payload: { workCenterId: string; planDate: string; availableMinutes: number; reservedMinutes: number; sourceDocumentNo: string }) =>
    request<AdvancedManufacturingOverview["capacityLoads"][number]>("/api/advanced-manufacturing/capacity-loads", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createManufacturingCostSnapshot: (payload: { workOrderId: string; materialCost: number; laborCost: number; machineCost: number; overheadCost: number }) =>
    request<AdvancedManufacturingOverview["costSnapshots"][number]>("/api/advanced-manufacturing/cost-snapshots", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  generateMrpSuggestion: (payload: { warehouseId: string; itemId: string; demandQuantity: number; supplyQuantity: number; sourceType: string }) =>
    request<AdvancedManufacturingOverview["mrpSuggestions"][number]>("/api/advanced-manufacturing/mrp-suggestions/generate", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  decideMrpSuggestion: (id: string, payload: { decision: "Accepted" | "Ignored"; note: string }) =>
    request<AdvancedManufacturingOverview["mrpSuggestions"][number]>(`/api/advanced-manufacturing/mrp-suggestions/${id}/decision`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  // 报表和质量追溯覆盖报表定义/执行/导出，以及检验和批次链路。
  getReportingOverview: () => request<ReportingOverview>("/api/reporting/overview"),
  upsertReportDefinition: (payload: { key: string; displayName: string; category: string; queryModel: string; parametersJson: string; isEnabled: boolean }) =>
    request<ReportingOverview["definitions"][number]>("/api/reporting/definitions", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  runReport: (payload: { reportDefinitionId: string; parametersJson: string }) =>
    request<ReportingOverview["runs"][number]>("/api/reporting/runs", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createReportExportTask: (payload: { reportRunRecordId: string; format: string }) =>
    request<ReportingOverview["exportTasks"][number]>("/api/reporting/export-tasks", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listQualitySourceCandidates: () => request<QualitySourceCandidate[]>("/api/quality/source-candidates"),
  listQualityInspections: () => request<QualityInspection[]>("/api/quality/inspections"),
  createQualityInspection: (payload: {
    sourceDocumentType: string;
    sourceDocumentId: string;
    itemId: string;
    inspectedQuantity: number;
    acceptedQuantity: number;
    rejectedQuantity: number;
    disposition: string;
    note: string;
  }) =>
    request<QualityInspection>("/api/quality/inspections", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listLotTraceEvents: () => request<LotTraceEvent[]>("/api/quality/lot-trace-events"),
  createLotTraceEvent: (payload: {
    lotNo: string;
    eventType: string;
    sourceDocumentType: string;
    sourceDocumentId: string;
    itemId: string;
    quantity: number;
    targetDocumentType: string;
    targetDocumentId?: string | null;
    targetDocumentNo: string;
    note: string;
  }) =>
    request<LotTraceEvent>("/api/quality/lot-trace-events", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  getLotTrace: (lotNo: string) => request<LotTrace>(`/api/quality/lots/${encodeURIComponent(lotNo)}`),
  // 计划执行覆盖补货建议、外协订单和条码执行记录。
  listPlanningSuggestions: () => request<PlanningSuggestion[]>("/api/planning/suggestions"),
  generatePlanningSuggestion: (payload: { warehouseId: string; itemId: string; minimumQuantity: number }) =>
    request<PlanningSuggestion>("/api/planning/suggestions/generate", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  decidePlanningSuggestion: (id: string, payload: { decision: "Accepted" | "Ignored"; note: string }) =>
    request<PlanningSuggestion>(`/api/planning/suggestions/${id}/decision`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listOutsourcingOrders: () => request<OutsourcingOrder[]>("/api/planning/outsourcing-orders"),
  createOutsourcingOrder: (payload: {
    supplierName: string;
    warehouseId: string;
    finishedItemId: string;
    plannedQuantity: number;
    materialLines: { itemId: string; quantity: number }[];
  }) =>
    request<OutsourcingOrder>("/api/planning/outsourcing-orders", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  issueOutsourcingMaterials: (id: string) =>
    request<OutsourcingOrder>(`/api/planning/outsourcing-orders/${id}/issue-materials`, {
      method: "POST",
    }),
  receiveOutsourcingOrder: (id: string, payload: { quantity: number }) =>
    request<OutsourcingOrder>(`/api/planning/outsourcing-orders/${id}/receive`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  listBarcodeExecutions: () => request<BarcodeExecution[]>("/api/planning/barcode-executions"),
  executeBarcode: (payload: { barcode: string; action: string; documentId?: string | null; note: string }) =>
    request<BarcodeExecution>("/api/planning/barcode-executions", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  // WMS 和移动作业承接仓内任务、波次、容器路线、离线任务和扫码事件。
  getWmsOverview: () => request<WmsOverview>("/api/wms/overview"),
  upsertWmsContainer: (payload: { code: string; containerType: string; warehouseId: string; currentLocationId?: string | null; status: string }) =>
    request<WarehouseContainer>("/api/wms/containers", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertWmsRoute: (payload: { warehouseId: string; fromLocationId: string; toLocationId: string; distanceMeters: number; priority: number; isEnabled: boolean }) =>
    request<WarehouseRoute>("/api/wms/routes", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createWmsPutAwayTask: (payload: { warehouseId: string; itemId: string; quantity: number; suggestedLocationId?: string | null; containerCode: string; sourceDocumentNo: string; assignedTo: string }) =>
    request<PutAwayTask>("/api/wms/put-away-tasks", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  completeWmsPutAwayTask: (id: string, payload: { targetLocationId: string }) =>
    request<PutAwayTask>(`/api/wms/put-away-tasks/${id}/complete`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createWmsPickingTask: (payload: { warehouseId: string; itemId: string; quantity: number; sourceLocationId?: string | null; assignedTo: string }) =>
    request<PickingTask>("/api/wms/picking-tasks", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  completeWmsPickingTask: (id: string, payload: { note: string }) =>
    request<PickingTask>(`/api/wms/picking-tasks/${id}/complete`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createWmsWave: (payload: { warehouseId: string; pickingTaskIds: string[] }) =>
    request<PickingWave>("/api/wms/waves", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  releaseWmsWave: (id: string) =>
    request<PickingWave>(`/api/wms/waves/${id}/release`, {
      method: "POST",
    }),
  getMobileWorkOverview: () => request<MobileWorkOverview>("/api/mobile-work/overview"),
  upsertMobileDevice: (payload: { deviceCode: string; displayName: string; assignedTo: string; isEnabled: boolean }) =>
    request<MobileWorkDevice>("/api/mobile-work/devices", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createMobileOfflineTask: (payload: { sourceModule: string; sourceTaskType: string; sourceTaskNo: string; payloadJson: string; assignedTo: string }) =>
    request<MobileWorkOfflineTask>("/api/mobile-work/offline-tasks", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  syncMobileOfflineTask: (id: string) =>
    request<MobileWorkOfflineTask>(`/api/mobile-work/offline-tasks/${id}/sync`, {
      method: "POST",
    }),
  completeMobileOfflineTask: (id: string) =>
    request<MobileWorkOfflineTask>(`/api/mobile-work/offline-tasks/${id}/complete`, {
      method: "POST",
    }),
  recordMobileScanEvent: (payload: { deviceCode: string; barcode: string; targetModule: string; action: string; documentNo: string; result: string; message: string }) =>
    request<MobileWorkScanEvent>("/api/mobile-work/scan-events", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  // 集成和文档交换处理外部通道、同步任务、Webhook、导入导出和打印任务。
  getIntegrationOverview: () => request<IntegrationOverview>("/api/integration/overview"),
  upsertIntegrationChannel: (payload: { channelKey: string; displayName: string; channelType: string; endpoint: string; isEnabled: boolean }) =>
    request<MessageChannel>("/api/integration/channels", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertIntegrationWebhook: (payload: { subscriptionKey: string; displayName: string; eventKey: string; targetUrl: string; secretName: string; isEnabled: boolean }) =>
    request<WebhookSubscription>("/api/integration/webhooks", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertIntegrationConnector: (payload: { connectorKey: string; displayName: string; provider: string; baseUrl: string; authMode: string; isEnabled: boolean }) =>
    request<ExternalConnector>("/api/integration/connectors", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createIntegrationSyncJob: (payload: { connectorKey: string; direction: string; payloadJson: string }) =>
    request<IntegrationSyncJob>("/api/integration/sync-jobs", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  startIntegrationSyncJob: (id: string) =>
    request<IntegrationSyncJob>(`/api/integration/sync-jobs/${id}/start`, {
      method: "POST",
    }),
  completeIntegrationSyncJob: (id: string) =>
    request<IntegrationSyncJob>(`/api/integration/sync-jobs/${id}/complete`, {
      method: "POST",
    }),
  failIntegrationSyncJob: (id: string, error: string) =>
    request<IntegrationSyncJob>(`/api/integration/sync-jobs/${id}/fail`, {
      method: "POST",
      body: JSON.stringify({ error }),
    }),
  retryIntegrationSyncJob: (id: string) =>
    request<IntegrationSyncJob>(`/api/integration/sync-jobs/${id}/retry`, {
      method: "POST",
    }),
  getDocumentExchangeOverview: () => request<DocumentExchangeOverview>("/api/document-exchange/overview"),
  upsertImportTemplate: (payload: { templateKey: string; displayName: string; targetModule: string; fileType: string; isEnabled: boolean }) =>
    request<DocumentExchangeOverview["importTemplates"][number]>("/api/document-exchange/import-templates", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  upsertImportFieldMapping: (payload: { templateKey: string; sourceField: string; targetField: string; isRequired: boolean; transformRule: string }) =>
    request<DocumentExchangeOverview["fieldMappings"][number]>("/api/document-exchange/field-mappings", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createImportBatch: (payload: { templateKey: string; fileName: string }) =>
    request<DocumentExchangeOverview["importBatches"][number]>("/api/document-exchange/import-batches", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  completeImportBatch: (id: string, payload: { rowCount: number; errorCount: number }) =>
    request<DocumentExchangeOverview["importBatches"][number]>(`/api/document-exchange/import-batches/${id}/complete`, {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  failImportBatch: (id: string, error: string) =>
    request<DocumentExchangeOverview["importBatches"][number]>(`/api/document-exchange/import-batches/${id}/fail`, {
      method: "POST",
      body: JSON.stringify({ error }),
    }),
  createExportFileTask: (payload: { sourceModule: string; fileName: string; format: string }) =>
    request<DocumentExchangeOverview["exportTasks"][number]>("/api/document-exchange/export-tasks", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  completeExportFileTask: (id: string) =>
    request<DocumentExchangeOverview["exportTasks"][number]>(`/api/document-exchange/export-tasks/${id}/complete`, {
      method: "POST",
    }),
  failExportFileTask: (id: string, error: string) =>
    request<DocumentExchangeOverview["exportTasks"][number]>(`/api/document-exchange/export-tasks/${id}/fail`, {
      method: "POST",
      body: JSON.stringify({ error }),
    }),
  upsertPrintTemplate: (payload: { templateKey: string; displayName: string; targetModule: string; contentType: string; templateBody: string; isEnabled: boolean }) =>
    request<DocumentExchangeOverview["printTemplates"][number]>("/api/document-exchange/print-templates", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  createPrintJob: (payload: { templateKey: string; documentNo: string }) =>
    request<DocumentExchangeOverview["printJobs"][number]>("/api/document-exchange/print-jobs", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  completePrintJob: (id: string) =>
    request<DocumentExchangeOverview["printJobs"][number]>(`/api/document-exchange/print-jobs/${id}/complete`, {
      method: "POST",
    }),
  failPrintJob: (id: string, error: string) =>
    request<DocumentExchangeOverview["printJobs"][number]>(`/api/document-exchange/print-jobs/${id}/fail`, {
      method: "POST",
      body: JSON.stringify({ error }),
    }),
};
