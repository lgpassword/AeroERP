namespace AeroERP.Modules.Quality.Contracts;

/// <summary>
/// Quality Source Candidate 数据传输对象。
/// </summary>
public sealed record QualitySourceCandidateDto(
    string SourceDocumentType,
    Guid SourceDocumentId,
    string SourceDocumentNo,
    string SourceName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    DateTimeOffset OccurredAtUtc);

/// <summary>
/// Quality Inspection 数据传输对象。
/// </summary>
public sealed record QualityInspectionDto(
    Guid Id,
    string InspectionNo,
    string SourceDocumentType,
    Guid SourceDocumentId,
    string SourceDocumentNo,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal InspectedQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity,
    string Result,
    string Disposition,
    string Inspector,
    string Note,
    DateTimeOffset InspectedAtUtc);

/// <summary>
/// Create Quality Inspection 请求参数。
/// </summary>
public sealed record CreateQualityInspectionRequest(
    string SourceDocumentType,
    Guid SourceDocumentId,
    Guid ItemId,
    decimal InspectedQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity,
    string Disposition,
    string Note);

/// <summary>
/// Lot Trace Event 数据传输对象。
/// </summary>
public sealed record LotTraceEventDto(
    Guid Id,
    string LotNo,
    string EventType,
    string SourceDocumentType,
    Guid SourceDocumentId,
    string SourceDocumentNo,
    string TargetDocumentType,
    Guid? TargetDocumentId,
    string TargetDocumentNo,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    string Actor,
    string Note,
    DateTimeOffset OccurredAtUtc);

/// <summary>
/// Create Lot Trace Event 请求参数。
/// </summary>
public sealed record CreateLotTraceEventRequest(
    string LotNo,
    string EventType,
    string SourceDocumentType,
    Guid SourceDocumentId,
    Guid ItemId,
    decimal Quantity,
    string TargetDocumentType,
    Guid? TargetDocumentId,
    string TargetDocumentNo,
    string Note);

/// <summary>
/// Lot Trace 数据传输对象。
/// </summary>
/// <param name="LotNo">Lot No 参数。</param>
/// <param name="Events">事件集合。</param>
public sealed record LotTraceDto(string LotNo, IReadOnlyList<LotTraceEventDto> Events);
