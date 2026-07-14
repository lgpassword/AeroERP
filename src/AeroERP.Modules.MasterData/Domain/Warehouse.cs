using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.MasterData.Domain;

/// <summary>
/// Warehouse 业务对象。
/// </summary>
public sealed class Warehouse : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Warehouse实例。
    /// </summary>
    private Warehouse()
    {
    }

    /// <summary>
    /// 初始化Warehouse实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="location">位置说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="organizationName">所属组织名称。</param>
    public Warehouse(string code, string name, string location, bool isEnabled, Guid? organizationId, string organizationName)
    {
        Code = code;
        Name = name;
        Location = location;
        IsEnabled = isEnabled;
        OrganizationId = organizationId;
        OrganizationName = organizationName;
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
    /// 位置说明。
    /// </summary>
    public string Location { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }
    /// <summary>
    /// 所属组织标识。
    /// </summary>
    public Guid? OrganizationId { get; private set; }
    /// <summary>
    /// 所属组织名称。
    /// </summary>
    public string OrganizationName { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="location">位置说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="organizationId">所属组织标识。</param>
    /// <param name="organizationName">所属组织名称。</param>
    public void Update(string code, string name, string location, bool isEnabled, Guid? organizationId, string organizationName)
    {
        Code = code;
        Name = name;
        Location = location;
        IsEnabled = isEnabled;
        OrganizationId = organizationId;
        OrganizationName = organizationName;
        Touch();
    }
}
