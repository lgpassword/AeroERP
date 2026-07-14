using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.DocumentExchange.Domain;

/// <summary>
/// Import Template 业务对象。
/// </summary>
public sealed class ImportTemplate : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Import Template实例。
    /// </summary>
    private ImportTemplate()
    {
    }

    /// <summary>
    /// 初始化Import Template实例。
    /// </summary>
    /// <param name="templateKey">template Key 参数。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="targetModule">target Module 参数。</param>
    /// <param name="fileType">file Type 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public ImportTemplate(string templateKey, string displayName, string targetModule, string fileType, bool isEnabled, string updatedBy)
    {
        TemplateKey = templateKey;
        DisplayName = displayName;
        TargetModule = targetModule;
        FileType = fileType;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Template Key。
    /// </summary>
    public string TemplateKey { get; private set; } = string.Empty;
    /// <summary>
    /// 界面显示名称。
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    /// <summary>
    /// Target Module。
    /// </summary>
    public string TargetModule { get; private set; } = string.Empty;
    /// <summary>
    /// File Type。
    /// </summary>
    public string FileType { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="targetModule">target Module 参数。</param>
    /// <param name="fileType">file Type 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string displayName, string targetModule, string fileType, bool isEnabled, string updatedBy)
    {
        DisplayName = displayName;
        TargetModule = targetModule;
        FileType = fileType;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
