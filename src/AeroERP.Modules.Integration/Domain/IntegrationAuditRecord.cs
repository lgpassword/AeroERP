using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Integration.Domain;

/// <summary>
/// Integration Audit Record 业务对象。
/// </summary>
public sealed class IntegrationAuditRecord : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Integration Audit Record实例。
    /// </summary>
    private IntegrationAuditRecord()
    {
    }

    /// <summary>
    /// 初始化Integration Audit Record实例。
    /// </summary>
    /// <param name="auditNo">audit No 参数。</param>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="targetKey">target Key 参数。</param>
    /// <param name="result">执行结果。</param>
    /// <param name="message">执行消息。</param>
    /// <param name="actor">操作人。</param>
    public IntegrationAuditRecord(string auditNo, string category, string action, string targetKey, string result, string message, string actor)
    {
        AuditNo = auditNo;
        Category = category;
        Action = action;
        TargetKey = targetKey;
        Result = result;
        Message = message;
        Actor = actor;
    }

    /// <summary>
    /// Audit No。
    /// </summary>
    public string AuditNo { get; private set; } = string.Empty;
    /// <summary>
    /// Category。
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    /// <summary>
    /// Action。
    /// </summary>
    public string Action { get; private set; } = string.Empty;
    /// <summary>
    /// Target Key。
    /// </summary>
    public string TargetKey { get; private set; } = string.Empty;
    /// <summary>
    /// 执行结果。
    /// </summary>
    public string Result { get; private set; } = string.Empty;
    /// <summary>
    /// 执行消息。
    /// </summary>
    public string Message { get; private set; } = string.Empty;
    /// <summary>
    /// 操作人。
    /// </summary>
    public string Actor { get; private set; } = string.Empty;
}
