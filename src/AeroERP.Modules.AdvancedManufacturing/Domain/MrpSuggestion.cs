using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.AdvancedManufacturing.Domain;

/// <summary>
/// Mrp Suggestion 业务对象。
/// </summary>
public sealed class MrpSuggestion : Entity, IAggregateRoot
{
    /// <summary>
    /// 初始化Mrp Suggestion实例。
    /// </summary>
    private MrpSuggestion()
    {
    }

    /// <summary>
    /// 初始化Mrp Suggestion实例。
    /// </summary>
    /// <param name="suggestionNo">suggestion No 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="currentQuantity">current Quantity 参数。</param>
    /// <param name="demandQuantity">demand Quantity 参数。</param>
    /// <param name="supplyQuantity">supply Quantity 参数。</param>
    /// <param name="suggestedQuantity">suggested Quantity 参数。</param>
    /// <param name="sourceType">来源单据类型。</param>
    /// <param name="createdBy">创建人。</param>
    public MrpSuggestion(
        string suggestionNo,
        Guid itemId,
        string itemCode,
        string itemName,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        decimal currentQuantity,
        decimal demandQuantity,
        decimal supplyQuantity,
        decimal suggestedQuantity,
        string sourceType,
        string createdBy)
    {
        SuggestionNo = suggestionNo;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        CurrentQuantity = currentQuantity;
        DemandQuantity = demandQuantity;
        SupplyQuantity = supplyQuantity;
        SuggestedQuantity = suggestedQuantity;
        SourceType = sourceType;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Suggestion No。
    /// </summary>
    public string SuggestionNo { get; private set; } = string.Empty;
    /// <summary>
    /// Item Id。
    /// </summary>
    public Guid ItemId { get; private set; }
    /// <summary>
    /// Item Code。
    /// </summary>
    public string ItemCode { get; private set; } = string.Empty;
    /// <summary>
    /// Item Name。
    /// </summary>
    public string ItemName { get; private set; } = string.Empty;
    /// <summary>
    /// Warehouse Id。
    /// </summary>
    public Guid WarehouseId { get; private set; }
    /// <summary>
    /// Warehouse Code。
    /// </summary>
    public string WarehouseCode { get; private set; } = string.Empty;
    /// <summary>
    /// Warehouse Name。
    /// </summary>
    public string WarehouseName { get; private set; } = string.Empty;
    /// <summary>
    /// Current Quantity。
    /// </summary>
    public decimal CurrentQuantity { get; private set; }
    /// <summary>
    /// Demand Quantity。
    /// </summary>
    public decimal DemandQuantity { get; private set; }
    /// <summary>
    /// Supply Quantity。
    /// </summary>
    public decimal SupplyQuantity { get; private set; }
    /// <summary>
    /// Suggested Quantity。
    /// </summary>
    public decimal SuggestedQuantity { get; private set; }
    /// <summary>
    /// 来源单据类型。
    /// </summary>
    public string SourceType { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = AdvancedManufacturingStatus.Open;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Decided By。
    /// </summary>
    public string DecidedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Decision Note。
    /// </summary>
    public string DecisionNote { get; private set; } = string.Empty;
    /// <summary>
    /// Decided At Utc。
    /// </summary>
    public DateTimeOffset? DecidedAtUtc { get; private set; }

    /// <summary>
    /// Decide。
    /// </summary>
    /// <param name="decision">处理决策。</param>
    /// <param name="note">备注。</param>
    /// <param name="actor">操作人。</param>
    public void Decide(string decision, string note, string actor)
    {
        Status = decision;
        DecisionNote = note;
        DecidedBy = actor;
        DecidedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}
