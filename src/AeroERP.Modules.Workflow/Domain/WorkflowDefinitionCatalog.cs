using AeroERP.Platform.Domain;

namespace AeroERP.Modules.Workflow.Domain;

/// <summary>
/// Workflow Definition Catalog 业务对象。
/// </summary>
public static class WorkflowDefinitionCatalog
{
    /// <summary>
    /// Procurement Request Review。
    /// </summary>
    public const string ProcurementRequestReview = "procurement-request-review";

    /// <summary>
    /// 创建Procurement Request Review。
    /// </summary>
    public static WorkflowDefinition CreateProcurementRequestReview() =>
        new(
            ProcurementRequestReview,
            "采购申请审批",
            "procurement",
            "ProcurementRequest",
            PlatformPermissions.ProcurementRequestReview);
}
