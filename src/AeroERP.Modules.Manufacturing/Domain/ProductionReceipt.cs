using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Manufacturing.Domain;

/// <summary>
/// Production Receipt 业务对象。
/// </summary>
public sealed class ProductionReceipt : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Production Receipt实例。
    /// </summary>
    private ProductionReceipt()
    {
    }

    /// <summary>
    /// 初始化Production Receipt实例。
    /// </summary>
    /// <param name="receiptNo">receipt No 参数。</param>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="workOrderNo">work Order No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="finishedItemId">finished Item Id 参数。</param>
    /// <param name="finishedItemCode">finished Item Code 参数。</param>
    /// <param name="finishedItemName">finished Item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="receivedBy">received By 参数。</param>
    /// <param name="unitCost">单位成本。</param>
    /// <param name="materialCost">material Cost 参数。</param>
    /// <param name="laborCost">labor Cost 参数。</param>
    /// <param name="machineCost">machine Cost 参数。</param>
    /// <param name="overheadCost">overhead Cost 参数。</param>
    public ProductionReceipt(
        string receiptNo,
        Guid workOrderId,
        string workOrderNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid finishedItemId,
        string finishedItemCode,
        string finishedItemName,
        decimal quantity,
        string unit,
        string receivedBy,
        decimal unitCost = 0m,
        decimal materialCost = 0m,
        decimal laborCost = 0m,
        decimal machineCost = 0m,
        decimal overheadCost = 0m)
    {
        if (unitCost < 0 || materialCost < 0 || laborCost < 0 || machineCost < 0 || overheadCost < 0)
        {
            throw new InvalidOperationException("完工成本不能为负数。");
        }

        ReceiptNo = receiptNo;
        WorkOrderId = workOrderId;
        WorkOrderNo = workOrderNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        FinishedItemId = finishedItemId;
        FinishedItemCode = finishedItemCode;
        FinishedItemName = finishedItemName;
        Quantity = quantity;
        Unit = unit;
        ReceivedBy = receivedBy;
        UnitCost = unitCost;
        MaterialCost = materialCost;
        LaborCost = laborCost;
        MachineCost = machineCost;
        OverheadCost = overheadCost;
        CostAmount = materialCost + laborCost + machineCost + overheadCost;
    }

    /// <summary>
    /// Receipt No。
    /// </summary>
    public string ReceiptNo { get; private set; } = string.Empty;
    /// <summary>
    /// Work Order Id。
    /// </summary>
    public Guid WorkOrderId { get; private set; }
    /// <summary>
    /// Work Order No。
    /// </summary>
    public string WorkOrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Warehouse Id。
    /// </summary>
    public Guid WarehouseId { get; private set; }
    /// <summary>
    /// Warehouse Code。
    /// </summary>
    public string WarehouseCode { get; private set; } = string.Empty;
    /// <summary>
    /// Warehouse Name。
    /// </summary>
    public string WarehouseName { get; private set; } = string.Empty;
    /// <summary>
    /// Finished Item Id。
    /// </summary>
    public Guid FinishedItemId { get; private set; }
    /// <summary>
    /// Finished Item Code。
    /// </summary>
    public string FinishedItemCode { get; private set; } = string.Empty;
    /// <summary>
    /// Finished Item Name。
    /// </summary>
    public string FinishedItemName { get; private set; } = string.Empty;
    /// <summary>
    /// 数量。
    /// </summary>
    public decimal Quantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// 单位成本。
    /// </summary>
    public decimal UnitCost { get; private set; }
    /// <summary>
    /// Material Cost。
    /// </summary>
    public decimal MaterialCost { get; private set; }
    /// <summary>
    /// Labor Cost。
    /// </summary>
    public decimal LaborCost { get; private set; }
    /// <summary>
    /// Machine Cost。
    /// </summary>
    public decimal MachineCost { get; private set; }
    /// <summary>
    /// Overhead Cost。
    /// </summary>
    public decimal OverheadCost { get; private set; }
    /// <summary>
    /// Cost Amount。
    /// </summary>
    public decimal CostAmount { get; private set; }
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = ProductionDocumentStatus.Completed;
    /// <summary>
    /// Received By。
    /// </summary>
    public string ReceivedBy { get; private set; } = string.Empty;
}
