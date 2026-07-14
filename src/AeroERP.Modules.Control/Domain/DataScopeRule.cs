using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Control.Domain;

/// <summary>
/// Data Scope Rule 业务对象。
/// </summary>
public sealed class DataScopeRule : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Data Scope Rule实例。
    /// </summary>
    private DataScopeRule()
    {
    }

    /// <summary>
    /// 初始化Data Scope Rule实例。
    /// </summary>
    /// <param name="roleKey">role Key 参数。</param>
    /// <param name="scopeType">scope Type 参数。</param>
    /// <param name="matchValue">match Value 参数。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    public DataScopeRule(string roleKey, string scopeType, string matchValue, string description, bool isEnabled)
    {
        RoleKey = roleKey;
        ScopeType = scopeType;
        MatchValue = matchValue;
        Description = description;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Role Key。
    /// </summary>
    public string RoleKey { get; private set; } = string.Empty;
    /// <summary>
    /// Scope Type。
    /// </summary>
    public string ScopeType { get; private set; } = string.Empty;
    /// <summary>
    /// Match Value。
    /// </summary>
    public string MatchValue { get; private set; } = string.Empty;
    /// <summary>
    /// 业务说明。
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="matchValue">match Value 参数。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string matchValue, string description, bool isEnabled)
    {
        MatchValue = matchValue;
        Description = description;
        IsEnabled = isEnabled;
        Touch();
    }
}
