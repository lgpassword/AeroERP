using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.PositionPermissions.Domain;

/// <summary>
/// Position Department 业务对象。
/// </summary>
public sealed class PositionDepartment : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Position Department实例。
    /// </summary>
    private PositionDepartment()
    {
    }

    /// <summary>
    /// 初始化Position Department实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="parentDepartmentId">parent Department Id 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public PositionDepartment(string code, string name, Guid? parentDepartmentId, bool isEnabled)
    {
        Code = code;
        Name = name;
        ParentDepartmentId = parentDepartmentId;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// 业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// Parent Department Id。
    /// </summary>
    public Guid? ParentDepartmentId { get; private set; }
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="parentDepartmentId">parent Department Id 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string code, string name, Guid? parentDepartmentId, bool isEnabled)
    {
        Code = code;
        Name = name;
        ParentDepartmentId = parentDepartmentId;
        IsEnabled = isEnabled;
        Touch();
    }
}
