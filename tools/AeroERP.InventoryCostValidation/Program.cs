using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Inventory.Contracts;
using AeroERP.Modules.Inventory.Services;
using AeroERP.Modules.AdvancedManufacturing.Contracts;
using AeroERP.Modules.Manufacturing.Contracts;
using AeroERP.Modules.MasterData.Contracts;
using AeroERP.Modules.MasterData.Services;
using AeroERP.Modules.Procurement.Domain;
using AeroERP.Modules.Sales.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Infrastructure.Services;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<AeroErpDbContext>()
    .UseInMemoryDatabase($"aeroerp-inventory-cost-validation-{Guid.NewGuid():N}")
    .Options;

await using var dbContext = new AeroErpDbContext(options);
var auditWriter = new NoOpAuditWriter();
var currentUser = new ValidationUserAccessor();
var masterData = new MasterDataService(dbContext, auditWriter, currentUser);
var inventory = new InventoryService(dbContext, auditWriter, currentUser);
var manufacturing = new ManufacturingService(dbContext, auditWriter, currentUser);
var advancedManufacturing = new AdvancedManufacturingService(dbContext, auditWriter, currentUser);
var cancellationToken = CancellationToken.None;

var supplier = await masterData.CreateSupplierAsync(
    new UpsertSupplierRequest("19A-SUP", "19A validation supplier", "Buyer", "10086", true, null, "CNY", "TAX-19A", "19A supplier"),
    cancellationToken);
var customer = await masterData.CreateCustomerAsync(
    new UpsertCustomerRequest("19A-CUS", "19A validation customer", "Seller", "10010", true, null, "CNY", "TAX-19A-C", "19A customer"),
    cancellationToken);
var warehouse = await masterData.CreateWarehouseAsync(
    new UpsertWarehouseRequest("19A-WH", "19A validation warehouse", "Validation", true, null),
    cancellationToken);
var componentItem = await masterData.CreateItemAsync(
    new UpsertItemRequest("19A-COMP", "19A validation component", "costed material", "PCS", true),
    cancellationToken);
var finishedItem = await masterData.CreateItemAsync(
    new UpsertItemRequest("19A-FG", "19A validation finished good", "finished good", "PCS", true),
    cancellationToken);

var firstOrder = await SeedReleasedProcurementOrderAsync("001", supplier, componentItem, 10m, cancellationToken);
await RequireAsync(inventory.ReceiveProcurementOrderAsync(
    new ReceiveProcurementOrderRequest(
        firstOrder.Id,
        warehouse.Id,
        null,
        [new InventoryCostInputRequest(componentItem.Id, 5m)]),
    cancellationToken));

var secondOrder = await SeedReleasedProcurementOrderAsync("002", supplier, componentItem, 10m, cancellationToken);
await RequireAsync(inventory.ReceiveProcurementOrderAsync(
    new ReceiveProcurementOrderRequest(
        secondOrder.Id,
        warehouse.Id,
        null,
        [new InventoryCostInputRequest(componentItem.Id, 15m)]),
    cancellationToken));

var weightedBalance = await GetBalanceAsync(inventory, warehouse.Id, componentItem.Id, cancellationToken);
AssertEqual(20m, weightedBalance.Quantity, "weighted balance quantity");
AssertEqual(10m, weightedBalance.UnitCost, "weighted balance unitCost");
AssertEqual(200m, weightedBalance.InventoryValue, "weighted balance inventoryValue");

var salesOrder = new SalesOrder(
    "SO-19A-001",
    Guid.NewGuid(),
    "SQ-19A-001",
    customer.Id,
    customer.Name,
    null,
    string.Empty,
    "CNY",
    "VAT invoice",
    0.13m,
    currentUser.DisplayName,
    [new SalesOrderLine(componentItem.Id, componentItem.Code, componentItem.Name, 4m, componentItem.Unit)]);
salesOrder.Confirm();
salesOrder.MarkReadyToShip();
dbContext.SalesOrders.Add(salesOrder);
await dbContext.SaveChangesAsync(cancellationToken);

var issue = await RequireAsync(inventory.IssueSalesOrderAsync(
    new IssueSalesOrderRequest(salesOrder.Id, warehouse.Id),
    cancellationToken));
var issueLine = issue.Lines.Single();
AssertEqual(10m, issueLine.UnitCost, "sales issue unitCost");
AssertEqual(40m, issueLine.CostAmount, "sales issue costAmount");

var afterSalesIssueBalance = await GetBalanceAsync(inventory, warehouse.Id, componentItem.Id, cancellationToken);
AssertEqual(16m, afterSalesIssueBalance.Quantity, "after sales issue quantity");
AssertEqual(10m, afterSalesIssueBalance.UnitCost, "after sales issue unitCost");
AssertEqual(160m, afterSalesIssueBalance.InventoryValue, "after sales issue inventoryValue");

