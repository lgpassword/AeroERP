namespace AeroERP.Platform.Services;

/// <summary>
/// Current User Accessor 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface ICurrentUserAccessor
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string UserName { get; }
    string DisplayName { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasRole(string roleKey);
    bool HasPermission(string permission);
    bool CanAccessModule(string moduleKey);
}
