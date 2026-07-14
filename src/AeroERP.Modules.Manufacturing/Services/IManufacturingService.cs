using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Manufacturing.Contracts;

namespace AeroERP.Modules.Manufacturing.Services;

/// <summary>
/// Manufacturing Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IManufacturingService
{
    /// <summary>
    /// 查询Boms。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<BomDto>> ListBomsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Bom。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<BomDto>> CreateBomAsync(CreateBomRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Work Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<WorkOrderDto>> ListWorkOrdersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Work Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<WorkOrderDto>> CreateWorkOrderAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 下达Work Order。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<WorkOrderDto>> ReleaseWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Production Issues。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ProductionIssueDto>> ListProductionIssuesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Execute Production Issue。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ProductionIssueDto>> ExecuteProductionIssueAsync(Guid workOrderId, ExecuteProductionIssueRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Production Receipts。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ProductionReceiptDto>> ListProductionReceiptsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Complete Production。
    /// </summary>
    /// <param name="workOrderId">work Order Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<ProductionReceiptDto>> CompleteProductionAsync(Guid workOrderId, CompleteProductionRequest request, CancellationToken cancellationToken);
}
