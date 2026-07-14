namespace AeroERP.Platform.Domain;

/// <summary>
/// Platform Permissions 业务对象。
/// </summary>
public static class PlatformPermissions
{
    /// <summary>
    /// Platform Manage。
    /// </summary>
    public const string PlatformManage = "platform.manage";
    /// <summary>
    /// Organization Manage。
    /// </summary>
    public const string OrganizationManage = "organization.manage";
    /// <summary>
    /// Identity User Read。
    /// </summary>
    public const string IdentityUserRead = "identity.user.read";
    /// <summary>
    /// Identity User Manage。
    /// </summary>
    public const string IdentityUserManage = "identity.user.manage";
    /// <summary>
    /// Identity User Password Manage。
    /// </summary>
    public const string IdentityUserPasswordManage = "identity.user.password.manage";
    /// <summary>
    /// Identity Role Manage。
    /// </summary>
    public const string IdentityRoleManage = "identity.role.manage";
    /// <summary>
    /// Plugin Manage。
    /// </summary>
    public const string PluginManage = "plugin.manage";
    /// <summary>
    /// Position Permissions Read。
    /// </summary>
    public const string PositionPermissionsRead = "position-permissions.read";
    /// <summary>
    /// Position Permissions Manage。
    /// </summary>
    public const string PositionPermissionsManage = "position-permissions.manage";
    /// <summary>
    /// Agent Review Submit。
    /// </summary>
    public const string AgentReviewSubmit = "agent.review.submit";
    /// <summary>
    /// Agent Review Decide。
    /// </summary>
    public const string AgentReviewDecide = "agent.review.decide";
    /// <summary>
    /// Master Data Read。
    /// </summary>
    public const string MasterDataRead = "master-data.read";
    /// <summary>
    /// Master Data Manage。
    /// </summary>
    public const string MasterDataManage = "master-data.manage";
    /// <summary>
    /// Procurement Read。
    /// </summary>
    public const string ProcurementRead = "procurement.read";
    /// <summary>
    /// Procurement Request Create。
    /// </summary>
    public const string ProcurementRequestCreate = "procurement.request.create";
    /// <summary>
    /// Procurement Request Review。
    /// </summary>
    public const string ProcurementRequestReview = "procurement.request.review";
    /// <summary>
    /// Procurement Order Create。
    /// </summary>
    public const string ProcurementOrderCreate = "procurement.order.create";
    /// <summary>
    /// Procurement Order Release。
    /// </summary>
    public const string ProcurementOrderRelease = "procurement.order.release";
    /// <summary>
    /// Inventory Read。
    /// </summary>
    public const string InventoryRead = "inventory.read";
    /// <summary>
    /// Inventory Receipt Manage。
    /// </summary>
    public const string InventoryReceiptManage = "inventory.receipt.manage";
    /// <summary>
    /// Inventory Issue Manage。
    /// </summary>
    public const string InventoryIssueManage = "inventory.issue.manage";
    /// <summary>
    /// Inventory Transfer Manage。
    /// </summary>
    public const string InventoryTransferManage = "inventory.transfer.manage";
    /// <summary>
    /// Inventory Count Manage。
    /// </summary>
    public const string InventoryCountManage = "inventory.count.manage";
    /// <summary>
    /// Inventory Location Manage。
    /// </summary>
    public const string InventoryLocationManage = "inventory.location.manage";
    /// <summary>
    /// Wms Read。
    /// </summary>
    public const string WmsRead = "wms.read";
    /// <summary>
    /// Wms Manage。
    /// </summary>
    public const string WmsManage = "wms.manage";
    /// <summary>
    /// Wms Execute。
    /// </summary>
    public const string WmsExecute = "wms.execute";
    /// <summary>
    /// Mobile Work Read。
    /// </summary>
    public const string MobileWorkRead = "mobile-work.read";
    /// <summary>
    /// Mobile Work Manage。
    /// </summary>
    public const string MobileWorkManage = "mobile-work.manage";
    /// <summary>
    /// Mobile Work Execute。
    /// </summary>
    public const string MobileWorkExecute = "mobile-work.execute";
    /// <summary>
    /// Integration Read。
    /// </summary>
    public const string IntegrationRead = "integration.read";
    /// <summary>
    /// Integration Manage。
    /// </summary>
    public const string IntegrationManage = "integration.manage";
    /// <summary>
    /// Integration Execute。
    /// </summary>
    public const string IntegrationExecute = "integration.execute";
    /// <summary>
    /// Document Exchange Read。
    /// </summary>
    public const string DocumentExchangeRead = "document-exchange.read";
    /// <summary>
    /// Document Exchange Manage。
    /// </summary>
    public const string DocumentExchangeManage = "document-exchange.manage";
    /// <summary>
    /// Document Exchange Execute。
    /// </summary>
    public const string DocumentExchangeExecute = "document-exchange.execute";
    /// <summary>
    /// Sales Read。
    /// </summary>
    public const string SalesRead = "sales.read";
    /// <summary>
    /// Sales Quotation Create。
    /// </summary>
    public const string SalesQuotationCreate = "sales.quotation.create";
    /// <summary>
    /// Sales Order Create。
    /// </summary>
    public const string SalesOrderCreate = "sales.order.create";
    /// <summary>
    /// Sales Order Manage。
    /// </summary>
    public const string SalesOrderManage = "sales.order.manage";
    /// <summary>
    /// Finance Read。
    /// </summary>
    public const string FinanceRead = "finance.read";
    /// <summary>
    /// Finance Accounting Manage。
    /// </summary>
    public const string FinanceAccountingManage = "finance.accounting.manage";
    /// <summary>
    /// Finance Voucher Manage。
    /// </summary>
    public const string FinanceVoucherManage = "finance.voucher.manage";
    /// <summary>
    /// Finance Voucher Review。
    /// </summary>
    public const string FinanceVoucherReview = "finance.voucher.review";
    /// <summary>
    /// Finance Payable Manage。
    /// </summary>
    public const string FinancePayableManage = "finance.payable.manage";
    /// <summary>
    /// Finance Receivable Manage。
    /// </summary>
    public const string FinanceReceivableManage = "finance.receivable.manage";
    /// <summary>
    /// Finance Settlement Manage。
    /// </summary>
    public const string FinanceSettlementManage = "finance.settlement.manage";
    /// <summary>
    /// Workflow Read。
    /// </summary>
    public const string WorkflowRead = "workflow.read";
    /// <summary>
    /// Workflow Task Decide。
    /// </summary>
    public const string WorkflowTaskDecide = "workflow.task.decide";
    /// <summary>
    /// Notification Read。
    /// </summary>
    public const string NotificationRead = "notification.read";
    /// <summary>
    /// Control Analytics Read。
    /// </summary>
    public const string ControlAnalyticsRead = "control.analytics.read";
    /// <summary>
    /// Control Data Scope Manage。
    /// </summary>
    public const string ControlDataScopeManage = "control.data-scope.manage";
    /// <summary>
    /// Control Numbering Manage。
    /// </summary>
    public const string ControlNumberingManage = "control.numbering.manage";
    /// <summary>
    /// Localization Read。
    /// </summary>
    public const string LocalizationRead = "localization.read";
    /// <summary>
    /// Localization Manage。
    /// </summary>
    public const string LocalizationManage = "localization.manage";
    /// <summary>
    /// Manufacturing Read。
    /// </summary>
    public const string ManufacturingRead = "manufacturing.read";
    /// <summary>
    /// Manufacturing Bom Manage。
    /// </summary>
    public const string ManufacturingBomManage = "manufacturing.bom.manage";
    /// <summary>
    /// Manufacturing Work Order Manage。
    /// </summary>
    public const string ManufacturingWorkOrderManage = "manufacturing.work-order.manage";
    /// <summary>
    /// Manufacturing Execution Manage。
    /// </summary>
    public const string ManufacturingExecutionManage = "manufacturing.execution.manage";
    /// <summary>
    /// Advanced Manufacturing Read。
    /// </summary>
    public const string AdvancedManufacturingRead = "advanced-manufacturing.read";
    /// <summary>
    /// Advanced Manufacturing Manage。
    /// </summary>
    public const string AdvancedManufacturingManage = "advanced-manufacturing.manage";
    /// <summary>
    /// Advanced Manufacturing Schedule。
    /// </summary>
    public const string AdvancedManufacturingSchedule = "advanced-manufacturing.schedule";
    /// <summary>
    /// Advanced Manufacturing Cost Manage。
    /// </summary>
    public const string AdvancedManufacturingCostManage = "advanced-manufacturing.cost.manage";
    /// <summary>
    /// Advanced Manufacturing Mrp Manage。
    /// </summary>
    public const string AdvancedManufacturingMrpManage = "advanced-manufacturing.mrp.manage";
    /// <summary>
    /// Reporting Read。
    /// </summary>
    public const string ReportingRead = "reporting.read";
    /// <summary>
    /// Reporting Manage。
    /// </summary>
    public const string ReportingManage = "reporting.manage";
    /// <summary>
    /// Reporting Export。
    /// </summary>
    public const string ReportingExport = "reporting.export";
    /// <summary>
    /// Quality Read。
    /// </summary>
    public const string QualityRead = "quality.read";
    /// <summary>
    /// Quality Inspection Manage。
    /// </summary>
    public const string QualityInspectionManage = "quality.inspection.manage";
    /// <summary>
    /// Quality Traceability Manage。
    /// </summary>
    public const string QualityTraceabilityManage = "quality.traceability.manage";
    /// <summary>
    /// Planning Read。
    /// </summary>
    public const string PlanningRead = "planning.read";
    /// <summary>
    /// Planning Manage。
    /// </summary>
    public const string PlanningManage = "planning.manage";
    /// <summary>
    /// Outsourcing Manage。
    /// </summary>
    public const string OutsourcingManage = "outsourcing.manage";
    /// <summary>
    /// Barcode Execute。
    /// </summary>
    public const string BarcodeExecute = "barcode.execute";