var bom = await RequireAsync(manufacturing.CreateBomAsync(
    new CreateBomRequest(
        finishedItem.Id,
        "19A",
        1m,
        true,
        [new CreateBomLineRequest(componentItem.Id, 3m)]),
    cancellationToken));
var workOrder = await RequireAsync(manufacturing.CreateWorkOrderAsync(
    new CreateWorkOrderRequest(bom.Id, 1m),
    cancellationToken));
await RequireAsync(manufacturing.ReleaseWorkOrderAsync(workOrder.Id, cancellationToken));
var productionIssue = await RequireAsync(manufacturing.ExecuteProductionIssueAsync(
    workOrder.Id,
    new ExecuteProductionIssueRequest(warehouse.Id),
    cancellationToken));

var productionIssueLine = productionIssue.Lines.Single();
AssertEqual(3m, productionIssueLine.Quantity, "production issue quantity");
AssertEqual(10m, productionIssueLine.UnitCost, "production issue unitCost");
AssertEqual(30m, productionIssueLine.CostAmount, "production issue costAmount");

var costSnapshot = await RequireAsync(advancedManufacturing.CreateCostSnapshotAsync(
    new CreateCostSnapshotRequest(workOrder.Id, 30m, 12m, 8m, 5m),
    cancellationToken));
AssertEqual(55m, costSnapshot.TotalCost, "manufacturing cost snapshot totalCost");

var productionReceipt = await RequireAsync(manufacturing.CompleteProductionAsync(
    workOrder.Id,
    new CompleteProductionRequest(warehouse.Id, 1m),
    cancellationToken));
AssertEqual(55m, productionReceipt.CostAmount, "production receipt costAmount");
AssertEqual(55m, productionReceipt.UnitCost, "production receipt unitCost");
AssertEqual(30m, productionReceipt.MaterialCost, "production receipt materialCost");
AssertEqual(12m, productionReceipt.LaborCost, "production receipt laborCost");
AssertEqual(8m, productionReceipt.MachineCost, "production receipt machineCost");
AssertEqual(5m, productionReceipt.OverheadCost, "production receipt overheadCost");

var finalComponentBalance = await GetBalanceAsync(inventory, warehouse.Id, componentItem.Id, cancellationToken);
AssertEqual(13m, finalComponentBalance.Quantity, "final component balance quantity");
AssertEqual(10m, finalComponentBalance.UnitCost, "final component balance unitCost");
AssertEqual(130m, finalComponentBalance.InventoryValue, "final component balance inventoryValue");

var finishedBalance = await GetBalanceAsync(inventory, warehouse.Id, finishedItem.Id, cancellationToken);
AssertEqual(1m, finishedBalance.Quantity, "finished balance quantity");
AssertEqual(55m, finishedBalance.UnitCost, "finished balance unitCost");
AssertEqual(55m, finishedBalance.InventoryValue, "finished balance inventoryValue");

var costedWorkOrder = (await manufacturing.ListWorkOrdersAsync(cancellationToken)).Single(x => x.Id == workOrder.Id);
AssertEqual(30m, costedWorkOrder.CostSummary.MaterialCost, "work order materialCost");
AssertEqual(12m, costedWorkOrder.CostSummary.LaborCost, "work order laborCost");
AssertEqual(8m, costedWorkOrder.CostSummary.MachineCost, "work order machineCost");
AssertEqual(5m, costedWorkOrder.CostSummary.OverheadCost, "work order overheadCost");
AssertEqual(55m, costedWorkOrder.CostSummary.TotalCost, "work order totalCost");
AssertEqual(55m, costedWorkOrder.CostSummary.ReceivedCost, "work order receivedCost");
AssertEqual(0m, costedWorkOrder.CostSummary.RemainingCost, "work order remainingCost");

var ledger = await inventory.ListInventoryLedgerAsync(warehouse.Id, componentItem.Id, cancellationToken);
AssertEqual(4, ledger.Count, "ledger entry count");
AssertEqual(20m, ledger.Sum(x => x.InQuantity), "ledger inQuantity");
AssertEqual(7m, ledger.Sum(x => x.OutQuantity), "ledger outQuantity");
AssertEqual(200m, ledger.Sum(x => x.InAmount), "ledger inAmount");
AssertEqual(70m, ledger.Sum(x => x.OutAmount), "ledger outAmount");

var salesIssueLedger = ledger.Single(x => x.DocumentType == "InventoryIssue");
AssertEqual(4m, salesIssueLedger.OutQuantity, "sales issue ledger outQuantity");
AssertEqual(40m, salesIssueLedger.OutAmount, "sales issue ledger outAmount");
AssertEqual(160m, salesIssueLedger.BalanceCostAfter, "sales issue ledger balanceCostAfter");

var productionIssueLedger = ledger.Single(x => x.DocumentType == "ProductionIssue");
AssertEqual(3m, productionIssueLedger.OutQuantity, "production issue ledger outQuantity");
AssertEqual(30m, productionIssueLedger.OutAmount, "production issue ledger outAmount");
AssertEqual(130m, productionIssueLedger.BalanceCostAfter, "production issue ledger balanceCostAfter");

