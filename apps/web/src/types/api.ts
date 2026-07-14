/** Web 前端使用的后端 DTO 集合，保持页面状态和 API client 的类型边界一致。 */

// 平台治理、模块可见性、智能代理审查、组织、角色和岗位权限。
export type ModuleVisibility = {
  id: string;
  key: string;
  displayName: string;
  isVisible: boolean;
  category: string;
};

export type AgentReview = {
  id: string;
  agentName: string;
  actionName: string;
  payload: string;
  status: string;
  requestedBy: string;
  reviewedBy?: string | null;
  reviewerComment?: string | null;
  requestedAtUtc: string;
  reviewedAtUtc?: string | null;
};

export type OrganizationSummary = {
  id: string;
  name: string;
  defaultRole: string;
  regionCode: string;
};

export type RoleSummary = {
  id: string;
  key: string;
  displayName: string;
  moduleKeys: string[];
};

export type PositionDepartment = {
  id: string;
  code: string;
  name: string;
  parentDepartmentId?: string | null;
  isEnabled: boolean;
  updatedAtUtc: string;
};

export type JobPosition = {
  id: string;
  code: string;
  name: string;
  departmentId: string;
  departmentName: string;
  description: string;
  isEnabled: boolean;
  updatedAtUtc: string;
};

export type PositionPermissionPackage = {
  id: string;
  displayName: string;
  description: string;
  moduleKeys: string[];
  permissions: string[];
  isEnabled: boolean;
  updatedAtUtc: string;
};

export type PositionRole = {
  id: string;
  displayName: string;
  isSystemProtected: boolean;
  moduleKeys: string[];
  permissions: string[];
};

export type PositionRoleBinding = {
  id: string;
  positionId: string;
  roleId: string;
  positionName: string;
  roleDisplayName: string;
};

export type PositionDataScopeRule = {
  id: string;
  positionId: string;
  positionName: string;
  scopeType: string;
  matchValue: string;
  description: string;
  isEnabled: boolean;
};

export type PositionPermissionOption = {
  key: string;
  displayName: string;
  moduleKey: string;
  moduleDisplayName: string;
};

export type PositionModuleOption = {
  key: string;
  displayName: string;
};

export type PositionPermissionOverview = {
  departments: PositionDepartment[];
  positions: JobPosition[];
  roles: PositionRole[];
  permissionPackages: PositionPermissionPackage[];
  roleBindings: PositionRoleBinding[];
  dataScopeRules: PositionDataScopeRule[];
  permissions: PositionPermissionOption[];
  modules: PositionModuleOption[];
};

export type UserSummary = {
  id: string;
  userName: string;
  displayName: string;
  isEnabled: boolean;
  roles: RoleSummary[];
};

export type CurrentUser = {
  id: string;
  userName: string;
  displayName: string;
  isEnabled: boolean;
  roles: string[];
  roleDisplayNames: string[];
  permissions: string[];
  visibleModuleKeys: string[];
};

export type LoginResponse = {
  accessToken: string;
  expiresAtUtc: string;
  user: CurrentUser;
};

// 主数据 DTO，作为采购、销售、库存和制造的公共引用数据。
export type Supplier = {
  id: string;
  code: string;
  name: string;
  contactName: string;
  phone: string;
  isEnabled: boolean;
  organizationId?: string | null;
  organizationName: string;
  currencyCode: string;
  taxpayerId: string;
  invoiceTitle: string;
};

export type Customer = {
  id: string;
  code: string;
  name: string;
  contactName: string;
  phone: string;
  isEnabled: boolean;
  organizationId?: string | null;
  organizationName: string;
  currencyCode: string;
  taxpayerId: string;
  invoiceTitle: string;
};

export type Item = {
  id: string;
  code: string;
  name: string;
  specification: string;
  unit: string;
  isEnabled: boolean;
};

export type Warehouse = {
  id: string;
  code: string;
  name: string;
  location: string;
  isEnabled: boolean;
  organizationId?: string | null;
  organizationName: string;
};

// 采购与销售闭环 DTO，承接申请/报价到订单状态流转。
export type ProcurementRequestLine = {
  itemId: string;
  itemName: string;
  quantity: number;
  unit: string;
};

