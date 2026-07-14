using AeroERP.Platform.Contracts;

namespace AeroERP.Platform.Services;

/// <summary>
/// Auth Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 执行Login。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 获取Current User。
    /// </summary>
    /// <param name="userId">用户标识。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
