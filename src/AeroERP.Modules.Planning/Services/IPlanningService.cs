using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Planning.Contracts;

namespace AeroERP.Modules.Planning.Services;

/// <summary>
/// Planning Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IPlanningService
{
    /// <summary>
    /// 查询Suggestions。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<PlanningSuggestionDto>> ListSuggestionsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 生成Suggestion。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PlanningSuggestionDto>> GenerateSuggestionAsync(GeneratePlanningSuggestionRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Decide Suggestion。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PlanningSuggestionDto>> DecideSuggestionAsync(Guid id, PlanningSuggestionDecisionRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Outsourcing Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<OutsourcingOrderDto>> ListOutsourcingOrdersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Outsourcing Order。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<OutsourcingOrderDto>> CreateOutsourcingOrderAsync(CreateOutsourcingOrderRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Issue Outsourcing Materials。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<OutsourcingOrderDto>> IssueOutsourcingMaterialsAsync(Guid orderId, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Receive Outsourcing Order。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<OutsourcingOrderDto>> ReceiveOutsourcingOrderAsync(Guid orderId, ReceiveOutsourcingOrderRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Barcode Executions。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<BarcodeExecutionDto>> ListBarcodeExecutionsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Execute Barcode。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<BarcodeExecutionDto>> ExecuteBarcodeAsync(BarcodeExecutionRequest request, CancellationToken cancellationToken);
}
