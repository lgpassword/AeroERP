using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Manufacturing.Domain;

/// <summary>
/// Bill Of Material 业务对象。
/// </summary>
public sealed class BillOfMaterial : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Bill Of Material实例。
    /// </summary>
    private BillOfMaterial()
    {
    }

    /// <summary>
    /// 初始化Bill Of Material实例。
    /// </summary>
    /// <param name="bomNo">bom No 参数。</param>
    /// <param name="finishedItemId">finished Item Id 参数。</param>
    /// <param name="finishedItemCode">finished Item Code 参数。</param>
    /// <param name="finishedItemName">finished Item Name 参数。</param>
    /// <param name="version">版本号。</param>
    /// <param name="baseQuantity">base Quantity 参数。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="lines">明细行集合。</param>
    public BillOfMaterial(
        string bomNo,
        Guid finishedItemId,
        string finishedItemCode,
        string finishedItemName,
        string version,
        decimal baseQuantity,
        string unit,
        bool isEnabled,
        IEnumerable<BillOfMaterialLine> lines)
    {
        BomNo = bomNo;
        FinishedItemId = finishedItemId;
        FinishedItemCode = finishedItemCode;
        FinishedItemName = finishedItemName;
        Version = version;
        BaseQuantity = baseQuantity;
        Unit = unit;
        IsEnabled = isEnabled;
        Lines = lines.ToList();
    }

    /// <summary>
    /// Bom No。
    /// </summary>
    public string BomNo { get; private set; } = string.Empty;
    /// <summary>
    /// Finished Item Id。
    /// </summary>
    public Guid FinishedItemId { get; private set; }
    /// <summary>
    /// Finished Item Code。
    /// </summary>
    public string FinishedItemCode { get; private set; } = string.Empty;
    /// <summary>
    /// Finished Item Name。
    /// </summary>
    public string FinishedItemName { get; private set; } = string.Empty;
    /// <summary>
    /// Version。
    /// </summary>
    public string Version { get; private set; } = string.Empty;
    /// <summary>
    /// Base Quantity。
    /// </summary>
    public decimal BaseQuantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }
    /// <summary>
    /// 明细行集合。
    /// </summary>
    public List<BillOfMaterialLine> Lines { get; private set; } = [];
}
