using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Bank Account 业务对象。
/// </summary>
public sealed class BankAccount : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Bank Account实例。
    /// </summary>
    private BankAccount()
    {
    }

    /// <summary>
    /// 初始化Bank Account实例。
    /// </summary>
    /// <param name="accountNo">account No 参数。</param>
    /// <param name="accountName">account Name 参数。</param>
    /// <param name="bankName">bank Name 参数。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public BankAccount(
        string accountNo,
        string accountName,
        string bankName,
        string currencyCode,
        bool isEnabled,
        string updatedBy)
    {
        AccountNo = accountNo;
        AccountName = accountName;
        BankName = bankName;
        CurrencyCode = currencyCode;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Account No。
    /// </summary>
    public string AccountNo { get; private set; } = string.Empty;
    /// <summary>
    /// Account Name。
    /// </summary>
    public string AccountName { get; private set; } = string.Empty;
    /// <summary>
    /// Bank Name。
    /// </summary>
    public string BankName { get; private set; } = string.Empty;
    /// <summary>
    /// 币种编码。
    /// </summary>
    public string CurrencyCode { get; private set; } = "CNY";
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; } = true;
    /// <summary>
    /// 最后更新人。
    /// </summary>
    public string UpdatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="accountName">account Name 参数。</param>
    /// <param name="bankName">bank Name 参数。</param>
    /// <param name="currencyCode">币种编码。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="updatedBy">最后更新人。</param>
    public void Update(string accountName, string bankName, string currencyCode, bool isEnabled, string updatedBy)
    {
        AccountName = accountName;
        BankName = bankName;
        CurrencyCode = currencyCode;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Touch();
    }
}
