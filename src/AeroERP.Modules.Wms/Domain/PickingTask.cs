using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Wms.Domain;

/// <summary>
/// Picking Task 业务对象。
/// </summary>
public sealed class PickingTask : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Picking Task实例。
    /// </summary>
    private PickingTask()
    {
    }

    /// <summary>
    /// 初始化Picking Task实例。
    /// </summary>
    /// <param name="taskNo">task No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="sourceLocationId">source Location Id 参数。</param>
    /// <param name="sourceLocationCode">source Location Code 参数。</param>
    /// <param name="sourceLocationName">source Location Name 参数。</param>
    /// <param name="assignedTo">assigne DTO 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public PickingTask(
        string taskNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal quantity,
        string unit,
        Guid? sourceLocationId,
        string sourceLocationCode,
        string sourceLocationName,
        string assignedTo,
        string createdBy)
    {
        TaskNo = taskNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        Quantity = quantity;
        Unit = unit;
        SourceLocationId = sourceLocationId;
        SourceLocationCode = sourceLocationCode;
        SourceLocationName = sourceLocationName;
        AssignedTo = assignedTo;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Task No。
    /// </summary>
    public string TaskNo { get; private set; } = string.Empty;
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
    /// Source Location Id。
    /// </summary>
    public Guid? SourceLocationId { get; private set; }
    /// <summary>
    /// Source Location Code。
    /// </summary>
    public string SourceLocationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Source Location Name。
    /// </summary>
    public string SourceLocationName { get; private set; } = string.Empty;
    /// <summary>
    /// Wave Id。
    /// </summary>
    public Guid? WaveId { get; private set; }
    /// <summary>
    /// Wave No。
    /// </summary>
    public string WaveNo { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = WmsTaskStatus.Planned;
    /// <summary>
    /// Assigned To。
    /// </summary>
    public string AssignedTo { get; private set; } = string.Empty;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed By。
    /// </summary>
    public string CompletedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Completed At Utc。
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Assign Wave。
    /// </summary>
    /// <param name="waveId">wave Id 参数。</param>
    /// <param name="waveNo">wave No 参数。</param>
    public void AssignWave(Guid waveId, string waveNo)
    {
        WaveId = waveId;
        WaveNo = waveNo;
        Touch();
    }

    /// <summary>
    /// Release。
    /// </summary>
    public void Release()
    {
        if (Status == WmsTaskStatus.Planned)
        {
            Status = WmsTaskStatus.Released;
            Touch();
        }
    }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="actor">操作人。</param>
    public void Complete(string actor)
    {
        Status = WmsTaskStatus.Completed;
        CompletedBy = actor;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
