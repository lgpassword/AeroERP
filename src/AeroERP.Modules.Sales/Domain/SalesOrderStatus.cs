namespace AeroERP.Modules.Sales.Domain;

/// <summary>
/// Sales Order 状态常量。
/// </summary>
public static class SalesOrderStatus
{
    /// <summary>
    /// 创建d。
    /// </summary>
    public const string Created = "Created";
    /// <summary>
    /// Confirmed。
    /// </summary>
    public const string Confirmed = "Confirmed";
    /// <summary>
    /// Ready To Ship。
    /// </summary>
    public const string ReadyToShip = "ReadyToShip";
    /// <summary>
    /// Shipped。
    /// </summary>
    public const string Shipped = "Shipped";
}
