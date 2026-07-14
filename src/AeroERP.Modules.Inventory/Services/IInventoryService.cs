using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Inventory.Contracts;

namespace AeroERP.Modules.Inventory.Services;

/// <summary>
/// Inventory Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// 查询Pending Procurement Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<PendingInventoryReceiptDto>> ListPendingProcurementOrdersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Pending Sales Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<PendingInventoryIssueDto>> ListPendingSalesOrdersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Receipts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<InventoryReceiptDto>> ListReceiptsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Issues。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<InventoryIssueDto>> ListIssuesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Transfers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<InventoryTransferDto>> ListTransfersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Count Adjustments。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<InventoryCountAdjustmentDto>> ListCountAdjustmentsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Movements。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<InventoryMovementDto>> ListMovementsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Inventory Ledger。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<InventoryLedgerEntryDto>> ListInventoryLedgerAsync(Guid? warehouseId, Guid? itemId, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Stock Balances。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<StockBalanceDto>> ListStockBalancesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Warehouse Locations。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<WarehouseLocationDto>> ListWarehouseLocationsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Location Stock Balances。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<LocationStockBalanceDto>> ListLocationStockBalancesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Warehouse Location。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<WarehouseLocationDto>> CreateWarehouseLocationAsync(CreateWarehouseLocationRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Receive Procurement Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<InventoryReceiptDto>> ReceiveProcurementOrderAsync(ReceiveProcurementOrderRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Issue Sales Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<InventoryIssueDto>> IssueSalesOrderAsync(IssueSalesOrderRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Transfer。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<InventoryTransferDto>> CreateTransferAsync(CreateInventoryTransferRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Count Adjustment。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<InventoryCountAdjustmentDto>> CreateCountAdjustmentAsync(CreateInventoryCountAdjustmentRequest request, CancellationToken cancellationToken);
}
