using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.DocumentExchange.Domain;

/// <summary>
/// Print Template 业务对象。
/// </summary>
public sealed class PrintTemplate : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Print Template实例。
    /// </summary>
    private PrintTemplate()
    {
    }

    /// <summary>
    /// 初始化Print Template实例。
    /// </summary>
    /// <param name="templateKey">template Key 参数。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="targetModule">target Module 参数。</param>
    /// <param name="contentType">content Type 参数。</param>
    /// <param name="templateBody">template Body 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public PrintTemplate(string templateKey, string displayName, string targetModule, string contentType, string templateBody, bool isEnabled, string updatedBy)
    {
        TemplateKey = templateKey;
        DisplayName = displayName;
        TargetModule = targetModule;
        ContentType = contentType;
        TemplateBody = templateBody;
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
    /// Content Type。
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;
    /// <summary>
    /// Template Body。
    /// </summary>
    public string TemplateBody { get; private set; } = string.Empty;
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
    /// <param name="contentType">content Type 参数。</param>
    /// <param name="templateBody">template Body 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string displayName, string targetModule, string contentType, string templateBody, bool isEnabled, string updatedBy)
    {
        DisplayName = displayName;
        TargetModule = targetModule;
        ContentType = contentType;
        TemplateBody = templateBody;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
