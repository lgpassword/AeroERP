namespace AeroERP.Platform.Contracts;

/// <summary>
/// Module Visibility 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Key">业务键。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="IsVisible">是否可见。</param>
/// <param name="Category">业务分类。</param>
public sealed record ModuleVisibilityDto(Guid Id, string Key, string DisplayName, bool IsVisible, string Category);
/// <summary>
/// Toggle Module Visibility 请求参数。
/// </summary>
/// <param name="IsVisible">是否可见。</param>
public sealed record ToggleModuleVisibilityRequest(bool IsVisible);

/// <summary>
/// Agent Review 数据传输对象。
/// </summary>
public sealed record AgentReviewDto(
    Guid Id,
    string AgentName,
    string ActionName,
    string Payload,
    string Status,
    string RequestedBy,
    string? ReviewedBy,
    string? ReviewerComment,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? ReviewedAtUtc);

/// <summary>
/// Submit Agent Review 请求参数。
/// </summary>
/// <param name="AgentName">Agent Name 参数。</param>
/// <param name="ActionName">Action Name 参数。</param>
/// <param name="Payload">业务载荷。</param>
public sealed record SubmitAgentReviewRequest(string AgentName, string ActionName, string Payload);
/// <summary>
/// Decide Agent Review 请求参数。
/// </summary>
/// <param name="Decision">处理决策。</param>
/// <param name="ReviewerComment">Reviewer Comment 参数。</param>
public sealed record DecideAgentReviewRequest(string Decision, string? ReviewerComment);

/// <summary>
/// Organization Summary 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Name">显示名称。</param>
/// <param name="DefaultRole">Default Role 参数。</param>
/// <param name="RegionCode">Region Code 参数。</param>
public sealed record OrganizationSummaryDto(Guid Id, string Name, string DefaultRole, string RegionCode);
/// <summary>
/// Create Organization 请求参数。
/// </summary>
/// <param name="Name">显示名称。</param>
/// <param name="DefaultRole">Default Role 参数。</param>
/// <param name="RegionCode">Region Code 参数。</param>
public sealed record CreateOrganizationRequest(string Name, string DefaultRole, string RegionCode);

/// <summary>
/// Role Summary 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="Key">业务键。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="ModuleKeys">Module Keys 参数。</param>
public sealed record RoleSummaryDto(Guid Id, string Key, string DisplayName, IReadOnlyList<string> ModuleKeys);
/// <summary>
/// User Summary 数据传输对象。
/// </summary>
/// <param name="Id">业务对象标识。</param>
/// <param name="UserName">登录用户名。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="Roles">角色集合。</param>
public sealed record UserSummaryDto(Guid Id, string UserName, string DisplayName, bool IsEnabled, IReadOnlyList<RoleSummaryDto> Roles);
/// <summary>
/// Current User 数据传输对象。
/// </summary>
public sealed record CurrentUserDto(
    Guid Id,
    string UserName,
    string DisplayName,
    bool IsEnabled,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> RoleDisplayNames,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> VisibleModuleKeys);
/// <summary>
/// Login 请求参数。
/// </summary>
/// <param name="UserName">登录用户名。</param>
/// <param name="Password">登录密码。</param>
public sealed record LoginRequest(string UserName, string Password);
/// <summary>
/// Login Response 数据记录。
/// </summary>
/// <param name="AccessToken">Access Token 参数。</param>
/// <param name="ExpiresAtUtc">Expires At Utc 参数。</param>
/// <param name="User">用户实体。</param>
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, CurrentUserDto User);
/// <summary>
/// Create User 请求参数。
/// </summary>
/// <param name="UserName">登录用户名。</param>
/// <param name="DisplayName">界面显示名称。</param>
/// <param name="Password">登录密码。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="RoleIds">Role Ids 参数。</param>
public sealed record CreateUserRequest(string UserName, string DisplayName, string Password, bool IsEnabled, IReadOnlyList<Guid> RoleIds);
/// <summary>
/// Update User Roles 请求参数。
/// </summary>
/// <param name="RoleIds">Role Ids 参数。</param>
public sealed record UpdateUserRolesRequest(IReadOnlyList<Guid> RoleIds);
/// <summary>
/// Update User Status 请求参数。
/// </summary>
/// <param name="IsEnabled">是否启用。</param>
public sealed record UpdateUserStatusRequest(bool IsEnabled);
/// <summary>
/// Reset User Password 请求参数。
/// </summary>
/// <param name="NewPassword">New Password 参数。</param>
public sealed record ResetUserPasswordRequest(string NewPassword);
/// <summary>
/// Change Password 请求参数。
/// </summary>
/// <param name="CurrentPassword">Current Password 参数。</param>
/// <param name="NewPassword">New Password 参数。</param>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
/// <summary>
/// Update Module Access 请求参数。
/// </summary>
/// <param name="ModuleKeys">Module Keys 参数。</param>
public sealed record UpdateModuleAccessRequest(IReadOnlyList<string> ModuleKeys);
