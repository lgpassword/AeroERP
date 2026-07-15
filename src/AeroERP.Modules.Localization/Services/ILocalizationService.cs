using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Localization.Contracts;

namespace AeroERP.Modules.Localization.Services;

/// <summary>
/// Localization Service 服务契约，定义模块对外提供的业务能力。
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// 查询Currencies。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<CurrencyDto>> ListCurrenciesAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Currency。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<CurrencyDto>> UpsertCurrencyAsync(UpsertCurrencyRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 获取Settings。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<LocalizationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 更新Settings。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<LocalizationSettingsDto>> UpdateSettingsAsync(UpdateLocalizationSettingsRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// 查询Content。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<IReadOnlyList<LocalizationContentDto>> ListContentAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 执行Upsert Content。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    Task<OperationResult<LocalizationContentDto>> UpsertContentAsync(UpsertLocalizationContentRequest request, CancellationToken cancellationToken);
}
