using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Control.Contracts;

namespace AeroERP.Modules.Control.Services;

/// <summary>
/// Control Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IControlService
{
    /// <summary>
    /// 获取Analytics。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<AnalyticsSnapshotDto> GetAnalyticsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Data Scope Rules。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<DataScopeRuleDto>> ListDataScopeRulesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Data Scope Rule。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<DataScopeRuleDto>> UpsertDataScopeRuleAsync(UpsertDataScopeRuleRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Numbering Rules。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<NumberingRuleDto>> ListNumberingRulesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Numbering Rule。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<NumberingRuleDto>> UpsertNumberingRuleAsync(UpsertNumberingRuleRequest request, CancellationToken cancellationToken);
}