export type ProcurementRequest = {
  id: string;
  requestNo: string;
  supplierId: string;
  supplierName: string;
  title: string;
  status: string;
  organizationId?: string | null;
  organizationName: string;
  currencyCode: string;
  taxInvoiceType: string;
  taxRate: number;
  lines: ProcurementRequestLine[];
  createdAtUtc: string;
};

export type ProcurementOrder = {
  id: string;
  orderNo: string;
  requestId: string;
  requestNo: string;
  supplierName: string;
  status: string;
  createdAtUtc: string;
};

export type SalesLine = {
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
};

export type SalesQuotation = {
  id: string;
  quotationNo: string;
  customerId: string;
  customerName: string;
  title: string;
  status: string;
  organizationId?: string | null;
  organizationName: string;
  currencyCode: string;
  taxInvoiceType: string;
  taxRate: number;
  lines: SalesLine[];
  createdAtUtc: string;
};

export type SalesOrder = {
  id: string;
  orderNo: string;
  quotationId: string;
  quotationNo: string;
  customerId: string;
  customerName: string;
  status: string;
  organizationId?: string | null;
  organizationName: string;
  currencyCode: string;
  taxInvoiceType: string;
  taxRate: number;
  lines: SalesLine[];
  createdAtUtc: string;
};

// 库存 DTO，覆盖入库、出库、调拨、盘点、流水、台账和库位余额。
export type InventoryReceiptLine = {
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  unitCost: number;
  costAmount: number;
};

export type PendingInventoryReceipt = {
  procurementOrderId: string;
  procurementOrderNo: string;
  requestNo: string;
  supplierName: string;
  lines: InventoryReceiptLine[];
  releasedAtUtc: string;
};

export type InventoryReceipt = {
  id: string;
  receiptNo: string;
  procurementOrderId: string;
  procurementOrderNo: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  locationId?: string | null;
  locationCode: string;
  locationName: string;
  supplierName: string;
  status: string;
  lines: InventoryReceiptLine[];
  receivedAtUtc: string;
};

export type PendingInventoryIssue = {
  salesOrderId: string;
  salesOrderNo: string;
  quotationNo: string;
  customerName: string;
  lines: InventoryReceiptLine[];
  readyAtUtc: string;
};

export type InventoryIssue = {
  id: string;
  issueNo: string;
  salesOrderId: string;
  salesOrderNo: string;
  quotationNo: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  locationId?: string | null;
  locationCode: string;
  locationName: string;
  customerName: string;
  status: string;
  lines: InventoryReceiptLine[];
  issuedAtUtc: string;
};

export type InventoryTransfer = {
  id: string;
  transferNo: string;
  fromWarehouseId: string;
  fromWarehouseCode: string;
  fromWarehouseName: string;
  fromLocationId?: string | null;
  fromLocationCode: string;
  fromLocationName: string;
  toWarehouseId: string;
  toWarehouseCode: string;
  toWarehouseName: string;
  toLocationId?: string | null;
  toLocationCode: string;
  toLocationName: string;
  reason: string;
  status: string;
  lines: InventoryReceiptLine[];
  executedAtUtc: string;
};

export type InventoryCountAdjustmentLine = {
  itemId: string;
  itemCode: string;
  itemName: string;
  beforeQuantity: number;
  countedQuantity: number;
  deltaQuantity: number;
  unit: string;
  unitCost: number;
  costAmount: number;
};

export type InventoryCountAdjustment = {
  id: string;
  countNo: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  locationId?: string | null;
  locationCode: string;
  locationName: string;
  reason: string;
  status: string;
  lines: InventoryCountAdjustmentLine[];
  countedAtUtc: string;
};

export type InventoryMovement = {
  id: string;
  documentType: string;
  documentNo: string;
  movementType: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  locationId?: string | null;
  locationCode: string;
  locationName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  changeQuantity: number;
  balanceAfter: number;
  unit: string;
  unitCost: number;
  costAmount: number;
  balanceCostAfter: number;
  actor: string;
  occurredAtUtc: string;
};

export type InventoryLedgerEntry = {
  id: string;
  documentType: string;
  documentNo: string;
  movementType: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  locationId?: string | null;
  locationCode: string;
  locationName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  inQuantity: number;
  outQuantity: number;
  balanceAfter: number;
  unit: string;
  unitCost: number;
  inAmount: number;
  outAmount: number;
  balanceCostAfter: number;
  actor: string;
  occurredAtUtc: string;
};

