# Requirements: AeroERP

**Defined:** 2026-07-07
**Core Value:** Deliver a modular ERP foundation whose business flows, plugin boundaries, UI behavior, and AI-agent governance remain clear, auditable, and closed-loop from click to persisted result.

## v1 Requirements

### Platform Foundation

- [x] **PLAT-01**: Developer can run the full system locally with backend, database, and frontend from the new AeroERP repository.
- [x] **PLAT-02**: Backend modules are packaged as separate class libraries/plugins with explicit dependency direction toward shared contracts/core only.
- [x] **PLAT-03**: Frontend business screens consume backend APIs or render explicit empty states without seeded demo records.
- [x] **PLAT-04**: UI style tokens, motion primitives, and shared visual components live in a dedicated UI library separated from business screens.
- [x] **PLAT-05**: Every visible UI command produces a state change, mutation result, validation result, or navigation result; no dead interactions are allowed.

### Agent Governance

- [x] **AIGV-01**: Project-level AGENTS.md defines mandatory review and audit rules for every intelligent-agent usage.
- [x] **AIGV-02**: System stores auditable records for agent review requests and outcomes.
- [x] **AIGV-03**: Users with permission can review, approve, or reject agent actions through the UI.

### Identity and Organization

- [x] **IDOR-01**: Authorized users can manage organizations, users, and roles.
- [x] **IDOR-02**: Module visibility can be controlled per plugin/module and permission scope.
- [x] **IDOR-03**: Navigation only shows modules the current user is allowed to see.

### Plugin Center

- [x] **PLUG-01**: Administrators can register installed modules/plugins in a central plugin catalog.
- [x] **PLUG-02**: Administrators can hide or show a plugin/module without deleting its implementation.
- [x] **PLUG-03**: Plugin visibility changes are reflected immediately in the UI and audited.

### Master Data

- [x] **MDAT-01**: Users can manage suppliers.
- [x] **MDAT-02**: Users can manage items/materials.
- [x] **MDAT-03**: Users can manage warehouses.
- [x] **MDAT-04**: Master data screens support empty-state UX and real create/edit persistence.

### Procurement

- [x] **PROC-01**: Users can create and submit procurement requests.
- [x] **PROC-02**: Authorized users can review procurement requests and convert approved ones into procurement orders.
- [x] **PROC-03**: Users can manage procurement orders with visible status transitions.
- [x] **PROC-04**: Procurement request and order screens form a closed business loop with real persistence and traceable relationships.

## v2 Requirements

### Sales

- [x] **SALE-01**: Users can manage customers.
- [x] **SALE-02**: Users can create sales quotations and sales orders.
- [x] **SALE-03**: Users can move sales orders through shipping-related lifecycle states.
- [x] **SALE-04**: Users can trace sales orders into outbound inventory transactions.
- [x] **SALE-05**: Users can inspect basic sales execution analytics without seeded metrics.

### Inventory

- [x] **INVT-01**: Users can receive released procurement orders into inventory and generate receipt records.
- [x] **INVT-02**: Users can inspect stock balances by warehouse and item.
- [x] **INVT-03**: Users can perform outbound and transfer inventory transactions.
- [x] **INVT-04**: Users can perform stock counting and produce auditable balance adjustments.
- [x] **INVT-05**: Users can inspect inventory movement history by document, warehouse, and item.
- [x] **INVT-06**: Users can manage location or bin-level stock when the warehouse module enables it.

### Finance and Settlement

- [x] **FINA-01**: Users can manage receivables and payables.
- [x] **FINA-02**: Users can post voucher-ready accounting events from business documents.
- [x] **FINA-03**: Users can record settlement actions against receivable and payable documents.
- [x] **FINA-04**: Core business documents can carry tax, invoice, and currency metadata without fake defaults.

### Workflow and Governance

- [x] **WFLO-01**: Business modules can register approval workflows without duplicating workflow code per module.
- [x] **WFLO-02**: Users can process pending approvals in a central inbox and inspect approval history.
- [x] **WFLO-03**: Business events can trigger persisted user-facing notifications.

