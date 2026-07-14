using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Inventory.Contracts;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.Inventory.Services;
using AeroERP.Modules.MasterData.Domain;
using AeroERP.Modules.Procurement.Domain;
using AeroERP.Modules.Sales.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Inventory.Services;

/// <summary>
/// Inventory Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class InventoryService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IInventoryService
{
    /// <summary>
    /// 查询Pending Procurement Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<PendingInventoryReceiptDto>> ListPendingProcurementOrdersAsync(CancellationToken cancellationToken)
    {
        var releasedOrders = await dbContext.ProcurementOrders
            .Where(x => x.Status == ProcurementOrderStatus.Released)
            .ToListAsync(cancellationToken);

        if (releasedOrders.Count == 0)
        {
            return [];
        }

        var requestIds = releasedOrders.Select(x => x.RequestId).Distinct().ToList();
        var requests = await dbContext.ProcurementRequests
            .Include(x => x.Lines)
            .Where(x => requestIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var itemIds = requests.Values.SelectMany(x => x.Lines).Select(x => x.ItemId).Distinct().ToList();
        var items = await dbContext.Items
            .Where(x => itemIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return releasedOrders
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Where(order => requests.ContainsKey(order.RequestId))
            .Select(order =>
            {
                var request = requests[order.RequestId];
                var lines = request.Lines
                    .Select(line =>
                    {
                        items.TryGetValue(line.ItemId, out var item);
                        return new InventoryReceiptLineDto(
                            line.ItemId,
                            item?.Code ?? string.Empty,
                            line.ItemName,
                            line.Quantity,
                            line.Unit);
                    })
                    .ToList();

                return new PendingInventoryReceiptDto(
                    order.Id,
                    order.OrderNo,
                    order.RequestNo,
                    order.SupplierName,
                    lines,
                    order.UpdatedAtUtc);
            })
            .ToList();
    }

    /// <summary>
    /// 查询Pending Sales Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<PendingInventoryIssueDto>> ListPendingSalesOrdersAsync(CancellationToken cancellationToken)
    {
        var readyOrders = await dbContext.SalesOrders
            .Include(x => x.Lines)
            .Where(x => x.Status == SalesOrderStatus.ReadyToShip)
            .ToListAsync(cancellationToken);

        return readyOrders
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(order => new PendingInventoryIssueDto(
                order.Id,
                order.OrderNo,
                order.QuotationNo,
                order.CustomerName,
                order.Lines.Select(MapLine).ToList(),
                order.UpdatedAtUtc))
            .ToList();
    }

    /// <summary>
    /// 查询Receipts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<InventoryReceiptDto>> ListReceiptsAsync(CancellationToken cancellationToken)
    {
        var receipts = await dbContext.InventoryReceipts
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return receipts
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapReceipt)
            .ToList();
    }

    /// <summary>
    /// 查询Issues。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<InventoryIssueDto>> ListIssuesAsync(CancellationToken cancellationToken)
    {
        var issues = await dbContext.InventoryIssues
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return issues
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapIssue)
            .ToList();
    }

    /// <summary>
    /// 查询Transfers。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<InventoryTransferDto>> ListTransfersAsync(CancellationToken cancellationToken)
    {
        var transfers = await dbContext.InventoryTransfers
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return transfers
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapTransfer)
            .ToList();
    }

    /// <summary>
    /// 查询Count Adjustments。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<InventoryCountAdjustmentDto>> ListCountAdjustmentsAsync(CancellationToken cancellationToken)
    {
        var counts = await dbContext.InventoryCountAdjustments
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return counts
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapCountAdjustment)
            .ToList();
    }

    /// <summary>
    /// 查询Movements。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<InventoryMovementDto>> ListMovementsAsync(CancellationToken cancellationToken)
    {
        var movements = await dbContext.InventoryMovements
            .ToListAsync(cancellationToken);

        return movements
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapMovement)
            .ToList();
    }

    /// <summary>
    /// 查询Inventory Ledger。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<InventoryLedgerEntryDto>> ListInventoryLedgerAsync(
        Guid? warehouseId,
        Guid? itemId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.InventoryMovements.AsQueryable();
        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (itemId.HasValue)
        {
            query = query.Where(x => x.ItemId == itemId.Value);
        }

        var movements = await query.ToListAsync(cancellationToken);

        return movements
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.DocumentNo)
            .Select(MapLedgerEntry)
            .ToList();
    }

    /// <summary>
    /// 查询Stock Balances。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<StockBalanceDto>> ListStockBalancesAsync(CancellationToken cancellationToken)
    {
        var balances = await dbContext.StockBalances
            .OrderBy(x => x.WarehouseCode)
            .ThenBy(x => x.ItemCode)
            .ToListAsync(cancellationToken);

        return balances
            .Select(balance => new StockBalanceDto(
                balance.Id,
                balance.WarehouseId,
                balance.WarehouseCode,
                balance.WarehouseName,
                balance.ItemId,
                balance.ItemCode,
                balance.ItemName,
                balance.Quantity,
                balance.Unit,
                balance.UnitCost,
                balance.InventoryValue,
                balance.UpdatedAtUtc))
            .ToList();
    }

    /// <summary>
    /// 查询Warehouse Locations。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<WarehouseLocationDto>> ListWarehouseLocationsAsync(CancellationToken cancellationToken)
    {
        var locations = await dbContext.WarehouseLocations
            .OrderBy(x => x.WarehouseCode)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return locations.Select(MapLocation).ToList();
    }

    /// <summary>
    /// 查询Location Stock Balances。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<LocationStockBalanceDto>> ListLocationStockBalancesAsync(CancellationToken cancellationToken)
    {
        var balances = await dbContext.LocationStockBalances
            .OrderBy(x => x.WarehouseCode)
            .ThenBy(x => x.LocationCode)
            .ThenBy(x => x.ItemCode)
            .ToListAsync(cancellationToken);

        return balances.Select(MapLocationBalance).ToList();
    }

    /// <summary>
    /// 创建Warehouse Location。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<WarehouseLocationDto>> CreateWarehouseLocationAsync(
        CreateWarehouseLocationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return OperationResult<WarehouseLocationDto>.Failure("库位编码不能为空。");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OperationResult<WarehouseLocationDto>.Failure("库位名称不能为空。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<WarehouseLocationDto>.Failure("未找到可用仓库。");
        }

        var code = request.Code.Trim();
        var exists = await dbContext.WarehouseLocations.AnyAsync(
            x => x.WarehouseId == warehouse.Id && x.Code == code,
            cancellationToken);
        if (exists)
        {
            return OperationResult<WarehouseLocationDto>.Failure("同一仓库下已存在相同库位编码。");
        }

        var actor = currentUser.GetActor();
        var location = new WarehouseLocation(
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            code,
            request.Name.Trim(),
            request.IsEnabled,
            actor);

        dbContext.WarehouseLocations.Add(location);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Inventory", "LocationCreated", actor, $"{warehouse.Code}:{location.Code}", cancellationToken);
        return OperationResult<WarehouseLocationDto>.Success(MapLocation(location));
    }

    /// <summary>
    /// Receive Procurement Order Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<InventoryReceiptDto>> ReceiveProcurementOrderAsync(
        ReceiveProcurementOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.ProcurementOrders
            .FirstOrDefaultAsync(x => x.Id == request.ProcurementOrderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<InventoryReceiptDto>.Failure("未找到采购订单。");
        }

        if (!string.Equals(order.Status, ProcurementOrderStatus.Released, StringComparison.Ordinal))
        {
            return OperationResult<InventoryReceiptDto>.Failure("只有已下达的采购订单才能执行入库。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<InventoryReceiptDto>.Failure("未找到可用仓库。");
        }

        var location = await GetEnabledLocationAsync(warehouse.Id, request.LocationId, cancellationToken);
        if (request.LocationId.HasValue && location is null)
        {
            return OperationResult<InventoryReceiptDto>.Failure("入库库位不存在、已停用或不属于所选仓库。");
        }

        var procurementRequest = await dbContext.ProcurementRequests
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == order.RequestId, cancellationToken);
        if (procurementRequest is null)
        {
            return OperationResult<InventoryReceiptDto>.Failure("采购订单缺少来源申请，无法执行入库。");
        }

        if (procurementRequest.Lines.Count == 0)
        {
            return OperationResult<InventoryReceiptDto>.Failure("采购申请没有行项目，无法执行入库。");
        }

        var itemIds = procurementRequest.Lines.Select(x => x.ItemId).Distinct().ToList();
        var items = await dbContext.Items
            .Where(x => itemIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (items.Count != itemIds.Count)
        {
            return OperationResult<InventoryReceiptDto>.Failure("存在已失效的物料，无法执行入库。");
        }

        var costInputs = NormalizeCostInputs(request.Costs);
        if (costInputs.Values.Any(x => x < 0))
        {
            return OperationResult<InventoryReceiptDto>.Failure("单位成本不能为负数。");
        }

        var actor = currentUser.GetActor();
        var lines = procurementRequest.Lines
            .Select(line =>
            {
                var item = items[line.ItemId];
                var unitCost = costInputs.TryGetValue(item.Id, out var requestedCost) ? requestedCost : 0m;
                return new InventoryReceiptLine(
                    item.Id,
                    item.Code,
                    item.Name,
                    line.Quantity,
                    line.Unit,
                    unitCost,
                    CostAmount(line.Quantity, unitCost));
            })
            .ToList();

        var receipt = new InventoryReceipt(
            $"IR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            order.Id,
            order.OrderNo,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            location?.Id,
            location?.Code ?? string.Empty,
            location?.Name ?? string.Empty,
            order.SupplierName,
            actor,
            lines);

        dbContext.InventoryReceipts.Add(receipt);

        foreach (var line in lines)
        {
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ItemId, cancellationToken);
            if (balance is null)
            {
                balance = new StockBalance(
                    warehouse.Id,
                    warehouse.Code,
                    warehouse.Name,
                    line.ItemId,
                    line.ItemCode,
                    line.ItemName,
                    line.Quantity,
                    line.Unit,
                    line.UnitCost);
                dbContext.StockBalances.Add(balance);
            }
            else
            {
                balance.Increase(line.Quantity, line.UnitCost);
            }

            var locationBalance = location is null
                ? null
                : await IncreaseLocationBalanceAsync(location, line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit, line.UnitCost, cancellationToken);

            AddMovement(
                "InventoryReceipt",
                receipt.ReceiptNo,
                InventoryMovementType.Receipt,
                warehouse,
                line.ItemId,
                line.ItemCode,
                line.ItemName,
                line.Quantity,
                locationBalance?.Quantity ?? balance.Quantity,
                line.Unit,
                actor,
                location,
                line.UnitCost,
                line.CostAmount,
                locationBalance?.InventoryValue ?? balance.InventoryValue);
        }

        order.Receive();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Inventory", "ReceiptCompleted", actor, $"{receipt.ReceiptNo}:{order.OrderNo}", cancellationToken);
        return OperationResult<InventoryReceiptDto>.Success(MapReceipt(receipt));
    }

    /// <summary>
    /// Issue Sales Order Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<InventoryIssueDto>> IssueSalesOrderAsync(
        IssueSalesOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<InventoryIssueDto>.Failure("未找到销售订单。");
        }

        if (!string.Equals(order.Status, SalesOrderStatus.ReadyToShip, StringComparison.Ordinal))
        {
            return OperationResult<InventoryIssueDto>.Failure("只有待出库的销售订单才能执行出库。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<InventoryIssueDto>.Failure("未找到可用仓库。");
        }

        var location = await GetEnabledLocationAsync(warehouse.Id, request.LocationId, cancellationToken);
        if (request.LocationId.HasValue && location is null)
        {
            return OperationResult<InventoryIssueDto>.Failure("出库库位不存在、已停用或不属于所选仓库。");
        }

        if (order.Lines.Count == 0)
        {
            return OperationResult<InventoryIssueDto>.Failure("销售订单没有行项目，无法执行出库。");
        }

        foreach (var line in order.Lines)
        {
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ItemId, cancellationToken);
            if (balance is null)
            {
                return OperationResult<InventoryIssueDto>.Failure($"仓库 {warehouse.Code} 中不存在物料 {line.ItemCode} 的库存。");
            }

            if (balance.Quantity < line.Quantity)
            {
                return OperationResult<InventoryIssueDto>.Failure($"物料 {line.ItemCode} 库存不足，无法完成出库。");
            }

            if (location is not null)
            {
                var locationBalance = await FindLocationStockBalanceAsync(location.Id, line.ItemId, cancellationToken);
                if (locationBalance is null || locationBalance.Quantity < line.Quantity)
                {
                    return OperationResult<InventoryIssueDto>.Failure($"库位 {location.Code} 中物料 {line.ItemCode} 库存不足，无法完成出库。");
                }
            }
        }

        var actor = currentUser.GetActor();
        var lines = order.Lines
            .Select(line =>
            {
                var balance = dbContext.StockBalances.Local.FirstOrDefault(x => x.WarehouseId == warehouse.Id && x.ItemId == line.ItemId)
                    ?? throw new InvalidOperationException("库存成本快照缺失。");
                return new InventoryIssueLine(
                    line.ItemId,
                    line.ItemCode,
                    line.ItemName,
                    line.Quantity,
                    line.Unit,
                    balance.UnitCost,
                    CostAmount(line.Quantity, balance.UnitCost));
            })
            .ToList();

        var issue = new InventoryIssue(
            $"IO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            order.Id,
            order.OrderNo,
            order.QuotationNo,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            location?.Id,
            location?.Code ?? string.Empty,
            location?.Name ?? string.Empty,
            order.CustomerName,
            actor,
            lines);

        dbContext.InventoryIssues.Add(issue);

        foreach (var line in lines)
        {
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ItemId, cancellationToken);
            var unitCost = balance!.UnitCost;
            var costAmount = balance.Decrease(line.Quantity, unitCost);
            var locationBalance = location is null
                ? null
                : await DecreaseLocationBalanceAsync(location, line.ItemId, line.Quantity, unitCost, cancellationToken);

            AddMovement(
                "InventoryIssue",
                issue.IssueNo,
                InventoryMovementType.Issue,
                warehouse,
                line.ItemId,
                line.ItemCode,
                line.ItemName,
                -line.Quantity,
                locationBalance?.Quantity ?? balance.Quantity,
                line.Unit,
                actor,
                location,
                unitCost,
                -costAmount,
                balance.InventoryValue);
        }

        order.Ship();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Inventory", "IssueCompleted", actor, $"{issue.IssueNo}:{order.OrderNo}", cancellationToken);
        return OperationResult<InventoryIssueDto>.Success(MapIssue(issue));
    }

    /// <summary>
    /// 创建Transfer。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<InventoryTransferDto>> CreateTransferAsync(
        CreateInventoryTransferRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
        {
            return OperationResult<InventoryTransferDto>.Failure("库存调拨至少需要一条物料行。");
        }

        if (request.FromWarehouseId == request.ToWarehouseId)
        {
            return OperationResult<InventoryTransferDto>.Failure("调出仓库和调入仓库不能相同。");
        }

        var fromWarehouse = await GetEnabledWarehouseAsync(request.FromWarehouseId, cancellationToken);
        var toWarehouse = await GetEnabledWarehouseAsync(request.ToWarehouseId, cancellationToken);
        if (fromWarehouse is null || toWarehouse is null)
        {
            return OperationResult<InventoryTransferDto>.Failure("调拨仓库不存在或已停用。");
        }

        var fromLocation = await GetEnabledLocationAsync(fromWarehouse.Id, request.FromLocationId, cancellationToken);
        var toLocation = await GetEnabledLocationAsync(toWarehouse.Id, request.ToLocationId, cancellationToken);
        if (request.FromLocationId.HasValue && fromLocation is null)
        {
            return OperationResult<InventoryTransferDto>.Failure("调出库位不存在、已停用或不属于调出仓库。");
        }

        if (request.ToLocationId.HasValue && toLocation is null)
        {
            return OperationResult<InventoryTransferDto>.Failure("调入库位不存在、已停用或不属于调入仓库。");
        }

        var normalized = NormalizeTransferLines(request.Lines);
        if (normalized.Count == 0)
        {
            return OperationResult<InventoryTransferDto>.Failure("库存调拨没有有效的物料数量。");
        }

        var itemIds = normalized.Select(x => x.ItemId).Distinct().ToList();
        var items = await dbContext.Items
            .Where(x => itemIds.Contains(x.Id) && x.IsEnabled)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (items.Count != itemIds.Count)
        {
            return OperationResult<InventoryTransferDto>.Failure("存在不存在或已停用的物料，无法执行调拨。");
        }

        foreach (var line in normalized)
        {
            var sourceBalance = await FindStockBalanceAsync(fromWarehouse.Id, line.ItemId, cancellationToken);
            if (sourceBalance is null || sourceBalance.Quantity < line.Quantity)
            {
                var itemCode = items.TryGetValue(line.ItemId, out var item) ? item.Code : line.ItemId.ToString();
                return OperationResult<InventoryTransferDto>.Failure($"物料 {itemCode} 在调出仓库中的库存不足。");
            }

            if (fromLocation is not null)
            {
                var sourceLocationBalance = await FindLocationStockBalanceAsync(fromLocation.Id, line.ItemId, cancellationToken);
                if (sourceLocationBalance is null || sourceLocationBalance.Quantity < line.Quantity)
                {
                    var itemCode = items.TryGetValue(line.ItemId, out var item) ? item.Code : line.ItemId.ToString();
                    return OperationResult<InventoryTransferDto>.Failure($"物料 {itemCode} 在调出库位中的库存不足。");
                }
            }
        }

        var actor = currentUser.GetActor();
        var transferLines = normalized
            .Select(line =>
            {
                var item = items[line.ItemId];
                var sourceBalance = dbContext.StockBalances.Local.FirstOrDefault(x => x.WarehouseId == fromWarehouse.Id && x.ItemId == item.Id)
                    ?? throw new InvalidOperationException("调拨成本快照缺失。");
                return new InventoryTransferLine(
                    item.Id,
                    item.Code,
                    item.Name,
                    line.Quantity,
                    line.Unit,
                    sourceBalance.UnitCost,
                    CostAmount(line.Quantity, sourceBalance.UnitCost));
            })
            .ToList();

        var transfer = new InventoryTransfer(
            $"IT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            fromWarehouse.Id,
            fromWarehouse.Code,
            fromWarehouse.Name,
            fromLocation?.Id,
            fromLocation?.Code ?? string.Empty,
            fromLocation?.Name ?? string.Empty,
            toWarehouse.Id,
            toWarehouse.Code,
            toWarehouse.Name,
            toLocation?.Id,
            toLocation?.Code ?? string.Empty,
            toLocation?.Name ?? string.Empty,
            request.Reason.Trim(),
            actor,
            transferLines);

        dbContext.InventoryTransfers.Add(transfer);

        foreach (var line in transferLines)
        {
            var sourceBalance = await FindStockBalanceAsync(fromWarehouse.Id, line.ItemId, cancellationToken);
            var unitCost = line.UnitCost;
            var costAmount = sourceBalance!.Decrease(line.Quantity, unitCost);
            var sourceLocationBalance = fromLocation is null
                ? null
                : await DecreaseLocationBalanceAsync(fromLocation, line.ItemId, line.Quantity, unitCost, cancellationToken);

            var targetBalance = await FindStockBalanceAsync(toWarehouse.Id, line.ItemId, cancellationToken);
            if (targetBalance is null)
            {
                targetBalance = new StockBalance(
                    toWarehouse.Id,
                    toWarehouse.Code,
                    toWarehouse.Name,
                    line.ItemId,
                    line.ItemCode,
                    line.ItemName,
                    line.Quantity,
                    line.Unit,
                    unitCost);
                dbContext.StockBalances.Add(targetBalance);
            }
            else
            {
                targetBalance.Increase(line.Quantity, unitCost);
            }

            var targetLocationBalance = toLocation is null
                ? null
                : await IncreaseLocationBalanceAsync(toLocation, line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit, unitCost, cancellationToken);

            AddMovement(
                "InventoryTransfer",
                transfer.TransferNo,
                InventoryMovementType.TransferOut,
                fromWarehouse,
                line.ItemId,
                line.ItemCode,
                line.ItemName,
                -line.Quantity,
                sourceLocationBalance?.Quantity ?? sourceBalance.Quantity,
                line.Unit,
                actor,
                fromLocation,
                unitCost,
                -costAmount,
                sourceBalance.InventoryValue);

            AddMovement(
                "InventoryTransfer",
                transfer.TransferNo,
                InventoryMovementType.TransferIn,
                toWarehouse,
                line.ItemId,
                line.ItemCode,
                line.ItemName,
                line.Quantity,
                targetLocationBalance?.Quantity ?? targetBalance.Quantity,
                line.Unit,
                actor,
                toLocation,
                unitCost,
                costAmount,
                targetBalance.InventoryValue);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Inventory", "TransferCompleted", actor, transfer.TransferNo, cancellationToken);
        return OperationResult<InventoryTransferDto>.Success(MapTransfer(transfer));
    }

    /// <summary>
    /// 创建Count Adjustment。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<InventoryCountAdjustmentDto>> CreateCountAdjustmentAsync(
        CreateInventoryCountAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
        {
            return OperationResult<InventoryCountAdjustmentDto>.Failure("库存盘点至少需要一条物料行。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<InventoryCountAdjustmentDto>.Failure("未找到可用仓库。");
        }

        var location = await GetEnabledLocationAsync(warehouse.Id, request.LocationId, cancellationToken);
        if (request.LocationId.HasValue && location is null)
        {
            return OperationResult<InventoryCountAdjustmentDto>.Failure("盘点库位不存在、已停用或不属于所选仓库。");
        }

        var normalized = NormalizeCountLines(request.Lines);
        if (normalized.Count == 0)
        {
            return OperationResult<InventoryCountAdjustmentDto>.Failure("库存盘点没有有效的物料数量。");
        }

        var itemIds = normalized.Select(x => x.ItemId).Distinct().ToList();
        var items = await dbContext.Items
            .Where(x => itemIds.Contains(x.Id) && x.IsEnabled)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (items.Count != itemIds.Count)
        {
            return OperationResult<InventoryCountAdjustmentDto>.Failure("存在不存在或已停用的物料，无法执行盘点。");
        }

        var actor = currentUser.GetActor();
        var lines = new List<InventoryCountAdjustmentLine>();
        var countNo = $"IC-{DateTime.UtcNow:yyyyMMddHHmmss}";

        foreach (var line in normalized)
        {
            var item = items[line.ItemId];
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ItemId, cancellationToken);
            var locationBalance = location is null
                ? null
                : await FindLocationStockBalanceAsync(location.Id, line.ItemId, cancellationToken);
            var beforeQuantity = location is null ? balance?.Quantity ?? 0m : locationBalance?.Quantity ?? 0m;
            var deltaQuantity = line.CountedQuantity - beforeQuantity;
            var unitCost = deltaQuantity > 0
                ? line.UnitCost ?? locationBalance?.UnitCost ?? balance?.UnitCost ?? 0m
                : locationBalance?.UnitCost ?? balance?.UnitCost ?? 0m;
            if (unitCost < 0)
            {
                return OperationResult<InventoryCountAdjustmentDto>.Failure("单位成本不能为负数。");
            }

            var costAmount = CostAmount(Math.Abs(deltaQuantity), unitCost);

            if (location is null && balance is null)
            {
                if (line.CountedQuantity > 0)
                {
                    balance = new StockBalance(
                        warehouse.Id,
                        warehouse.Code,
                        warehouse.Name,
                        item.Id,
                        item.Code,
                        item.Name,
                        line.CountedQuantity,
                        item.Unit,
                        unitCost);
                    dbContext.StockBalances.Add(balance);
                }
            }
            else if (location is null && balance is not null)
            {
                if (deltaQuantity > 0)
                {
                    balance.Increase(deltaQuantity, unitCost);
                }
                else if (deltaQuantity < 0)
                {
                    costAmount = balance.Decrease(Math.Abs(deltaQuantity), unitCost);
                }
            }
            else if (location is WarehouseLocation selectedLocation)
            {
                if (locationBalance is null)
                {
                    if (line.CountedQuantity > 0)
                    {
                        locationBalance = new LocationStockBalance(
                            warehouse.Id,
                            warehouse.Code,
                            warehouse.Name,
                            selectedLocation.Id,
                            selectedLocation.Code,
                            selectedLocation.Name,
                            item.Id,
                            item.Code,
                            item.Name,
                            line.CountedQuantity,
                            item.Unit,
                            unitCost);
                        dbContext.LocationStockBalances.Add(locationBalance);
                    }
                }
                else
                {
                    if (deltaQuantity > 0)
                    {
                        locationBalance.Increase(deltaQuantity, unitCost);
                    }
                    else if (deltaQuantity < 0)
                    {
                        costAmount = locationBalance.Decrease(Math.Abs(deltaQuantity), unitCost);
                    }
                }

                if (deltaQuantity > 0)
                {
                    if (balance is null)
                    {
                        balance = new StockBalance(
                            warehouse.Id,
                            warehouse.Code,
                            warehouse.Name,
                            item.Id,
                            item.Code,
                            item.Name,
                            deltaQuantity,
                            item.Unit,
                            unitCost);
                        dbContext.StockBalances.Add(balance);
                    }
                    else
                    {
                        balance.Increase(deltaQuantity, unitCost);
                    }
                }
                else if (deltaQuantity < 0)
                {
                    if (balance is null)
                    {
                        return OperationResult<InventoryCountAdjustmentDto>.Failure($"仓库 {warehouse.Code} 缺少物料 {item.Code} 的库存，无法执行库位盘亏。");
                    }

                    try
                    {
                        balance.Decrease(Math.Abs(deltaQuantity), unitCost);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return OperationResult<InventoryCountAdjustmentDto>.Failure(ex.Message);
                    }
                }
            }

            if (deltaQuantity != 0)
            {
                AddMovement(
                    "InventoryCountAdjustment",
                    countNo,
                    deltaQuantity > 0 ? InventoryMovementType.CountIncrease : InventoryMovementType.CountDecrease,
                    warehouse,
                    item.Id,
                    item.Code,
                    item.Name,
                    deltaQuantity,
                    line.CountedQuantity,
                    item.Unit,
                    actor,
                    location,
                    unitCost,
                    deltaQuantity > 0 ? costAmount : -costAmount,
                    balance?.InventoryValue ?? 0m);
            }

            lines.Add(new InventoryCountAdjustmentLine(
                item.Id,
                item.Code,
                item.Name,
                beforeQuantity,
                line.CountedQuantity,
                deltaQuantity,
                item.Unit,
                unitCost,
                deltaQuantity == 0 ? 0m : costAmount));
        }

        var adjustment = new InventoryCountAdjustment(
            countNo,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            location?.Id,
            location?.Code ?? string.Empty,
            location?.Name ?? string.Empty,
            request.Reason.Trim(),
            actor,
            lines);

        dbContext.InventoryCountAdjustments.Add(adjustment);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Inventory", "CountCompleted", actor, adjustment.CountNo, cancellationToken);
        return OperationResult<InventoryCountAdjustmentDto>.Success(MapCountAdjustment(adjustment));
    }

    /// <summary>
    /// 获取Enabled Warehouse。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<Warehouse?> GetEnabledWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        return await dbContext.Warehouses
            .FirstOrDefaultAsync(x => x.Id == warehouseId && x.IsEnabled, cancellationToken);
    }

    /// <summary>
    /// 获取Enabled Location。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<WarehouseLocation?> GetEnabledLocationAsync(Guid warehouseId, Guid? locationId, CancellationToken cancellationToken)
    {
        if (!locationId.HasValue)
        {
            return null;
        }

        return await dbContext.WarehouseLocations.FirstOrDefaultAsync(
            x => x.Id == locationId.Value && x.WarehouseId == warehouseId && x.IsEnabled,
            cancellationToken);
    }

    /// <summary>
    /// Find Stock Balance Async。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<StockBalance?> FindStockBalanceAsync(Guid warehouseId, Guid itemId, CancellationToken cancellationToken)
    {
        return dbContext.StockBalances.Local.FirstOrDefault(x => x.WarehouseId == warehouseId && x.ItemId == itemId)
            ?? await dbContext.StockBalances.FirstOrDefaultAsync(
                x => x.WarehouseId == warehouseId && x.ItemId == itemId,
                cancellationToken);
    }

    /// <summary>
    /// Find Location Stock Balance Async。
    /// </summary>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<LocationStockBalance?> FindLocationStockBalanceAsync(Guid locationId, Guid itemId, CancellationToken cancellationToken)
    {
        return dbContext.LocationStockBalances.Local.FirstOrDefault(x => x.LocationId == locationId && x.ItemId == itemId)
            ?? await dbContext.LocationStockBalances.FirstOrDefaultAsync(
                x => x.LocationId == locationId && x.ItemId == itemId,
                cancellationToken);
    }

    /// <summary>
    /// Increase Location Balance Async。
    /// </summary>
    /// <param name="location">位置说明。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="unitCost">单位成本。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<LocationStockBalance> IncreaseLocationBalanceAsync(
        WarehouseLocation location,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal quantity,
        string unit,
        decimal unitCost,
        CancellationToken cancellationToken)
    {
        var balance = await FindLocationStockBalanceAsync(location.Id, itemId, cancellationToken);
        if (balance is null)
        {
            balance = new LocationStockBalance(
                location.WarehouseId,
                location.WarehouseCode,
                location.WarehouseName,
                location.Id,
                location.Code,
                location.Name,
                itemId,
                itemCode,
                itemName,
                quantity,
                unit,
                unitCost);
            dbContext.LocationStockBalances.Add(balance);
        }
        else
        {
            balance.Increase(quantity, unitCost);
        }

        return balance;
    }

    /// <summary>
    /// Decrease Location Balance Async。
    /// </summary>
    /// <param name="location">位置说明。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unitCost">单位成本。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<LocationStockBalance> DecreaseLocationBalanceAsync(
        WarehouseLocation location,
        Guid itemId,
        decimal quantity,
        decimal unitCost,
        CancellationToken cancellationToken)
    {
        var balance = await FindLocationStockBalanceAsync(location.Id, itemId, cancellationToken);
        if (balance is null)
        {
            throw new InvalidOperationException("库位库存不存在，无法执行扣减。");
        }

        balance.Decrease(quantity, unitCost);
        return balance;
    }

    /// <summary>
    /// Add Movement。
    /// </summary>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="documentNo">业务单据编号。</param>
    /// <param name="movementType">movement Type 参数。</param>
    /// <param name="warehouse">仓库对象。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="changeQuantity">change Quantity 参数。</param>
    /// <param name="balanceAfter">balance After 参数。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="location">位置说明。</param>
    /// <param name="unitCost">单位成本。</param>
    /// <param name="costAmount">成本金额。</param>
    /// <param name="balanceCostAfter">balance Cost After 参数。</param>
    private void AddMovement(
        string documentType,
        string documentNo,
        string movementType,
        Warehouse warehouse,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal changeQuantity,
        decimal balanceAfter,
        string unit,
        string actor,
        WarehouseLocation? location = null,
        decimal unitCost = 0m,
        decimal costAmount = 0m,
        decimal balanceCostAfter = 0m)
    {
        dbContext.InventoryMovements.Add(new InventoryMovement(
            documentType,
            documentNo,
            movementType,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            itemId,
            itemCode,
            itemName,
            changeQuantity,
            balanceAfter,
            unit,
            actor,
            location?.Id,
            location?.Code ?? string.Empty,
            location?.Name ?? string.Empty,
            unitCost,
            costAmount,
            balanceCostAfter));
    }

    /// <summary>
    /// Cost Amount。
    /// </summary>
    /// <param name="quantity">数量。</param>
    /// <param name="unitCost">单位成本。</param>
    private static decimal CostAmount(decimal quantity, decimal unitCost) => quantity * unitCost;

    /// <summary>
    /// Normalize Cost Inputs。
    /// </summary>
    /// <param name="costs">成本集合。</param>
    private static Dictionary<Guid, decimal> NormalizeCostInputs(IReadOnlyList<InventoryCostInputRequest>? costs)
    {
        if (costs is null || costs.Count == 0)
        {
            return [];
        }

        return costs
            .Where(x => x.ItemId != Guid.Empty)
            .GroupBy(x => x.ItemId)
            .ToDictionary(x => x.Key, x => x.Last().UnitCost);
    }

    /// <summary>
    /// Normalize Transfer Lines。
    /// </summary>
    /// <param name="lines">明细行集合。</param>
    private static IReadOnlyList<CreateInventoryTransferLineRequest> NormalizeTransferLines(
        IReadOnlyList<CreateInventoryTransferLineRequest> lines)
    {
        return lines
            .Where(x => x.Quantity > 0 && !string.IsNullOrWhiteSpace(x.Unit))
            .GroupBy(x => new { x.ItemId, Unit = x.Unit.Trim() })
            .Select(group => new CreateInventoryTransferLineRequest(
                group.Key.ItemId,
                group.Sum(x => x.Quantity),
                group.Key.Unit))
            .ToList();
    }

    /// <summary>
    /// Normalize Count Lines。
    /// </summary>
    /// <param name="lines">明细行集合。</param>
    private static IReadOnlyList<CreateInventoryCountLineRequest> NormalizeCountLines(
        IReadOnlyList<CreateInventoryCountLineRequest> lines)
    {
        return lines
            .Where(x => x.CountedQuantity >= 0)
            .GroupBy(x => x.ItemId)
            .Select(group => new CreateInventoryCountLineRequest(
                group.Key,
                group.Last().CountedQuantity,
                group.Last().UnitCost))
            .ToList();
    }

    /// <summary>
    /// 注册Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static InventoryReceiptLineDto MapLine(SalesOrderLine line) =>
        new(line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit);

    /// <summary>
    /// 注册Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static InventoryReceiptLineDto MapLine(InventoryReceiptLine line) =>
        new(line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit, line.UnitCost, line.CostAmount);

    /// <summary>
    /// 注册Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static InventoryReceiptLineDto MapLine(InventoryIssueLine line) =>
        new(line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit, line.UnitCost, line.CostAmount);

    /// <summary>
    /// 注册Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static InventoryReceiptLineDto MapLine(InventoryTransferLine line) =>
        new(line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit, line.UnitCost, line.CostAmount);

    /// <summary>
    /// 注册Receipt 路由。
    /// </summary>
    /// <param name="receipt">入库单。</param>
    private static InventoryReceiptDto MapReceipt(InventoryReceipt receipt) =>
        new(
            receipt.Id,
            receipt.ReceiptNo,
            receipt.ProcurementOrderId,
            receipt.ProcurementOrderNo,
            receipt.WarehouseId,
            receipt.WarehouseCode,
            receipt.WarehouseName,
            receipt.LocationId,
            receipt.LocationCode,
            receipt.LocationName,
            receipt.SupplierName,
            receipt.Status,
            receipt.Lines.Select(MapLine).ToList(),
            receipt.CreatedAtUtc);

    /// <summary>
    /// 注册Issue 路由。
    /// </summary>
    /// <param name="issue">出库单。</param>
    private static InventoryIssueDto MapIssue(InventoryIssue issue) =>
        new(
            issue.Id,
            issue.IssueNo,
            issue.SalesOrderId,
            issue.SalesOrderNo,
            issue.QuotationNo,
            issue.WarehouseId,
            issue.WarehouseCode,
            issue.WarehouseName,
            issue.LocationId,
            issue.LocationCode,
            issue.LocationName,
            issue.CustomerName,
            issue.Status,
            issue.Lines.Select(MapLine).ToList(),
            issue.CreatedAtUtc);

    /// <summary>
    /// 注册Transfer 路由。
    /// </summary>
    /// <param name="transfer">调拨单。</param>
    private static InventoryTransferDto MapTransfer(InventoryTransfer transfer) =>
        new(
            transfer.Id,
            transfer.TransferNo,
            transfer.FromWarehouseId,
            transfer.FromWarehouseCode,
            transfer.FromWarehouseName,
            transfer.FromLocationId,
            transfer.FromLocationCode,
            transfer.FromLocationName,
            transfer.ToWarehouseId,
            transfer.ToWarehouseCode,
            transfer.ToWarehouseName,
            transfer.ToLocationId,
            transfer.ToLocationCode,
            transfer.ToLocationName,
            transfer.Reason,
            transfer.Status,
            transfer.Lines.Select(MapLine).ToList(),
            transfer.CreatedAtUtc);

    /// <summary>
    /// 注册Count Adjustment 路由。
    /// </summary>
    /// <param name="adjustment">盘点调整。</param>
    private static InventoryCountAdjustmentDto MapCountAdjustment(InventoryCountAdjustment adjustment) =>
        new(
            adjustment.Id,
            adjustment.CountNo,
            adjustment.WarehouseId,
            adjustment.WarehouseCode,
            adjustment.WarehouseName,
            adjustment.LocationId,
            adjustment.LocationCode,
            adjustment.LocationName,
            adjustment.Reason,
            adjustment.Status,
            adjustment.Lines
                .Select(line => new InventoryCountAdjustmentLineDto(
                    line.ItemId,
                    line.ItemCode,
                    line.ItemName,
                    line.BeforeQuantity,
                    line.CountedQuantity,
                    line.DeltaQuantity,
                    line.Unit,
                    line.UnitCost,
                    line.CostAmount))
                .ToList(),
            adjustment.CreatedAtUtc);

    /// <summary>
    /// 注册Movement 路由。
    /// </summary>
    /// <param name="movement">库存流水。</param>
    private static InventoryMovementDto MapMovement(InventoryMovement movement) =>
        new(
            movement.Id,
            movement.DocumentType,
            movement.DocumentNo,
            movement.MovementType,
            movement.WarehouseId,
            movement.WarehouseCode,
            movement.WarehouseName,
            movement.LocationId,
            movement.LocationCode,
            movement.LocationName,
            movement.ItemId,
            movement.ItemCode,
            movement.ItemName,
            movement.ChangeQuantity,
            movement.BalanceAfter,
            movement.Unit,
            movement.UnitCost,
            movement.CostAmount,
            movement.BalanceCostAfter,
            movement.Actor,
            movement.CreatedAtUtc);

    /// <summary>
    /// 注册Ledger Entry 路由。
    /// </summary>
    /// <param name="movement">库存流水。</param>
    private static InventoryLedgerEntryDto MapLedgerEntry(InventoryMovement movement)
    {
        var inQuantity = movement.ChangeQuantity > 0 ? movement.ChangeQuantity : 0m;
        var outQuantity = movement.ChangeQuantity < 0 ? Math.Abs(movement.ChangeQuantity) : 0m;
        var inAmount = movement.CostAmount > 0 ? movement.CostAmount : 0m;
        var outAmount = movement.CostAmount < 0 ? Math.Abs(movement.CostAmount) : 0m;

        return new InventoryLedgerEntryDto(
            movement.Id,
            movement.DocumentType,
            movement.DocumentNo,
            movement.MovementType,
            movement.WarehouseId,
            movement.WarehouseCode,
            movement.WarehouseName,
            movement.LocationId,
            movement.LocationCode,
            movement.LocationName,
            movement.ItemId,
            movement.ItemCode,
            movement.ItemName,
            inQuantity,
            outQuantity,
            movement.BalanceAfter,
            movement.Unit,
            movement.UnitCost,
            inAmount,
            outAmount,
            movement.BalanceCostAfter,
            movement.Actor,
            movement.CreatedAtUtc);
    }

    /// <summary>
    /// 注册Location 路由。
    /// </summary>
    /// <param name="location">位置说明。</param>
    private static WarehouseLocationDto MapLocation(WarehouseLocation location) =>
        new(
            location.Id,
            location.WarehouseId,
            location.WarehouseCode,
            location.WarehouseName,
            location.Code,
            location.Name,
            location.IsEnabled,
            location.CreatedBy,
            location.UpdatedAtUtc);

    /// <summary>
    /// 注册Location Balance 路由。
    /// </summary>
    /// <param name="balance">库存余额。</param>
    private static LocationStockBalanceDto MapLocationBalance(LocationStockBalance balance) =>
        new(
            balance.Id,
            balance.WarehouseId,
            balance.WarehouseCode,
            balance.WarehouseName,
            balance.LocationId,
            balance.LocationCode,
            balance.LocationName,
            balance.ItemId,
            balance.ItemCode,
            balance.ItemName,
            balance.Quantity,
            balance.Unit,
            balance.UnitCost,
            balance.InventoryValue,
            balance.UpdatedAtUtc);
}
