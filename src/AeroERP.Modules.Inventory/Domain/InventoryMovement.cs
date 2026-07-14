using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Movement 业务对象。
/// </summary>
public sealed class InventoryMovement : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Inventory Movement实例。
    /// </summary>
    private InventoryMovement()
    {
    }

    /// <summary>
    /// 初始化Inventory Movement实例。
    /// </summary>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="documentNo">业务单据编号。</param>
    /// <param name="movementType">movement Type 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="changeQuantity">change Quantity 参数。</param>
    /// <param name="balanceAfter">balance After 参数。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="locationCode">location Code 参数。</param>
    /// <param name="locationName">location Name 参数。</param>
    /// <param name="unitCost">单位成本。</param>
    /// <param name="costAmount">成本金额。</param>
    /// <param name="balanceCostAfter">balance Cost After 参数。</param>
    public InventoryMovement(
        string documentType,
        string documentNo,
        string movementType,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal changeQuantity,
        decimal balanceAfter,
        string unit,
        string actor,
        Guid? locationId = null,
        string locationCode = "",
        string locationName = "",
        decimal unitCost = 0m,
        decimal costAmount = 0m,
        decimal balanceCostAfter = 0m)
    {
        DocumentType = documentType;
        DocumentNo = documentNo;
        MovementType = movementType;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        ChangeQuantity = changeQuantity;
        BalanceAfter = balanceAfter;
        Unit = unit;
        Actor = actor;
        LocationId = locationId;
        LocationCode = locationCode;
        LocationName = locationName;
        UnitCost = unitCost;
        CostAmount = costAmount;
        BalanceCostAfter = balanceCostAfter;
    }

    /// <summary>
    /// 业务单据类型。
    /// </summary>
    public string DocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// 业务单据编号。
    /// </summary>
    public string DocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// Movement Type。
    /// </summary>
    public string MovementType { get; private set; } = string.Empty;
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
    /// Location Id。
    /// </summary>
    public Guid? LocationId { get; private set; }
    /// <summary>
    /// Location Code。
    /// </summary>
    public string LocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Location Name。
    /// </summary>
    public string LocationName { get; private set; } = string.Empty;
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
    /// Change Quantity。
    /// </summary>
    public decimal ChangeQuantity { get; private set; }
    /// <summary>
    /// Balance After。
    /// </summary>
    public decimal BalanceAfter { get; private set; }
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
    /// <summary>
    /// Balance Cost After。
    /// </summary>
    public decimal BalanceCostAfter { get; private set; }
    /// <summary>
    /// 操作人。
    /// </summary>
    public string Actor { get; private set; } = string.Empty;
}
