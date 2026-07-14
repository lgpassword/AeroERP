using AeroERP.Platform.Domain;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Services;

/// <summary>
/// Aero Erp Db Context 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IAeroErpDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<AppRole> Roles { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<PluginModule> PluginModules { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    DbSet<AgentReviewRequest> AgentReviewRequests { get; }
    /// <summary>
    /// 执行Save Changes。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
