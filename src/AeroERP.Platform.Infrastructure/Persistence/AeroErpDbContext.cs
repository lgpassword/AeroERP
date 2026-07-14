using AeroERP.Modules.AdvancedManufacturing.Domain;
using AeroERP.Modules.Control.Domain;
using AeroERP.Modules.DocumentExchange.Domain;
using AeroERP.Modules.MasterData.Domain;
using AeroERP.Modules.Finance.Domain;
using AeroERP.Modules.Integration.Domain;
using AeroERP.Modules.Inventory.Domain;
using AeroERP.Modules.Localization.Domain;
using AeroERP.Modules.Manufacturing.Domain;
using AeroERP.Modules.MobileWork.Domain;
using AeroERP.Modules.Planning.Domain;
using AeroERP.Modules.PositionPermissions.Domain;
using AeroERP.Modules.Procurement.Domain;
using AeroERP.Modules.Quality.Domain;
using AeroERP.Modules.Reporting.Domain;
using AeroERP.Modules.Sales.Domain;
using AeroERP.Modules.Workflow.Domain;
using AeroERP.Modules.Wms.Domain;
using AeroERP.Platform.Domain;
using AeroERP.Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Persistence;

/// <summary>
/// Aero Erp Db Context 业务对象。
/// </summary>
/// <param name="options">配置选项。</param>
public sealed class AeroErpDbContext(DbContextOptions<AeroErpDbContext> options)
    : DbContext(options), IAeroErpDbContext
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AppRole> Roles => Set<AppRole>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<PluginModule> PluginModules => Set<PluginModule>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<AgentReviewRequest> AgentReviewRequests => Set<AgentReviewRequest>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<ProcurementRequest> ProcurementRequests => Set<ProcurementRequest>();
    public DbSet<ProcurementOrder> ProcurementOrders => Set<ProcurementOrder>();
    public DbSet<SalesQuotation> SalesQuotations => Set<SalesQuotation>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<InventoryReceipt> InventoryReceipts => Set<InventoryReceipt>();
    public DbSet<InventoryIssue> InventoryIssues => Set<InventoryIssue>();
    public DbSet<InventoryTransfer> InventoryTransfers => Set<InventoryTransfer>();
    public DbSet<InventoryCountAdjustment> InventoryCountAdjustments => Set<InventoryCountAdjustment>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<LocationStockBalance> LocationStockBalances => Set<LocationStockBalance>();
    public DbSet<AccountingAccount> AccountingAccounts => Set<AccountingAccount>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<GeneralLedgerVoucher> GeneralLedgerVouchers => Set<GeneralLedgerVoucher>();
    public DbSet<GeneralLedgerVoucherLine> GeneralLedgerVoucherLines => Set<GeneralLedgerVoucherLine>();
    public DbSet<Payable> Payables => Set<Payable>();
    public DbSet<Receivable> Receivables => Set<Receivable>();
    public DbSet<FinanceInvoice> FinanceInvoices => Set<FinanceInvoice>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<ApprovalTask> ApprovalTasks => Set<ApprovalTask>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DataScopeRule> DataScopeRules => Set<DataScopeRule>();
    public DbSet<NumberingRule> NumberingRules => Set<NumberingRule>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<LocalizationSettings> LocalizationSettings => Set<LocalizationSettings>();
    public DbSet<LocalizationContent> LocalizationContents => Set<LocalizationContent>();
    public DbSet<BillOfMaterial> BillOfMaterials => Set<BillOfMaterial>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ProductionIssue> ProductionIssues => Set<ProductionIssue>();
    public DbSet<ProductionReceipt> ProductionReceipts => Set<ProductionReceipt>();
    public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
    public DbSet<LotTraceEvent> LotTraceEvents => Set<LotTraceEvent>();
    public DbSet<PlanningSuggestion> PlanningSuggestions => Set<PlanningSuggestion>();
    public DbSet<OutsourcingOrder> OutsourcingOrders => Set<OutsourcingOrder>();
    public DbSet<BarcodeExecution> BarcodeExecutions => Set<BarcodeExecution>();
    public DbSet<PositionDepartment> PositionDepartments => Set<PositionDepartment>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<PermissionPackage> PermissionPackages => Set<PermissionPackage>();
    public DbSet<PositionRoleBinding> PositionRoleBindings => Set<PositionRoleBinding>();
    public DbSet<PositionDataScopeRule> PositionDataScopeRules => Set<PositionDataScopeRule>();
    public DbSet<RolePermissionGrant> RolePermissionGrants => Set<RolePermissionGrant>();
    public DbSet<PutAwayTask> PutAwayTasks => Set<PutAwayTask>();
    public DbSet<PickingTask> PickingTasks => Set<PickingTask>();
    public DbSet<PickingWave> PickingWaves => Set<PickingWave>();
    public DbSet<WarehouseContainer> WarehouseContainers => Set<WarehouseContainer>();
    public DbSet<WarehouseRoute> WarehouseRoutes => Set<WarehouseRoute>();
    public DbSet<PdaWorkQueueItem> PdaWorkQueueItems => Set<PdaWorkQueueItem>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<ManufacturingRouting> ManufacturingRoutings => Set<ManufacturingRouting>();
    public DbSet<RoutingOperation> RoutingOperations => Set<RoutingOperation>();
    public DbSet<OperationSchedule> OperationSchedules => Set<OperationSchedule>();
    public DbSet<CapacityLoad> CapacityLoads => Set<CapacityLoad>();
    public DbSet<ManufacturingCostSnapshot> ManufacturingCostSnapshots => Set<ManufacturingCostSnapshot>();
    public DbSet<MrpSuggestion> MrpSuggestions => Set<MrpSuggestion>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportRunRecord> ReportRunRecords => Set<ReportRunRecord>();
    public DbSet<ReportExportTask> ReportExportTasks => Set<ReportExportTask>();
    public DbSet<MobileDevice> MobileDevices => Set<MobileDevice>();
    public DbSet<MobileOfflineTask> MobileOfflineTasks => Set<MobileOfflineTask>();
    public DbSet<MobileScanEvent> MobileScanEvents => Set<MobileScanEvent>();
    public DbSet<MessageChannel> MessageChannels => Set<MessageChannel>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<ExternalConnector> ExternalConnectors => Set<ExternalConnector>();
    public DbSet<IntegrationSyncJob> IntegrationSyncJobs => Set<IntegrationSyncJob>();
    public DbSet<IntegrationAuditRecord> IntegrationAuditRecords => Set<IntegrationAuditRecord>();
    public DbSet<ImportTemplate> ImportTemplates => Set<ImportTemplate>();
    public DbSet<ImportFieldMapping> ImportFieldMappings => Set<ImportFieldMapping>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ExportFileTask> ExportFileTasks => Set<ExportFileTask>();
    public DbSet<PrintTemplate> PrintTemplates => Set<PrintTemplate>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
    public DbSet<FileAuditRecord> FileAuditRecords => Set<FileAuditRecord>();

    /// <summary>
    /// On Model Creating。
    /// </summary>
    /// <param name="modelBuilder">model Builder 参数。</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<AppUser>().Property(x => x.UserName).HasMaxLength(64);
        modelBuilder.Entity<AppUser>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<AppUser>().Property(x => x.PasswordHash).HasMaxLength(512);
        modelBuilder.Entity<AppUser>()
            .HasMany(x => x.RoleAssignments)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppUser>().Navigation(x => x.RoleAssignments).AutoInclude();

        modelBuilder.Entity<UserRoleAssignment>().HasKey(x => new { x.UserId, x.RoleId });

        modelBuilder.Entity<AppRole>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<AppRole>().Property(x => x.Key).HasMaxLength(64);
        modelBuilder.Entity<AppRole>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<AppRole>()
            .HasMany(x => x.ModuleAccesses)
            .WithOne()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppRole>().Navigation(x => x.ModuleAccesses).AutoInclude();

        modelBuilder.Entity<RoleModuleAccess>().HasKey(x => new { x.RoleId, x.ModuleKey });
        modelBuilder.Entity<RoleModuleAccess>().Property(x => x.ModuleKey).HasMaxLength(128);

        modelBuilder.Entity<Organization>().HasIndex(x => x.Name).IsUnique();

        modelBuilder.Entity<PluginModule>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<PluginModule>().Property(x => x.Key).HasMaxLength(128);
        modelBuilder.Entity<PluginModule>().Property(x => x.DisplayName).HasMaxLength(128);

        modelBuilder.Entity<AuditEvent>().Property(x => x.Category).HasMaxLength(64);
        modelBuilder.Entity<AuditEvent>().Property(x => x.Action).HasMaxLength(64);

        modelBuilder.Entity<AgentReviewRequest>().Property(x => x.AgentName).HasMaxLength(128);
        modelBuilder.Entity<AgentReviewRequest>().Property(x => x.ActionName).HasMaxLength(128);
        modelBuilder.Entity<AgentReviewRequest>().Property(x => x.Status).HasMaxLength(32);

        modelBuilder.Entity<Customer>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Supplier>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Item>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Warehouse>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Customer>().Property(x => x.OrganizationName).HasMaxLength(128);
        modelBuilder.Entity<Customer>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<Customer>().Property(x => x.TaxpayerId).HasMaxLength(64);
        modelBuilder.Entity<Customer>().Property(x => x.InvoiceTitle).HasMaxLength(128);
        modelBuilder.Entity<Supplier>().Property(x => x.OrganizationName).HasMaxLength(128);
        modelBuilder.Entity<Supplier>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<Supplier>().Property(x => x.TaxpayerId).HasMaxLength(64);
        modelBuilder.Entity<Supplier>().Property(x => x.InvoiceTitle).HasMaxLength(128);
        modelBuilder.Entity<Warehouse>().Property(x => x.OrganizationName).HasMaxLength(128);

        modelBuilder.Entity<ProcurementRequest>().HasIndex(x => x.RequestNo).IsUnique();
        modelBuilder.Entity<ProcurementRequest>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ProcurementRequest>().Property(x => x.OrganizationName).HasMaxLength(128);
        modelBuilder.Entity<ProcurementRequest>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<ProcurementRequest>().Property(x => x.TaxInvoiceType).HasMaxLength(64);
        modelBuilder.Entity<ProcurementRequest>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.ProcurementRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProcurementRequestLine>().HasKey(x => x.Id);
        modelBuilder.Entity<ProcurementRequestLine>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<ProcurementOrder>().HasIndex(x => x.OrderNo).IsUnique();
        modelBuilder.Entity<ProcurementOrder>().Property(x => x.Status).HasMaxLength(32);

        modelBuilder.Entity<SalesQuotation>().HasIndex(x => x.QuotationNo).IsUnique();
        modelBuilder.Entity<SalesQuotation>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<SalesQuotation>().Property(x => x.OrganizationName).HasMaxLength(128);
        modelBuilder.Entity<SalesQuotation>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<SalesQuotation>().Property(x => x.TaxInvoiceType).HasMaxLength(64);
        modelBuilder.Entity<SalesQuotation>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.SalesQuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SalesLine>().ToTable("SalesQuotationLine");
        modelBuilder.Entity<SalesLine>().HasKey(x => x.Id);
        modelBuilder.Entity<SalesLine>().HasIndex(x => x.SalesQuotationId);
        modelBuilder.Entity<SalesLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<SalesLine>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<SalesOrder>().HasIndex(x => x.OrderNo).IsUnique();
        modelBuilder.Entity<SalesOrder>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<SalesOrder>().Property(x => x.OrganizationName).HasMaxLength(128);
        modelBuilder.Entity<SalesOrder>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<SalesOrder>().Property(x => x.TaxInvoiceType).HasMaxLength(64);
        modelBuilder.Entity<SalesOrder>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SalesOrderLine>().ToTable("SalesOrderLine");
        modelBuilder.Entity<SalesOrderLine>().HasKey(x => x.Id);
        modelBuilder.Entity<SalesOrderLine>().HasIndex(x => x.SalesOrderId);
        modelBuilder.Entity<SalesOrderLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<SalesOrderLine>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<InventoryReceipt>().HasIndex(x => x.ReceiptNo).IsUnique();
        modelBuilder.Entity<InventoryReceipt>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<InventoryReceipt>().Property(x => x.LocationCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryReceipt>().Property(x => x.LocationName).HasMaxLength(128);
        modelBuilder.Entity<InventoryReceipt>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.InventoryReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryReceiptLine>().ToTable("InventoryReceiptLine");
        modelBuilder.Entity<InventoryReceiptLine>().HasKey(x => x.Id);
        modelBuilder.Entity<InventoryReceiptLine>().HasIndex(x => x.InventoryReceiptId);
        modelBuilder.Entity<InventoryReceiptLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryReceiptLine>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<InventoryIssue>().HasIndex(x => x.IssueNo).IsUnique();
        modelBuilder.Entity<InventoryIssue>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<InventoryIssue>().Property(x => x.LocationCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryIssue>().Property(x => x.LocationName).HasMaxLength(128);
        modelBuilder.Entity<InventoryIssue>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.InventoryIssueId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryIssueLine>().ToTable("InventoryIssueLine");
        modelBuilder.Entity<InventoryIssueLine>().HasKey(x => x.Id);
        modelBuilder.Entity<InventoryIssueLine>().HasIndex(x => x.InventoryIssueId);
        modelBuilder.Entity<InventoryIssueLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryIssueLine>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<InventoryTransfer>().HasIndex(x => x.TransferNo).IsUnique();
        modelBuilder.Entity<InventoryTransfer>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<InventoryTransfer>().Property(x => x.FromLocationCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryTransfer>().Property(x => x.FromLocationName).HasMaxLength(128);
        modelBuilder.Entity<InventoryTransfer>().Property(x => x.ToLocationCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryTransfer>().Property(x => x.ToLocationName).HasMaxLength(128);
        modelBuilder.Entity<InventoryTransfer>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.InventoryTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryTransferLine>().ToTable("InventoryTransferLine");
        modelBuilder.Entity<InventoryTransferLine>().HasKey(x => x.Id);
        modelBuilder.Entity<InventoryTransferLine>().HasIndex(x => x.InventoryTransferId);
        modelBuilder.Entity<InventoryTransferLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryTransferLine>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<InventoryCountAdjustment>().HasIndex(x => x.CountNo).IsUnique();
        modelBuilder.Entity<InventoryCountAdjustment>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<InventoryCountAdjustment>().Property(x => x.LocationCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryCountAdjustment>().Property(x => x.LocationName).HasMaxLength(128);
        modelBuilder.Entity<InventoryCountAdjustment>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.InventoryCountAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryCountAdjustmentLine>().ToTable("InventoryCountAdjustmentLine");
        modelBuilder.Entity<InventoryCountAdjustmentLine>().HasKey(x => x.Id);
        modelBuilder.Entity<InventoryCountAdjustmentLine>().HasIndex(x => x.InventoryCountAdjustmentId);
        modelBuilder.Entity<InventoryCountAdjustmentLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryCountAdjustmentLine>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<InventoryMovement>().Property(x => x.DocumentType).HasMaxLength(64);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.DocumentNo).HasMaxLength(64);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.MovementType).HasMaxLength(32);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.LocationCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.LocationName).HasMaxLength(128);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<InventoryMovement>().Property(x => x.Actor).HasMaxLength(128);
        modelBuilder.Entity<InventoryMovement>().HasIndex(x => x.DocumentNo);
        modelBuilder.Entity<InventoryMovement>().HasIndex(x => new { x.WarehouseId, x.ItemId });

        modelBuilder.Entity<StockBalance>()
            .HasIndex(x => new { x.WarehouseId, x.ItemId })
            .IsUnique();
        modelBuilder.Entity<StockBalance>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<StockBalance>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<StockBalance>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<StockBalance>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<WarehouseLocation>().HasIndex(x => new { x.WarehouseId, x.Code }).IsUnique();
        modelBuilder.Entity<WarehouseLocation>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<WarehouseLocation>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<WarehouseLocation>().Property(x => x.Code).HasMaxLength(64);
        modelBuilder.Entity<WarehouseLocation>().Property(x => x.Name).HasMaxLength(128);
        modelBuilder.Entity<WarehouseLocation>().Property(x => x.CreatedBy).HasMaxLength(128);

        modelBuilder.Entity<LocationStockBalance>()
            .HasIndex(x => new { x.LocationId, x.ItemId })
            .IsUnique();
        modelBuilder.Entity<LocationStockBalance>().HasIndex(x => new { x.WarehouseId, x.ItemId });
        modelBuilder.Entity<LocationStockBalance>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<LocationStockBalance>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<LocationStockBalance>().Property(x => x.LocationCode).HasMaxLength(64);
        modelBuilder.Entity<LocationStockBalance>().Property(x => x.LocationName).HasMaxLength(128);
        modelBuilder.Entity<LocationStockBalance>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<LocationStockBalance>().Property(x => x.ItemName).HasMaxLength(128);

        modelBuilder.Entity<AccountingAccount>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<AccountingAccount>().HasIndex(x => x.ParentAccountId);
        modelBuilder.Entity<AccountingAccount>().Property(x => x.Code).HasMaxLength(64);
        modelBuilder.Entity<AccountingAccount>().Property(x => x.Name).HasMaxLength(128);
        modelBuilder.Entity<AccountingAccount>().Property(x => x.Type).HasMaxLength(32);
        modelBuilder.Entity<AccountingAccount>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<AccountingPeriod>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<AccountingPeriod>().HasIndex(x => new { x.Year, x.Month }).IsUnique();
        modelBuilder.Entity<AccountingPeriod>().Property(x => x.Name).HasMaxLength(16);
        modelBuilder.Entity<AccountingPeriod>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<AccountingPeriod>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<AccountingPeriod>().Property(x => x.ClosedBy).HasMaxLength(128);

        modelBuilder.Entity<GeneralLedgerVoucher>().HasIndex(x => x.VoucherNo).IsUnique();
        modelBuilder.Entity<GeneralLedgerVoucher>().HasIndex(x => new { x.AccountingPeriodId, x.Status });
        modelBuilder.Entity<GeneralLedgerVoucher>().HasIndex(x => new { x.SourceType, x.SourceId }).IsUnique().HasFilter("\"SourceId\" IS NOT NULL");
        modelBuilder.Entity<GeneralLedgerVoucher>().Ignore(x => x.TotalDebit);
        modelBuilder.Entity<GeneralLedgerVoucher>().Ignore(x => x.TotalCredit);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.VoucherNo).HasMaxLength(64);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.AccountingPeriodName).HasMaxLength(16);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.Summary).HasMaxLength(256);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.SourceType).HasMaxLength(64);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.SourceNo).HasMaxLength(64);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.SubmittedBy).HasMaxLength(128);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.ReviewedBy).HasMaxLength(128);
        modelBuilder.Entity<GeneralLedgerVoucher>().Property(x => x.ReviewNote).HasMaxLength(256);
        modelBuilder.Entity<GeneralLedgerVoucher>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.GeneralLedgerVoucherId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GeneralLedgerVoucher>().Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.Entity<GeneralLedgerVoucherLine>().HasKey(x => x.Id);
        modelBuilder.Entity<GeneralLedgerVoucherLine>().HasIndex(x => x.GeneralLedgerVoucherId);
        modelBuilder.Entity<GeneralLedgerVoucherLine>().HasIndex(x => x.AccountingAccountId);
        modelBuilder.Entity<GeneralLedgerVoucherLine>().Property(x => x.AccountCode).HasMaxLength(64);
        modelBuilder.Entity<GeneralLedgerVoucherLine>().Property(x => x.AccountName).HasMaxLength(128);
        modelBuilder.Entity<GeneralLedgerVoucherLine>().Property(x => x.Summary).HasMaxLength(256);

        modelBuilder.Entity<Payable>().HasIndex(x => x.PayableNo).IsUnique();
        modelBuilder.Entity<Payable>().HasIndex(x => x.ProcurementOrderId);
        modelBuilder.Entity<Payable>().HasIndex(x => x.InventoryReceiptId).IsUnique();
        modelBuilder.Entity<Payable>().HasIndex(x => new { x.Status, x.DueDate });
        modelBuilder.Entity<Payable>().Ignore(x => x.RemainingAmount);
        modelBuilder.Entity<Payable>().Property(x => x.PayableNo).HasMaxLength(64);
        modelBuilder.Entity<Payable>().Property(x => x.ProcurementOrderNo).HasMaxLength(64);
        modelBuilder.Entity<Payable>().Property(x => x.InventoryReceiptNo).HasMaxLength(64);
        modelBuilder.Entity<Payable>().Property(x => x.SupplierName).HasMaxLength(128);
        modelBuilder.Entity<Payable>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<Payable>().Property(x => x.TaxInvoiceType).HasMaxLength(64);
        modelBuilder.Entity<Payable>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<Payable>().Property(x => x.SourceType).HasMaxLength(64);
        modelBuilder.Entity<Payable>().Property(x => x.CreatedBy).HasMaxLength(128);

        modelBuilder.Entity<Receivable>().HasIndex(x => x.ReceivableNo).IsUnique();
        modelBuilder.Entity<Receivable>().HasIndex(x => x.SalesOrderId);
        modelBuilder.Entity<Receivable>().HasIndex(x => x.InventoryIssueId).IsUnique();
        modelBuilder.Entity<Receivable>().HasIndex(x => new { x.Status, x.DueDate });
        modelBuilder.Entity<Receivable>().Ignore(x => x.RemainingAmount);
        modelBuilder.Entity<Receivable>().Property(x => x.ReceivableNo).HasMaxLength(64);
        modelBuilder.Entity<Receivable>().Property(x => x.SalesOrderNo).HasMaxLength(64);
        modelBuilder.Entity<Receivable>().Property(x => x.InventoryIssueNo).HasMaxLength(64);
        modelBuilder.Entity<Receivable>().Property(x => x.CustomerName).HasMaxLength(128);
        modelBuilder.Entity<Receivable>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<Receivable>().Property(x => x.TaxInvoiceType).HasMaxLength(64);
        modelBuilder.Entity<Receivable>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<Receivable>().Property(x => x.SourceType).HasMaxLength(64);
        modelBuilder.Entity<Receivable>().Property(x => x.CreatedBy).HasMaxLength(128);

        modelBuilder.Entity<FinanceInvoice>().HasIndex(x => x.InvoiceNo).IsUnique();
        modelBuilder.Entity<FinanceInvoice>().HasIndex(x => new { x.Direction, x.SourceId }).IsUnique();
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.InvoiceNo).HasMaxLength(64);
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.Direction).HasMaxLength(32);
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.SourceNo).HasMaxLength(64);
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.CounterpartyName).HasMaxLength(128);
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.TaxInvoiceType).HasMaxLength(64);
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.Note).HasMaxLength(256);
        modelBuilder.Entity<FinanceInvoice>().Property(x => x.CreatedBy).HasMaxLength(128);

        modelBuilder.Entity<BankAccount>().HasIndex(x => x.AccountNo).IsUnique();
        modelBuilder.Entity<BankAccount>().Property(x => x.AccountNo).HasMaxLength(64);
        modelBuilder.Entity<BankAccount>().Property(x => x.AccountName).HasMaxLength(128);
        modelBuilder.Entity<BankAccount>().Property(x => x.BankName).HasMaxLength(128);
        modelBuilder.Entity<BankAccount>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<BankAccount>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<BankStatementLine>().HasIndex(x => x.StatementNo).IsUnique();
        modelBuilder.Entity<BankStatementLine>().HasIndex(x => x.BankAccountId);
        modelBuilder.Entity<BankStatementLine>().HasIndex(x => new { x.ReconciliationStatus, x.TransactionDate });
        modelBuilder.Entity<BankStatementLine>().HasIndex(x => x.SettlementId).IsUnique();
        modelBuilder.Entity<BankStatementLine>().Property(x => x.StatementNo).HasMaxLength(64);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.BankAccountNo).HasMaxLength(64);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.BankAccountName).HasMaxLength(128);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.Direction).HasMaxLength(32);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.CounterpartyName).HasMaxLength(128);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.BankReferenceNo).HasMaxLength(128);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.Summary).HasMaxLength(256);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.ReconciliationStatus).HasMaxLength(32);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.SettlementNo).HasMaxLength(64);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.ReconciledBy).HasMaxLength(128);
        modelBuilder.Entity<BankStatementLine>().Property(x => x.CreatedBy).HasMaxLength(128);

        modelBuilder.Entity<Settlement>().HasIndex(x => x.SettlementNo).IsUnique();
        modelBuilder.Entity<Settlement>().HasIndex(x => new { x.TargetType, x.TargetId });
        modelBuilder.Entity<Settlement>().HasIndex(x => x.BankAccountId);
        modelBuilder.Entity<Settlement>().HasIndex(x => x.BankStatementLineId).IsUnique();
        modelBuilder.Entity<Settlement>().HasIndex(x => x.ReconciliationStatus);
        modelBuilder.Entity<Settlement>().Property(x => x.SettlementNo).HasMaxLength(64);
        modelBuilder.Entity<Settlement>().Property(x => x.TargetType).HasMaxLength(32);
        modelBuilder.Entity<Settlement>().Property(x => x.TargetNo).HasMaxLength(64);
        modelBuilder.Entity<Settlement>().Property(x => x.CounterpartyName).HasMaxLength(128);
        modelBuilder.Entity<Settlement>().Property(x => x.CurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<Settlement>().Property(x => x.BankAccountNo).HasMaxLength(64);
        modelBuilder.Entity<Settlement>().Property(x => x.BankAccountName).HasMaxLength(128);
        modelBuilder.Entity<Settlement>().Property(x => x.Method).HasMaxLength(64);
        modelBuilder.Entity<Settlement>().Property(x => x.ReconciliationStatus).HasMaxLength(32);
        modelBuilder.Entity<Settlement>().Property(x => x.BankStatementNo).HasMaxLength(64);
        modelBuilder.Entity<Settlement>().Property(x => x.ReconciledBy).HasMaxLength(128);
        modelBuilder.Entity<Settlement>().Property(x => x.SettledBy).HasMaxLength(128);

        modelBuilder.Entity<WorkflowDefinition>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<WorkflowDefinition>().Property(x => x.Key).HasMaxLength(128);
        modelBuilder.Entity<WorkflowDefinition>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<WorkflowDefinition>().Property(x => x.ModuleKey).HasMaxLength(128);
        modelBuilder.Entity<WorkflowDefinition>().Property(x => x.DocumentType).HasMaxLength(64);
        modelBuilder.Entity<WorkflowDefinition>().Property(x => x.RequiredPermission).HasMaxLength(128);

        modelBuilder.Entity<WorkflowInstance>().HasIndex(x => new { x.DefinitionKey, x.DocumentId });
        modelBuilder.Entity<WorkflowInstance>().Property(x => x.DefinitionKey).HasMaxLength(128);
        modelBuilder.Entity<WorkflowInstance>().Property(x => x.DefinitionName).HasMaxLength(128);
        modelBuilder.Entity<WorkflowInstance>().Property(x => x.DocumentType).HasMaxLength(64);
        modelBuilder.Entity<WorkflowInstance>().Property(x => x.DocumentNo).HasMaxLength(64);
        modelBuilder.Entity<WorkflowInstance>().Property(x => x.Title).HasMaxLength(256);
        modelBuilder.Entity<WorkflowInstance>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<WorkflowInstance>().Property(x => x.SubmittedBy).HasMaxLength(128);

        modelBuilder.Entity<ApprovalTask>().HasIndex(x => new { x.WorkflowInstanceId, x.Status });
        modelBuilder.Entity<ApprovalTask>().HasIndex(x => x.DocumentId);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.DefinitionKey).HasMaxLength(128);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.DefinitionName).HasMaxLength(128);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.DocumentType).HasMaxLength(64);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.DocumentNo).HasMaxLength(64);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.Title).HasMaxLength(256);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.SubmittedBy).HasMaxLength(128);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.RequiredPermission).HasMaxLength(128);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.DecidedBy).HasMaxLength(128);
        modelBuilder.Entity<ApprovalTask>().Property(x => x.Decision).HasMaxLength(32);

        modelBuilder.Entity<Notification>().HasIndex(x => x.Status);
        modelBuilder.Entity<Notification>().HasIndex(x => x.RelatedDocumentId);
        modelBuilder.Entity<Notification>().Property(x => x.Title).HasMaxLength(160);
        modelBuilder.Entity<Notification>().Property(x => x.Category).HasMaxLength(64);
        modelBuilder.Entity<Notification>().Property(x => x.RelatedDocumentType).HasMaxLength(64);
        modelBuilder.Entity<Notification>().Property(x => x.RelatedDocumentNo).HasMaxLength(64);
        modelBuilder.Entity<Notification>().Property(x => x.RecipientPermission).HasMaxLength(128);
        modelBuilder.Entity<Notification>().Property(x => x.Status).HasMaxLength(32);

        modelBuilder.Entity<DataScopeRule>().HasIndex(x => new { x.RoleKey, x.ScopeType }).IsUnique();
        modelBuilder.Entity<DataScopeRule>().Property(x => x.RoleKey).HasMaxLength(64);
        modelBuilder.Entity<DataScopeRule>().Property(x => x.ScopeType).HasMaxLength(64);
        modelBuilder.Entity<DataScopeRule>().Property(x => x.MatchValue).HasMaxLength(128);

        modelBuilder.Entity<NumberingRule>().HasIndex(x => x.DocumentType).IsUnique();
        modelBuilder.Entity<NumberingRule>().Property(x => x.DocumentType).HasMaxLength(64);
        modelBuilder.Entity<NumberingRule>().Property(x => x.Prefix).HasMaxLength(32);

        modelBuilder.Entity<Currency>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Currency>().Property(x => x.Code).HasMaxLength(8);
        modelBuilder.Entity<Currency>().Property(x => x.Name).HasMaxLength(64);
        modelBuilder.Entity<Currency>().Property(x => x.Symbol).HasMaxLength(8);

        modelBuilder.Entity<LocalizationSettings>().Property(x => x.DefaultCurrencyCode).HasMaxLength(8);
        modelBuilder.Entity<LocalizationSettings>().Property(x => x.TaxInvoiceType).HasMaxLength(64);
        modelBuilder.Entity<LocalizationSettings>().Property(x => x.TaxpayerId).HasMaxLength(64);
        modelBuilder.Entity<LocalizationSettings>().Property(x => x.InvoiceTitle).HasMaxLength(128);

        modelBuilder.Entity<LocalizationContent>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<LocalizationContent>().Property(x => x.Key).HasMaxLength(160);
        modelBuilder.Entity<LocalizationContent>().Property(x => x.Category).HasMaxLength(64);
        modelBuilder.Entity<LocalizationContent>().Property(x => x.ChineseText).HasMaxLength(512);
        modelBuilder.Entity<LocalizationContent>().Property(x => x.EnglishText).HasMaxLength(512);

        modelBuilder.Entity<BillOfMaterial>().ToTable("BillOfMaterials");
        modelBuilder.Entity<BillOfMaterial>().HasIndex(x => x.BomNo).IsUnique();
        modelBuilder.Entity<BillOfMaterial>().HasIndex(x => new { x.FinishedItemId, x.Version }).IsUnique();
        modelBuilder.Entity<BillOfMaterial>().Property(x => x.BomNo).HasMaxLength(64);
        modelBuilder.Entity<BillOfMaterial>().Property(x => x.FinishedItemCode).HasMaxLength(64);
        modelBuilder.Entity<BillOfMaterial>().Property(x => x.FinishedItemName).HasMaxLength(128);
        modelBuilder.Entity<BillOfMaterial>().Property(x => x.Version).HasMaxLength(32);
        modelBuilder.Entity<BillOfMaterial>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<BillOfMaterial>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.BillOfMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BillOfMaterialLine>().ToTable("BillOfMaterialLine");
        modelBuilder.Entity<BillOfMaterialLine>().HasKey(x => x.Id);
        modelBuilder.Entity<BillOfMaterialLine>().HasIndex(x => x.BillOfMaterialId);
        modelBuilder.Entity<BillOfMaterialLine>().Property(x => x.ComponentItemCode).HasMaxLength(64);
        modelBuilder.Entity<BillOfMaterialLine>().Property(x => x.ComponentItemName).HasMaxLength(128);
        modelBuilder.Entity<BillOfMaterialLine>().Property(x => x.Unit).HasMaxLength(32);

        modelBuilder.Entity<WorkOrder>().HasIndex(x => x.WorkOrderNo).IsUnique();
        modelBuilder.Entity<WorkOrder>().Property(x => x.WorkOrderNo).HasMaxLength(64);
        modelBuilder.Entity<WorkOrder>().Property(x => x.BomNo).HasMaxLength(64);
        modelBuilder.Entity<WorkOrder>().Property(x => x.BomVersion).HasMaxLength(32);
        modelBuilder.Entity<WorkOrder>().Property(x => x.FinishedItemCode).HasMaxLength(64);
        modelBuilder.Entity<WorkOrder>().Property(x => x.FinishedItemName).HasMaxLength(128);
        modelBuilder.Entity<WorkOrder>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<WorkOrder>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<WorkOrder>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<WorkOrder>()
            .HasMany(x => x.MaterialLines)
            .WithOne()
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkOrderMaterialLine>().ToTable("WorkOrderMaterialLine");
        modelBuilder.Entity<WorkOrderMaterialLine>().HasKey(x => x.Id);
        modelBuilder.Entity<WorkOrderMaterialLine>().HasIndex(x => x.WorkOrderId);
        modelBuilder.Entity<WorkOrderMaterialLine>().Ignore(x => x.RemainingQuantity);
        modelBuilder.Entity<WorkOrderMaterialLine>().Property(x => x.ComponentItemCode).HasMaxLength(64);
        modelBuilder.Entity<WorkOrderMaterialLine>().Property(x => x.ComponentItemName).HasMaxLength(128);
        modelBuilder.Entity<WorkOrderMaterialLine>().Property(x => x.Unit).HasMaxLength(32);

        modelBuilder.Entity<ProductionIssue>().HasIndex(x => x.IssueNo).IsUnique();
        modelBuilder.Entity<ProductionIssue>().HasIndex(x => x.WorkOrderId);
        modelBuilder.Entity<ProductionIssue>().Property(x => x.IssueNo).HasMaxLength(64);
        modelBuilder.Entity<ProductionIssue>().Property(x => x.WorkOrderNo).HasMaxLength(64);
        modelBuilder.Entity<ProductionIssue>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<ProductionIssue>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<ProductionIssue>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ProductionIssue>().Property(x => x.IssuedBy).HasMaxLength(128);
        modelBuilder.Entity<ProductionIssue>()
            .HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.ProductionIssueId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductionIssueLine>().ToTable("ProductionIssueLine");
        modelBuilder.Entity<ProductionIssueLine>().HasKey(x => x.Id);
        modelBuilder.Entity<ProductionIssueLine>().HasIndex(x => x.ProductionIssueId);
        modelBuilder.Entity<ProductionIssueLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<ProductionIssueLine>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<ProductionIssueLine>().Property(x => x.Unit).HasMaxLength(32);

        modelBuilder.Entity<ProductionReceipt>().HasIndex(x => x.ReceiptNo).IsUnique();
        modelBuilder.Entity<ProductionReceipt>().HasIndex(x => x.WorkOrderId);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.ReceiptNo).HasMaxLength(64);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.WorkOrderNo).HasMaxLength(64);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.FinishedItemCode).HasMaxLength(64);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.FinishedItemName).HasMaxLength(128);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ProductionReceipt>().Property(x => x.ReceivedBy).HasMaxLength(128);

        modelBuilder.Entity<QualityInspection>().HasIndex(x => x.InspectionNo).IsUnique();
        modelBuilder.Entity<QualityInspection>().HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId });
        modelBuilder.Entity<QualityInspection>().Property(x => x.InspectionNo).HasMaxLength(64);
        modelBuilder.Entity<QualityInspection>().Property(x => x.SourceDocumentType).HasMaxLength(64);
        modelBuilder.Entity<QualityInspection>().Property(x => x.SourceDocumentNo).HasMaxLength(64);
        modelBuilder.Entity<QualityInspection>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<QualityInspection>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<QualityInspection>().Property(x => x.Result).HasMaxLength(32);
        modelBuilder.Entity<QualityInspection>().Property(x => x.Disposition).HasMaxLength(128);
        modelBuilder.Entity<QualityInspection>().Property(x => x.Inspector).HasMaxLength(128);

        modelBuilder.Entity<LotTraceEvent>().HasIndex(x => x.LotNo);
        modelBuilder.Entity<LotTraceEvent>().HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId });
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.LotNo).HasMaxLength(64);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.EventType).HasMaxLength(64);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.SourceDocumentType).HasMaxLength(64);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.SourceDocumentNo).HasMaxLength(64);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.TargetDocumentType).HasMaxLength(64);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.TargetDocumentNo).HasMaxLength(64);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<LotTraceEvent>().Property(x => x.Actor).HasMaxLength(128);

        modelBuilder.Entity<PlanningSuggestion>().HasIndex(x => x.SuggestionNo).IsUnique();
        modelBuilder.Entity<PlanningSuggestion>().HasIndex(x => new { x.WarehouseId, x.ItemId, x.Status });
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.SuggestionNo).HasMaxLength(64);
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<PlanningSuggestion>().Property(x => x.CreatedBy).HasMaxLength(128);

        modelBuilder.Entity<OutsourcingOrder>().HasIndex(x => x.OrderNo).IsUnique();
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.OrderNo).HasMaxLength(64);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.SupplierName).HasMaxLength(128);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.FinishedItemCode).HasMaxLength(64);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.FinishedItemName).HasMaxLength(128);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<OutsourcingOrder>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<OutsourcingOrder>()
            .HasMany(x => x.MaterialLines)
            .WithOne()
            .HasForeignKey(x => x.OutsourcingOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OutsourcingOrderLine>().ToTable("OutsourcingOrderLine");
        modelBuilder.Entity<OutsourcingOrderLine>().HasKey(x => x.Id);
        modelBuilder.Entity<OutsourcingOrderLine>().HasIndex(x => x.OutsourcingOrderId);
        modelBuilder.Entity<OutsourcingOrderLine>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<OutsourcingOrderLine>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<OutsourcingOrderLine>().Property(x => x.Unit).HasMaxLength(32);

        modelBuilder.Entity<BarcodeExecution>().HasIndex(x => x.ExecutionNo).IsUnique();
        modelBuilder.Entity<BarcodeExecution>().Property(x => x.ExecutionNo).HasMaxLength(64);
        modelBuilder.Entity<BarcodeExecution>().Property(x => x.Barcode).HasMaxLength(160);
        modelBuilder.Entity<BarcodeExecution>().Property(x => x.Action).HasMaxLength(64);
        modelBuilder.Entity<BarcodeExecution>().Property(x => x.Result).HasMaxLength(32);
        modelBuilder.Entity<BarcodeExecution>().Property(x => x.DocumentType).HasMaxLength(64);
        modelBuilder.Entity<BarcodeExecution>().Property(x => x.DocumentNo).HasMaxLength(64);
        modelBuilder.Entity<BarcodeExecution>().Property(x => x.Actor).HasMaxLength(128);

        modelBuilder.Entity<PositionDepartment>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<PositionDepartment>().Property(x => x.Code).HasMaxLength(64);
        modelBuilder.Entity<PositionDepartment>().Property(x => x.Name).HasMaxLength(128);

        modelBuilder.Entity<JobPosition>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<JobPosition>().HasIndex(x => x.DepartmentId);
        modelBuilder.Entity<JobPosition>().Property(x => x.Code).HasMaxLength(64);
        modelBuilder.Entity<JobPosition>().Property(x => x.Name).HasMaxLength(128);
        modelBuilder.Entity<JobPosition>().Property(x => x.DepartmentName).HasMaxLength(128);
        modelBuilder.Entity<JobPosition>().Property(x => x.Description).HasMaxLength(256);

        modelBuilder.Entity<PermissionPackage>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<PermissionPackage>().Property(x => x.Key).HasMaxLength(80);
        modelBuilder.Entity<PermissionPackage>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<PermissionPackage>().Property(x => x.Description).HasMaxLength(256);

        modelBuilder.Entity<PositionRoleBinding>().HasIndex(x => new { x.PositionId, x.RoleId }).IsUnique();
        modelBuilder.Entity<PositionRoleBinding>().HasIndex(x => x.RoleId);
        modelBuilder.Entity<PositionRoleBinding>()
            .HasOne<JobPosition>()
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PositionRoleBinding>()
            .HasOne<AppRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PositionDataScopeRule>().HasIndex(x => new { x.PositionId, x.ScopeType }).IsUnique();
        modelBuilder.Entity<PositionDataScopeRule>().Property(x => x.ScopeType).HasMaxLength(64);
        modelBuilder.Entity<PositionDataScopeRule>().Property(x => x.MatchValue).HasMaxLength(128);
        modelBuilder.Entity<PositionDataScopeRule>().Property(x => x.Description).HasMaxLength(256);
        modelBuilder.Entity<PositionDataScopeRule>()
            .HasOne<JobPosition>()
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermissionGrant>().HasIndex(x => new { x.RoleId, x.Permission }).IsUnique();
        modelBuilder.Entity<RolePermissionGrant>().Property(x => x.Permission).HasMaxLength(128);
        modelBuilder.Entity<RolePermissionGrant>()
            .HasOne<AppRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PutAwayTask>().HasIndex(x => x.TaskNo).IsUnique();
        modelBuilder.Entity<PutAwayTask>().HasIndex(x => new { x.WarehouseId, x.Status });
        modelBuilder.Entity<PutAwayTask>().Property(x => x.TaskNo).HasMaxLength(64);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.SuggestedLocationCode).HasMaxLength(64);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.SuggestedLocationName).HasMaxLength(128);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.ContainerCode).HasMaxLength(64);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.SourceDocumentNo).HasMaxLength(64);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.AssignedTo).HasMaxLength(128);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<PutAwayTask>().Property(x => x.CompletedBy).HasMaxLength(128);

        modelBuilder.Entity<PickingTask>().HasIndex(x => x.TaskNo).IsUnique();
        modelBuilder.Entity<PickingTask>().HasIndex(x => new { x.WarehouseId, x.Status });
        modelBuilder.Entity<PickingTask>().HasIndex(x => x.WaveId);
        modelBuilder.Entity<PickingTask>().Property(x => x.TaskNo).HasMaxLength(64);
        modelBuilder.Entity<PickingTask>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<PickingTask>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<PickingTask>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<PickingTask>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<PickingTask>().Property(x => x.Unit).HasMaxLength(32);
        modelBuilder.Entity<PickingTask>().Property(x => x.SourceLocationCode).HasMaxLength(64);
        modelBuilder.Entity<PickingTask>().Property(x => x.SourceLocationName).HasMaxLength(128);
        modelBuilder.Entity<PickingTask>().Property(x => x.WaveNo).HasMaxLength(64);
        modelBuilder.Entity<PickingTask>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<PickingTask>().Property(x => x.AssignedTo).HasMaxLength(128);
        modelBuilder.Entity<PickingTask>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<PickingTask>().Property(x => x.CompletedBy).HasMaxLength(128);

        modelBuilder.Entity<PickingWave>().HasIndex(x => x.WaveNo).IsUnique();
        modelBuilder.Entity<PickingWave>().HasIndex(x => new { x.WarehouseId, x.Status });
        modelBuilder.Entity<PickingWave>().Property(x => x.WaveNo).HasMaxLength(64);
        modelBuilder.Entity<PickingWave>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<PickingWave>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<PickingWave>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<PickingWave>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<PickingWave>().Property(x => x.ReleasedBy).HasMaxLength(128);

        modelBuilder.Entity<WarehouseContainer>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<WarehouseContainer>().HasIndex(x => x.WarehouseId);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.Code).HasMaxLength(64);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.ContainerType).HasMaxLength(64);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.CurrentLocationCode).HasMaxLength(64);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.CurrentLocationName).HasMaxLength(128);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<WarehouseContainer>().Property(x => x.LastHandledBy).HasMaxLength(128);

        modelBuilder.Entity<WarehouseRoute>().HasIndex(x => new { x.WarehouseId, x.FromLocationId, x.ToLocationId }).IsUnique();
        modelBuilder.Entity<WarehouseRoute>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<WarehouseRoute>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<WarehouseRoute>().Property(x => x.FromLocationCode).HasMaxLength(64);
        modelBuilder.Entity<WarehouseRoute>().Property(x => x.FromLocationName).HasMaxLength(128);
        modelBuilder.Entity<WarehouseRoute>().Property(x => x.ToLocationCode).HasMaxLength(64);
        modelBuilder.Entity<WarehouseRoute>().Property(x => x.ToLocationName).HasMaxLength(128);

        modelBuilder.Entity<PdaWorkQueueItem>().HasIndex(x => new { x.TaskType, x.TaskId }).IsUnique();
        modelBuilder.Entity<PdaWorkQueueItem>().HasIndex(x => new { x.WarehouseId, x.Status });
        modelBuilder.Entity<PdaWorkQueueItem>().Property(x => x.TaskType).HasMaxLength(32);
        modelBuilder.Entity<PdaWorkQueueItem>().Property(x => x.TaskNo).HasMaxLength(64);
        modelBuilder.Entity<PdaWorkQueueItem>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<PdaWorkQueueItem>().Property(x => x.LocationCode).HasMaxLength(64);
        modelBuilder.Entity<PdaWorkQueueItem>().Property(x => x.AssignedTo).HasMaxLength(128);
        modelBuilder.Entity<PdaWorkQueueItem>().Property(x => x.Status).HasMaxLength(32);

        modelBuilder.Entity<WorkCenter>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<WorkCenter>().HasIndex(x => x.WarehouseId);
        modelBuilder.Entity<WorkCenter>().Property(x => x.Code).HasMaxLength(64);
        modelBuilder.Entity<WorkCenter>().Property(x => x.Name).HasMaxLength(128);
        modelBuilder.Entity<WorkCenter>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<WorkCenter>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<WorkCenter>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<ManufacturingRouting>().HasIndex(x => x.RoutingNo).IsUnique();
        modelBuilder.Entity<ManufacturingRouting>().HasIndex(x => new { x.FinishedItemId, x.Version }).IsUnique();
        modelBuilder.Entity<ManufacturingRouting>().Property(x => x.RoutingNo).HasMaxLength(64);
        modelBuilder.Entity<ManufacturingRouting>().Property(x => x.FinishedItemCode).HasMaxLength(64);
        modelBuilder.Entity<ManufacturingRouting>().Property(x => x.FinishedItemName).HasMaxLength(128);
        modelBuilder.Entity<ManufacturingRouting>().Property(x => x.Version).HasMaxLength(32);
        modelBuilder.Entity<ManufacturingRouting>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ManufacturingRouting>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<ManufacturingRouting>()
            .HasMany(x => x.Operations)
            .WithOne()
            .HasForeignKey(x => x.ManufacturingRoutingId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ManufacturingRouting>().Navigation(x => x.Operations).UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.Entity<RoutingOperation>().ToTable("RoutingOperations");
        modelBuilder.Entity<RoutingOperation>().HasKey(x => x.Id);
        modelBuilder.Entity<RoutingOperation>().HasIndex(x => new { x.ManufacturingRoutingId, x.Sequence }).IsUnique();
        modelBuilder.Entity<RoutingOperation>().HasIndex(x => x.WorkCenterId);
        modelBuilder.Entity<RoutingOperation>().Property(x => x.OperationCode).HasMaxLength(64);
        modelBuilder.Entity<RoutingOperation>().Property(x => x.OperationName).HasMaxLength(128);
        modelBuilder.Entity<RoutingOperation>().Property(x => x.WorkCenterCode).HasMaxLength(64);
        modelBuilder.Entity<RoutingOperation>().Property(x => x.WorkCenterName).HasMaxLength(128);

        modelBuilder.Entity<OperationSchedule>().HasIndex(x => x.ScheduleNo).IsUnique();
        modelBuilder.Entity<OperationSchedule>().HasIndex(x => x.WorkOrderId);
        modelBuilder.Entity<OperationSchedule>().HasIndex(x => new { x.WorkCenterId, x.Status });
        modelBuilder.Entity<OperationSchedule>().Property(x => x.ScheduleNo).HasMaxLength(64);
        modelBuilder.Entity<OperationSchedule>().Property(x => x.WorkOrderNo).HasMaxLength(64);
        modelBuilder.Entity<OperationSchedule>().Property(x => x.OperationCode).HasMaxLength(64);
        modelBuilder.Entity<OperationSchedule>().Property(x => x.OperationName).HasMaxLength(128);
        modelBuilder.Entity<OperationSchedule>().Property(x => x.WorkCenterCode).HasMaxLength(64);
        modelBuilder.Entity<OperationSchedule>().Property(x => x.WorkCenterName).HasMaxLength(128);
        modelBuilder.Entity<OperationSchedule>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<OperationSchedule>().Property(x => x.ScheduledBy).HasMaxLength(128);

        modelBuilder.Entity<CapacityLoad>().HasIndex(x => new { x.WorkCenterId, x.PlanDate }).IsUnique();
        modelBuilder.Entity<CapacityLoad>().Property(x => x.WorkCenterCode).HasMaxLength(64);
        modelBuilder.Entity<CapacityLoad>().Property(x => x.WorkCenterName).HasMaxLength(128);
        modelBuilder.Entity<CapacityLoad>().Property(x => x.SourceDocumentNo).HasMaxLength(64);
        modelBuilder.Entity<CapacityLoad>().Property(x => x.UpdatedBy).HasMaxLength(128);
        modelBuilder.Entity<CapacityLoad>().Ignore(x => x.RemainingMinutes);

        modelBuilder.Entity<ManufacturingCostSnapshot>().HasIndex(x => x.SnapshotNo).IsUnique();
        modelBuilder.Entity<ManufacturingCostSnapshot>().HasIndex(x => x.WorkOrderId);
        modelBuilder.Entity<ManufacturingCostSnapshot>().Property(x => x.SnapshotNo).HasMaxLength(64);
        modelBuilder.Entity<ManufacturingCostSnapshot>().Property(x => x.WorkOrderNo).HasMaxLength(64);
        modelBuilder.Entity<ManufacturingCostSnapshot>().Property(x => x.FinishedItemCode).HasMaxLength(64);
        modelBuilder.Entity<ManufacturingCostSnapshot>().Property(x => x.FinishedItemName).HasMaxLength(128);
        modelBuilder.Entity<ManufacturingCostSnapshot>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<ManufacturingCostSnapshot>().Ignore(x => x.TotalCost);

        modelBuilder.Entity<MrpSuggestion>().HasIndex(x => x.SuggestionNo).IsUnique();
        modelBuilder.Entity<MrpSuggestion>().HasIndex(x => new { x.WarehouseId, x.ItemId, x.Status });
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.SuggestionNo).HasMaxLength(64);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.ItemCode).HasMaxLength(64);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.ItemName).HasMaxLength(128);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.WarehouseCode).HasMaxLength(64);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.WarehouseName).HasMaxLength(128);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.SourceType).HasMaxLength(64);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.DecidedBy).HasMaxLength(128);
        modelBuilder.Entity<MrpSuggestion>().Property(x => x.DecisionNote).HasMaxLength(256);

        modelBuilder.Entity<ReportDefinition>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<ReportDefinition>().Property(x => x.Key).HasMaxLength(96);
        modelBuilder.Entity<ReportDefinition>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<ReportDefinition>().Property(x => x.Category).HasMaxLength(64);
        modelBuilder.Entity<ReportDefinition>().Property(x => x.QueryModel).HasMaxLength(64);
        modelBuilder.Entity<ReportDefinition>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<ReportRunRecord>().HasIndex(x => x.RunNo).IsUnique();
        modelBuilder.Entity<ReportRunRecord>().HasIndex(x => x.ReportDefinitionId);
        modelBuilder.Entity<ReportRunRecord>().Property(x => x.RunNo).HasMaxLength(64);
        modelBuilder.Entity<ReportRunRecord>().Property(x => x.ReportKey).HasMaxLength(96);
        modelBuilder.Entity<ReportRunRecord>().Property(x => x.ReportName).HasMaxLength(128);
        modelBuilder.Entity<ReportRunRecord>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ReportRunRecord>().Property(x => x.RunBy).HasMaxLength(128);

        modelBuilder.Entity<ReportExportTask>().HasIndex(x => x.ExportNo).IsUnique();
        modelBuilder.Entity<ReportExportTask>().HasIndex(x => x.ReportRunRecordId);
        modelBuilder.Entity<ReportExportTask>().Property(x => x.ExportNo).HasMaxLength(64);
        modelBuilder.Entity<ReportExportTask>().Property(x => x.ReportName).HasMaxLength(128);
        modelBuilder.Entity<ReportExportTask>().Property(x => x.Format).HasMaxLength(16);
        modelBuilder.Entity<ReportExportTask>().Property(x => x.FileName).HasMaxLength(192);
        modelBuilder.Entity<ReportExportTask>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ReportExportTask>().Property(x => x.RequestedBy).HasMaxLength(128);

        modelBuilder.Entity<MobileDevice>().HasIndex(x => x.DeviceCode).IsUnique();
        modelBuilder.Entity<MobileDevice>().HasIndex(x => x.AssignedTo);
        modelBuilder.Entity<MobileDevice>().Property(x => x.DeviceCode).HasMaxLength(64);
        modelBuilder.Entity<MobileDevice>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<MobileDevice>().Property(x => x.AssignedTo).HasMaxLength(128);
        modelBuilder.Entity<MobileDevice>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<MobileOfflineTask>().HasIndex(x => x.TaskNo).IsUnique();
        modelBuilder.Entity<MobileOfflineTask>().HasIndex(x => new { x.AssignedTo, x.Status });
        modelBuilder.Entity<MobileOfflineTask>().HasIndex(x => new { x.SourceModule, x.SourceTaskType, x.SourceTaskNo });
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.TaskNo).HasMaxLength(64);
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.SourceModule).HasMaxLength(64);
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.SourceTaskType).HasMaxLength(64);
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.SourceTaskNo).HasMaxLength(64);
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.AssignedTo).HasMaxLength(128);
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<MobileOfflineTask>().Property(x => x.CompletedBy).HasMaxLength(128);

        modelBuilder.Entity<MobileScanEvent>().HasIndex(x => x.ScanNo).IsUnique();
        modelBuilder.Entity<MobileScanEvent>().HasIndex(x => x.DeviceCode);
        modelBuilder.Entity<MobileScanEvent>().HasIndex(x => new { x.TargetModule, x.DocumentNo });
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.ScanNo).HasMaxLength(64);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.DeviceCode).HasMaxLength(64);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.Barcode).HasMaxLength(192);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.TargetModule).HasMaxLength(64);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.Action).HasMaxLength(64);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.DocumentNo).HasMaxLength(64);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.Result).HasMaxLength(32);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.Message).HasMaxLength(256);
        modelBuilder.Entity<MobileScanEvent>().Property(x => x.Actor).HasMaxLength(128);

        modelBuilder.Entity<MessageChannel>().HasIndex(x => x.ChannelKey).IsUnique();
        modelBuilder.Entity<MessageChannel>().Property(x => x.ChannelKey).HasMaxLength(80);
        modelBuilder.Entity<MessageChannel>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<MessageChannel>().Property(x => x.ChannelType).HasMaxLength(64);
        modelBuilder.Entity<MessageChannel>().Property(x => x.Endpoint).HasMaxLength(256);
        modelBuilder.Entity<MessageChannel>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<WebhookSubscription>().HasIndex(x => x.SubscriptionKey).IsUnique();
        modelBuilder.Entity<WebhookSubscription>().HasIndex(x => x.EventKey);
        modelBuilder.Entity<WebhookSubscription>().Property(x => x.SubscriptionKey).HasMaxLength(80);
        modelBuilder.Entity<WebhookSubscription>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<WebhookSubscription>().Property(x => x.EventKey).HasMaxLength(96);
        modelBuilder.Entity<WebhookSubscription>().Property(x => x.TargetUrl).HasMaxLength(256);
        modelBuilder.Entity<WebhookSubscription>().Property(x => x.SecretName).HasMaxLength(128);
        modelBuilder.Entity<WebhookSubscription>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<ExternalConnector>().HasIndex(x => x.ConnectorKey).IsUnique();
        modelBuilder.Entity<ExternalConnector>().Property(x => x.ConnectorKey).HasMaxLength(80);
        modelBuilder.Entity<ExternalConnector>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<ExternalConnector>().Property(x => x.Provider).HasMaxLength(96);
        modelBuilder.Entity<ExternalConnector>().Property(x => x.BaseUrl).HasMaxLength(256);
        modelBuilder.Entity<ExternalConnector>().Property(x => x.AuthMode).HasMaxLength(64);
        modelBuilder.Entity<ExternalConnector>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<IntegrationSyncJob>().HasIndex(x => x.JobNo).IsUnique();
        modelBuilder.Entity<IntegrationSyncJob>().HasIndex(x => new { x.ConnectorKey, x.Status });
        modelBuilder.Entity<IntegrationSyncJob>().Property(x => x.JobNo).HasMaxLength(64);
        modelBuilder.Entity<IntegrationSyncJob>().Property(x => x.ConnectorKey).HasMaxLength(80);
        modelBuilder.Entity<IntegrationSyncJob>().Property(x => x.Direction).HasMaxLength(32);
        modelBuilder.Entity<IntegrationSyncJob>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<IntegrationSyncJob>().Property(x => x.LastError).HasMaxLength(512);
        modelBuilder.Entity<IntegrationSyncJob>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<IntegrationSyncJob>().Property(x => x.CompletedBy).HasMaxLength(128);

        modelBuilder.Entity<IntegrationAuditRecord>().HasIndex(x => x.AuditNo).IsUnique();
        modelBuilder.Entity<IntegrationAuditRecord>().HasIndex(x => new { x.Category, x.TargetKey });
        modelBuilder.Entity<IntegrationAuditRecord>().Property(x => x.AuditNo).HasMaxLength(64);
        modelBuilder.Entity<IntegrationAuditRecord>().Property(x => x.Category).HasMaxLength(64);
        modelBuilder.Entity<IntegrationAuditRecord>().Property(x => x.Action).HasMaxLength(64);
        modelBuilder.Entity<IntegrationAuditRecord>().Property(x => x.TargetKey).HasMaxLength(128);
        modelBuilder.Entity<IntegrationAuditRecord>().Property(x => x.Result).HasMaxLength(32);
        modelBuilder.Entity<IntegrationAuditRecord>().Property(x => x.Message).HasMaxLength(512);
        modelBuilder.Entity<IntegrationAuditRecord>().Property(x => x.Actor).HasMaxLength(128);

        modelBuilder.Entity<ImportTemplate>().HasIndex(x => x.TemplateKey).IsUnique();
        modelBuilder.Entity<ImportTemplate>().Property(x => x.TemplateKey).HasMaxLength(80);
        modelBuilder.Entity<ImportTemplate>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<ImportTemplate>().Property(x => x.TargetModule).HasMaxLength(64);
        modelBuilder.Entity<ImportTemplate>().Property(x => x.FileType).HasMaxLength(32);
        modelBuilder.Entity<ImportTemplate>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<ImportFieldMapping>().HasIndex(x => new { x.TemplateKey, x.TargetField }).IsUnique();
        modelBuilder.Entity<ImportFieldMapping>().Property(x => x.TemplateKey).HasMaxLength(80);
        modelBuilder.Entity<ImportFieldMapping>().Property(x => x.SourceField).HasMaxLength(128);
        modelBuilder.Entity<ImportFieldMapping>().Property(x => x.TargetField).HasMaxLength(128);
        modelBuilder.Entity<ImportFieldMapping>().Property(x => x.TransformRule).HasMaxLength(256);
        modelBuilder.Entity<ImportFieldMapping>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<ImportBatch>().HasIndex(x => x.BatchNo).IsUnique();
        modelBuilder.Entity<ImportBatch>().HasIndex(x => new { x.TemplateKey, x.Status });
        modelBuilder.Entity<ImportBatch>().Property(x => x.BatchNo).HasMaxLength(64);
        modelBuilder.Entity<ImportBatch>().Property(x => x.TemplateKey).HasMaxLength(80);
        modelBuilder.Entity<ImportBatch>().Property(x => x.FileName).HasMaxLength(192);
        modelBuilder.Entity<ImportBatch>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ImportBatch>().Property(x => x.ErrorMessage).HasMaxLength(512);
        modelBuilder.Entity<ImportBatch>().Property(x => x.CreatedBy).HasMaxLength(128);
        modelBuilder.Entity<ImportBatch>().Property(x => x.CompletedBy).HasMaxLength(128);

        modelBuilder.Entity<ExportFileTask>().HasIndex(x => x.ExportNo).IsUnique();
        modelBuilder.Entity<ExportFileTask>().HasIndex(x => new { x.SourceModule, x.Status });
        modelBuilder.Entity<ExportFileTask>().Property(x => x.ExportNo).HasMaxLength(64);
        modelBuilder.Entity<ExportFileTask>().Property(x => x.SourceModule).HasMaxLength(64);
        modelBuilder.Entity<ExportFileTask>().Property(x => x.FileName).HasMaxLength(192);
        modelBuilder.Entity<ExportFileTask>().Property(x => x.Format).HasMaxLength(16);
        modelBuilder.Entity<ExportFileTask>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<ExportFileTask>().Property(x => x.RequestedBy).HasMaxLength(128);
        modelBuilder.Entity<ExportFileTask>().Property(x => x.CompletedBy).HasMaxLength(128);

        modelBuilder.Entity<PrintTemplate>().HasIndex(x => x.TemplateKey).IsUnique();
        modelBuilder.Entity<PrintTemplate>().Property(x => x.TemplateKey).HasMaxLength(80);
        modelBuilder.Entity<PrintTemplate>().Property(x => x.DisplayName).HasMaxLength(128);
        modelBuilder.Entity<PrintTemplate>().Property(x => x.TargetModule).HasMaxLength(64);
        modelBuilder.Entity<PrintTemplate>().Property(x => x.ContentType).HasMaxLength(64);
        modelBuilder.Entity<PrintTemplate>().Property(x => x.UpdatedBy).HasMaxLength(128);

        modelBuilder.Entity<PrintJob>().HasIndex(x => x.JobNo).IsUnique();
        modelBuilder.Entity<PrintJob>().HasIndex(x => new { x.TemplateKey, x.Status });
        modelBuilder.Entity<PrintJob>().Property(x => x.JobNo).HasMaxLength(64);
        modelBuilder.Entity<PrintJob>().Property(x => x.TemplateKey).HasMaxLength(80);
        modelBuilder.Entity<PrintJob>().Property(x => x.DocumentNo).HasMaxLength(96);
        modelBuilder.Entity<PrintJob>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<PrintJob>().Property(x => x.RequestedBy).HasMaxLength(128);
        modelBuilder.Entity<PrintJob>().Property(x => x.CompletedBy).HasMaxLength(128);

        modelBuilder.Entity<FileAuditRecord>().HasIndex(x => x.AuditNo).IsUnique();
        modelBuilder.Entity<FileAuditRecord>().HasIndex(x => new { x.Category, x.TargetNo });
        modelBuilder.Entity<FileAuditRecord>().Property(x => x.AuditNo).HasMaxLength(64);
        modelBuilder.Entity<FileAuditRecord>().Property(x => x.Category).HasMaxLength(64);
        modelBuilder.Entity<FileAuditRecord>().Property(x => x.Action).HasMaxLength(64);
        modelBuilder.Entity<FileAuditRecord>().Property(x => x.TargetNo).HasMaxLength(128);
        modelBuilder.Entity<FileAuditRecord>().Property(x => x.Result).HasMaxLength(32);
        modelBuilder.Entity<FileAuditRecord>().Property(x => x.Message).HasMaxLength(512);
        modelBuilder.Entity<FileAuditRecord>().Property(x => x.Actor).HasMaxLength(128);
    }
}
