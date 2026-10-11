# Operations and runbooks

## Current operational posture

This repository is a portfolio and educational showcase. It has **no approved
production deployment, hosting target, or operating owner** recorded here.

## Local operations

Use the repository README for local Docker Compose, backend, frontend, and test
commands. Treat those commands as the supported development workflow.

## Historical deployment material

Prior Docker Swarm, Jenkins, Nginx, Firebase, backup, and deployment records
are preserved in [`legacy/deployment/`](legacy/deployment/). They are not
current instructions and must not be run, configured, or treated as approved
infrastructure without a separately scoped review.

The retained material may reference old paths, branches, component versions,
placeholder values, or infrastructure that does not exist. It is kept for
learning and traceability only.

## Before any future deployment

- [ ] Confirm a business owner, environment owner, and operating budget.
- [ ] Review all secrets and external integrations; use a managed secret store.
- [ ] Validate the target stack, health checks, backups, restoration, logging,
  monitoring, and rollback in a non-production environment.
- [ ] Record the approved runbook and release gate in a new delivery issue.
