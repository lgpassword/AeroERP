using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.DocumentExchange.Domain;

/// <summary>
/// Import Field Mapping 业务对象。
/// </summary>
public sealed class ImportFieldMapping : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Import Field Mapping实例。
    /// </summary>
    private ImportFieldMapping()
    {
    }

    /// <summary>
    /// 初始化Import Field Mapping实例。
    /// </summary>
    /// <param name="templateKey">template Key 参数。</param>
    /// <param name="sourceField">source Field 参数。</param>
    /// <param name="targetField">target Field 参数。</param>
    /// <param name="isRequired">is Required 参数。</param>
    /// <param name="transformRule">transform Rule 参数。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public ImportFieldMapping(string templateKey, string sourceField, string targetField, bool isRequired, string transformRule, string updatedBy)
    {
        TemplateKey = templateKey;
        SourceField = sourceField;
        TargetField = targetField;
        IsRequired = isRequired;
        TransformRule = transformRule;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Template Key。
    /// </summary>
    public string TemplateKey { get; private set; } = string.Empty;
    /// <summary>
    /// Source Field。
    /// </summary>
    public string SourceField { get; private set; } = string.Empty;
    /// <summary>
    /// Target Field。
    /// </summary>
    public string TargetField { get; private set; } = string.Empty;
    /// <summary>
    /// Is Required。
    /// </summary>
    public bool IsRequired { get; private set; }
    /// <summary>
    /// Transform Rule。
    /// </summary>
    public string TransformRule { get; private set; } = string.Empty;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="sourceField">source Field 参数。</param>
    /// <param name="targetField">target Field 参数。</param>
    /// <param name="isRequired">is Required 参数。</param>
    /// <param name="transformRule">transform Rule 参数。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string sourceField, string targetField, bool isRequired, string transformRule, string updatedBy)
    {
        SourceField = sourceField;
        TargetField = targetField;
        IsRequired = isRequired;
        TransformRule = transformRule;
        UpdatedBy = updatedBy;
        Touch();
    }
}
