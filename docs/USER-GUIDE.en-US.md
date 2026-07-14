# AeroERP User Guide

This guide explains how to run AeroERP locally and how to review the main operating flows.

## 1. Start The System

Start the backend:

```powershell
.\scripts\start-apphost-single.ps1 -StopExisting
```

Backend endpoints:

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

## 2. Recommended First Steps

1. Open Platform Governance and review users, roles, permissions, organizations, visible modules, audit events, and agent-review requests.
2. Open Master Data and maintain suppliers, customers, items, and warehouses.
3. Open Localization and confirm currencies, tax invoice types, tax rates, invoice titles, and organization defaults.

## 3. Procurement To Inventory

1. Create a procurement request.
2. Approve the request.
3. Convert the approved request to a procurement order.
4. Release the order.
5. Open Inventory and review pending procurement orders.
6. Create an inventory receipt.
7. Review stock balances, location balances, movements, and inventory ledger amounts.
8. Open Finance and generate payables from the receipt or order.

## 4. Sales To Shipment

1. Create a sales quotation.
2. Convert the quotation to a sales order.
3. Confirm the sales order.
4. Mark the order ready to ship.
5. Open Inventory and review pending sales orders.
6. Create an inventory issue.
7. Review stock balance and issue cost.
8. Open Finance and generate receivables from the issue or order.

## 5. Finance Flow

1. Create or review accounting accounts.
2. Create an accounting period.
3. Generate vouchers from payables, receivables, or settlements.
4. Submit vouchers for review.
5. Approve or reject vouchers.
6. Create bank accounts.
7. Enter bank statement lines.
8. Settle payables or receivables.
9. Reconcile bank statement lines with settlements.
10. Review aging, trial balance, income statement, and balance-sheet base views.

The system blocks period closing when draft or pending-review vouchers still exist.

## 6. Manufacturing, Planning, And WMS

Manufacturing flow:

1. Maintain BOM records.
2. Create a work order.
3. Release the work order.
4. Issue materials.
5. Receive finished goods.
6. Review work-order cost and finished-goods inventory cost.

Planning can generate replenishment suggestions from real warehouses, items, and balances. It also supports outsourcing material issue, outsourcing receipt, and barcode execution records.

WMS supports containers, routes, put-away tasks, picking tasks, and picking waves.

## 7. Workflow, Notifications, And Quality

- Workflow handles approval tasks and workflow instances.
- Notifications expose read-state handling.
- Quality creates inspection records and lot-trace events from real receipt, production, and shipment sources.
- Lot trace queries show event chains by lot number.

## 8. Permissions And Module Visibility

AeroERP screens and actions are permission-aware:

- No read permission means no business data is exposed.
- No manage permission means actions are hidden or rendered as read-only states.
- Hidden modules disappear from navigation and entry points.
- Agent actions must be reviewable, auditable, and permission-controlled.

## 9. Screenshots

Screenshots are stored in:

```text
docs/images/
```

See:

```text
docs/SCREENSHOTS.md
```
