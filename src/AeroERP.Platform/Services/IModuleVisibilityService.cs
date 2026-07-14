using AeroERP.Platform.Contracts;

namespace AeroERP.Platform.Services;

/// <summary>
/// Module Visibility Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IModuleVisibilityService
{
    /// <summary>
    /// 查询业务对象。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ModuleVisibilityDto>> ListAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 查询Visible。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<ModuleVisibilityDto>> ListVisibleAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Toggle。
    /// </summary>
    /// <param name="moduleId">module Id 参数。</param>
    /// <param name="isVisible">是否可见。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<ModuleVisibilityDto?> ToggleAsync(Guid moduleId, bool isVisible, CancellationToken cancellationToken);
}
