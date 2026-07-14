namespace AeroERP.Modules.PositionPermissions.Contracts;

/// <summary>
/// Department 数据传输对象。
/// </summary>
public sealed record DepartmentDto(
    Guid Id,
    string Code,
    string Name,
    Guid? ParentDepartmentId,
    bool IsEnabled,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Job Position 数据传输对象。
/// </summary>
public sealed record JobPositionDto(
    Guid Id,
    string Code,
    string Name,
    Guid DepartmentId,
    string DepartmentName,
    string Description,
    bool IsEnabled,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Permission Package 数据传输对象。
/// </summary>
public sealed record PermissionPackageDto(
    Guid Id,
    string DisplayName,
    string Description,
    IReadOnlyList<string> ModuleKeys,
    IReadOnlyList<string> Permissions,
    bool IsEnabled,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Position Role 数据传输对象。
/// </summary>
public sealed record PositionRoleDto(
    Guid Id,
    string DisplayName,
    bool IsSystemProtected,
    IReadOnlyList<string> ModuleKeys,
    IReadOnlyList<string> Permissions);

/// <summary>
/// Position Role Binding 数据传输对象。
/// </summary>
public sealed record PositionRoleBindingDto(
    Guid Id,
    Guid PositionId,
    Guid RoleId,
    string PositionName,
    string RoleDisplayName);

/// <summary>
/// Position Data Scope Rule 数据传输对象。
/// </summary>
public sealed record PositionDataScopeRuleDto(
    Guid Id,
    Guid PositionId,
    string PositionName,
    string ScopeType,
    string MatchValue,
    string Description,
    bool IsEnabled);

/// <summary>
/// Permission Option 数据传输对象。
/// </summary>
public sealed record PermissionOptionDto(
    string Key,
    string DisplayName,
    string ModuleKey,
    string ModuleDisplayName);

/// <summary>
/// Module Option 数据传输对象。
/// </summary>
/// <param name="Key">业务键。</param>
/// <param name="DisplayName">界面显示名称。</param>
public sealed record ModuleOptionDto(string Key, string DisplayName);

/// <summary>
/// Position Permission Overview 数据传输对象。
/// </summary>
public sealed record PositionPermissionOverviewDto(
    IReadOnlyList<DepartmentDto> Departments,
    IReadOnlyList<JobPositionDto> Positions,
    IReadOnlyList<PositionRoleDto> Roles,
    IReadOnlyList<PermissionPackageDto> PermissionPackages,
    IReadOnlyList<PositionRoleBindingDto> RoleBindings,
    IReadOnlyList<PositionDataScopeRuleDto> DataScopeRules,
    IReadOnlyList<PermissionOptionDto> Permissions,
    IReadOnlyList<ModuleOptionDto> Modules);

/// <summary>
/// Upsert Department 请求参数。
/// </summary>
public sealed record UpsertDepartmentRequest(
    Guid? Id,
    string Code,
    string Name,
    Guid? ParentDepartmentId,
    bool IsEnabled);

/// <summary>
/// Upsert Job Position 请求参数。
/// </summary>
public sealed record UpsertJobPositionRequest(
    Guid? Id,
    string Code,
    string Name,
    Guid DepartmentId,
    string Description,
    bool IsEnabled);

/// <summary>
/// Upsert Permission Package 请求参数。
/// </summary>
public sealed record UpsertPermissionPackageRequest(
    Guid? Id,
    string DisplayName,
    string Description,
    IReadOnlyList<string> ModuleKeys,
    IReadOnlyList<string> Permissions,
    bool IsEnabled);

/// <summary>
/// Upsert Custom Role 请求参数。
/// </summary>
public sealed record UpsertCustomRoleRequest(
    Guid? Id,
    string DisplayName,
    IReadOnlyList<string> ModuleKeys,
    IReadOnlyList<string> Permissions);

/// <summary>
/// Update Position Role Bindings 请求参数。
/// </summary>
/// <param name="RoleIds">Role Ids 参数。</param>
public sealed record UpdatePositionRoleBindingsRequest(IReadOnlyList<Guid> RoleIds);

/// <summary>
/// Upsert Position Data Scope Rule 请求参数。
/// </summary>
public sealed record UpsertPositionDataScopeRuleRequest(
    string ScopeType,
    string MatchValue,
    string Description,
    bool IsEnabled);

/// <summary>
/// Update Position Data Scope Rules 请求参数。
/// </summary>
/// <param name="Rules">规则集合。</param>
public sealed record UpdatePositionDataScopeRulesRequest(IReadOnlyList<UpsertPositionDataScopeRuleRequest> Rules);
