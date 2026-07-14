namespace AeroERP.Modules.AdvancedManufacturing.Contracts;

/// <summary>
/// Advanced Manufacturing Warehouse Option 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
public sealed record AdvancedManufacturingWarehouseOptionDto(Guid Id, string Code, string Name);
/// <summary>
/// Advanced Manufacturing Item Option 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Unit">计量单位。</param>
public sealed record AdvancedManufacturingItemOptionDto(Guid Id, string Code, string Name, string Unit);
/// <summary>
/// Advanced Manufacturing Work Order Option 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="WorkOrderNo">Work Order No 参数。</param>
/// <param name="FinishedItemId">Finished Item Id 参数。</param>
/// <param name="FinishedItemCode">Finished Item Code 参数。</param>
/// <param name="FinishedItemName">Finished Item Name 参数。</param>
/// <param name="PlannedQuantity">Planned Quantity 参数。</param>
/// <param name="Unit">计量单位。</param>
/// <param name="Status">业务状态。</param>
public sealed record AdvancedManufacturingWorkOrderOptionDto(Guid Id, string WorkOrderNo, Guid FinishedItemId, string FinishedItemCode, string FinishedItemName, decimal PlannedQuantity, string Unit, string Status);

/// <summary>
/// Work Center 数据传输对象。
/// </summary>
public sealed record WorkCenterDto(
    Guid Id,
    string Code,
    string Name,
    Guid WarehouseId,
    string WarehouseName,
    decimal CapacityMinutesPerDay,
    decimal HourlyCostRate,
    bool IsEnabled,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Routing Operation 数据传输对象。
/// </summary>
public sealed record RoutingOperationDto(
    Guid Id,
    int Sequence,
    string OperationCode,
    string OperationName,
    Guid WorkCenterId,
    string WorkCenterCode,
    string WorkCenterName,
    decimal StandardMinutes,
    decimal LaborCostRate,
    decimal MachineCostRate);

/// <summary>
/// Manufacturing Routing 数据传输对象。
/// </summary>
public sealed record ManufacturingRoutingDto(
    Guid Id,
    string RoutingNo,
    Guid FinishedItemId,
    string FinishedItemCode,
    string FinishedItemName,
    string Version,
    string Status,
    string CreatedBy,
    IReadOnlyList<RoutingOperationDto> Operations,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Operation Schedule 数据传输对象。
/// </summary>
public sealed record OperationScheduleDto(
    Guid Id,
    string ScheduleNo,
    Guid WorkOrderId,
    string WorkOrderNo,
    Guid RoutingOperationId,
    string OperationCode,
    string OperationName,
    Guid WorkCenterId,
    string WorkCenterCode,
    string WorkCenterName,
    DateTimeOffset PlannedStartUtc,
    DateTimeOffset PlannedEndUtc,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    string Status,
    string ScheduledBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Capacity Load 数据传输对象。
/// </summary>
public sealed record CapacityLoadDto(
    Guid Id,
    Guid WorkCenterId,
    string WorkCenterCode,
    string WorkCenterName,
    DateOnly PlanDate,
    decimal AvailableMinutes,
    decimal ReservedMinutes,
    decimal RemainingMinutes,
    string SourceDocumentNo,
    string UpdatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Manufacturing Cost Snapshot 数据传输对象。
/// </summary>
public sealed record ManufacturingCostSnapshotDto(
    Guid Id,
    string SnapshotNo,
    Guid WorkOrderId,
    string WorkOrderNo,
    Guid FinishedItemId,
    string FinishedItemCode,
    string FinishedItemName,
    decimal PlannedQuantity,
    decimal MaterialCost,
    decimal LaborCost,
    decimal MachineCost,
    decimal OverheadCost,
    decimal TotalCost,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Mrp Suggestion 数据传输对象。
/// </summary>
public sealed record MrpSuggestionDto(
    Guid Id,
    string SuggestionNo,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    decimal CurrentQuantity,
    decimal DemandQuantity,
    decimal SupplyQuantity,
    decimal SuggestedQuantity,
    string SourceType,
    string Status,
    string CreatedBy,
    string DecidedBy,
    string DecisionNote,
    DateTimeOffset? DecidedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Advanced Manufacturing Overview 数据传输对象。
/// </summary>
public sealed record AdvancedManufacturingOverviewDto(
    IReadOnlyList<WorkCenterDto> WorkCenters,
    IReadOnlyList<ManufacturingRoutingDto> Routings,
    IReadOnlyList<OperationScheduleDto> OperationSchedules,
    IReadOnlyList<CapacityLoadDto> CapacityLoads,
    IReadOnlyList<ManufacturingCostSnapshotDto> CostSnapshots,
    IReadOnlyList<MrpSuggestionDto> MrpSuggestions,
    IReadOnlyList<AdvancedManufacturingWarehouseOptionDto> Warehouses,
    IReadOnlyList<AdvancedManufacturingItemOptionDto> Items,
    IReadOnlyList<AdvancedManufacturingWorkOrderOptionDto> WorkOrders);

/// <summary>
/// Upsert Work Center 请求参数。
/// </summary>
public sealed record UpsertWorkCenterRequest(
    string Code,
    string Name,
    Guid WarehouseId,
    decimal CapacityMinutesPerDay,
    decimal HourlyCostRate,
    bool IsEnabled);

/// <summary>
/// Create Routing Operation 请求参数。
/// </summary>
public sealed record CreateRoutingOperationRequest(
    int Sequence,
    string OperationCode,
    string OperationName,
    Guid WorkCenterId,
    decimal StandardMinutes,
    decimal LaborCostRate,
    decimal MachineCostRate);

/// <summary>
/// Create Manufacturing Routing 请求参数。
/// </summary>
public sealed record CreateManufacturingRoutingRequest(
    Guid FinishedItemId,
    string Version,
    IReadOnlyList<CreateRoutingOperationRequest> Operations);

/// <summary>
/// Create Operation Schedule 请求参数。
/// </summary>
public sealed record CreateOperationScheduleRequest(
    Guid WorkOrderId,
    Guid RoutingOperationId,
    DateTimeOffset PlannedStartUtc,
    DateTimeOffset PlannedEndUtc,
    decimal PlannedQuantity);

/// <summary>
/// Complete Operation Schedule 请求参数。
/// </summary>
/// <param name="CompletedQuantity">Completed Quantity 参数。</param>
public sealed record CompleteOperationScheduleRequest(decimal CompletedQuantity);

/// <summary>
/// Upsert Capacity Load 请求参数。
/// </summary>
public sealed record UpsertCapacityLoadRequest(
    Guid WorkCenterId,
    DateOnly PlanDate,
    decimal AvailableMinutes,
    decimal ReservedMinutes,
    string SourceDocumentNo);

/// <summary>
/// Create Cost Snapshot 请求参数。
/// </summary>
public sealed record CreateCostSnapshotRequest(
    Guid WorkOrderId,
    decimal MaterialCost,
    decimal LaborCost,
    decimal MachineCost,
    decimal OverheadCost);

/// <summary>
/// Generate Mrp Suggestion 请求参数。
/// </summary>
public sealed record GenerateMrpSuggestionRequest(
    Guid WarehouseId,
    Guid ItemId,
    decimal DemandQuantity,
    decimal SupplyQuantity,
    string SourceType);

/// <summary>
/// Decide Mrp Suggestion 请求参数。
/// </summary>
/// <param name="Decision">处理决策。</param>
/// <param name="Note">备注。</param>
public sealed record DecideMrpSuggestionRequest(string Decision, string Note);
