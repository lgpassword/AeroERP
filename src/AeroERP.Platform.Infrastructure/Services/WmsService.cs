using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.MasterData.Domain;
using AeroERP.Modules.Wms.Contracts;
using AeroERP.Modules.Wms.Domain;
using AeroERP.Modules.Wms.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Wms Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class WmsService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IWmsService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<WmsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var putAwayTasks = (await dbContext.PutAwayTasks
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(50)
            .ToList();
        var pickingTasks = (await dbContext.PickingTasks
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(50)
            .ToList();
        var waves = (await dbContext.PickingWaves
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(50)
            .ToList();
        var containers = await dbContext.WarehouseContainers
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        var routes = await dbContext.WarehouseRoutes
            .AsNoTracking()
            .OrderBy(x => x.WarehouseCode)
            .ThenBy(x => x.Priority)
            .ToListAsync(cancellationToken);
        var queue = (await dbContext.PdaWorkQueueItems
                .AsNoTracking()
                .ToListAsync(cancellationToken))
            .OrderBy(x => x.Status)
            .ThenBy(x => x.Priority)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var warehouses = await dbContext.Warehouses
            .AsNoTracking()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        var locations = await dbContext.WarehouseLocations
            .AsNoTracking()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.WarehouseCode)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
        var items = await dbContext.Items
            .AsNoTracking()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return new WmsOverviewDto(
            putAwayTasks.Select(MapPutAway).ToList(),
            pickingTasks.Select(MapPicking).ToList(),
            waves.Select(MapWave).ToList(),
            containers.Select(MapContainer).ToList(),
            routes.Select(MapRoute).ToList(),
            queue.Select(MapQueue).ToList(),
            warehouses.Select(x => new WmsWarehouseOptionDto(x.Id, x.Code, x.Name)).ToList(),
            locations.Select(x => new WmsLocationOptionDto(x.Id, x.WarehouseId, x.WarehouseName, x.Code, x.Name)).ToList(),
            items.Select(x => new WmsItemOptionDto(x.Id, x.Code, x.Name, x.Unit)).ToList());
    }

    /// <summary>
    /// Upsert Container Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<WarehouseContainerDto>> UpsertContainerAsync(UpsertWarehouseContainerRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeText(request.Code).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
        {
            return OperationResult<WarehouseContainerDto>.Failure("容器编码不能为空。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<WarehouseContainerDto>.Failure("未找到可用仓库。");
        }

        var location = await GetEnabledLocationAsync(warehouse.Id, request.CurrentLocationId, cancellationToken);
        if (request.CurrentLocationId.HasValue && location is null)
        {
            return OperationResult<WarehouseContainerDto>.Failure("容器所在库位不存在、已停用或不属于所选仓库。");
        }

        var actor = currentUser.GetActor();
        var container = await dbContext.WarehouseContainers.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (container is null)
        {
            container = new WarehouseContainer(
                code,
                NormalizeText(request.ContainerType),
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                location?.Id,
                location?.Code ?? string.Empty,
                location?.Name ?? string.Empty,
                NormalizeText(request.Status),
                actor);
            dbContext.WarehouseContainers.Add(container);
        }
        else
        {
            container.Update(
                NormalizeText(request.ContainerType),
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                location?.Id,
                location?.Code ?? string.Empty,
                location?.Name ?? string.Empty,
                NormalizeText(request.Status),
                actor);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "ContainerUpserted", actor, code, cancellationToken);
        return OperationResult<WarehouseContainerDto>.Success(MapContainer(container));
    }

    /// <summary>
    /// Upsert Route Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<WarehouseRouteDto>> UpsertRouteAsync(UpsertWarehouseRouteRequest request, CancellationToken cancellationToken)
    {
        if (request.FromLocationId == request.ToLocationId)
        {
            return OperationResult<WarehouseRouteDto>.Failure("起点库位和终点库位不能相同。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<WarehouseRouteDto>.Failure("未找到可用仓库。");
        }

        var fromLocation = await GetEnabledLocationAsync(warehouse.Id, request.FromLocationId, cancellationToken);
        var toLocation = await GetEnabledLocationAsync(warehouse.Id, request.ToLocationId, cancellationToken);
        if (fromLocation is null || toLocation is null)
        {
            return OperationResult<WarehouseRouteDto>.Failure("路径库位不存在、已停用或不属于所选仓库。");
        }

        if (request.DistanceMeters <= 0)
        {
            return OperationResult<WarehouseRouteDto>.Failure("路径距离必须大于 0。");
        }

        var route = await dbContext.WarehouseRoutes.FirstOrDefaultAsync(
            x => x.WarehouseId == warehouse.Id && x.FromLocationId == fromLocation.Id && x.ToLocationId == toLocation.Id,
            cancellationToken);
        if (route is null)
        {
            route = new WarehouseRoute(
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                fromLocation.Id,
                fromLocation.Code,
                fromLocation.Name,
                toLocation.Id,
                toLocation.Code,
                toLocation.Name,
                request.DistanceMeters,
                request.Priority,
                request.IsEnabled);
            dbContext.WarehouseRoutes.Add(route);
        }
        else
        {
            route.Update(request.DistanceMeters, request.Priority, request.IsEnabled);
        }

        var actor = currentUser.GetActor();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "RouteUpserted", actor, $"{fromLocation.Code}->{toLocation.Code}", cancellationToken);
        return OperationResult<WarehouseRouteDto>.Success(MapRoute(route));
    }

    /// <summary>
    /// 创建Put Away Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PutAwayTaskDto>> CreatePutAwayTaskAsync(CreatePutAwayTaskRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return OperationResult<PutAwayTaskDto>.Failure("上架数量必须大于 0。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        var item = await GetEnabledItemAsync(request.ItemId, cancellationToken);
        if (warehouse is null || item is null)
        {
            return OperationResult<PutAwayTaskDto>.Failure("仓库或物料不存在、已停用。");
        }

        var location = await GetEnabledLocationAsync(warehouse.Id, request.SuggestedLocationId, cancellationToken);
        if (request.SuggestedLocationId.HasValue && location is null)
        {
            return OperationResult<PutAwayTaskDto>.Failure("建议上架库位不存在、已停用或不属于所选仓库。");
        }

        var actor = currentUser.GetActor();
        var task = new PutAwayTask(
            NextNo("PA"),
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            item.Id,
            item.Code,
            item.Name,
            request.Quantity,
            item.Unit,
            location?.Id,
            location?.Code ?? string.Empty,
            location?.Name ?? string.Empty,
            NormalizeText(request.ContainerCode),
            NormalizeText(request.SourceDocumentNo),
            NormalizeText(request.AssignedTo),
            actor);

        dbContext.PutAwayTasks.Add(task);
        AddQueue("上架", task.Id, task.TaskNo, warehouse, location?.Code ?? string.Empty, task.AssignedTo, 20);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "PutAwayTaskCreated", actor, task.TaskNo, cancellationToken);
        return OperationResult<PutAwayTaskDto>.Success(MapPutAway(task));
    }

    /// <summary>
    /// Complete Put Away Task Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PutAwayTaskDto>> CompletePutAwayTaskAsync(Guid id, CompletePutAwayTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await dbContext.PutAwayTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return OperationResult<PutAwayTaskDto>.Failure("上架任务不存在。");
        }

        if (task.Status == WmsTaskStatus.Completed)
        {
            return OperationResult<PutAwayTaskDto>.Failure("上架任务已完成。");
        }

        var location = await GetEnabledLocationAsync(task.WarehouseId, request.TargetLocationId, cancellationToken);
        if (location is null)
        {
            return OperationResult<PutAwayTaskDto>.Failure("目标上架库位不存在、已停用或不属于任务仓库。");
        }

        var actor = currentUser.GetActor();
        task.Complete(location.Id, location.Code, location.Name, actor);
        await CompleteQueueAsync("上架", task.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "PutAwayTaskCompleted", actor, task.TaskNo, cancellationToken);
        return OperationResult<PutAwayTaskDto>.Success(MapPutAway(task));
    }

    /// <summary>
    /// 创建Picking Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PickingTaskDto>> CreatePickingTaskAsync(CreatePickingTaskRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return OperationResult<PickingTaskDto>.Failure("拣货数量必须大于 0。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        var item = await GetEnabledItemAsync(request.ItemId, cancellationToken);
        if (warehouse is null || item is null)
        {
            return OperationResult<PickingTaskDto>.Failure("仓库或物料不存在、已停用。");
        }

        var location = await GetEnabledLocationAsync(warehouse.Id, request.SourceLocationId, cancellationToken);
        if (request.SourceLocationId.HasValue && location is null)
        {
            return OperationResult<PickingTaskDto>.Failure("来源拣货库位不存在、已停用或不属于所选仓库。");
        }

        var actor = currentUser.GetActor();
        var task = new PickingTask(
            NextNo("PK"),
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            item.Id,
            item.Code,
            item.Name,
            request.Quantity,
            item.Unit,
            location?.Id,
            location?.Code ?? string.Empty,
            location?.Name ?? string.Empty,
            NormalizeText(request.AssignedTo),
            actor);

        dbContext.PickingTasks.Add(task);
        AddQueue("拣货", task.Id, task.TaskNo, warehouse, location?.Code ?? string.Empty, task.AssignedTo, 30);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "PickingTaskCreated", actor, task.TaskNo, cancellationToken);
        return OperationResult<PickingTaskDto>.Success(MapPicking(task));
    }

    /// <summary>
    /// Complete Picking Task Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PickingTaskDto>> CompletePickingTaskAsync(Guid id, CompletePickingTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await dbContext.PickingTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return OperationResult<PickingTaskDto>.Failure("拣货任务不存在。");
        }

        if (task.Status == WmsTaskStatus.Completed)
        {
            return OperationResult<PickingTaskDto>.Failure("拣货任务已完成。");
        }

        var actor = currentUser.GetActor();
        task.Complete(actor);
        await CompleteQueueAsync("拣货", task.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "PickingTaskCompleted", actor, task.TaskNo, cancellationToken);
        return OperationResult<PickingTaskDto>.Success(MapPicking(task));
    }

    /// <summary>
    /// 创建Wave。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PickingWaveDto>> CreateWaveAsync(CreatePickingWaveRequest request, CancellationToken cancellationToken)
    {
        if (request.PickingTaskIds.Count == 0)
        {
            return OperationResult<PickingWaveDto>.Failure("波次至少需要包含一个拣货任务。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<PickingWaveDto>.Failure("未找到可用仓库。");
        }

        var taskIds = request.PickingTaskIds.Distinct().ToList();
        var tasks = await dbContext.PickingTasks
            .Where(x => taskIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        if (tasks.Count != taskIds.Count)
        {
            return OperationResult<PickingWaveDto>.Failure("包含不存在的拣货任务。");
        }

        if (tasks.Any(x => x.WarehouseId != warehouse.Id))
        {
            return OperationResult<PickingWaveDto>.Failure("波次只能包含同一仓库的拣货任务。");
        }

        if (tasks.Any(x => x.Status == WmsTaskStatus.Completed || x.WaveId.HasValue))
        {
            return OperationResult<PickingWaveDto>.Failure("已完成或已分配波次的拣货任务不能再次组波。");
        }

        var actor = currentUser.GetActor();
        var wave = new PickingWave(NextNo("WV"), warehouse.Id, warehouse.Code, warehouse.Name, actor);
        dbContext.PickingWaves.Add(wave);
        foreach (var task in tasks)
        {
            task.AssignWave(wave.Id, wave.WaveNo);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "PickingWaveCreated", actor, wave.WaveNo, cancellationToken);
        return OperationResult<PickingWaveDto>.Success(MapWave(wave));
    }

    /// <summary>
    /// Release Wave Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<PickingWaveDto>> ReleaseWaveAsync(Guid id, CancellationToken cancellationToken)
    {
        var wave = await dbContext.PickingWaves.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (wave is null)
        {
            return OperationResult<PickingWaveDto>.Failure("波次不存在。");
        }

        if (wave.Status != WmsTaskStatus.Planned)
        {
            return OperationResult<PickingWaveDto>.Failure("只有计划状态的波次可以释放。");
        }

        var tasks = await dbContext.PickingTasks
            .Where(x => x.WaveId == wave.Id)
            .ToListAsync(cancellationToken);
        if (tasks.Count == 0)
        {
            return OperationResult<PickingWaveDto>.Failure("波次没有拣货任务，无法释放。");
        }

        var actor = currentUser.GetActor();
        wave.Release(actor);
        foreach (var task in tasks)
        {
            task.Release();
            await ReleaseQueueAsync("拣货", task.Id, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("WMS", "PickingWaveReleased", actor, wave.WaveNo, cancellationToken);
        return OperationResult<PickingWaveDto>.Success(MapWave(wave));
    }

    /// <summary>
    /// 获取Enabled Warehouse。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<Warehouse?> GetEnabledWarehouseAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Warehouses.FirstOrDefaultAsync(x => x.Id == id && x.IsEnabled, cancellationToken);

    /// <summary>
    /// 获取Enabled Item。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<Item?> GetEnabledItemAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Items.FirstOrDefaultAsync(x => x.Id == id && x.IsEnabled, cancellationToken);

    /// <summary>
    /// 获取Enabled Location。
    /// </summary>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="locationId">location Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<WarehouseLocation?> GetEnabledLocationAsync(Guid warehouseId, Guid? locationId, CancellationToken cancellationToken)
    {
        if (!locationId.HasValue)
        {
            return null;
        }

        return await dbContext.WarehouseLocations.FirstOrDefaultAsync(
            x => x.Id == locationId.Value && x.WarehouseId == warehouseId && x.IsEnabled,
            cancellationToken);
    }

    /// <summary>
    /// Add Queue。
    /// </summary>
    /// <param name="taskType">task Type 参数。</param>
    /// <param name="taskId">task Id 参数。</param>
    /// <param name="taskNo">task No 参数。</param>
    /// <param name="warehouse">仓库对象。</param>
    /// <param name="locationCode">location Code 参数。</param>
    /// <param name="assignedTo">assigne DTO 参数。</param>
    /// <param name="priority">优先级。</param>
    private void AddQueue(string taskType, Guid taskId, string taskNo, Warehouse warehouse, string locationCode, string assignedTo, int priority)
    {
        dbContext.PdaWorkQueueItems.Add(new PdaWorkQueueItem(
            taskType,
            taskId,
            taskNo,
            warehouse.Id,
            warehouse.Name,
            locationCode,
            assignedTo,
            priority));
    }

    /// <summary>
    /// Release Queue Async。
    /// </summary>
    /// <param name="taskType">task Type 参数。</param>
    /// <param name="taskId">task Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task ReleaseQueueAsync(string taskType, Guid taskId, CancellationToken cancellationToken)
    {
        var item = await dbContext.PdaWorkQueueItems.FirstOrDefaultAsync(
            x => x.TaskType == taskType && x.TaskId == taskId,
            cancellationToken);
        item?.Release();
    }

    /// <summary>
    /// Complete Queue Async。
    /// </summary>
    /// <param name="taskType">task Type 参数。</param>
    /// <param name="taskId">task Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task CompleteQueueAsync(string taskType, Guid taskId, CancellationToken cancellationToken)
    {
        var item = await dbContext.PdaWorkQueueItems.FirstOrDefaultAsync(
            x => x.TaskType == taskType && x.TaskId == taskId,
            cancellationToken);
        item?.Complete();
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
    /// 注册Put Away 路由。
    /// </summary>
    /// <param name="task">任务对象。</param>
    private static PutAwayTaskDto MapPutAway(PutAwayTask task) =>
        new(
            task.Id,
            task.TaskNo,
            task.WarehouseId,
            task.WarehouseName,
            task.ItemId,
            task.ItemCode,
            task.ItemName,
            task.Quantity,
            task.Unit,
            task.SuggestedLocationId,
            task.SuggestedLocationName,
            task.ContainerCode,
            task.SourceDocumentNo,
            task.Status,
            task.AssignedTo,
            task.CreatedBy,
            task.CompletedBy,
            task.CompletedAtUtc,
            task.UpdatedAtUtc);

    /// <summary>
    /// 注册Picking 路由。
    /// </summary>
    /// <param name="task">任务对象。</param>
    private static PickingTaskDto MapPicking(PickingTask task) =>
        new(
            task.Id,
            task.TaskNo,
            task.WarehouseId,
            task.WarehouseName,
            task.ItemId,
            task.ItemCode,
            task.ItemName,
            task.Quantity,
            task.Unit,
            task.SourceLocationId,
            task.SourceLocationName,
            task.WaveId,
            task.WaveNo,
            task.Status,
            task.AssignedTo,
            task.CreatedBy,
            task.CompletedBy,
            task.CompletedAtUtc,
            task.UpdatedAtUtc);

    /// <summary>
    /// 注册Wave 路由。
    /// </summary>
    /// <param name="wave">波次对象。</param>
    private static PickingWaveDto MapWave(PickingWave wave) =>
        new(
            wave.Id,
            wave.WaveNo,
            wave.WarehouseId,
            wave.WarehouseName,
            wave.Status,
            wave.CreatedBy,
            wave.ReleasedBy,
            wave.ReleasedAtUtc,
            wave.UpdatedAtUtc);

    /// <summary>
    /// 注册Container 路由。
    /// </summary>
    /// <param name="container">容器对象。</param>
    private static WarehouseContainerDto MapContainer(WarehouseContainer container) =>
        new(
            container.Id,
            container.Code,
            container.ContainerType,
            container.WarehouseId,
            container.WarehouseName,
            container.CurrentLocationId,
            container.CurrentLocationName,
            container.Status,
            container.LastHandledBy,
            container.UpdatedAtUtc);

    /// <summary>
    /// 注册Route 路由。
    /// </summary>
    /// <param name="route">路径对象。</param>
    private static WarehouseRouteDto MapRoute(WarehouseRoute route) =>
        new(
            route.Id,
            route.WarehouseId,
            route.WarehouseName,
            route.FromLocationId,
            route.FromLocationName,
            route.ToLocationId,
            route.ToLocationName,
            route.DistanceMeters,
            route.Priority,
            route.IsEnabled,
            route.UpdatedAtUtc);

    /// <summary>
    /// 注册Queue 路由。
    /// </summary>
    /// <param name="item">物料对象。</param>
    private static PdaWorkQueueItemDto MapQueue(PdaWorkQueueItem item) =>
        new(
            item.Id,
            item.TaskType,
            item.TaskId,
            item.TaskNo,
            item.WarehouseId,
            item.WarehouseName,
            item.LocationCode,
            item.AssignedTo,
            item.Priority,
            item.Status,
            item.CompletedAtUtc,
            item.UpdatedAtUtc);
}
