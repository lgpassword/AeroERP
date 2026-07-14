using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Manufacturing.Domain;

/// <summary>
/// Bill Of Material 明细行实体。
/// </summary>
public sealed class BillOfMaterialLine : Entity
{
    /// <summary>
    /// 初始化Bill Of Material Line实例。
    /// </summary>
    private BillOfMaterialLine()
    {
    }

    /// <summary>
    /// 初始化Bill Of Material Line实例。
    /// </summary>
    /// <param name="componentItemId">component Item Id 参数。</param>
    /// <param name="componentItemCode">component Item Code 参数。</param>
    /// <param name="componentItemName">component Item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    public BillOfMaterialLine(
        Guid componentItemId,
        string componentItemCode,
        string componentItemName,
        decimal quantity,
        string unit)
    {
        ComponentItemId = componentItemId;
        ComponentItemCode = componentItemCode;
        ComponentItemName = componentItemName;
        Quantity = quantity;
        Unit = unit;
    }

    /// <summary>
    /// Bill Of Material Id。
    /// </summary>
    public Guid BillOfMaterialId { get; private set; }
    /// <summary>
    /// Component Item Id。
    /// </summary>
    public Guid ComponentItemId { get; private set; }
    /// <summary>
    /// Component Item Code。
    /// </summary>
    public string ComponentItemCode { get; private set; } = string.Empty;
    /// <summary>
    /// Component Item Name。
    /// </summary>
    public string ComponentItemName { get; private set; } = string.Empty;
    /// <summary>
    /// 数量。
    /// </summary>
    public decimal Quantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
}
