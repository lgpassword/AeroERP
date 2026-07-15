using AeroERP.Platform.Domain;
using AeroERP.Platform.Services;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Audit Writer 业务对象。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
public sealed class AuditWriter(IAeroErpDbContext dbContext) : IAuditWriter
{
    /// <summary>
    /// Write Async。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="detail">详细说明。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task WriteAsync(string category, string action, string actor, string detail, CancellationToken cancellationToken)
    {
        dbContext.AuditEvents.Add(new AuditEvent(category, action, actor, detail));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
