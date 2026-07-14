using AeroERP.BuildingBlocks.Abstractions;

namespace AeroERP.Platform.Infrastructure.Persistence;

/// <summary>
/// Document Exchange Plugin Schema Initializer 业务对象。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
public sealed class DocumentExchangePluginSchemaInitializer(AeroErpDbContext dbContext) : IPluginSchemaInitializer
{
    public string PluginKey => "aeroerp.document-exchange";

    /// <summary>
    /// Initialize Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public Task InitializeAsync(CancellationToken cancellationToken) =>
        SchemaBootstrapper.EnsureDocumentExchangeSchemaAsync(dbContext, cancellationToken);
}
