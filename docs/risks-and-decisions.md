# Risks and decisions

## Assumptions

- The README's .NET, Node.js, Docker, and endpoint instructions represent the
  intended local experience and should be revalidated before presenting a live demo.
- Existing `doc/` records are useful history but may describe previous states.

## Risk register

| Risk | Likelihood | Impact | Mitigation | Status |
| --- | --- | --- | --- | --- |
| Documentation drifts from the runnable solution | Medium | Medium | Verify setup/test commands whenever the stack changes | Open |
| Demo setup contains machine-specific configuration | Medium | Medium | Document required local configuration without committing secrets | Open |
| Historical records are mistaken for current architecture | Medium | Low | Keep the `docs/` / `doc/` boundary explicit | Mitigated |
| Portfolio claims exceed verified evidence | Low | High | Describe only tested capabilities and approved deployment state | Open |

## Decision log

| Date | Decision | Rationale |
| --- | --- | --- |
| 2026-10-11 | Add `docs/` without replacing `doc/` | Preserve project history while providing a concise current-state layer |
| 2026-10-11 | Keep deployment out of the showcase baseline | Hosting, secrets, data, and operating ownership are not yet approved |
