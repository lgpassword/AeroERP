namespace AeroERP.Modules.MobileWork.Contracts;

/// <summary>
/// Mobile Work Metric 数据传输对象。
/// </summary>
/// <param name="Key">业务键。</param>
/// <param name="Label">界面显示标签。</param>
/// <param name="Value">数值或配置值。</param>
/// <param name="Unit">计量单位。</param>
public sealed record MobileWorkMetricDto(string Key, string Label, decimal Value, string Unit);

/// <summary>
/// Mobile Work Device 数据传输对象。
/// </summary>
public sealed record MobileWorkDeviceDto(
    Guid Id,
    string DeviceCode,
    string DisplayName,
    string AssignedTo,
    bool IsEnabled,
    string UpdatedBy,
    DateTimeOffset LastSeenAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Mobile Work Offline Task 数据传输对象。
/// </summary>
public sealed record MobileWorkOfflineTaskDto(
    Guid Id,
    string TaskNo,
    string SourceModule,
    string SourceTaskType,
    string SourceTaskNo,
    string PayloadJson,
    string AssignedTo,
    string Status,
    string CreatedBy,
    string CompletedBy,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Mobile Work Scan Event 数据传输对象。
/// </summary>
public sealed record MobileWorkScanEventDto(
    Guid Id,
    string ScanNo,
    string DeviceCode,
    string Barcode,
    string TargetModule,
    string Action,
    string DocumentNo,
    string Result,
    string Message,
    string Actor,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Mobile Work Queue Entry 数据传输对象。
/// </summary>
public sealed record MobileWorkQueueEntryDto(
    Guid Id,
    string SourceModule,
    string TaskType,
    Guid TaskId,
    string TaskNo,
    string WarehouseName,
    string LocationCode,
    string AssignedTo,
    int Priority,
    string Status,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Mobile Work Overview 数据传输对象。
/// </summary>
public sealed record MobileWorkOverviewDto(
    IReadOnlyList<MobileWorkDeviceDto> Devices,
    IReadOnlyList<MobileWorkOfflineTaskDto> OfflineTasks,
    IReadOnlyList<MobileWorkScanEventDto> ScanEvents,
    IReadOnlyList<MobileWorkQueueEntryDto> WorkQueue,
    IReadOnlyList<MobileWorkMetricDto> Metrics);

/// <summary>
/// Upsert Mobile Device 请求参数。
/// </summary>
public sealed record UpsertMobileDeviceRequest(
    string DeviceCode,
    string DisplayName,
    string AssignedTo,
    bool IsEnabled);

/// <summary>
/// Create Mobile Offline Task 请求参数。
/// </summary>
public sealed record CreateMobileOfflineTaskRequest(
    string SourceModule,
    string SourceTaskType,
    string SourceTaskNo,
    string PayloadJson,
    string AssignedTo);

/// <summary>
/// Record Mobile Scan Event 请求参数。
/// </summary>
public sealed record RecordMobileScanEventRequest(
    string DeviceCode,
    string Barcode,
    string TargetModule,
    string Action,
    string DocumentNo,
    string Result,
    string Message);
