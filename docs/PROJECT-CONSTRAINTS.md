# AeroERP Project Constraints

These constraints are part of the project contract. New code, documentation, and cleanup work should follow them.

## Source Structure

- Keep runtime code inside production source folders: `src/`, `apps/`, `packages/`, `tests/`, `tools/`, and `scripts/`.
- Keep backend responsibilities split by project boundary:
  - `AeroERP.BuildingBlocks.*` for shared low-level primitives.
  - `AeroERP.Platform.*` for platform contracts and runtime policy.
  - `AeroERP.Modules.*` for business modules.
  - `AeroERP.AppHost` for composition and startup only.
- Keep frontend responsibilities split between the UI style package, the UI kit, and the web app shell.
- Avoid lateral business-module dependencies. Modules should depend on platform contracts and shared building blocks.

## Product Rules

- User-facing screens must show persisted data or an explicit empty state with a useful next action.
- Do not seed demo data into user-facing screens.
- Do not ship dead UI. Every visible command must navigate, mutate state, validate input, open or close a real workflow surface, or persist a backend result.
- New modules need both backend behavior and usable UI surfaces.
- Upstream and downstream business flows should be closed-loop, with the next business effect visible to the user.

## Code Quality

- Prefer direct, readable code over compatibility layers or broad abstractions.
- Add comments only where the code is not self-explanatory or where a business rule would otherwise be hard to infer.
- Remove template leftovers, unused assets, and generated artifacts from the source tree.
- Keep validation tools and tests when they prove important business flows.
- Generated outputs such as `dist/`, `bin/`, `obj/`, `.artifacts/`, `.vs/`, and logs should stay out of the source tree.

## Governance

- Module visibility changes must be permission-controlled and audited.
- Hidden modules must disappear from navigation and entry points for unauthorized users while preserving persisted data integrity.
- Any intelligent-agent workflow must persist review requests, reviewer decisions, timestamps, actors, and review state.
- Final execution of agent-driven actions must remain permission controlled and must not auto-approve by default.

## Documentation

- Keep `README.md` for operating the project.
- Keep `docs/MODULES.md` for module ownership and UI mapping.
- Keep this document for project constraints.
- Avoid tool-specific AI instruction files in the production source tree unless they are required by the project workflow.