export type StockBalance = {
  id: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  unitCost: number;
  inventoryValue: number;
  updatedAtUtc: string;
};

export type WarehouseLocation = {
  id: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  code: string;
  name: string;
  isEnabled: boolean;
  createdBy: string;
  updatedAtUtc: string;
};

export type LocationStockBalance = {
  id: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  locationId: string;
  locationCode: string;
  locationName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  unitCost: number;
  inventoryValue: number;
  updatedAtUtc: string;
};

// 制造 DTO，覆盖 BOM、工单、生产领料、完工入库和成本汇总。
export type BomLine = {
  id: string;
  componentItemId: string;
  componentItemCode: string;
  componentItemName: string;
  quantity: number;
  unit: string;
};

export type Bom = {
  id: string;
  bomNo: string;
  finishedItemId: string;
  finishedItemCode: string;
  finishedItemName: string;
  version: string;
  baseQuantity: number;
  unit: string;
  isEnabled: boolean;
  lines: BomLine[];
  updatedAtUtc: string;
};

export type WorkOrderMaterialLine = {
  id: string;
  componentItemId: string;
  componentItemCode: string;
  componentItemName: string;
  requiredQuantity: number;
  issuedQuantity: number;
  unit: string;
};

export type WorkOrderCostSummary = {
  materialCost: number;
  laborCost: number;
  machineCost: number;
  overheadCost: number;
  totalCost: number;
  receivedCost: number;
  remainingCost: number;
  receivedQuantity: number;
  unitCost: number;
  snapshotTotalCost: number;
  totalCostVariance: number;
  costSource: string;
};

export type WorkOrder = {
  id: string;
  workOrderNo: string;
  bomId: string;
  bomNo: string;
  bomVersion: string;
  finishedItemId: string;
  finishedItemCode: string;
  finishedItemName: string;
  plannedQuantity: number;
  completedQuantity: number;
  unit: string;
  status: string;
  createdBy: string;
  materialLines: WorkOrderMaterialLine[];
  costSummary: WorkOrderCostSummary;
  updatedAtUtc: string;
};

export type ProductionIssueLine = {
  id: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  unitCost: number;
  costAmount: number;
};

export type ProductionIssue = {
  id: string;
  issueNo: string;
  workOrderId: string;
  workOrderNo: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  status: string;
  issuedBy: string;
  lines: ProductionIssueLine[];
  issuedAtUtc: string;
};

export type ProductionReceipt = {
  id: string;
  receiptNo: string;
  workOrderId: string;
  workOrderNo: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  finishedItemId: string;
  finishedItemCode: string;
  finishedItemName: string;
  quantity: number;
  unit: string;
  unitCost: number;
  materialCost: number;
  laborCost: number;
  machineCost: number;
  overheadCost: number;
  costAmount: number;
  status: string;
  receivedBy: string;
  receivedAtUtc: string;
};

// 高级制造 DTO，覆盖工作中心、工艺路线、工序排程、产能负载、成本快照和 MRP 建议。
export type AdvancedManufacturingWarehouseOption = {
  id: string;
  code: string;
  name: string;
};

export type AdvancedManufacturingItemOption = {
  id: string;
  code: string;
  name: string;
  unit: string;
};

export type AdvancedManufacturingWorkOrderOption = {
  id: string;
  workOrderNo: string;
  finishedItemId: string;
  finishedItemCode: string;
  finishedItemName: string;
  plannedQuantity: number;
  unit: string;
  status: string;
};

