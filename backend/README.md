# Backend API - Document Management System

A .NET 8 Web API for document management, built with ASP.NET Core, Entity Framework Core, and JWT authentication.

## Overview

This backend provides RESTful APIs for managing documents, folders, users, permissions, and workflows in a document management system. It supports file uploads to Firebase Storage, user authentication, role-based authorization, and database operations with SQL Server.

## Technologies

- **Framework**: .NET 8.0
- **Web Framework**: ASP.NET Core Web API
- **Database**: SQL Server with Entity Framework Core 8.0
- **Authentication**: JWT Bearer Tokens
- **File Storage**: Firebase Storage (configurable)
- **Email**: SMTP or MailKit
- **Testing**: xUnit, Moq
- **Documentation**: Swagger/OpenAPI
- **Containerization**: Docker with multi-stage builds

## Architecture

### Project Structure

```
backend/
├── Controllers/          # API controllers
├── Data/                # EF Core DbContext and seeders
├── Models/              # Entity models and DTOs
├── Services/            # Business logic services
├── Interfaces/          # Service contracts
├── Filters/             # Action filters
├── Helpers/             # Utility classes
├── Migrations/          # EF Core migrations
├── Wrappers/            # Response wrappers
├── Properties/          # Launch settings
└── FileStorage/         # Local file storage (dev)
```

### Key Components

- **DocumentDbContext**: EF Core context with SQL Server provider
- **JWT Authentication**: Bearer token validation with configurable issuer/audience
- **Health Checks**: `/health` endpoint with database connectivity checks
- **AutoMapper**: Object-to-object mapping for DTOs
- **Role-based Authorization**: Admin, User, and custom roles
- **File Upload**: Support for multiple storage providers

## API Endpoints

### Authentication

- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/forgot-password` - Password reset request

### Documents

- `GET /api/documents` - List documents
- `POST /api/documents` - Upload document
- `GET /api/documents/{id}` - Get document details
- `PUT /api/documents/{id}` - Update document
- `DELETE /api/documents/{id}` - Delete document
- `GET /api/documents/{id}/download` - Download document

### Folders

- `GET /api/folders` - List folders
- `POST /api/folders` - Create folder
- `PUT /api/folders/{id}` - Update folder
- `DELETE /api/folders/{id}` - Delete folder

### Users & Permissions

- `GET /api/admin/users` - List users (Admin only)
- `POST /api/admin/users` - Create user (Admin only)
- `GET /api/permissions` - Get user permissions
- `POST /api/permissions` - Grant permissions

### Tags

- `GET /api/tags` - List tags
- `POST /api/tags` - Create tag
- `PUT /api/tags/{id}` - Update tag
- `DELETE /api/tags/{id}` - Delete tag

### Health

- `GET /api/health` - Health check with database status

## Configuration

### Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CatalogDB;User Id=sa;Password=...
ConnectionStrings__DockerConnection=Server=sqlserver;Database=CatalogDB;User Id=sa;Password=...

# JWT
JwtSettings__SecretKey=your-jwt-secret-key
JwtSettings__Issuer=EdmSystem
JwtSettings__Audience=EdmClient
JwtSettings__ExpirationMinutes=60
JwtSettings__RefreshTokenExpirationDays=7

# File Storage
Firebase__StorageBucket=your-firebase-bucket
Firebase__CredentialsJson=your-firebase-credentials-json

# Email
Smtp__Provider=smtp
Smtp__Host=smtp.gmail.com
Smtp__Port=587
Smtp__Username=your-email@gmail.com
Smtp__Password=your-app-password

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80
Frontend__Url=http://localhost:4200
```

### Docker Secrets (Production)

For production deployments, sensitive values are read from Docker secrets:

- `/run/secrets/jwt_secret_key`
- `/run/secrets/sa_password`

## Database

### Migrations

Database schema is managed through EF Core migrations. Migrations are run automatically as a separate job during deployment.

To create a new migration:

```bash
dotnet ef migrations add MigrationName
```

To apply migrations manually:

```bash
dotnet ef database update
```

### Seeding

Initial data is seeded on application startup (development only). Includes default admin user and sample data.

## Health Checks

The API includes health monitoring at `/health`:

