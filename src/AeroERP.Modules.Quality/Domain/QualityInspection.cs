using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Quality.Domain;

/// <summary>
/// Quality Inspection 业务对象。
/// </summary>
public sealed class QualityInspection : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Quality Inspection实例。
    /// </summary>
    private QualityInspection()
    {
    }

    /// <summary>
    /// 初始化Quality Inspection实例。
    /// </summary>
    /// <param name="inspectionNo">inspection No 参数。</param>
    /// <param name="sourceDocumentType">source Document Type 参数。</param>
    /// <param name="sourceDocumentId">source Document Id 参数。</param>
    /// <param name="sourceDocumentNo">source Document No 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="inspectedQuantity">inspected Quantity 参数。</param>
    /// <param name="acceptedQuantity">accepted Quantity 参数。</param>
    /// <param name="rejectedQuantity">rejected Quantity 参数。</param>
    /// <param name="result">执行结果。</param>
    /// <param name="disposition">处置意见。</param>
    /// <param name="inspector">质检员。</param>
    /// <param name="note">备注。</param>
    public QualityInspection(
        string inspectionNo,
        string sourceDocumentType,
        Guid sourceDocumentId,
        string sourceDocumentNo,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal inspectedQuantity,
        decimal acceptedQuantity,
        decimal rejectedQuantity,
        string result,
        string disposition,
        string inspector,
        string note)
    {
        InspectionNo = inspectionNo;
        SourceDocumentType = sourceDocumentType;
        SourceDocumentId = sourceDocumentId;
        SourceDocumentNo = sourceDocumentNo;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        InspectedQuantity = inspectedQuantity;
        AcceptedQuantity = acceptedQuantity;
        RejectedQuantity = rejectedQuantity;
        Result = result;
        Disposition = disposition;
        Inspector = inspector;
        Note = note;
    }

    /// <summary>
    /// Inspection No。
    /// </summary>
    public string InspectionNo { get; private set; } = string.Empty;
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
    /// Inspected Quantity。
    /// </summary>
    public decimal InspectedQuantity { get; private set; }
    /// <summary>
    /// Accepted Quantity。
    /// </summary>
    public decimal AcceptedQuantity { get; private set; }
    /// <summary>
    /// Rejected Quantity。
    /// </summary>
    public decimal RejectedQuantity { get; private set; }
    /// <summary>
    /// 执行结果。
    /// </summary>
    public string Result { get; private set; } = string.Empty;
    /// <summary>
    /// Disposition。
    /// </summary>
    public string Disposition { get; private set; } = string.Empty;
    /// <summary>
    /// Inspector。
    /// </summary>
    public string Inspector { get; private set; } = string.Empty;
    /// <summary>
    /// 备注。
    /// </summary>
    public string Note { get; private set; } = string.Empty;
}
