using System.Security.Claims;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Services;
using Microsoft.AspNetCore.Http;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Current User Accessor 业务对象。
/// </summary>
/// <param name="httpContextAccessor">http Context Accessor 参数。</param>
public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var value) ? value : null;
        }
    }

    public string UserName => Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public string DisplayName => Principal?.FindFirstValue(PlatformClaimTypes.DisplayName) ?? UserName;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList() ?? [];

    public IReadOnlyList<string> Permissions =>
        Principal?.FindAll(PlatformClaimTypes.Permission).Select(x => x.Value).ToList() ?? [];

    /// <summary>
    /// 判断是否存在Role。
    /// </summary>
    /// <param name="roleKey">role Key 参数。</param>
    public bool HasRole(string roleKey) =>
        Roles.Any(x => string.Equals(x, roleKey, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 判断是否存在Permission。
    /// </summary>
    /// <param name="permission">权限编码。</param>
    public bool HasPermission(string permission) =>
        Permissions.Any(x => string.Equals(x, permission, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 判断是否允许Access Module。
    /// </summary>
    /// <param name="moduleKey">模块键。</param>
    public bool CanAccessModule(string moduleKey)
    {
        if (HasRole(PlatformRoleCatalog.PlatformAdmin))
        {
            return true;
        }

        return Principal?.FindAll(PlatformClaimTypes.Module)
            .Any(x => string.Equals(x.Value, moduleKey, StringComparison.OrdinalIgnoreCase)) ?? false;
    }
}
