using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Workflow.Domain;

/// <summary>
/// Workflow Definition 业务对象。
/// </summary>
public sealed class WorkflowDefinition : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Workflow Definition实例。
    /// </summary>
    private WorkflowDefinition()
    {
    }

    /// <summary>
    /// 初始化Workflow Definition实例。
    /// </summary>
    /// <param name="key">业务键。</param>
    /// <param name="displayName">界面显示名称。</param>
    /// <param name="moduleKey">模块键。</param>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="requiredPermission">required Permission 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public WorkflowDefinition(
        string key,
        string displayName,
        string moduleKey,
        string documentType,
        string requiredPermission,
        bool isEnabled = true)
    {
        Key = key;
        DisplayName = displayName;
        ModuleKey = moduleKey;
        DocumentType = documentType;
        RequiredPermission = requiredPermission;
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
    /// Module Key。
    /// </summary>
    public string ModuleKey { get; private set; } = string.Empty;
    /// <summary>
    /// 业务单据类型。
    /// </summary>
    public string DocumentType { get; private set; } = string.Empty;
    /// <summary>
    /// Required Permission。
    /// </summary>
    public string RequiredPermission { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;
}
