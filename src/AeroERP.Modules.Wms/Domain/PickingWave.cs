using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Wms.Domain;

/// <summary>
/// Picking Wave 业务对象。
/// </summary>
public sealed class PickingWave : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Picking Wave实例。
    /// </summary>
    private PickingWave()
    {
    }

    /// <summary>
    /// 初始化Picking Wave实例。
    /// </summary>
    /// <param name="waveNo">wave No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public PickingWave(string waveNo, Guid warehouseId, string warehouseCode, string warehouseName, string createdBy)
    {
        WaveNo = waveNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Wave No。
    /// </summary>
    public string WaveNo { get; private set; } = string.Empty;
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
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = WmsTaskStatus.Planned;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Released By。
    /// </summary>
    public string ReleasedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Released At Utc。
    /// </summary>
    public DateTimeOffset? ReleasedAtUtc { get; private set; }

    /// <summary>
    /// Release。
    /// </summary>
    /// <param name="actor">操作人。</param>
    public void Release(string actor)
    {
        Status = WmsTaskStatus.Released;
        ReleasedBy = actor;
        ReleasedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
