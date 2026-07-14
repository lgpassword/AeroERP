namespace AeroERP.Modules.Manufacturing.Contracts;

/// <summary>
/// Bom Line 数据传输对象。
/// </summary>
public sealed record BomLineDto(
    Guid Id,
    Guid ComponentItemId,
    string ComponentItemCode,
    string ComponentItemName,
    decimal Quantity,
    string Unit);

/// <summary>
/// Bom 数据传输对象。
/// </summary>
public sealed record BomDto(
    Guid Id,
    string BomNo,
    Guid FinishedItemId,
    string FinishedItemCode,
    string FinishedItemName,
    string Version,
    decimal BaseQuantity,
    string Unit,
    bool IsEnabled,
    IReadOnlyList<BomLineDto> Lines,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Work Order Material Line 数据传输对象。
/// </summary>
public sealed record WorkOrderMaterialLineDto(
    Guid Id,
    Guid ComponentItemId,
    string ComponentItemCode,
    string ComponentItemName,
    decimal RequiredQuantity,
    decimal IssuedQuantity,
    string Unit);

/// <summary>
/// Work Order Cost Summary 数据传输对象。
/// </summary>
public sealed record WorkOrderCostSummaryDto(
    decimal MaterialCost,
    decimal LaborCost,
    decimal MachineCost,
    decimal OverheadCost,
    decimal TotalCost,
    decimal ReceivedCost,
    decimal RemainingCost,
    decimal ReceivedQuantity,
    decimal UnitCost,
    decimal SnapshotTotalCost,
    decimal TotalCostVariance,
    string CostSource);

/// <summary>
/// Work Order 数据传输对象。
/// </summary>
public sealed record WorkOrderDto(
    Guid Id,
    string WorkOrderNo,
    Guid BomId,
    string BomNo,
    string BomVersion,
    Guid FinishedItemId,
    string FinishedItemCode,
    string FinishedItemName,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    string Unit,
    string Status,
    string CreatedBy,
    IReadOnlyList<WorkOrderMaterialLineDto> MaterialLines,
    WorkOrderCostSummaryDto CostSummary,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Production Issue Line 数据传输对象。
/// </summary>
public sealed record ProductionIssueLineDto(
    Guid Id,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    decimal UnitCost = 0m,
    decimal CostAmount = 0m);

/// <summary>
/// Production Issue 数据传输对象。
/// </summary>
public sealed record ProductionIssueDto(
    Guid Id,
    string IssueNo,
    Guid WorkOrderId,
    string WorkOrderNo,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string Status,
    string IssuedBy,
    IReadOnlyList<ProductionIssueLineDto> Lines,
    DateTimeOffset IssuedAtUtc);

/// <summary>
/// Production Receipt 数据传输对象。
/// </summary>
public sealed record ProductionReceiptDto(
    Guid Id,
    string ReceiptNo,
    Guid WorkOrderId,
    string WorkOrderNo,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid FinishedItemId,
    string FinishedItemCode,
    string FinishedItemName,
    decimal Quantity,
    string Unit,
    decimal UnitCost,
    decimal MaterialCost,
    decimal LaborCost,
    decimal MachineCost,
    decimal OverheadCost,
    decimal CostAmount,
    string Status,
    string ReceivedBy,
    DateTimeOffset ReceivedAtUtc);

/// <summary>
/// Create Bom Line 请求参数。
/// </summary>
/// <param name="ComponentItemId">Component Item Id 参数。</param>
/// <param name="Quantity">数量。</param>
public sealed record CreateBomLineRequest(Guid ComponentItemId, decimal Quantity);

/// <summary>
/// Create Bom 请求参数。
/// </summary>
public sealed record CreateBomRequest(
    Guid FinishedItemId,
    string Version,
    decimal BaseQuantity,
    bool IsEnabled,
    IReadOnlyList<CreateBomLineRequest> Lines);

/// <summary>
/// Create Work Order 请求参数。
/// </summary>
/// <param name="BomId">Bom Id 参数。</param>
/// <param name="PlannedQuantity">Planned Quantity 参数。</param>
public sealed record CreateWorkOrderRequest(Guid BomId, decimal PlannedQuantity);

/// <summary>
/// Execute Production Issue 请求参数。
/// </summary>
/// <param name="WarehouseId">仓库标识。</param>
public sealed record ExecuteProductionIssueRequest(Guid WarehouseId);

/// <summary>
/// Complete Production 请求参数。
/// </summary>
/// <param name="WarehouseId">仓库标识。</param>
/// <param name="Quantity">数量。</param>
public sealed record CompleteProductionRequest(Guid WarehouseId, decimal Quantity);
