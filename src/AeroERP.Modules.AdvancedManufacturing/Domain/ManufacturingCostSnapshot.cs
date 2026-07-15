using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.AdvancedManufacturing.Domain;

/// <summary>
/// Manufacturing Cost Snapshot 业务对象。
/// </summary>
public sealed class ManufacturingCostSnapshot : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Manufacturing Cost Snapshot实例。
    /// </summary>
    private ManufacturingCostSnapshot()
    {
    }

    /// <summary>
    /// 初始化Manufacturing Cost Snapshot实例。
    /// </summary>
    /// <param name="snapshotNo">snapshot No 参数。</param>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="workOrderNo">work Order No 参数。</param>
    /// <param name="finishedItemId">finished Item Id 参数。</param>
    /// <param name="finishedItemCode">finished Item Code 参数。</param>
    /// <param name="finishedItemName">finished Item Name 参数。</param>
    /// <param name="plannedQuantity">planned Quantity 参数。</param>
    /// <param name="materialCost">material Cost 参数。</param>
    /// <param name="laborCost">labor Cost 参数。</param>
    /// <param name="machineCost">machine Cost 参数。</param>
    /// <param name="overheadCost">overhead Cost 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public ManufacturingCostSnapshot(
        string snapshotNo,
        Guid workOrderId,
        string workOrderNo,
        Guid finishedItemId,
        string finishedItemCode,
        string finishedItemName,
        decimal plannedQuantity,
        decimal materialCost,
        decimal laborCost,
        decimal machineCost,
        decimal overheadCost,
        string createdBy)
    {
        SnapshotNo = snapshotNo;
        WorkOrderId = workOrderId;
        WorkOrderNo = workOrderNo;
        FinishedItemId = finishedItemId;
        FinishedItemCode = finishedItemCode;
        FinishedItemName = finishedItemName;
        PlannedQuantity = plannedQuantity;
        MaterialCost = materialCost;
        LaborCost = laborCost;
        MachineCost = machineCost;
        OverheadCost = overheadCost;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Snapshot No。
    /// </summary>
    public string SnapshotNo { get; private set; } = string.Empty;
    /// <summary>
    /// Work Order Id。
    /// </summary>
    public Guid WorkOrderId { get; private set; }
    /// <summary>
    /// Work Order No。
    /// </summary>
    public string WorkOrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Finished Item Id。
    /// </summary>
    public Guid FinishedItemId { get; private set; }
    /// <summary>
    /// Finished Item Code。
    /// </summary>
    public string FinishedItemCode { get; private set; } = string.Empty;
    /// <summary>
    /// Finished Item Name。
    /// </summary>
    public string FinishedItemName { get; private set; } = string.Empty;
    /// <summary>
    /// Planned Quantity。
    /// </summary>
    public decimal PlannedQuantity { get; private set; }
    /// <summary>
    /// Material Cost。
    /// </summary>
    public decimal MaterialCost { get; private set; }
    /// <summary>
    /// Labor Cost。
    /// </summary>
    public decimal LaborCost { get; private set; }
    /// <summary>
    /// Machine Cost。
    /// </summary>
    public decimal MachineCost { get; private set; }
    /// <summary>
    /// Overhead Cost。
    /// </summary>
    public decimal OverheadCost { get; private set; }
    public decimal TotalCost => MaterialCost + LaborCost + MachineCost + OverheadCost;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
}
