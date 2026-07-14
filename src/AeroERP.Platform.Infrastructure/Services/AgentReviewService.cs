using AeroERP.Platform.Contracts;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Agent Review Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
/// <param name="auditWriter">audit Writer 参数。</param>
/// <param name="currentUser">current User 参数。</param>
public sealed class AgentReviewService(IAeroErpDbContext dbContext, IAuditWriter auditWriter, ICurrentUserAccessor currentUser) : IAgentReviewService
{
    /// <summary>
    /// 查询业务对象。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<AgentReviewDto>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.AgentReviewRequests
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(Map)
            .ToList();
    }

    /// <summary>
    /// Submit Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<AgentReviewDto> SubmitAsync(SubmitAgentReviewRequest request, CancellationToken cancellationToken)
    {
        var actor = currentUser.GetActor();
        var entity = new AgentReviewRequest(request.AgentName, request.ActionName, request.Payload, actor);
        dbContext.AgentReviewRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AgentReview", "Submitted", actor, $"{request.AgentName}:{request.ActionName}", cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// Decide Async。
    /// </summary>
    /// <param name="id">业务对象标识。</param>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<AgentReviewDto?> DecideAsync(Guid id, DecideAgentReviewRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AgentReviewRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var actor = currentUser.GetActor();
        entity.Decide(request.Decision, actor, request.ReviewerComment);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("AgentReview", "Decided", actor, $"{entity.AgentName}:{entity.ActionName}:{request.Decision}", cancellationToken);
        return Map(entity);
    }

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="entity">业务实体。</param>
    private static AgentReviewDto Map(AgentReviewRequest entity)
    {
        return new AgentReviewDto(
            entity.Id,
            entity.AgentName,
            entity.ActionName,
            entity.Payload,
            entity.Status,
            entity.RequestedBy,
            entity.ReviewedBy,
            entity.ReviewerComment,
            entity.CreatedAtUtc,
            entity.ReviewedAtUtc);
    }
}
