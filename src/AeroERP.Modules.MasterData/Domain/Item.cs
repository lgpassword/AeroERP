using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.MasterData.Domain;

/// <summary>
/// Item 业务对象。
/// </summary>
public sealed class Item : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Item实例。
    /// </summary>
    private Item()
    {
    }

    /// <summary>
    /// 初始化Item实例。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="specification">规格型号。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="isEnabled">是否启用。</param>
    public Item(string code, string name, string specification, string unit, bool isEnabled)
    {
        Code = code;
        Name = name;
        Specification = specification;
        Unit = unit;
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
    /// Specification。
    /// </summary>
    public string Specification { get; private set; } = string.Empty;
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// 更新当前业务对象。
    /// </summary>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="specification">规格型号。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="isEnabled">是否启用。</param>
    public void Update(string code, string name, string specification, string unit, bool isEnabled)
    {
        Code = code;
        Name = name;
        Specification = specification;
        Unit = unit;
        IsEnabled = isEnabled;
        Touch();
    }
}
