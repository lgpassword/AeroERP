using AeroERP.BuildingBlocks.Abstractions;

namespace AeroERP.Platform.Infrastructure.Persistence;

/// <summary>
/// Core Plugin Schema Initializer 业务对象。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
public sealed class CorePluginSchemaInitializer(AeroErpDbContext dbContext) : IPluginSchemaInitializer
{
    public string PluginKey => "aeroerp.core";

    /// <summary>
    /// Initialize Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public Task InitializeAsync(CancellationToken cancellationToken) =>
        SchemaBootstrapper.EnsureIncrementalSchemaAsync(dbContext, cancellationToken);
}
