# AeroERP Agent Rules

## Workflow

- Use `beads` as the primary task tracker for all non-trivial work.
- Keep `.planning/` artifacts current when scope, architecture, or phase boundaries change.
- Do not start implementation without first creating or claiming the relevant bead.
- Close completed beads before ending a session.

## Architecture Guardrails

- Treat AeroERP as a new system. Do not copy application runtime code from `WebVella-ERP-master` unless explicitly approved and reviewed for fit.
- Keep backend responsibilities split across dedicated class libraries:
  - `AeroERP.BuildingBlocks.*` for shared low-level concerns
  - `AeroERP.Platform.*` for cross-cutting platform runtime
  - `AeroERP.Modules.*` for business modules
  - `AeroERP.AppHost` for composition only
- Keep frontend responsibilities split across dedicated packages/apps:
  - UI style/token/motion library
  - Shared business UI component package
  - Main web app shell
- Modules/plugins must depend inward on shared contracts and building blocks only. Avoid lateral module dependencies.

## Product Rules

- No seeded demo/test data in user-facing screens.
- A screen may render:
  - live persisted data, or
  - an explicit empty state with clear next actions.
- Never ship dead UI. Every clickable command must cause:
  - navigation,
  - a state mutation,
  - a validation response,
  - a modal/drawer state change tied to a real workflow, or
  - a persisted backend result.
- Every new module must include its own usable UI surfaces, not only backend code.
- Upstream/downstream flows must stay closed-loop. Do not add buttons or states that terminate without a defined next business effect.

## AI Agent Review Policy

- Every intelligent-agent usage must be reviewable and auditable.
- Any feature involving AI/agent behavior must:
  - persist review requests,
  - persist reviewer decisions,
  - record timestamps and actors,
  - expose review state in the UI,
  - restrict final execution behind permission checks.
- Do not auto-approve agent actions by default.
- New agent integrations must update both the runtime policy implementation and the UI review surfaces.

## Plugin Visibility Policy

- Plugins/modules must support visible/hidden state.
- Visibility changes must be permission-controlled and audited.
- Hidden modules must disappear from navigation and entry points for unauthorized users without breaking persistence integrity.

## UI Standards

- UI style primitives, tokens, and motion belong in the UI style library, not inside business modules.
- Business screens may compose shared UI components but must not redefine the visual system ad hoc.
- Use restrained enterprise UI styling with meaningful motion:
  - route transitions,
  - drawer/modal entry and exit,
  - optimistic and completion feedback,
  - empty-state to populated-state changes.

## Verification

- Before marking work complete, verify:
  - the relevant module builds,
  - the UI path is navigable,
  - empty states are coherent,
  - commands are not dead,
  - permissions and visibility rules hold,
  - audit events are written where required.
