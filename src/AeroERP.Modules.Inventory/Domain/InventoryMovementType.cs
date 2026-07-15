namespace AeroERP.Modules.Inventory.Domain;

/// <summary>
/// Inventory Movement 类型常量。
/// </summary>
public static class InventoryMovementType
{
    /// <summary>
    /// Receipt。
    /// </summary>
    public const string Receipt = "Receipt";
    /// <summary>
    /// Issue。
    /// </summary>
    public const string Issue = "Issue";
    /// <summary>
    /// Transfer Out。
    /// </summary>
    public const string TransferOut = "TransferOut";
    /// <summary>
    /// Transfer In。
    /// </summary>
    public const string TransferIn = "TransferIn";
    /// <summary>
    /// Count Increase。
    /// </summary>
    public const string CountIncrease = "CountIncrease";
    /// <summary>
    /// Count Decrease。
    /// </summary>
    public const string CountDecrease = "CountDecrease";
}
