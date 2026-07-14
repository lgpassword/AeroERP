using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.PositionPermissions.Domain;

/// <summary>
/// Job Position 业务对象。
/// </summary>
public sealed class JobPosition : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Job Position实例。
    /// </summary>
    private JobPosition()
    {
    }

    /// <summary>
    /// 初始化Job Position实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="departmentId">department Id 参数。</param>
    /// <param name="departmentName">department Name 参数。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    public JobPosition(string code, string name, Guid departmentId, string departmentName, string description, bool isEnabled)
    {
        Code = code;
        Name = name;
        DepartmentId = departmentId;
        DepartmentName = departmentName;
        Description = description;
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
    /// Department Id。
    /// </summary>
    public Guid DepartmentId { get; private set; }
    /// <summary>
    /// Department Name。
    /// </summary>
    public string DepartmentName { get; private set; } = string.Empty;
    /// <summary>
    /// 业务说明。
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="departmentId">department Id 参数。</param>
    /// <param name="departmentName">department Name 参数。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string code, string name, Guid departmentId, string departmentName, string description, bool isEnabled)
    {
        Code = code;
        Name = name;
        DepartmentId = departmentId;
        DepartmentName = departmentName;
        Description = description;
        IsEnabled = isEnabled;
        Touch();
    }
}
