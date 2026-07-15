namespace AeroERP.Platform.Domain;

/// <summary>
/// Platform Role Catalog 业务对象。
/// </summary>
public static class PlatformRoleCatalog
{
    /// <summary>
    /// Platform Admin。
    /// </summary>
    public const string PlatformAdmin = "platform-admin";
    /// <summary>
    /// Operations Manager。
    /// </summary>
    public const string OperationsManager = "operations-manager";
    /// <summary>
    /// Purchaser。
    /// </summary>
    public const string Purchaser = "purchaser";

    /// <summary>
    /// Permission Map。
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> PermissionMap =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [PlatformAdmin] = PlatformPermissions.All,
            [OperationsManager] =
            [
                PlatformPermissions.AgentReviewSubmit,
                PlatformPermissions.MasterDataRead,
                PlatformPermissions.MasterDataManage,
                PlatformPermissions.ProcurementRead,
                PlatformPermissions.ProcurementRequestReview,
                PlatformPermissions.ProcurementOrderCreate,
                PlatformPermissions.ProcurementOrderRelease,
                PlatformPermissions.InventoryRead,
                PlatformPermissions.InventoryReceiptManage,
                PlatformPermissions.InventoryIssueManage,
                PlatformPermissions.InventoryTransferManage,
                PlatformPermissions.InventoryCountManage,
                PlatformPermissions.InventoryLocationManage,
                PlatformPermissions.WmsRead,
                PlatformPermissions.WmsManage,
                PlatformPermissions.WmsExecute,
                PlatformPermissions.MobileWorkRead,
                PlatformPermissions.MobileWorkManage,
                PlatformPermissions.MobileWorkExecute,
                PlatformPermissions.IntegrationRead,
                PlatformPermissions.IntegrationManage,
                PlatformPermissions.IntegrationExecute,
                PlatformPermissions.DocumentExchangeRead,
                PlatformPermissions.DocumentExchangeManage,
                PlatformPermissions.DocumentExchangeExecute,
                PlatformPermissions.SalesRead,
                PlatformPermissions.SalesQuotationCreate,
                PlatformPermissions.SalesOrderCreate,
                PlatformPermissions.SalesOrderManage,
                PlatformPermissions.FinanceRead,
                PlatformPermissions.FinanceAccountingManage,
                PlatformPermissions.FinanceVoucherManage,
                PlatformPermissions.FinanceVoucherReview,
                PlatformPermissions.FinancePayableManage,
                PlatformPermissions.FinanceReceivableManage,
                PlatformPermissions.FinanceSettlementManage,
                PlatformPermissions.WorkflowRead,
                PlatformPermissions.WorkflowTaskDecide,
                PlatformPermissions.NotificationRead,
                PlatformPermissions.ControlAnalyticsRead,
                PlatformPermissions.ControlDataScopeManage,
                PlatformPermissions.ControlNumberingManage,
                PlatformPermissions.LocalizationRead,
                PlatformPermissions.LocalizationManage,
                PlatformPermissions.ManufacturingRead,
                PlatformPermissions.ManufacturingBomManage,
                PlatformPermissions.ManufacturingWorkOrderManage,
                PlatformPermissions.ManufacturingExecutionManage,
                PlatformPermissions.AdvancedManufacturingRead,
                PlatformPermissions.AdvancedManufacturingManage,
                PlatformPermissions.AdvancedManufacturingSchedule,
                PlatformPermissions.AdvancedManufacturingCostManage,
                PlatformPermissions.AdvancedManufacturingMrpManage,
                PlatformPermissions.ReportingRead,
                PlatformPermissions.ReportingManage,
                PlatformPermissions.ReportingExport,
                PlatformPermissions.QualityRead,
                PlatformPermissions.QualityInspectionManage,
                PlatformPermissions.QualityTraceabilityManage,
                PlatformPermissions.PlanningRead,
                PlatformPermissions.PlanningManage,
                PlatformPermissions.OutsourcingManage,
                PlatformPermissions.BarcodeExecute
            ],
            [Purchaser] =
            [
                PlatformPermissions.AgentReviewSubmit,
                PlatformPermissions.MasterDataRead,
                PlatformPermissions.ProcurementRead,
                PlatformPermissions.ProcurementRequestCreate,
                PlatformPermissions.InventoryRead,
                PlatformPermissions.WorkflowRead,
                PlatformPermissions.NotificationRead
            ]
        };

    /// <summary>
    /// 获取Permissions。
    /// </summary>
    /// <param name="roleKey">role Key 参数。</param>
    public static IReadOnlyList<string> GetPermissions(string roleKey)
    {
        return PermissionMap.TryGetValue(roleKey, out var permissions)
            ? permissions
            : [];
    }

    /// <summary>
    /// Is System Role。
    /// </summary>
    /// <param name="roleKey">role Key 参数。</param>
    public static bool IsSystemRole(string roleKey)
    {
        return PermissionMap.ContainsKey(roleKey);
    }

    /// <summary>
    /// Resolve Permissions。
    /// </summary>
    /// <param name="roleKeys">role Keys 参数。</param>
    public static IReadOnlyList<string> ResolvePermissions(IEnumerable<string> roleKeys)
    {
        return roleKeys
            .SelectMany(GetPermissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }
}