export type WorkCenter = {
  id: string;
  code: string;
  name: string;
  warehouseId: string;
  warehouseName: string;
  capacityMinutesPerDay: number;
  hourlyCostRate: number;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type RoutingOperation = {
  id: string;
  sequence: number;
  operationCode: string;
  operationName: string;
  workCenterId: string;
  workCenterCode: string;
  workCenterName: string;
  standardMinutes: number;
  laborCostRate: number;
  machineCostRate: number;
};

export type ManufacturingRouting = {
  id: string;
  routingNo: string;
  finishedItemId: string;
  finishedItemCode: string;
  finishedItemName: string;
  version: string;
  status: string;
  createdBy: string;
  operations: RoutingOperation[];
  updatedAtUtc: string;
};

export type OperationSchedule = {
  id: string;
  scheduleNo: string;
  workOrderId: string;
  workOrderNo: string;
  routingOperationId: string;
  operationCode: string;
  operationName: string;
  workCenterId: string;
  workCenterCode: string;
  workCenterName: string;
  plannedStartUtc: string;
  plannedEndUtc: string;
  plannedQuantity: number;
  completedQuantity: number;
  status: string;
  scheduledBy: string;
  updatedAtUtc: string;
};

export type CapacityLoad = {
  id: string;
  workCenterId: string;
  workCenterCode: string;
  workCenterName: string;
  planDate: string;
  availableMinutes: number;
  reservedMinutes: number;
  remainingMinutes: number;
  sourceDocumentNo: string;
  updatedBy: string;
  updatedAtUtc: string;
};

export type ManufacturingCostSnapshot = {
  id: string;
  snapshotNo: string;
  workOrderId: string;
  workOrderNo: string;
  finishedItemId: string;
  finishedItemCode: string;
  finishedItemName: string;
  plannedQuantity: number;
  materialCost: number;
  laborCost: number;
  machineCost: number;
  overheadCost: number;
  totalCost: number;
  createdBy: string;
  createdAtUtc: string;
};

export type MrpSuggestion = {
  id: string;
  suggestionNo: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  currentQuantity: number;
  demandQuantity: number;
  supplyQuantity: number;
  suggestedQuantity: number;
  sourceType: string;
  status: string;
  createdBy: string;
  decidedBy: string;
  decisionNote: string;
  decidedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type AdvancedManufacturingOverview = {
  workCenters: WorkCenter[];
  routings: ManufacturingRouting[];
  operationSchedules: OperationSchedule[];
  capacityLoads: CapacityLoad[];
  costSnapshots: ManufacturingCostSnapshot[];
  mrpSuggestions: MrpSuggestion[];
  warehouses: AdvancedManufacturingWarehouseOption[];
  items: AdvancedManufacturingItemOption[];
  workOrders: AdvancedManufacturingWorkOrderOption[];
};

// 质量追溯 DTO，串联来源单据、检验结果和批次事件链。
export type QualitySourceCandidate = {
  sourceDocumentType: string;
  sourceDocumentId: string;
  sourceDocumentNo: string;
  sourceName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  occurredAtUtc: string;
};

export type QualityInspection = {
  id: string;
  inspectionNo: string;
  sourceDocumentType: string;
  sourceDocumentId: string;
  sourceDocumentNo: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  inspectedQuantity: number;
  acceptedQuantity: number;
  rejectedQuantity: number;
  result: string;
  disposition: string;
  inspector: string;
  note: string;
  inspectedAtUtc: string;
};

export type LotTraceEvent = {
  id: string;
  lotNo: string;
  eventType: string;
  sourceDocumentType: string;
  sourceDocumentId: string;
  sourceDocumentNo: string;
  targetDocumentType: string;
  targetDocumentId?: string | null;
  targetDocumentNo: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  actor: string;
  note: string;
  occurredAtUtc: string;
};

export type LotTrace = {
  lotNo: string;
  events: LotTraceEvent[];
};

// 计划执行 DTO，覆盖补货建议、外协订单和条码执行记录。
export type PlanningSuggestion = {
  id: string;
  suggestionNo: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  currentQuantity: number;
  minimumQuantity: number;
  suggestedQuantity: number;
  unit: string;
  status: string;
  createdBy: string;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type OutsourcingOrderLine = {
  id: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
};

export type OutsourcingOrder = {
  id: string;
  orderNo: string;
  supplierName: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  finishedItemId: string;
  finishedItemCode: string;
  finishedItemName: string;
  plannedQuantity: number;
  receivedQuantity: number;
  unit: string;
  status: string;
  createdBy: string;
  materialLines: OutsourcingOrderLine[];
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type BarcodeExecution = {
  id: string;
  executionNo: string;
  barcode: string;
  action: string;
  result: string;
  message: string;
  documentType: string;
  documentId?: string | null;
  documentNo: string;
  actor: string;
  createdAtUtc: string;
};

// WMS DTO，覆盖上架、拣货、波次、容器、库内路线和 PDA 队列。
export type WmsWarehouseOption = {
  id: string;
  code: string;
  name: string;
};

export type WmsLocationOption = {
  id: string;
  warehouseId: string;
  warehouseName: string;
  code: string;
  name: string;
};

export type WmsItemOption = {
  id: string;
  code: string;
  name: string;
  unit: string;
};

export type PutAwayTask = {
  id: string;
  taskNo: string;
  warehouseId: string;
  warehouseName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  suggestedLocationId?: string | null;
  suggestedLocationName: string;
  containerCode: string;
  sourceDocumentNo: string;
  status: string;
  assignedTo: string;
  createdBy: string;
  completedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type PickingTask = {
  id: string;
  taskNo: string;
  warehouseId: string;
  warehouseName: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  quantity: number;
  unit: string;
  sourceLocationId?: string | null;
  sourceLocationName: string;
  waveId?: string | null;
  waveNo: string;
  status: string;
  assignedTo: string;
  createdBy: string;
  completedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type PickingWave = {
  id: string;
  waveNo: string;
  warehouseId: string;
  warehouseName: string;
  status: string;
  createdBy: string;
  releasedBy: string;
  releasedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type WarehouseContainer = {
  id: string;
  code: string;
  containerType: string;
  warehouseId: string;
  warehouseName: string;
  currentLocationId?: string | null;
  currentLocationName: string;
  status: string;
  lastHandledBy: string;
  updatedAtUtc: string;
};

export type WarehouseRoute = {
  id: string;
  warehouseId: string;
  warehouseName: string;
  fromLocationId: string;
  fromLocationName: string;
  toLocationId: string;
  toLocationName: string;
  distanceMeters: number;
  priority: number;
  isEnabled: boolean;
  updatedAtUtc: string;
};

export type PdaWorkQueueItem = {
  id: string;
  taskType: string;
  taskId: string;
  taskNo: string;
  warehouseId: string;
  warehouseName: string;
  locationCode: string;
  assignedTo: string;
  priority: number;
  status: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type WmsOverview = {
  putAwayTasks: PutAwayTask[];
  pickingTasks: PickingTask[];
  waves: PickingWave[];
  containers: WarehouseContainer[];
  routes: WarehouseRoute[];
  pdaQueue: PdaWorkQueueItem[];
  warehouses: WmsWarehouseOption[];
  locations: WmsLocationOption[];
  items: WmsItemOption[];
};

// 移动作业 DTO，覆盖终端、离线任务、扫码事件和执行队列。
export type MobileWorkMetric = {
  key: string;
  label: string;
  value: number;
  unit: string;
};

export type MobileWorkDevice = {
  id: string;
  deviceCode: string;
  displayName: string;
  assignedTo: string;
  isEnabled: boolean;
  updatedBy: string;
  lastSeenAtUtc: string;
  updatedAtUtc: string;
};

export type MobileWorkOfflineTask = {
  id: string;
  taskNo: string;
  sourceModule: string;
  sourceTaskType: string;
  sourceTaskNo: string;
  payloadJson: string;
  assignedTo: string;
  status: string;
  createdBy: string;
  completedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type MobileWorkScanEvent = {
  id: string;
  scanNo: string;
  deviceCode: string;
  barcode: string;
  targetModule: string;
  action: string;
  documentNo: string;
  result: string;
  message: string;
  actor: string;
  createdAtUtc: string;
};

export type MobileWorkQueueEntry = {
  id: string;
  sourceModule: string;
  taskType: string;
  taskId: string;
  taskNo: string;
  warehouseName: string;
  locationCode: string;
  assignedTo: string;
  priority: number;
  status: string;
  updatedAtUtc: string;
};

export type MobileWorkOverview = {
  devices: MobileWorkDevice[];
  offlineTasks: MobileWorkOfflineTask[];
  scanEvents: MobileWorkScanEvent[];
  workQueue: MobileWorkQueueEntry[];
  metrics: MobileWorkMetric[];
};

// 集成 DTO，覆盖消息通道、Webhook、外部连接器、同步任务和审计记录。
export type IntegrationMetric = {
  key: string;
  label: string;
  value: number;
  unit: string;
};

export type MessageChannel = {
  id: string;
  channelKey: string;
  displayName: string;
  channelType: string;
  endpoint: string;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type WebhookSubscription = {
  id: string;
  subscriptionKey: string;
  displayName: string;
  eventKey: string;
  targetUrl: string;
  secretName: string;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type ExternalConnector = {
  id: string;
  connectorKey: string;
  displayName: string;
  provider: string;
  baseUrl: string;
  authMode: string;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type IntegrationSyncJob = {
  id: string;
  jobNo: string;
  connectorKey: string;
  direction: string;
  payloadJson: string;
  status: string;
  attemptCount: number;
  lastError: string;
  createdBy: string;
  completedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type IntegrationAuditRecord = {
  id: string;
  auditNo: string;
  category: string;
  action: string;
  targetKey: string;
  result: string;
  message: string;
  actor: string;
  createdAtUtc: string;
};

export type IntegrationOverview = {
  channels: MessageChannel[];
  webhooks: WebhookSubscription[];
  connectors: ExternalConnector[];
  syncJobs: IntegrationSyncJob[];
  auditRecords: IntegrationAuditRecord[];
  metrics: IntegrationMetric[];
};

// 文档交换 DTO，覆盖导入模板、字段映射、导入批次、导出文件、打印模板和文件审计。
export type DocumentExchangeMetric = {
  key: string;
  label: string;
  value: number;
  unit: string;
};

export type ImportTemplate = {
  id: string;
  templateKey: string;
  displayName: string;
  targetModule: string;
  fileType: string;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type ImportFieldMapping = {
  id: string;
  templateKey: string;
  sourceField: string;
  targetField: string;
  isRequired: boolean;
  transformRule: string;
  updatedBy: string;
  updatedAtUtc: string;
};

export type ImportBatch = {
  id: string;
  batchNo: string;
  templateKey: string;
  fileName: string;
  status: string;
  rowCount: number;
  errorCount: number;
  errorMessage: string;
  createdBy: string;
  completedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type ExportFileTask = {
  id: string;
  exportNo: string;
  sourceModule: string;
  fileName: string;
  format: string;
  status: string;
  requestedBy: string;
  completedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type PrintTemplate = {
  id: string;
  templateKey: string;
  displayName: string;
  targetModule: string;
  contentType: string;
  templateBody: string;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type PrintJob = {
  id: string;
  jobNo: string;
  templateKey: string;
  documentNo: string;
  status: string;
  requestedBy: string;
  completedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type FileAuditRecord = {
  id: string;
  auditNo: string;
  category: string;
  action: string;
  targetNo: string;
  result: string;
  message: string;
  actor: string;
  createdAtUtc: string;
};

export type DocumentExchangeOverview = {
  importTemplates: ImportTemplate[];
  fieldMappings: ImportFieldMapping[];
  importBatches: ImportBatch[];
  exportTasks: ExportFileTask[];
  printTemplates: PrintTemplate[];
  printJobs: PrintJob[];
  auditRecords: FileAuditRecord[];
  metrics: DocumentExchangeMetric[];
};

// 财务 DTO，覆盖会计科目、期间、凭证、报表、往来、发票、银行和结算。
export type AccountingAccount = {
  id: string;
  code: string;
  name: string;
  type: string;
  parentAccountId?: string | null;
  parentAccountCode: string;
  parentAccountName: string;
  isActive: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type AccountingPeriod = {
  id: string;
  year: number;
  month: number;
  name: string;
  startDate: string;
  endDate: string;
  status: string;
  createdBy: string;
  closedBy: string;
  closedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type GeneralLedgerVoucherLine = {
  id: string;
  accountingAccountId: string;
  accountCode: string;
  accountName: string;
  summary: string;
  debitAmount: number;
  creditAmount: number;
};

export type GeneralLedgerVoucher = {
  id: string;
  voucherNo: string;
  accountingPeriodId: string;
  accountingPeriodName: string;
  documentDate: string;
  summary: string;
  sourceType: string;
  sourceId?: string | null;
  sourceNo: string;
  status: string;
  totalDebit: number;
  totalCredit: number;
  createdBy: string;
  submittedBy: string;
  submittedAtUtc?: string | null;
  reviewedBy: string;
  reviewedAtUtc?: string | null;
  reviewNote: string;
  lines: GeneralLedgerVoucherLine[];
  updatedAtUtc: string;
};

export type TrialBalanceLine = {
  accountingAccountId: string;
  accountCode: string;
  accountName: string;
  accountType: string;
  debitAmount: number;
  creditAmount: number;
  endingDebit: number;
  endingCredit: number;
};

export type IncomeStatement = {
  revenue: number;
  cost: number;
  expense: number;
  profit: number;
};

export type BalanceSheet = {
  assets: number;
  liabilities: number;
  equity: number;
  retainedEarnings: number;
  totalLiabilitiesAndEquity: number;
  difference: number;
};

export type FinanceReportSnapshot = {
  accountingPeriodId?: string | null;
  accountingPeriodName: string;
  startDate?: string | null;
  endDate?: string | null;
  approvedVoucherCount: number;
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;
  trialBalance: TrialBalanceLine[];
  incomeStatement: IncomeStatement;
  balanceSheet: BalanceSheet;
};

export type Payable = {
  id: string;
  payableNo: string;
  procurementOrderId: string;
  procurementOrderNo: string;
  inventoryReceiptId?: string | null;
  inventoryReceiptNo: string;
  supplierName: string;
  amount: number;
  netAmount: number;
  taxAmount: number;
  taxRate: number;
  taxInvoiceType: string;
  settledAmount: number;
  remainingAmount: number;
  currencyCode: string;
  dueDate: string;
  overdueDays: number;
  status: string;
  sourceType: string;
  createdAtUtc: string;
};

export type Receivable = {
  id: string;
  receivableNo: string;
  salesOrderId: string;
  salesOrderNo: string;
  inventoryIssueId?: string | null;
  inventoryIssueNo: string;
  customerName: string;
  amount: number;
  netAmount: number;
  taxAmount: number;
  taxRate: number;
  taxInvoiceType: string;
  settledAmount: number;
  remainingAmount: number;
  currencyCode: string;
  dueDate: string;
  overdueDays: number;
  status: string;
  sourceType: string;
  createdAtUtc: string;
};

export type AgingBucket = {
  bucket: string;
  bucketName: string;
  count: number;
  amount: number;
};

export type AgingEntry = {
  id: string;
  documentNo: string;
  counterpartyName: string;
  sourceNo: string;
  amount: number;
  settledAmount: number;
  remainingAmount: number;
  currencyCode: string;
  dueDate: string;
  overdueDays: number;
  bucket: string;
  status: string;
};

export type AgingSide = {
  totalOpenAmount: number;
  totalOverdueAmount: number;
  openCount: number;
  overdueCount: number;
  buckets: AgingBucket[];
  entries: AgingEntry[];
};

export type FinanceAgingSnapshot = {
  asOfDate: string;
  payables: AgingSide;
  receivables: AgingSide;
};

export type FinanceInvoice = {
  id: string;
  invoiceNo: string;
  direction: "Payable" | "Receivable";
  sourceId: string;
  sourceNo: string;
  counterpartyName: string;
  taxInvoiceType: string;
  taxRate: number;
  grossAmount: number;
  netAmount: number;
  taxAmount: number;
  currencyCode: string;
  invoiceDate: string;
  note: string;
  createdBy: string;
  createdAtUtc: string;
};

export type BankAccount = {
  id: string;
  accountNo: string;
  accountName: string;
  bankName: string;
  currencyCode: string;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type BankStatementLine = {
  id: string;
  statementNo: string;
  bankAccountId: string;
  bankAccountNo: string;
  bankAccountName: string;
  transactionDate: string;
  direction: "Inflow" | "Outflow";
  amount: number;
  currencyCode: string;
  counterpartyName: string;
  bankReferenceNo: string;
  summary: string;
  reconciliationStatus: string;
  settlementId?: string | null;
  settlementNo: string;
  reconciledBy: string;
  reconciledAtUtc?: string | null;
  createdBy: string;
  createdAtUtc: string;
};

export type Settlement = {
  id: string;
  settlementNo: string;
  targetType: string;
  targetId: string;
  targetNo: string;
  counterpartyName: string;
  amount: number;
  currencyCode: string;
  bankAccountId: string;
  bankAccountNo: string;
  bankAccountName: string;
  method: string;
  note: string;
  reconciliationStatus: string;
  bankStatementLineId?: string | null;
  bankStatementNo: string;
  reconciledBy: string;
  reconciledAtUtc?: string | null;
  settledBy: string;
  settledAtUtc: string;
};

// 工作流 DTO，覆盖流程定义、实例、审批任务和通知阅读状态。
export type WorkflowDefinition = {
  id: string;
  key: string;
  displayName: string;
  moduleKey: string;
  documentType: string;
  requiredPermission: string;
  isEnabled: boolean;
  createdAtUtc: string;
};

export type WorkflowInstance = {
  id: string;
  definitionId: string;
  definitionKey: string;
  definitionName: string;
  documentType: string;
  documentId: string;
  documentNo: string;
  title: string;
  status: string;
  submittedBy: string;
  submittedAtUtc: string;
  completedAtUtc?: string | null;
};

export type ApprovalTask = {
  id: string;
  workflowInstanceId: string;
  definitionKey: string;
  definitionName: string;
  documentType: string;
  documentId: string;
  documentNo: string;
  title: string;
  status: string;
  submittedBy: string;
  requiredPermission: string;
  decidedBy?: string | null;
  decision?: string | null;
  comment?: string | null;
  createdAtUtc: string;
  decidedAtUtc?: string | null;
};

export type WorkflowNotification = {
  id: string;
  title: string;
  message: string;
  category: string;
  relatedDocumentType: string;
  relatedDocumentId: string;
  relatedDocumentNo: string;
  recipientPermission: string;
  status: string;
  createdAtUtc: string;
  readAtUtc?: string | null;
};

// 经营分析与报表 DTO，覆盖驾驶舱指标、报表定义、运行记录和导出任务。
export type AnalyticsMetric = {
  key: string;
  label: string;
  value: number;
  unit: string;
};

export type AnalyticsSnapshot = {
  procurement: AnalyticsMetric[];
  sales: AnalyticsMetric[];
  inventory: AnalyticsMetric[];
  finance: AnalyticsMetric[];
  generatedAtUtc: string;
};

export type BusinessMetric = {
  key: string;
  label: string;
  value: number;
  unit: string;
};

export type ReportDefinition = {
  id: string;
  key: string;
  displayName: string;
  category: string;
  queryModel: string;
  parametersJson: string;
  isEnabled: boolean;
  updatedBy: string;
  updatedAtUtc: string;
};

export type ReportRunRecord = {
  id: string;
  runNo: string;
  reportDefinitionId: string;
  reportKey: string;
  reportName: string;
  parametersJson: string;
  resultSummaryJson: string;
  rowCount: number;
  status: string;
  runBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type ReportExportTask = {
  id: string;
  exportNo: string;
  reportRunRecordId: string;
  reportName: string;
  format: string;
  fileName: string;
  status: string;
  requestedBy: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export type ReportingOverview = {
  definitions: ReportDefinition[];
  runs: ReportRunRecord[];
  exportTasks: ReportExportTask[];
  liveMetrics: BusinessMetric[];
};

// 管控与本地化 DTO，覆盖数据权限、编号规则、币种、税务设置和界面词条。
export type DataScopeRule = {
  id: string;
  roleKey: string;
  scopeType: string;
  matchValue: string;
  description: string;
  isEnabled: boolean;
  createdAtUtc: string;
};

export type NumberingRule = {
  id: string;
  documentType: string;
  prefix: string;
  useDateSegment: boolean;
  nextSequence: number;
  padding: number;
  isEnabled: boolean;
  createdAtUtc: string;
};

export type Currency = {
  id: string;
  code: string;
  name: string;
  symbol: string;
  exchangeRateToBase: number;
  isBase: boolean;
  isEnabled: boolean;
};

export type LocalizationSettings = {
  id: string;
  defaultCurrencyCode: string;
  taxInvoiceType: string;
  taxpayerId: string;
  invoiceTitle: string;
  defaultTaxRate: number;
};

export type LocalizationContent = {
  id: string;
  key: string;
  category: string;
  chineseText: string;
  englishText: string;
  isEnabled: boolean;
  updatedAtUtc: string;
};
