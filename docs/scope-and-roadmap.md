# Scope and roadmap

## In scope

- REST API management of categories, posts, post tags, products, tags, and users.
- Angular standalone frontend for the application experience.
- Entity Framework Core persistence with SQL Server and an in-memory option for tests.
- Local Docker Compose development and backend/frontend test commands described in the README.
- Current-state documentation for a portfolio reviewer or contributor.

## Out of scope

- Claiming production readiness, availability guarantees, or a hosted service.
- Changing product features, deployments, dependencies, or infrastructure as part of this documentation work.
- Rewriting the retained legacy records in `doc/`.

## Modernization direction

| Priority | Outcome | Evidence needed before work begins |
| --- | --- | --- |
| P1 | Keep the README, architecture, and runnable workflow aligned | Confirm current compose and test commands on a clean environment |
| P2 | Improve showcase quality where a verified gap exists | Scoped issue, acceptance criteria, and validation plan |
| P3 | Consider hosting only after security, secrets, data, and cost ownership are defined | Human approval and deployment plan |

Live tasks belong in GitHub Issues and the portfolio project, not this document.
