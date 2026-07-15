using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Planning.Domain;

/// <summary>
/// Outsourcing Order 明细行实体。
/// </summary>
public sealed class OutsourcingOrderLine : Entity
{
    /// <summary>
    /// 初始化Outsourcing Order Line实例。
    /// </summary>
    private OutsourcingOrderLine()
    {
    }

    /// <summary>
    /// 初始化Outsourcing Order Line实例。
    /// </summary>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    public OutsourcingOrderLine(Guid itemId, string itemCode, string itemName, decimal quantity, string unit)
    {
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        Quantity = quantity;
        Unit = unit;
    }

    /// <summary>
    /// Outsourcing Order Id。
    /// </summary>
    public Guid OutsourcingOrderId { get; private set; }
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
