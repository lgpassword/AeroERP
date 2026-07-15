namespace AeroERP.Platform.Domain;

/// <summary>
/// Role Module Access 业务对象。
/// </summary>
public sealed class RoleModuleAccess
{
    /// <summary>
    /// 初始化Role Module Access实例。
    /// </summary>
    private RoleModuleAccess()
    {
    }

    /// <summary>
    /// 初始化Role Module Access实例。
    /// </summary>
    /// <param name="roleId">角色标识。</param>
    /// <param name="moduleKey">模块键。</param>
    public RoleModuleAccess(Guid roleId, string moduleKey)
    {
        RoleId = roleId;
        ModuleKey = moduleKey;
    }

    /// <summary>
    /// 角色标识。
    /// </summary>
    public Guid RoleId { get; private set; }
    /// <summary>
    /// Module Key。
    /// </summary>
    public string ModuleKey { get; private set; } = string.Empty;
}
