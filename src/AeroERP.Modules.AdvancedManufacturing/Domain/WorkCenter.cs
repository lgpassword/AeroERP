using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.AdvancedManufacturing.Domain;

/// <summary>
/// Work Center 业务对象。
/// </summary>
public sealed class WorkCenter : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Work Center实例。
    /// </summary>
    private WorkCenter()
    {
    }

    /// <summary>
    /// 初始化Work Center实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="capacityMinutesPerDay">capacity Minutes Per Day 参数。</param>
    /// <param name="hourlyCostRate">hourly Cost Rate 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public WorkCenter(
        string code,
        string name,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        decimal capacityMinutesPerDay,
        decimal hourlyCostRate,
        bool isEnabled,
        string updatedBy)
    {
        Code = code;
        Name = name;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        CapacityMinutesPerDay = capacityMinutesPerDay;
        HourlyCostRate = hourlyCostRate;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
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
    /// Capacity Minutes Per Day。
    /// </summary>
    public decimal CapacityMinutesPerDay { get; private set; }
    /// <summary>
    /// Hourly Cost Rate。
    /// </summary>
    public decimal HourlyCostRate { get; private set; }
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="name">显示名称。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="capacityMinutesPerDay">capacity Minutes Per Day 参数。</param>
    /// <param name="hourlyCostRate">hourly Cost Rate 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(
        string name,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        decimal capacityMinutesPerDay,
        decimal hourlyCostRate,
        bool isEnabled,
        string updatedBy)
    {
        Name = name;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        CapacityMinutesPerDay = capacityMinutesPerDay;
        HourlyCostRate = hourlyCostRate;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
