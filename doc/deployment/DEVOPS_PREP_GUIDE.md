# DevOps Preparation & Deployment Guide

**Purpose:** prepare and standardize application packaging and deployment for multiple technologies (dotnet, java, laravel/php, html/static, vue, angular) and databases (MySQL, SQL Server) on the existing Docker-based infrastructure.

**Scope:** build, containerize, configure, CI/CD, and deploy-ready checklists for each application type.

## 1. Requirements & Prerequisites

- Infrastructure: Docker Engine, Docker Compose, Docker Swarm (or Kubernetes if preferred), nginx reverse-proxy, Jenkins (or other CI), container registry (private or Docker Hub), monitoring (Prometheus/Grafana or similar).
- Host-level: ports open for ingress (80/443), firewall rules, adequate disk for images/volumes, backups path.
- Secrets store: Docker secrets, Jenkins credentials, or external secret manager.

Reference: see doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md for infrastructure provisioning.

## 2. Common Patterns (all apps)

- Build artifact in CI, produce a small runtime container image.
- Use environment variables for configuration. Do not commit secrets.
- Expose a health endpoint (`/health`, `/healthz`) and readiness probes.
- Use non-root user in containers when possible.
- Write a small `Dockerfile`, `.dockerignore`, and a `docker-compose.yml` for local dev.
- Provide a Helm chart or Docker Stack file for production (we use Docker Swarm stacks in this infra).
- Mount persistent volumes for uploads and database data.

Checklist for each app before deployment:

- Build script (`build.sh` or CI pipeline) that creates a production artifact.
- `Dockerfile` tuned for multi-stage build and minimal runtime image.
- `health` endpoint implemented and documented.
- `README` with runtime env vars and ports.
- DB migration script or containerized migration job.

## 3. Per-Technology Preparation

### 3.1 .NET (Core / 6 / 7 / 8+)

- Build: `dotnet publish -c Release -o out` in CI.
- Dockerfile (multi-stage): build SDK stage -> runtime image (mcr.microsoft.com/dotnet/aspnet:8.0).
- Config: use `appsettings.{Environment}.json` + environment overrides. Read connection strings from env vars like `CONNECTION_STRING`.
- Migrations: run `dotnet ef database update` as a one-off job in deployment or at container startup with a guarded script.
- Health: `/health` using AspNetCore HealthChecks.

Example Dockerfile snippet:

```
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENV ASPNETCORE_URLS=http://+:80
HEALTHCHECK --interval=30s --timeout=5s CMD curl -f http://localhost/health || exit 1
ENTRYPOINT ["dotnet","YourApp.dll"]
```

### 3.2 Java (Spring Boot / JAR or WAR)

- Build: `mvn -DskipTests package` or `./gradlew bootJar` in CI.
- Dockerfile: build stage to produce a fat JAR, runtime image using `eclipse-temurin:17-jre` or similar.
- Pass JVM options via env var `JAVA_OPTS`.
- Health: actuator `/actuator/health`.

Example Dockerfile:

```
FROM maven:3.9.0-eclipse-temurin-17 AS build
WORKDIR /app
COPY pom.xml .
COPY src ./src
RUN mvn package -DskipTests

FROM eclipse-temurin:17-jre
COPY --from=build /app/target/app.jar /app/app.jar
ENV JAVA_OPTS="-Xms256m -Xmx512m"
HEALTHCHECK --interval=30s CMD curl -f http://localhost/actuator/health || exit 1
ENTRYPOINT ["sh","-c","java $JAVA_OPTS -jar /app/app.jar"]
```

### 3.3 Laravel / PHP

- Build: use Composer to install dependencies in CI (prefer building image in CI to avoid composer on host): `composer install --no-dev --optimize-autoloader`.
- Web server: PHP-FPM + nginx. Ensure `storage` and `bootstrap/cache` writable by container user.
- Env: `.env` with `APP_KEY`, DB credentials, queue settings.
- Migrations: `php artisan migrate --force` as a one-off job.

Example Dockerfile (PHP-FPM):

```
FROM php:8.2-fpm
WORKDIR /var/www/html
RUN apt-get update && apt-get install -y zip unzip git
COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
COPY . .
RUN composer install --no-dev --optimize-autoloader
RUN chown -R www-data:www-data storage bootstrap/cache
EXPOSE 9000
CMD ["php-fpm"]
```

