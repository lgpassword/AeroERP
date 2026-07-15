namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Receipt 明细行实体。
/// </summary>
public sealed class InventoryReceiptLine
{
    /// <summary>
    /// 初始化Inventory Receipt Line实例。
    /// </summary>
    private InventoryReceiptLine()
    {
    }

    /// <summary>
    /// 初始化Inventory Receipt Line实例。
    /// </summary>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="unitCost">单位成本。</param>
    /// <param name="costAmount">成本金额。</param>
    public InventoryReceiptLine(
        Guid itemId,
        string itemCode,
        string itemName,
        decimal quantity,
        string unit,
        decimal unitCost = 0m,
        decimal costAmount = 0m)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        Quantity = quantity;
        Unit = unit;
        UnitCost = unitCost;
        CostAmount = costAmount;
    }

    /// <summary>
    /// 主键标识。
    /// </summary>
    public Guid Id { get; private set; }
    /// <summary>
    /// Inventory Receipt Id。
    /// </summary>
    public Guid InventoryReceiptId { get; private set; }
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
    /// Cost Amount。
    /// </summary>
    public decimal CostAmount { get; private set; }
}
