namespace AeroERP.BuildingBlocks.Results;

/// <summary>
/// Operation Result 数据记录。
/// </summary>
/// <param name="IsSuccess">Is Success 参数。</param>
/// <param name="Value">数值或配置值。</param>
/// <param name="Error">错误信息。</param>
public sealed record OperationResult<T>(bool IsSuccess, T? Value, string? Error)
{
    /// <summary>
    /// Success。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    public static OperationResult<T> Success(T value) => new(true, value, null);
    /// <summary>
    /// Failure。
    /// </summary>
    /// <param name="error">错误信息。</param>
    public static OperationResult<T> Failure(string error) => new(false, default, error);
}
