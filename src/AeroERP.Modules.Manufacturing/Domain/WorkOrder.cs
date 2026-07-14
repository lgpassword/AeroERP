using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Manufacturing.Domain;

/// <summary>
/// Work Order 业务对象。
/// </summary>
public sealed class WorkOrder : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Work Order实例。
    /// </summary>
    private WorkOrder()
    {
    }

    /// <summary>
    /// 初始化Work Order实例。
    /// </summary>
    /// <param name="workOrderNo">work Order No 参数。</param>
    /// <param name="bomId">bom Id 参数。</param>
    /// <param name="bomNo">bom No 参数。</param>
    /// <param name="bomVersion">bom Version 参数。</param>
    /// <param name="finishedItemId">finished Item Id 参数。</param>
    /// <param name="finishedItemCode">finished Item Code 参数。</param>
    /// <param name="finishedItemName">finished Item Name 参数。</param>
    /// <param name="plannedQuantity">planned Quantity 参数。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="createdBy">创建人。</param>
    /// <param name="materialLines">material Lines 参数。</param>
    public WorkOrder(
        string workOrderNo,
        Guid bomId,
        string bomNo,
        string bomVersion,
        Guid finishedItemId,
        string finishedItemCode,
        string finishedItemName,
        decimal plannedQuantity,
        string unit,
        string createdBy,
        IEnumerable<WorkOrderMaterialLine> materialLines)
    {
        WorkOrderNo = workOrderNo;
        BomId = bomId;
        BomNo = bomNo;
        BomVersion = bomVersion;
        FinishedItemId = finishedItemId;
        FinishedItemCode = finishedItemCode;
        FinishedItemName = finishedItemName;
        PlannedQuantity = plannedQuantity;
        Unit = unit;
        CreatedBy = createdBy;
        MaterialLines = materialLines.ToList();
    }

    /// <summary>
    /// Work Order No。
    /// </summary>
    public string WorkOrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Bom Id。
    /// </summary>
    public Guid BomId { get; private set; }
    /// <summary>
    /// Bom No。
    /// </summary>
    public string BomNo { get; private set; } = string.Empty;
    /// <summary>
    /// Bom Version。
    /// </summary>
    public string BomVersion { get; private set; } = string.Empty;
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
    /// Completed Quantity。
    /// </summary>
    public decimal CompletedQuantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = WorkOrderStatus.Draft;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Released At Utc。
    /// </summary>
    public DateTimeOffset? ReleasedAtUtc { get; private set; }
    /// <summary>
    /// Closed At Utc。
    /// </summary>
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    /// <summary>
    /// Material Lines。
    /// </summary>
    public List<WorkOrderMaterialLine> MaterialLines { get; private set; } = [];

    /// <summary>
    /// Release。
    /// </summary>
    public void Release()
    {
        if (!string.Equals(Status, WorkOrderStatus.Draft, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有草稿工单可以下达。");
        }

        Status = WorkOrderStatus.Released;
        ReleasedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Mark Materials Issued。
    /// </summary>
    public void MarkMaterialsIssued()
    {
        if (!string.Equals(Status, WorkOrderStatus.Released, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有已下达工单可以生产领料。");
        }

        Status = WorkOrderStatus.MaterialsIssued;
        Touch();
    }

    /// <summary>
    /// Complete。
    /// </summary>
    /// <param name="quantity">数量。</param>
    public void Complete(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("完工数量必须大于零。");
        }

        if (CompletedQuantity + quantity > PlannedQuantity)
        {
            throw new InvalidOperationException("完工数量不能超过工单计划数量。");
        }

        CompletedQuantity += quantity;
        Status = CompletedQuantity == PlannedQuantity
            ? WorkOrderStatus.Completed
            : WorkOrderStatus.PartiallyCompleted;
        ClosedAtUtc = string.Equals(Status, WorkOrderStatus.Completed, StringComparison.Ordinal)
            ? DateTimeOffset.UtcNow
            : null;
        Touch();
    }
}
