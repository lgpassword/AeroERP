using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Procurement.Contracts;

namespace AeroERP.Modules.Procurement.Services;

/// <summary>
/// Procurement Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IProcurementService
{
    /// <summary>
    /// 查询Requests。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ProcurementRequestDto>> ListRequestsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Request。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ProcurementRequestDto>> CreateRequestAsync(CreateProcurementRequestRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Decide Request。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ProcurementRequestDto>> DecideRequestAsync(Guid id, DecideProcurementRequestRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Convert To Order。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ProcurementOrderDto>> ConvertToOrderAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ProcurementOrderDto>> ListOrdersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 下达Order。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ProcurementOrderDto>> ReleaseOrderAsync(Guid id, CancellationToken cancellationToken);
}
