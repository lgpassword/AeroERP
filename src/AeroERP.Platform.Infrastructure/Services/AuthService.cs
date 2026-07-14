using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AeroERP.Platform.Contracts;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Auth Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
public sealed class AuthService(
    AeroErpDbContext dbContext,
    IConfiguration configuration,
    PasswordHasher<AppUser> passwordHasher) : IAuthService
{
    /// <summary>
    /// Login Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == request.UserName, cancellationToken);

        if (user is null || !user.IsEnabled)
        {
            return null;
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var currentUser = await GetCurrentUserAsync(user.Id, cancellationToken);
        if (currentUser is null)
        {
            return null;
        }

        return CreateResponse(user, currentUser);
    }

    /// <summary>
    /// 获取Current User。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.RoleAssignments)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleIds = user.RoleAssignments.Select(x => x.RoleId).ToList();
        var roles = await dbContext.Roles
            .Where(x => roleIds.Contains(x.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sortedRoles = roles.OrderBy(x => x.DisplayName).ToList();
        var visibleModuleKeys = await ResolveVisibleModulesAsync(sortedRoles, cancellationToken);
        var permissions = PlatformRoleCatalog.ResolvePermissions(sortedRoles.Select(x => x.Key))
            .Concat(await ResolveCustomPermissionsAsync(roleIds, cancellationToken))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
        return new CurrentUserDto(
            user.Id,
            user.UserName,
            user.DisplayName,
            user.IsEnabled,
            sortedRoles.Select(x => x.Key).ToList(),
            sortedRoles.Select(x => x.DisplayName).ToList(),
            permissions,
            visibleModuleKeys);
    }

    /// <summary>
    /// 创建Response。
    /// </summary>
    /// <param name="user">用户实体。</param>
    /// <param name="currentUser">current User 参数。</param>
    private LoginResponse CreateResponse(AppUser user, CurrentUserDto currentUser)
    {
        var key = configuration["Auth:Jwt:Key"] ?? "AeroERP_Local_Dev_Key_Change_Me_Immediately_2026";
        var issuer = configuration["Auth:Jwt:Issuer"] ?? "AeroERP";
        var audience = configuration["Auth:Jwt:Audience"] ?? "AeroERP.Web";
        var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(PlatformClaimTypes.DisplayName, currentUser.DisplayName)
        };

        claims.AddRange(currentUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(currentUser.Permissions.Select(permission => new Claim(PlatformClaimTypes.Permission, permission)));
        claims.AddRange(currentUser.VisibleModuleKeys.Select(module => new Claim(PlatformClaimTypes.Module, module)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc, currentUser);
    }

    /// <summary>
    /// Resolve Visible Modules Async。
    /// </summary>
    /// <param name="roles">角色集合。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<string>> ResolveVisibleModulesAsync(IEnumerable<AppRole> roles, CancellationToken cancellationToken)
    {
        var allowedModules = roles
            .SelectMany(x => x.ModuleAccesses)
            .Select(x => x.ModuleKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (allowedModules.Count == 0)
        {
            return [];
        }

        return await dbContext.PluginModules
            .Where(x => x.IsVisible && allowedModules.Contains(x.Key))
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DisplayName)
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Resolve Custom Permissions Async。
    /// </summary>
    /// <param name="roleIds">role Ids 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<IReadOnlyList<string>> ResolveCustomPermissionsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        return await dbContext.RolePermissionGrants
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => x.Permission)
            .ToListAsync(cancellationToken);
    }
}
