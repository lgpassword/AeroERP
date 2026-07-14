using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Sales.Contracts;

namespace AeroERP.Modules.Sales.Services;

/// <summary>
/// Sales Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface ISalesService
{
    /// <summary>
    /// 查询Quotations。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<SalesQuotationDto>> ListQuotationsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建Quotation。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<SalesQuotationDto>> CreateQuotationAsync(CreateSalesQuotationRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Convert To Order。
    /// </summary>
    /// <param name="quotationId">quotation Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<SalesOrderDto>> ConvertToOrderAsync(Guid quotationId, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Orders。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<SalesOrderDto>> ListOrdersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Confirm Order。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<SalesOrderDto>> ConfirmOrderAsync(Guid orderId, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Mark Order Ready To Ship。
    /// </summary>
    /// <param name="orderId">order Id 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<SalesOrderDto>> MarkOrderReadyToShipAsync(Guid orderId, CancellationToken cancellationToken);
}
