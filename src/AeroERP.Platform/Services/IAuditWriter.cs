namespace AeroERP.Platform.Services;

/// <summary>
/// Audit Writer 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface IAuditWriter
{
    /// <summary>
    /// 执行Write。
    /// </summary>
    /// <param name="category">业务分类。</param>
    /// <param name="action">业务动作。</param>
    /// <param name="actor">操作人。</param>
    /// <param name="detail">详细说明。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task WriteAsync(string category, string action, string actor, string detail, CancellationToken cancellationToken);
}
