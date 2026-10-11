# Architecture

## System context

```text
Browser
  │
  ▼
Angular 17 standalone frontend
  │ HTTP
  ▼
.NET 8 Web API ── Entity Framework Core ── SQL Server
  │
  └── Swagger / OpenAPI for API exploration
```

Docker Compose supports the documented local development workflow. The README
also describes an in-memory database option for backend tests.

## Components

| Component | Responsibility | Technology |
| --- | --- | --- |
| Frontend | User interface and client-side application flow | Angular 17, Angular Material, Tailwind CSS, RxJS |
| API | REST endpoints and application services | .NET 8 Web API, C# |
| Persistence | Entities, migrations, data access | Entity Framework Core 8, SQL Server |
| Tests | Backend service/controller coverage and frontend test command | .NET test tooling, Angular test tooling |
| Local runtime | Development, debugging, and container workflow | Docker, Docker Compose |

## Key boundaries

- Controllers expose API endpoints; service and data layers hold application and
  persistence concerns.
- Swagger provides local interactive API documentation.
- Production deployment architecture is not documented or claimed by this repository.

## Historical references

See [`../doc/`](../doc/) for prior modeling, diagrams, deployment notes, and
troubleshooting records. Create an ADR only for a future choice that materially
changes these boundaries.
