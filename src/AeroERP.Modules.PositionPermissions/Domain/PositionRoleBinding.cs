using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.PositionPermissions.Domain;

/// <summary>
/// Position Role Binding 业务对象。
/// </summary>
public sealed class PositionRoleBinding : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Position Role Binding实例。
    /// </summary>
    private PositionRoleBinding()
    {
    }

    /// <summary>
    /// 初始化Position Role Binding实例。
    /// </summary>
    /// <param name="positionId">position Id 参数。</param>
    /// <param name="roleId">角色标识。</param>
    public PositionRoleBinding(Guid positionId, Guid roleId)
    {
        PositionId = positionId;
        RoleId = roleId;
    }

    /// <summary>
    /// Position Id。
    /// </summary>
    public Guid PositionId { get; private set; }
    /// <summary>
    /// 角色标识。
    /// </summary>
    public Guid RoleId { get; private set; }
}
