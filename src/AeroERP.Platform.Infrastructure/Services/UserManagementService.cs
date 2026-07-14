using AeroERP.Platform.Contracts;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// User Management Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class UserManagementService(
    AeroErpDbContext dbContext,
    PasswordHasher<AppUser> passwordHasher,
    IAuditWriter auditWriter,
    ICurrentUserAccessor currentUser) : IUserManagementService
{
    /// <summary>
    /// 查询Roles。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<RoleSummaryDto>> ListRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return roles.Select(MapRole).ToList();
    }

    /// <summary>
    /// 查询Users。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<UserSummaryDto>> ListUsersAsync(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Include(x => x.RoleAssignments)
            .AsNoTracking()
            .OrderBy(x => x.UserName)
            .ToListAsync(cancellationToken);

        var roles = await dbContext.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return users.Select(user =>
        {
            var roleMap = roles.Where(role => user.RoleAssignments.Any(x => x.RoleId == role.Id)).Select(MapRole).ToList();
            return new UserSummaryDto(user.Id, user.UserName, user.DisplayName, user.IsEnabled, roleMap);
        }).ToList();
    }

    /// <summary>
    /// 创建User。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<UserSummaryDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await EnsureUserNameAvailableAsync(request.UserName, null, cancellationToken);
        var roleIds = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
        var user = new AppUser(request.UserName.Trim(), request.DisplayName.Trim(), string.Empty, request.IsEnabled);
        user.SetPasswordHash(passwordHasher.HashPassword(user, request.Password));
        user.UpdateRoles(roleIds);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Identity", "UserCreated", currentUser.GetActor(), user.UserName, cancellationToken);

        var roles = await dbContext.Roles.Where(x => roleIds.Contains(x.Id)).ToListAsync(cancellationToken);
        return new UserSummaryDto(user.Id, user.UserName, user.DisplayName, user.IsEnabled, roles.Select(MapRole).ToList());
    }

    /// <summary>
    /// 更新User Status。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<UserSummaryDto?> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.RoleAssignments)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (currentUser.UserId == user.Id && !request.IsEnabled)
        {
            throw new InvalidOperationException("不能停用当前登录账号。");
        }

        user.SetEnabled(request.IsEnabled);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Identity", "UserStatusUpdated", currentUser.GetActor(), $"{user.UserName}:{request.IsEnabled}", cancellationToken);

        var roleIds = user.RoleAssignments.Select(x => x.RoleId).ToList();
        var roles = await dbContext.Roles.Where(x => roleIds.Contains(x.Id)).ToListAsync(cancellationToken);
        return new UserSummaryDto(user.Id, user.UserName, user.DisplayName, user.IsEnabled, roles.Select(MapRole).ToList());
    }

    /// <summary>
    /// 更新User Roles。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<UserSummaryDto?> UpdateUserRolesAsync(Guid userId, UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        var roleIds = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
        var user = await dbContext.Users
            .Include(x => x.RoleAssignments)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.UpdateRoles(roleIds);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Identity", "UserRolesUpdated", currentUser.GetActor(), user.UserName, cancellationToken);

        var roles = await dbContext.Roles.Where(x => roleIds.Contains(x.Id)).ToListAsync(cancellationToken);
        return new UserSummaryDto(user.Id, user.UserName, user.DisplayName, user.IsEnabled, roles.Select(MapRole).ToList());
    }

    /// <summary>
    /// Reset User Password Async。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<bool> ResetUserPasswordAsync(Guid userId, ResetUserPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        ValidatePassword(request.NewPassword);
        user.SetPasswordHash(passwordHasher.HashPassword(user, request.NewPassword));
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Identity", "UserPasswordReset", currentUser.GetActor(), user.UserName, cancellationToken);
        return true;
    }

    /// <summary>
    /// Change Current User Password Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task ChangeCurrentUserPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new InvalidOperationException("当前未登录。");
        }

        ValidatePassword(request.NewPassword);
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUser.UserId.Value, cancellationToken)
            ?? throw new InvalidOperationException("当前账号不存在。");

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException("当前密码不正确。");
        }

        user.SetPasswordHash(passwordHasher.HashPassword(user, request.NewPassword));
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Identity", "CurrentUserPasswordChanged", currentUser.GetActor(), user.UserName, cancellationToken);
    }

    /// <summary>
    /// 更新Role Modules。
    /// </summary>
    /// <param name="roleId">角色标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<RoleSummaryDto?> UpdateRoleModulesAsync(Guid roleId, UpdateModuleAccessRequest request, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(x => x.ModuleAccesses)
            .FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);

        if (role is null)
        {
            return null;
        }

        var moduleKeys = request.ModuleKeys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingModules = await dbContext.PluginModules
            .Where(x => moduleKeys.Contains(x.Key))
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);

        if (existingModules.Count != moduleKeys.Count)
        {
            throw new InvalidOperationException("存在无效模块键。");
        }

        if (string.Equals(role.Key, PlatformRoleCatalog.PlatformAdmin, StringComparison.OrdinalIgnoreCase)
            && !moduleKeys.Contains("platform", StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("平台管理员角色必须保留平台治理模块权限。");
        }

        role.SetModuleAccess(moduleKeys);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Identity", "RoleModulesUpdated", currentUser.GetActor(), role.Key, cancellationToken);
        return MapRole(role);
    }

    /// <summary>
    /// Ensure User Name Available Async。
    /// </summary>
    /// <param name="userName">登录用户名。</param>
    /// <param name="userId">用户标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task EnsureUserNameAvailableAsync(string userName, Guid? userId, CancellationToken cancellationToken)
    {
        var normalized = userName.Trim();
        var exists = await dbContext.Users.AnyAsync(
            x => x.UserName == normalized && (!userId.HasValue || x.Id != userId.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("登录账号已存在。");
        }
    }

    /// <summary>
    /// Validate Password。
    /// </summary>
    /// <param name="password">登录密码。</param>
    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 8)
        {
            throw new InvalidOperationException("密码长度不能少于 8 位。");
        }
    }

    /// <summary>
    /// Validate Role Ids Async。
    /// </summary>
    /// <param name="roleIds">role Ids 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<List<Guid>> ValidateRoleIdsAsync(IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken)
    {
        var distinctIds = roleIds.Distinct().ToList();
        var count = await dbContext.Roles.CountAsync(x => distinctIds.Contains(x.Id), cancellationToken);
        if (count != distinctIds.Count)
        {
            throw new InvalidOperationException("存在无效角色。");
        }

        return distinctIds;
    }

    /// <summary>
    /// 注册Role 路由。
    /// </summary>
    /// <param name="role">角色实体。</param>
    private static RoleSummaryDto MapRole(AppRole role) =>
        new(role.Id, role.Key, role.DisplayName, role.ModuleAccesses.Select(x => x.ModuleKey).OrderBy(x => x).ToList());
}
