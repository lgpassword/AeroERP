using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Quality.Domain;

/// <summary>
/// Lot Trace Event 业务对象。
/// </summary>
public sealed class LotTraceEvent : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Lot Trace Event实例。
    /// </summary>
    private LotTraceEvent()
    {
    }

    /// <summary>
    /// 初始化Lot Trace Event实例。
    /// </summary>
    /// <param name="lotNo">lot No 参数。</param>
    /// <param name="eventType">event Type 参数。</param>
    /// <param name="sourceDocumentType">source Document Type 参数。</param>
    /// <param name="sourceDocumentId">source Document Id 参数。</param>
    /// <param name="sourceDocumentNo">source Document No 参数。</param>
    /// <param name="targetDocumentType">target Document Type 参数。</param>
    /// <param name="targetDocumentId">target Document Id 参数。</param>
    /// <param name="targetDocumentNo">target Document No 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="quantity">数量。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="note">备注。</param>
    public LotTraceEvent(
        string lotNo,
        string eventType,
        string sourceDocumentType,
        Guid sourceDocumentId,
        string sourceDocumentNo,
        string targetDocumentType,
        Guid? targetDocumentId,
        string targetDocumentNo,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal quantity,
        string unit,
        string actor,
        string note)
    {
        LotNo = lotNo;
        EventType = eventType;
        SourceDocumentType = sourceDocumentType;
        SourceDocumentId = sourceDocumentId;
        SourceDocumentNo = sourceDocumentNo;
        TargetDocumentType = targetDocumentType;
        TargetDocumentId = targetDocumentId;
        TargetDocumentNo = targetDocumentNo;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        Quantity = quantity;
        Unit = unit;
        Actor = actor;
        Note = note;
    }

    /// <summary>
    /// Lot No。
    /// </summary>
    public string LotNo { get; private set; } = string.Empty;
    /// <summary>
    /// Event Type。
    /// </summary>
    public string EventType { get; private set; } = string.Empty;
    /// <summary>
    /// Source Document Type。
    /// </summary>
    public string SourceDocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// Source Document Id。
    /// </summary>
    public Guid SourceDocumentId { get; private set; }
    /// <summary>
    /// Source Document No。
    /// </summary>
    public string SourceDocumentNo { get; private set; } = string.Empty;
    /// <summary>
    /// Target Document Type。
    /// </summary>
    public string TargetDocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// Target Document Id。
    /// </summary>
    public Guid? TargetDocumentId { get; private set; }
    /// <summary>
    /// Target Document No。
    /// </summary>
    public string TargetDocumentNo { get; private set; } = string.Empty;
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
    /// 操作人。
    /// </summary>
    public string Actor { get; private set; } = string.Empty;
    /// <summary>
    /// 备注。
    /// </summary>
    public string Note { get; private set; } = string.Empty;
}
