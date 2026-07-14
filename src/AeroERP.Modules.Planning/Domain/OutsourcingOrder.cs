using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Planning.Domain;

/// <summary>
/// Outsourcing Order 业务对象。
/// </summary>
public sealed class OutsourcingOrder : Entity
{
    /// <summary>
    /// _material Lines。
    /// </summary>
    private readonly List<OutsourcingOrderLine> _materialLines = [];

    /// <summary>
    /// 初始化Outsourcing Order实例。
    /// </summary>
    private OutsourcingOrder()
    {
    }

    /// <summary>
    /// 初始化Outsourcing Order实例。
    /// </summary>
    /// <param name="orderNo">order No 参数。</param>
    /// <param name="supplierName">supplier Name 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="finishedItemId">finished Item Id 参数。</param>
    /// <param name="finishedItemCode">finished Item Code 参数。</param>
    /// <param name="finishedItemName">finished Item Name 参数。</param>
    /// <param name="plannedQuantity">planned Quantity 参数。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="createdBy">创建人。</param>
    /// <param name="materialLines">material Lines 参数。</param>
    public OutsourcingOrder(
        string orderNo,
        string supplierName,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid finishedItemId,
        string finishedItemCode,
        string finishedItemName,
        decimal plannedQuantity,
        string unit,
        string createdBy,
        IEnumerable<OutsourcingOrderLine> materialLines)
    {
        OrderNo = orderNo;
        SupplierName = supplierName;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        FinishedItemId = finishedItemId;
        FinishedItemCode = finishedItemCode;
        FinishedItemName = finishedItemName;
        PlannedQuantity = plannedQuantity;
        Unit = unit;
        CreatedBy = createdBy;
        Status = OutsourcingOrderStatus.Created;
        _materialLines.AddRange(materialLines);
    }

    /// <summary>
    /// Order No。
    /// </summary>
    public string OrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Supplier Name。
    /// </summary>
    public string SupplierName { get; private set; } = string.Empty;
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
    /// Received Quantity。
    /// </summary>
    public decimal ReceivedQuantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = string.Empty;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    public IReadOnlyList<OutsourcingOrderLine> MaterialLines => _materialLines;

    /// <summary>
    /// Mark Materials Issued。
    /// </summary>
    public void MarkMaterialsIssued()
    {
        if (!string.Equals(Status, OutsourcingOrderStatus.Created, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有已创建外协单可以发料。");
        }

        Status = OutsourcingOrderStatus.MaterialsIssued;
        Touch();
    }

    /// <summary>
    /// Receive。
    /// </summary>
    /// <param name="quantity">数量。</param>
    public void Receive(decimal quantity)
    {
        if (!string.Equals(Status, OutsourcingOrderStatus.MaterialsIssued, StringComparison.Ordinal) &&
            !string.Equals(Status, OutsourcingOrderStatus.PartiallyReceived, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有已发料外协单可以收料。");
        }

        if (quantity <= 0)
        {
            throw new InvalidOperationException("收料数量必须大于零。");
        }

        if (ReceivedQuantity + quantity > PlannedQuantity)
        {
            throw new InvalidOperationException("收料数量不能超过外协计划数量。");
        }

        ReceivedQuantity += quantity;
        Status = ReceivedQuantity == PlannedQuantity
            ? OutsourcingOrderStatus.Completed
            : OutsourcingOrderStatus.PartiallyReceived;
        Touch();
    }
}
