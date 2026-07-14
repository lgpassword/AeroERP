namespace AeroERP.Modules.Finance.Domain;

/// <summary>
/// Accounting Account 类型常量。
/// </summary>
public static class AccountingAccountType
{
    /// <summary>
    /// Asset。
    /// </summary>
    public const string Asset = "Asset";
    /// <summary>
    /// Liability。
    /// </summary>
    public const string Liability = "Liability";
    /// <summary>
    /// Equity。
    /// </summary>
    public const string Equity = "Equity";
    /// <summary>
    /// Revenue。
    /// </summary>
    public const string Revenue = "Revenue";
    /// <summary>
    /// Expense。
    /// </summary>
    public const string Expense = "Expense";
    /// <summary>
    /// Cost。
    /// </summary>
    public const string Cost = "Cost";

    /// <summary>
    /// Is Valid。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    public static bool IsValid(string value) =>
        string.Equals(value, Asset, StringComparison.Ordinal) ||
        string.Equals(value, Liability, StringComparison.Ordinal) ||
        string.Equals(value, Equity, StringComparison.Ordinal) ||
        string.Equals(value, Revenue, StringComparison.Ordinal) ||
        string.Equals(value, Expense, StringComparison.Ordinal) ||
        string.Equals(value, Cost, StringComparison.Ordinal);
}
