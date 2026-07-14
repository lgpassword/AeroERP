using AeroERP.Modules.Control.Domain;
using AeroERP.Modules.Localization.Domain;
using AeroERP.Modules.Workflow.Domain;
using AeroERP.BuildingBlocks.Abstractions;
using AeroERP.Platform.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Persistence;

/// <summary>
/// Seeder 业务对象。
/// </summary>
public static class Seeder
{
    /// <summary>
    /// Seed Platform Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="modules">模块描述集合。</param>
    /// <param name="passwordHasher">password Hasher 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task SeedPlatformAsync(
        AeroErpDbContext dbContext,
        IReadOnlyCollection<AeroErpModuleDescriptor> modules,
        PasswordHasher<AppUser> passwordHasher,
        CancellationToken cancellationToken = default)
    {
        var existing = dbContext.PluginModules.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var pending = modules
            .Where(x => !existing.Contains(x.Key))
            .Select(x => new PluginModule(x.Key, x.DisplayName, x.Category, true, "system"))
            .ToList();

        if (pending.Count > 0)
        {
            dbContext.PluginModules.AddRange(pending);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var moduleKeys = modules.Select(x => x.Key).ToList();
        var existingModules = await dbContext.PluginModules
            .Where(x => moduleKeys.Contains(x.Key))
            .ToListAsync(cancellationToken);
        var moduleMetadataChanged = false;
        foreach (var module in existingModules)
        {
            var source = modules.First(x => string.Equals(x.Key, module.Key, StringComparison.OrdinalIgnoreCase));
            if (module.DisplayName == source.DisplayName && module.Category == source.Category)
            {
                continue;
            }

            module.UpdateMetadata(source.DisplayName, source.Category, "system");
            moduleMetadataChanged = true;
        }

        if (moduleMetadataChanged)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.WorkflowDefinitions.AnyAsync(x => x.Key == WorkflowDefinitionCatalog.ProcurementRequestReview, cancellationToken))
        {
            dbContext.WorkflowDefinitions.Add(WorkflowDefinitionCatalog.CreateProcurementRequestReview());
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.NumberingRules.AnyAsync(cancellationToken))
        {
            dbContext.NumberingRules.AddRange(
                new NumberingRule(DocumentTypeKeys.ProcurementRequest, "PR-", true, 4, true),
                new NumberingRule(DocumentTypeKeys.SalesQuotation, "SQ-", true, 4, true));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Currencies.AnyAsync(cancellationToken))
        {
            dbContext.Currencies.AddRange(
                new Currency("CNY", "人民币", "¥", 1m, true, true),
                new Currency("USD", "美元", "$", 7.2m, false, true));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.LocalizationSettings.AnyAsync(cancellationToken))
        {
            dbContext.LocalizationSettings.Add(new LocalizationSettings("CNY", "增值税普通发票", string.Empty, string.Empty, 0.13m));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Roles.AnyAsync(cancellationToken))
        {
            var adminRole = new AppRole(PlatformRoleCatalog.PlatformAdmin, "平台管理员");
            adminRole.SetModuleAccess(["platform", "master-data", "procurement", "sales", "inventory", "wms", "mobile-work", "integration", "document-exchange", "finance", "workflow", "control", "localization", "position-permissions", "manufacturing", "advanced-manufacturing", "reporting", "quality", "planning"]);

            var operationsRole = new AppRole(PlatformRoleCatalog.OperationsManager, "运营经理");
            operationsRole.SetModuleAccess(["master-data", "procurement", "sales", "inventory", "wms", "mobile-work", "integration", "document-exchange", "finance", "workflow", "control", "localization", "manufacturing", "advanced-manufacturing", "reporting", "quality", "planning"]);

            var purchaserRole = new AppRole(PlatformRoleCatalog.Purchaser, "采购专员");
            purchaserRole.SetModuleAccess(["procurement", "inventory", "workflow"]);

            dbContext.Roles.AddRange(adminRole, operationsRole, purchaserRole);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var roles = await dbContext.Roles
                .Include(x => x.ModuleAccesses)
                .Where(x =>
                    x.Key == PlatformRoleCatalog.PlatformAdmin ||
                    x.Key == PlatformRoleCatalog.OperationsManager ||
                    x.Key == PlatformRoleCatalog.Purchaser)
                .ToListAsync(cancellationToken);

            var changed = false;
            foreach (var role in roles)
            {
                var nextModules = role.Key == PlatformRoleCatalog.PlatformAdmin
                    ? role.ModuleAccesses.Select(x => x.ModuleKey).Concat(["platform", "master-data", "procurement", "sales", "inventory", "wms", "mobile-work", "integration", "document-exchange", "finance", "workflow", "control", "localization", "position-permissions", "manufacturing", "advanced-manufacturing", "reporting", "quality", "planning"])
                    : role.Key == PlatformRoleCatalog.OperationsManager
                        ? role.ModuleAccesses.Select(x => x.ModuleKey).Concat(["master-data", "procurement", "sales", "inventory", "wms", "mobile-work", "integration", "document-exchange", "finance", "workflow", "control", "localization", "manufacturing", "advanced-manufacturing", "reporting", "quality", "planning"])
                        : role.ModuleAccesses.Select(x => x.ModuleKey).Concat(["procurement", "inventory", "workflow"]);

                var before = role.ModuleAccesses.Select(x => x.ModuleKey).OrderBy(x => x).ToArray();
                role.SetModuleAccess(nextModules);
                var after = role.ModuleAccesses.Select(x => x.ModuleKey).OrderBy(x => x).ToArray();
                changed |= !before.SequenceEqual(after, StringComparer.OrdinalIgnoreCase);
            }

            if (changed)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            var adminRole = await dbContext.Roles.FirstAsync(x => x.Key == PlatformRoleCatalog.PlatformAdmin, cancellationToken);
            var adminUser = new AppUser("admin", "系统管理员", string.Empty, true);
            adminUser.SetPasswordHash(passwordHasher.HashPassword(adminUser, "Admin@123456"));
            adminUser.UpdateRoles([adminRole.Id]);

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
