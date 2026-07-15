using AeroERP.Platform.Contracts;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Organization Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
/// <param name="auditWriter">audit Writer 参数。</param>
/// <param name="currentUser">current User 参数。</param>
public sealed class OrganizationService(IAeroErpDbContext dbContext, IAuditWriter auditWriter, ICurrentUserAccessor currentUser) : IOrganizationService
{
    /// <summary>
    /// 查询业务对象。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<OrganizationSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Organizations
            .OrderBy(x => x.Name)
            .Select(x => new OrganizationSummaryDto(x.Id, x.Name, x.DefaultRole, x.RegionCode))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 创建业务对象。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OrganizationSummaryDto> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var organization = new Organization(request.Name, request.DefaultRole, request.RegionCode);
        dbContext.Organizations.Add(organization);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Organization", "Created", currentUser.GetActor(), request.Name, cancellationToken);
        return new OrganizationSummaryDto(organization.Id, organization.Name, organization.DefaultRole, organization.RegionCode);
    }
}
