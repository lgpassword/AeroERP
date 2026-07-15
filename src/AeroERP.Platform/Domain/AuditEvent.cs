using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Platform.Domain;

/// <summary>
/// Audit Event 业务对象。
/// </summary>
public sealed class AuditEvent : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Audit Event实例。
    /// </summary>
    private AuditEvent()
    {
    }

    /// <summary>
    /// 初始化Audit Event实例。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="detail">详细说明。</param>
    public AuditEvent(string category, string action, string actor, string detail)
    {
        Category = category;
        Action = action;
        Actor = actor;
        Detail = detail;
    }

    /// <summary>
    /// Category。
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    /// <summary>
    /// Action。
    /// </summary>
    public string Action { get; private set; } = string.Empty;
    /// <summary>
    /// 操作人。
    /// </summary>
    public string Actor { get; private set; } = string.Empty;
    /// <summary>
    /// Detail。
    /// </summary>
    public string Detail { get; private set; } = string.Empty;
}
