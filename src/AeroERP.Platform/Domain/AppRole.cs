using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Platform.Domain;

/// <summary>
/// App Role 业务对象。
/// </summary>
public sealed class AppRole : Entity, IAggregateRoot
{
    /// <summary>
    /// _module Accesses。
    /// </summary>
    private readonly List<RoleModuleAccess> _moduleAccesses = [];

    /// <summary>
    /// 初始化App Role实例。
    /// </summary>
    private AppRole()
    {
    }

    /// <summary>
    /// 初始化App Role实例。
    /// </summary>
    /// <param name="key">业务键。</param>
    /// <param name="displayName">界面显示名称。</param>
    public AppRole(string key, string displayName)
    {
        Key = key;
        DisplayName = displayName;
    }

    /// <summary>
    /// Key。
    /// </summary>
    public string Key { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    public List<RoleModuleAccess> ModuleAccesses => _moduleAccesses;

    /// <summary>
    /// 更新Display Name。
    /// </summary>
    /// <param name="displayName">界面显示名称。</param>
    public void UpdateDisplayName(string displayName)
    {
        DisplayName = displayName;
        Touch();
    }

    /// <summary>
    /// Set Module Access。
    /// </summary>
    /// <param name="moduleKeys">module Keys 参数。</param>
    public void SetModuleAccess(IEnumerable<string> moduleKeys)
    {
        _moduleAccesses.Clear();
        foreach (var moduleKey in moduleKeys
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Select(x => x.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _moduleAccesses.Add(new RoleModuleAccess(Id, moduleKey));
        }

        Touch();
    }
}
