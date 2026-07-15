# AeroERP

## What This Is

AeroERP is a new modular ERP platform for Chinese enterprise operations, built as a .NET modular monolith with a separate React frontend and an isolated UI style library. It is designed for teams that need strong architectural boundaries, plugin-based business modules, auditable AI-assisted delivery, and real interactive screens that either show live data or meaningful empty states.

## Core Value

Deliver a modular ERP foundation whose business flows, plugin boundaries, UI behavior, and AI-agent governance remain clear, auditable, and closed-loop from click to persisted result.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] Build a new standalone AeroERP project instead of extending the legacy WebVella codebase directly.
- [ ] Enforce beads-first execution and GSD planning artifacts inside the project.
- [ ] Structure backend business capabilities as class libraries or plugins with clear ownership and dependency direction.
- [ ] Split UI implementation, UI design tokens, and business modules into separate deliverables with explicit boundaries.
- [ ] Require every AI agent usage to be reviewed, auditable, and documented inside the project.
- [ ] Support plugin/module visibility toggling with permission control.
- [ ] Avoid seeded demo/test data; screens must bind to live data sources or render meaningful empty states.
- [ ] Ensure no dead-end UI interactions; every rendered command must produce a visible state change, navigation change, validation response, or persisted mutation.
- [ ] Deliver the first working operational slice with a closed loop around organizational control, master data, procurement request, and procurement order handling.

### Out of Scope

- Extending the old WebVella ERP solution in place — this project must be a clean new implementation.
- Full finance, manufacturing, payroll, and tax platform in v1 — these will be staged as later modules after the foundation is stable.
- Fake dashboards and hardcoded counts — forbidden because they create dead UX and break trust.

## Context

The project starts in a greenfield directory under the current workspace and uses the prior WebVella review only as architectural input, not as an implementation base. The intended system should behave like a modular enterprise ERP platform with pluginized business domains, a separate UI theme package, and explicit governance for AI-assisted engineering. Chinese enterprise ERP priorities are driving the module roadmap: organization, permissions, document flows, procurement, sales, inventory, receivables/payables, and later finance localization.

## Constraints

- **Tech stack**: .NET 10 + ASP.NET Core + PostgreSQL + React/Vite — approved by the user as the default project foundation.
- **Architecture**: Modular monolith with plugin/class-library modules — required to keep responsibilities clear and prevent god-code.
- **UI separation**: Visual system must live in a dedicated library/package — required so UI style changes do not leak into business modules.
- **Data policy**: No seeded demo/test data in user-facing screens — UI must use real persistence or explicit empty states.
- **Agent governance**: Every AI agent action must be reviewable and auditable — required by project policy and AGENTS.md.
- **Interaction quality**: No dead clicks or dead screens — every UI element must have a meaningful outcome.
- **Module closure**: Upstream/downstream flows must remain closed-loop — no incomplete handoffs between request, approval, order, and persistence states.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Use a new `AeroERP` repository root | Avoid coupling to legacy architecture and preserve clean boundaries | — Pending |
| Use a modular monolith backend | Faster coordinated delivery than microservices while preserving clear ownership | — Pending |
| Use separate libraries for modules, contracts, and UI style | Keeps business logic, presentation, and shared visuals isolated | — Pending |
| Make AI-agent review a first-class platform concern | User explicitly requires audited intelligent-agent usage | — Pending |
| Start with organization + plugin center + master data + procurement loop | Smallest slice that proves architecture and business closure | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition**:
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone**:
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-07-07 after initialization*
