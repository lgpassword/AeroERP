using AeroERP.Platform.Contracts;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Services;

/// <summary>
/// Module Visibility Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
/// <param name="auditWriter">audit Writer 参数。</param>
/// <param name="currentUser">current User 参数。</param>
public sealed class ModuleVisibilityService(IAeroErpDbContext dbContext, IAuditWriter auditWriter, ICurrentUserAccessor currentUser) : IModuleVisibilityService
{
    /// <summary>
    /// 查询业务对象。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ModuleVisibilityDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.PluginModules
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DisplayName)
            .Select(x => new ModuleVisibilityDto(x.Id, x.Key, x.DisplayName, x.IsVisible, x.Category))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 查询Visible。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<ModuleVisibilityDto>> ListVisibleAsync(CancellationToken cancellationToken)
    {
        var modules = await dbContext.PluginModules
            .Where(x => x.IsVisible)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DisplayName)
            .Select(x => new ModuleVisibilityDto(x.Id, x.Key, x.DisplayName, x.IsVisible, x.Category))
            .ToListAsync(cancellationToken);

        return modules
            .Where(x => currentUser.CanAccessModule(x.Key))
            .ToList();
    }

    /// <summary>
    /// Toggle Async。
    /// </summary>
    /// <param name="moduleId">module Id 参数。</param>
    /// <param name="isVisible">是否可见。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<ModuleVisibilityDto?> ToggleAsync(Guid moduleId, bool isVisible, CancellationToken cancellationToken)
    {
        var module = await dbContext.PluginModules.FirstOrDefaultAsync(x => x.Id == moduleId, cancellationToken);
        if (module is null)
        {
            return null;
        }

        var actor = currentUser.GetActor();
        module.SetVisibility(isVisible, actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Plugin", "VisibilityChanged", actor, $"{module.Key}:{isVisible}", cancellationToken);
        return new ModuleVisibilityDto(module.Id, module.Key, module.DisplayName, module.IsVisible, module.Category);
    }
}
