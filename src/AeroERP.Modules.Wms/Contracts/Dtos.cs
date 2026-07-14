namespace AeroERP.Modules.Wms.Contracts;

/// <summary>
/// Wms Warehouse Option 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
public sealed record WmsWarehouseOptionDto(Guid Id, string Code, string Name);
/// <summary>
/// Wms Location Option 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="WarehouseId">仓库标识。</param>
/// <param name="WarehouseName">Warehouse Name 参数。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
public sealed record WmsLocationOptionDto(Guid Id, Guid WarehouseId, string WarehouseName, string Code, string Name);
/// <summary>
/// Wms Item Option 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Unit">计量单位。</param>
public sealed record WmsItemOptionDto(Guid Id, string Code, string Name, string Unit);

/// <summary>
/// Put Away Task 数据传输对象。
/// </summary>
public sealed record PutAwayTaskDto(
    Guid Id,
    string TaskNo,
    Guid WarehouseId,
    string WarehouseName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    Guid? SuggestedLocationId,
    string SuggestedLocationName,
    string ContainerCode,
    string SourceDocumentNo,
    string Status,
    string AssignedTo,
    string CreatedBy,
    string CompletedBy,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Picking Task 数据传输对象。
/// </summary>
public sealed record PickingTaskDto(
    Guid Id,
    string TaskNo,
    Guid WarehouseId,
    string WarehouseName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal Quantity,
    string Unit,
    Guid? SourceLocationId,
    string SourceLocationName,
    Guid? WaveId,
    string WaveNo,
    string Status,
    string AssignedTo,
    string CreatedBy,
    string CompletedBy,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Picking Wave 数据传输对象。
/// </summary>
public sealed record PickingWaveDto(
    Guid Id,
    string WaveNo,
    Guid WarehouseId,
    string WarehouseName,
    string Status,
    string CreatedBy,
    string ReleasedBy,
    DateTimeOffset? ReleasedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Warehouse Container 数据传输对象。
/// </summary>
public sealed record WarehouseContainerDto(
    Guid Id,
    string Code,
    string ContainerType,
    Guid WarehouseId,
    string WarehouseName,
    Guid? CurrentLocationId,
    string CurrentLocationName,
    string Status,
    string LastHandledBy,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Warehouse Route 数据传输对象。
/// </summary>
public sealed record WarehouseRouteDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    Guid FromLocationId,
    string FromLocationName,
    Guid ToLocationId,
    string ToLocationName,
    decimal DistanceMeters,
    int Priority,
    bool IsEnabled,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Pda Work Queue Item 数据传输对象。
/// </summary>
public sealed record PdaWorkQueueItemDto(
    Guid Id,
    string TaskType,
    Guid TaskId,
    string TaskNo,
    Guid WarehouseId,
    string WarehouseName,
    string LocationCode,
    string AssignedTo,
    int Priority,
    string Status,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Wms Overview 数据传输对象。
/// </summary>
public sealed record WmsOverviewDto(
    IReadOnlyList<PutAwayTaskDto> PutAwayTasks,
    IReadOnlyList<PickingTaskDto> PickingTasks,
    IReadOnlyList<PickingWaveDto> Waves,
    IReadOnlyList<WarehouseContainerDto> Containers,
    IReadOnlyList<WarehouseRouteDto> Routes,
    IReadOnlyList<PdaWorkQueueItemDto> PdaQueue,
    IReadOnlyList<WmsWarehouseOptionDto> Warehouses,
    IReadOnlyList<WmsLocationOptionDto> Locations,
    IReadOnlyList<WmsItemOptionDto> Items);

/// <summary>
/// Upsert Warehouse Container 请求参数。
/// </summary>
public sealed record UpsertWarehouseContainerRequest(
    string Code,
    string ContainerType,
    Guid WarehouseId,
    Guid? CurrentLocationId,
    string Status);

/// <summary>
/// Upsert Warehouse Route 请求参数。
/// </summary>
public sealed record UpsertWarehouseRouteRequest(
    Guid WarehouseId,
    Guid FromLocationId,
    Guid ToLocationId,
    decimal DistanceMeters,
    int Priority,
    bool IsEnabled);

/// <summary>
/// Create Put Away Task 请求参数。
/// </summary>
public sealed record CreatePutAwayTaskRequest(
    Guid WarehouseId,
    Guid ItemId,
    decimal Quantity,
    Guid? SuggestedLocationId,
    string ContainerCode,
    string SourceDocumentNo,
    string AssignedTo);

/// <summary>
/// Complete Put Away Task 请求参数。
/// </summary>
/// <param name="TargetLocationId">Target Location Id 参数。</param>
public sealed record CompletePutAwayTaskRequest(Guid TargetLocationId);

/// <summary>
/// Create Picking Task 请求参数。
/// </summary>
public sealed record CreatePickingTaskRequest(
    Guid WarehouseId,
    Guid ItemId,
    decimal Quantity,
    Guid? SourceLocationId,
    string AssignedTo);

/// <summary>
/// Complete Picking Task 请求参数。
/// </summary>
/// <param name="Note">备注。</param>
public sealed record CompletePickingTaskRequest(string Note);

/// <summary>
/// Create Picking Wave 请求参数。
/// </summary>
/// <param name="WarehouseId">仓库标识。</param>
/// <param name="PickingTaskIds">Picking Task Ids 参数。</param>
public sealed record CreatePickingWaveRequest(Guid WarehouseId, IReadOnlyList<Guid> PickingTaskIds);
