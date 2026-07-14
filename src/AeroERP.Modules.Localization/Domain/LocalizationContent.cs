using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Localization.Domain;

/// <summary>
/// Localization Content 业务对象。
/// </summary>
public sealed class LocalizationContent : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Localization Content实例。
    /// </summary>
    private LocalizationContent()
    {
    }

    /// <summary>
    /// 初始化Localization Content实例。
    /// </summary>
    /// <param name="key">业务键。</param>
    /// <param name="category">业务分类。</param>
    /// <param name="chineseText">chinese Text 参数。</param>
    /// <param name="englishText">english Text 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public LocalizationContent(string key, string category, string chineseText, string englishText, bool isEnabled)
    {
        Key = key;
        Category = category;
        ChineseText = chineseText;
        EnglishText = englishText;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Key。
    /// </summary>
    public string Key { get; private set; } = string.Empty;
    /// <summary>
    /// Category。
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    /// <summary>
    /// Chinese Text。
    /// </summary>
    public string ChineseText { get; private set; } = string.Empty;
    /// <summary>
    /// English Text。
    /// </summary>
    public string EnglishText { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="chineseText">chinese Text 参数。</param>
    /// <param name="englishText">english Text 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string category, string chineseText, string englishText, bool isEnabled)
    {
        Category = category;
        ChineseText = chineseText;
        EnglishText = englishText;
        IsEnabled = isEnabled;
        Touch();
    }
}
