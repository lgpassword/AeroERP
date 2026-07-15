# AeroERP

AeroERP is a modular ERP workspace built with a .NET backend and a React/Vite web console. It is designed as a new implementation rather than a direct runtime copy of WebVella ERP, with clear boundaries between platform governance, business modules, shared UI packages, verification tools, and documentation.

## What It Provides

- Platform governance: users, roles, permissions, visible modules, organizations, audit events, and agent-review controls.
- Master data: suppliers, customers, items, warehouses, and organization/currency/tax metadata.
- Procurement: request submission, approval, order conversion, order release, and receipt handoff.
- Sales: quotation, order conversion, confirmation, ready-to-ship state, and inventory issue handoff.
- Inventory and WMS: receipts, issues, transfers, counts, stock balances, warehouse locations, ledger costing, put-away, picking, waves, containers, and route records.
- Finance: accounts, periods, vouchers, payables, receivables, tax invoices, bank accounts, statements, settlements, reconciliation, aging, trial balance, income statement, and balance-sheet base views.
- Workflow and notifications: approval tasks, workflow instances, and notification read-state handling.
- Manufacturing and planning: BOM, work orders, material issue, finished-goods receipt, routing, capacity, MRP suggestions, outsourcing, and barcode execution.
- Reporting, quality, integration, document exchange, localization, mobile work, control, and position permission modules.

User-facing screens do not ship seeded demo business data. Pages either read persisted data or show explicit empty states with next actions.

## Screenshots

Screenshots are stored in [docs/images](docs/images). If they are not present in your local copy yet, run the application and follow [docs/SCREENSHOTS.md](docs/SCREENSHOTS.md) to regenerate them.

## Repository Layout

| Path | Purpose |
| --- | --- |
| `src/` | .NET backend source code, including AppHost, platform libraries, building blocks, and business modules. |
| `apps/web/` | Main React web console. |
| `packages/ui-style/` | Shared design tokens, styling, and motion primitives. |
| `packages/ui-kit/` | Shared business UI components. |
| `tests/` | Backend integration/smoke tests. |
| `tools/` | Validation tools for finance reports and inventory/manufacturing cost flows. |
| `scripts/` | Local start and verification scripts. |
| `docs/` | Architecture, module, usage, and collaboration documentation. |

## Prerequisites

- .NET SDK matching [global.json](global.json), currently `10.0.204`.
- Node.js and npm compatible with the checked-in lockfile.
- Windows PowerShell for the provided scripts.
- Optional: PostgreSQL for a persistent shared database. Without PostgreSQL, development uses local SQLite.

## Build

```powershell
dotnet restore AeroERP.slnx
dotnet build AeroERP.slnx --no-restore --disable-build-servers
npm install
npm run build
```

The front-end build runs the shared UI packages first and then builds the web application.

## Run Locally

Start the backend:

```powershell
.\scripts\start-apphost-single.ps1 -StopExisting
```

The backend listens on:

- API: `http://localhost:5099/api`
- Swagger: `http://localhost:5099/swagger`

Start the web console:

```powershell
npm install
npm run dev --workspace @aeroerp/web -- --host 0.0.0.0 --port 5173
```

Open:

```text
http://localhost:5173
```

Default bootstrap account:

```text
User name: admin
Password: Admin@123456
```

## Verification

```powershell
.\scripts\verify.ps1
```

The script restores and builds the backend, runs AppHost tests, and builds the front-end workspace.

For isolated service-level checks:

```powershell
dotnet run --project tools\AeroERP.FinanceReportValidation\AeroERP.FinanceReportValidation.csproj
dotnet run --project tools\AeroERP.InventoryCostValidation\AeroERP.InventoryCostValidation.csproj
```

## Basic Operating Flow

1. Sign in with the bootstrap administrator account.
2. Create or review organizations, roles, users, and visible modules in Platform.
3. Maintain master data: suppliers, customers, items, warehouses, locations, currencies, and tax settings.
4. Run procurement: create a request, approve it, convert it to an order, release it, and receive it into inventory.
5. Run sales: create a quotation, convert it to an order, confirm it, mark it ready to ship, and issue inventory.
6. Review inventory balances, movement history, location balances, and inventory ledger cost amounts.
7. Generate payables or receivables from real business documents, settle through bank accounts, reconcile bank statement lines, and review finance reports.
8. Use workflow, quality, planning, WMS, mobile work, reporting, integration, and document exchange modules as needed.

More detail is available in [docs/USER-GUIDE.zh-CN.md](docs/USER-GUIDE.zh-CN.md).

## Collaboration

Contributions are welcome, but direct uploads to the protected main branch are not allowed. Use feature branches and pull requests. See [CONTRIBUTING.md](CONTRIBUTING.md) and [docs/GITHUB-GOVERNANCE.md](docs/GITHUB-GOVERNANCE.md).

Required expectations:

- Do not commit logs, local databases, build outputs, `node_modules`, `.artifacts`, or local IDE caches.
- Keep module boundaries intact.
- Add or update documentation when module behavior changes.
- Verify backend and front-end builds before requesting review.
- All AI/agent-related behavior must be reviewable, auditable, and permission-controlled.

## License

MIT.
