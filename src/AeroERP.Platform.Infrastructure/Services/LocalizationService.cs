using AeroERP.BuildingBlocks.Results;
using AeroERP.Modules.Localization.Contracts;
using AeroERP.Modules.Localization.Domain;
using AeroERP.Modules.Localization.Services;
using AeroERP.Platform.Infrastructure.Persistence;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Modules.Localization.Services;

/// <summary>
/// Localization Service 业务服务实现，承载模块核心应用逻辑。
/// </summary>
/// <param name="dbContext">db Context 参数。</param>
/// <param name="auditWriter">audit Writer 参数。</param>
/// <param name="currentUser">current User 参数。</param>
public sealed class LocalizationService(AeroErpDbContext dbContext, IAuditWriter auditWriter, ICurrentUserAccessor currentUser) : ILocalizationService
{
    /// <summary>
    /// readonly。
    /// </summary>
    /// <param name="Key">业务键。</param>
    /// <param name="Category">业务分类。</param>
    /// <param name="ChineseText">Chinese Text 参数。</param>
    /// <param name="EnglishText">English Text 参数。</param>
    private static readonly (string Key, string Category, string ChineseText, string EnglishText)[] DefaultContent =
    [
        ("app.name", "应用框架", "AeroERP", "AeroERP"),
        ("app.workspace", "应用框架", "AeroERP 工作台", "AeroERP Workspace"),
        ("app.tagline", "应用框架", "模块化企业运营平台", "Modular enterprise operations platform"),
        ("action.logout", "通用动作", "退出登录", "Sign out"),
        ("action.refresh", "通用动作", "刷新数据", "Refresh"),
        ("language.zh", "语言切换", "中文", "Chinese"),
        ("language.en", "语言切换", "英文", "English"),
        ("language.current", "语言切换", "界面语言", "Language"),
        ("module.platform", "模块导航", "平台治理", "Platform Governance"),
        ("module.master-data", "模块导航", "主数据", "Master Data"),
        ("module.procurement", "模块导航", "采购管理", "Procurement"),
        ("module.sales", "模块导航", "销售管理", "Sales"),
        ("module.inventory", "模块导航", "库存管理", "Inventory"),
        ("module.finance", "模块导航", "财务结算", "Finance"),
        ("module.workflow", "模块导航", "审批中心", "Workflow"),
        ("module.control", "模块导航", "经营管控", "Business Control"),
        ("module.localization", "模块导航", "语言与本地化", "Language and Localization"),
        ("module.manufacturing", "模块导航", "制造管理", "Manufacturing"),
        ("module.quality", "模块导航", "质量追溯", "Quality Traceability"),
        ("module.planning", "模块导航", "计划执行", "Planning Execution"),
        ("role.platform-admin", "职位角色", "平台管理员", "Platform Administrator"),
        ("role.operations-manager", "职位角色", "运营经理", "Operations Manager"),
        ("role.purchaser", "职位角色", "采购专员", "Purchaser"),
        ("localization.title", "语言与本地化", "语言与本地化", "Language and Localization"),
        ("localization.no-access.title", "语言与本地化", "无语言与本地化权限", "No Language and Localization Permission"),
        ("localization.no-access.description", "语言与本地化", "当前账号不能查看组织、币种、本地化设置或界面文本。", "This account cannot view organizations, currencies, localization settings, or interface text."),
        ("localization.content.title", "语言与本地化", "界面与内容翻译", "Interface and Content Translation"),
        ("localization.content.hint", "语言与本地化", "维护中文内容对应的英文文本；切换到英文时会优先使用这里保存的英文内容。", "Maintain English text for Chinese content; English mode uses the saved English text first."),
        ("localization.content.empty.title", "语言与本地化", "暂无翻译内容", "No Translation Content"),
        ("localization.content.empty.description", "语言与本地化", "系统会自动准备基础界面文本，也可以手工新增业务内容翻译。", "The system prepares base interface text automatically, and business content translations can be added manually."),
        ("localization.content.saved", "语言与本地化", "翻译内容已保存。", "Translation content saved."),
        ("localization.content.add", "语言与本地化", "新增翻译内容", "Add Translation Content"),
        ("localization.content.key", "语言与本地化", "内容键", "Content Key"),
        ("localization.content.category", "语言与本地化", "分类", "Category"),
        ("localization.content.zh", "语言与本地化", "中文内容", "Chinese Text"),
        ("localization.content.en", "语言与本地化", "英文内容", "English Text"),
        ("localization.content.enabled", "语言与本地化", "启用翻译", "Enable Translation"),
        ("localization.content.save", "语言与本地化", "保存翻译", "Save Translation")
    ];

