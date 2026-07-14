using AeroERP.Platform.Contracts;

namespace AeroERP.Platform.Services;

/// <summary>
/// Organization Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IOrganizationService
{
    /// <summary>
    /// 查询业务对象。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<OrganizationSummaryDto>> ListAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 创建业务对象。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OrganizationSummaryDto> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken);
}
