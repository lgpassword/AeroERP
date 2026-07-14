namespace AeroERP.Modules.Planning.Contracts;

/// <summary>
/// Planning Suggestion 数据传输对象。
/// </summary>
public sealed record PlanningSuggestionDto(
    Guid Id,
    string SuggestionNo,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal CurrentQuantity,
    decimal MinimumQuantity,
    decimal SuggestedQuantity,
    string Unit,
    string Status,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Generate Planning Suggestion 请求参数。
/// </summary>
public sealed record GeneratePlanningSuggestionRequest(
    Guid WarehouseId,
    Guid ItemId,
    decimal MinimumQuantity);

/// <summary>
/// Planning Suggestion Decision 请求参数。
/// </summary>
/// <param name="Decision">处理决策。</param>
/// <param name="Note">备注。</param>
public sealed record PlanningSuggestionDecisionRequest(string Decision, string Note);

/// <summary>
/// Outsourcing Order Line 数据传输对象。
/// </summary>
public sealed record OutsourcingOrderLineDto(
    Guid Id,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit);

/// <summary>
/// Outsourcing Order 数据传输对象。
/// </summary>
public sealed record OutsourcingOrderDto(
    Guid Id,
    string OrderNo,
    string SupplierName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid FinishedItemId,
    string FinishedItemCode,
    string FinishedItemName,
    decimal PlannedQuantity,
    decimal ReceivedQuantity,
    string Unit,
    string Status,
    string CreatedBy,
    IReadOnlyList<OutsourcingOrderLineDto> MaterialLines,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Create Outsourcing Order 请求参数。
/// </summary>
public sealed record CreateOutsourcingOrderRequest(
    string SupplierName,
    Guid WarehouseId,
    Guid FinishedItemId,
    decimal PlannedQuantity,
    IReadOnlyList<CreateOutsourcingOrderLineRequest> MaterialLines);

/// <summary>
/// Create Outsourcing Order Line 请求参数。
/// </summary>
public sealed record CreateOutsourcingOrderLineRequest(
    Guid ItemId,
    decimal Quantity);

/// <summary>
/// Receive Outsourcing Order 请求参数。
/// </summary>
/// <param name="Quantity">数量。</param>
public sealed record ReceiveOutsourcingOrderRequest(decimal Quantity);

/// <summary>
/// Barcode Execution 数据传输对象。
/// </summary>
public sealed record BarcodeExecutionDto(
    Guid Id,
    string ExecutionNo,
    string Barcode,
    string Action,
    string Result,
    string Message,
    string DocumentType,
    Guid? DocumentId,
    string DocumentNo,
    string Actor,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Barcode Execution 请求参数。
/// </summary>
public sealed record BarcodeExecutionRequest(
    string Barcode,
    string Action,
    Guid? DocumentId,
    string Note);
