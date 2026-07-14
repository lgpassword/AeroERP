namespace AeroERP.Modules.Inventory.Contracts;

/// <summary>
/// Inventory Receipt Line 数据传输对象。
/// </summary>
public sealed record InventoryReceiptLineDto(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    decimal UnitCost = 0m,
    decimal CostAmount = 0m);

/// <summary>
/// Inventory Count Adjustment Line 数据传输对象。
/// </summary>
public sealed record InventoryCountAdjustmentLineDto(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal BeforeQuantity,
    decimal CountedQuantity,
    decimal DeltaQuantity,
    string Unit,
    decimal UnitCost = 0m,
    decimal CostAmount = 0m);

/// <summary>
/// Inventory Receipt 数据传输对象。
/// </summary>
public sealed record InventoryReceiptDto(
    Guid Id,
    string ReceiptNo,
    Guid ProcurementOrderId,
    string ProcurementOrderNo,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid? LocationId,
    string LocationCode,
    string LocationName,
    string SupplierName,
    string Status,
    IReadOnlyList<InventoryReceiptLineDto> Lines,
    DateTimeOffset ReceivedAtUtc);

/// <summary>
/// Inventory Issue 数据传输对象。
/// </summary>
public sealed record InventoryIssueDto(
    Guid Id,
    string IssueNo,
    Guid SalesOrderId,
    string SalesOrderNo,
    string QuotationNo,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid? LocationId,
    string LocationCode,
    string LocationName,
    string CustomerName,
    string Status,
    IReadOnlyList<InventoryReceiptLineDto> Lines,
    DateTimeOffset IssuedAtUtc);

/// <summary>
/// Inventory Transfer 数据传输对象。
/// </summary>
public sealed record InventoryTransferDto(
    Guid Id,
    string TransferNo,
    Guid FromWarehouseId,
    string FromWarehouseCode,
    string FromWarehouseName,
    Guid? FromLocationId,
    string FromLocationCode,
    string FromLocationName,
    Guid ToWarehouseId,
    string ToWarehouseCode,
    string ToWarehouseName,
    Guid? ToLocationId,
    string ToLocationCode,
    string ToLocationName,
    string Reason,
    string Status,
    IReadOnlyList<InventoryReceiptLineDto> Lines,
    DateTimeOffset ExecutedAtUtc);

/// <summary>
/// Inventory Count Adjustment 数据传输对象。
/// </summary>
public sealed record InventoryCountAdjustmentDto(
    Guid Id,
    string CountNo,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid? LocationId,
    string LocationCode,
    string LocationName,
    string Reason,
    string Status,
    IReadOnlyList<InventoryCountAdjustmentLineDto> Lines,
    DateTimeOffset CountedAtUtc);

/// <summary>
/// Inventory Movement 数据传输对象。
/// </summary>
public sealed record InventoryMovementDto(
    Guid Id,
    string DocumentType,
    string DocumentNo,
    string MovementType,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid? LocationId,
    string LocationCode,
    string LocationName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal ChangeQuantity,
    decimal BalanceAfter,
    string Unit,
    decimal UnitCost,
    decimal CostAmount,
    decimal BalanceCostAfter,
    string Actor,
    DateTimeOffset OccurredAtUtc);

/// <summary>
/// Inventory Ledger Entry 数据传输对象。
/// </summary>
public sealed record InventoryLedgerEntryDto(
    Guid Id,
    string DocumentType,
    string DocumentNo,
    string MovementType,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid? LocationId,
    string LocationCode,
    string LocationName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal InQuantity,
    decimal OutQuantity,
    decimal BalanceAfter,
    string Unit,
    decimal UnitCost,
    decimal InAmount,
    decimal OutAmount,
    decimal BalanceCostAfter,
    string Actor,
    DateTimeOffset OccurredAtUtc);

/// <summary>
/// Stock Balance 数据传输对象。
/// </summary>
public sealed record StockBalanceDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    decimal UnitCost,
    decimal InventoryValue,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Warehouse Location 数据传输对象。
/// </summary>
public sealed record WarehouseLocationDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string Code,
    string Name,
    bool IsEnabled,
    string CreatedBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Location Stock Balance 数据传输对象。
/// </summary>
public sealed record LocationStockBalanceDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid LocationId,
    string LocationCode,
    string LocationName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    decimal UnitCost,
    decimal InventoryValue,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Pending Inventory Receipt 数据传输对象。
/// </summary>
public sealed record PendingInventoryReceiptDto(
    Guid ProcurementOrderId,
    string ProcurementOrderNo,
    string RequestNo,
    string SupplierName,
    IReadOnlyList<InventoryReceiptLineDto> Lines,
    DateTimeOffset ReleasedAtUtc);

/// <summary>
/// Pending Inventory Issue 数据传输对象。
/// </summary>
public sealed record PendingInventoryIssueDto(
    Guid SalesOrderId,
    string SalesOrderNo,
    string QuotationNo,
    string CustomerName,
    IReadOnlyList<InventoryReceiptLineDto> Lines,
    DateTimeOffset ReadyAtUtc);

/// <summary>
/// Inventory Cost Input 请求参数。
/// </summary>
/// <param name="ItemId">物料标识。</param>
/// <param name="UnitCost">单位成本。</param>
public sealed record InventoryCostInputRequest(Guid ItemId, decimal UnitCost);

/// <summary>
/// Receive Procurement Order 请求参数。
/// </summary>
public sealed record ReceiveProcurementOrderRequest(
    Guid ProcurementOrderId,
    Guid WarehouseId,
    Guid? LocationId = null,
    IReadOnlyList<InventoryCostInputRequest>? Costs = null);

/// <summary>
/// Issue Sales Order 请求参数。
/// </summary>
/// <param name="SalesOrderId">Sales Order Id 参数。</param>
/// <param name="WarehouseId">仓库标识。</param>
/// <param name="LocationId">Location Id 参数。</param>
public sealed record IssueSalesOrderRequest(Guid SalesOrderId, Guid WarehouseId, Guid? LocationId = null);

/// <summary>
/// Create Inventory Transfer Line 请求参数。
/// </summary>
/// <param name="ItemId">物料标识。</param>
/// <param name="Quantity">数量。</param>
/// <param name="Unit">计量单位。</param>
public sealed record CreateInventoryTransferLineRequest(Guid ItemId, decimal Quantity, string Unit);

/// <summary>
/// Create Inventory Transfer 请求参数。
/// </summary>
public sealed record CreateInventoryTransferRequest(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    Guid? FromLocationId,
    Guid? ToLocationId,
    string Reason,
    IReadOnlyList<CreateInventoryTransferLineRequest> Lines);

/// <summary>
/// Create Inventory Count Line 请求参数。
/// </summary>
/// <param name="ItemId">物料标识。</param>
/// <param name="CountedQuantity">Counted Quantity 参数。</param>
/// <param name="UnitCost">单位成本。</param>
public sealed record CreateInventoryCountLineRequest(Guid ItemId, decimal CountedQuantity, decimal? UnitCost = null);

/// <summary>
/// Create Inventory Count Adjustment 请求参数。
/// </summary>
public sealed record CreateInventoryCountAdjustmentRequest(
    Guid WarehouseId,
    Guid? LocationId,
    string Reason,
    IReadOnlyList<CreateInventoryCountLineRequest> Lines);

/// <summary>
/// Create Warehouse Location 请求参数。
/// </summary>
public sealed record CreateWarehouseLocationRequest(
    Guid WarehouseId,
    string Code,
    string Name,
    bool IsEnabled);
