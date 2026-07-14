namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Count Adjustment 明细行实体。
/// </summary>
public sealed class InventoryCountAdjustmentLine
{
    /// <summary>
    /// 初始化Inventory Count Adjustment Line实例。
    /// </summary>
    private InventoryCountAdjustmentLine()
    {
    }

    /// <summary>
    /// 初始化Inventory Count Adjustment Line实例。
    /// </summary>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="beforeQuantity">before Quantity 参数。</param>
    /// <param name="countedQuantity">counted Quantity 参数。</param>
    /// <param name="deltaQuantity">delta Quantity 参数。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="unitCost">单位成本。</param>
    /// <param name="costAmount">成本金额。</param>
    public InventoryCountAdjustmentLine(
        Guid itemId,
        string itemCode,
        string itemName,
        decimal beforeQuantity,
        decimal countedQuantity,
        decimal deltaQuantity,
        string unit,
        decimal unitCost = 0m,
        decimal costAmount = 0m)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        BeforeQuantity = beforeQuantity;
        CountedQuantity = countedQuantity;
        DeltaQuantity = deltaQuantity;
        Unit = unit;
        UnitCost = unitCost;
        CostAmount = costAmount;
    }

    /// <summary>
    /// 主键标识。
    /// </summary>
    public Guid Id { get; private set; }
    /// <summary>
    /// Inventory Count Adjustment Id。
    /// </summary>
    public Guid InventoryCountAdjustmentId { get; private set; }
    /// <summary>
    /// Item Id。
    /// </summary>
    public Guid ItemId { get; private set; }
    /// <summary>
    /// Item Code。
    /// </summary>
    public string ItemCode { get; private set; } = string.Empty;
    /// <summary>
    /// Item Name。
    /// </summary>
    public string ItemName { get; private set; } = string.Empty;
    /// <summary>
    /// Before Quantity。
    /// </summary>
    public decimal BeforeQuantity { get; private set; }
    /// <summary>
    /// Counted Quantity。
    /// </summary>
    public decimal CountedQuantity { get; private set; }
    /// <summary>
    /// Delta Quantity。
    /// </summary>
    public decimal DeltaQuantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// 单位成本。
    /// </summary>
    public decimal UnitCost { get; private set; }
    /// <summary>
    /// Cost Amount。
    /// </summary>
    public decimal CostAmount { get; private set; }
}
