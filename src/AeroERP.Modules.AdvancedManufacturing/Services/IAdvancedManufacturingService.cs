using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.AdvancedManufacturing.Contracts;

namespace AeroERP.Modules.AdvancedManufacturing.Services;

/// <summary>
/// Advanced Manufacturing Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IAdvancedManufacturingService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<AdvancedManufacturingOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Work Center。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<WorkCenterDto>> UpsertWorkCenterAsync(UpsertWorkCenterRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Routing。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ManufacturingRoutingDto>> CreateRoutingAsync(CreateManufacturingRoutingRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Activate Routing。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ManufacturingRoutingDto>> ActivateRoutingAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Operation Schedule。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<OperationScheduleDto>> CreateOperationScheduleAsync(CreateOperationScheduleRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 下达Operation Schedule。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<OperationScheduleDto>> ReleaseOperationScheduleAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Operation Schedule。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<OperationScheduleDto>> CompleteOperationScheduleAsync(Guid id, CompleteOperationScheduleRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Capacity Load。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<CapacityLoadDto>> UpsertCapacityLoadAsync(UpsertCapacityLoadRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Cost Snapshot。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ManufacturingCostSnapshotDto>> CreateCostSnapshotAsync(CreateCostSnapshotRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 生成Mrp Suggestion。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MrpSuggestionDto>> GenerateMrpSuggestionAsync(GenerateMrpSuggestionRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Decide Mrp Suggestion。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MrpSuggestionDto>> DecideMrpSuggestionAsync(Guid id, DecideMrpSuggestionRequest request, CancellationToken cancellationToken);
}