    /// <summary>
    /// All。
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        PlatformManage,
        OrganizationManage,
        IdentityUserRead,
        IdentityUserManage,
        IdentityUserPasswordManage,
        IdentityRoleManage,
        PluginManage,
        PositionPermissionsRead,
        PositionPermissionsManage,
        AgentReviewSubmit,
        AgentReviewDecide,
        MasterDataRead,
        MasterDataManage,
        ProcurementRead,
        ProcurementRequestCreate,
        ProcurementRequestReview,
        ProcurementOrderCreate,
        ProcurementOrderRelease,
        InventoryRead,
        InventoryReceiptManage,
        InventoryIssueManage,
        InventoryTransferManage,
        InventoryCountManage,
        InventoryLocationManage,
        WmsRead,
        WmsManage,
        WmsExecute,
        MobileWorkRead,
        MobileWorkManage,
        MobileWorkExecute,
        IntegrationRead,
        IntegrationManage,
        IntegrationExecute,
        DocumentExchangeRead,
        DocumentExchangeManage,
        DocumentExchangeExecute,
        SalesRead,
        SalesQuotationCreate,
        SalesOrderCreate,
        SalesOrderManage,
        FinanceRead,
        FinanceAccountingManage,
        FinanceVoucherManage,
        FinanceVoucherReview,
        FinancePayableManage,
        FinanceReceivableManage,
        FinanceSettlementManage,
        WorkflowRead,
        WorkflowTaskDecide,
        NotificationRead,
        ControlAnalyticsRead,
        ControlDataScopeManage,
        ControlNumberingManage,
        LocalizationRead,
        LocalizationManage,
        ManufacturingRead,
        ManufacturingBomManage,
        ManufacturingWorkOrderManage,
        ManufacturingExecutionManage,
        AdvancedManufacturingRead,
        AdvancedManufacturingManage,
        AdvancedManufacturingSchedule,
        AdvancedManufacturingCostManage,
        AdvancedManufacturingMrpManage,
        ReportingRead,
        ReportingManage,
        ReportingExport,
        QualityRead,
        QualityInspectionManage,
        QualityTraceabilityManage,
        PlanningRead,
        PlanningManage,
        OutsourcingManage,
        BarcodeExecute
    ];
}
