using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.AdvancedManufacturing.Domain;

/// <summary>
/// Manufacturing Routing 业务对象。
/// </summary>
public sealed class ManufacturingRouting : Entity, IAggregateRoot
{
    /// <summary>
    /// _operations。
    /// </summary>
    private readonly List<RoutingOperation> _operations = [];

    /// <summary>
    /// 初始化Manufacturing Routing实例。
    /// </summary>
    private ManufacturingRouting()
    {
    }

    /// <summary>
    /// 初始化Manufacturing Routing实例。
    /// </summary>
    /// <param name="routingNo">routing No 参数。</param>
    /// <param name="finishedItemId">finished Item Id 参数。</param>
    /// <param name="finishedItemCode">finished Item Code 参数。</param>
    /// <param name="finishedItemName">finished Item Name 参数。</param>
    /// <param name="version">版本号。</param>
    /// <param name="createdBy">创建人。</param>
    /// <param name="operations">工序集合。</param>
    public ManufacturingRouting(
        string routingNo,
        Guid finishedItemId,
        string finishedItemCode,
        string finishedItemName,
        string version,
        string createdBy,
        IEnumerable<RoutingOperation> operations)
    {
        RoutingNo = routingNo;
        FinishedItemId = finishedItemId;
        FinishedItemCode = finishedItemCode;
        FinishedItemName = finishedItemName;
        Version = version;
        CreatedBy = createdBy;
        _operations.AddRange(operations);
    }

    /// <summary>
    /// Routing No。
    /// </summary>
    public string RoutingNo { get; private set; } = string.Empty;
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
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = AdvancedManufacturingStatus.Draft;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    public IReadOnlyCollection<RoutingOperation> Operations => _operations;

    /// <summary>
    /// Replace Operations。
    /// </summary>
    /// <param name="operations">工序集合。</param>
    public void ReplaceOperations(IEnumerable<RoutingOperation> operations)
    {
        _operations.Clear();
        _operations.AddRange(operations);
        Status = AdvancedManufacturingStatus.Draft;
        Touch();
    }

    /// <summary>
    /// Activate。
    /// </summary>
    public void Activate()
    {
        Status = AdvancedManufacturingStatus.Active;
        Touch();
    }
}
