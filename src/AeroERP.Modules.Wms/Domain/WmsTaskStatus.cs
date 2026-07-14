namespace AeroERP.Modules.Wms.Domain;

/// <summary>
/// Wms Task 状态常量。
/// </summary>
public static class WmsTaskStatus
{
    /// <summary>
    /// Planned。
    /// </summary>
    public const string Planned = "Planned";
    /// <summary>
    /// Released。
    /// </summary>
    public const string Released = "Released";
    /// <summary>
    /// Completed。
    /// </summary>
    public const string Completed = "Completed";
    /// <summary>
    /// 判断是否允许celled。
    /// </summary>
    public const string Cancelled = "Cancelled";
}
