# AeroERP Modules

This document is the production-facing map of AeroERP modules. It explains where each module lives, what it owns, and which UI surface exposes it.

## Platform Layer

| Area | Backend | UI | Responsibility |
| --- | --- | --- | --- |
| App host | `src/AeroERP.AppHost` | `apps/web/src/App.tsx` | Composes modules, endpoints, auth, persistence, and frontend routing. |
| Building blocks | `src/AeroERP.BuildingBlocks` | N/A | Shared domain primitives, module descriptors, plugin descriptors, and operation results. |
| Platform contracts | `src/AeroERP.Platform` | `PlatformPage.tsx` | Users, roles, permissions, organizations, module visibility, audit events, and agent review policy contracts. |
| Platform runtime | `src/AeroERP.Platform.Infrastructure` | Shared API client | EF Core persistence, schema bootstrapping, auth implementation, audit writing, and service implementations. |

## Business Modules

| Module | Backend project | UI page | Scope |
| --- | --- | --- | --- |
| Master Data | `AeroERP.Modules.MasterData` | `MasterDataPage.tsx` | Customers, suppliers, items, and warehouses. |
| Procurement | `AeroERP.Modules.Procurement` | `ProcurementPage.tsx` | Purchase requests, review, order creation, and release. |
| Sales | `AeroERP.Modules.Sales` | `SalesPage.tsx` | Customers, quotations, orders, and sales issue handoff. |
| Inventory | `AeroERP.Modules.Inventory` | `InventoryPage.tsx` | Receipts, issues, transfers, counts, balances, locations, movement ledger, and weighted average cost movement. |
| Finance | `AeroERP.Modules.Finance` | `FinancePage.tsx` | Accounts, periods, vouchers, payables, receivables, invoices, bank accounts, statements, settlements, and reports. |
| Workflow | `AeroERP.Modules.Workflow` | `WorkflowPage.tsx` | Workflow definitions, instances, approval tasks, and notifications. |
| Control | `AeroERP.Modules.Control` | `ControlPage.tsx` | Analytics, data scopes, role options, and numbering rules. |
| Localization | `AeroERP.Modules.Localization` | `LocalizationPage.tsx` | Currencies, tax invoice settings, localized content, and organization settings. |
| Manufacturing | `AeroERP.Modules.Manufacturing` | `ManufacturingPage.tsx` | BOMs, work orders, production issues, production receipts, and basic manufacturing cost capture. |
| Advanced Manufacturing | `AeroERP.Modules.AdvancedManufacturing` | `AdvancedManufacturingPage.tsx` | Work centers, routings, operation schedules, capacity, cost snapshots, and MRP suggestions. |
| WMS | `AeroERP.Modules.Wms` | `WmsPage.tsx` | Put-away, picking, waves, containers, warehouse routes, and PDA queue work. |
| Mobile Work | `AeroERP.Modules.MobileWork` | `MobileWorkPage.tsx` | Mobile devices, offline tasks, scan events, and mobile work queues. |
| Integration | `AeroERP.Modules.Integration` | `IntegrationPage.tsx` | Message channels, connectors, sync jobs, webhooks, and integration audit records. |
| Document Exchange | `AeroERP.Modules.DocumentExchange` | `DocumentExchangePage.tsx` | Import templates, field mappings, import batches, export tasks, print templates, print jobs, and file audits. |
| Reporting | `AeroERP.Modules.Reporting` | `ReportingPage.tsx` | Report definitions, run records, export tasks, and reporting status. |
| Quality | `AeroERP.Modules.Quality` | `QualityPage.tsx` | Quality inspections, quality document types, lot trace events, and trace chain lookup. |
| Planning | `AeroERP.Modules.Planning` | `PlanningPage.tsx` | Planning suggestions, outsourcing orders, barcode execution, and planning execution history. |
| Position Permissions | `AeroERP.Modules.PositionPermissions` | `PositionPermissionsPage.tsx` | Job positions, departments, permission packages, role grants, role bindings, and data scopes by position. |

## Frontend Packages

| Package | Scope |
| --- | --- |
| `packages/ui-style` | Shared design tokens, motion defaults, and style-level primitives. |
| `packages/ui-kit` | Shared business UI components such as page shells, section blocks, empty states, and KPI tiles. |
| `apps/web` | AeroERP web workspace, route shell, module pages, auth context, API client, and i18n context. |

## Route Metadata

Frontend module paths are centralized in `apps/web/src/modules/moduleNavigation.ts`. Route guards, default route selection, and shell navigation should use that file instead of duplicating module path logic.
