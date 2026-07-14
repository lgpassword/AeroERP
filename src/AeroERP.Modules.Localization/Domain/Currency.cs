using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Localization.Domain;

/// <summary>
/// Currency 业务对象。
/// </summary>
public sealed class Currency : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Currency实例。
    /// </summary>
    private Currency()
    {
    }

    /// <summary>
    /// 初始化Currency实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="symbol">币种符号。</param>
    /// <param name="exchangeRateToBase">exchange Rate To Base 参数。</param>
    /// <param name="isBase">is Base 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public Currency(string code, string name, string symbol, decimal exchangeRateToBase, bool isBase, bool isEnabled)
    {
        Code = code;
        Name = name;
        Symbol = symbol;
        ExchangeRateToBase = exchangeRateToBase;
        IsBase = isBase;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// 业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// Symbol。
    /// </summary>
    public string Symbol { get; private set; } = string.Empty;
    /// <summary>
    /// Exchange Rate To Base。
    /// </summary>
    public decimal ExchangeRateToBase { get; private set; } = 1m;
    /// <summary>
    /// Is Base。
    /// </summary>
    public bool IsBase { get; private set; }
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="name">显示名称。</param>
    /// <param name="symbol">币种符号。</param>
    /// <param name="exchangeRateToBase">exchange Rate To Base 参数。</param>
    /// <param name="isBase">is Base 参数。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string name, string symbol, decimal exchangeRateToBase, bool isBase, bool isEnabled)
    {
        Name = name;
        Symbol = symbol;
        ExchangeRateToBase = exchangeRateToBase;
        IsBase = isBase;
        IsEnabled = isEnabled;
        Touch();
    }
}
