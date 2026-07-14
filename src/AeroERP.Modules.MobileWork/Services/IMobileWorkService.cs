using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.MobileWork.Contracts;

namespace AeroERP.Modules.MobileWork.Services;

/// <summary>
/// Mobile Work Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IMobileWorkService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<MobileWorkOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Device。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MobileWorkDeviceDto>> UpsertDeviceAsync(UpsertMobileDeviceRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 创建Offline Task。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MobileWorkOfflineTaskDto>> CreateOfflineTaskAsync(CreateMobileOfflineTaskRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Mark Offline Task Synced。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MobileWorkOfflineTaskDto>> MarkOfflineTaskSyncedAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Offline Task。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MobileWorkOfflineTaskDto>> CompleteOfflineTaskAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Record Scan Event。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<MobileWorkScanEventDto>> RecordScanEventAsync(RecordMobileScanEventRequest request, CancellationToken cancellationToken);
}