### 3.4 Static HTML / SPA (Vue, Angular)

- Build step: `npm ci && npm run build` in CI to produce `dist/` (Vue) or `dist/project-name` (Angular).
- Serve with nginx or a static server. Build into an nginx image copying `dist/` into `/usr/share/nginx/html`.
- Ensure proper caching headers and `index.html` fallback for SPA routes.

Example Dockerfile:

```
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
```

### 3.5 Vue / Angular specifics

- Environment injection: build-time config (e.g., `env.js`) or runtime with simple fallback script that reads `window.__ENV__`.
- Base href for Angular must match reverse proxy path.

## 4. Databases

### 4.1 MySQL

- Init scripts: place `.sql` files in a `docker-entrypoint-initdb.d/` directory for first-run initialization.
- Persist with a named volume or host path: `/var/lib/mysql`.
- Create a dedicated user and database for each app, avoid using root/`root`.

Example docker-compose service snippet:

```
mysql:
  image: mysql:8.0
  environment:
    MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
    MYSQL_DATABASE: ${MYSQL_DATABASE}
    MYSQL_USER: ${MYSQL_USER}
    MYSQL_PASSWORD: ${MYSQL_PASSWORD}
  volumes:
    - mysql-data:/var/lib/mysql
    - ./db/init:/docker-entrypoint-initdb.d:ro
```

### 4.2 SQL Server

- Use persistent volume and strong `SA` password from secrets.
- Consider running migrations from the application or a migration container.

Example SQL Server snippet:

```
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    SA_PASSWORD: ${SA_PASSWORD}
    ACCEPT_EULA: "Y"
  volumes:
    - mssql-data:/var/opt/mssql
```

## 5. Secrets & Configuration

- Local dev: `.env` files (never commit). Production: use Docker secrets or a vault.
- Docker secrets example (Swarm):
  - Create secret: `docker secret create prod_db_password ./secrets/db_password.txt`
  - Reference secret in service and read from `/run/secrets/prod_db_password` inside container.

## 6. Healthchecks, Probes, and Logging

- Add `HEALTHCHECK` in Dockerfile for container-level checks.
- Ensure app returns healthy status quickly; readiness endpoints for startup tasks.
- Log to stdout/stderr; aggregate with the host logging driver or a log shipper (Fluentd/ELK).

## 7. CI/CD (Jenkins) — minimal pipeline

Use a branch-aware Jenkins pipeline that builds, tests, tags, and deploys images depending on the branch:

- `development` -> QA: build images, push to registry, render `docker-stack.qa.yaml` with the registry image tags, deploy to Swarm (QA).
- `main` -> Prod: build images, push to registry, render `docker-stack.yaml` (or `docker-stack.prod.yaml` if you create one) with registry image tags, deploy to Swarm (Prod).

Key recommendations and concrete steps:

1. Credentials to configure in Jenkins (credential IDs used in examples):

- `docker-registry-url` — Secret text with registry host (e.g. `registry.example.com/myorg`).
- `docker-registry-credentials` — Username/password for the registry.
- `sql-server-password`, `firebase-storage-bucket`, `firebase-credentials-json`, `jwt-secret-key` — as used by the pipeline.

2. Image publish & stack render (recommended flow executed by pipeline):

- Build images locally in CI:

```bash
docker build -t edm-backend:${GIT_COMMIT_SHORT} -t edm-backend:latest ./backend
docker build -t edm-frontend:${GIT_COMMIT_SHORT} -t edm-frontend:latest ./frontend
```

- Login to registry and push tags (use `docker-registry-credentials`):

```bash
echo $REG_PASS | docker login -u $REG_USER --password-stdin ${DOCKER_REGISTRY}
docker tag edm-backend:latest ${DOCKER_REGISTRY}/edm-backend:${GIT_COMMIT_SHORT}
docker push ${DOCKER_REGISTRY}/edm-backend:${GIT_COMMIT_SHORT}
docker push ${DOCKER_REGISTRY}/edm-backend:latest
```

- Render a temporary stack file that replaces local image references with registry tags before calling `docker stack deploy`:

