using AeroERP.BuildingBlocks.Abstractions;

namespace AeroERP.Platform.Infrastructure.Persistence;

/// <summary>
/// Wms Plugin Schema Initializer 业务对象。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
public sealed class WmsPluginSchemaInitializer(AeroErpDbContext dbContext) : IPluginSchemaInitializer
{
    public string PluginKey => "aeroerp.wms";

    /// <summary>
    /// Initialize Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public Task InitializeAsync(CancellationToken cancellationToken) =>
        SchemaBootstrapper.EnsureWmsSchemaAsync(dbContext, cancellationToken);
}
