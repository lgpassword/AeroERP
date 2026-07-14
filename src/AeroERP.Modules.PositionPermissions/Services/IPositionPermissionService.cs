using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.PositionPermissions.Contracts;

namespace AeroERP.Modules.PositionPermissions.Services;

/// <summary>
/// Position Permission Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IPositionPermissionService
{
    /// <summary>
    /// 获取Overview。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<PositionPermissionOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Department。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<DepartmentDto>> UpsertDepartmentAsync(UpsertDepartmentRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Position。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<JobPositionDto>> UpsertPositionAsync(UpsertJobPositionRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Custom Role。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PositionRoleDto>> UpsertCustomRoleAsync(UpsertCustomRoleRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Permission Package。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<PermissionPackageDto>> UpsertPermissionPackageAsync(UpsertPermissionPackageRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新Position Role Bindings。
    /// </summary>
    /// <param name="positionId">position Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<IReadOnlyList<PositionRoleBindingDto>>> UpdatePositionRoleBindingsAsync(Guid positionId, UpdatePositionRoleBindingsRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新Position Data Scope Rules。
    /// </summary>
    /// <param name="positionId">position Id 参数。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<IReadOnlyList<PositionDataScopeRuleDto>>> UpdatePositionDataScopeRulesAsync(Guid positionId, UpdatePositionDataScopeRulesRequest request, CancellationToken cancellationToken);
}
