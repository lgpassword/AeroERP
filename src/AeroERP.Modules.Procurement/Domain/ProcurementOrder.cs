using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Procurement.Domain;

/// <summary>
/// Procurement Order 业务对象。
/// </summary>
public sealed class ProcurementOrder : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Procurement Order实例。
    /// </summary>
    private ProcurementOrder()
    {
    }

    /// <summary>
    /// 初始化Procurement Order实例。
    /// </summary>
    /// <param name="orderNo">order No 参数。</param>
    /// <param name="requestId">request Id 参数。</param>
    /// <param name="requestNo">request No 参数。</param>
    /// <param name="supplierId">供应商标识。</param>
    /// <param name="supplierName">supplier Name 参数。</param>
    /// <param name="createdBy">创建人。</param>
    public ProcurementOrder(string orderNo, Guid requestId, string requestNo, Guid supplierId, string supplierName, string createdBy)
    {
        OrderNo = orderNo;
        RequestId = requestId;
        RequestNo = requestNo;
        SupplierId = supplierId;
        SupplierName = supplierName;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Order No。
    /// </summary>
    public string OrderNo { get; private set; } = string.Empty;
    /// <summary>
    /// Request Id。
    /// </summary>
    public Guid RequestId { get; private set; }
    /// <summary>
    /// Request No。
    /// </summary>
    public string RequestNo { get; private set; } = string.Empty;
    /// <summary>
    /// Supplier Id。
    /// </summary>
    public Guid SupplierId { get; private set; }
    /// <summary>
    /// Supplier Name。
    /// </summary>
    public string SupplierName { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = ProcurementOrderStatus.Created;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Release。
    /// </summary>
    public void Release()
    {
        Status = ProcurementOrderStatus.Released;
        Touch();
    }

    /// <summary>
    /// Receive。
    /// </summary>
    public void Receive()
    {
        Status = ProcurementOrderStatus.Received;
        Touch();
    }
}
