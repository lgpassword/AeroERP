using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Platform.Domain;

/// <summary>
/// Agent Review 请求参数。
/// </summary>
public sealed class AgentReviewRequest : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Agent Review Request实例。
    /// </summary>
    private AgentReviewRequest()
    {
    }

    /// <summary>
    /// 初始化Agent Review Request实例。
    /// </summary>
    /// <param name="agentName">agent Name 参数。</param>
    /// <param name="actionName">action Name 参数。</param>
    /// <param name="payload">业务载荷。</param>
    /// <param name="requestedBy">requested By 参数。</param>
    public AgentReviewRequest(string agentName, string actionName, string payload, string requestedBy)
    {
        AgentName = agentName;
        ActionName = actionName;
        Payload = payload;
        RequestedBy = requestedBy;
        Status = AgentReviewStatus.Pending;
    }

    /// <summary>
    /// Agent Name。
    /// </summary>
    public string AgentName { get; private set; } = string.Empty;
    /// <summary>
    /// Action Name。
    /// </summary>
    public string ActionName { get; private set; } = string.Empty;
    /// <summary>
    /// Payload。
    /// </summary>
    public string Payload { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = AgentReviewStatus.Pending;
    /// <summary>
    /// Requested By。
    /// </summary>
    public string RequestedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Reviewed By。
    /// </summary>
    public string? ReviewedBy { get; private set; }
    /// <summary>
    /// Reviewer Comment。
    /// </summary>
    public string? ReviewerComment { get; private set; }
    /// <summary>
    /// Reviewed At Utc。
    /// </summary>
    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    /// <summary>
    /// Decide。
    /// </summary>
    /// <param name="decision">处理决策。</param>
    /// <param name="reviewedBy">reviewed By 参数。</param>
    /// <param name="reviewerComment">reviewer Comment 参数。</param>
    public void Decide(string decision, string reviewedBy, string? reviewerComment)
    {
        Status = decision;
        ReviewedBy = reviewedBy;
        ReviewerComment = reviewerComment;
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
