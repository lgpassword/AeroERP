# State: AeroERP

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-07-07)

**Core value:** Deliver a modular ERP foundation whose business flows, plugin boundaries, UI behavior, and AI-agent governance remain clear, auditable, and closed-loop from click to persisted result.
**Current focus:** Phase 19 库存成本、制造成本与成本月结已启动；Phase 19A 库存成本基础与存货明细账、Phase 19B 制造成本归集与完工成本已通过 single-file AppHost + 隔离 SQLite HTTP 验证。

## Status

- Current milestone: Milestone 5
- Current phase: Phase 19 / Manufacturing costing and completion cost
- Lifecycle status: procurement, inventory, inventory costing, inventory ledger, location/bin stock, sales, accounting account, accounting period, manual general ledger voucher, business-source voucher, voucher review, period close constraints, trial balance, income statement basics, balance sheet basics, receivable, payable, AP/AR aging, AP/AR tax split, finance invoice record, bank account, bank statement line, bank reconciliation, settlement, workflow approval, notification, analytics, data-scope, numbering, localization, currency, manufacturing BOM, work order, production issue, completion receipt, quality inspection, lot traceability, planning suggestion, outsourcing execution, and barcode/PDA execution loops implemented with authentication, Chinese UI, permissions, audit events, and real persistence

## Notes

