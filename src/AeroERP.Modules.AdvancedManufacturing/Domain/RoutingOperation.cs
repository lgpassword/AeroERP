using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.AdvancedManufacturing.Domain;

/// <summary>
/// Routing Operation 业务对象。
/// </summary>
public sealed class RoutingOperation : Entity
{
    /// <summary>
    /// 初始化Routing Operation实例。
    /// </summary>
    private RoutingOperation()
    {
    }

    /// <summary>
    /// 初始化Routing Operation实例。
    /// </summary>
    /// <param name="routingId">routing Id 参数。</param>
    /// <param name="sequence">排序序号。</param>
    /// <param name="operationCode">operation Code 参数。</param>
    /// <param name="operationName">operation Name 参数。</param>
    /// <param name="workCenterId">work Center Id 参数。</param>
    /// <param name="workCenterCode">work Center Code 参数。</param>
    /// <param name="workCenterName">work Center Name 参数。</param>
    /// <param name="standardMinutes">standard Minutes 参数。</param>
    /// <param name="laborCostRate">labor Cost Rate 参数。</param>
    /// <param name="machineCostRate">machine Cost Rate 参数。</param>
    public RoutingOperation(
        Guid routingId,
        int sequence,
        string operationCode,
        string operationName,
        Guid workCenterId,
        string workCenterCode,
        string workCenterName,
        decimal standardMinutes,
        decimal laborCostRate,
        decimal machineCostRate)
    {
        ManufacturingRoutingId = routingId;
        Sequence = sequence;
        OperationCode = operationCode;
        OperationName = operationName;
        WorkCenterId = workCenterId;
        WorkCenterCode = workCenterCode;
        WorkCenterName = workCenterName;
        StandardMinutes = standardMinutes;
        LaborCostRate = laborCostRate;
        MachineCostRate = machineCostRate;
    }

    /// <summary>
    /// Manufacturing Routing Id。
    /// </summary>
    public Guid ManufacturingRoutingId { get; private set; }
    /// <summary>
    /// Sequence。
    /// </summary>
    public int Sequence { get; private set; }
    /// <summary>
    /// Operation Code。
    /// </summary>
    public string OperationCode { get; private set; } = string.Empty;
    /// <summary>
    /// Operation Name。
    /// </summary>
    public string OperationName { get; private set; } = string.Empty;
    /// <summary>
    /// Work Center Id。
    /// </summary>
    public Guid WorkCenterId { get; private set; }
    /// <summary>
    /// Work Center Code。
    /// </summary>
    public string WorkCenterCode { get; private set; } = string.Empty;
    /// <summary>
    /// Work Center Name。
    /// </summary>
    public string WorkCenterName { get; private set; } = string.Empty;
    /// <summary>
    /// Standard Minutes。
    /// </summary>
    public decimal StandardMinutes { get; private set; }
    /// <summary>
    /// Labor Cost Rate。
    /// </summary>
    public decimal LaborCostRate { get; private set; }
    /// <summary>
    /// Machine Cost Rate。
    /// </summary>
    public decimal MachineCostRate { get; private set; }
}
