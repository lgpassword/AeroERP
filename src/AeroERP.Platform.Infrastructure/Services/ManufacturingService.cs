using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.AdvancedManufacturing.Domain;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.Manufacturing.Contracts;
using AeroERP.Modules.Manufacturing.Domain;
using AeroERP.Modules.Manufacturing.Services;
using AeroERP.Modules.MasterData.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Manufacturing Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class ManufacturingService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IManufacturingService
{
    /// <summary>
    /// 查询Boms。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<BomDto>> ListBomsAsync(CancellationToken cancellationToken)
    {
        var boms = await dbContext.BillOfMaterials
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return boms
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(MapBom)
            .ToList();
    }

    /// <summary>
    /// 创建Bom。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<BomDto>> CreateBomAsync(CreateBomRequest request, CancellationToken cancellationToken)
    {
        if (request.BaseQuantity <= 0)
        {
            return OperationResult<BomDto>.Failure("BOM 基准数量必须大于零。");
        }

        if (request.Lines.Count == 0)
        {
            return OperationResult<BomDto>.Failure("BOM 至少需要一条组件物料。");
        }

        var finishedItem = await dbContext.Items
            .FirstOrDefaultAsync(x => x.Id == request.FinishedItemId && x.IsEnabled, cancellationToken);
        if (finishedItem is null)
        {
            return OperationResult<BomDto>.Failure("成品物料不存在或已停用。");
        }

        var normalizedLines = NormalizeBomLines(request.Lines);
        if (normalizedLines.Count == 0)
        {
            return OperationResult<BomDto>.Failure("BOM 组件数量必须大于零。");
        }

        if (normalizedLines.Any(x => x.ComponentItemId == finishedItem.Id))
        {
            return OperationResult<BomDto>.Failure("BOM 组件不能与成品物料相同。");
        }

        var componentIds = normalizedLines.Select(x => x.ComponentItemId).Distinct().ToList();
        var components = await dbContext.Items
            .Where(x => componentIds.Contains(x.Id) && x.IsEnabled)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (components.Count != componentIds.Count)
        {
            return OperationResult<BomDto>.Failure("存在不存在或已停用的组件物料。");
        }

        var version = string.IsNullOrWhiteSpace(request.Version)
            ? "V1"
            : request.Version.Trim();

        var duplicate = await dbContext.BillOfMaterials.AnyAsync(
            x => x.FinishedItemId == finishedItem.Id && x.Version == version,
            cancellationToken);
        if (duplicate)
        {
            return OperationResult<BomDto>.Failure("该成品物料的 BOM 版本已存在。");
        }

        var lines = normalizedLines
            .Select(line =>
            {
                var component = components[line.ComponentItemId];
                return new BillOfMaterialLine(component.Id, component.Code, component.Name, line.Quantity, component.Unit);
            })
            .ToList();

        var bom = new BillOfMaterial(
            $"BOM-{DateTime.UtcNow:yyyyMMddHHmmss}",
            finishedItem.Id,
            finishedItem.Code,
            finishedItem.Name,
            version,
            request.BaseQuantity,
            finishedItem.Unit,
            request.IsEnabled,
            lines);

        dbContext.BillOfMaterials.Add(bom);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Manufacturing", "BomCreated", currentUser.GetActor(), bom.BomNo, cancellationToken);
        return OperationResult<BomDto>.Success(MapBom(bom));
    }

    /// <summary>
    /// 查询Work Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<WorkOrderDto>> ListWorkOrdersAsync(CancellationToken cancellationToken)
    {
        var workOrders = await dbContext.WorkOrders
            .Include(x => x.MaterialLines)
            .ToListAsync(cancellationToken);

        var result = new List<WorkOrderDto>();
        foreach (var workOrder in workOrders.OrderByDescending(x => x.UpdatedAtUtc))
        {
            result.Add(await MapWorkOrderAsync(workOrder, cancellationToken));
        }

        return result;
    }

    /// <summary>
    /// 创建Work Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<WorkOrderDto>> CreateWorkOrderAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.PlannedQuantity <= 0)
        {
            return OperationResult<WorkOrderDto>.Failure("工单计划数量必须大于零。");
        }

        var bom = await dbContext.BillOfMaterials
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.BomId, cancellationToken);
        if (bom is null)
        {
            return OperationResult<WorkOrderDto>.Failure("未找到 BOM。");
        }

        if (!bom.IsEnabled)
        {
            return OperationResult<WorkOrderDto>.Failure("BOM 已停用，不能创建工单。");
        }

        if (bom.Lines.Count == 0)
        {
            return OperationResult<WorkOrderDto>.Failure("BOM 没有组件行，不能创建工单。");
        }

        var factor = request.PlannedQuantity / bom.BaseQuantity;
        var materialLines = bom.Lines
            .Select(line => new WorkOrderMaterialLine(
                line.ComponentItemId,
                line.ComponentItemCode,
                line.ComponentItemName,
                line.Quantity * factor,
                line.Unit))
            .ToList();

        var actor = currentUser.GetActor();
        var workOrder = new WorkOrder(
            $"WO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            bom.Id,
            bom.BomNo,
            bom.Version,
            bom.FinishedItemId,
            bom.FinishedItemCode,
            bom.FinishedItemName,
            request.PlannedQuantity,
            bom.Unit,
            actor,
            materialLines);

        dbContext.WorkOrders.Add(workOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Manufacturing", "WorkOrderCreated", actor, workOrder.WorkOrderNo, cancellationToken);
        return OperationResult<WorkOrderDto>.Success(await MapWorkOrderAsync(workOrder, cancellationToken));
    }

    /// <summary>
    /// Release Work Order Async。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<WorkOrderDto>> ReleaseWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken)
    {
        var workOrder = await dbContext.WorkOrders
            .Include(x => x.MaterialLines)
            .FirstOrDefaultAsync(x => x.Id == workOrderId, cancellationToken);
        if (workOrder is null)
        {
            return OperationResult<WorkOrderDto>.Failure("未找到工单。");
        }

        try
        {
            workOrder.Release();
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<WorkOrderDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Manufacturing", "WorkOrderReleased", currentUser.GetActor(), workOrder.WorkOrderNo, cancellationToken);
        return OperationResult<WorkOrderDto>.Success(await MapWorkOrderAsync(workOrder, cancellationToken));
    }

    /// <summary>
    /// 查询Production Issues。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ProductionIssueDto>> ListProductionIssuesAsync(CancellationToken cancellationToken)
    {
        var issues = await dbContext.ProductionIssues
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return issues
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapProductionIssue)
            .ToList();
    }

    /// <summary>
    /// Execute Production Issue Async。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ProductionIssueDto>> ExecuteProductionIssueAsync(
        Guid workOrderId,
        ExecuteProductionIssueRequest request,
        CancellationToken cancellationToken)
    {
        var workOrder = await dbContext.WorkOrders
            .Include(x => x.MaterialLines)
            .FirstOrDefaultAsync(x => x.Id == workOrderId, cancellationToken);
        if (workOrder is null)
        {
            return OperationResult<ProductionIssueDto>.Failure("未找到工单。");
        }

        if (!string.Equals(workOrder.Status, WorkOrderStatus.Released, StringComparison.Ordinal))
        {
            return OperationResult<ProductionIssueDto>.Failure("只有已下达工单可以生产领料。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<ProductionIssueDto>.Failure("未找到可用领料仓库。");
        }

        var remainingLines = workOrder.MaterialLines
            .Where(x => x.RemainingQuantity > 0)
            .ToList();
        if (remainingLines.Count == 0)
        {
            return OperationResult<ProductionIssueDto>.Failure("工单没有待领物料。");
        }

        foreach (var line in remainingLines)
        {
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ComponentItemId, cancellationToken);
            if (balance is null || balance.Quantity < line.RemainingQuantity)
            {
                return OperationResult<ProductionIssueDto>.Failure($"物料 {line.ComponentItemCode} 库存不足，无法生产领料。");
            }
        }

        var actor = currentUser.GetActor();
        var issueNo = $"PI-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var issueLines = remainingLines
            .Select(line =>
            {
                var balance = dbContext.StockBalances.Local.FirstOrDefault(x => x.WarehouseId == warehouse.Id && x.ItemId == line.ComponentItemId)
                    ?? throw new InvalidOperationException("生产领料成本快照缺失。");
                return new ProductionIssueLine(
                    line.ComponentItemId,
                    line.ComponentItemCode,
                    line.ComponentItemName,
                    line.RemainingQuantity,
                    line.Unit,
                    balance.UnitCost,
                    CostAmount(line.RemainingQuantity, balance.UnitCost));
            })
            .ToList();

        var issue = new ProductionIssue(
            issueNo,
            workOrder.Id,
            workOrder.WorkOrderNo,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            actor,
            issueLines);

        dbContext.ProductionIssues.Add(issue);

        foreach (var line in remainingLines)
        {
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ComponentItemId, cancellationToken);
            var unitCost = balance!.UnitCost;
            var costAmount = balance.Decrease(line.RemainingQuantity, unitCost);
            AddMovement(
                "ProductionIssue",
                issueNo,
                InventoryMovementType.Issue,
                warehouse,
                line.ComponentItemId,
                line.ComponentItemCode,
                line.ComponentItemName,
                -line.RemainingQuantity,
                balance.Quantity,
                line.Unit,
                actor,
                unitCost,
                -costAmount,
                balance.InventoryValue);
            line.IssueRemaining();
        }

        try
        {
            workOrder.MarkMaterialsIssued();
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<ProductionIssueDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Manufacturing", "ProductionIssueCompleted", actor, $"{issue.IssueNo}:{workOrder.WorkOrderNo}", cancellationToken);
        return OperationResult<ProductionIssueDto>.Success(MapProductionIssue(issue));
    }

    /// <summary>
    /// 查询Production Receipts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ProductionReceiptDto>> ListProductionReceiptsAsync(CancellationToken cancellationToken)
    {
        var receipts = await dbContext.ProductionReceipts.ToListAsync(cancellationToken);
        return receipts
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapProductionReceipt)
            .ToList();
    }

    /// <summary>
    /// Complete Production Async。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ProductionReceiptDto>> CompleteProductionAsync(
        Guid workOrderId,
        CompleteProductionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return OperationResult<ProductionReceiptDto>.Failure("完工入库数量必须大于零。");
        }

        var workOrder = await dbContext.WorkOrders
            .Include(x => x.MaterialLines)
            .FirstOrDefaultAsync(x => x.Id == workOrderId, cancellationToken);
        if (workOrder is null)
        {
            return OperationResult<ProductionReceiptDto>.Failure("未找到工单。");
        }

        if (!string.Equals(workOrder.Status, WorkOrderStatus.MaterialsIssued, StringComparison.Ordinal) &&
            !string.Equals(workOrder.Status, WorkOrderStatus.PartiallyCompleted, StringComparison.Ordinal))
        {
            return OperationResult<ProductionReceiptDto>.Failure("只有已领料或部分完工的工单可以完工入库。");
        }

        if (workOrder.CompletedQuantity + request.Quantity > workOrder.PlannedQuantity)
        {
            return OperationResult<ProductionReceiptDto>.Failure("完工入库数量不能超过工单剩余计划数量。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<ProductionReceiptDto>.Failure("未找到可用入库仓库。");
        }

        var actor = currentUser.GetActor();
        var receiptNo = $"PRC-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var costState = await BuildWorkOrderCostStateAsync(workOrder.Id, cancellationToken);
        var completionCosts = CalculateCompletionCosts(workOrder, request.Quantity, costState);
        var receiptUnitCost = request.Quantity == 0 ? 0m : completionCosts.TotalCost / request.Quantity;
        var balance = await FindStockBalanceAsync(warehouse.Id, workOrder.FinishedItemId, cancellationToken);
        if (balance is null)
        {
            balance = new StockBalance(
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                workOrder.FinishedItemId,
                workOrder.FinishedItemCode,
                workOrder.FinishedItemName,
                request.Quantity,
                workOrder.Unit,
                receiptUnitCost);
            dbContext.StockBalances.Add(balance);
        }
        else
        {
            balance.Increase(request.Quantity, receiptUnitCost);
        }

        AddMovement(
            "ProductionReceipt",
            receiptNo,
            InventoryMovementType.Receipt,
            warehouse,
            workOrder.FinishedItemId,
            workOrder.FinishedItemCode,
            workOrder.FinishedItemName,
            request.Quantity,
            balance.Quantity,
            workOrder.Unit,
            actor,
            receiptUnitCost,
            completionCosts.TotalCost,
            balance.InventoryValue);

        var receipt = new ProductionReceipt(
            receiptNo,
            workOrder.Id,
            workOrder.WorkOrderNo,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            workOrder.FinishedItemId,
            workOrder.FinishedItemCode,
            workOrder.FinishedItemName,
            request.Quantity,
            workOrder.Unit,
            actor,
            receiptUnitCost,
            completionCosts.MaterialCost,
            completionCosts.LaborCost,
            completionCosts.MachineCost,
            completionCosts.OverheadCost);

        try
        {
            workOrder.Complete(request.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<ProductionReceiptDto>.Failure(ex.Message);
        }

        dbContext.ProductionReceipts.Add(receipt);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Manufacturing", "ProductionReceiptCompleted", actor, $"{receipt.ReceiptNo}:{workOrder.WorkOrderNo}", cancellationToken);
        return OperationResult<ProductionReceiptDto>.Success(MapProductionReceipt(receipt));
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
            unitCost: unitCost,
            costAmount: costAmount,
            balanceCostAfter: balanceCostAfter));
    }

    /// <summary>
    /// Build Work Order Cost State Async。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<WorkOrderCostState> BuildWorkOrderCostStateAsync(Guid workOrderId, CancellationToken cancellationToken)
    {
        var issues = await dbContext.ProductionIssues
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.WorkOrderId == workOrderId)
            .ToListAsync(cancellationToken);
        var receipts = await dbContext.ProductionReceipts
            .AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId)
            .ToListAsync(cancellationToken);

        var materialCost = issues.SelectMany(x => x.Lines).Sum(x => x.CostAmount);
        var snapshot = await dbContext.ManufacturingCostSnapshots
            .AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var laborCost = 0m;
        var machineCost = 0m;
        var overheadCost = 0m;
        var costSource = "材料成本";
        if (snapshot is not null)
        {
            laborCost = snapshot.LaborCost;
            machineCost = snapshot.MachineCost;
            overheadCost = snapshot.OverheadCost;
            costSource = "成本快照";
        }
        else
        {
            var operationCosts = await CalculateOperationCostsAsync(workOrderId, cancellationToken);
            laborCost = operationCosts.LaborCost;
            machineCost = operationCosts.MachineCost;
            overheadCost = operationCosts.OverheadCost;
            if (operationCosts.TotalCost > 0)
            {
                costSource = "工序实绩";
            }
        }

        var receivedCost = receipts.Sum(x => x.CostAmount);
        var receivedQuantity = receipts.Sum(x => x.Quantity);
        return new WorkOrderCostState(
            materialCost,
            laborCost,
            machineCost,
            overheadCost,
            receipts.Sum(x => x.MaterialCost),
            receipts.Sum(x => x.LaborCost),
            receipts.Sum(x => x.MachineCost),
            receipts.Sum(x => x.OverheadCost),
            receivedCost,
            receivedQuantity,
            snapshot?.TotalCost ?? 0m,
            costSource);
    }

    /// <summary>
    /// Calculate Operation Costs Async。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<OperationCostState> CalculateOperationCostsAsync(Guid workOrderId, CancellationToken cancellationToken)
    {
        var schedules = await dbContext.OperationSchedules
            .AsNoTracking()
            .Where(x => x.WorkOrderId == workOrderId && x.Status == AdvancedManufacturingStatus.Completed && x.CompletedQuantity > 0)
            .ToListAsync(cancellationToken);
        if (schedules.Count == 0)
        {
            return new OperationCostState(0m, 0m, 0m);
        }

        var operationIds = schedules.Select(x => x.RoutingOperationId).Distinct().ToList();
        var workCenterIds = schedules.Select(x => x.WorkCenterId).Distinct().ToList();
        var operations = await dbContext.RoutingOperations
            .AsNoTracking()
            .Where(x => operationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var workCenters = await dbContext.WorkCenters
            .AsNoTracking()
            .Where(x => workCenterIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var laborCost = 0m;
        var machineCost = 0m;
        var overheadCost = 0m;
        foreach (var schedule in schedules)
        {
            if (!operations.TryGetValue(schedule.RoutingOperationId, out var operation))
            {
                continue;
            }

            var hours = schedule.CompletedQuantity * operation.StandardMinutes / 60m;
            laborCost += hours * operation.LaborCostRate;
            machineCost += hours * operation.MachineCostRate;
            if (workCenters.TryGetValue(schedule.WorkCenterId, out var workCenter))
            {
                overheadCost += hours * workCenter.HourlyCostRate;
            }
        }

        return new OperationCostState(laborCost, machineCost, overheadCost);
    }

    /// <summary>
    /// Calculate Completion Costs。
    /// </summary>
    /// <param name="workOrder">work Order 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="costState">cost State 参数。</param>
    private static CompletionCostAllocation CalculateCompletionCosts(
        WorkOrder workOrder,
        decimal quantity,
        WorkOrderCostState costState)
    {
        var cumulativeRatio = workOrder.PlannedQuantity <= 0
            ? 1m
            : Math.Min(1m, (workOrder.CompletedQuantity + quantity) / workOrder.PlannedQuantity);
        var isFinalCompletion = workOrder.CompletedQuantity + quantity == workOrder.PlannedQuantity;

        return new CompletionCostAllocation(
            AllocateComponent(costState.MaterialCost, costState.ReceivedMaterialCost, cumulativeRatio, isFinalCompletion),
            AllocateComponent(costState.LaborCost, costState.ReceivedLaborCost, cumulativeRatio, isFinalCompletion),
            AllocateComponent(costState.MachineCost, costState.ReceivedMachineCost, cumulativeRatio, isFinalCompletion),
            AllocateComponent(costState.OverheadCost, costState.ReceivedOverheadCost, cumulativeRatio, isFinalCompletion));
    }

    /// <summary>
    /// Allocate Component。
    /// </summary>
    /// <param name="totalCost">total Cost 参数。</param>
    /// <param name="receivedCost">received Cost 参数。</param>
    /// <param name="cumulativeRatio">cumulative Ratio 参数。</param>
    /// <param name="isFinalCompletion">is Final Completion 参数。</param>
    private static decimal AllocateComponent(decimal totalCost, decimal receivedCost, decimal cumulativeRatio, bool isFinalCompletion)
    {
        var targetCost = isFinalCompletion ? totalCost : totalCost * cumulativeRatio;
        return Math.Max(0m, targetCost - receivedCost);
    }

    /// <summary>
    /// 注册Cost Summary 路由。
    /// </summary>
    /// <param name="workOrder">work Order 参数。</param>
    /// <param name="costState">cost State 参数。</param>
    private static WorkOrderCostSummaryDto MapCostSummary(WorkOrder workOrder, WorkOrderCostState costState)
    {
        var totalCost = costState.TotalCost;
        var remainingCost = Math.Max(0m, totalCost - costState.ReceivedCost);
        var unitCost = workOrder.PlannedQuantity <= 0 ? 0m : totalCost / workOrder.PlannedQuantity;
        return new WorkOrderCostSummaryDto(
            costState.MaterialCost,
            costState.LaborCost,
            costState.MachineCost,
            costState.OverheadCost,
            totalCost,
            costState.ReceivedCost,
            remainingCost,
            costState.ReceivedQuantity,
            unitCost,
            costState.SnapshotTotalCost,
            costState.SnapshotTotalCost == 0 ? 0m : totalCost - costState.SnapshotTotalCost,
            costState.CostSource);
    }

    /// <summary>
    /// Cost Amount。
    /// </summary>
    /// <param name="quantity">数量。</param>
    /// <param name="unitCost">单位成本。</param>
    private static decimal CostAmount(decimal quantity, decimal unitCost) => quantity * unitCost;

    /// <summary>
    /// Normalize Bom Lines。
    /// </summary>
    /// <param name="lines">明细行集合。</param>
    private static IReadOnlyList<CreateBomLineRequest> NormalizeBomLines(IReadOnlyList<CreateBomLineRequest> lines)
    {
        return lines
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.ComponentItemId)
            .Select(group => new CreateBomLineRequest(
                group.Key,
                group.Sum(x => x.Quantity)))
            .ToList();
    }

    /// <summary>
    /// 注册Bom 路由。
    /// </summary>
    /// <param name="bom">物料清单。</param>
    private static BomDto MapBom(BillOfMaterial bom) =>
        new(
            bom.Id,
            bom.BomNo,
            bom.FinishedItemId,
            bom.FinishedItemCode,
            bom.FinishedItemName,
            bom.Version,
            bom.BaseQuantity,
            bom.Unit,
            bom.IsEnabled,
            bom.Lines.Select(MapBomLine).ToList(),
            bom.UpdatedAtUtc);

    /// <summary>
    /// 注册Bom Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static BomLineDto MapBomLine(BillOfMaterialLine line) =>
        new(
            line.Id,
            line.ComponentItemId,
            line.ComponentItemCode,
            line.ComponentItemName,
            line.Quantity,
            line.Unit);

    /// <summary>
    /// 注册Work Order 路由。
    /// </summary>
    /// <param name="workOrder">work Order 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<WorkOrderDto> MapWorkOrderAsync(WorkOrder workOrder, CancellationToken cancellationToken)
    {
        var costState = await BuildWorkOrderCostStateAsync(workOrder.Id, cancellationToken);
        return MapWorkOrder(workOrder, MapCostSummary(workOrder, costState));
    }

    /// <summary>
    /// 注册Work Order 路由。
    /// </summary>
    /// <param name="workOrder">work Order 参数。</param>
    /// <param name="costSummary">cost Summary 参数。</param>
    private static WorkOrderDto MapWorkOrder(WorkOrder workOrder, WorkOrderCostSummaryDto costSummary) =>
        new(
            workOrder.Id,
            workOrder.WorkOrderNo,
            workOrder.BomId,
            workOrder.BomNo,
            workOrder.BomVersion,
            workOrder.FinishedItemId,
            workOrder.FinishedItemCode,
            workOrder.FinishedItemName,
            workOrder.PlannedQuantity,
            workOrder.CompletedQuantity,
            workOrder.Unit,
            workOrder.Status,
            workOrder.CreatedBy,
            workOrder.MaterialLines.Select(MapWorkOrderMaterialLine).ToList(),
            costSummary,
            workOrder.UpdatedAtUtc);

    /// <summary>
    /// 注册Work Order Material Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static WorkOrderMaterialLineDto MapWorkOrderMaterialLine(WorkOrderMaterialLine line) =>
        new(
            line.Id,
            line.ComponentItemId,
            line.ComponentItemCode,
            line.ComponentItemName,
            line.RequiredQuantity,
            line.IssuedQuantity,
            line.Unit);

    /// <summary>
    /// 注册Production Issue 路由。
    /// </summary>
    /// <param name="issue">出库单。</param>
    private static ProductionIssueDto MapProductionIssue(ProductionIssue issue) =>
        new(
            issue.Id,
            issue.IssueNo,
            issue.WorkOrderId,
            issue.WorkOrderNo,
            issue.WarehouseId,
            issue.WarehouseCode,
            issue.WarehouseName,
            issue.Status,
            issue.IssuedBy,
            issue.Lines.Select(MapProductionIssueLine).ToList(),
            issue.CreatedAtUtc);

    /// <summary>
    /// 注册Production Issue Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static ProductionIssueLineDto MapProductionIssueLine(ProductionIssueLine line) =>
        new(
            line.Id,
            line.ItemId,
            line.ItemCode,
            line.ItemName,
            line.Quantity,
            line.Unit,
            line.UnitCost,
            line.CostAmount);

    /// <summary>
    /// 注册Production Receipt 路由。
    /// </summary>
    /// <param name="receipt">入库单。</param>
    private static ProductionReceiptDto MapProductionReceipt(ProductionReceipt receipt) =>
        new(
            receipt.Id,
            receipt.ReceiptNo,
            receipt.WorkOrderId,
            receipt.WorkOrderNo,
            receipt.WarehouseId,
            receipt.WarehouseCode,
            receipt.WarehouseName,
            receipt.FinishedItemId,
            receipt.FinishedItemCode,
            receipt.FinishedItemName,
            receipt.Quantity,
            receipt.Unit,
            receipt.UnitCost,
            receipt.MaterialCost,
            receipt.LaborCost,
            receipt.MachineCost,
            receipt.OverheadCost,
            receipt.CostAmount,
            receipt.Status,
            receipt.ReceivedBy,
            receipt.CreatedAtUtc);

    /// <summary>
    /// Operation Cost State 数据记录。
    /// </summary>
    /// <param name="LaborCost">Labor Cost 参数。</param>
    /// <param name="MachineCost">Machine Cost 参数。</param>
    /// <param name="OverheadCost">Overhead Cost 参数。</param>
    private sealed record OperationCostState(decimal LaborCost, decimal MachineCost, decimal OverheadCost)
    {
        public decimal TotalCost => LaborCost + MachineCost + OverheadCost;
    }

    /// <summary>
    /// Work Order Cost State 数据记录。
    /// </summary>
    private sealed record WorkOrderCostState(
        decimal MaterialCost,
        decimal LaborCost,
        decimal MachineCost,
        decimal OverheadCost,
        decimal ReceivedMaterialCost,
        decimal ReceivedLaborCost,
        decimal ReceivedMachineCost,
        decimal ReceivedOverheadCost,
        decimal ReceivedCost,
        decimal ReceivedQuantity,
        decimal SnapshotTotalCost,
        string CostSource)
    {
        public decimal TotalCost => MaterialCost + LaborCost + MachineCost + OverheadCost;
    }

    /// <summary>
    /// Completion Cost Allocation 数据记录。
    /// </summary>
    private sealed record CompletionCostAllocation(
        decimal MaterialCost,
        decimal LaborCost,
        decimal MachineCost,
        decimal OverheadCost)
    {
        public decimal TotalCost => MaterialCost + LaborCost + MachineCost + OverheadCost;
    }
}
