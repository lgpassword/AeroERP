using AeroERP.Modules.AdvancedManufacturing.Services;
using AeroERP.Modules.Control.Services;
using AeroERP.Modules.DocumentExchange.Services;
using AeroERP.Modules.MasterData.Services;
using AeroERP.Modules.Finance.Services;
using AeroERP.Modules.Integration.Services;
using AeroERP.Modules.Inventory.Services;
using AeroERP.Modules.Localization.Services;
using AeroERP.Modules.Manufacturing.Services;
using AeroERP.Modules.MobileWork.Services;
using AeroERP.Modules.Planning.Services;
using AeroERP.Modules.PositionPermissions.Services;
using AeroERP.Modules.Procurement.Services;
using AeroERP.Modules.Quality.Services;
using AeroERP.Modules.Reporting.Services;
using AeroERP.Modules.Sales.Services;
using AeroERP.Modules.Workflow.Services;
using AeroERP.Modules.Wms.Services;
using AeroERP.BuildingBlocks.Abstractions;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Infrastructure.Services;
using AeroERP.Platform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroERP.Platform.Infrastructure;

/// <summary>
/// Service Collection Extensions 业务对象。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Aero Erp Infrastructure。
    /// </summary>
    /// <param name="services">依赖注入服务集合。</param>
    /// <param name="configuration">应用配置。</param>
    public static IServiceCollection AddAeroErpInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var postgres = configuration.GetConnectionString("Postgres");
        var sqlite = configuration.GetConnectionString("Sqlite") ?? "Data Source=data/aeroerp-dev.db";

        services.AddDbContext<AeroErpDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(postgres))
            {
                options.UseNpgsql(postgres);
            }
            else
            {
                options.UseSqlite(sqlite);
            }
        });

        services.AddScoped<IAeroErpDbContext>(sp => sp.GetRequiredService<AeroErpDbContext>());
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<PasswordHasher<AppUser>>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IModuleVisibilityService, ModuleVisibilityService>();
        services.AddScoped<IAgentReviewService, AgentReviewService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IProcurementService, ProcurementService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IControlService, ControlService>();
        services.AddScoped<INumberingService, NumberingService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IManufacturingService, ManufacturingService>();
        services.AddScoped<IQualityService, QualityService>();
        services.AddScoped<IPlanningService, PlanningService>();
        services.AddScoped<IPositionPermissionService, PositionPermissionService>();
        services.AddScoped<IWmsService, WmsService>();
        services.AddScoped<IAdvancedManufacturingService, AdvancedManufacturingService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IMobileWorkService, MobileWorkService>();
        services.AddScoped<IIntegrationService, IntegrationService>();
        services.AddScoped<IDocumentExchangeService, DocumentExchangeService>();
        services.AddScoped<IPluginSchemaInitializer, CorePluginSchemaInitializer>();
        services.AddScoped<IPluginSchemaInitializer, PositionPermissionPluginSchemaInitializer>();
        services.AddScoped<IPluginSchemaInitializer, WmsPluginSchemaInitializer>();
        services.AddScoped<IPluginSchemaInitializer, AdvancedManufacturingPluginSchemaInitializer>();
        services.AddScoped<IPluginSchemaInitializer, ReportingPluginSchemaInitializer>();
        services.AddScoped<IPluginSchemaInitializer, MobileWorkPluginSchemaInitializer>();
        services.AddScoped<IPluginSchemaInitializer, IntegrationPluginSchemaInitializer>();
        services.AddScoped<IPluginSchemaInitializer, DocumentExchangePluginSchemaInitializer>();

        return services;
    }
}