### Reporting and Control

- [x] **RPTG-01**: Users can inspect procurement, sales, inventory, and settlement analytics through live reporting endpoints.
- [x] **RPTG-02**: Administrators can configure document numbering rules per module.
- [x] **RPTG-03**: The platform can restrict data visibility by organization, warehouse, or ownership scope.

### Manufacturing, Quality, and Planning

- [x] **MFQP-01**: Users can manage BOMs and create work orders.
- [x] **MFQP-02**: Users can execute production issue and completion flows against inventory.
- [x] **MFQP-03**: Users can create quality inspection records tied to procurement, production, or shipment documents.
- [x] **MFQP-04**: Users can trace lot or batch movement across receipt, storage, production, and shipment.
- [x] **MFQP-05**: Users can generate planning or replenishment suggestions from demand and stock data.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Demo charts with fabricated metrics | Forbidden by product rule against dead data |
| Full manufacturing/MRP in v1 | Too broad for the first architectural slice |
| Full Chinese tax and invoice platform in v1 | Important later, but not needed to validate the core architecture |
| Mobile app | Web-first delivery is the current priority |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| PLAT-01 | Phase 1 | Delivered |
| PLAT-02 | Phase 1 | Delivered |
| PLAT-03 | Phase 2 | Delivered |
| PLAT-04 | Phase 2 | Delivered |
| PLAT-05 | Phase 2 | Delivered |
| AIGV-01 | Phase 1 | Delivered |
| AIGV-02 | Phase 3 | Delivered |
| AIGV-03 | Phase 3 | Delivered |
| IDOR-01 | Phase 3 | Delivered |
| IDOR-02 | Phase 3 | Delivered |
| IDOR-03 | Phase 3 | Delivered |
| PLUG-01 | Phase 3 | Delivered |
| PLUG-02 | Phase 3 | Delivered |
| PLUG-03 | Phase 3 | Delivered |
| MDAT-01 | Phase 4 | Delivered |
| MDAT-02 | Phase 4 | Delivered |
| MDAT-03 | Phase 4 | Delivered |
| MDAT-04 | Phase 4 | Delivered |
| PROC-01 | Phase 5 | Delivered |
| PROC-02 | Phase 5 | Delivered |
| PROC-03 | Phase 5 | Delivered |
| PROC-04 | Phase 5 | Delivered |
| INVT-01 | Phase 6 | Delivered |
| INVT-02 | Phase 6 | Delivered |
| INVT-03 | Phase 8 | Delivered |
| SALE-01 | Phase 7 | Delivered |
| SALE-02 | Phase 7 | Delivered |
| SALE-03 | Phase 7 | Delivered |
| SALE-04 | Phase 8 | Delivered |
| SALE-05 | Phase 11 | Delivered |
| INVT-04 | Phase 8 | Delivered |
| INVT-05 | Phase 8 | Delivered |
| INVT-06 | Phase 16 | Delivered |
| FINA-01 | Phase 9 | Delivered |
| FINA-02 | Phase 9 | Delivered |
| FINA-03 | Phase 9 | Delivered |
| FINA-04 | Phase 12 | Delivered |
| WFLO-01 | Phase 10 | Delivered |
| WFLO-02 | Phase 10 | Delivered |
| WFLO-03 | Phase 10 | Delivered |
| RPTG-01 | Phase 11 | Delivered |
| RPTG-02 | Phase 11 | Delivered |
| RPTG-03 | Phase 11 | Delivered |
| MFQP-01 | Phase 13 | Delivered |
| MFQP-02 | Phase 13 | Delivered |
| MFQP-03 | Phase 14 | Delivered |
| MFQP-04 | Phase 14 | Delivered |
| MFQP-05 | Phase 15 | Delivered |

**Coverage:**
- delivered requirements: 48 total
- planned future requirements: 0 total
- unmapped: 0 ✓

---
*Requirements defined: 2026-07-07*
*Last updated: 2026-07-12 after INVT-06 location/bin stock closure*
