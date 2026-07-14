using AeroERP.Platform.Contracts;

namespace AeroERP.Platform.Services;

/// <summary>
/// User Management Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// 查询Roles。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<RoleSummaryDto>> ListRolesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Users。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<UserSummaryDto>> ListUsersAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建User。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<UserSummaryDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新User Roles。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<UserSummaryDto?> UpdateUserRolesAsync(Guid userId, UpdateUserRolesRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新User Status。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<UserSummaryDto?> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Reset User Password。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<bool> ResetUserPasswordAsync(Guid userId, ResetUserPasswordRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 执行Change Current User Password。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task ChangeCurrentUserPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 更新Role Modules。
    /// </summary>
    /// <param name="roleId">角色标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<RoleSummaryDto?> UpdateRoleModulesAsync(Guid roleId, UpdateModuleAccessRequest request, CancellationToken cancellationToken);
}
