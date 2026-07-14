using AeroERP.Platform.Contracts;

namespace AeroERP.Platform.Services;

/// <summary>
/// Agent Review Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IAgentReviewService
{
    /// <summary>
    /// 查询业务对象。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<AgentReviewDto>> ListAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 提交业务对象。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<AgentReviewDto> SubmitAsync(SubmitAgentReviewRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Decide。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<AgentReviewDto?> DecideAsync(Guid id, DecideAgentReviewRequest request, CancellationToken cancellationToken);
}
