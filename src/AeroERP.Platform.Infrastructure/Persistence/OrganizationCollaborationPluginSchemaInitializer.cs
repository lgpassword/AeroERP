using AeroERP.BuildingBlocks.Abstractions;

namespace AeroERP.Platform.Infrastructure.Persistence;

/// <summary>
/// 组织协同插件表结构初始化器。
/// </summary>
public sealed class OrganizationCollaborationPluginSchemaInitializer(AeroErpDbContext dbContext) : IPluginSchemaInitializer
{
    public string PluginKey => "aeroerp.organization";

    public Task InitializeAsync(CancellationToken cancellationToken) =>
        SchemaBootstrapper.EnsureOrganizationCollaborationSchemaAsync(dbContext, cancellationToken);
}
