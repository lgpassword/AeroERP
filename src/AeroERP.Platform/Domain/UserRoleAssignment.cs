namespace AeroERP.Platform.Domain;

/// <summary>
/// User Role Assignment 业务对象。
/// </summary>
public sealed class UserRoleAssignment
{
    /// <summary>
    /// 初始化User Role Assignment实例。
    /// </summary>
    private UserRoleAssignment()
    {
    }

    /// <summary>
    /// 初始化User Role Assignment实例。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="roleId">角色标识。</param>
    public UserRoleAssignment(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    /// <summary>
    /// User Id。
    /// </summary>
    public Guid UserId { get; private set; }
    /// <summary>
    /// 角色标识。
    /// </summary>
    public Guid RoleId { get; private set; }
}