- The backend builds and exposes platform governance, plugin visibility, agent review, master data, and procurement APIs.
- The frontend builds as a separate React workspace with a dedicated UI style library and shared UI kit.
- Phase 17A added public AppHost health endpoints `/health/live` and `/health/ready`; readiness validates database connectivity and reports module/plugin counts.
- Phase 17A added `tests/AeroERP.AppHost.Tests` with xUnit + `WebApplicationFactory` smoke coverage for root catalog, health endpoints, Swagger, and anonymous rejection on `/api/platform/auth/me`.
- Phase 17A verification passed: `dotnet build AeroERP.slnx --no-restore --disable-build-servers`, `dotnet test tests\AeroERP.AppHost.Tests\AeroERP.AppHost.Tests.csproj --no-build`, and `npm run build`.
- Phase 17B added `scripts/verify.ps1` as a reusable local quality gate for .NET restore/build/test and frontend production build.
- Phase 17C expanded AppHost integration tests to cover default administrator login, JWT-backed `/api/platform/auth/me`, and `/api/platform/visible-modules`; `scripts/verify.ps1` passes with 8 backend tests.
- Phase 18A added finance accounting foundation: `AccountingAccount`, `AccountingPeriod`, `/api/finance/accounting-accounts`, `/api/finance/accounting-periods`, `finance.accounting.manage`, audit events, SQLite/PostgreSQL bootstrap tables, and finance page UI for account maintenance plus period close/reopen.
- Phase 18A verification passed through `.\scripts\verify.ps1`: restore/build/test/frontend build all succeeded; AppHost smoke tests now cover accounting account creation plus period create/close/reopen.
- Phase 18B added manual general ledger voucher foundation: voucher header, debit/credit lines, `finance.voucher.manage`, `finance.voucher.review`, API endpoints for create/submit/approve/reject, audit events, SQLite/PostgreSQL bootstrap tables, and finance page UI for voucher entry and review.
- Phase 18B verification passed through `.\scripts\verify.ps1`: backend build 0 warnings/errors, AppHost smoke tests cover manual voucher create/submit/approve, and frontend production build succeeds.
- Phase 18C added business-source general ledger vouchers from payable, receivable, and settlement records; vouchers now persist source type/id/no, block duplicate source voucher generation, and accounting period close now blocks draft/submitted vouchers. Finance UI exposes source voucher generation with user-selected debit/credit accounts, source display in voucher lists, and period close disabled when unresolved vouchers remain.
- Phase 18C verification passed through `.\scripts\verify.ps1`: backend build 0 warnings/errors, AppHost smoke tests 9 passed including payable-source voucher generation and pending-voucher close blocking, and frontend production build succeeds.
- Phase 18D added AP/AR due dates and aging risk: new payable/receivable records receive a default 30-day due date, list DTOs expose due date and overdue days, `/api/finance/aging` returns payable/receivable aging buckets and overdue details, and Finance UI shows aging distribution plus overdue records.
- Phase 18D verification passed through `.\scripts\verify.ps1`: backend build 0 warnings/errors, AppHost smoke tests 9 passed including due date and aging API assertions, and frontend production build succeeds.
- Phase 18E added finance tax split and invoice records: payables/receivables now persist tax invoice type, tax rate, net amount, and tax amount; procurement payables inherit tax settings from the source procurement request, sales receivables inherit from the sales order, and `/api/finance/invoices` supports listing and creating one invoice per payable/receivable source with audit events.
- Phase 18E frontend adds tax split display on payable/receivable cards, invoice registration controls gated by finance settlement management permission, and a finance invoice history section with invoice date, source document, counterparty, gross/net/tax amounts, creator, and timestamp.
- Phase 18E smoke tests now include payable tax split assertions plus finance invoice create/list/duplicate-rejection coverage in the existing payable-source business voucher flow.
- Phase 18E verification: `dotnet build AeroERP.slnx --no-restore --disable-build-servers` passes with 0 warnings/errors and `npm run build --workspace @aeroerp/web` passes. That session previously hit Windows application control policy `0x800711C7` during `dotnet test`, but Phase 18F later reran the full `.\scripts\verify.ps1` gate successfully on this machine.
- Phase 18F added bank account, bank statement, and reconciliation depth: settlements now persist bank account snapshots and reconciliation status; settlement creation requires an enabled same-currency bank account; bank statement lines can be recorded against maintained ERP bank accounts; reconciliation matches only when bank account, direction, currency, and amount align.
- Phase 18F frontend adds bank account maintenance, bank statement entry, bank reconciliation candidates, required bank account selection on payable/receivable settlement cards, and reconciliation state display in settlement history.
- Phase 18F smoke tests now include bank account create/list, payable settlement with bank account, bank statement line create/list, reconciliation execution, and matched status assertions for both statement lines and settlements.
- Phase 18F verification passed through `.\scripts\verify.ps1`: restore/build succeeded, AppHost smoke tests 9 passed including bank reconciliation flow, and frontend workspace production build succeeded.
- Phase 18G added finance report snapshots derived from approved GL vouchers: `/api/finance/reports` returns trial balance lines, income statement basics, and balance sheet basics for all periods or one selected accounting period.
- Phase 18G frontend adds a finance report section with period filter, debit/credit trial status, trial balance account lines, income statement basics, and balance sheet basics; empty state appears until approved vouchers exist.
- Phase 18G smoke tests assert reports only include approved vouchers by creating a same-period draft voucher and verifying report totals remain at the approved amount.
- Phase 18G verification: `dotnet build AeroERP.slnx --no-restore --disable-build-servers` passes with 0 warnings/errors, `npm run build --workspace @aeroerp/web` passes, and `dotnet run --project tools\AeroERP.FinanceReportValidation\AeroERP.FinanceReportValidation.csproj` passes with approved voucher count 1, trial balance debit/credit 100/100, profit 100, and balance sheet difference 0. Full AppHost/xUnit verification remains blocked by Windows application control policy `0x800711C7`: `dotnet test` cannot load `AeroERP.AppHost.Tests.dll`, and single-file AppHost with isolated SQLite cannot load `e_sqlite3`.
- Phase 19A added inventory costing foundation: receipt/issue/transfer/count lines, inventory movements, stock balances, location stock balances, and production issue lines now carry unit cost, cost amount, inventory value, and movement balance cost where applicable.
- Phase 19A uses warehouse-level moving weighted average as the default inventory valuation basis. Purchase receipts accept unit cost inputs; sales issues, transfers, count decreases, production material issues, and outsourcing material issues issue inventory at warehouse balance cost. Finished-goods cost rollup remains out of scope for 19A and belongs to Phase 19B.
- Phase 19A added `/api/inventory/ledger` and an inventory page "存货明细账" panel with warehouse/item filters and quantity/amount columns. Empty states remain explicit and no seeded business data is introduced.
- Phase 19A added `tools/AeroERP.InventoryCostValidation`, a service-level validation tool intended to assert moving weighted average, sales issue cost, production issue cost, and ledger in/out amount mapping. It builds as part of `AeroERP.slnx`.
- Phase 19A fixed `/api/inventory/ledger` SQLite compatibility by materializing filtered movements before ordering by `DateTimeOffset`, avoiding SQLite ORDER BY translation failures.
- Phase 19A verification passes `dotnet build AeroERP.slnx --no-restore --disable-build-servers`, `npm run build --workspace @aeroerp/web`, and a single-file AppHost + isolated SQLite HTTP flow: two purchase receipts `10 @ 5 + 10 @ 15` produce `20 @ 10 = 200`, sales issue `4 @ 10 = 40`, production issue `3 @ 10 = 30`, final balance `13 @ 10 = 130`, and `/api/inventory/ledger` returns 4 entries with in amount 200 and out amount 70. Direct `dotnet run --project tools\AeroERP.InventoryCostValidation\AeroERP.InventoryCostValidation.csproj --no-restore --no-build` remains blocked by Windows application control policy `0x800711C7`; that environment issue remains tracked in `AeroERP-lkb`.
- Phase 19B adds manufacturing cost collection and finished-goods completion costing: work orders now expose material, labor, machine, overhead, total, received, remaining, unit cost, cost source, and cost variance; production receipts persist unit cost and component cost splits; completion receipt writes finished-goods stock balance and inventory movement cost from the work order cost rollup.
- Phase 19B costing uses production issue actual material cost plus the latest advanced manufacturing cost snapshot for labor, machine, and overhead. If no snapshot exists, it derives labor, machine, and overhead from completed operation schedules, routing operation rates, and work center hourly cost rates. Cost month-end, locking, and recomputation audit remain Phase 19C scope.
- Phase 19B frontend updates the manufacturing page to show work order cost composition, expected unit cost, remaining cost, completion receipt cost split, and snapshot variance without adding dead controls.
- Phase 19B verification passes `dotnet build AeroERP.slnx --no-restore --disable-build-servers`, `npm run build --workspace @aeroerp/web`, and single-file AppHost + isolated SQLite HTTP validation: production issue material cost 30, cost snapshot labor 12 / machine 8 / overhead 5, completion receipt `1 @ 55 = 55`, finished-goods balance `1 @ 55 = 55`, and finished-goods inventory ledger in amount 55. Direct tool execution remains blocked by Windows application control policy `0x800711C7`, but `tools/AeroERP.InventoryCostValidation` has been extended and compiles with the same assertions.
- Runtime defaults to PostgreSQL when configured and falls back to local SQLite for immediate local startup.
- User-facing screens rely on persistence or explicit empty states; no fake dashboard records are rendered.
- Platform identity now uses JWT login, seeded system roles, and a single bootstrap administrator account.
- Module visibility and page navigation are now filtered by both plugin visibility and user role module grants.
- The main operator-facing UI is now Chinese-first and keeps all actions wired to real backend APIs.
- Platform governance now includes account enable/disable, self-service password change, administrator password reset, and permission-driven button visibility.
- JWT validation now rechecks user enablement and refreshes roles, permissions, and visible module claims on every authorized request.
- Audit event listing now works under the default SQLite development runtime.
- Frontend permission constants are now centralized so platform, master data, and procurement pages share the same permission mapping source.
- Master data page now separates readable, manageable, loading, error, and empty states by `master-data.read` and `master-data.manage`.
- Procurement page now separates request create, request review, order create, order release, and master-data dependency states by real permission checks, without exposing dead buttons.
- Inventory page now exposes pending procurement receipts, receipt execution, receipt history, and stock balances from real APIs, with permission-driven read/manage separation.
- Released procurement orders now provide a live navigation path into the inventory module, keeping the procurement downstream flow closed-loop.
- The master data module now includes customers and extends the analysis-first UI to cover customer, supplier, item, and warehouse structures without demo data.
- A new sales module is now registered, permission-controlled, and visible in top navigation only when the current user can access it.
- Sales APIs now support customer-based quotation creation, quotation-to-order conversion, order confirmation, and ready-to-ship transitions with auditable persistence.
- The sales page now forms a real UI loop around customer selection, quotation creation, sales order creation, and status progression to ready-to-ship.
- The inventory module now supports outbound issue, warehouse transfer, stock counting, movement history, and balance visibility through real APIs and Chinese execution UI.
- The inventory page has been redesigned into an execution console with animated in-page navigation, permission-aware actions, and no seeded test data.
- Sales orders in `ReadyToShip` now link directly into the outbound panel, and released procurement orders now link directly into the receipt panel.
- Inventory receipt and issue actions now require real warehouse selection per document row, avoiding shared form state across multiple business documents.
- Phase 8 frontend and backend both compile successfully after inventory execution closure was completed.
- Existing SQLite development databases are upgraded in place for inventory tables during startup; local data no longer needs to be dropped for this module.
- Existing SQLite development databases are now upgraded in place for customer and sales tables during startup.
- Procurement, inventory, and agent-review list APIs now sort in memory after query materialization to remain compatible with SQLite `DateTimeOffset` limitations.
- The seeded purchaser role now receives the `inventory` module grant alongside `inventory.read`, avoiding permission-visible but route-hidden behavior.
- The seeded platform administrator and operations manager roles now include the `sales` module grant and related sales permissions.
- A dedicated Chinese planning note now captures the gap analysis against mainstream ERP products and the recommended staged roadmap: `.planning/ERP_GAP_ANALYSIS.zh-CN.md`.
- The roadmap now extends beyond the first procurement-inventory loop into sales, warehouse execution, settlement, workflow governance, reporting, manufacturing, quality, and planning.
- A dedicated finance module is now registered as `finance` / `财务结算`, permission-controlled, and visible only to accounts with module access.
- Finance APIs now expose payables, receivables, and settlements through `/api/finance` with separate read, payable manage, receivable manage, and settlement manage permissions.
- Payables can be generated from completed procurement receipts or received procurement orders, with duplicate-source protection across receipt and order paths.
- Receivables can be generated from completed inventory issues or shipped sales orders, with duplicate-source protection across issue and order paths.
- Settlement actions persist real settlement records, update payable/receivable settled and remaining amounts, and write finance audit events.
- Finance invoice actions persist tax invoice records from real payable/receivable sources, carry the source tax split, block duplicate invoice registration per source, and write finance audit events.
- Bank account actions maintain ERP-side bank accounts; bank statement actions record user-entered/import-ready statement lines; reconciliation actions persist reviewer-confirmed matches between statement lines and settlement records with audit events.
- The finance page now provides Chinese empty states, real source-document candidates, amount entry, payment/collection settlement actions, bank account maintenance, bank statement entry, reconciliation matching, finance reports, and settlement history without seeded demo data.
- Procurement orders in `Received` state and sales orders in `Shipped` state now link into the finance settlement page when the account has finance module access.
- Existing SQLite and PostgreSQL development databases are upgraded in place for finance tables during startup.
- Phase 9 frontend and backend both compile successfully after finance settlement closure was completed.
- A dedicated workflow module is now registered as `workflow` / `审批中心`, permission-controlled, and visible only to accounts with module access.
- Workflow APIs now expose definitions, instances, approval tasks, and notifications through `/api/workflow` with separate read, task decision, and notification permissions.
- The seeded workflow definition `procurement-request-review` now creates approval instances and tasks when a procurement request is submitted.
- The approval center can approve or reject procurement request tasks, update the underlying procurement request status, write workflow audit events, and create result notifications.
- Workflow notifications can be listed and marked read or unread through real persisted API calls.
- The procurement page now routes submitted requests to the approval center for review instead of keeping approval actions isolated inside the procurement page.
- Existing SQLite and PostgreSQL development databases are upgraded in place for workflow tables during startup.
- Phase 10 frontend and backend both compile successfully after workflow approval closure was completed.
- A dedicated control module is now registered as `control` / `经营管控`, permission-controlled, and visible only to accounts with module access.
- Control APIs now expose live analytics, data scope rules, and numbering rules through `/api/control` with separate analytics, data-scope, and numbering permissions.
- The analytics endpoint reports real procurement, sales, inventory, and finance metrics from persisted tables without seeded metrics.
- Data scope rules now support a first concrete enforcement path: non-admin sales order lists can be filtered by customer-name match rules per role.
- Numbering rules now support procurement request and sales quotation prefixes, date segments, padding, and persisted next sequence.
- New procurement requests and sales quotations now consume numbering rules instead of hardcoded timestamp-only document numbers.
- The control page now provides Chinese analytics, data scope configuration, and numbering rule configuration surfaces with real API actions and empty states.
- Existing SQLite and PostgreSQL development databases are upgraded in place for control tables during startup.
- Phase 11 frontend and backend both compile successfully after analytics, data-scope, and numbering closure was completed.
- A dedicated localization module is now registered as `localization` / `组织本地化`, permission-controlled, and visible only to accounts with module access.
- Localization APIs now expose currencies and default localization settings through `/api/localization` with separate read and manage permissions.
- Default CNY/USD currency records and default tax settings are seeded as system configuration, not business demo data.
- Customers, suppliers, and warehouses now carry organization ownership fields; customers and suppliers also carry default currency, taxpayer ID, and invoice title fields.
- Procurement requests and sales quotations now store organization, currency, tax invoice type, and tax rate metadata at creation time.
- Sales orders inherit organization, currency, tax invoice type, and tax rate from the source quotation.
- Payables, receivables, and settlements now carry currency codes inherited from their business sources.
- The localization page now provides Chinese currency, default tax settings, and organization visibility surfaces with real API actions and empty states.
- Existing SQLite and PostgreSQL development databases are upgraded in place for localization tables and new organization/currency/tax fields during startup.
- Phase 12 frontend and backend both compile successfully after localization closure was completed.
- A dedicated manufacturing module is now registered as `manufacturing` / `制造管理`, permission-controlled, and visible only to accounts with module access.
- Manufacturing APIs now expose BOMs, work orders, production issues, and production receipts through `/api/manufacturing` with separate read, BOM manage, work order manage, and execution permissions.
- BOMs are built from live enabled item master data, and work orders derive planned component quantities from the selected BOM.
- Work orders can be released, issued to production, partially or fully completed, and remain visible through the manufacturing page lifecycle.
- Production issue deducts raw material stock balances, writes inventory movement records, persists production issue documents, and audits the execution action.
- Production completion receipt increases finished-goods stock balances, writes inventory movement records, persists production receipt documents, and advances work order completion state.
- The manufacturing page provides Chinese empty states, real BOM creation, work order creation/release, production issue, completion receipt, and execution history without seeded demo data.
- Existing SQLite and PostgreSQL development databases are upgraded in place for manufacturing tables during startup.
- Phase 13 frontend and backend both compile successfully after manufacturing closure was completed.
- A dedicated quality module is now registered as `quality` / `质量追溯`, permission-controlled, and visible only to accounts with module access.
- Quality APIs now expose source candidates, quality inspections, lot trace events, and lot trace queries through `/api/quality` with separate read, inspection manage, and traceability manage permissions.
- Quality source candidates are built from persisted procurement inventory receipts, manufacturing completion receipts, and sales inventory issues instead of seeded demo data.
- Quality inspections can be created against real source document lines and validate inspected, accepted, and rejected quantities against the source quantity.
- Lot trace events can be created against real source document lines, infer incoming, production completion, or shipment event type when needed, and can be queried by lot number in occurrence order.
- Quality inspection and lot trace event creation write audit events.
- The quality page provides Chinese empty states, source selection, inspection history, lot event history, and lot trace query without seeded demo data.
- Inventory receipt records, inventory issue records, and manufacturing completion receipt records now provide a quality-traceability navigation entry only when the user has quality module access.
- Existing SQLite and PostgreSQL development databases are upgraded in place for quality tables during startup.
- Phase 14 frontend and backend compile successfully after quality traceability closure was completed.
- Phase 14 runtime HTTP verification was previously blocked on this machine by Windows application control, but Phase 15 runtime verification later succeeded with AppHost running on `http://localhost:5099`.
- A dedicated planning module is now registered as `planning` / `计划执行`, permission-controlled, and visible only to accounts with module access.
- Planning APIs now expose planning suggestions, outsourcing orders, and barcode executions through `/api/planning` with separate read, planning manage, outsourcing manage, and barcode execute permissions.
- Planning suggestions are generated from live enabled warehouses, enabled item master data, and persisted stock balances; open suggestions can be accepted or ignored and write planning audit events.
- Outsourcing orders can be created against real warehouses and items, issued to deduct material stock, and received to increase finished-goods stock while writing inventory movement records and audit events.
- Barcode/PDA execution persists each scan result, supports stock lookup by item barcode/code, and can execute outsourcing issue or receive actions against a resolved outsourcing order.
- The planning page provides Chinese empty states, replenishment suggestion generation, suggestion decision actions, outsourcing creation/issue/receive actions, and barcode execution history without seeded demo data.
- Existing SQLite and PostgreSQL development databases are upgraded in place for planning suggestion, outsourcing, and barcode execution tables during startup.
- Phase 15 frontend and backend compile successfully after planning execution closure was completed.
- Phase 15 runtime HTTP verification succeeded: root module listing includes `planning`; admin login returns planning module and `planning.read`; planning list endpoints return successfully; barcode stock lookup with an unknown code persists a failed validation record.
- INVT-06 is now delivered through the inventory module as a narrow location/bin stock slice rather than a full WMS expansion.
- Inventory APIs now expose warehouse locations and location stock balances through `/api/inventory/locations` and `/api/inventory/location-balances`.
- A new `inventory.location.manage` permission controls warehouse location creation; platform administrator and operations manager roles receive it through the central permission catalog.
- Inventory receipt, issue, transfer, and count requests can carry optional location IDs. When a location is provided, the operation updates location stock balances, records location snapshots on inventory documents, and writes inventory movements with location context.
- Warehouse-level stock balances remain the compatibility source for existing manufacturing, planning, and non-location inventory flows.
- The inventory execution page now includes location management, optional location selectors on receipt/issue/transfer/count workflows, location stock balance visibility, and location context in movement/balance rows.
- Existing SQLite and PostgreSQL development databases are upgraded in place for warehouse locations, location stock balances, and location fields on inventory documents/movements.
- INVT-06 frontend and backend compile successfully after location/bin stock closure was completed.
- INVT-06 runtime HTTP verification succeeded: admin has `inventory.location.manage`; a location was created under an existing warehouse; location list and location balance endpoints returned successfully.
- Technical closure is complete: `SQLitePCLRaw.lib.e_sqlite3` is pinned to `3.53.3`, `Npgsql.EntityFrameworkCore.PostgreSQL` is upgraded to `10.0.3`, vulnerable/outdated package scans are clean, Vite production chunks are split below the default warning threshold, and `dotnet build AeroERP.slnx --no-restore` now reports 0 warnings and 0 errors.
- Branch `feature/localization-content-module` adds an independent language and localization content module: persisted Chinese/English content entries, `/api/localization/content` read/upsert endpoints, frontend Chinese/English switching, editable English content, translated shell/module/role labels, and removal of visible role internal keys from platform/control screens. `dotnet build AeroERP.slnx --no-restore`, `npm run build`, and runtime HTTP validation now pass using the Windows PowerShell AppHost launch path.
- GSD/beads minimal plugin foundation task `AeroERP-10f` is complete: typed module descriptors, plugin descriptor catalog, plugin schema initializer interface, core schema initializer registration, and AppHost startup execution of registered plugin schema initializers before seeding. Root API now returns `aeroerp.core` plus independent plugin descriptors.
- Windows Smart App Control / code integrity still blocks some launch paths such as PowerShell 7/direct published artifacts, but the verified local workaround is starting AppHost with `C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe` and `dotnet run --no-build --project src\AeroERP.AppHost\AeroERP.AppHost.csproj --urls http://localhost:5099`.
- The independent `aeroerp.position-permissions` plugin is implemented and verified: module key `position-permissions` / `岗位权限`, dedicated schema initializer, departments, job positions, permission packages, custom role permission grants, position-role bindings, position data scopes, `/api/position-permissions` APIs, Chinese frontend page, navigation entry, and audit events. `dotnet build AeroERP.slnx --no-restore`, `npm run build`, HTTP API validation, and `http://localhost:5173/position-permissions` route validation pass.
- The independent `aeroerp.wms` plugin backend and frontend execution console are implemented: WMS schema initializer, put-away tasks, picking tasks, picking waves, warehouse containers, warehouse routes, PDA work queue, `/api/wms` APIs, `wms.read` / `wms.manage` / `wms.execute` permissions, Chinese navigation entry, and a no-dead-button frontend covering all WMS actions. `npm run build` and `dotnet build AeroERP.slnx --no-restore` pass with 0 warnings and 0 errors after the WMS frontend work.
- WMS final HTTP/runtime verification remains blocked by bead `AeroERP-9f6`: Windows application control policy `0x800711C7` blocks AppHost from loading `AeroERP.Modules.Workflow.dll` after the WMS rebuild, even after `Unblock-File`, system Windows PowerShell, and `cmd.exe` launch attempts. Do not close `AeroERP-bku` or parent `AeroERP-v6m` until that environment blocker is resolved or explicitly accepted as a runtime-only blocker.
- The independent `aeroerp.advanced-manufacturing` plugin is implemented up to runtime validation: domain models, schema initializer, SQLite/PostgreSQL bootstrap, permissions, plugin/module catalog, default role module access, `/api/advanced-manufacturing` APIs, service audit events, Chinese frontend page, navigation entry, and build verification. It covers work centers, routings, routing operations, operation schedules, capacity loads, manufacturing cost snapshots, and MRP suggestions while reusing existing warehouses, items, stock balances, and manufacturing work orders.
- Advanced manufacturing final HTTP/runtime validation is also blocked by `AeroERP-9f6`, because AppHost cannot start on this machine while the Windows application control policy blocks `AeroERP.Modules.Workflow.dll`.
- The independent `aeroerp.reporting` plugin is implemented up to runtime validation: report definitions, report run records, report export tasks, schema initializer, SQLite/PostgreSQL bootstrap, permissions, plugin/module catalog, default role module access, `/api/reporting` APIs, service audit events, Chinese frontend page, navigation entry, and build verification. Report execution reads existing procurement, sales, inventory, finance, and manufacturing tables for aggregate summaries; it does not copy business detail tables.
- Reporting final HTTP/runtime validation is also blocked by `AeroERP-9f6`.

## Session Continuity

Last session: 2026-07-13
Stopped at: WMS frontend bead `AeroERP-wjm`, advanced manufacturing implementation beads `AeroERP-cs2` / `AeroERP-b1b` / `AeroERP-x3c`, and reporting implementation beads `AeroERP-dve` / `AeroERP-8nb` / `AeroERP-fpg` completed. WMS validation `AeroERP-bku`, advanced manufacturing validation `AeroERP-bmx`, and reporting validation `AeroERP-mzn` are blocked by `AeroERP-9f6`.
Resume file: `.planning/HANDOFF.json`