- **HTTP 200**: All checks pass
- **HTTP 503**: One or more checks fail

Checks include:

- Database connectivity
- Application responsiveness

## Testing

### Running Tests

```bash
# Run unit tests
dotnet test

# Run with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Run coverage script
.\scripts\coverage.ps1
```

### Test Coverage

Current test coverage includes:

- Controllers: Authentication, Documents, Folders, Permissions, Tags
- Services: Auth, Permissions, File Storage
- Integration tests with in-memory database
- Unit tests with mocked dependencies

**Coverage Report**: Generated using coverlet and reportgenerator. View `coverage/index.html` after running coverage script.

### Test Structure

```
backend.tests/
├── Controllers/     # Controller unit tests
├── Services/        # Service unit tests
├── Fixtures/        # Test fixtures and data
├── Helpers/         # Test utilities
└── GlobalUsings.cs  # Global using statements
```

## Development

### Prerequisites

- .NET 8.0 SDK
- SQL Server (local or Docker)
- Node.js (for frontend development)
- Docker (optional, for containerized development)

### Setup

1. Clone the repository
2. Navigate to backend directory
3. Restore packages:
   ```bash
   dotnet restore
   ```
4. Update connection strings in `appsettings.json` or user secrets
5. Run migrations:
   ```bash
   dotnet ef database update
   ```
6. Run the application:
   ```bash
   dotnet run
   ```

### Docker Development

```bash
# Build and run with Docker Compose
docker-compose -f ../docker-compose.yaml up --build

# Or use the debug compose
docker-compose -f ../docker-compose.debug.yaml up --build
```

## Build & Deployment

### Building

```bash
# Development build
dotnet build

# Production build
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish
```

### Docker

The application includes a multi-stage Dockerfile for production builds:

```bash
# Build image
docker build -t edm-backend .

# Run container
docker run -p 80:80 edm-backend
```

### CI/CD

Jenkins pipeline handles:

- Automated testing
- Docker image building and publishing
- Database migrations as separate jobs
- Swarm deployment with health checks

## Security

- JWT token authentication
- Role-based authorization
- Input validation and sanitization
- SQL injection prevention via EF Core
- CORS configuration for frontend
- Docker secrets for sensitive data in production

## Monitoring

- Health checks for container orchestration
- Structured logging with Serilog (configurable)
- Performance monitoring endpoints
- Error tracking and alerting

## Troubleshooting

### Common Issues

1. **Database Connection**: Ensure SQL Server is running and connection string is correct
2. **JWT Tokens**: Verify secret key matches between issuer and validator
3. **File Uploads**: Check Firebase credentials and bucket permissions
4. **Migrations**: Run `dotnet ef database update` if schema is out of sync

### Logs

Application logs are written to console. In production, configure logging to external providers.

## Contributing

1. Follow the existing code structure
2. Add unit tests for new features
3. Update API documentation
4. Ensure health checks pass
5. Test with Docker Compose

## License

[Add license information]

---

# Shortcuts & Commands

1. Reverse engineering data structures
   Scaffold-DbContext "Connection String" Microsoft.EntityFrameworkCore.SqlServer -ContextDir Data -OutputDir Models [(optional) -ContextNamespace NewNamespace.Data] [(optional) -Tables Categories,Customers,Employees,Orders,Products,Shippers,Suppliers] -Force -DataAnnotation
   dotnet ef dbcontext scaffold "Connection String" Microsoft.EntityFrameworkCore.SqlServer -ContextDir Data -OutputDir Models [(optional) -ContextNamespace NewNamespace.Data] [(optional) -Namespace NewNamespace.Models] [(optional) -Tables Categories,Customers,Employees,Orders,Products,Shippers,Suppliers] -Force -DataAnnotation

2. Manage secrets
   1. Right click on project -> Manage User Secrets
   2. Create an entry for the connection string

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Northwind;Trusted_Connection=True;MultipleActiveResultSets=true",
    "DockerConnection": "Data Source=127.0.0.1,1434;Initial Catalog=zeh;User ID=sa;Password=A!234567a;TrustServerCertificate=True;"
  }
}
```

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=Northwind;Trusted_Connection=True;MultipleActiveResultSets=true"

# References

https://www.youtube.com/watch?v=hpLvXNASyTI
