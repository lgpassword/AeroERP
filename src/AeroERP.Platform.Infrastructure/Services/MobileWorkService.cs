using System.Text.Json;
using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.MobileWork.Contracts;
using AeroERP.Modules.MobileWork.Domain;
using AeroERP.Modules.MobileWork.Services;
using AeroERP.Modules.Wms.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Mobile Work Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class MobileWorkService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IMobileWorkService
{
    /// <summary>
    /// Mobile Source Modules。
    /// </summary>
    private static readonly string[] MobileSourceModules = ["wms", "inventory", "manufacturing", "quality", "planning"];

    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<MobileWorkOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var accessibleModules = MobileSourceModules.Where(currentUser.CanAccessModule).ToArray();
        var devices = await dbContext.MobileDevices
            .AsNoTracking()
            .OrderBy(x => x.DeviceCode)
            .ToListAsync(cancellationToken);
        var offlineTasks = (await dbContext.MobileOfflineTasks
                .AsNoTracking()
                .Where(x => accessibleModules.Contains(x.SourceModule))
                .ToListAsync(cancellationToken))
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var scanEvents = (await dbContext.MobileScanEvents
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(80)
            .ToList();
        var pdaQueue = accessibleModules.Contains("wms", StringComparer.OrdinalIgnoreCase)
            ? (await dbContext.PdaWorkQueueItems
                    .AsNoTracking()
                    .Where(x => x.Status != WmsTaskStatus.Completed)
                    .ToListAsync(cancellationToken))
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Priority)
                .ThenByDescending(x => x.UpdatedAtUtc)
                .Take(80)
                .ToList()
            : [];

        var workQueue = pdaQueue.Select(MapQueue).Concat(offlineTasks
            .Where(x => x.Status != MobileWorkStatus.Completed)
            .Select(MapOfflineQueue))
            .OrderBy(x => x.Status)
            .ThenBy(x => x.Priority)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .ToList();

        var today = DateTimeOffset.UtcNow.Date;
        var metrics = new List<MobileWorkMetricDto>
        {
            new("enabled-devices", "启用设备", devices.Count(x => x.IsEnabled), "台"),
            new("offline-open", "待同步任务", offlineTasks.Count(x => x.Status != MobileWorkStatus.Completed), "条"),
            new("pda-open", "PDA 队列", pdaQueue.Count, "条"),
            new("scan-today", "今日扫码", scanEvents.Count(x => x.CreatedAtUtc >= today), "次")
        };

        return new MobileWorkOverviewDto(
            devices.Select(MapDevice).ToList(),
            offlineTasks.Select(MapOfflineTask).ToList(),
            scanEvents.Select(MapScanEvent).ToList(),
            workQueue,
            metrics);
    }

    /// <summary>
    /// Upsert Device Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MobileWorkDeviceDto>> UpsertDeviceAsync(UpsertMobileDeviceRequest request, CancellationToken cancellationToken)
    {
        var deviceCode = NormalizeCode(request.DeviceCode);
        var displayName = NormalizeText(request.DisplayName);
        if (string.IsNullOrWhiteSpace(deviceCode) || string.IsNullOrWhiteSpace(displayName))
        {
            return OperationResult<MobileWorkDeviceDto>.Failure("设备编码和设备名称不能为空。");
        }

        var actor = currentUser.GetActor();
        var device = await dbContext.MobileDevices.FirstOrDefaultAsync(x => x.DeviceCode == deviceCode, cancellationToken);
        if (device is null)
        {
            device = new MobileDevice(deviceCode, displayName, NormalizeText(request.AssignedTo), request.IsEnabled, actor);
            dbContext.MobileDevices.Add(device);
        }
        else
        {
            device.Update(displayName, NormalizeText(request.AssignedTo), request.IsEnabled, actor);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MobileWork", "DeviceUpserted", actor, deviceCode, cancellationToken);
        return OperationResult<MobileWorkDeviceDto>.Success(MapDevice(device));
    }

    /// <summary>
    /// 创建Offline Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MobileWorkOfflineTaskDto>> CreateOfflineTaskAsync(CreateMobileOfflineTaskRequest request, CancellationToken cancellationToken)
    {
        var sourceModule = NormalizeKey(request.SourceModule);
        var sourceTaskType = NormalizeText(request.SourceTaskType);
        var sourceTaskNo = NormalizeText(request.SourceTaskNo);
        if (string.IsNullOrWhiteSpace(sourceModule) || string.IsNullOrWhiteSpace(sourceTaskType) || string.IsNullOrWhiteSpace(sourceTaskNo))
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("来源模块、任务类型和来源任务号不能为空。");
        }

        if (!currentUser.CanAccessModule(sourceModule))
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("当前账号不能访问该来源模块。");
        }

        string payloadJson;
        try
        {
            payloadJson = NormalizeJson(request.PayloadJson);
        }
        catch (JsonException)
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("任务载荷 JSON 格式无效。");
        }

        var hasOpenTask = await dbContext.MobileOfflineTasks.AnyAsync(
            x => x.SourceModule == sourceModule
                && x.SourceTaskType == sourceTaskType
                && x.SourceTaskNo == sourceTaskNo
                && x.Status != MobileWorkStatus.Completed,
            cancellationToken);
        if (hasOpenTask)
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("该来源任务已有未完成的移动离线缓存。");
        }

        var actor = currentUser.GetActor();
        var task = new MobileOfflineTask(
            NextNo("MWT"),
            sourceModule,
            sourceTaskType,
            sourceTaskNo,
            payloadJson,
            NormalizeText(request.AssignedTo),
            actor);

        dbContext.MobileOfflineTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MobileWork", "OfflineTaskCreated", actor, task.TaskNo, cancellationToken);
        return OperationResult<MobileWorkOfflineTaskDto>.Success(MapOfflineTask(task));
    }

    /// <summary>
    /// Mark Offline Task Synced Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MobileWorkOfflineTaskDto>> MarkOfflineTaskSyncedAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await dbContext.MobileOfflineTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("离线任务不存在。");
        }

        if (task.Status == MobileWorkStatus.Completed)
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("已完成的离线任务不能再次同步。");
        }

        var actor = currentUser.GetActor();
        task.MarkSynced();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MobileWork", "OfflineTaskSynced", actor, task.TaskNo, cancellationToken);
        return OperationResult<MobileWorkOfflineTaskDto>.Success(MapOfflineTask(task));
    }

    /// <summary>
    /// Complete Offline Task Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MobileWorkOfflineTaskDto>> CompleteOfflineTaskAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await dbContext.MobileOfflineTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("离线任务不存在。");
        }

        if (task.Status == MobileWorkStatus.Completed)
        {
            return OperationResult<MobileWorkOfflineTaskDto>.Failure("离线任务已完成。");
        }

        var actor = currentUser.GetActor();
        task.Complete(actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MobileWork", "OfflineTaskCompleted", actor, task.TaskNo, cancellationToken);
        return OperationResult<MobileWorkOfflineTaskDto>.Success(MapOfflineTask(task));
    }

    /// <summary>
    /// Record Scan Event Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MobileWorkScanEventDto>> RecordScanEventAsync(RecordMobileScanEventRequest request, CancellationToken cancellationToken)
    {
        var deviceCode = NormalizeCode(request.DeviceCode);
        var barcode = NormalizeText(request.Barcode);
        var targetModule = NormalizeKey(request.TargetModule);
        var action = NormalizeText(request.Action);
        if (string.IsNullOrWhiteSpace(deviceCode) || string.IsNullOrWhiteSpace(barcode) || string.IsNullOrWhiteSpace(targetModule) || string.IsNullOrWhiteSpace(action))
        {
            return OperationResult<MobileWorkScanEventDto>.Failure("设备、条码、目标模块和动作不能为空。");
        }

        var device = await dbContext.MobileDevices.AsNoTracking().FirstOrDefaultAsync(x => x.DeviceCode == deviceCode, cancellationToken);
        if (device is null || !device.IsEnabled)
        {
            return OperationResult<MobileWorkScanEventDto>.Failure("移动设备不存在或已停用。");
        }

        var targetExists = await dbContext.PluginModules.AsNoTracking().AnyAsync(x => x.Key == targetModule && x.IsVisible, cancellationToken);
        if (!targetExists || !currentUser.CanAccessModule(targetModule))
        {
            return OperationResult<MobileWorkScanEventDto>.Failure("目标模块不存在、未启用或当前账号不可访问。");
        }

        var actor = currentUser.GetActor();
        var scan = new MobileScanEvent(
            NextNo("MWS"),
            deviceCode,
            barcode,
            targetModule,
            action,
            NormalizeText(request.DocumentNo),
            NormalizeText(request.Result),
            NormalizeText(request.Message),
            actor);

        dbContext.MobileScanEvents.Add(scan);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("MobileWork", "ScanEventRecorded", actor, $"{scan.ScanNo}:{barcode}", cancellationToken);
        return OperationResult<MobileWorkScanEventDto>.Success(MapScanEvent(scan));
    }

    /// <summary>
    /// Next No。
    /// </summary>
    /// <param name="prefix">编号前缀。</param>
    private static string NextNo(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    /// <summary>
    /// Normalize Text。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeText(string value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Normalize Key。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeKey(string value) => NormalizeText(value).ToLowerInvariant();

    /// <summary>
    /// Normalize Code。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeCode(string value) => NormalizeText(value).ToUpperInvariant();

    /// <summary>
    /// Normalize Json。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static string NormalizeJson(string value)
    {
        var text = NormalizeText(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return "{}";
        }

        using var document = JsonDocument.Parse(text);
        return document.RootElement.GetRawText();
    }

    /// <summary>
    /// 注册Device 路由。
    /// </summary>
    /// <param name="device">移动设备。</param>
    private static MobileWorkDeviceDto MapDevice(MobileDevice device) =>
        new(device.Id, device.DeviceCode, device.DisplayName, device.AssignedTo, device.IsEnabled, device.UpdatedBy, device.LastSeenAtUtc, device.UpdatedAtUtc);

    /// <summary>
    /// 注册Offline Task 路由。
    /// </summary>
    /// <param name="task">任务对象。</param>
    private static MobileWorkOfflineTaskDto MapOfflineTask(MobileOfflineTask task) =>
        new(task.Id, task.TaskNo, task.SourceModule, task.SourceTaskType, task.SourceTaskNo, task.PayloadJson, task.AssignedTo, task.Status, task.CreatedBy, task.CompletedBy, task.CompletedAtUtc, task.UpdatedAtUtc);

    /// <summary>
    /// 注册Scan Event 路由。
    /// </summary>
    /// <param name="scan">扫码记录。</param>
    private static MobileWorkScanEventDto MapScanEvent(MobileScanEvent scan) =>
        new(scan.Id, scan.ScanNo, scan.DeviceCode, scan.Barcode, scan.TargetModule, scan.Action, scan.DocumentNo, scan.Result, scan.Message, scan.Actor, scan.CreatedAtUtc);

    /// <summary>
    /// 注册Queue 路由。
    /// </summary>
    /// <param name="item">物料对象。</param>
    private static MobileWorkQueueEntryDto MapQueue(PdaWorkQueueItem item) =>
        new(item.Id, "wms", item.TaskType, item.TaskId, item.TaskNo, item.WarehouseName, item.LocationCode, item.AssignedTo, item.Priority, item.Status, item.UpdatedAtUtc);

    /// <summary>
    /// 注册Offline Queue 路由。
    /// </summary>
    /// <param name="task">任务对象。</param>
    private static MobileWorkQueueEntryDto MapOfflineQueue(MobileOfflineTask task) =>
        new(task.Id, task.SourceModule, task.SourceTaskType, task.Id, task.TaskNo, string.Empty, string.Empty, task.AssignedTo, 50, task.Status, task.UpdatedAtUtc);
}
