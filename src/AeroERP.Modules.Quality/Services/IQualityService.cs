using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Quality.Contracts;

namespace AeroERP.Modules.Quality.Services;

/// <summary>
/// Quality Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IQualityService
{
    /// <summary>
    /// 查询Source Candidates。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<QualitySourceCandidateDto>> ListSourceCandidatesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Inspections。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<QualityInspectionDto>> ListInspectionsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Inspection。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<QualityInspectionDto>> CreateInspectionAsync(CreateQualityInspectionRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Lot Trace Events。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<LotTraceEventDto>> ListLotTraceEventsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Lot Trace Event。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<LotTraceEventDto>> CreateLotTraceEventAsync(CreateLotTraceEventRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 获取Lot Trace。
    /// </summary>
    /// <param name="lotNo">lot No 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<LotTraceDto> GetLotTraceAsync(string lotNo, CancellationToken cancellationToken);
}
