namespace AeroERP.Modules.Manufacturing.Domain;

/// <summary>
/// Work Order 状态常量。
/// </summary>
public static class WorkOrderStatus
{
    /// <summary>
    /// Draft。
    /// </summary>
    public const string Draft = "Draft";
    /// <summary>
    /// Released。
    /// </summary>
    public const string Released = "Released";
    /// <summary>
    /// Materials Issued。
    /// </summary>
    public const string MaterialsIssued = "MaterialsIssued";
    /// <summary>
    /// Partially Completed。
    /// </summary>
    public const string PartiallyCompleted = "PartiallyCompleted";
    /// <summary>
    /// Completed。
    /// </summary>
    public const string Completed = "Completed";
}