var finishedLedger = await inventory.ListInventoryLedgerAsync(warehouse.Id, finishedItem.Id, cancellationToken);
var productionReceiptLedger = finishedLedger.Single(x => x.DocumentType == "ProductionReceipt");
AssertEqual(1m, productionReceiptLedger.InQuantity, "production receipt ledger inQuantity");
AssertEqual(55m, productionReceiptLedger.InAmount, "production receipt ledger inAmount");
AssertEqual(55m, productionReceiptLedger.BalanceCostAfter, "production receipt ledger balanceCostAfter");

Console.WriteLine("Inventory cost validation passed.");
Console.WriteLine($"Moving weighted balance: {weightedBalance.Quantity} @ {weightedBalance.UnitCost} = {weightedBalance.InventoryValue}");
Console.WriteLine($"Sales issue cost: {issueLine.Quantity} @ {issueLine.UnitCost} = {issueLine.CostAmount}");
Console.WriteLine($"Production issue cost: {productionIssueLine.Quantity} @ {productionIssueLine.UnitCost} = {productionIssueLine.CostAmount}");
Console.WriteLine($"Production receipt cost: {productionReceipt.Quantity} @ {productionReceipt.UnitCost} = {productionReceipt.CostAmount}");
Console.WriteLine($"Ledger totals: component in {ledger.Sum(x => x.InAmount)}, component out {ledger.Sum(x => x.OutAmount)}, finished in {finishedLedger.Sum(x => x.InAmount)}");

async Task<ProcurementOrder> SeedReleasedProcurementOrderAsync(
    string suffix,
    SupplierDto supplierDto,
    ItemDto item,
    decimal quantity,
    CancellationToken ct)
{
    var request = new ProcurementRequest(
        $"PR-19A-{suffix}",
        supplierDto.Id,
        supplierDto.Name,
        $"19A validation request {suffix}",
        null,
        string.Empty,
        "CNY",
        "VAT invoice",
        0.13m,
        [new ProcurementRequestLine(item.Id, item.Name, quantity, item.Unit)]);
    var order = new ProcurementOrder(
        $"PO-19A-{suffix}",
        request.Id,
        request.RequestNo,
        supplierDto.Id,
        supplierDto.Name,
        currentUser.DisplayName);
    order.Release();

    dbContext.ProcurementRequests.Add(request);
    dbContext.ProcurementOrders.Add(order);
    await dbContext.SaveChangesAsync(ct);
    return order;
}

static async Task<T> RequireAsync<T>(Task<OperationResult<T>> task)
{
    var result = await task;
    if (!result.IsSuccess || result.Value is null)
    {
        throw new InvalidOperationException(result.Error ?? "Operation failed.");
    }

    return result.Value;
}

static async Task<StockBalanceDto> GetBalanceAsync(
    InventoryService inventory,
    Guid warehouseId,
    Guid itemId,
    CancellationToken cancellationToken)
{
    var balances = await inventory.ListStockBalancesAsync(cancellationToken);
    return balances.Single(x => x.WarehouseId == warehouseId && x.ItemId == itemId);
}

static void AssertEqual<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected {name} to be {expected}, got {actual}.");
    }
}

sealed class NoOpAuditWriter : IAuditWriter
{
    /// <summary>
    /// Write Async。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="detail">详细说明。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public Task WriteAsync(string category, string action, string actor, string detail, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

sealed class ValidationUserAccessor : ICurrentUserAccessor
{
    public bool IsAuthenticated => true;
    /// <summary>
    /// User Id。
    /// </summary>
    public Guid? UserId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public string UserName => "inventory-cost-validation";
    public string DisplayName => "Inventory Cost Validation";
    public IReadOnlyList<string> Roles => ["platform-admin"];
    public IReadOnlyList<string> Permissions =>
    [
        "inventory.read",
        "inventory.receipt.manage",
        "inventory.issue.manage",
        "manufacturing.read",
        "manufacturing.workorder.manage",
        "manufacturing.execution.manage",
        "advanced-manufacturing.read",
        "advanced-manufacturing.cost.manage",
        "master-data.read",
        "master-data.manage"
    ];

    /// <summary>
    /// 判断是否存在Role。
    /// </summary>
    /// <param name="roleKey">role Key 参数。</param>
    public bool HasRole(string roleKey) => Roles.Contains(roleKey);
    /// <summary>
    /// 判断是否存在Permission。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    public bool HasPermission(string permission) => Permissions.Contains(permission);
    /// <summary>
    /// 判断是否允许Access Module。
    /// </summary>
    /// <param name="moduleKey">模块键。</param>
    public bool CanAccessModule(string moduleKey) => moduleKey is "inventory" or "manufacturing" or "advanced-manufacturing" or "master-data";
}