    /// <summary>
    /// 查询Currencies。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<CurrencyDto>> ListCurrenciesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Currencies.OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return entities.Select(Map).ToList();
    }

    /// <summary>
    /// Upsert Currency Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<CurrencyDto>> UpsertCurrencyAsync(UpsertCurrencyRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name) || request.ExchangeRateToBase <= 0)
        {
            return OperationResult<CurrencyDto>.Failure("币种代码、名称和汇率必须有效。");
        }

        var entity = await dbContext.Currencies.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (entity is null)
        {
            entity = new Currency(code, request.Name.Trim(), request.Symbol.Trim(), request.ExchangeRateToBase, request.IsBase, request.IsEnabled);
            dbContext.Currencies.Add(entity);
        }
        else
        {
            entity.Update(request.Name.Trim(), request.Symbol.Trim(), request.ExchangeRateToBase, request.IsBase, request.IsEnabled);
        }

        if (request.IsBase)
        {
            var others = await dbContext.Currencies.Where(x => x.Code != code && x.IsBase).ToListAsync(cancellationToken);
            foreach (var other in others)
            {
                other.Update(other.Name, other.Symbol, other.ExchangeRateToBase, false, other.IsEnabled);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Localization", "CurrencyUpserted", currentUser.GetActor(), code, cancellationToken);
        return OperationResult<CurrencyDto>.Success(Map(entity));
    }

    /// <summary>
    /// 获取Settings。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<LocalizationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        return Map(settings);
    }

    /// <summary>
    /// 更新Settings。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<LocalizationSettingsDto>> UpdateSettingsAsync(UpdateLocalizationSettingsRequest request, CancellationToken cancellationToken)
    {
        if (request.DefaultTaxRate is < 0 or > 1)
        {
            return OperationResult<LocalizationSettingsDto>.Failure("默认税率必须在 0 到 1 之间。");
        }

        var currencyCode = request.DefaultCurrencyCode.Trim().ToUpperInvariant();
        var currencyExists = await dbContext.Currencies.AnyAsync(x => x.Code == currencyCode && x.IsEnabled, cancellationToken);
        if (!currencyExists)
        {
            return OperationResult<LocalizationSettingsDto>.Failure("默认币种不存在或已停用。");
        }

        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.Update(currencyCode, request.TaxInvoiceType.Trim(), request.TaxpayerId.Trim(), request.InvoiceTitle.Trim(), request.DefaultTaxRate);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Localization", "SettingsUpdated", currentUser.GetActor(), currencyCode, cancellationToken);
        return OperationResult<LocalizationSettingsDto>.Success(Map(settings));
    }

    /// <summary>
    /// 查询Content。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<IReadOnlyList<LocalizationContentDto>> ListContentAsync(CancellationToken cancellationToken)
    {
        await EnsureDefaultContentAsync(cancellationToken);
        var entities = await dbContext.LocalizationContents
            .AsNoTracking()
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToList();
    }

    /// <summary>
    /// Upsert Content Async。
    /// </summary>
    /// <param name="request">请求参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public async Task<OperationResult<LocalizationContentDto>> UpsertContentAsync(UpsertLocalizationContentRequest request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim();
        var category = string.IsNullOrWhiteSpace(request.Category) ? "自定义内容" : request.Category.Trim();
        var chineseText = request.ChineseText.Trim();
        var englishText = request.EnglishText.Trim();

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(chineseText))
        {
            return OperationResult<LocalizationContentDto>.Failure("内容键和中文内容不能为空。");
        }

        if (key.Length > 160 || category.Length > 64 || chineseText.Length > 512 || englishText.Length > 512)
        {
            return OperationResult<LocalizationContentDto>.Failure("内容键、分类或文本长度超过限制。");
        }

        var entity = await dbContext.LocalizationContents.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (entity is null)
        {
            entity = new LocalizationContent(key, category, chineseText, englishText, request.IsEnabled);
            dbContext.LocalizationContents.Add(entity);
        }
        else
        {
            entity.Update(category, chineseText, englishText, request.IsEnabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync("Localization", "ContentUpserted", currentUser.GetActor(), key, cancellationToken);
        return OperationResult<LocalizationContentDto>.Success(Map(entity));
    }

    /// <summary>
    /// 获取Or Create Settings。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task<LocalizationSettings> GetOrCreateSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.LocalizationSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new LocalizationSettings("CNY", "增值税普通发票", string.Empty, string.Empty, 0.13m);
        dbContext.LocalizationSettings.Add(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        return settings;
    }

    /// <summary>
    /// Ensure Default Content Async。
    /// </summary>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private async Task EnsureDefaultContentAsync(CancellationToken cancellationToken)
    {
        var existingKeys = await dbContext.LocalizationContents
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);
        var existing = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasChanges = false;

        foreach (var (key, category, chineseText, englishText) in DefaultContent)
        {
            if (existing.Contains(key))
            {
                continue;
            }

            dbContext.LocalizationContents.Add(new LocalizationContent(key, category, chineseText, englishText, true));
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="currency">币种对象。</param>
    private static CurrencyDto Map(Currency currency) =>
        new(currency.Id, currency.Code, currency.Name, currency.Symbol, currency.ExchangeRateToBase, currency.IsBase, currency.IsEnabled);

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="settings">本地化设置。</param>
    private static LocalizationSettingsDto Map(LocalizationSettings settings) =>
        new(settings.Id, settings.DefaultCurrencyCode, settings.TaxInvoiceType, settings.TaxpayerId, settings.InvoiceTitle, settings.DefaultTaxRate);

    /// <summary>
    /// 注册业务对象 路由。
    /// </summary>
    /// <param name="content">本地化内容。</param>
    private static LocalizationContentDto Map(LocalizationContent content) =>
        new(content.Id, content.Key, content.Category, content.ChineseText, content.EnglishText, content.IsEnabled, content.UpdatedAtUtc);
}
