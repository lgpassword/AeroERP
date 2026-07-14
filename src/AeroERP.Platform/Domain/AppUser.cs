using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Platform.Domain;

/// <summary>
/// App User 业务对象。
/// </summary>
public sealed class AppUser : Entity, IAggregateRoot
{
    /// <summary>
    /// _role Assignments。
    /// </summary>
    private readonly List<UserRoleAssignment> _roleAssignments = [];

    /// <summary>
    /// 初始化App User实例。
    /// </summary>
    private AppUser()
    {
    }

    /// <summary>
    /// 初始化App User实例。
    /// </summary>
    /// <param name="userName">登录用户名。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="passwordHash">password Hash 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public AppUser(string userName, string displayName, string passwordHash, bool isEnabled)
    {
        UserName = userName;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// 登录用户名。
    /// </summary>
    public string UserName { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    /// <summary>
    /// 密码哈希值。
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }
    public List<UserRoleAssignment> RoleAssignments => _roleAssignments;

    /// <summary>
    /// 更新Roles。
    /// </summary>
    /// <param name="roleIds">role Ids 参数。</param>
    public void UpdateRoles(IEnumerable<Guid> roleIds)
    {
        _roleAssignments.Clear();
        foreach (var roleId in roleIds.Distinct())
        {
            _roleAssignments.Add(new UserRoleAssignment(Id, roleId));
        }

        Touch();
    }

    /// <summary>
    /// Set Password Hash。
    /// </summary>
    /// <param name="passwordHash">password Hash 参数。</param>
    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        Touch();
    }

    /// <summary>
    /// Set Enabled。
    /// </summary>
    /// <param name="isEnabled">是否启用。</param>
    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        Touch();
    }
}
