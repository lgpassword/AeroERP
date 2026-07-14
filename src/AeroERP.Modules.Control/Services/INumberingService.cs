namespace AeroERP.Modules.Control.Services;

/// <summary>
/// Numbering Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface INumberingService
{
    /// <summary>
    /// 执行Next。
    /// </summary>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="fallbackPrefix">fallback Prefix 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<string> NextAsync(string documentType, string fallbackPrefix, CancellationToken cancellationToken);
}
