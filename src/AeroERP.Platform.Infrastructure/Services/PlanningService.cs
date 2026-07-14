using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.MasterData.Domain;
using AeroERP.Modules.Planning.Contracts;
using AeroERP.Modules.Planning.Domain;
using AeroERP.Modules.Planning.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Planning Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class PlanningService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IPlanningService
{
    /// <summary>
    /// 查询Suggestions。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<PlanningSuggestionDto>> ListSuggestionsAsync(CancellationToken cancellationToken)
    {
        var suggestions = await dbContext.PlanningSuggestions.ToListAsync(cancellationToken);
        return suggestions
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(MapSuggestion)
            .ToList();
    }

    /// <summary>
    /// Generate Suggestion Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PlanningSuggestionDto>> GenerateSuggestionAsync(
        GeneratePlanningSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MinimumQuantity <= 0)
        {
            return OperationResult<PlanningSuggestionDto>.Failure("最低库存必须大于零。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<PlanningSuggestionDto>.Failure("未找到可用仓库。");
        }

        var item = await dbContext.Items.FirstOrDefaultAsync(x => x.Id == request.ItemId && x.IsEnabled, cancellationToken);
        if (item is null)
        {
            return OperationResult<PlanningSuggestionDto>.Failure("未找到可用物料。");
        }

        var balance = await FindStockBalanceAsync(warehouse.Id, item.Id, cancellationToken);
        var currentQuantity = balance?.Quantity ?? 0m;
        if (currentQuantity >= request.MinimumQuantity)
        {
            return OperationResult<PlanningSuggestionDto>.Failure("当前库存未低于最低库存，无需生成补货建议。");
        }

        var existingOpen = await dbContext.PlanningSuggestions.AnyAsync(
            x => x.WarehouseId == warehouse.Id && x.ItemId == item.Id && x.Status == PlanningSuggestionStatus.Open,
            cancellationToken);
        if (existingOpen)
        {
            return OperationResult<PlanningSuggestionDto>.Failure("该仓库物料已存在待处理计划建议。");
        }

        var actor = currentUser.GetActor();
        var suggestion = new PlanningSuggestion(
            $"PS-{DateTime.UtcNow:yyyyMMddHHmmss}",
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            item.Id,
            item.Code,
            item.Name,
            currentQuantity,
            request.MinimumQuantity,
            request.MinimumQuantity - currentQuantity,
            item.Unit,
            actor);

        dbContext.PlanningSuggestions.Add(suggestion);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Planning", "SuggestionGenerated", actor, $"{suggestion.SuggestionNo}:{suggestion.ItemCode}", cancellationToken);
        return OperationResult<PlanningSuggestionDto>.Success(MapSuggestion(suggestion));
    }

    /// <summary>
    /// Decide Suggestion Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PlanningSuggestionDto>> DecideSuggestionAsync(
        Guid id,
        PlanningSuggestionDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var suggestion = await dbContext.PlanningSuggestions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (suggestion is null)
        {
            return OperationResult<PlanningSuggestionDto>.Failure("未找到计划建议。");
        }

        var decision = request.Decision.Trim();
        if (!string.Equals(decision, PlanningSuggestionStatus.Accepted, StringComparison.Ordinal) &&
            !string.Equals(decision, PlanningSuggestionStatus.Ignored, StringComparison.Ordinal))
        {
            return OperationResult<PlanningSuggestionDto>.Failure("计划建议决策只能是采纳或忽略。");
        }

        try
        {
            suggestion.Decide(decision, request.Note.Trim());
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<PlanningSuggestionDto>.Failure(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Planning", "SuggestionDecided", currentUser.GetActor(), $"{suggestion.SuggestionNo}:{suggestion.Status}", cancellationToken);
        return OperationResult<PlanningSuggestionDto>.Success(MapSuggestion(suggestion));
    }

    /// <summary>
    /// 查询Outsourcing Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<OutsourcingOrderDto>> ListOutsourcingOrdersAsync(CancellationToken cancellationToken)
    {
        var orders = await dbContext.OutsourcingOrders
            .Include(x => x.MaterialLines)
            .ToListAsync(cancellationToken);

        return orders
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(MapOutsourcingOrder)
            .ToList();
    }

    /// <summary>
    /// 创建Outsourcing Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<OutsourcingOrderDto>> CreateOutsourcingOrderAsync(
        CreateOutsourcingOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SupplierName))
        {
            return OperationResult<OutsourcingOrderDto>.Failure("外协供应商不能为空。");
        }

        if (request.PlannedQuantity <= 0)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("外协计划数量必须大于零。");
        }

        if (request.MaterialLines.Count == 0)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("外协单至少需要一条发料物料。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("未找到可用外协仓库。");
        }

        var finishedItem = await dbContext.Items.FirstOrDefaultAsync(x => x.Id == request.FinishedItemId && x.IsEnabled, cancellationToken);
        if (finishedItem is null)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("未找到可用外协成品物料。");
        }

        var normalizedLines = request.MaterialLines
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.ItemId)
            .Select(group => new CreateOutsourcingOrderLineRequest(group.Key, group.Sum(x => x.Quantity)))
            .ToList();
        if (normalizedLines.Count == 0)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("外协发料数量必须大于零。");
        }

        var itemIds = normalizedLines.Select(x => x.ItemId).Distinct().ToList();
        var items = await dbContext.Items
            .Where(x => itemIds.Contains(x.Id) && x.IsEnabled)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (items.Count != itemIds.Count)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("存在不存在或已停用的外协发料物料。");
        }

        var actor = currentUser.GetActor();
        var order = new OutsourcingOrder(
            $"OS-{DateTime.UtcNow:yyyyMMddHHmmss}",
            request.SupplierName.Trim(),
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            finishedItem.Id,
            finishedItem.Code,
            finishedItem.Name,
            request.PlannedQuantity,
            finishedItem.Unit,
            actor,
            normalizedLines.Select(line =>
            {
                var item = items[line.ItemId];
                return new OutsourcingOrderLine(item.Id, item.Code, item.Name, line.Quantity, item.Unit);
            }));

        dbContext.OutsourcingOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Planning", "OutsourcingOrderCreated", actor, order.OrderNo, cancellationToken);
        return OperationResult<OutsourcingOrderDto>.Success(MapOutsourcingOrder(order));
    }

    /// <summary>
    /// Issue Outsourcing Materials Async。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<OutsourcingOrderDto>> IssueOutsourcingMaterialsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.OutsourcingOrders
            .Include(x => x.MaterialLines)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("未找到外协单。");
        }

        var warehouse = await GetEnabledWarehouseAsync(order.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("外协仓库不可用。");
        }

        foreach (var line in order.MaterialLines)
        {
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ItemId, cancellationToken);
            if (balance is null || balance.Quantity < line.Quantity)
            {
                return OperationResult<OutsourcingOrderDto>.Failure($"物料 {line.ItemCode} 库存不足，无法外协发料。");
            }
        }

        try
        {
            order.MarkMaterialsIssued();
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<OutsourcingOrderDto>.Failure(ex.Message);
        }

        var actor = currentUser.GetActor();
        foreach (var line in order.MaterialLines)
        {
            var balance = await FindStockBalanceAsync(warehouse.Id, line.ItemId, cancellationToken);
            var unitCost = balance!.UnitCost;
            var costAmount = balance.Decrease(line.Quantity, unitCost);
            AddMovement(
                "OutsourcingOrder",
                order.OrderNo,
                InventoryMovementType.Issue,
                warehouse,
                line.ItemId,
                line.ItemCode,
                line.ItemName,
                -line.Quantity,
                balance.Quantity,
                line.Unit,
                actor,
                unitCost,
                -costAmount,
                balance.InventoryValue);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Planning", "OutsourcingMaterialsIssued", actor, order.OrderNo, cancellationToken);
        return OperationResult<OutsourcingOrderDto>.Success(MapOutsourcingOrder(order));
    }

    /// <summary>
    /// Receive Outsourcing Order Async。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<OutsourcingOrderDto>> ReceiveOutsourcingOrderAsync(
        Guid orderId,
        ReceiveOutsourcingOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("外协收料数量必须大于零。");
        }

        var order = await dbContext.OutsourcingOrders
            .Include(x => x.MaterialLines)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("未找到外协单。");
        }

        var warehouse = await GetEnabledWarehouseAsync(order.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<OutsourcingOrderDto>.Failure("外协仓库不可用。");
        }

        try
        {
            order.Receive(request.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<OutsourcingOrderDto>.Failure(ex.Message);
        }

        var actor = currentUser.GetActor();
        var unitCost = 0m;
        var costAmount = 0m;
        var balance = await FindStockBalanceAsync(warehouse.Id, order.FinishedItemId, cancellationToken);
        if (balance is null)
        {
            balance = new StockBalance(
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                order.FinishedItemId,
                order.FinishedItemCode,
                order.FinishedItemName,
                request.Quantity,
                order.Unit,
                unitCost);
            dbContext.StockBalances.Add(balance);
        }
        else
        {
            balance.Increase(request.Quantity, unitCost);
        }

        AddMovement(
            "OutsourcingOrder",
            order.OrderNo,
            InventoryMovementType.Receipt,
            warehouse,
            order.FinishedItemId,
            order.FinishedItemCode,
            order.FinishedItemName,
            request.Quantity,
            balance.Quantity,
            order.Unit,
            actor,
            unitCost,
            costAmount,
            balance.InventoryValue);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Planning", "OutsourcingReceived", actor, $"{order.OrderNo}:{request.Quantity}", cancellationToken);
        return OperationResult<OutsourcingOrderDto>.Success(MapOutsourcingOrder(order));
    }

    /// <summary>
    /// 查询Barcode Executions。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<BarcodeExecutionDto>> ListBarcodeExecutionsAsync(CancellationToken cancellationToken)
    {
        var executions = await dbContext.BarcodeExecutions.ToListAsync(cancellationToken);
        return executions
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapBarcodeExecution)
            .ToList();
    }

    /// <summary>
    /// Execute Barcode Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<BarcodeExecutionDto>> ExecuteBarcodeAsync(
        BarcodeExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var barcode = request.Barcode.Trim();
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return OperationResult<BarcodeExecutionDto>.Failure("条码不能为空。");
        }

        var action = request.Action.Trim();
        if (string.IsNullOrWhiteSpace(action))
        {
            return OperationResult<BarcodeExecutionDto>.Failure("扫码动作不能为空。");
        }

        var actor = currentUser.GetActor();
        var documentType = "Barcode";
        Guid? documentId = request.DocumentId;
        var documentNo = barcode;
        var result = "Success";
        var message = request.Note.Trim();

        if (string.Equals(action, "OutsourcingIssue", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(action, "OutsourcingReceive", StringComparison.OrdinalIgnoreCase))
        {
            var order = await ResolveOutsourcingOrderAsync(barcode, request.DocumentId, cancellationToken);
            if (order is null)
            {
                result = "Failed";
                message = "未找到条码关联的外协单。";
            }
            else
            {
                documentType = "OutsourcingOrder";
                documentId = order.Id;
                documentNo = order.OrderNo;

                var operation = string.Equals(action, "OutsourcingIssue", StringComparison.OrdinalIgnoreCase)
                    ? await IssueOutsourcingMaterialsAsync(order.Id, cancellationToken)
                    : await ReceiveOutsourcingOrderAsync(order.Id, new ReceiveOutsourcingOrderRequest(order.PlannedQuantity - order.ReceivedQuantity), cancellationToken);

                result = operation.IsSuccess ? "Success" : "Failed";
                message = operation.IsSuccess ? "扫码执行外协动作成功。" : operation.Error ?? "扫码执行外协动作失败。";
            }
        }
        else if (string.Equals(action, "StockLookup", StringComparison.OrdinalIgnoreCase))
        {
            var balance = await dbContext.StockBalances.FirstOrDefaultAsync(
                x => x.ItemCode == barcode || x.ItemId.ToString() == barcode,
                cancellationToken);
            result = balance is null ? "Failed" : "Success";
            documentType = "StockBalance";
            documentId = balance?.Id;
            documentNo = balance is null ? barcode : $"{balance.WarehouseCode}:{balance.ItemCode}";
            message = balance is null
                ? "未找到条码关联库存。"
                : $"当前库存 {balance.Quantity} {balance.Unit}。";
        }
        else
        {
            result = "Failed";
            message = "不支持的扫码动作。";
        }

        var execution = new BarcodeExecution(
            $"BC-{DateTime.UtcNow:yyyyMMddHHmmss}",
            barcode,
            action,
            result,
            message,
            documentType,
            documentId,
            documentNo,
            actor);
        dbContext.BarcodeExecutions.Add(execution);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Planning", "BarcodeExecuted", actor, $"{execution.ExecutionNo}:{execution.Result}", cancellationToken);
        return OperationResult<BarcodeExecutionDto>.Success(MapBarcodeExecution(execution));
    }

    /// <summary>
    /// Resolve Outsourcing Order Async。
    /// </summary>
    /// <param name="barcode">条码内容。</param>
    /// <param name="documentId">业务单据标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<OutsourcingOrder?> ResolveOutsourcingOrderAsync(string barcode, Guid? documentId, CancellationToken cancellationToken)
    {
        if (documentId.HasValue)
        {
            return await dbContext.OutsourcingOrders
                .Include(x => x.MaterialLines)
                .FirstOrDefaultAsync(x => x.Id == documentId.Value, cancellationToken);
        }

        return await dbContext.OutsourcingOrders
            .Include(x => x.MaterialLines)
            .FirstOrDefaultAsync(x => x.OrderNo == barcode, cancellationToken);
    }

    /// <summary>
    /// 获取Enabled Warehouse。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<Warehouse?> GetEnabledWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        return await dbContext.Warehouses.FirstOrDefaultAsync(x => x.Id == warehouseId && x.IsEnabled, cancellationToken);
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
            ?? await dbContext.StockBalances.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ItemId == itemId, cancellationToken);
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
    /// 注册Suggestion 路由。
    /// </summary>
    /// <param name="suggestion">计划建议。</param>
    private static PlanningSuggestionDto MapSuggestion(PlanningSuggestion suggestion) =>
        new(
            suggestion.Id,
            suggestion.SuggestionNo,
            suggestion.WarehouseId,
            suggestion.WarehouseCode,
            suggestion.WarehouseName,
            suggestion.ItemId,
            suggestion.ItemCode,
            suggestion.ItemName,
            suggestion.CurrentQuantity,
            suggestion.MinimumQuantity,
            suggestion.SuggestedQuantity,
            suggestion.Unit,
            suggestion.Status,
            suggestion.CreatedBy,
            suggestion.CreatedAtUtc,
            suggestion.UpdatedAtUtc);

    /// <summary>
    /// 注册Outsourcing Order 路由。
    /// </summary>
    /// <param name="order">业务订单。</param>
    private static OutsourcingOrderDto MapOutsourcingOrder(OutsourcingOrder order) =>
        new(
            order.Id,
            order.OrderNo,
            order.SupplierName,
            order.WarehouseId,
            order.WarehouseCode,
            order.WarehouseName,
            order.FinishedItemId,
            order.FinishedItemCode,
            order.FinishedItemName,
            order.PlannedQuantity,
            order.ReceivedQuantity,
            order.Unit,
            order.Status,
            order.CreatedBy,
            order.MaterialLines.Select(MapOutsourcingOrderLine).ToList(),
            order.CreatedAtUtc,
            order.UpdatedAtUtc);

    /// <summary>
    /// 注册Outsourcing Order Line 路由。
    /// </summary>
    /// <param name="line">明细行。</param>
    private static OutsourcingOrderLineDto MapOutsourcingOrderLine(OutsourcingOrderLine line) =>
        new(line.Id, line.ItemId, line.ItemCode, line.ItemName, line.Quantity, line.Unit);

    /// <summary>
    /// 注册Barcode Execution 路由。
    /// </summary>
    /// <param name="execution">执行记录。</param>
    private static BarcodeExecutionDto MapBarcodeExecution(BarcodeExecution execution) =>
        new(
            execution.Id,
            execution.ExecutionNo,
            execution.Barcode,
            execution.Action,
            execution.Result,
            execution.Message,
            execution.DocumentType,
            execution.DocumentId,
            execution.DocumentNo,
            execution.Actor,
            execution.CreatedAtUtc);
}
