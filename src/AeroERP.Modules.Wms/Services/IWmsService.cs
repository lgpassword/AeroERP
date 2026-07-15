using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Wms.Contracts;

namespace AeroERP.Modules.Wms.Services;

/// <summary>
/// Wms Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IWmsService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<WmsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Container。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<WarehouseContainerDto>> UpsertContainerAsync(UpsertWarehouseContainerRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Route。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<WarehouseRouteDto>> UpsertRouteAsync(UpsertWarehouseRouteRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Put Away Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PutAwayTaskDto>> CreatePutAwayTaskAsync(CreatePutAwayTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Put Away Task。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PutAwayTaskDto>> CompletePutAwayTaskAsync(Guid id, CompletePutAwayTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Picking Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PickingTaskDto>> CreatePickingTaskAsync(CreatePickingTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Picking Task。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PickingTaskDto>> CompletePickingTaskAsync(Guid id, CompletePickingTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Wave。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PickingWaveDto>> CreateWaveAsync(CreatePickingWaveRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 下达Wave。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PickingWaveDto>> ReleaseWaveAsync(Guid id, CancellationToken cancellationToken);
}
