# Backend Dockerfile with debugging support
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
EXPOSE 80

# Install debugger
RUN apt-get update && apt-get install -y unzip && \
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

# Development stage
FROM base AS development
WORKDIR /src
COPY ["Backend.csproj", "./"]
RUN dotnet restore "Backend.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Backend.csproj" -c Debug -o /app/build

# Keep container running and enable hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--project", "Backend.csproj", "--urls", "http://+:80", "--no-launch-profile"]

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
EXPOSE 80
COPY --from=development /app/build .
ENTRYPOINT ["dotnet", "Backend.dll"]