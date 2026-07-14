namespace AeroERP.BuildingBlocks.Abstractions;

/// <summary>
/// Plugin Schema Initializer 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IPluginSchemaInitializer
{
    string PluginKey { get; }
    /// <summary>
    /// 执行Initialize。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task InitializeAsync(CancellationToken cancellationToken);
}
