using AeroERP.Modules.Control.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Control.Services;

/// <summary>
/// Numbering Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
public sealed class NumberingService(AeroErpDbContext dbContext) : INumberingService
{
    /// <summary>
    /// Next Async。
    /// </summary>
    /// <param name="documentType">业务单据类型。</param>
    /// <param name="fallbackPrefix">fallback Prefix 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<string> NextAsync(string documentType, string fallbackPrefix, CancellationToken cancellationToken)
    {
        var rule = await dbContext.NumberingRules.FirstOrDefaultAsync(
            x => x.DocumentType == documentType && x.IsEnabled,
            cancellationToken);

        if (rule is null)
        {
            return $"{fallbackPrefix}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        return rule.Generate();
    }
}
