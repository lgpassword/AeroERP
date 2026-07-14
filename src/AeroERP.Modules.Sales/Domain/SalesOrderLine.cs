namespace AeroERP.Modules.Sales.Domain;

/// <summary>
/// Sales Order 明细行实体。
/// </summary>
public sealed class SalesOrderLine
{
    /// <summary>
    /// 初始化Sales Order Line实例。
    /// </summary>
    private SalesOrderLine()
    {
    }

    /// <summary>
    /// 初始化Sales Order Line实例。
    /// </summary>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    public SalesOrderLine(Guid itemId, string itemCode, string itemName, decimal quantity, string unit)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        Quantity = quantity;
        Unit = unit;
    }

    /// <summary>
    /// 主键标识。
    /// </summary>
    public Guid Id { get; private set; }
    /// <summary>
    /// Sales Order Id。
    /// </summary>
    public Guid SalesOrderId { get; private set; }
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
}
