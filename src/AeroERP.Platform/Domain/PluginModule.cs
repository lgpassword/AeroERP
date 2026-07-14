using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Platform.Domain;

/// <summary>
/// Plugin Module 业务对象。
/// </summary>
public sealed class PluginModule : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Plugin Module实例。
    /// </summary>
    private PluginModule()
    {
    }

    /// <summary>
    /// 初始化Plugin Module实例。
    /// </summary>
    /// <param name="key">业务键。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="category">业务分类。</param>
    /// <param name="isVisible">是否可见。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public PluginModule(string key, string displayName, string category, bool isVisible, string updatedBy)
    {
        Key = key;
        DisplayName = displayName;
        Category = category;
        IsVisible = isVisible;
        LastChangedBy = updatedBy;
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
    /// Category。
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    /// <summary>
    /// 是否可见。
    /// </summary>
    public bool IsVisible { get; private set; }
    /// <summary>
    /// Last Changed By。
    /// </summary>
    public string LastChangedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Set Visibility。
    /// </summary>
    /// <param name="isVisible">是否可见。</param>
    /// <param name="changedBy">changed By 参数。</param>
    public void SetVisibility(bool isVisible, string changedBy)
    {
        IsVisible = isVisible;
        LastChangedBy = changedBy;
        Touch();
    }

    /// <summary>
    /// 更新Metadata。
    /// </summary>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="category">业务分类。</param>
    /// <param name="changedBy">changed By 参数。</param>
    public void UpdateMetadata(string displayName, string category, string changedBy)
    {
        DisplayName = displayName;
        Category = category;
        LastChangedBy = changedBy;
        Touch();
    }
}
