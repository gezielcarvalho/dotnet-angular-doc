# Delivery and quality

## Delivery workflow

1. Create a scoped GitHub Issue with objective, exclusions, acceptance criteria,
   validation, risks, and human gates.
2. Add owner-approved work to **Personal Portfolio & Engineering Labs**.
3. Implement through a pull request with evidence appropriate to the change.
4. Obtain review and human acceptance before treating work as complete.
5. Treat deployment as a separate, explicitly authorized decision.

## Definition of Done

- [ ] Acceptance criteria are met.
- [ ] Relevant backend and/or frontend tests have been run, or a clear reason is recorded.
- [ ] README and `docs/` material is current when behavior or architecture changes.
- [ ] The smallest practical rollback is known, or `Not required` is explained.
- [ ] Required review and acceptance have occurred.

## Current validation commands

```bash
# Backend
cd backend.tests
dotnet test

# Frontend
cd frontend
npm test
```

Run the Docker Compose workflow documented in the repository README when a
change affects integration or local runtime behavior.

## Release boundary

This showcase has no approved production deployment path. Do not add secrets,
hosting, external data, or production claims without a separately approved issue.
