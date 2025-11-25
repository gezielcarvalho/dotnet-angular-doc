# dotnet-angular-catalog

A full-stack catalog management application built with .NET 8 and Angular 17. The solution provides a RESTful API for managing products, categories, posts, tags, and users with a modern Angular frontend.

## Projects

- **Backend**: A .NET 8 Web API project with Entity Framework Core, Swagger documentation, and pagination support
- **Frontend**: An Angular 17 standalone application with Material Design, Tailwind CSS, and Font Awesome icons
- **Backend.Tests**: Unit tests for backend services and controllers

## Backend

The backend is a .NET 8 Web API with the following structure:

- **Controllers**: RESTful API endpoints for Categories, Posts, PostTags, Products, Tags, and Users
- **Data**: Entity Framework Core DbContext and database migrations
- **Models**: Domain entities and DTOs (Data Transfer Objects)
- **Services**: Business logic and service layer implementations
- **Filters**: Request filtering (pagination)
- **Helpers**: Utility classes for pagination and stored procedures
- **Wrappers**: Response wrappers for consistent API responses

**Key Features:**

- Entity Framework Core 8 with SQL Server support
- Swagger/OpenAPI documentation
- Pagination support
- In-memory database option for testing
- Database migrations included

## Frontend

The frontend is an Angular 17 standalone application with modern features:

- **Standalone Components**: Built with Angular 17's standalone architecture (no NgModules)
- **Angular Material**: Material Design components for UI
- **Tailwind CSS**: Utility-first CSS framework for styling
- **Font Awesome**: Icon library integration
- **RxJS**: Reactive programming for async operations
- **TypeScript 5.2**: Latest TypeScript features

**Project Structure:**

- **src/app**: Components, services, and routing configuration
- **src/assets**: Static assets and images
- **src/styles.css**: Global styles with Tailwind directives

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for containerized development)
- Visual Studio 2022 or VS Code (optional)

### Running with Docker (Recommended)

The easiest way to run the application is using Docker Compose:

```bash
# Start both frontend and backend in development mode with hot reload
docker-compose up

# Or run in detached mode
docker-compose up -d

# View logs
docker-compose logs -f

# Stop containers
docker-compose down
```

The application will be available at:

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5175
- **Swagger UI**: http://localhost:5175/swagger

#### Debug Mode

For debugging with VS Code or Visual Studio:

```bash
# Start containers in debug mode (keeps containers running for debugger attachment)
docker-compose -f docker-compose.debug.yaml up
```

### Running Locally (Without Docker)

#### Backend

```bash
cd backend
dotnet restore
dotnet run
```

The API will be available at http://localhost:5175 with Swagger documentation at http://localhost:5175/swagger

#### Frontend

```bash
cd frontend
npm install
npm start
```

The application will be available at http://localhost:4200

### Running Tests

```bash
# Backend tests
cd backend.tests
dotnet test

# Frontend tests
cd frontend
npm test
```

## Docker Configuration

The project includes comprehensive Docker support:

- **Dockerfile** (Backend): Multi-stage build with development and production targets
- **Dockerfile** (Frontend): Node.js development with nginx production build
- **docker-compose.yaml**: Development environment with hot reload
- **docker-compose.debug.yaml**: Debug configuration for IDE attachment
- **.dockerignore**: Optimized to exclude unnecessary files

**Features:**

- Hot reload for both frontend and backend during development
- Volume mounting for source code changes
- Persistent volumes for node_modules and NuGet packages
- Debugger support in development containers
- Production-ready nginx configuration for frontend
- Network isolation with custom bridge network

## Database

The application uses Entity Framework Core with migrations. By default, it's configured to use SQL Server, but can also use an in-memory database for testing.

To apply migrations:

```bash
cd backend
dotnet ef database update
```

## API Documentation

Once the backend is running, visit http://localhost:5175/swagger for interactive API documentation powered by Swagger/OpenAPI.

## References

**Original Inspiration:**

- [AngularDotNetSolution](https://github.com/gezielcarvalho/AngularDotNetSolution)
- [DSCatalog Design](https://www.figma.com/file/1n0aifcfatWv9ozp16XCrq/DSCatalog-Bootcamp)
- [Generate Test Data](https://www.generatedata.com/)
- [Pagination in ASP.NET Core](https://codewithmukesh.com/blog/pagination-in-aspnet-core-webapi/)

**Angular Standalone Components:**

- [Angular Standalone Components Unleashed](https://blogs.halodoc.io/angular-standalone-components-unleashed-exploring-the-magic-of-a-world-without-ngmodule/)
- [Standalone Angular Applications](https://dev.to/this-is-angular/angular-revisited-standalone-angular-applications-the-replacement-for-ngmodules-238m)

**Additional Resources:**

- [Docker Tutorial](https://www.youtube.com/watch?v=5ZLmcDi30YI)
- [Advanced Docker](https://www.youtube.com/watch?v=PR7xz5vQKGg)

## License

This project is for educational and demonstration purposes.
