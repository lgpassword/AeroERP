using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Warehouse Location 业务对象。
/// </summary>
public sealed class WarehouseLocation : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Warehouse Location实例。
    /// </summary>
    private WarehouseLocation()
    {
    }

    /// <summary>
    /// 初始化Warehouse Location实例。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="createdBy">创建人。</param>
    public WarehouseLocation(
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        string code,
        string name,
        bool isEnabled,
        string createdBy)
    {
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        Code = code.Trim();
        Name = name.Trim();
        IsEnabled = isEnabled;
        CreatedBy = createdBy;
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
    /// 业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
}
