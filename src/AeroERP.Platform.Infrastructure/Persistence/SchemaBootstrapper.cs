using Microsoft.EntityFrameworkCore;

namespace AeroERP.Platform.Infrastructure.Persistence;

/// <summary>
/// Schema Bootstrapper 业务对象。
/// </summary>
public static class SchemaBootstrapper
{
    /// <summary>
    /// Ensure Incremental Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureIncrementalSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteExtensionSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresExtensionSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Extension Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteExtensionSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "Customers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "ContactName" TEXT NOT NULL,
                "Phone" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Customers_Code" ON "Customers" ("Code");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesQuotations" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_SalesQuotations" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "QuotationNo" TEXT NOT NULL,
                "CustomerId" TEXT NOT NULL,
                "CustomerName" TEXT NOT NULL,
                "Title" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "SalesOrderId" TEXT NULL,
                CONSTRAINT "FK_SalesQuotations_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_SalesQuotations_QuotationNo" ON "SalesQuotations" ("QuotationNo");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesQuotationLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_SalesQuotationLine" PRIMARY KEY,
                "SalesQuotationId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                CONSTRAINT "FK_SalesQuotationLine_SalesQuotations_SalesQuotationId" FOREIGN KEY ("SalesQuotationId") REFERENCES "SalesQuotations" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_SalesQuotationLine_SalesQuotationId" ON "SalesQuotationLine" ("SalesQuotationId");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesOrders" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_SalesOrders" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "OrderNo" TEXT NOT NULL,
                "QuotationId" TEXT NOT NULL,
                "QuotationNo" TEXT NOT NULL,
                "CustomerId" TEXT NOT NULL,
                "CustomerName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                CONSTRAINT "FK_SalesOrders_SalesQuotations_QuotationId" FOREIGN KEY ("QuotationId") REFERENCES "SalesQuotations" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_SalesOrders_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_SalesOrders_OrderNo" ON "SalesOrders" ("OrderNo");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesOrderLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_SalesOrderLine" PRIMARY KEY,
                "SalesOrderId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                CONSTRAINT "FK_SalesOrderLine_SalesOrders_SalesOrderId" FOREIGN KEY ("SalesOrderId") REFERENCES "SalesOrders" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_SalesOrderLine_SalesOrderId" ON "SalesOrderLine" ("SalesOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryReceipts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryReceipts" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ReceiptNo" TEXT NOT NULL,
                "ProcurementOrderId" TEXT NOT NULL,
                "ProcurementOrderNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "SupplierName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "ReceivedBy" TEXT NOT NULL,
                CONSTRAINT "FK_InventoryReceipts_ProcurementOrders_ProcurementOrderId" FOREIGN KEY ("ProcurementOrderId") REFERENCES "ProcurementOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_InventoryReceipts_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryReceipts_ReceiptNo" ON "InventoryReceipts" ("ReceiptNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryReceiptLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryReceiptLine" PRIMARY KEY,
                "InventoryReceiptId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "CostAmount" TEXT NOT NULL DEFAULT '0',
                CONSTRAINT "FK_InventoryReceiptLine_InventoryReceipts_InventoryReceiptId" FOREIGN KEY ("InventoryReceiptId") REFERENCES "InventoryReceipts" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryReceiptLine_InventoryReceiptId" ON "InventoryReceiptLine" ("InventoryReceiptId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryIssues" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryIssues" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "IssueNo" TEXT NOT NULL,
                "SalesOrderId" TEXT NOT NULL,
                "SalesOrderNo" TEXT NOT NULL,
                "QuotationNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "CustomerName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "IssuedBy" TEXT NOT NULL,
                CONSTRAINT "FK_InventoryIssues_SalesOrders_SalesOrderId" FOREIGN KEY ("SalesOrderId") REFERENCES "SalesOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_InventoryIssues_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryIssues_IssueNo" ON "InventoryIssues" ("IssueNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryIssueLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryIssueLine" PRIMARY KEY,
                "InventoryIssueId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "CostAmount" TEXT NOT NULL DEFAULT '0',
                CONSTRAINT "FK_InventoryIssueLine_InventoryIssues_InventoryIssueId" FOREIGN KEY ("InventoryIssueId") REFERENCES "InventoryIssues" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryIssueLine_InventoryIssueId" ON "InventoryIssueLine" ("InventoryIssueId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryTransfers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryTransfers" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TransferNo" TEXT NOT NULL,
                "FromWarehouseId" TEXT NOT NULL,
                "FromWarehouseCode" TEXT NOT NULL,
                "FromWarehouseName" TEXT NOT NULL,
                "ToWarehouseId" TEXT NOT NULL,
                "ToWarehouseCode" TEXT NOT NULL,
                "ToWarehouseName" TEXT NOT NULL,
                "Reason" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "ExecutedBy" TEXT NOT NULL,
                CONSTRAINT "FK_InventoryTransfers_Warehouses_FromWarehouseId" FOREIGN KEY ("FromWarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_InventoryTransfers_Warehouses_ToWarehouseId" FOREIGN KEY ("ToWarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryTransfers_TransferNo" ON "InventoryTransfers" ("TransferNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryTransferLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryTransferLine" PRIMARY KEY,
                "InventoryTransferId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "CostAmount" TEXT NOT NULL DEFAULT '0',
                CONSTRAINT "FK_InventoryTransferLine_InventoryTransfers_InventoryTransferId" FOREIGN KEY ("InventoryTransferId") REFERENCES "InventoryTransfers" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryTransferLine_InventoryTransferId" ON "InventoryTransferLine" ("InventoryTransferId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryCountAdjustments" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryCountAdjustments" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "CountNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "Reason" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CountedBy" TEXT NOT NULL,
                CONSTRAINT "FK_InventoryCountAdjustments_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryCountAdjustments_CountNo" ON "InventoryCountAdjustments" ("CountNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryCountAdjustmentLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryCountAdjustmentLine" PRIMARY KEY,
                "InventoryCountAdjustmentId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "BeforeQuantity" TEXT NOT NULL,
                "CountedQuantity" TEXT NOT NULL,
                "DeltaQuantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "CostAmount" TEXT NOT NULL DEFAULT '0',
                CONSTRAINT "FK_InventoryCountAdjustmentLine_InventoryCountAdjustments_InventoryCountAdjustmentId" FOREIGN KEY ("InventoryCountAdjustmentId") REFERENCES "InventoryCountAdjustments" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryCountAdjustmentLine_InventoryCountAdjustmentId" ON "InventoryCountAdjustmentLine" ("InventoryCountAdjustmentId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryMovements" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_InventoryMovements" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "DocumentType" TEXT NOT NULL,
                "DocumentNo" TEXT NOT NULL,
                "MovementType" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "LocationId" TEXT NULL,
                "LocationCode" TEXT NOT NULL DEFAULT '',
                "LocationName" TEXT NOT NULL DEFAULT '',
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "ChangeQuantity" TEXT NOT NULL,
                "BalanceAfter" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "CostAmount" TEXT NOT NULL DEFAULT '0',
                "BalanceCostAfter" TEXT NOT NULL DEFAULT '0',
                "Actor" TEXT NOT NULL,
                CONSTRAINT "FK_InventoryMovements_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_InventoryMovements_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryMovements_DocumentNo" ON "InventoryMovements" ("DocumentNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_InventoryMovements_WarehouseId_ItemId" ON "InventoryMovements" ("WarehouseId", "ItemId");""",
            """
            CREATE TABLE IF NOT EXISTS "StockBalances" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_StockBalances" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "InventoryValue" TEXT NOT NULL DEFAULT '0',
                CONSTRAINT "FK_StockBalances_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_StockBalances_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_StockBalances_WarehouseId_ItemId" ON "StockBalances" ("WarehouseId", "ItemId");""",
            """
            CREATE TABLE IF NOT EXISTS "WarehouseLocations" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WarehouseLocations" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                CONSTRAINT "FK_WarehouseLocations_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WarehouseLocations_WarehouseId_Code" ON "WarehouseLocations" ("WarehouseId", "Code");""",
            """
            CREATE TABLE IF NOT EXISTS "LocationStockBalances" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_LocationStockBalances" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "LocationId" TEXT NOT NULL,
                "LocationCode" TEXT NOT NULL,
                "LocationName" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "InventoryValue" TEXT NOT NULL DEFAULT '0',
                CONSTRAINT "FK_LocationStockBalances_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_LocationStockBalances_WarehouseLocations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "WarehouseLocations" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_LocationStockBalances_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_LocationStockBalances_LocationId_ItemId" ON "LocationStockBalances" ("LocationId", "ItemId");""",
            """CREATE INDEX IF NOT EXISTS "IX_LocationStockBalances_WarehouseId_ItemId" ON "LocationStockBalances" ("WarehouseId", "ItemId");""",
            """
            CREATE TABLE IF NOT EXISTS "AccountingAccounts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingAccounts" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Type" TEXT NOT NULL,
                "ParentAccountId" TEXT NULL,
                "IsActive" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_AccountingAccounts_Code" ON "AccountingAccounts" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_AccountingAccounts_ParentAccountId" ON "AccountingAccounts" ("ParentAccountId");""",
            """
            CREATE TABLE IF NOT EXISTS "AccountingPeriods" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingPeriods" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Year" INTEGER NOT NULL,
                "Month" INTEGER NOT NULL,
                "Name" TEXT NOT NULL,
                "StartDate" TEXT NOT NULL,
                "EndDate" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "ClosedBy" TEXT NOT NULL,
                "ClosedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_AccountingPeriods_Name" ON "AccountingPeriods" ("Name");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_AccountingPeriods_Year_Month" ON "AccountingPeriods" ("Year", "Month");""",
            """
            CREATE TABLE IF NOT EXISTS "GeneralLedgerVouchers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_GeneralLedgerVouchers" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "VoucherNo" TEXT NOT NULL,
                "AccountingPeriodId" TEXT NOT NULL,
                "AccountingPeriodName" TEXT NOT NULL,
                "DocumentDate" TEXT NOT NULL,
                "Summary" TEXT NOT NULL,
                "SourceType" TEXT NOT NULL,
                "SourceId" TEXT NULL,
                "SourceNo" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "SubmittedBy" TEXT NOT NULL,
                "SubmittedAtUtc" TEXT NULL,
                "ReviewedBy" TEXT NOT NULL,
                "ReviewedAtUtc" TEXT NULL,
                "ReviewNote" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_GeneralLedgerVouchers_VoucherNo" ON "GeneralLedgerVouchers" ("VoucherNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_GeneralLedgerVouchers_AccountingPeriodId_Status" ON "GeneralLedgerVouchers" ("AccountingPeriodId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "GeneralLedgerVoucherLines" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_GeneralLedgerVoucherLines" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "GeneralLedgerVoucherId" TEXT NOT NULL,
                "AccountingAccountId" TEXT NOT NULL,
                "AccountCode" TEXT NOT NULL,
                "AccountName" TEXT NOT NULL,
                "Summary" TEXT NOT NULL,
                "DebitAmount" TEXT NOT NULL,
                "CreditAmount" TEXT NOT NULL,
                CONSTRAINT "FK_GeneralLedgerVoucherLines_GeneralLedgerVouchers_GeneralLedgerVoucherId" FOREIGN KEY ("GeneralLedgerVoucherId") REFERENCES "GeneralLedgerVouchers" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_GeneralLedgerVoucherLines_GeneralLedgerVoucherId" ON "GeneralLedgerVoucherLines" ("GeneralLedgerVoucherId");""",
            """CREATE INDEX IF NOT EXISTS "IX_GeneralLedgerVoucherLines_AccountingAccountId" ON "GeneralLedgerVoucherLines" ("AccountingAccountId");""",
            """
            CREATE TABLE IF NOT EXISTS "Payables" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Payables" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "PayableNo" TEXT NOT NULL,
                "ProcurementOrderId" TEXT NOT NULL,
                "ProcurementOrderNo" TEXT NOT NULL,
                "InventoryReceiptId" TEXT NULL,
                "InventoryReceiptNo" TEXT NOT NULL,
                "SupplierName" TEXT NOT NULL,
                "Amount" TEXT NOT NULL,
                "NetAmount" TEXT NOT NULL,
                "TaxAmount" TEXT NOT NULL,
                "TaxRate" TEXT NOT NULL,
                "TaxInvoiceType" TEXT NOT NULL,
                "SettledAmount" TEXT NOT NULL,
                "DueDate" TEXT NULL,
                "Status" TEXT NOT NULL,
                "SourceType" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                CONSTRAINT "FK_Payables_ProcurementOrders_ProcurementOrderId" FOREIGN KEY ("ProcurementOrderId") REFERENCES "ProcurementOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Payables_InventoryReceipts_InventoryReceiptId" FOREIGN KEY ("InventoryReceiptId") REFERENCES "InventoryReceipts" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Payables_PayableNo" ON "Payables" ("PayableNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_Payables_ProcurementOrderId" ON "Payables" ("ProcurementOrderId");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Payables_InventoryReceiptId" ON "Payables" ("InventoryReceiptId") WHERE "InventoryReceiptId" IS NOT NULL;""",
            """
            CREATE TABLE IF NOT EXISTS "Receivables" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Receivables" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ReceivableNo" TEXT NOT NULL,
                "SalesOrderId" TEXT NOT NULL,
                "SalesOrderNo" TEXT NOT NULL,
                "InventoryIssueId" TEXT NULL,
                "InventoryIssueNo" TEXT NOT NULL,
                "CustomerName" TEXT NOT NULL,
                "Amount" TEXT NOT NULL,
                "NetAmount" TEXT NOT NULL,
                "TaxAmount" TEXT NOT NULL,
                "TaxRate" TEXT NOT NULL,
                "TaxInvoiceType" TEXT NOT NULL,
                "SettledAmount" TEXT NOT NULL,
                "DueDate" TEXT NULL,
                "Status" TEXT NOT NULL,
                "SourceType" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                CONSTRAINT "FK_Receivables_SalesOrders_SalesOrderId" FOREIGN KEY ("SalesOrderId") REFERENCES "SalesOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Receivables_InventoryIssues_InventoryIssueId" FOREIGN KEY ("InventoryIssueId") REFERENCES "InventoryIssues" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Receivables_ReceivableNo" ON "Receivables" ("ReceivableNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_Receivables_SalesOrderId" ON "Receivables" ("SalesOrderId");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Receivables_InventoryIssueId" ON "Receivables" ("InventoryIssueId") WHERE "InventoryIssueId" IS NOT NULL;""",
            """
            CREATE TABLE IF NOT EXISTS "FinanceInvoices" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_FinanceInvoices" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "InvoiceNo" TEXT NOT NULL,
                "Direction" TEXT NOT NULL,
                "SourceId" TEXT NOT NULL,
                "SourceNo" TEXT NOT NULL,
                "CounterpartyName" TEXT NOT NULL,
                "TaxInvoiceType" TEXT NOT NULL,
                "TaxRate" TEXT NOT NULL,
                "GrossAmount" TEXT NOT NULL,
                "NetAmount" TEXT NOT NULL,
                "TaxAmount" TEXT NOT NULL,
                "CurrencyCode" TEXT NOT NULL,
                "InvoiceDate" TEXT NOT NULL,
                "Note" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_InvoiceNo" ON "FinanceInvoices" ("InvoiceNo");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_Direction_SourceId" ON "FinanceInvoices" ("Direction", "SourceId");""",
            """
            CREATE TABLE IF NOT EXISTS "BankAccounts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_BankAccounts" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "AccountNo" TEXT NOT NULL,
                "AccountName" TEXT NOT NULL,
                "BankName" TEXT NOT NULL,
                "CurrencyCode" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankAccounts_AccountNo" ON "BankAccounts" ("AccountNo");""",
            """
            CREATE TABLE IF NOT EXISTS "BankStatementLines" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_BankStatementLines" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "StatementNo" TEXT NOT NULL,
                "BankAccountId" TEXT NOT NULL,
                "BankAccountNo" TEXT NOT NULL,
                "BankAccountName" TEXT NOT NULL,
                "TransactionDate" TEXT NOT NULL,
                "Direction" TEXT NOT NULL,
                "Amount" TEXT NOT NULL,
                "CurrencyCode" TEXT NOT NULL,
                "CounterpartyName" TEXT NOT NULL,
                "BankReferenceNo" TEXT NOT NULL,
                "Summary" TEXT NOT NULL,
                "ReconciliationStatus" TEXT NOT NULL,
                "SettlementId" TEXT NULL,
                "SettlementNo" TEXT NOT NULL,
                "ReconciledBy" TEXT NOT NULL,
                "ReconciledAtUtc" TEXT NULL,
                "CreatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_StatementNo" ON "BankStatementLines" ("StatementNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_BankAccountId" ON "BankStatementLines" ("BankAccountId");""",
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_ReconciliationStatus_TransactionDate" ON "BankStatementLines" ("ReconciliationStatus", "TransactionDate");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_SettlementId" ON "BankStatementLines" ("SettlementId") WHERE "SettlementId" IS NOT NULL;""",
            """
            CREATE TABLE IF NOT EXISTS "Settlements" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Settlements" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "SettlementNo" TEXT NOT NULL,
                "TargetType" TEXT NOT NULL,
                "TargetId" TEXT NOT NULL,
                "TargetNo" TEXT NOT NULL,
                "CounterpartyName" TEXT NOT NULL,
                "Amount" TEXT NOT NULL,
                "CurrencyCode" TEXT NOT NULL,
                "BankAccountId" TEXT NOT NULL,
                "BankAccountNo" TEXT NOT NULL,
                "BankAccountName" TEXT NOT NULL,
                "Method" TEXT NOT NULL,
                "Note" TEXT NOT NULL,
                "ReconciliationStatus" TEXT NOT NULL,
                "BankStatementLineId" TEXT NULL,
                "BankStatementNo" TEXT NOT NULL,
                "ReconciledBy" TEXT NOT NULL,
                "ReconciledAtUtc" TEXT NULL,
                "SettledBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Settlements_SettlementNo" ON "Settlements" ("SettlementNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_TargetType_TargetId" ON "Settlements" ("TargetType", "TargetId");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkflowDefinitions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WorkflowDefinitions" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Key" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "ModuleKey" TEXT NOT NULL,
                "DocumentType" TEXT NOT NULL,
                "RequiredPermission" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkflowDefinitions_Key" ON "WorkflowDefinitions" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkflowInstances" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WorkflowInstances" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "DefinitionId" TEXT NOT NULL,
                "DefinitionKey" TEXT NOT NULL,
                "DefinitionName" TEXT NOT NULL,
                "DocumentType" TEXT NOT NULL,
                "DocumentId" TEXT NOT NULL,
                "DocumentNo" TEXT NOT NULL,
                "Title" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "SubmittedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_WorkflowInstances_DefinitionKey_DocumentId" ON "WorkflowInstances" ("DefinitionKey", "DocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "ApprovalTasks" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ApprovalTasks" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WorkflowInstanceId" TEXT NOT NULL,
                "DefinitionKey" TEXT NOT NULL,
                "DefinitionName" TEXT NOT NULL,
                "DocumentType" TEXT NOT NULL,
                "DocumentId" TEXT NOT NULL,
                "DocumentNo" TEXT NOT NULL,
                "Title" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "SubmittedBy" TEXT NOT NULL,
                "RequiredPermission" TEXT NOT NULL,
                "DecidedBy" TEXT NULL,
                "Decision" TEXT NULL,
                "Comment" TEXT NULL,
                "DecidedAtUtc" TEXT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_ApprovalTasks_WorkflowInstanceId_Status" ON "ApprovalTasks" ("WorkflowInstanceId", "Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_ApprovalTasks_DocumentId" ON "ApprovalTasks" ("DocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "Notifications" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Notifications" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Title" TEXT NOT NULL,
                "Message" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "RelatedDocumentType" TEXT NOT NULL,
                "RelatedDocumentId" TEXT NOT NULL,
                "RelatedDocumentNo" TEXT NOT NULL,
                "RecipientPermission" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "ReadAtUtc" TEXT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_Notifications_Status" ON "Notifications" ("Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_Notifications_RelatedDocumentId" ON "Notifications" ("RelatedDocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "DataScopeRules" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_DataScopeRules" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "RoleKey" TEXT NOT NULL,
                "ScopeType" TEXT NOT NULL,
                "MatchValue" TEXT NOT NULL,
                "Description" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_DataScopeRules_RoleKey_ScopeType" ON "DataScopeRules" ("RoleKey", "ScopeType");""",
            """
            CREATE TABLE IF NOT EXISTS "NumberingRules" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_NumberingRules" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "DocumentType" TEXT NOT NULL,
                "Prefix" TEXT NOT NULL,
                "UseDateSegment" INTEGER NOT NULL,
                "NextSequence" INTEGER NOT NULL,
                "Padding" INTEGER NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_NumberingRules_DocumentType" ON "NumberingRules" ("DocumentType");""",
            """
            CREATE TABLE IF NOT EXISTS "Currencies" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Currencies" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Symbol" TEXT NOT NULL,
                "ExchangeRateToBase" TEXT NOT NULL,
                "IsBase" INTEGER NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Currencies_Code" ON "Currencies" ("Code");""",
            """
            CREATE TABLE IF NOT EXISTS "LocalizationSettings" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_LocalizationSettings" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "DefaultCurrencyCode" TEXT NOT NULL,
                "TaxInvoiceType" TEXT NOT NULL,
                "TaxpayerId" TEXT NOT NULL,
                "InvoiceTitle" TEXT NOT NULL,
                "DefaultTaxRate" TEXT NOT NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "LocalizationContents" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_LocalizationContents" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Key" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "ChineseText" TEXT NOT NULL,
                "EnglishText" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_LocalizationContents_Key" ON "LocalizationContents" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "BillOfMaterials" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_BillOfMaterials" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "BomNo" TEXT NOT NULL,
                "FinishedItemId" TEXT NOT NULL,
                "FinishedItemCode" TEXT NOT NULL,
                "FinishedItemName" TEXT NOT NULL,
                "Version" TEXT NOT NULL,
                "BaseQuantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                CONSTRAINT "FK_BillOfMaterials_Items_FinishedItemId" FOREIGN KEY ("FinishedItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BillOfMaterials_BomNo" ON "BillOfMaterials" ("BomNo");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BillOfMaterials_FinishedItemId_Version" ON "BillOfMaterials" ("FinishedItemId", "Version");""",
            """
            CREATE TABLE IF NOT EXISTS "BillOfMaterialLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_BillOfMaterialLine" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "BillOfMaterialId" TEXT NOT NULL,
                "ComponentItemId" TEXT NOT NULL,
                "ComponentItemCode" TEXT NOT NULL,
                "ComponentItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                CONSTRAINT "FK_BillOfMaterialLine_BillOfMaterials_BillOfMaterialId" FOREIGN KEY ("BillOfMaterialId") REFERENCES "BillOfMaterials" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_BillOfMaterialLine_Items_ComponentItemId" FOREIGN KEY ("ComponentItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_BillOfMaterialLine_BillOfMaterialId" ON "BillOfMaterialLine" ("BillOfMaterialId");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkOrders" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WorkOrders" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WorkOrderNo" TEXT NOT NULL,
                "BomId" TEXT NOT NULL,
                "BomNo" TEXT NOT NULL,
                "BomVersion" TEXT NOT NULL,
                "FinishedItemId" TEXT NOT NULL,
                "FinishedItemCode" TEXT NOT NULL,
                "FinishedItemName" TEXT NOT NULL,
                "PlannedQuantity" TEXT NOT NULL,
                "CompletedQuantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "ReleasedAtUtc" TEXT NULL,
                "ClosedAtUtc" TEXT NULL,
                CONSTRAINT "FK_WorkOrders_BillOfMaterials_BomId" FOREIGN KEY ("BomId") REFERENCES "BillOfMaterials" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_WorkOrders_Items_FinishedItemId" FOREIGN KEY ("FinishedItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkOrders_WorkOrderNo" ON "WorkOrders" ("WorkOrderNo");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkOrderMaterialLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WorkOrderMaterialLine" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WorkOrderId" TEXT NOT NULL,
                "ComponentItemId" TEXT NOT NULL,
                "ComponentItemCode" TEXT NOT NULL,
                "ComponentItemName" TEXT NOT NULL,
                "RequiredQuantity" TEXT NOT NULL,
                "IssuedQuantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                CONSTRAINT "FK_WorkOrderMaterialLine_WorkOrders_WorkOrderId" FOREIGN KEY ("WorkOrderId") REFERENCES "WorkOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_WorkOrderMaterialLine_Items_ComponentItemId" FOREIGN KEY ("ComponentItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_WorkOrderMaterialLine_WorkOrderId" ON "WorkOrderMaterialLine" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "ProductionIssues" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ProductionIssues" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "IssueNo" TEXT NOT NULL,
                "WorkOrderId" TEXT NOT NULL,
                "WorkOrderNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "IssuedBy" TEXT NOT NULL,
                CONSTRAINT "FK_ProductionIssues_WorkOrders_WorkOrderId" FOREIGN KEY ("WorkOrderId") REFERENCES "WorkOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_ProductionIssues_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductionIssues_IssueNo" ON "ProductionIssues" ("IssueNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ProductionIssues_WorkOrderId" ON "ProductionIssues" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "ProductionIssueLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ProductionIssueLine" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ProductionIssueId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "CostAmount" TEXT NOT NULL DEFAULT '0',
                CONSTRAINT "FK_ProductionIssueLine_ProductionIssues_ProductionIssueId" FOREIGN KEY ("ProductionIssueId") REFERENCES "ProductionIssues" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_ProductionIssueLine_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_ProductionIssueLine_ProductionIssueId" ON "ProductionIssueLine" ("ProductionIssueId");""",
            """
            CREATE TABLE IF NOT EXISTS "ProductionReceipts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ProductionReceipts" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ReceiptNo" TEXT NOT NULL,
                "WorkOrderId" TEXT NOT NULL,
                "WorkOrderNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "FinishedItemId" TEXT NOT NULL,
                "FinishedItemCode" TEXT NOT NULL,
                "FinishedItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "UnitCost" TEXT NOT NULL DEFAULT '0',
                "MaterialCost" TEXT NOT NULL DEFAULT '0',
                "LaborCost" TEXT NOT NULL DEFAULT '0',
                "MachineCost" TEXT NOT NULL DEFAULT '0',
                "OverheadCost" TEXT NOT NULL DEFAULT '0',
                "CostAmount" TEXT NOT NULL DEFAULT '0',
                "Status" TEXT NOT NULL,
                "ReceivedBy" TEXT NOT NULL,
                CONSTRAINT "FK_ProductionReceipts_WorkOrders_WorkOrderId" FOREIGN KEY ("WorkOrderId") REFERENCES "WorkOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_ProductionReceipts_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_ProductionReceipts_Items_FinishedItemId" FOREIGN KEY ("FinishedItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductionReceipts_ReceiptNo" ON "ProductionReceipts" ("ReceiptNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ProductionReceipts_WorkOrderId" ON "ProductionReceipts" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "QualityInspections" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_QualityInspections" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "InspectionNo" TEXT NOT NULL,
                "SourceDocumentType" TEXT NOT NULL,
                "SourceDocumentId" TEXT NOT NULL,
                "SourceDocumentNo" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "InspectedQuantity" TEXT NOT NULL,
                "AcceptedQuantity" TEXT NOT NULL,
                "RejectedQuantity" TEXT NOT NULL,
                "Result" TEXT NOT NULL,
                "Disposition" TEXT NOT NULL,
                "Inspector" TEXT NOT NULL,
                "Note" TEXT NOT NULL,
                CONSTRAINT "FK_QualityInspections_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_QualityInspections_InspectionNo" ON "QualityInspections" ("InspectionNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_QualityInspections_SourceDocumentType_SourceDocumentId" ON "QualityInspections" ("SourceDocumentType", "SourceDocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "LotTraceEvents" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_LotTraceEvents" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "LotNo" TEXT NOT NULL,
                "EventType" TEXT NOT NULL,
                "SourceDocumentType" TEXT NOT NULL,
                "SourceDocumentId" TEXT NOT NULL,
                "SourceDocumentNo" TEXT NOT NULL,
                "TargetDocumentType" TEXT NOT NULL,
                "TargetDocumentId" TEXT NULL,
                "TargetDocumentNo" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "Actor" TEXT NOT NULL,
                "Note" TEXT NOT NULL,
                CONSTRAINT "FK_LotTraceEvents_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_LotTraceEvents_LotNo" ON "LotTraceEvents" ("LotNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_LotTraceEvents_SourceDocumentType_SourceDocumentId" ON "LotTraceEvents" ("SourceDocumentType", "SourceDocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "PlanningSuggestions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PlanningSuggestions" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "SuggestionNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "CurrentQuantity" TEXT NOT NULL,
                "MinimumQuantity" TEXT NOT NULL,
                "SuggestedQuantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "DecisionNote" TEXT NOT NULL,
                CONSTRAINT "FK_PlanningSuggestions_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_PlanningSuggestions_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PlanningSuggestions_SuggestionNo" ON "PlanningSuggestions" ("SuggestionNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PlanningSuggestions_WarehouseId_ItemId_Status" ON "PlanningSuggestions" ("WarehouseId", "ItemId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "OutsourcingOrders" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_OutsourcingOrders" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "OrderNo" TEXT NOT NULL,
                "SupplierName" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "FinishedItemId" TEXT NOT NULL,
                "FinishedItemCode" TEXT NOT NULL,
                "FinishedItemName" TEXT NOT NULL,
                "PlannedQuantity" TEXT NOT NULL,
                "ReceivedQuantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                CONSTRAINT "FK_OutsourcingOrders_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_OutsourcingOrders_Items_FinishedItemId" FOREIGN KEY ("FinishedItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_OutsourcingOrders_OrderNo" ON "OutsourcingOrders" ("OrderNo");""",
            """
            CREATE TABLE IF NOT EXISTS "OutsourcingOrderLine" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_OutsourcingOrderLine" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "OutsourcingOrderId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                CONSTRAINT "FK_OutsourcingOrderLine_OutsourcingOrders_OutsourcingOrderId" FOREIGN KEY ("OutsourcingOrderId") REFERENCES "OutsourcingOrders" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_OutsourcingOrderLine_Items_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Items" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_OutsourcingOrderLine_OutsourcingOrderId" ON "OutsourcingOrderLine" ("OutsourcingOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "BarcodeExecutions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_BarcodeExecutions" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ExecutionNo" TEXT NOT NULL,
                "Barcode" TEXT NOT NULL,
                "Action" TEXT NOT NULL,
                "Result" TEXT NOT NULL,
                "Message" TEXT NOT NULL,
                "DocumentType" TEXT NOT NULL,
                "DocumentId" TEXT NULL,
                "DocumentNo" TEXT NOT NULL,
                "Actor" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BarcodeExecutions_ExecutionNo" ON "BarcodeExecutions" ("ExecutionNo");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }

        await EnsureSqliteColumnAsync(dbContext, "Customers", "OrganizationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Customers", "OrganizationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Customers", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Customers", "TaxpayerId", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Customers", "InvoiceTitle", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Suppliers", "OrganizationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Suppliers", "OrganizationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Suppliers", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Suppliers", "TaxpayerId", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Suppliers", "InvoiceTitle", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Warehouses", "OrganizationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Warehouses", "OrganizationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryReceipts", "LocationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryReceipts", "LocationCode", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryReceipts", "LocationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryIssues", "LocationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryIssues", "LocationCode", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryIssues", "LocationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransfers", "FromLocationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransfers", "FromLocationCode", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransfers", "FromLocationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransfers", "ToLocationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransfers", "ToLocationCode", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransfers", "ToLocationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryCountAdjustments", "LocationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryCountAdjustments", "LocationCode", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryCountAdjustments", "LocationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryMovements", "LocationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryMovements", "LocationCode", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryMovements", "LocationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryMovements", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryMovements", "CostAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryMovements", "BalanceCostAfter", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryReceiptLine", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryReceiptLine", "CostAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryIssueLine", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryIssueLine", "CostAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransferLine", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryTransferLine", "CostAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryCountAdjustmentLine", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "InventoryCountAdjustmentLine", "CostAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "StockBalances", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "StockBalances", "InventoryValue", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "LocationStockBalances", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "LocationStockBalances", "InventoryValue", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionIssueLine", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionIssueLine", "CostAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionReceipts", "UnitCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionReceipts", "MaterialCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionReceipts", "LaborCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionReceipts", "MachineCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionReceipts", "OverheadCost", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProductionReceipts", "CostAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProcurementRequests", "OrganizationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProcurementRequests", "OrganizationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProcurementRequests", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProcurementRequests", "TaxInvoiceType", "TEXT NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "ProcurementRequests", "TaxRate", "TEXT NOT NULL DEFAULT '0.13'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesQuotations", "OrganizationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesQuotations", "OrganizationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesQuotations", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesQuotations", "TaxInvoiceType", "TEXT NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesQuotations", "TaxRate", "TEXT NOT NULL DEFAULT '0.13'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesOrders", "OrganizationId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesOrders", "OrganizationName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesOrders", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesOrders", "TaxInvoiceType", "TEXT NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "SalesOrders", "TaxRate", "TEXT NOT NULL DEFAULT '0.13'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "GeneralLedgerVouchers", "SourceType", "TEXT NOT NULL DEFAULT 'Manual'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "GeneralLedgerVouchers", "SourceId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "GeneralLedgerVouchers", "SourceNo", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Payables", "DueDate", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Receivables", "DueDate", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Payables", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Payables", "NetAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Payables", "TaxAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Payables", "TaxRate", "TEXT NOT NULL DEFAULT '0.13'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Payables", "TaxInvoiceType", "TEXT NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Receivables", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Receivables", "NetAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Receivables", "TaxAmount", "TEXT NOT NULL DEFAULT '0'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Receivables", "TaxRate", "TEXT NOT NULL DEFAULT '0.13'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Receivables", "TaxInvoiceType", "TEXT NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "CurrencyCode", "TEXT NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "BankAccountId", "TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "BankAccountNo", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "BankAccountName", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "ReconciliationStatus", "TEXT NOT NULL DEFAULT 'Unmatched'", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "BankStatementLineId", "TEXT NULL", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "BankStatementNo", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "ReconciledBy", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureSqliteColumnAsync(dbContext, "Settlements", "ReconciledAtUtc", "TEXT NULL", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "FinanceInvoices" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_FinanceInvoices" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "InvoiceNo" TEXT NOT NULL,
                "Direction" TEXT NOT NULL,
                "SourceId" TEXT NOT NULL,
                "SourceNo" TEXT NOT NULL,
                "CounterpartyName" TEXT NOT NULL,
                "TaxInvoiceType" TEXT NOT NULL,
                "TaxRate" TEXT NOT NULL,
                "GrossAmount" TEXT NOT NULL,
                "NetAmount" TEXT NOT NULL,
                "TaxAmount" TEXT NOT NULL,
                "CurrencyCode" TEXT NOT NULL,
                "InvoiceDate" TEXT NOT NULL,
                "Note" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL
            );
            """,
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_InvoiceNo" ON "FinanceInvoices" ("InvoiceNo");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_Direction_SourceId" ON "FinanceInvoices" ("Direction", "SourceId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "BankAccounts" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_BankAccounts" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "AccountNo" TEXT NOT NULL,
                "AccountName" TEXT NOT NULL,
                "BankName" TEXT NOT NULL,
                "CurrencyCode" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankAccounts_AccountNo" ON "BankAccounts" ("AccountNo");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "BankStatementLines" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_BankStatementLines" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "StatementNo" TEXT NOT NULL,
                "BankAccountId" TEXT NOT NULL,
                "BankAccountNo" TEXT NOT NULL,
                "BankAccountName" TEXT NOT NULL,
                "TransactionDate" TEXT NOT NULL,
                "Direction" TEXT NOT NULL,
                "Amount" TEXT NOT NULL,
                "CurrencyCode" TEXT NOT NULL,
                "CounterpartyName" TEXT NOT NULL,
                "BankReferenceNo" TEXT NOT NULL,
                "Summary" TEXT NOT NULL,
                "ReconciliationStatus" TEXT NOT NULL,
                "SettlementId" TEXT NULL,
                "SettlementNo" TEXT NOT NULL,
                "ReconciledBy" TEXT NOT NULL,
                "ReconciledAtUtc" TEXT NULL,
                "CreatedBy" TEXT NOT NULL
            );
            """,
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_StatementNo" ON "BankStatementLines" ("StatementNo");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_BankAccountId" ON "BankStatementLines" ("BankAccountId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_ReconciliationStatus_TransactionDate" ON "BankStatementLines" ("ReconciliationStatus", "TransactionDate");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_SettlementId" ON "BankStatementLines" ("SettlementId") WHERE "SettlementId" IS NOT NULL;""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_GeneralLedgerVouchers_SourceType_SourceId" ON "GeneralLedgerVouchers" ("SourceType", "SourceId") WHERE "SourceId" IS NOT NULL;""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Payables_Status_DueDate" ON "Payables" ("Status", "DueDate");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Receivables_Status_DueDate" ON "Receivables" ("Status", "DueDate");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_BankAccountId" ON "Settlements" ("BankAccountId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Settlements_BankStatementLineId" ON "Settlements" ("BankStatementLineId") WHERE "BankStatementLineId" IS NOT NULL;""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_ReconciliationStatus" ON "Settlements" ("ReconciliationStatus");""",
            cancellationToken);
    }

    /// <summary>
    /// Ensure Postgres Extension Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresExtensionSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "Customers" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "ContactName" text NOT NULL,
                "Phone" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Customers_Code" ON "Customers" ("Code");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesQuotations" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "QuotationNo" text NOT NULL,
                "CustomerId" uuid NOT NULL REFERENCES "Customers" ("Id") ON DELETE CASCADE,
                "CustomerName" text NOT NULL,
                "Title" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "SalesOrderId" uuid NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_SalesQuotations_QuotationNo" ON "SalesQuotations" ("QuotationNo");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesQuotationLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "SalesQuotationId" uuid NOT NULL REFERENCES "SalesQuotations" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_SalesQuotationLine_SalesQuotationId" ON "SalesQuotationLine" ("SalesQuotationId");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesOrders" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "OrderNo" text NOT NULL,
                "QuotationId" uuid NOT NULL REFERENCES "SalesQuotations" ("Id") ON DELETE CASCADE,
                "QuotationNo" text NOT NULL,
                "CustomerId" uuid NOT NULL REFERENCES "Customers" ("Id") ON DELETE CASCADE,
                "CustomerName" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_SalesOrders_OrderNo" ON "SalesOrders" ("OrderNo");""",
            """
            CREATE TABLE IF NOT EXISTS "SalesOrderLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "SalesOrderId" uuid NOT NULL REFERENCES "SalesOrders" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_SalesOrderLine_SalesOrderId" ON "SalesOrderLine" ("SalesOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryReceipts" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ReceiptNo" text NOT NULL,
                "ProcurementOrderId" uuid NOT NULL REFERENCES "ProcurementOrders" ("Id") ON DELETE CASCADE,
                "ProcurementOrderNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "SupplierName" text NOT NULL,
                "Status" text NOT NULL,
                "ReceivedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryReceipts_ReceiptNo" ON "InventoryReceipts" ("ReceiptNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryReceiptLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "InventoryReceiptId" uuid NOT NULL REFERENCES "InventoryReceipts" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "CostAmount" numeric NOT NULL DEFAULT 0
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryReceiptLine_InventoryReceiptId" ON "InventoryReceiptLine" ("InventoryReceiptId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryIssues" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "IssueNo" text NOT NULL,
                "SalesOrderId" uuid NOT NULL REFERENCES "SalesOrders" ("Id") ON DELETE CASCADE,
                "SalesOrderNo" text NOT NULL,
                "QuotationNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "CustomerName" text NOT NULL,
                "Status" text NOT NULL,
                "IssuedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryIssues_IssueNo" ON "InventoryIssues" ("IssueNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryIssueLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "InventoryIssueId" uuid NOT NULL REFERENCES "InventoryIssues" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "CostAmount" numeric NOT NULL DEFAULT 0
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryIssueLine_InventoryIssueId" ON "InventoryIssueLine" ("InventoryIssueId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryTransfers" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TransferNo" text NOT NULL,
                "FromWarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "FromWarehouseCode" text NOT NULL,
                "FromWarehouseName" text NOT NULL,
                "ToWarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "ToWarehouseCode" text NOT NULL,
                "ToWarehouseName" text NOT NULL,
                "Reason" text NOT NULL,
                "Status" text NOT NULL,
                "ExecutedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryTransfers_TransferNo" ON "InventoryTransfers" ("TransferNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryTransferLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "InventoryTransferId" uuid NOT NULL REFERENCES "InventoryTransfers" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "CostAmount" numeric NOT NULL DEFAULT 0
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryTransferLine_InventoryTransferId" ON "InventoryTransferLine" ("InventoryTransferId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryCountAdjustments" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "CountNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "Reason" text NOT NULL,
                "Status" text NOT NULL,
                "CountedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryCountAdjustments_CountNo" ON "InventoryCountAdjustments" ("CountNo");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryCountAdjustmentLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "InventoryCountAdjustmentId" uuid NOT NULL REFERENCES "InventoryCountAdjustments" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "BeforeQuantity" numeric NOT NULL,
                "CountedQuantity" numeric NOT NULL,
                "DeltaQuantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "CostAmount" numeric NOT NULL DEFAULT 0
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryCountAdjustmentLine_InventoryCountAdjustmentId" ON "InventoryCountAdjustmentLine" ("InventoryCountAdjustmentId");""",
            """
            CREATE TABLE IF NOT EXISTS "InventoryMovements" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "DocumentType" text NOT NULL,
                "DocumentNo" text NOT NULL,
                "MovementType" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "LocationId" uuid NULL,
                "LocationCode" text NOT NULL DEFAULT '',
                "LocationName" text NOT NULL DEFAULT '',
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "ChangeQuantity" numeric NOT NULL,
                "BalanceAfter" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "CostAmount" numeric NOT NULL DEFAULT 0,
                "BalanceCostAfter" numeric NOT NULL DEFAULT 0,
                "Actor" text NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_InventoryMovements_DocumentNo" ON "InventoryMovements" ("DocumentNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_InventoryMovements_WarehouseId_ItemId" ON "InventoryMovements" ("WarehouseId", "ItemId");""",
            """
            CREATE TABLE IF NOT EXISTS "StockBalances" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "InventoryValue" numeric NOT NULL DEFAULT 0
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_StockBalances_WarehouseId_ItemId" ON "StockBalances" ("WarehouseId", "ItemId");""",
            """
            CREATE TABLE IF NOT EXISTS "WarehouseLocations" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WarehouseLocations_WarehouseId_Code" ON "WarehouseLocations" ("WarehouseId", "Code");""",
            """
            CREATE TABLE IF NOT EXISTS "LocationStockBalances" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "LocationId" uuid NOT NULL REFERENCES "WarehouseLocations" ("Id") ON DELETE CASCADE,
                "LocationCode" text NOT NULL,
                "LocationName" text NOT NULL,
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "InventoryValue" numeric NOT NULL DEFAULT 0
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_LocationStockBalances_LocationId_ItemId" ON "LocationStockBalances" ("LocationId", "ItemId");""",
            """CREATE INDEX IF NOT EXISTS "IX_LocationStockBalances_WarehouseId_ItemId" ON "LocationStockBalances" ("WarehouseId", "ItemId");""",
            """
            CREATE TABLE IF NOT EXISTS "AccountingAccounts" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "Type" text NOT NULL,
                "ParentAccountId" uuid NULL,
                "IsActive" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_AccountingAccounts_Code" ON "AccountingAccounts" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_AccountingAccounts_ParentAccountId" ON "AccountingAccounts" ("ParentAccountId");""",
            """
            CREATE TABLE IF NOT EXISTS "AccountingPeriods" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Year" integer NOT NULL,
                "Month" integer NOT NULL,
                "Name" text NOT NULL,
                "StartDate" date NOT NULL,
                "EndDate" date NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "ClosedBy" text NOT NULL,
                "ClosedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_AccountingPeriods_Name" ON "AccountingPeriods" ("Name");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_AccountingPeriods_Year_Month" ON "AccountingPeriods" ("Year", "Month");""",
            """
            CREATE TABLE IF NOT EXISTS "GeneralLedgerVouchers" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "VoucherNo" text NOT NULL,
                "AccountingPeriodId" uuid NOT NULL,
                "AccountingPeriodName" text NOT NULL,
                "DocumentDate" date NOT NULL,
                "Summary" text NOT NULL,
                "SourceType" text NOT NULL,
                "SourceId" uuid NULL,
                "SourceNo" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "SubmittedBy" text NOT NULL,
                "SubmittedAtUtc" timestamp with time zone NULL,
                "ReviewedBy" text NOT NULL,
                "ReviewedAtUtc" timestamp with time zone NULL,
                "ReviewNote" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_GeneralLedgerVouchers_VoucherNo" ON "GeneralLedgerVouchers" ("VoucherNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_GeneralLedgerVouchers_AccountingPeriodId_Status" ON "GeneralLedgerVouchers" ("AccountingPeriodId", "Status");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_GeneralLedgerVouchers_SourceType_SourceId" ON "GeneralLedgerVouchers" ("SourceType", "SourceId") WHERE "SourceId" IS NOT NULL;""",
            """
            CREATE TABLE IF NOT EXISTS "GeneralLedgerVoucherLines" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "GeneralLedgerVoucherId" uuid NOT NULL REFERENCES "GeneralLedgerVouchers" ("Id") ON DELETE CASCADE,
                "AccountingAccountId" uuid NOT NULL,
                "AccountCode" text NOT NULL,
                "AccountName" text NOT NULL,
                "Summary" text NOT NULL,
                "DebitAmount" numeric NOT NULL,
                "CreditAmount" numeric NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_GeneralLedgerVoucherLines_GeneralLedgerVoucherId" ON "GeneralLedgerVoucherLines" ("GeneralLedgerVoucherId");""",
            """CREATE INDEX IF NOT EXISTS "IX_GeneralLedgerVoucherLines_AccountingAccountId" ON "GeneralLedgerVoucherLines" ("AccountingAccountId");""",
            """
            CREATE TABLE IF NOT EXISTS "Payables" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "PayableNo" text NOT NULL,
                "ProcurementOrderId" uuid NOT NULL REFERENCES "ProcurementOrders" ("Id") ON DELETE CASCADE,
                "ProcurementOrderNo" text NOT NULL,
                "InventoryReceiptId" uuid NULL REFERENCES "InventoryReceipts" ("Id") ON DELETE CASCADE,
                "InventoryReceiptNo" text NOT NULL,
                "SupplierName" text NOT NULL,
                "Amount" numeric NOT NULL,
                "NetAmount" numeric NOT NULL,
                "TaxAmount" numeric NOT NULL,
                "TaxRate" numeric NOT NULL,
                "TaxInvoiceType" text NOT NULL,
                "SettledAmount" numeric NOT NULL,
                "DueDate" date NULL,
                "Status" text NOT NULL,
                "SourceType" text NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Payables_PayableNo" ON "Payables" ("PayableNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_Payables_ProcurementOrderId" ON "Payables" ("ProcurementOrderId");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Payables_InventoryReceiptId" ON "Payables" ("InventoryReceiptId") WHERE "InventoryReceiptId" IS NOT NULL;""",
            """CREATE INDEX IF NOT EXISTS "IX_Payables_Status_DueDate" ON "Payables" ("Status", "DueDate");""",
            """
            CREATE TABLE IF NOT EXISTS "Receivables" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ReceivableNo" text NOT NULL,
                "SalesOrderId" uuid NOT NULL REFERENCES "SalesOrders" ("Id") ON DELETE CASCADE,
                "SalesOrderNo" text NOT NULL,
                "InventoryIssueId" uuid NULL REFERENCES "InventoryIssues" ("Id") ON DELETE CASCADE,
                "InventoryIssueNo" text NOT NULL,
                "CustomerName" text NOT NULL,
                "Amount" numeric NOT NULL,
                "NetAmount" numeric NOT NULL,
                "TaxAmount" numeric NOT NULL,
                "TaxRate" numeric NOT NULL,
                "TaxInvoiceType" text NOT NULL,
                "SettledAmount" numeric NOT NULL,
                "DueDate" date NULL,
                "Status" text NOT NULL,
                "SourceType" text NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Receivables_ReceivableNo" ON "Receivables" ("ReceivableNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_Receivables_SalesOrderId" ON "Receivables" ("SalesOrderId");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Receivables_InventoryIssueId" ON "Receivables" ("InventoryIssueId") WHERE "InventoryIssueId" IS NOT NULL;""",
            """CREATE INDEX IF NOT EXISTS "IX_Receivables_Status_DueDate" ON "Receivables" ("Status", "DueDate");""",
            """
            CREATE TABLE IF NOT EXISTS "FinanceInvoices" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "InvoiceNo" text NOT NULL,
                "Direction" text NOT NULL,
                "SourceId" uuid NOT NULL,
                "SourceNo" text NOT NULL,
                "CounterpartyName" text NOT NULL,
                "TaxInvoiceType" text NOT NULL,
                "TaxRate" numeric NOT NULL,
                "GrossAmount" numeric NOT NULL,
                "NetAmount" numeric NOT NULL,
                "TaxAmount" numeric NOT NULL,
                "CurrencyCode" text NOT NULL,
                "InvoiceDate" date NOT NULL,
                "Note" text NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_InvoiceNo" ON "FinanceInvoices" ("InvoiceNo");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_Direction_SourceId" ON "FinanceInvoices" ("Direction", "SourceId");""",
            """
            CREATE TABLE IF NOT EXISTS "BankAccounts" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "AccountNo" text NOT NULL,
                "AccountName" text NOT NULL,
                "BankName" text NOT NULL,
                "CurrencyCode" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankAccounts_AccountNo" ON "BankAccounts" ("AccountNo");""",
            """
            CREATE TABLE IF NOT EXISTS "BankStatementLines" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "StatementNo" text NOT NULL,
                "BankAccountId" uuid NOT NULL,
                "BankAccountNo" text NOT NULL,
                "BankAccountName" text NOT NULL,
                "TransactionDate" date NOT NULL,
                "Direction" text NOT NULL,
                "Amount" numeric NOT NULL,
                "CurrencyCode" text NOT NULL,
                "CounterpartyName" text NOT NULL,
                "BankReferenceNo" text NOT NULL,
                "Summary" text NOT NULL,
                "ReconciliationStatus" text NOT NULL,
                "SettlementId" uuid NULL,
                "SettlementNo" text NOT NULL,
                "ReconciledBy" text NOT NULL,
                "ReconciledAtUtc" timestamp with time zone NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_StatementNo" ON "BankStatementLines" ("StatementNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_BankAccountId" ON "BankStatementLines" ("BankAccountId");""",
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_ReconciliationStatus_TransactionDate" ON "BankStatementLines" ("ReconciliationStatus", "TransactionDate");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_SettlementId" ON "BankStatementLines" ("SettlementId") WHERE "SettlementId" IS NOT NULL;""",
            """
            CREATE TABLE IF NOT EXISTS "Settlements" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "SettlementNo" text NOT NULL,
                "TargetType" text NOT NULL,
                "TargetId" uuid NOT NULL,
                "TargetNo" text NOT NULL,
                "CounterpartyName" text NOT NULL,
                "Amount" numeric NOT NULL,
                "CurrencyCode" text NOT NULL,
                "BankAccountId" uuid NOT NULL,
                "BankAccountNo" text NOT NULL,
                "BankAccountName" text NOT NULL,
                "Method" text NOT NULL,
                "Note" text NOT NULL,
                "ReconciliationStatus" text NOT NULL,
                "BankStatementLineId" uuid NULL,
                "BankStatementNo" text NOT NULL,
                "ReconciledBy" text NOT NULL,
                "ReconciledAtUtc" timestamp with time zone NULL,
                "SettledBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Settlements_SettlementNo" ON "Settlements" ("SettlementNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_TargetType_TargetId" ON "Settlements" ("TargetType", "TargetId");""",
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_BankAccountId" ON "Settlements" ("BankAccountId");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Settlements_BankStatementLineId" ON "Settlements" ("BankStatementLineId") WHERE "BankStatementLineId" IS NOT NULL;""",
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_ReconciliationStatus" ON "Settlements" ("ReconciliationStatus");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkflowDefinitions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Key" text NOT NULL,
                "DisplayName" text NOT NULL,
                "ModuleKey" text NOT NULL,
                "DocumentType" text NOT NULL,
                "RequiredPermission" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkflowDefinitions_Key" ON "WorkflowDefinitions" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkflowInstances" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "DefinitionId" uuid NOT NULL,
                "DefinitionKey" text NOT NULL,
                "DefinitionName" text NOT NULL,
                "DocumentType" text NOT NULL,
                "DocumentId" uuid NOT NULL,
                "DocumentNo" text NOT NULL,
                "Title" text NOT NULL,
                "Status" text NOT NULL,
                "SubmittedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_WorkflowInstances_DefinitionKey_DocumentId" ON "WorkflowInstances" ("DefinitionKey", "DocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "ApprovalTasks" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WorkflowInstanceId" uuid NOT NULL,
                "DefinitionKey" text NOT NULL,
                "DefinitionName" text NOT NULL,
                "DocumentType" text NOT NULL,
                "DocumentId" uuid NOT NULL,
                "DocumentNo" text NOT NULL,
                "Title" text NOT NULL,
                "Status" text NOT NULL,
                "SubmittedBy" text NOT NULL,
                "RequiredPermission" text NOT NULL,
                "DecidedBy" text NULL,
                "Decision" text NULL,
                "Comment" text NULL,
                "DecidedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_ApprovalTasks_WorkflowInstanceId_Status" ON "ApprovalTasks" ("WorkflowInstanceId", "Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_ApprovalTasks_DocumentId" ON "ApprovalTasks" ("DocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "Notifications" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Title" text NOT NULL,
                "Message" text NOT NULL,
                "Category" text NOT NULL,
                "RelatedDocumentType" text NOT NULL,
                "RelatedDocumentId" uuid NOT NULL,
                "RelatedDocumentNo" text NOT NULL,
                "RecipientPermission" text NOT NULL,
                "Status" text NOT NULL,
                "ReadAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_Notifications_Status" ON "Notifications" ("Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_Notifications_RelatedDocumentId" ON "Notifications" ("RelatedDocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "DataScopeRules" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "RoleKey" text NOT NULL,
                "ScopeType" text NOT NULL,
                "MatchValue" text NOT NULL,
                "Description" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_DataScopeRules_RoleKey_ScopeType" ON "DataScopeRules" ("RoleKey", "ScopeType");""",
            """
            CREATE TABLE IF NOT EXISTS "NumberingRules" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "DocumentType" text NOT NULL,
                "Prefix" text NOT NULL,
                "UseDateSegment" boolean NOT NULL,
                "NextSequence" integer NOT NULL,
                "Padding" integer NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_NumberingRules_DocumentType" ON "NumberingRules" ("DocumentType");""",
            """
            CREATE TABLE IF NOT EXISTS "Currencies" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "Symbol" text NOT NULL,
                "ExchangeRateToBase" numeric NOT NULL,
                "IsBase" boolean NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Currencies_Code" ON "Currencies" ("Code");""",
            """
            CREATE TABLE IF NOT EXISTS "LocalizationSettings" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "DefaultCurrencyCode" text NOT NULL,
                "TaxInvoiceType" text NOT NULL,
                "TaxpayerId" text NOT NULL,
                "InvoiceTitle" text NOT NULL,
                "DefaultTaxRate" numeric NOT NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "LocalizationContents" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Key" text NOT NULL,
                "Category" text NOT NULL,
                "ChineseText" text NOT NULL,
                "EnglishText" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_LocalizationContents_Key" ON "LocalizationContents" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "BillOfMaterials" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "BomNo" text NOT NULL,
                "FinishedItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "FinishedItemCode" text NOT NULL,
                "FinishedItemName" text NOT NULL,
                "Version" text NOT NULL,
                "BaseQuantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BillOfMaterials_BomNo" ON "BillOfMaterials" ("BomNo");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BillOfMaterials_FinishedItemId_Version" ON "BillOfMaterials" ("FinishedItemId", "Version");""",
            """
            CREATE TABLE IF NOT EXISTS "BillOfMaterialLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "BillOfMaterialId" uuid NOT NULL REFERENCES "BillOfMaterials" ("Id") ON DELETE CASCADE,
                "ComponentItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ComponentItemCode" text NOT NULL,
                "ComponentItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_BillOfMaterialLine_BillOfMaterialId" ON "BillOfMaterialLine" ("BillOfMaterialId");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkOrders" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WorkOrderNo" text NOT NULL,
                "BomId" uuid NOT NULL REFERENCES "BillOfMaterials" ("Id") ON DELETE CASCADE,
                "BomNo" text NOT NULL,
                "BomVersion" text NOT NULL,
                "FinishedItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "FinishedItemCode" text NOT NULL,
                "FinishedItemName" text NOT NULL,
                "PlannedQuantity" numeric NOT NULL,
                "CompletedQuantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "ReleasedAtUtc" timestamp with time zone NULL,
                "ClosedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkOrders_WorkOrderNo" ON "WorkOrders" ("WorkOrderNo");""",
            """
            CREATE TABLE IF NOT EXISTS "WorkOrderMaterialLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WorkOrderId" uuid NOT NULL REFERENCES "WorkOrders" ("Id") ON DELETE CASCADE,
                "ComponentItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ComponentItemCode" text NOT NULL,
                "ComponentItemName" text NOT NULL,
                "RequiredQuantity" numeric NOT NULL,
                "IssuedQuantity" numeric NOT NULL,
                "Unit" text NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_WorkOrderMaterialLine_WorkOrderId" ON "WorkOrderMaterialLine" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "ProductionIssues" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "IssueNo" text NOT NULL,
                "WorkOrderId" uuid NOT NULL REFERENCES "WorkOrders" ("Id") ON DELETE CASCADE,
                "WorkOrderNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "Status" text NOT NULL,
                "IssuedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductionIssues_IssueNo" ON "ProductionIssues" ("IssueNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ProductionIssues_WorkOrderId" ON "ProductionIssues" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "ProductionIssueLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ProductionIssueId" uuid NOT NULL REFERENCES "ProductionIssues" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "CostAmount" numeric NOT NULL DEFAULT 0
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_ProductionIssueLine_ProductionIssueId" ON "ProductionIssueLine" ("ProductionIssueId");""",
            """
            CREATE TABLE IF NOT EXISTS "ProductionReceipts" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ReceiptNo" text NOT NULL,
                "WorkOrderId" uuid NOT NULL REFERENCES "WorkOrders" ("Id") ON DELETE CASCADE,
                "WorkOrderNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "FinishedItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "FinishedItemCode" text NOT NULL,
                "FinishedItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "UnitCost" numeric NOT NULL DEFAULT 0,
                "MaterialCost" numeric NOT NULL DEFAULT 0,
                "LaborCost" numeric NOT NULL DEFAULT 0,
                "MachineCost" numeric NOT NULL DEFAULT 0,
                "OverheadCost" numeric NOT NULL DEFAULT 0,
                "CostAmount" numeric NOT NULL DEFAULT 0,
                "Status" text NOT NULL,
                "ReceivedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductionReceipts_ReceiptNo" ON "ProductionReceipts" ("ReceiptNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ProductionReceipts_WorkOrderId" ON "ProductionReceipts" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "QualityInspections" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "InspectionNo" text NOT NULL,
                "SourceDocumentType" text NOT NULL,
                "SourceDocumentId" uuid NOT NULL,
                "SourceDocumentNo" text NOT NULL,
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "InspectedQuantity" numeric NOT NULL,
                "AcceptedQuantity" numeric NOT NULL,
                "RejectedQuantity" numeric NOT NULL,
                "Result" text NOT NULL,
                "Disposition" text NOT NULL,
                "Inspector" text NOT NULL,
                "Note" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_QualityInspections_InspectionNo" ON "QualityInspections" ("InspectionNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_QualityInspections_SourceDocumentType_SourceDocumentId" ON "QualityInspections" ("SourceDocumentType", "SourceDocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "LotTraceEvents" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "LotNo" text NOT NULL,
                "EventType" text NOT NULL,
                "SourceDocumentType" text NOT NULL,
                "SourceDocumentId" uuid NOT NULL,
                "SourceDocumentNo" text NOT NULL,
                "TargetDocumentType" text NOT NULL,
                "TargetDocumentId" uuid NULL,
                "TargetDocumentNo" text NOT NULL,
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "Actor" text NOT NULL,
                "Note" text NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_LotTraceEvents_LotNo" ON "LotTraceEvents" ("LotNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_LotTraceEvents_SourceDocumentType_SourceDocumentId" ON "LotTraceEvents" ("SourceDocumentType", "SourceDocumentId");""",
            """
            CREATE TABLE IF NOT EXISTS "PlanningSuggestions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "SuggestionNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "CurrentQuantity" numeric NOT NULL,
                "MinimumQuantity" numeric NOT NULL,
                "SuggestedQuantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "DecisionNote" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PlanningSuggestions_SuggestionNo" ON "PlanningSuggestions" ("SuggestionNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PlanningSuggestions_WarehouseId_ItemId_Status" ON "PlanningSuggestions" ("WarehouseId", "ItemId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "OutsourcingOrders" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "OrderNo" text NOT NULL,
                "SupplierName" text NOT NULL,
                "WarehouseId" uuid NOT NULL REFERENCES "Warehouses" ("Id") ON DELETE CASCADE,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "FinishedItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "FinishedItemCode" text NOT NULL,
                "FinishedItemName" text NOT NULL,
                "PlannedQuantity" numeric NOT NULL,
                "ReceivedQuantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_OutsourcingOrders_OrderNo" ON "OutsourcingOrders" ("OrderNo");""",
            """
            CREATE TABLE IF NOT EXISTS "OutsourcingOrderLine" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "OutsourcingOrderId" uuid NOT NULL REFERENCES "OutsourcingOrders" ("Id") ON DELETE CASCADE,
                "ItemId" uuid NOT NULL REFERENCES "Items" ("Id") ON DELETE CASCADE,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_OutsourcingOrderLine_OutsourcingOrderId" ON "OutsourcingOrderLine" ("OutsourcingOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "BarcodeExecutions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ExecutionNo" text NOT NULL,
                "Barcode" text NOT NULL,
                "Action" text NOT NULL,
                "Result" text NOT NULL,
                "Message" text NOT NULL,
                "DocumentType" text NOT NULL,
                "DocumentId" uuid NULL,
                "DocumentNo" text NOT NULL,
                "Actor" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BarcodeExecutions_ExecutionNo" ON "BarcodeExecutions" ("ExecutionNo");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }

        await EnsurePostgresColumnAsync(dbContext, "Customers", "OrganizationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Customers", "OrganizationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Customers", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Customers", "TaxpayerId", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Customers", "InvoiceTitle", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Suppliers", "OrganizationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Suppliers", "OrganizationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Suppliers", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Suppliers", "TaxpayerId", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Suppliers", "InvoiceTitle", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Warehouses", "OrganizationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Warehouses", "OrganizationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryReceipts", "LocationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryReceipts", "LocationCode", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryReceipts", "LocationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryIssues", "LocationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryIssues", "LocationCode", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryIssues", "LocationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransfers", "FromLocationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransfers", "FromLocationCode", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransfers", "FromLocationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransfers", "ToLocationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransfers", "ToLocationCode", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransfers", "ToLocationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryCountAdjustments", "LocationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryCountAdjustments", "LocationCode", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryCountAdjustments", "LocationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryMovements", "LocationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryMovements", "LocationCode", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryMovements", "LocationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryMovements", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryMovements", "CostAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryMovements", "BalanceCostAfter", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryReceiptLine", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryReceiptLine", "CostAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryIssueLine", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryIssueLine", "CostAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransferLine", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryTransferLine", "CostAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryCountAdjustmentLine", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "InventoryCountAdjustmentLine", "CostAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "StockBalances", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "StockBalances", "InventoryValue", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "LocationStockBalances", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "LocationStockBalances", "InventoryValue", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionIssueLine", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionIssueLine", "CostAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionReceipts", "UnitCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionReceipts", "MaterialCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionReceipts", "LaborCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionReceipts", "MachineCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionReceipts", "OverheadCost", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProductionReceipts", "CostAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProcurementRequests", "OrganizationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProcurementRequests", "OrganizationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProcurementRequests", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProcurementRequests", "TaxInvoiceType", "text NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "ProcurementRequests", "TaxRate", "numeric NOT NULL DEFAULT 0.13", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesQuotations", "OrganizationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesQuotations", "OrganizationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesQuotations", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesQuotations", "TaxInvoiceType", "text NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesQuotations", "TaxRate", "numeric NOT NULL DEFAULT 0.13", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesOrders", "OrganizationId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesOrders", "OrganizationName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesOrders", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesOrders", "TaxInvoiceType", "text NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "SalesOrders", "TaxRate", "numeric NOT NULL DEFAULT 0.13", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "GeneralLedgerVouchers", "SourceType", "text NOT NULL DEFAULT 'Manual'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "GeneralLedgerVouchers", "SourceId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "GeneralLedgerVouchers", "SourceNo", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Payables", "DueDate", "date NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Receivables", "DueDate", "date NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Payables", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Payables", "NetAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Payables", "TaxAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Payables", "TaxRate", "numeric NOT NULL DEFAULT 0.13", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Payables", "TaxInvoiceType", "text NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Receivables", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Receivables", "NetAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Receivables", "TaxAmount", "numeric NOT NULL DEFAULT 0", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Receivables", "TaxRate", "numeric NOT NULL DEFAULT 0.13", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Receivables", "TaxInvoiceType", "text NOT NULL DEFAULT '增值税普通发票'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "CurrencyCode", "text NOT NULL DEFAULT 'CNY'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "BankAccountId", "uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "BankAccountNo", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "BankAccountName", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "ReconciliationStatus", "text NOT NULL DEFAULT 'Unmatched'", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "BankStatementLineId", "uuid NULL", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "BankStatementNo", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "ReconciledBy", "text NOT NULL DEFAULT ''", cancellationToken);
        await EnsurePostgresColumnAsync(dbContext, "Settlements", "ReconciledAtUtc", "timestamp with time zone NULL", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "FinanceInvoices" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "InvoiceNo" text NOT NULL,
                "Direction" text NOT NULL,
                "SourceId" uuid NOT NULL,
                "SourceNo" text NOT NULL,
                "CounterpartyName" text NOT NULL,
                "TaxInvoiceType" text NOT NULL,
                "TaxRate" numeric NOT NULL,
                "GrossAmount" numeric NOT NULL,
                "NetAmount" numeric NOT NULL,
                "TaxAmount" numeric NOT NULL,
                "CurrencyCode" text NOT NULL,
                "InvoiceDate" date NOT NULL,
                "Note" text NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_InvoiceNo" ON "FinanceInvoices" ("InvoiceNo");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinanceInvoices_Direction_SourceId" ON "FinanceInvoices" ("Direction", "SourceId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "BankAccounts" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "AccountNo" text NOT NULL,
                "AccountName" text NOT NULL,
                "BankName" text NOT NULL,
                "CurrencyCode" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankAccounts_AccountNo" ON "BankAccounts" ("AccountNo");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "BankStatementLines" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "StatementNo" text NOT NULL,
                "BankAccountId" uuid NOT NULL,
                "BankAccountNo" text NOT NULL,
                "BankAccountName" text NOT NULL,
                "TransactionDate" date NOT NULL,
                "Direction" text NOT NULL,
                "Amount" numeric NOT NULL,
                "CurrencyCode" text NOT NULL,
                "CounterpartyName" text NOT NULL,
                "BankReferenceNo" text NOT NULL,
                "Summary" text NOT NULL,
                "ReconciliationStatus" text NOT NULL,
                "SettlementId" uuid NULL,
                "SettlementNo" text NOT NULL,
                "ReconciledBy" text NOT NULL,
                "ReconciledAtUtc" timestamp with time zone NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_StatementNo" ON "BankStatementLines" ("StatementNo");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_BankAccountId" ON "BankStatementLines" ("BankAccountId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_BankStatementLines_ReconciliationStatus_TransactionDate" ON "BankStatementLines" ("ReconciliationStatus", "TransactionDate");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_BankStatementLines_SettlementId" ON "BankStatementLines" ("SettlementId") WHERE "SettlementId" IS NOT NULL;""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_GeneralLedgerVouchers_SourceType_SourceId" ON "GeneralLedgerVouchers" ("SourceType", "SourceId") WHERE "SourceId" IS NOT NULL;""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Payables_Status_DueDate" ON "Payables" ("Status", "DueDate");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Receivables_Status_DueDate" ON "Receivables" ("Status", "DueDate");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_BankAccountId" ON "Settlements" ("BankAccountId");""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Settlements_BankStatementLineId" ON "Settlements" ("BankStatementLineId") WHERE "BankStatementLineId" IS NOT NULL;""",
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """CREATE INDEX IF NOT EXISTS "IX_Settlements_ReconciliationStatus" ON "Settlements" ("ReconciliationStatus");""",
            cancellationToken);
    }

    /// <summary>
    /// Ensure Position Permission Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsurePositionPermissionSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqlitePositionPermissionSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresPositionPermissionSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Position Permission Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqlitePositionPermissionSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "PositionDepartments" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PositionDepartments" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "ParentDepartmentId" TEXT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PositionDepartments_Code" ON "PositionDepartments" ("Code");""",
            """
            CREATE TABLE IF NOT EXISTS "JobPositions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_JobPositions" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "DepartmentId" TEXT NOT NULL,
                "DepartmentName" TEXT NOT NULL,
                "Description" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                CONSTRAINT "FK_JobPositions_PositionDepartments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "PositionDepartments" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_JobPositions_Code" ON "JobPositions" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_JobPositions_DepartmentId" ON "JobPositions" ("DepartmentId");""",
            """
            CREATE TABLE IF NOT EXISTS "PermissionPackages" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PermissionPackages" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Key" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "Description" TEXT NOT NULL,
                "ModuleKeys" TEXT NOT NULL,
                "Permissions" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PermissionPackages_Key" ON "PermissionPackages" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "PositionRoleBindings" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PositionRoleBindings" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "PositionId" TEXT NOT NULL,
                "RoleId" TEXT NOT NULL,
                CONSTRAINT "FK_PositionRoleBindings_JobPositions_PositionId" FOREIGN KEY ("PositionId") REFERENCES "JobPositions" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_PositionRoleBindings_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PositionRoleBindings_PositionId_RoleId" ON "PositionRoleBindings" ("PositionId", "RoleId");""",
            """CREATE INDEX IF NOT EXISTS "IX_PositionRoleBindings_RoleId" ON "PositionRoleBindings" ("RoleId");""",
            """
            CREATE TABLE IF NOT EXISTS "PositionDataScopeRules" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PositionDataScopeRules" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "PositionId" TEXT NOT NULL,
                "ScopeType" TEXT NOT NULL,
                "MatchValue" TEXT NOT NULL,
                "Description" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                CONSTRAINT "FK_PositionDataScopeRules_JobPositions_PositionId" FOREIGN KEY ("PositionId") REFERENCES "JobPositions" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PositionDataScopeRules_PositionId_ScopeType" ON "PositionDataScopeRules" ("PositionId", "ScopeType");""",
            """
            CREATE TABLE IF NOT EXISTS "RolePermissionGrants" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_RolePermissionGrants" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "RoleId" TEXT NOT NULL,
                "Permission" TEXT NOT NULL,
                CONSTRAINT "FK_RolePermissionGrants_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_RolePermissionGrants_RoleId_Permission" ON "RolePermissionGrants" ("RoleId", "Permission");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Postgres Position Permission Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresPositionPermissionSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "PositionDepartments" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "ParentDepartmentId" uuid NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PositionDepartments_Code" ON "PositionDepartments" ("Code");""",
            """
            CREATE TABLE IF NOT EXISTS "JobPositions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "DepartmentId" uuid NOT NULL REFERENCES "PositionDepartments" ("Id") ON DELETE CASCADE,
                "DepartmentName" text NOT NULL,
                "Description" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_JobPositions_Code" ON "JobPositions" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_JobPositions_DepartmentId" ON "JobPositions" ("DepartmentId");""",
            """
            CREATE TABLE IF NOT EXISTS "PermissionPackages" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Key" text NOT NULL,
                "DisplayName" text NOT NULL,
                "Description" text NOT NULL,
                "ModuleKeys" text NOT NULL,
                "Permissions" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PermissionPackages_Key" ON "PermissionPackages" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "PositionRoleBindings" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "PositionId" uuid NOT NULL REFERENCES "JobPositions" ("Id") ON DELETE CASCADE,
                "RoleId" uuid NOT NULL REFERENCES "Roles" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PositionRoleBindings_PositionId_RoleId" ON "PositionRoleBindings" ("PositionId", "RoleId");""",
            """CREATE INDEX IF NOT EXISTS "IX_PositionRoleBindings_RoleId" ON "PositionRoleBindings" ("RoleId");""",
            """
            CREATE TABLE IF NOT EXISTS "PositionDataScopeRules" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "PositionId" uuid NOT NULL REFERENCES "JobPositions" ("Id") ON DELETE CASCADE,
                "ScopeType" text NOT NULL,
                "MatchValue" text NOT NULL,
                "Description" text NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PositionDataScopeRules_PositionId_ScopeType" ON "PositionDataScopeRules" ("PositionId", "ScopeType");""",
            """
            CREATE TABLE IF NOT EXISTS "RolePermissionGrants" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "RoleId" uuid NOT NULL REFERENCES "Roles" ("Id") ON DELETE CASCADE,
                "Permission" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_RolePermissionGrants_RoleId_Permission" ON "RolePermissionGrants" ("RoleId", "Permission");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Wms Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureWmsSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteWmsSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresWmsSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Wms Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteWmsSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "PutAwayTasks" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PutAwayTasks" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TaskNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "SuggestedLocationId" TEXT NULL,
                "SuggestedLocationCode" TEXT NOT NULL,
                "SuggestedLocationName" TEXT NOT NULL,
                "ContainerCode" TEXT NOT NULL,
                "SourceDocumentNo" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "AssignedTo" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "CompletedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PutAwayTasks_TaskNo" ON "PutAwayTasks" ("TaskNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PutAwayTasks_WarehouseId_Status" ON "PutAwayTasks" ("WarehouseId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "PickingTasks" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PickingTasks" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TaskNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "Quantity" TEXT NOT NULL,
                "Unit" TEXT NOT NULL,
                "SourceLocationId" TEXT NULL,
                "SourceLocationCode" TEXT NOT NULL,
                "SourceLocationName" TEXT NOT NULL,
                "WaveId" TEXT NULL,
                "WaveNo" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "AssignedTo" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "CompletedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PickingTasks_TaskNo" ON "PickingTasks" ("TaskNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PickingTasks_WarehouseId_Status" ON "PickingTasks" ("WarehouseId", "Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_PickingTasks_WaveId" ON "PickingTasks" ("WaveId");""",
            """
            CREATE TABLE IF NOT EXISTS "PickingWaves" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PickingWaves" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WaveNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "ReleasedBy" TEXT NOT NULL,
                "ReleasedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PickingWaves_WaveNo" ON "PickingWaves" ("WaveNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PickingWaves_WarehouseId_Status" ON "PickingWaves" ("WarehouseId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "WarehouseContainers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WarehouseContainers" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "ContainerType" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "CurrentLocationId" TEXT NULL,
                "CurrentLocationCode" TEXT NOT NULL,
                "CurrentLocationName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "LastHandledBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WarehouseContainers_Code" ON "WarehouseContainers" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_WarehouseContainers_WarehouseId" ON "WarehouseContainers" ("WarehouseId");""",
            """
            CREATE TABLE IF NOT EXISTS "WarehouseRoutes" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WarehouseRoutes" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "FromLocationId" TEXT NOT NULL,
                "FromLocationCode" TEXT NOT NULL,
                "FromLocationName" TEXT NOT NULL,
                "ToLocationId" TEXT NOT NULL,
                "ToLocationCode" TEXT NOT NULL,
                "ToLocationName" TEXT NOT NULL,
                "DistanceMeters" TEXT NOT NULL,
                "Priority" INTEGER NOT NULL,
                "IsEnabled" INTEGER NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WarehouseRoutes_WarehouseId_FromLocationId_ToLocationId" ON "WarehouseRoutes" ("WarehouseId", "FromLocationId", "ToLocationId");""",
            """
            CREATE TABLE IF NOT EXISTS "PdaWorkQueueItems" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PdaWorkQueueItems" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TaskType" TEXT NOT NULL,
                "TaskId" TEXT NOT NULL,
                "TaskNo" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "LocationCode" TEXT NOT NULL,
                "AssignedTo" TEXT NOT NULL,
                "Priority" INTEGER NOT NULL,
                "Status" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PdaWorkQueueItems_TaskType_TaskId" ON "PdaWorkQueueItems" ("TaskType", "TaskId");""",
            """CREATE INDEX IF NOT EXISTS "IX_PdaWorkQueueItems_WarehouseId_Status" ON "PdaWorkQueueItems" ("WarehouseId", "Status");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Postgres Wms Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresWmsSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "PutAwayTasks" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TaskNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "SuggestedLocationId" uuid NULL,
                "SuggestedLocationCode" text NOT NULL,
                "SuggestedLocationName" text NOT NULL,
                "ContainerCode" text NOT NULL,
                "SourceDocumentNo" text NOT NULL,
                "Status" text NOT NULL,
                "AssignedTo" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "CompletedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PutAwayTasks_TaskNo" ON "PutAwayTasks" ("TaskNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PutAwayTasks_WarehouseId_Status" ON "PutAwayTasks" ("WarehouseId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "PickingTasks" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TaskNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "Quantity" numeric NOT NULL,
                "Unit" text NOT NULL,
                "SourceLocationId" uuid NULL,
                "SourceLocationCode" text NOT NULL,
                "SourceLocationName" text NOT NULL,
                "WaveId" uuid NULL,
                "WaveNo" text NOT NULL,
                "Status" text NOT NULL,
                "AssignedTo" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "CompletedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PickingTasks_TaskNo" ON "PickingTasks" ("TaskNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PickingTasks_WarehouseId_Status" ON "PickingTasks" ("WarehouseId", "Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_PickingTasks_WaveId" ON "PickingTasks" ("WaveId");""",
            """
            CREATE TABLE IF NOT EXISTS "PickingWaves" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WaveNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "ReleasedBy" text NOT NULL,
                "ReleasedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PickingWaves_WaveNo" ON "PickingWaves" ("WaveNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PickingWaves_WarehouseId_Status" ON "PickingWaves" ("WarehouseId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "WarehouseContainers" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Code" text NOT NULL,
                "ContainerType" text NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "CurrentLocationId" uuid NULL,
                "CurrentLocationCode" text NOT NULL,
                "CurrentLocationName" text NOT NULL,
                "Status" text NOT NULL,
                "LastHandledBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WarehouseContainers_Code" ON "WarehouseContainers" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_WarehouseContainers_WarehouseId" ON "WarehouseContainers" ("WarehouseId");""",
            """
            CREATE TABLE IF NOT EXISTS "WarehouseRoutes" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "FromLocationId" uuid NOT NULL,
                "FromLocationCode" text NOT NULL,
                "FromLocationName" text NOT NULL,
                "ToLocationId" uuid NOT NULL,
                "ToLocationCode" text NOT NULL,
                "ToLocationName" text NOT NULL,
                "DistanceMeters" numeric NOT NULL,
                "Priority" integer NOT NULL,
                "IsEnabled" boolean NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WarehouseRoutes_WarehouseId_FromLocationId_ToLocationId" ON "WarehouseRoutes" ("WarehouseId", "FromLocationId", "ToLocationId");""",
            """
            CREATE TABLE IF NOT EXISTS "PdaWorkQueueItems" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TaskType" text NOT NULL,
                "TaskId" uuid NOT NULL,
                "TaskNo" text NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseName" text NOT NULL,
                "LocationCode" text NOT NULL,
                "AssignedTo" text NOT NULL,
                "Priority" integer NOT NULL,
                "Status" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PdaWorkQueueItems_TaskType_TaskId" ON "PdaWorkQueueItems" ("TaskType", "TaskId");""",
            """CREATE INDEX IF NOT EXISTS "IX_PdaWorkQueueItems_WarehouseId_Status" ON "PdaWorkQueueItems" ("WarehouseId", "Status");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Advanced Manufacturing Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureAdvancedManufacturingSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteAdvancedManufacturingSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresAdvancedManufacturingSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Advanced Manufacturing Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteAdvancedManufacturingSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "WorkCenters" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WorkCenters" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "CapacityMinutesPerDay" TEXT NOT NULL,
                "HourlyCostRate" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkCenters_Code" ON "WorkCenters" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_WorkCenters_WarehouseId" ON "WorkCenters" ("WarehouseId");""",
            """
            CREATE TABLE IF NOT EXISTS "ManufacturingRoutings" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ManufacturingRoutings" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "RoutingNo" TEXT NOT NULL,
                "FinishedItemId" TEXT NOT NULL,
                "FinishedItemCode" TEXT NOT NULL,
                "FinishedItemName" TEXT NOT NULL,
                "Version" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ManufacturingRoutings_RoutingNo" ON "ManufacturingRoutings" ("RoutingNo");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ManufacturingRoutings_FinishedItemId_Version" ON "ManufacturingRoutings" ("FinishedItemId", "Version");""",
            """
            CREATE TABLE IF NOT EXISTS "RoutingOperations" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_RoutingOperations" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ManufacturingRoutingId" TEXT NOT NULL,
                "Sequence" INTEGER NOT NULL,
                "OperationCode" TEXT NOT NULL,
                "OperationName" TEXT NOT NULL,
                "WorkCenterId" TEXT NOT NULL,
                "WorkCenterCode" TEXT NOT NULL,
                "WorkCenterName" TEXT NOT NULL,
                "StandardMinutes" TEXT NOT NULL,
                "LaborCostRate" TEXT NOT NULL,
                "MachineCostRate" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_RoutingOperations_ManufacturingRoutingId_Sequence" ON "RoutingOperations" ("ManufacturingRoutingId", "Sequence");""",
            """CREATE INDEX IF NOT EXISTS "IX_RoutingOperations_WorkCenterId" ON "RoutingOperations" ("WorkCenterId");""",
            """
            CREATE TABLE IF NOT EXISTS "OperationSchedules" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_OperationSchedules" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ScheduleNo" TEXT NOT NULL,
                "WorkOrderId" TEXT NOT NULL,
                "WorkOrderNo" TEXT NOT NULL,
                "RoutingOperationId" TEXT NOT NULL,
                "OperationCode" TEXT NOT NULL,
                "OperationName" TEXT NOT NULL,
                "WorkCenterId" TEXT NOT NULL,
                "WorkCenterCode" TEXT NOT NULL,
                "WorkCenterName" TEXT NOT NULL,
                "PlannedStartUtc" TEXT NOT NULL,
                "PlannedEndUtc" TEXT NOT NULL,
                "PlannedQuantity" TEXT NOT NULL,
                "CompletedQuantity" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "ScheduledBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_OperationSchedules_ScheduleNo" ON "OperationSchedules" ("ScheduleNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_OperationSchedules_WorkOrderId" ON "OperationSchedules" ("WorkOrderId");""",
            """CREATE INDEX IF NOT EXISTS "IX_OperationSchedules_WorkCenterId_Status" ON "OperationSchedules" ("WorkCenterId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "CapacityLoads" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_CapacityLoads" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "WorkCenterId" TEXT NOT NULL,
                "WorkCenterCode" TEXT NOT NULL,
                "WorkCenterName" TEXT NOT NULL,
                "PlanDate" TEXT NOT NULL,
                "AvailableMinutes" TEXT NOT NULL,
                "ReservedMinutes" TEXT NOT NULL,
                "SourceDocumentNo" TEXT NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CapacityLoads_WorkCenterId_PlanDate" ON "CapacityLoads" ("WorkCenterId", "PlanDate");""",
            """
            CREATE TABLE IF NOT EXISTS "ManufacturingCostSnapshots" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ManufacturingCostSnapshots" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "SnapshotNo" TEXT NOT NULL,
                "WorkOrderId" TEXT NOT NULL,
                "WorkOrderNo" TEXT NOT NULL,
                "FinishedItemId" TEXT NOT NULL,
                "FinishedItemCode" TEXT NOT NULL,
                "FinishedItemName" TEXT NOT NULL,
                "PlannedQuantity" TEXT NOT NULL,
                "MaterialCost" TEXT NOT NULL,
                "LaborCost" TEXT NOT NULL,
                "MachineCost" TEXT NOT NULL,
                "OverheadCost" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ManufacturingCostSnapshots_SnapshotNo" ON "ManufacturingCostSnapshots" ("SnapshotNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ManufacturingCostSnapshots_WorkOrderId" ON "ManufacturingCostSnapshots" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "MrpSuggestions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_MrpSuggestions" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "SuggestionNo" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                "ItemCode" TEXT NOT NULL,
                "ItemName" TEXT NOT NULL,
                "WarehouseId" TEXT NOT NULL,
                "WarehouseCode" TEXT NOT NULL,
                "WarehouseName" TEXT NOT NULL,
                "CurrentQuantity" TEXT NOT NULL,
                "DemandQuantity" TEXT NOT NULL,
                "SupplyQuantity" TEXT NOT NULL,
                "SuggestedQuantity" TEXT NOT NULL,
                "SourceType" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "DecidedBy" TEXT NOT NULL,
                "DecisionNote" TEXT NOT NULL,
                "DecidedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MrpSuggestions_SuggestionNo" ON "MrpSuggestions" ("SuggestionNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_MrpSuggestions_WarehouseId_ItemId_Status" ON "MrpSuggestions" ("WarehouseId", "ItemId", "Status");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Postgres Advanced Manufacturing Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresAdvancedManufacturingSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "WorkCenters" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "CapacityMinutesPerDay" numeric NOT NULL,
                "HourlyCostRate" numeric NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkCenters_Code" ON "WorkCenters" ("Code");""",
            """CREATE INDEX IF NOT EXISTS "IX_WorkCenters_WarehouseId" ON "WorkCenters" ("WarehouseId");""",
            """
            CREATE TABLE IF NOT EXISTS "ManufacturingRoutings" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "RoutingNo" text NOT NULL,
                "FinishedItemId" uuid NOT NULL,
                "FinishedItemCode" text NOT NULL,
                "FinishedItemName" text NOT NULL,
                "Version" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ManufacturingRoutings_RoutingNo" ON "ManufacturingRoutings" ("RoutingNo");""",
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ManufacturingRoutings_FinishedItemId_Version" ON "ManufacturingRoutings" ("FinishedItemId", "Version");""",
            """
            CREATE TABLE IF NOT EXISTS "RoutingOperations" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ManufacturingRoutingId" uuid NOT NULL,
                "Sequence" integer NOT NULL,
                "OperationCode" text NOT NULL,
                "OperationName" text NOT NULL,
                "WorkCenterId" uuid NOT NULL,
                "WorkCenterCode" text NOT NULL,
                "WorkCenterName" text NOT NULL,
                "StandardMinutes" numeric NOT NULL,
                "LaborCostRate" numeric NOT NULL,
                "MachineCostRate" numeric NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_RoutingOperations_ManufacturingRoutingId_Sequence" ON "RoutingOperations" ("ManufacturingRoutingId", "Sequence");""",
            """CREATE INDEX IF NOT EXISTS "IX_RoutingOperations_WorkCenterId" ON "RoutingOperations" ("WorkCenterId");""",
            """
            CREATE TABLE IF NOT EXISTS "OperationSchedules" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ScheduleNo" text NOT NULL,
                "WorkOrderId" uuid NOT NULL,
                "WorkOrderNo" text NOT NULL,
                "RoutingOperationId" uuid NOT NULL,
                "OperationCode" text NOT NULL,
                "OperationName" text NOT NULL,
                "WorkCenterId" uuid NOT NULL,
                "WorkCenterCode" text NOT NULL,
                "WorkCenterName" text NOT NULL,
                "PlannedStartUtc" timestamp with time zone NOT NULL,
                "PlannedEndUtc" timestamp with time zone NOT NULL,
                "PlannedQuantity" numeric NOT NULL,
                "CompletedQuantity" numeric NOT NULL,
                "Status" text NOT NULL,
                "ScheduledBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_OperationSchedules_ScheduleNo" ON "OperationSchedules" ("ScheduleNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_OperationSchedules_WorkOrderId" ON "OperationSchedules" ("WorkOrderId");""",
            """CREATE INDEX IF NOT EXISTS "IX_OperationSchedules_WorkCenterId_Status" ON "OperationSchedules" ("WorkCenterId", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "CapacityLoads" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "WorkCenterId" uuid NOT NULL,
                "WorkCenterCode" text NOT NULL,
                "WorkCenterName" text NOT NULL,
                "PlanDate" date NOT NULL,
                "AvailableMinutes" numeric NOT NULL,
                "ReservedMinutes" numeric NOT NULL,
                "SourceDocumentNo" text NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CapacityLoads_WorkCenterId_PlanDate" ON "CapacityLoads" ("WorkCenterId", "PlanDate");""",
            """
            CREATE TABLE IF NOT EXISTS "ManufacturingCostSnapshots" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "SnapshotNo" text NOT NULL,
                "WorkOrderId" uuid NOT NULL,
                "WorkOrderNo" text NOT NULL,
                "FinishedItemId" uuid NOT NULL,
                "FinishedItemCode" text NOT NULL,
                "FinishedItemName" text NOT NULL,
                "PlannedQuantity" numeric NOT NULL,
                "MaterialCost" numeric NOT NULL,
                "LaborCost" numeric NOT NULL,
                "MachineCost" numeric NOT NULL,
                "OverheadCost" numeric NOT NULL,
                "CreatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ManufacturingCostSnapshots_SnapshotNo" ON "ManufacturingCostSnapshots" ("SnapshotNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ManufacturingCostSnapshots_WorkOrderId" ON "ManufacturingCostSnapshots" ("WorkOrderId");""",
            """
            CREATE TABLE IF NOT EXISTS "MrpSuggestions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "SuggestionNo" text NOT NULL,
                "ItemId" uuid NOT NULL,
                "ItemCode" text NOT NULL,
                "ItemName" text NOT NULL,
                "WarehouseId" uuid NOT NULL,
                "WarehouseCode" text NOT NULL,
                "WarehouseName" text NOT NULL,
                "CurrentQuantity" numeric NOT NULL,
                "DemandQuantity" numeric NOT NULL,
                "SupplyQuantity" numeric NOT NULL,
                "SuggestedQuantity" numeric NOT NULL,
                "SourceType" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "DecidedBy" text NOT NULL,
                "DecisionNote" text NOT NULL,
                "DecidedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MrpSuggestions_SuggestionNo" ON "MrpSuggestions" ("SuggestionNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_MrpSuggestions_WarehouseId_ItemId_Status" ON "MrpSuggestions" ("WarehouseId", "ItemId", "Status");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Reporting Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureReportingSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteReportingSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresReportingSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Reporting Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteReportingSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "ReportDefinitions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ReportDefinitions" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "Key" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "QueryModel" TEXT NOT NULL,
                "ParametersJson" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ReportDefinitions_Key" ON "ReportDefinitions" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "ReportRunRecords" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ReportRunRecords" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "RunNo" TEXT NOT NULL,
                "ReportDefinitionId" TEXT NOT NULL,
                "ReportKey" TEXT NOT NULL,
                "ReportName" TEXT NOT NULL,
                "ParametersJson" TEXT NOT NULL,
                "ResultSummaryJson" TEXT NOT NULL,
                "RowCount" INTEGER NOT NULL,
                "Status" TEXT NOT NULL,
                "RunBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ReportRunRecords_RunNo" ON "ReportRunRecords" ("RunNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ReportRunRecords_ReportDefinitionId" ON "ReportRunRecords" ("ReportDefinitionId");""",
            """
            CREATE TABLE IF NOT EXISTS "ReportExportTasks" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ReportExportTasks" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ExportNo" TEXT NOT NULL,
                "ReportRunRecordId" TEXT NOT NULL,
                "ReportName" TEXT NOT NULL,
                "Format" TEXT NOT NULL,
                "FileName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "RequestedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ReportExportTasks_ExportNo" ON "ReportExportTasks" ("ExportNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ReportExportTasks_ReportRunRecordId" ON "ReportExportTasks" ("ReportRunRecordId");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Postgres Reporting Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresReportingSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "ReportDefinitions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "Key" text NOT NULL,
                "DisplayName" text NOT NULL,
                "Category" text NOT NULL,
                "QueryModel" text NOT NULL,
                "ParametersJson" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ReportDefinitions_Key" ON "ReportDefinitions" ("Key");""",
            """
            CREATE TABLE IF NOT EXISTS "ReportRunRecords" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "RunNo" text NOT NULL,
                "ReportDefinitionId" uuid NOT NULL,
                "ReportKey" text NOT NULL,
                "ReportName" text NOT NULL,
                "ParametersJson" text NOT NULL,
                "ResultSummaryJson" text NOT NULL,
                "RowCount" integer NOT NULL,
                "Status" text NOT NULL,
                "RunBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ReportRunRecords_RunNo" ON "ReportRunRecords" ("RunNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ReportRunRecords_ReportDefinitionId" ON "ReportRunRecords" ("ReportDefinitionId");""",
            """
            CREATE TABLE IF NOT EXISTS "ReportExportTasks" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ExportNo" text NOT NULL,
                "ReportRunRecordId" uuid NOT NULL,
                "ReportName" text NOT NULL,
                "Format" text NOT NULL,
                "FileName" text NOT NULL,
                "Status" text NOT NULL,
                "RequestedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ReportExportTasks_ExportNo" ON "ReportExportTasks" ("ExportNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ReportExportTasks_ReportRunRecordId" ON "ReportExportTasks" ("ReportRunRecordId");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Mobile Work Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureMobileWorkSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteMobileWorkSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresMobileWorkSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Mobile Work Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteMobileWorkSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "MobileDevices" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_MobileDevices" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "DeviceCode" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "AssignedTo" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL,
                "LastSeenAtUtc" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MobileDevices_DeviceCode" ON "MobileDevices" ("DeviceCode");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileDevices_AssignedTo" ON "MobileDevices" ("AssignedTo");""",
            """
            CREATE TABLE IF NOT EXISTS "MobileOfflineTasks" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_MobileOfflineTasks" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TaskNo" TEXT NOT NULL,
                "SourceModule" TEXT NOT NULL,
                "SourceTaskType" TEXT NOT NULL,
                "SourceTaskNo" TEXT NOT NULL,
                "PayloadJson" TEXT NOT NULL,
                "AssignedTo" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "CompletedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MobileOfflineTasks_TaskNo" ON "MobileOfflineTasks" ("TaskNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileOfflineTasks_AssignedTo_Status" ON "MobileOfflineTasks" ("AssignedTo", "Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileOfflineTasks_SourceModule_SourceTaskType_SourceTaskNo" ON "MobileOfflineTasks" ("SourceModule", "SourceTaskType", "SourceTaskNo");""",
            """
            CREATE TABLE IF NOT EXISTS "MobileScanEvents" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_MobileScanEvents" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ScanNo" TEXT NOT NULL,
                "DeviceCode" TEXT NOT NULL,
                "Barcode" TEXT NOT NULL,
                "TargetModule" TEXT NOT NULL,
                "Action" TEXT NOT NULL,
                "DocumentNo" TEXT NOT NULL,
                "Result" TEXT NOT NULL,
                "Message" TEXT NOT NULL,
                "Actor" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MobileScanEvents_ScanNo" ON "MobileScanEvents" ("ScanNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileScanEvents_DeviceCode" ON "MobileScanEvents" ("DeviceCode");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileScanEvents_TargetModule_DocumentNo" ON "MobileScanEvents" ("TargetModule", "DocumentNo");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Postgres Mobile Work Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresMobileWorkSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "MobileDevices" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "DeviceCode" text NOT NULL,
                "DisplayName" text NOT NULL,
                "AssignedTo" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL,
                "LastSeenAtUtc" timestamp with time zone NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MobileDevices_DeviceCode" ON "MobileDevices" ("DeviceCode");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileDevices_AssignedTo" ON "MobileDevices" ("AssignedTo");""",
            """
            CREATE TABLE IF NOT EXISTS "MobileOfflineTasks" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TaskNo" text NOT NULL,
                "SourceModule" text NOT NULL,
                "SourceTaskType" text NOT NULL,
                "SourceTaskNo" text NOT NULL,
                "PayloadJson" text NOT NULL,
                "AssignedTo" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "CompletedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MobileOfflineTasks_TaskNo" ON "MobileOfflineTasks" ("TaskNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileOfflineTasks_AssignedTo_Status" ON "MobileOfflineTasks" ("AssignedTo", "Status");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileOfflineTasks_SourceModule_SourceTaskType_SourceTaskNo" ON "MobileOfflineTasks" ("SourceModule", "SourceTaskType", "SourceTaskNo");""",
            """
            CREATE TABLE IF NOT EXISTS "MobileScanEvents" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ScanNo" text NOT NULL,
                "DeviceCode" text NOT NULL,
                "Barcode" text NOT NULL,
                "TargetModule" text NOT NULL,
                "Action" text NOT NULL,
                "DocumentNo" text NOT NULL,
                "Result" text NOT NULL,
                "Message" text NOT NULL,
                "Actor" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MobileScanEvents_ScanNo" ON "MobileScanEvents" ("ScanNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileScanEvents_DeviceCode" ON "MobileScanEvents" ("DeviceCode");""",
            """CREATE INDEX IF NOT EXISTS "IX_MobileScanEvents_TargetModule_DocumentNo" ON "MobileScanEvents" ("TargetModule", "DocumentNo");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Integration Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureIntegrationSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteIntegrationSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresIntegrationSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Integration Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteIntegrationSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "MessageChannels" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_MessageChannels" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ChannelKey" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "ChannelType" TEXT NOT NULL,
                "Endpoint" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MessageChannels_ChannelKey" ON "MessageChannels" ("ChannelKey");""",
            """
            CREATE TABLE IF NOT EXISTS "WebhookSubscriptions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebhookSubscriptions" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "SubscriptionKey" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "EventKey" TEXT NOT NULL,
                "TargetUrl" TEXT NOT NULL,
                "SecretName" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebhookSubscriptions_SubscriptionKey" ON "WebhookSubscriptions" ("SubscriptionKey");""",
            """CREATE INDEX IF NOT EXISTS "IX_WebhookSubscriptions_EventKey" ON "WebhookSubscriptions" ("EventKey");""",
            """
            CREATE TABLE IF NOT EXISTS "ExternalConnectors" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ExternalConnectors" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ConnectorKey" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "Provider" TEXT NOT NULL,
                "BaseUrl" TEXT NOT NULL,
                "AuthMode" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ExternalConnectors_ConnectorKey" ON "ExternalConnectors" ("ConnectorKey");""",
            """
            CREATE TABLE IF NOT EXISTS "IntegrationSyncJobs" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_IntegrationSyncJobs" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "JobNo" TEXT NOT NULL,
                "ConnectorKey" TEXT NOT NULL,
                "Direction" TEXT NOT NULL,
                "PayloadJson" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "AttemptCount" INTEGER NOT NULL,
                "LastError" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "CompletedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_IntegrationSyncJobs_JobNo" ON "IntegrationSyncJobs" ("JobNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_IntegrationSyncJobs_ConnectorKey_Status" ON "IntegrationSyncJobs" ("ConnectorKey", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "IntegrationAuditRecords" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_IntegrationAuditRecords" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "AuditNo" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "Action" TEXT NOT NULL,
                "TargetKey" TEXT NOT NULL,
                "Result" TEXT NOT NULL,
                "Message" TEXT NOT NULL,
                "Actor" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_IntegrationAuditRecords_AuditNo" ON "IntegrationAuditRecords" ("AuditNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_IntegrationAuditRecords_Category_TargetKey" ON "IntegrationAuditRecords" ("Category", "TargetKey");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Postgres Integration Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresIntegrationSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "MessageChannels" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ChannelKey" text NOT NULL,
                "DisplayName" text NOT NULL,
                "ChannelType" text NOT NULL,
                "Endpoint" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_MessageChannels_ChannelKey" ON "MessageChannels" ("ChannelKey");""",
            """
            CREATE TABLE IF NOT EXISTS "WebhookSubscriptions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "SubscriptionKey" text NOT NULL,
                "DisplayName" text NOT NULL,
                "EventKey" text NOT NULL,
                "TargetUrl" text NOT NULL,
                "SecretName" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebhookSubscriptions_SubscriptionKey" ON "WebhookSubscriptions" ("SubscriptionKey");""",
            """CREATE INDEX IF NOT EXISTS "IX_WebhookSubscriptions_EventKey" ON "WebhookSubscriptions" ("EventKey");""",
            """
            CREATE TABLE IF NOT EXISTS "ExternalConnectors" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ConnectorKey" text NOT NULL,
                "DisplayName" text NOT NULL,
                "Provider" text NOT NULL,
                "BaseUrl" text NOT NULL,
                "AuthMode" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ExternalConnectors_ConnectorKey" ON "ExternalConnectors" ("ConnectorKey");""",
            """
            CREATE TABLE IF NOT EXISTS "IntegrationSyncJobs" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "JobNo" text NOT NULL,
                "ConnectorKey" text NOT NULL,
                "Direction" text NOT NULL,
                "PayloadJson" text NOT NULL,
                "Status" text NOT NULL,
                "AttemptCount" integer NOT NULL,
                "LastError" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "CompletedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_IntegrationSyncJobs_JobNo" ON "IntegrationSyncJobs" ("JobNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_IntegrationSyncJobs_ConnectorKey_Status" ON "IntegrationSyncJobs" ("ConnectorKey", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "IntegrationAuditRecords" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "AuditNo" text NOT NULL,
                "Category" text NOT NULL,
                "Action" text NOT NULL,
                "TargetKey" text NOT NULL,
                "Result" text NOT NULL,
                "Message" text NOT NULL,
                "Actor" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_IntegrationAuditRecords_AuditNo" ON "IntegrationAuditRecords" ("AuditNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_IntegrationAuditRecords_Category_TargetKey" ON "IntegrationAuditRecords" ("Category", "TargetKey");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Document Exchange Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureDocumentExchangeSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteDocumentExchangeSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresDocumentExchangeSchemaAsync(dbContext, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Document Exchange Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteDocumentExchangeSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "ImportTemplates" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ImportTemplates" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TemplateKey" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "TargetModule" TEXT NOT NULL,
                "FileType" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ImportTemplates_TemplateKey" ON "ImportTemplates" ("TemplateKey");""",
            """
            CREATE TABLE IF NOT EXISTS "ImportFieldMappings" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ImportFieldMappings" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TemplateKey" TEXT NOT NULL,
                "SourceField" TEXT NOT NULL,
                "TargetField" TEXT NOT NULL,
                "IsRequired" INTEGER NOT NULL,
                "TransformRule" TEXT NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ImportFieldMappings_TemplateKey_TargetField" ON "ImportFieldMappings" ("TemplateKey", "TargetField");""",
            """
            CREATE TABLE IF NOT EXISTS "ImportBatches" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ImportBatches" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "BatchNo" TEXT NOT NULL,
                "TemplateKey" TEXT NOT NULL,
                "FileName" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "RowCount" INTEGER NOT NULL,
                "ErrorCount" INTEGER NOT NULL,
                "ErrorMessage" TEXT NOT NULL,
                "CreatedBy" TEXT NOT NULL,
                "CompletedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ImportBatches_BatchNo" ON "ImportBatches" ("BatchNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ImportBatches_TemplateKey_Status" ON "ImportBatches" ("TemplateKey", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "ExportFileTasks" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ExportFileTasks" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ExportNo" TEXT NOT NULL,
                "SourceModule" TEXT NOT NULL,
                "FileName" TEXT NOT NULL,
                "Format" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "RequestedBy" TEXT NOT NULL,
                "CompletedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ExportFileTasks_ExportNo" ON "ExportFileTasks" ("ExportNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ExportFileTasks_SourceModule_Status" ON "ExportFileTasks" ("SourceModule", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "PrintTemplates" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PrintTemplates" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "TemplateKey" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "TargetModule" TEXT NOT NULL,
                "ContentType" TEXT NOT NULL,
                "TemplateBody" TEXT NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "UpdatedBy" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PrintTemplates_TemplateKey" ON "PrintTemplates" ("TemplateKey");""",
            """
            CREATE TABLE IF NOT EXISTS "PrintJobs" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_PrintJobs" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "JobNo" TEXT NOT NULL,
                "TemplateKey" TEXT NOT NULL,
                "DocumentNo" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "RequestedBy" TEXT NOT NULL,
                "CompletedBy" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PrintJobs_JobNo" ON "PrintJobs" ("JobNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PrintJobs_TemplateKey_Status" ON "PrintJobs" ("TemplateKey", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "FileAuditRecords" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_FileAuditRecords" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "AuditNo" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "Action" TEXT NOT NULL,
                "TargetNo" TEXT NOT NULL,
                "Result" TEXT NOT NULL,
                "Message" TEXT NOT NULL,
                "Actor" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FileAuditRecords_AuditNo" ON "FileAuditRecords" ("AuditNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_FileAuditRecords_Category_TargetNo" ON "FileAuditRecords" ("Category", "TargetNo");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Postgres Document Exchange Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresDocumentExchangeSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "ImportTemplates" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TemplateKey" text NOT NULL,
                "DisplayName" text NOT NULL,
                "TargetModule" text NOT NULL,
                "FileType" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ImportTemplates_TemplateKey" ON "ImportTemplates" ("TemplateKey");""",
            """
            CREATE TABLE IF NOT EXISTS "ImportFieldMappings" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TemplateKey" text NOT NULL,
                "SourceField" text NOT NULL,
                "TargetField" text NOT NULL,
                "IsRequired" boolean NOT NULL,
                "TransformRule" text NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ImportFieldMappings_TemplateKey_TargetField" ON "ImportFieldMappings" ("TemplateKey", "TargetField");""",
            """
            CREATE TABLE IF NOT EXISTS "ImportBatches" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "BatchNo" text NOT NULL,
                "TemplateKey" text NOT NULL,
                "FileName" text NOT NULL,
                "Status" text NOT NULL,
                "RowCount" integer NOT NULL,
                "ErrorCount" integer NOT NULL,
                "ErrorMessage" text NOT NULL,
                "CreatedBy" text NOT NULL,
                "CompletedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ImportBatches_BatchNo" ON "ImportBatches" ("BatchNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ImportBatches_TemplateKey_Status" ON "ImportBatches" ("TemplateKey", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "ExportFileTasks" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ExportNo" text NOT NULL,
                "SourceModule" text NOT NULL,
                "FileName" text NOT NULL,
                "Format" text NOT NULL,
                "Status" text NOT NULL,
                "RequestedBy" text NOT NULL,
                "CompletedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ExportFileTasks_ExportNo" ON "ExportFileTasks" ("ExportNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_ExportFileTasks_SourceModule_Status" ON "ExportFileTasks" ("SourceModule", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "PrintTemplates" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "TemplateKey" text NOT NULL,
                "DisplayName" text NOT NULL,
                "TargetModule" text NOT NULL,
                "ContentType" text NOT NULL,
                "TemplateBody" text NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "UpdatedBy" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PrintTemplates_TemplateKey" ON "PrintTemplates" ("TemplateKey");""",
            """
            CREATE TABLE IF NOT EXISTS "PrintJobs" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "JobNo" text NOT NULL,
                "TemplateKey" text NOT NULL,
                "DocumentNo" text NOT NULL,
                "Status" text NOT NULL,
                "RequestedBy" text NOT NULL,
                "CompletedBy" text NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_PrintJobs_JobNo" ON "PrintJobs" ("JobNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_PrintJobs_TemplateKey_Status" ON "PrintJobs" ("TemplateKey", "Status");""",
            """
            CREATE TABLE IF NOT EXISTS "FileAuditRecords" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "AuditNo" text NOT NULL,
                "Category" text NOT NULL,
                "Action" text NOT NULL,
                "TargetNo" text NOT NULL,
                "Result" text NOT NULL,
                "Message" text NOT NULL,
                "Actor" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_FileAuditRecords_AuditNo" ON "FileAuditRecords" ("AuditNo");""",
            """CREATE INDEX IF NOT EXISTS "IX_FileAuditRecords_Category_TargetNo" ON "FileAuditRecords" ("Category", "TargetNo");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Organization Collaboration Schema Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    public static async Task EnsureOrganizationCollaborationSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            await EnsureSqliteOrganizationCollaborationSchemaAsync(dbContext, cancellationToken);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await EnsurePostgresOrganizationCollaborationSchemaAsync(dbContext, cancellationToken);
        }
    }

    private static async Task EnsureSqliteOrganizationCollaborationSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "CollaborationConversations" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_CollaborationConversations" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ConversationKey" TEXT NOT NULL,
                "ScopeType" TEXT NOT NULL,
                "Title" TEXT NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CollaborationConversations_ConversationKey" ON "CollaborationConversations" ("ConversationKey");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationParticipants" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_CollaborationParticipants" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ConversationId" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "UserName" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                CONSTRAINT "FK_CollaborationParticipants_CollaborationConversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "CollaborationConversations" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CollaborationParticipants_ConversationId_UserId" ON "CollaborationParticipants" ("ConversationId", "UserId");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationMessages" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_CollaborationMessages" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ConversationId" TEXT NOT NULL,
                "SenderUserId" TEXT NOT NULL,
                "SenderUserName" TEXT NOT NULL,
                "SenderDisplayName" TEXT NOT NULL,
                "Content" TEXT NOT NULL,
                CONSTRAINT "FK_CollaborationMessages_CollaborationConversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "CollaborationConversations" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_CollaborationMessages_ConversationId_CreatedAtUtc" ON "CollaborationMessages" ("ConversationId", "CreatedAtUtc");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationAttachments" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_CollaborationAttachments" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ConversationId" TEXT NOT NULL,
                "MessageId" TEXT NOT NULL,
                "FileName" TEXT NOT NULL,
                "ContentType" TEXT NOT NULL,
                "SizeBytes" INTEGER NOT NULL,
                "Content" BLOB NOT NULL,
                "UploadedByUserId" TEXT NOT NULL,
                "UploadedBy" TEXT NOT NULL,
                CONSTRAINT "FK_CollaborationAttachments_CollaborationMessages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "CollaborationMessages" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_CollaborationAttachments_ConversationId_MessageId" ON "CollaborationAttachments" ("ConversationId", "MessageId");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationReadStates" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_CollaborationReadStates" PRIMARY KEY,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "ConversationId" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "LastReadMessageId" TEXT NULL,
                "LastReadAtUtc" TEXT NULL,
                CONSTRAINT "FK_CollaborationReadStates_CollaborationConversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "CollaborationConversations" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CollaborationReadStates_ConversationId_UserId" ON "CollaborationReadStates" ("ConversationId", "UserId");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    private static async Task EnsurePostgresOrganizationCollaborationSchemaAsync(AeroErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "CollaborationConversations" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ConversationKey" text NOT NULL,
                "ScopeType" text NOT NULL,
                "Title" text NOT NULL
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CollaborationConversations_ConversationKey" ON "CollaborationConversations" ("ConversationKey");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationParticipants" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ConversationId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "UserName" text NOT NULL,
                "DisplayName" text NOT NULL,
                CONSTRAINT "FK_CollaborationParticipants_CollaborationConversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "CollaborationConversations" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CollaborationParticipants_ConversationId_UserId" ON "CollaborationParticipants" ("ConversationId", "UserId");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationMessages" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ConversationId" uuid NOT NULL,
                "SenderUserId" uuid NOT NULL,
                "SenderUserName" text NOT NULL,
                "SenderDisplayName" text NOT NULL,
                "Content" text NOT NULL,
                CONSTRAINT "FK_CollaborationMessages_CollaborationConversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "CollaborationConversations" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_CollaborationMessages_ConversationId_CreatedAtUtc" ON "CollaborationMessages" ("ConversationId", "CreatedAtUtc");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationAttachments" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ConversationId" uuid NOT NULL,
                "MessageId" uuid NOT NULL,
                "FileName" text NOT NULL,
                "ContentType" text NOT NULL,
                "SizeBytes" bigint NOT NULL,
                "Content" bytea NOT NULL,
                "UploadedByUserId" uuid NOT NULL,
                "UploadedBy" text NOT NULL,
                CONSTRAINT "FK_CollaborationAttachments_CollaborationMessages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "CollaborationMessages" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE INDEX IF NOT EXISTS "IX_CollaborationAttachments_ConversationId_MessageId" ON "CollaborationAttachments" ("ConversationId", "MessageId");""",
            """
            CREATE TABLE IF NOT EXISTS "CollaborationReadStates" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ConversationId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "LastReadMessageId" uuid NULL,
                "LastReadAtUtc" timestamp with time zone NULL,
                CONSTRAINT "FK_CollaborationReadStates_CollaborationConversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "CollaborationConversations" ("Id") ON DELETE CASCADE
            );
            """,
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_CollaborationReadStates_ConversationId_UserId" ON "CollaborationReadStates" ("ConversationId", "UserId");"""
        };

        foreach (var statement in statements)
        {
            await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
        }
    }

    /// <summary>
    /// Ensure Sqlite Column Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="tableName">table Name 参数。</param>
    /// <param name="columnName">column Name 参数。</param>
    /// <param name="columnDefinition">column Definition 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsureSqliteColumnAsync(AeroErpDbContext dbContext, string tableName, string columnName, string columnDefinition, CancellationToken cancellationToken)
    {
        ValidateSchemaIdentifier(tableName);
        ValidateSchemaIdentifier(columnName);
        var columnQuery = $"SELECT name FROM pragma_table_info('{tableName}')";
        var columns = await dbContext.Database.SqlQueryRaw<string>(columnQuery).ToListAsync(cancellationToken);
        if (columns.Any(x => string.Equals(x, columnName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var sql = $"""ALTER TABLE "{tableName}" ADD COLUMN "{columnName}" {columnDefinition};""";
        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Ensure Postgres Column Async。
    /// </summary>
    /// <param name="dbContext">db Context 参数。</param>
    /// <param name="tableName">table Name 参数。</param>
    /// <param name="columnName">column Name 参数。</param>
    /// <param name="columnDefinition">column Definition 参数。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    private static async Task EnsurePostgresColumnAsync(AeroErpDbContext dbContext, string tableName, string columnName, string columnDefinition, CancellationToken cancellationToken)
    {
        ValidateSchemaIdentifier(tableName);
        ValidateSchemaIdentifier(columnName);
        var sql = $"""ALTER TABLE "{tableName}" ADD COLUMN IF NOT EXISTS "{columnName}" {columnDefinition};""";
        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Validate Schema Identifier。
    /// </summary>
    /// <param name="value">数值或配置值。</param>
    private static void ValidateSchemaIdentifier(string value)
    {
        if (value.Any(ch => !char.IsLetterOrDigit(ch) && ch != '_'))
        {
            throw new InvalidOperationException("Invalid schema identifier.");
        }
    }
}
