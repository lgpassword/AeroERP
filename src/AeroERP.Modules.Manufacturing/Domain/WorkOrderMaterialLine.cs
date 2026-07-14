using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Manufacturing.Domain;

/// <summary>
/// Work Order Material 明细行实体。
/// </summary>
public sealed class WorkOrderMaterialLine : Entity
{
    /// <summary>
    /// 初始化Work Order Material Line实例。
    /// </summary>
    private WorkOrderMaterialLine()
    {
    }

    /// <summary>
    /// 初始化Work Order Material Line实例。
    /// </summary>
    /// <param name="componentItemId">component Item Id 参数。</param>
    /// <param name="componentItemCode">component Item Code 参数。</param>
    /// <param name="componentItemName">component Item Name 参数。</param>
    /// <param name="requiredQuantity">required Quantity 参数。</param>
    /// <param name="unit">计量单位。</param>
    public WorkOrderMaterialLine(
        Guid componentItemId,
        string componentItemCode,
        string componentItemName,
        decimal requiredQuantity,
        string unit)
    {
        ComponentItemId = componentItemId;
        ComponentItemCode = componentItemCode;
        ComponentItemName = componentItemName;
        RequiredQuantity = requiredQuantity;
        Unit = unit;
    }

    /// <summary>
    /// Work Order Id。
    /// </summary>
    public Guid WorkOrderId { get; private set; }
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
    /// Required Quantity。
    /// </summary>
    public decimal RequiredQuantity { get; private set; }
    /// <summary>
    /// Issued Quantity。
    /// </summary>
    public decimal IssuedQuantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;

    public decimal RemainingQuantity => RequiredQuantity - IssuedQuantity;

    /// <summary>
    /// Issue Remaining。
    /// </summary>
    public void IssueRemaining()
    {
        if (RemainingQuantity <= 0)
        {
            throw new InvalidOperationException("该物料已完成领料。");
        }

        IssuedQuantity = RequiredQuantity;
        Touch();
    }
}
