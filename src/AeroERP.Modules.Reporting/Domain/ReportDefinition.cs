using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Reporting.Domain;

/// <summary>
/// Report Definition 业务对象。
/// </summary>
public sealed class ReportDefinition : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Report Definition实例。
    /// </summary>
    private ReportDefinition()
    {
    }

    /// <summary>
    /// 初始化Report Definition实例。
    /// </summary>
    /// <param name="key">业务键。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="category">业务分类。</param>
    /// <param name="queryModel">query Model 参数。</param>
    /// <param name="parametersJson">parameters Json 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public ReportDefinition(string key, string displayName, string category, string queryModel, string parametersJson, bool isEnabled, string updatedBy)
    {
        Key = key;
        DisplayName = displayName;
        Category = category;
        QueryModel = queryModel;
        ParametersJson = parametersJson;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
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
    /// Query Model。
    /// </summary>
    public string QueryModel { get; private set; } = string.Empty;
    /// <summary>
    /// Parameters Json。
    /// </summary>
    public string ParametersJson { get; private set; } = "{}";
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
    /// <param name="category">业务分类。</param>
    /// <param name="queryModel">query Model 参数。</param>
    /// <param name="parametersJson">parameters Json 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string displayName, string category, string queryModel, string parametersJson, bool isEnabled, string updatedBy)
    {
        DisplayName = displayName;
        Category = category;
        QueryModel = queryModel;
        ParametersJson = parametersJson;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
