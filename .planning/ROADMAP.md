# Roadmap: AeroERP

## Milestone 1: Foundation and First Closed Loop

### Phase 1: Project Governance and Solution Skeleton
**Goal:** Establish the repository structure, governance artifacts, and modular solution skeleton required for disciplined implementation.
**Mode:** mvp
**Success Criteria**:
1. `.planning` artifacts and project `AGENTS.md` exist and define beads/GSD workflow plus AI-agent review rules.
2. The solution builds with backend, frontend, shared contracts, and UI style package placeholders.
3. Architectural boundaries are represented directly in the repository structure.

### Phase 2: Shared UI and Interaction Framework
**Goal:** Build the frontend shell, design system, and interaction model that all modules will use.
**Mode:** mvp
**Success Criteria**:
1. Shared UI style package exposes tokens, layout primitives, and motion utilities.
2. Frontend renders real loading, empty, form, validation, and mutation states with no fake dashboard data.
3. Navigation and page containers support animated, responsive business screens.

### Phase 3: Governance Runtime, Identity, and Plugin Center
**Goal:** Implement the platform runtime for identity, organization, audit, agent review, and plugin visibility control.
**Mode:** mvp
**Success Criteria**:
1. Organization/role model and permissions exist in persistence.
2. Plugin center supports hide/show and audit of module visibility.
3. Agent review flow records requests, decisions, and visibility in auditable storage and UI.

### Phase 4: Master Data Modules
**Goal:** Deliver live master data management for suppliers, items, and warehouses using the plugin/module architecture.
**Mode:** mvp
**Success Criteria**:
1. Supplier, item, and warehouse modules persist real data with empty-state support.
2. UI screens use shared UI package components and motion patterns.
3. Master data APIs and screens remain separated by module and shared contract boundaries.

### Phase 5: Procurement Request and Order Closed Loop
**Goal:** Deliver a complete procurement request-to-order flow with real persistence and reviewable state transitions.
**Mode:** mvp
**Success Criteria**:
1. Users can create procurement requests and submit them for review.
2. Authorized users can approve requests and convert them to procurement orders.
3. Request and order detail screens show linked lifecycle state with no dead actions.

### Phase 6: Inventory Receiving and Stock Visibility
**Goal:** Extend the procurement flow into inventory so released orders can be received into warehouses and reflected in stock balances.
**Mode:** mvp
**Success Criteria**:
1. Released procurement orders appear in a pending receipt list and can be received into enabled warehouses.
2. Inventory receipts and stock balances persist and are visible through dedicated inventory screens.
3. Procurement-to-inventory navigation, module visibility, and permission-driven actions remain closed-loop with no dead UI.

## Requirement Mapping

| Requirement | Phase |
|-------------|-------|
| PLAT-01 | Phase 1 |
| PLAT-02 | Phase 1 |
| AIGV-01 | Phase 1 |
| PLAT-03 | Phase 2 |
| PLAT-04 | Phase 2 |
| PLAT-05 | Phase 2 |
| AIGV-02 | Phase 3 |
| AIGV-03 | Phase 3 |
| IDOR-01 | Phase 3 |
| IDOR-02 | Phase 3 |
| IDOR-03 | Phase 3 |
| PLUG-01 | Phase 3 |
| PLUG-02 | Phase 3 |
| PLUG-03 | Phase 3 |
| MDAT-01 | Phase 4 |
| MDAT-02 | Phase 4 |
| MDAT-03 | Phase 4 |
| MDAT-04 | Phase 4 |
| PROC-01 | Phase 5 |
| PROC-02 | Phase 5 |
| PROC-03 | Phase 5 |
| PROC-04 | Phase 5 |
| INVT-01 | Phase 6 |
| INVT-02 | Phase 6 |

## Milestone 2: Revenue and Warehouse Execution

