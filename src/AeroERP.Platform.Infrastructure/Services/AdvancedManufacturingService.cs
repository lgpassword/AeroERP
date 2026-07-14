using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.AdvancedManufacturing.Contracts;
using AeroERP.Modules.AdvancedManufacturing.Domain;
using AeroERP.Modules.AdvancedManufacturing.Services;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.Manufacturing.Domain;
using AeroERP.Modules.MasterData.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Advanced Manufacturing Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class AdvancedManufacturingService(
    AeroErpDbContext dbContext,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IAdvancedManufacturingService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<AdvancedManufacturingOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var workCenters = await dbContext.WorkCenters.AsNoTracking().OrderBy(x => x.Code).ToListAsync(cancellationToken);
        var routings = (await dbContext.ManufacturingRoutings
                .AsNoTracking()
                .Include(x => x.Operations)
                .ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(50)
            .ToList();
        var schedules = (await dbContext.OperationSchedules.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var capacityLoads = await dbContext.CapacityLoads.AsNoTracking().OrderByDescending(x => x.PlanDate).ThenBy(x => x.WorkCenterCode).Take(80).ToListAsync(cancellationToken);
        var costSnapshots = (await dbContext.ManufacturingCostSnapshots.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(80)
            .ToList();
        var mrpSuggestions = (await dbContext.MrpSuggestions.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();
        var warehouses = await dbContext.Warehouses.AsNoTracking().Where(x => x.IsEnabled).OrderBy(x => x.Code).ToListAsync(cancellationToken);
        var items = await dbContext.Items.AsNoTracking().Where(x => x.IsEnabled).OrderBy(x => x.Code).ToListAsync(cancellationToken);
        var workOrders = (await dbContext.WorkOrders.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(80)
            .ToList();

        return new AdvancedManufacturingOverviewDto(
            workCenters.Select(MapWorkCenter).ToList(),
            routings.Select(MapRouting).ToList(),
            schedules.Select(MapSchedule).ToList(),
            capacityLoads.Select(MapCapacityLoad).ToList(),
            costSnapshots.Select(MapCostSnapshot).ToList(),
            mrpSuggestions.Select(MapMrpSuggestion).ToList(),
            warehouses.Select(x => new AdvancedManufacturingWarehouseOptionDto(x.Id, x.Code, x.Name)).ToList(),
            items.Select(x => new AdvancedManufacturingItemOptionDto(x.Id, x.Code, x.Name, x.Unit)).ToList(),
            workOrders.Select(MapWorkOrderOption).ToList());
    }

    /// <summary>
    /// Upsert Work Center Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<WorkCenterDto>> UpsertWorkCenterAsync(UpsertWorkCenterRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeText(request.Code).ToUpperInvariant();
        var name = NormalizeText(request.Name);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<WorkCenterDto>.Failure("工作中心编码和名称不能为空。");
        }

        if (request.CapacityMinutesPerDay <= 0 || request.HourlyCostRate < 0)
        {
            return OperationResult<WorkCenterDto>.Failure("日可用产能必须大于 0，小时成本不能小于 0。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return OperationResult<WorkCenterDto>.Failure("未找到可用仓库。");
        }

        var actor = currentUser.GetActor();
        var workCenter = await dbContext.WorkCenters.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (workCenter is null)
        {
            workCenter = new WorkCenter(code, name, warehouse.Id, warehouse.Code, warehouse.Name, request.CapacityMinutesPerDay, request.HourlyCostRate, request.IsEnabled, actor);
            dbContext.WorkCenters.Add(workCenter);
        }
        else
        {
            workCenter.Update(name, warehouse.Id, warehouse.Code, warehouse.Name, request.CapacityMinutesPerDay, request.HourlyCostRate, request.IsEnabled, actor);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "WorkCenterUpserted", actor, code, cancellationToken);
        return OperationResult<WorkCenterDto>.Success(MapWorkCenter(workCenter));
    }

    /// <summary>
    /// 创建Routing。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ManufacturingRoutingDto>> CreateRoutingAsync(CreateManufacturingRoutingRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version) || request.Operations.Count == 0)
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("工艺路线版本和工序不能为空。");
        }

        if (request.Operations.Select(x => x.Sequence).Distinct().Count() != request.Operations.Count)
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("工序顺序不能重复。");
        }

        if (request.Operations.Any(x => x.Sequence <= 0 || x.StandardMinutes <= 0 || x.LaborCostRate < 0 || x.MachineCostRate < 0))
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("工序顺序、标准工时和成本参数无效。");
        }

        var item = await GetEnabledItemAsync(request.FinishedItemId, cancellationToken);
        if (item is null)
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("未找到可用成品物料。");
        }

        var version = NormalizeText(request.Version);
        if (await dbContext.ManufacturingRoutings.AnyAsync(x => x.FinishedItemId == item.Id && x.Version == version, cancellationToken))
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("该物料和版本的工艺路线已存在。");
        }

        var workCenterIds = request.Operations.Select(x => x.WorkCenterId).Distinct().ToList();
        var workCenters = await dbContext.WorkCenters.Where(x => workCenterIds.Contains(x.Id) && x.IsEnabled).ToListAsync(cancellationToken);
        if (workCenters.Count != workCenterIds.Count)
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("工序包含不存在或停用的工作中心。");
        }

        var actor = currentUser.GetActor();
        var routing = new ManufacturingRouting(NextNo("RT"), item.Id, item.Code, item.Name, version, actor, []);
        var operations = request.Operations
            .OrderBy(x => x.Sequence)
            .Select(x =>
            {
                var workCenter = workCenters.First(center => center.Id == x.WorkCenterId);
                return new RoutingOperation(
                    routing.Id,
                    x.Sequence,
                    NormalizeText(x.OperationCode).ToUpperInvariant(),
                    NormalizeText(x.OperationName),
                    workCenter.Id,
                    workCenter.Code,
                    workCenter.Name,
                    x.StandardMinutes,
                    x.LaborCostRate,
                    x.MachineCostRate);
            })
            .ToList();

        if (operations.Any(x => string.IsNullOrWhiteSpace(x.OperationCode) || string.IsNullOrWhiteSpace(x.OperationName)))
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("工序编码和名称不能为空。");
        }

        routing.ReplaceOperations(operations);
        dbContext.ManufacturingRoutings.Add(routing);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "RoutingCreated", actor, routing.RoutingNo, cancellationToken);
        return OperationResult<ManufacturingRoutingDto>.Success(MapRouting(routing));
    }

    /// <summary>
    /// Activate Routing Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ManufacturingRoutingDto>> ActivateRoutingAsync(Guid id, CancellationToken cancellationToken)
    {
        var routing = await dbContext.ManufacturingRoutings.Include(x => x.Operations).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (routing is null)
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("工艺路线不存在。");
        }

        if (routing.Operations.Count == 0)
        {
            return OperationResult<ManufacturingRoutingDto>.Failure("没有工序的工艺路线不能启用。");
        }

        routing.Activate();
        var actor = currentUser.GetActor();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "RoutingActivated", actor, routing.RoutingNo, cancellationToken);
        return OperationResult<ManufacturingRoutingDto>.Success(MapRouting(routing));
    }

    /// <summary>
    /// 创建Operation Schedule。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<OperationScheduleDto>> CreateOperationScheduleAsync(CreateOperationScheduleRequest request, CancellationToken cancellationToken)
    {
        if (request.PlannedQuantity <= 0 || request.PlannedEndUtc <= request.PlannedStartUtc)
        {
            return OperationResult<OperationScheduleDto>.Failure("计划数量和计划时间无效。");
        }

        var workOrder = await dbContext.WorkOrders.FirstOrDefaultAsync(x => x.Id == request.WorkOrderId, cancellationToken);
        var operation = await dbContext.RoutingOperations.FirstOrDefaultAsync(x => x.Id == request.RoutingOperationId, cancellationToken);
        if (workOrder is null || operation is null)
        {
            return OperationResult<OperationScheduleDto>.Failure("工单或工序不存在。");
        }

        var workCenter = await dbContext.WorkCenters.FirstOrDefaultAsync(x => x.Id == operation.WorkCenterId && x.IsEnabled, cancellationToken);
        if (workCenter is null)
        {
            return OperationResult<OperationScheduleDto>.Failure("工序工作中心不存在或已停用。");
        }

        var actor = currentUser.GetActor();
        var schedule = new OperationSchedule(
            NextNo("OS"),
            workOrder.Id,
            workOrder.WorkOrderNo,
            operation.Id,
            operation.OperationCode,
            operation.OperationName,
            workCenter.Id,
            workCenter.Code,
            workCenter.Name,
            request.PlannedStartUtc,
            request.PlannedEndUtc,
            request.PlannedQuantity,
            actor);
        dbContext.OperationSchedules.Add(schedule);
        await ReserveCapacityAsync(workCenter, schedule.ScheduleNo, request.PlannedStartUtc, request.PlannedEndUtc, actor, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "OperationScheduleCreated", actor, schedule.ScheduleNo, cancellationToken);
        return OperationResult<OperationScheduleDto>.Success(MapSchedule(schedule));
    }

    /// <summary>
    /// Release Operation Schedule Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<OperationScheduleDto>> ReleaseOperationScheduleAsync(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await dbContext.OperationSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (schedule is null)
        {
            return OperationResult<OperationScheduleDto>.Failure("工序计划不存在。");
        }

        if (schedule.Status != AdvancedManufacturingStatus.Planned)
        {
            return OperationResult<OperationScheduleDto>.Failure("只有计划状态的工序可以释放。");
        }

        schedule.Release();
        var actor = currentUser.GetActor();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "OperationScheduleReleased", actor, schedule.ScheduleNo, cancellationToken);
        return OperationResult<OperationScheduleDto>.Success(MapSchedule(schedule));
    }

    /// <summary>
    /// Complete Operation Schedule Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<OperationScheduleDto>> CompleteOperationScheduleAsync(Guid id, CompleteOperationScheduleRequest request, CancellationToken cancellationToken)
    {
        if (request.CompletedQuantity <= 0)
        {
            return OperationResult<OperationScheduleDto>.Failure("完工数量必须大于 0。");
        }

        var schedule = await dbContext.OperationSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (schedule is null)
        {
            return OperationResult<OperationScheduleDto>.Failure("工序计划不存在。");
        }

        if (request.CompletedQuantity > schedule.PlannedQuantity)
        {
            return OperationResult<OperationScheduleDto>.Failure("工序完工数量不能超过计划数量。");
        }

        schedule.Complete(request.CompletedQuantity);
        var actor = currentUser.GetActor();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "OperationScheduleCompleted", actor, schedule.ScheduleNo, cancellationToken);
        return OperationResult<OperationScheduleDto>.Success(MapSchedule(schedule));
    }

    /// <summary>
    /// Upsert Capacity Load Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<CapacityLoadDto>> UpsertCapacityLoadAsync(UpsertCapacityLoadRequest request, CancellationToken cancellationToken)
    {
        if (request.AvailableMinutes <= 0 || request.ReservedMinutes < 0)
        {
            return OperationResult<CapacityLoadDto>.Failure("可用产能必须大于 0，占用产能不能小于 0。");
        }

        var workCenter = await dbContext.WorkCenters.FirstOrDefaultAsync(x => x.Id == request.WorkCenterId && x.IsEnabled, cancellationToken);
        if (workCenter is null)
        {
            return OperationResult<CapacityLoadDto>.Failure("工作中心不存在或已停用。");
        }

        var actor = currentUser.GetActor();
        var load = await dbContext.CapacityLoads.FirstOrDefaultAsync(x => x.WorkCenterId == workCenter.Id && x.PlanDate == request.PlanDate, cancellationToken);
        if (load is null)
        {
            load = new CapacityLoad(workCenter.Id, workCenter.Code, workCenter.Name, request.PlanDate, request.AvailableMinutes, request.ReservedMinutes, NormalizeText(request.SourceDocumentNo), actor);
            dbContext.CapacityLoads.Add(load);
        }
        else
        {
            load.Update(request.AvailableMinutes, request.ReservedMinutes, NormalizeText(request.SourceDocumentNo), actor);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "CapacityLoadUpserted", actor, $"{workCenter.Code}:{request.PlanDate:yyyy-MM-dd}", cancellationToken);
        return OperationResult<CapacityLoadDto>.Success(MapCapacityLoad(load));
    }

    /// <summary>
    /// 创建Cost Snapshot。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<ManufacturingCostSnapshotDto>> CreateCostSnapshotAsync(CreateCostSnapshotRequest request, CancellationToken cancellationToken)
    {
        if (request.MaterialCost < 0 || request.LaborCost < 0 || request.MachineCost < 0 || request.OverheadCost < 0)
        {
            return OperationResult<ManufacturingCostSnapshotDto>.Failure("成本金额不能小于 0。");
        }

        var workOrder = await dbContext.WorkOrders.FirstOrDefaultAsync(x => x.Id == request.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            return OperationResult<ManufacturingCostSnapshotDto>.Failure("工单不存在。");
        }

        var actor = currentUser.GetActor();
        var snapshot = new ManufacturingCostSnapshot(
            NextNo("MC"),
            workOrder.Id,
            workOrder.WorkOrderNo,
            workOrder.FinishedItemId,
            workOrder.FinishedItemCode,
            workOrder.FinishedItemName,
            workOrder.PlannedQuantity,
            request.MaterialCost,
            request.LaborCost,
            request.MachineCost,
            request.OverheadCost,
            actor);
        dbContext.ManufacturingCostSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "CostSnapshotCreated", actor, snapshot.SnapshotNo, cancellationToken);
        return OperationResult<ManufacturingCostSnapshotDto>.Success(MapCostSnapshot(snapshot));
    }

    /// <summary>
    /// Generate Mrp Suggestion Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MrpSuggestionDto>> GenerateMrpSuggestionAsync(GenerateMrpSuggestionRequest request, CancellationToken cancellationToken)
    {
        if (request.DemandQuantity <= 0 || request.SupplyQuantity < 0)
        {
            return OperationResult<MrpSuggestionDto>.Failure("需求数量必须大于 0，供给数量不能小于 0。");
        }

        var warehouse = await GetEnabledWarehouseAsync(request.WarehouseId, cancellationToken);
        var item = await GetEnabledItemAsync(request.ItemId, cancellationToken);
        if (warehouse is null || item is null)
        {
            return OperationResult<MrpSuggestionDto>.Failure("仓库或物料不存在、已停用。");
        }

        var balance = await dbContext.StockBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WarehouseId == warehouse.Id && x.ItemId == item.Id, cancellationToken);
        var currentQuantity = balance?.Quantity ?? 0m;
        var suggestedQuantity = Math.Max(0m, request.DemandQuantity - currentQuantity - request.SupplyQuantity);
        var actor = currentUser.GetActor();
        var suggestion = new MrpSuggestion(
            NextNo("MRP"),
            item.Id,
            item.Code,
            item.Name,
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            currentQuantity,
            request.DemandQuantity,
            request.SupplyQuantity,
            suggestedQuantity,
            NormalizeText(request.SourceType),
            actor);

        dbContext.MrpSuggestions.Add(suggestion);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "MrpSuggestionGenerated", actor, suggestion.SuggestionNo, cancellationToken);
        return OperationResult<MrpSuggestionDto>.Success(MapMrpSuggestion(suggestion));
    }

    /// <summary>
    /// Decide Mrp Suggestion Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<MrpSuggestionDto>> DecideMrpSuggestionAsync(Guid id, DecideMrpSuggestionRequest request, CancellationToken cancellationToken)
    {
        var suggestion = await dbContext.MrpSuggestions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (suggestion is null)
        {
            return OperationResult<MrpSuggestionDto>.Failure("MRP 建议不存在。");
        }

        if (suggestion.Status != AdvancedManufacturingStatus.Open)
        {
            return OperationResult<MrpSuggestionDto>.Failure("只有待处理 MRP 建议可以决策。");
        }

        var decision = NormalizeText(request.Decision);
        if (decision is not AdvancedManufacturingStatus.Accepted and not AdvancedManufacturingStatus.Ignored)
        {
            return OperationResult<MrpSuggestionDto>.Failure("MRP 决策只能是 Accepted 或 Ignored。");
        }

        var actor = currentUser.GetActor();
        suggestion.Decide(decision, NormalizeText(request.Note), actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AdvancedManufacturing", "MrpSuggestionDecided", actor, $"{suggestion.SuggestionNo}:{decision}", cancellationToken);
        return OperationResult<MrpSuggestionDto>.Success(MapMrpSuggestion(suggestion));
    }

    /// <summary>
    /// Reserve Capacity Async。
    /// </summary>
    /// <param name="workCenter">work Center 参数。</param>
    /// <param name="scheduleNo">schedule No 参数。</param>
    /// <param name="startUtc">start Utc 参数。</param>
    /// <param name="endUtc">end Utc 参数。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task ReserveCapacityAsync(WorkCenter workCenter, string scheduleNo, DateTimeOffset startUtc, DateTimeOffset endUtc, string actor, CancellationToken cancellationToken)
    {
        var planDate = DateOnly.FromDateTime(startUtc.UtcDateTime);
        var reservedMinutes = (decimal)(endUtc - startUtc).TotalMinutes;
        var load = await dbContext.CapacityLoads.FirstOrDefaultAsync(x => x.WorkCenterId == workCenter.Id && x.PlanDate == planDate, cancellationToken);
        if (load is null)
        {
            dbContext.CapacityLoads.Add(new CapacityLoad(workCenter.Id, workCenter.Code, workCenter.Name, planDate, workCenter.CapacityMinutesPerDay, reservedMinutes, scheduleNo, actor));
            return;
        }

        load.Update(load.AvailableMinutes, load.ReservedMinutes + reservedMinutes, scheduleNo, actor);
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
    /// 注册Work Center 路由。
    /// </summary>
    /// <param name="workCenter">work Center 参数。</param>
    private static WorkCenterDto MapWorkCenter(WorkCenter workCenter) =>
        new(workCenter.Id, workCenter.Code, workCenter.Name, workCenter.WarehouseId, workCenter.WarehouseName, workCenter.CapacityMinutesPerDay, workCenter.HourlyCostRate, workCenter.IsEnabled, workCenter.UpdatedBy, workCenter.UpdatedAtUtc);

    /// <summary>
    /// 注册Routing 路由。
    /// </summary>
    /// <param name="routing">工艺路线。</param>
    private static ManufacturingRoutingDto MapRouting(ManufacturingRouting routing) =>
        new(
            routing.Id,
            routing.RoutingNo,
            routing.FinishedItemId,
            routing.FinishedItemCode,
            routing.FinishedItemName,
            routing.Version,
            routing.Status,
            routing.CreatedBy,
            routing.Operations.OrderBy(x => x.Sequence).Select(MapOperation).ToList(),
            routing.UpdatedAtUtc);

    /// <summary>
    /// 注册Operation 路由。
    /// </summary>
    /// <param name="operation">工序对象。</param>
    private static RoutingOperationDto MapOperation(RoutingOperation operation) =>
        new(operation.Id, operation.Sequence, operation.OperationCode, operation.OperationName, operation.WorkCenterId, operation.WorkCenterCode, operation.WorkCenterName, operation.StandardMinutes, operation.LaborCostRate, operation.MachineCostRate);

    /// <summary>
    /// 注册Schedule 路由。
    /// </summary>
    /// <param name="schedule">工序计划。</param>
    private static OperationScheduleDto MapSchedule(OperationSchedule schedule) =>
        new(schedule.Id, schedule.ScheduleNo, schedule.WorkOrderId, schedule.WorkOrderNo, schedule.RoutingOperationId, schedule.OperationCode, schedule.OperationName, schedule.WorkCenterId, schedule.WorkCenterCode, schedule.WorkCenterName, schedule.PlannedStartUtc, schedule.PlannedEndUtc, schedule.PlannedQuantity, schedule.CompletedQuantity, schedule.Status, schedule.ScheduledBy, schedule.UpdatedAtUtc);

    /// <summary>
    /// 注册Capacity Load 路由。
    /// </summary>
    /// <param name="load">产能负载。</param>
    private static CapacityLoadDto MapCapacityLoad(CapacityLoad load) =>
        new(load.Id, load.WorkCenterId, load.WorkCenterCode, load.WorkCenterName, load.PlanDate, load.AvailableMinutes, load.ReservedMinutes, load.RemainingMinutes, load.SourceDocumentNo, load.UpdatedBy, load.UpdatedAtUtc);

    /// <summary>
    /// 注册Cost Snapshot 路由。
    /// </summary>
    /// <param name="snapshot">成本快照。</param>
    private static ManufacturingCostSnapshotDto MapCostSnapshot(ManufacturingCostSnapshot snapshot) =>
        new(snapshot.Id, snapshot.SnapshotNo, snapshot.WorkOrderId, snapshot.WorkOrderNo, snapshot.FinishedItemId, snapshot.FinishedItemCode, snapshot.FinishedItemName, snapshot.PlannedQuantity, snapshot.MaterialCost, snapshot.LaborCost, snapshot.MachineCost, snapshot.OverheadCost, snapshot.TotalCost, snapshot.CreatedBy, snapshot.CreatedAtUtc);

    /// <summary>
    /// 注册Mrp Suggestion 路由。
    /// </summary>
    /// <param name="suggestion">计划建议。</param>
    private static MrpSuggestionDto MapMrpSuggestion(MrpSuggestion suggestion) =>
        new(suggestion.Id, suggestion.SuggestionNo, suggestion.ItemId, suggestion.ItemCode, suggestion.ItemName, suggestion.WarehouseId, suggestion.WarehouseCode, suggestion.WarehouseName, suggestion.CurrentQuantity, suggestion.DemandQuantity, suggestion.SupplyQuantity, suggestion.SuggestedQuantity, suggestion.SourceType, suggestion.Status, suggestion.CreatedBy, suggestion.DecidedBy, suggestion.DecisionNote, suggestion.DecidedAtUtc, suggestion.UpdatedAtUtc);

    /// <summary>
    /// 注册Work Order Option 路由。
    /// </summary>
    /// <param name="workOrder">work Order 参数。</param>
    private static AdvancedManufacturingWorkOrderOptionDto MapWorkOrderOption(WorkOrder workOrder) =>
        new(workOrder.Id, workOrder.WorkOrderNo, workOrder.FinishedItemId, workOrder.FinishedItemCode, workOrder.FinishedItemName, workOrder.PlannedQuantity, workOrder.Unit, workOrder.Status);
}
