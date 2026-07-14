using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.PositionPermissions.Domain;

/// <summary>
/// Permission Package 业务对象。
/// </summary>
public sealed class PermissionPackage : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Permission Package实例。
    /// </summary>
    private PermissionPackage()
    {
    }

    /// <summary>
    /// 初始化Permission Package实例。
    /// </summary>
    /// <param name="key">业务键。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="moduleKeys">module Keys 参数。</param>
    /// <param name="permissions">权限编码集合。</param>
    /// <param name="isEnabled">是否启用。</param>
    public PermissionPackage(string key, string displayName, string description, string moduleKeys, string permissions, bool isEnabled)
    {
        Key = key;
        DisplayName = displayName;
        Description = description;
        ModuleKeys = moduleKeys;
        Permissions = permissions;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Key。
    /// </summary>
    public string Key { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    /// <summary>
    /// 业务说明。
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>
    /// Module Keys。
    /// </summary>
    public string ModuleKeys { get; private set; } = string.Empty;
    /// <summary>
    /// 权限编码集合。
    /// </summary>
    public string Permissions { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="moduleKeys">module Keys 参数。</param>
    /// <param name="permissions">权限编码集合。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string displayName, string description, string moduleKeys, string permissions, bool isEnabled)
    {
        DisplayName = displayName;
        Description = description;
        ModuleKeys = moduleKeys;
        Permissions = permissions;
        IsEnabled = isEnabled;
        Touch();
    }
}
