using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.PositionPermissions.Domain;

/// <summary>
/// Role Permission Grant 业务对象。
/// </summary>
public sealed class RolePermissionGrant : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Role Permission Grant实例。
    /// </summary>
    private RolePermissionGrant()
    {
    }

    /// <summary>
    /// 初始化Role Permission Grant实例。
    /// </summary>
    /// <param name="roleId">角色标识。</param>
    /// <param name="permission">权限编码。</param>
    public RolePermissionGrant(Guid roleId, string permission)
    {
        RoleId = roleId;
        Permission = permission;
    }

    /// <summary>
    /// 角色标识。
    /// </summary>
    public Guid RoleId { get; private set; }
    /// <summary>
    /// 权限编码。
    /// </summary>
    public string Permission { get; private set; } = string.Empty;
}