```bash
SRC=${QA_COMPOSE:-docker-stack.qa.yaml}
OUT=/tmp/docker-stack-${BRANCH}-${GIT_COMMIT_SHORT}.yaml
sed -e "s|image: edm-backend:latest|image: ${DOCKER_REGISTRY}/edm-backend:${GIT_COMMIT_SHORT}|g" \
    -e "s|image: edm-frontend:latest|image: ${DOCKER_REGISTRY}/edm-frontend:${GIT_COMMIT_SHORT}|g" \
    ${SRC} > ${OUT}
docker stack deploy -c ${OUT} ${STACK_NAME} --with-registry-auth
```

3. Migration & health checks

- After deploy, run health checks (e.g., `curl -f http://localhost/api/health`) and then run DB migrations as a one-off command or job inside the backend container:

```bash
BACKEND_CONTAINER=$(docker ps -q -f name=${STACK_NAME}_backend | head -n 1)
docker exec $BACKEND_CONTAINER dotnet ef database update || echo "migrations failed or already applied"
```

4. Checklist for image substitution in CI

- [ ] Ensure `docker-registry-url` and `docker-registry-credentials` exist in Jenkins credentials.
- [ ] CI sets `GIT_COMMIT_SHORT` and `BRANCH` environment variables.
- [ ] Build, tag, and push images to `${DOCKER_REGISTRY}`.
- [ ] Render the stack file (using `sed` or a templating tool) to replace `edm-backend:latest` and `edm-frontend:latest` with `${DOCKER_REGISTRY}/...:${GIT_COMMIT_SHORT}`.
- [ ] Deploy using `docker stack deploy -c <rendered-file> <stack-name> --with-registry-auth`.

Add the pipeline example `Jenkinsfile` (in this repo at `doc/deployment/Jenkinsfile`) which implements the flow above; customize credential IDs to match your Jenkins instance.

## 8. Database Migrations Strategy

- Option A (recommended): Run migrations as a separate Kubernetes Job / Swarm one-off service prior to new release.
- Option B: Application runs migrations at startup in a leader/one-time check (only if safe and idempotent).

Example one-off migration service (Swarm):

```
docker service create --name app_migrate --env-file .env --mount type=volume,source=mysql-data,target=/var/lib/mysql registry/app:tag sh -c "dotnet ef database update"
```

## 9. Backups

- Databases: daily dumps using `mysqldump`/`sqlcmd`, keep off-host copies.
- Files: sync storage to object storage (S3-compatible) or scheduled rsync.

## 10. Monitoring & Alerts

- Expose metrics endpoints (Prometheus) and track uptime, error rate, and latency.
- Set SLO/alerts for CPU, memory, and rate of 5xx responses.

## 11. Production Checklist (per application)

- [ ] CI pipeline builds and pushes a tagged image.
- [ ] Dockerfile uses multi-stage build and HEALTHCHECK.
- [ ] App supports env-driven config; secrets not in image.
- [ ] DB migrations tested and runnable as job.
- [ ] Volumes and backups configured.
- [ ] Load test in staging and adjust resource limits.
- [ ] TLS configured at reverse proxy (nginx) with automatic renewal.

## 12. Sample Docker Compose (local dev)

```
version: '3.8'
services:
  app:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - CONNECTION_STRING=mysql://user:pass@mysql:3306/db
    depends_on:
      - mysql
  mysql:
    image: mysql:8
    environment:
      MYSQL_ROOT_PASSWORD: rootpw
      MYSQL_DATABASE: appdb
      MYSQL_USER: app
      MYSQL_PASSWORD: apass
    volumes:
      - mysql-data:/var/lib/mysql
volumes:
  mysql-data:
```

## 13. Jenkins + Swarm Deploy Tips

- Keep a versioned `docker-stack.yml` in repo; CI writes the image tag into it before deploy.
- Use `docker stack deploy --with-registry-auth` from Jenkins to ensure nodes can pull images.
- For zero-downtime, use rolling update settings and healthchecks in the stack file.

## 14. Example nginx reverse-proxy (basic)

Use `nginx` in front of services with TLS termination and proxy pass to internal services.

```
server {
  listen 80;
  server_name example.com;
  location / {
    proxy_pass http://frontend:80;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
  }
}
```

## 15. Next Steps & Maintenance

- Document each application's environment variables in its repo `README.md`.
- Add automated smoke tests in CI after deployment.
- Add regular security patching for base images.

---

If you want, I can:

- add per-repo Dockerfile templates and sample `docker-stack.yml` entries for each technology,
- create a Jenkinsfile example customized per-app, or
- add automated migration job examples.

Tell me which you'd like next.