### Phase 7: Sales Foundation Closed Loop
**Goal:** Extend the current procurement-centric system into a real sales transaction flow with customer-facing document lifecycle and permission-aware UI.
**Mode:** mvp
**Success Criteria**:
1. Users can manage customers and create sales quotations or sales orders from live master data.
2. Sales documents expose visible lifecycle transitions and remain governed by module visibility and role permissions.
3. The frontend provides real empty, loading, validation, mutation, and linked-document states without fake summary data.

### Phase 8: Inventory Outbound, Transfer, and Counting
**Goal:** Turn inventory into a full execution module by supporting outbound, transfer, counting, and movement visibility.
**Mode:** mvp
**Success Criteria**:
1. Users can execute outbound inventory transactions from released sales or operational requests.
2. Users can transfer stock across warehouses or locations and inspect movement history.
3. Users can perform stock counting adjustments with auditable before/after balance results.

### Phase 9: Receivables, Payables, and Basic Settlement
**Goal:** Add the minimum finance settlement layer required to connect procurement and sales documents to receivable and payable records.
**Mode:** mvp
**Success Criteria**:
1. Procurement orders and receipts can generate payable-facing records.
2. Sales orders and shipments can generate receivable-facing records.
3. Users can record settlement actions and trace business documents to their financial follow-up records.

## Milestone 3: Workflow Governance and Enterprise Control

### Phase 10: Workflow Engine, Inbox, and Notifications
**Goal:** Introduce a reusable approval and work-item runtime so key document actions flow through a consistent review and notification chain.
**Mode:** mvp
**Success Criteria**:
1. Business modules can register approval definitions without embedding bespoke review logic in each page.
2. Users can process pending work in a central inbox and inspect approval history.
3. Notification events are persisted and reflected in visible UI states for relevant users.

### Phase 11: Reporting, Data Scope Permissions, and Document Numbering
**Goal:** Strengthen operational governance with cross-module analytics, scoped data access, and configurable document coding rules.
**Mode:** mvp
**Success Criteria**:
1. Procurement, sales, inventory, and platform modules expose a shared reporting contract for dashboards and tabular analytics.
2. Data scope permissions can limit visible organizations, warehouses, or document ownership beyond route-level module access.
3. Document numbering rules can be configured and applied consistently across enabled modules.

### Phase 12: Multi-Organization and Localization Base
**Goal:** Prepare the platform for more realistic enterprise deployment with multi-company, multi-currency, and localization-ready document fields.
**Mode:** mvp
**Success Criteria**:
1. Users can operate in multiple organizations or companies with scoped access boundaries.
2. Settlement and pricing flows can store and display multi-currency values.
3. Core business documents expose localization-ready tax and invoice metadata without breaking module boundaries.

## Milestone 4: Manufacturing, Quality, and Planning

### Phase 13: Manufacturing Base
**Goal:** Add the first manufacturing slice around BOM, work orders, material issue, and completion receipt.
**Mode:** mvp
**Success Criteria**:
1. Users can maintain BOM structures and create work orders from approved demand.
2. Work orders can consume stock through material issue transactions and produce finished receipts.
3. Production and inventory flows remain auditable and visible in the UI.

### Phase 14: Quality and Traceability
**Goal:** Introduce quality control checkpoints and traceable lot-based flows across purchasing, production, and shipping.
**Mode:** mvp
**Success Criteria**:
1. Incoming, in-process, or outgoing quality records can be created and linked to business documents.
2. Rejected quantities and disposition actions visibly affect downstream document actions.
3. Inventory and quality history can trace a batch or lot through receipt, movement, and release decisions.

### Phase 15: Planning, Outsourcing, and Device Execution
**Goal:** Expand from operational execution into replenishment planning, outsourced processing, and barcode-ready warehouse operations.
**Mode:** mvp
**Success Criteria**:
1. The system can produce replenishment or planning suggestions from demand and stock positions.
2. Outsourced processing can be tracked through dedicated documents and inventory impacts.
3. Warehouse execution contracts are ready for barcode or PDA-driven workflows without rewriting core module logic.
