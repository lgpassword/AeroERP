namespace AeroERP.BuildingBlocks.Domain;

/// <summary>
/// Entity 业务对象。
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// 主键标识。
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();
    /// <summary>
    /// 创建时间，使用 UTC。
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
    /// <summary>
    /// 最后更新时间，使用 UTC。
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Touch。
    /// </summary>
    public void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
