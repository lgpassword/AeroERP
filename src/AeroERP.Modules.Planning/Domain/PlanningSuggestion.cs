using AeroERP.BuildingBlocks.Domain;

namespace AeroERP.Modules.Planning.Domain;

/// <summary>
/// Planning Suggestion 业务对象。
/// </summary>
public sealed class PlanningSuggestion : Entity
{
    /// <summary>
    /// 初始化Planning Suggestion实例。
    /// </summary>
    private PlanningSuggestion()
    {
    }

    /// <summary>
    /// 初始化Planning Suggestion实例。
    /// </summary>
    /// <param name="suggestionNo">suggestion No 参数。</param>
    /// <param name="warehouseId">仓库标识。</param>
    /// <param name="warehouseCode">warehouse Code 参数。</param>
    /// <param name="warehouseName">warehouse Name 参数。</param>
    /// <param name="itemId">物料标识。</param>
    /// <param name="itemCode">item Code 参数。</param>
    /// <param name="itemName">item Name 参数。</param>
    /// <param name="currentQuantity">current Quantity 参数。</param>
    /// <param name="minimumQuantity">minimum Quantity 参数。</param>
    /// <param name="suggestedQuantity">suggested Quantity 参数。</param>
    /// <param name="unit">计量单位。</param>
    /// <param name="createdBy">创建人。</param>
    public PlanningSuggestion(
        string suggestionNo,
        Guid warehouseId,
        string warehouseCode,
        string warehouseName,
        Guid itemId,
        string itemCode,
        string itemName,
        decimal currentQuantity,
        decimal minimumQuantity,
        decimal suggestedQuantity,
        string unit,
        string createdBy)
    {
        SuggestionNo = suggestionNo;
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
        WarehouseName = warehouseName;
        ItemId = itemId;
        ItemCode = itemCode;
        ItemName = itemName;
        CurrentQuantity = currentQuantity;
        MinimumQuantity = minimumQuantity;
        SuggestedQuantity = suggestedQuantity;
        Unit = unit;
        CreatedBy = createdBy;
        Status = PlanningSuggestionStatus.Open;
    }

    /// <summary>
    /// Suggestion No。
    /// </summary>
    public string SuggestionNo { get; private set; } = string.Empty;
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
    /// Current Quantity。
    /// </summary>
    public decimal CurrentQuantity { get; private set; }
    /// <summary>
    /// Minimum Quantity。
    /// </summary>
    public decimal MinimumQuantity { get; private set; }
    /// <summary>
    /// Suggested Quantity。
    /// </summary>
    public decimal SuggestedQuantity { get; private set; }
    /// <summary>
    /// 计量单位。
    /// </summary>
    public string Unit { get; private set; } = string.Empty;
    /// <summary>
    /// 当前业务状态。
    /// </summary>
    public string Status { get; private set; } = string.Empty;
    /// <summary>
    /// 创建人。
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    /// <summary>
    /// Decision Note。
    /// </summary>
    public string DecisionNote { get; private set; } = string.Empty;

    /// <summary>
    /// Decide。
    /// </summary>
    /// <param name="decision">处理决策。</param>
    /// <param name="note">备注。</param>
    public void Decide(string decision, string note)
    {
        if (!string.Equals(Status, PlanningSuggestionStatus.Open, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("只有待处理建议可以决策。");
        }

        Status = decision;
        DecisionNote = note;
        Touch();
    }
}
