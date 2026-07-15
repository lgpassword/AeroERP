using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Platform.Domain;

/// <summary>
/// Organization 业务对象。
/// </summary>
public sealed class Organization : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Organization实例。
    /// </summary>
    private Organization()
    {
    }

    /// <summary>
    /// 初始化Organization实例。
    /// </summary>
    /// <param name="name">显示名称。</param>
    /// <param name="defaultRole">default Role 参数。</param>
    /// <param name="regionCode">region Code 参数。</param>
    public Organization(string name, string defaultRole, string regionCode)
    {
        Name = name;
        DefaultRole = defaultRole;
        RegionCode = regionCode;
    }

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// Default Role。
    /// </summary>
    public string DefaultRole { get; private set; } = string.Empty;
    /// <summary>
    /// Region Code。
    /// </summary>
    public string RegionCode { get; private set; } = string.Empty;
}
