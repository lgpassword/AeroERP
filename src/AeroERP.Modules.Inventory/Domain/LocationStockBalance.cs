using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Location Stock Balance 业务对象。
/// </summary>
public sealed class LocationStockBalance : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Location Stock Balance实例。
    /// </summary>
    private LocationStockBalance()
    {
    }

    /// <summary>
    /// 初始化Location Stock Balance实例。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="locationCode">location Code 参数。</param>
    /// <param name="locationName">location Name 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="unitCost">单位成本。</param>
    public LocationStockBalance(
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid locationId,
        string locationCode,
        string locationName,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal quantity,
        string unit,
        decimal unitCost = 0m)
    {
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        LocationId = locationId;
        LocationCode = locationCode;
        LocationName = locationName;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        Quantity = quantity;
        Unit = unit;
        UnitCost = unitCost < 0 ? 0m : unitCost;
        InventoryValue = Quantity * UnitCost;
    }

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
    public Guid LocationId { get; private set; }
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
    /// Inventory Value。
    /// </summary>
    public decimal InventoryValue { get; private set; }

    /// <summary>
    /// Increase。
    /// </summary>
    /// <param name="quantity">数量。</param>
    public void Increase(decimal quantity)
    {
        Increase(quantity, UnitCost);
    }

    /// <summary>
    /// Increase。
    /// </summary>
    /// <param name="quantity">数量。</param>
    /// <param name="unitCost">单位成本。</param>
    public void Increase(decimal quantity, decimal unitCost)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("增加数量必须大于零。");
        }

        if (unitCost < 0)
        {
            throw new InvalidOperationException("单位成本不能为负数。");
        }

        var incomingValue = quantity * unitCost;
        Quantity += quantity;
        InventoryValue += incomingValue;
        UnitCost = Quantity == 0 ? 0m : InventoryValue / Quantity;
        Touch();
    }

    /// <summary>
    /// Decrease。
    /// </summary>
    /// <param name="quantity">数量。</param>
    public decimal Decrease(decimal quantity)
    {
        return Decrease(quantity, UnitCost);
    }

    /// <summary>
    /// Decrease。
    /// </summary>
    /// <param name="quantity">数量。</param>
    /// <param name="unitCost">单位成本。</param>
    public decimal Decrease(decimal quantity, decimal unitCost)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("扣减数量必须大于零。");
        }

        if (Quantity < quantity)
        {
            throw new InvalidOperationException("库位库存不足，无法执行扣减。");
        }

        if (unitCost < 0)
        {
            throw new InvalidOperationException("单位成本不能为负数。");
        }

        var costAmount = quantity * unitCost;
        Quantity -= quantity;
        InventoryValue -= costAmount;
        if (Quantity == 0)
        {
            UnitCost = 0m;
            InventoryValue = 0m;
        }
        else
        {
            UnitCost = InventoryValue / Quantity;
        }

        Touch();
        return costAmount;
    }

    /// <summary>
    /// Set Quantity。
    /// </summary>
    /// <param name="quantity">数量。</param>
    public void SetQuantity(decimal quantity)
    {
        if (quantity < 0)
        {
            throw new InvalidOperationException("库位库存数量不能为负数。");
        }

        Quantity = quantity;
        InventoryValue = Quantity * UnitCost;
        Touch();
    }
}
