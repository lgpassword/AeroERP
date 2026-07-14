using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.PositionPermissions.Domain;

/// <summary>
/// Position Data Scope Rule 业务对象。
/// </summary>
public sealed class PositionDataScopeRule : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Position Data Scope Rule实例。
    /// </summary>
    private PositionDataScopeRule()
    {
    }

    /// <summary>
    /// 初始化Position Data Scope Rule实例。
    /// </summary>
    /// <param name="positionId">position Id 参数。</param>
    /// <param name="scopeType">scope Type 参数。</param>
    /// <param name="matchValue">match Value 参数。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    public PositionDataScopeRule(Guid positionId, string scopeType, string matchValue, string description, bool isEnabled)
    {
        PositionId = positionId;
        ScopeType = scopeType;
        MatchValue = matchValue;
        Description = description;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Position Id。
    /// </summary>
    public Guid PositionId { get; private set; }
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
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="scopeType">scope Type 参数。</param>
    /// <param name="matchValue">match Value 参数。</param>
    /// <param name="description">业务说明。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string scopeType, string matchValue, string description, bool isEnabled)
    {
        ScopeType = scopeType;
        MatchValue = matchValue;
        Description = description;
        IsEnabled = isEnabled;
        Touch();
    }
}
