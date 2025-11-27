# Application Deployment Guide - Catalog Application

**Date:** November 27, 2025  
**Application:** .NET 8 + Angular Catalog Application  
**Infrastructure Required:** Docker Swarm, Jenkins, Portainer

## Prerequisites

Before deploying the application, ensure:

- ✅ Infrastructure is set up (see `/doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md`)
- ✅ Docker Swarm is active
- ✅ Jenkins is running
- ✅ Portainer is accessible

---

## Application Architecture

### Services

- **Backend:** .NET 8 Web API (2 replicas)
- **Frontend:** Angular 17 SPA (2 replicas)
- **Database:** SQL Server 2022 (1 replica)
- **Nginx:** Reverse proxy and load balancer (1 replica)

### Network

- Overlay network: `catalog-network-swarm`
- Internal service discovery via DNS

---

## Step 1: Prepare Configuration Files

### 1.1 Create Environment File

Create `.env` with your configuration:

```bash
# Database Configuration
SA_PASSWORD=YOUR_STRONG_PASSWORD_HERE
MSSQL_PID=Developer

# Backend Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80

# Frontend Configuration
NODE_ENV=production
```

**⚠️ Important:** Never commit `.env` to version control!

### 1.2 Create Server Configuration

Create `server-config.sh`:

```bash
#!/bin/bash
# Server Configuration
export SERVER_IP="YOUR_SERVER_IP"
export DEPLOY_USER="deploy"
export APP_DIR="/opt/apps/catalog"
export GITHUB_REPO="https://github.com/YOUR_USERNAME/YOUR_REPO.git"
export BACKUP_DIR="/opt/backups/catalog"
export STACK_NAME="catalog"
```

**⚠️ Important:** Never commit `server-config.sh` to version control!

---

## Step 2: Upload Application Files

### 2.1 Files to Upload via MobaXterm

Upload these files to `/root/deployment/`:

- [ ] `docker-stack.qa.yaml` (from repository root)
- [ ] `.env` (your customized version)
- [ ] `server-config.sh` (your customized version)
- [ ] `doc/deployment/deploy-app.sh` (deployment script)
- [ ] `doc/deployment/backup-db.sh` (backup script)

### 2.2 Upload via SFTP Browser

1. Connect via MobaXterm
2. Use SFTP browser (left panel)
3. Navigate to `/root/deployment/`
4. Drag and drop files from local machine

---

## Step 3: Deploy Application

### 3.1 Make Scripts Executable

```bash
cd /root/deployment
chmod +x deploy-app.sh
chmod +x backup-db.sh
```

### 3.2 Run Deployment Script

```bash
./deploy-app.sh
```

The script will:

1. Load configuration
2. Create application directory
3. Copy files to app directory
4. Deploy Docker stack
5. Wait for services to start
6. Check health endpoints
7. Run database migrations

### 3.3 Verify Deployment

```bash
# Check service status
docker service ls

# View stack services
docker stack services catalog

# Check individual service
docker service ps catalog_backend --no-trunc
docker service ps catalog_frontend --no-trunc
```

---

## Step 4: Access Application

### Via Web Browser

- **Application:** `http://YOUR_SERVER_IP`
- **API:** `http://YOUR_SERVER_IP/api`
- **Health Check:** `http://YOUR_SERVER_IP/health`

### Test Endpoints

```bash
# Health check
curl http://YOUR_SERVER_IP/health

# API endpoint
curl http://YOUR_SERVER_IP/api/documents

# Frontend
curl http://YOUR_SERVER_IP
```

---

## Step 5: Configure CI/CD Pipeline

### 5.1 Jenkins Credentials

**Manage Jenkins → Manage Credentials → (global) → Add Credentials**

1. **GitHub Credentials:**

   - Kind: Username with password
   - Username: `your-github-username`
   - Password: GitHub Personal Access Token
   - ID: `github-credentials`

2. **SQL Server Password:**

   - Kind: Secret text
   - Secret: (from your `.env` file)
   - ID: `sql-server-password`

3. **GitHub Webhook Secret:**
   ```bash
   # Generate secret
   openssl rand -hex 20
   ```
   - Kind: Secret text
   - Secret: (generated value)
   - ID: `github-webhook-secret`

### 5.2 Create Jenkins Pipeline Job

1. **Jenkins → New Item**
2. **Name:** `catalog-deployment`
3. **Type:** Pipeline
4. **Configure:**
   - ✓ GitHub project: `https://github.com/YOUR_USERNAME/YOUR_REPO/`
   - ✓ GitHub hook trigger for GITScm polling
   - Pipeline from SCM → Git
   - Repository URL: `https://github.com/YOUR_USERNAME/YOUR_REPO.git`
   - Credentials: `github-credentials`
   - Branch: `*/development`
   - Script Path: `Jenkinsfile`
5. **Save**

### 5.3 Configure GitHub Webhook

1. Go to: `https://github.com/YOUR_USERNAME/YOUR_REPO/settings/hooks`
2. **Add webhook**
3. Configure:
   - Payload URL: `http://YOUR_SERVER_IP:8080/github-webhook/`
   - Content type: `application/json`
   - Secret: (your `github-webhook-secret`)
   - Events: ✓ Pull requests, ✓ Pushes
   - ✓ Active
4. **Add webhook**

---

## Application Management

### View Logs

```bash
# Backend logs
docker service logs catalog_backend -f

# Frontend logs
docker service logs catalog_frontend -f

# Database logs
docker service logs catalog_sqlserver -f

# All services
docker service logs -f $(docker service ls -q -f name=catalog)
```

### Scale Services

```bash
# Scale backend
docker service scale catalog_backend=3

# Scale frontend
docker service scale catalog_frontend=2

# View updated status
docker service ls
```

### Update Application

#### Option 1: Via Jenkins (Automated)

- Merge PR to `development` branch
- Jenkins automatically builds and deploys

#### Option 2: Manual Update

```bash
cd /opt/apps/catalog

# Pull latest code (if using Git deployment)
git pull origin development

# Redeploy stack
docker stack deploy -c docker-stack.qa.yaml catalog

# Check rollout status
docker service ps catalog_backend
```

### Restart Services

```bash
# Force update (restarts containers)
docker service update --force catalog_backend
docker service update --force catalog_frontend

# Or redeploy entire stack
cd /opt/apps/catalog
docker stack deploy -c docker-stack.qa.yaml catalog
```

---

## Database Management

### Run Migrations

```bash
# Find backend container
BACKEND_CONTAINER=$(docker ps -q -f name=catalog_backend | head -n 1)

# Run migrations
docker exec $BACKEND_CONTAINER dotnet ef database update
```

### Backup Database

```bash
# Use backup script
cd /root/deployment
./backup-db.sh

# Or manual backup
SQLSERVER_CONTAINER=$(docker ps -q -f name=catalog_sqlserver)
docker exec $SQLSERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "YOUR_PASSWORD" \
  -Q "BACKUP DATABASE [CatalogDB] TO DISK = '/tmp/backup.bak' WITH INIT, COMPRESSION"
docker cp $SQLSERVER_CONTAINER:/tmp/backup.bak /opt/backups/catalog/
```

### Restore Database

```bash
# Copy backup to container
docker cp /opt/backups/catalog/backup.bak $SQLSERVER_CONTAINER:/tmp/

# Restore database
docker exec $SQLSERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "YOUR_PASSWORD" \
  -Q "RESTORE DATABASE [CatalogDB] FROM DISK = '/tmp/backup.bak' WITH REPLACE"
```

---

## Monitoring

### Health Checks

```bash
# Application health
curl http://localhost/health

# Individual services
docker service ls
docker stack ps catalog

# Resource usage
docker stats
```

### Via Portainer

1. Open: `http://YOUR_SERVER_IP:9000`
2. Navigate to: Stacks → catalog
3. View:
   - Service status
   - Container logs
   - Resource usage
   - Network topology

---

## Troubleshooting

### Backend Not Starting

```bash
# View detailed errors
docker service ps catalog_backend --no-trunc

# Check logs
docker service logs catalog_backend --tail=100

# Inspect service
docker service inspect catalog_backend

# Check database connection
BACKEND_CONTAINER=$(docker ps -q -f name=catalog_backend)
docker exec $BACKEND_CONTAINER dotnet --info
```

### Database Connection Issues

```bash
# Test database connection
SQLSERVER_CONTAINER=$(docker ps -q -f name=catalog_sqlserver)
docker exec -it $SQLSERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "YOUR_PASSWORD" \
  -Q "SELECT @@VERSION"

# Check database exists
docker exec $SQLSERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "YOUR_PASSWORD" \
  -Q "SELECT name FROM sys.databases"
```

### Frontend Not Loading

```bash
# Check nginx logs
docker service logs catalog_nginx

# Check frontend logs
docker service logs catalog_frontend

# Test frontend container
FRONTEND_CONTAINER=$(docker ps -q -f name=catalog_frontend)
docker exec $FRONTEND_CONTAINER ls -la /usr/share/nginx/html
```

### Cannot Access Application

```bash
# Check firewall
sudo ufw status

# Check if nginx is listening
docker service ps catalog_nginx

# Test locally
curl http://localhost
curl http://localhost/api/health
```

---

## Backup Strategy

### Automated Backups

Create cron job for daily backups:

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * /root/deployment/backup-db.sh >> /var/log/backup.log 2>&1
```

### Download Backups

Via MobaXterm:

1. Connect to server
2. SFTP browser → `/opt/backups/catalog/`
3. Right-click backup file → Download

---

## Rollback Procedure

### Quick Rollback

```bash
cd /opt/apps/catalog

# Option 1: Rollback to previous git commit
git log --oneline
git checkout PREVIOUS_COMMIT_HASH
docker stack deploy -c docker-stack.qa.yaml catalog

# Option 2: Restore from backup
# 1. Stop current stack
docker stack rm catalog
sleep 10

# 2. Restore database
# (see Database Restore above)

# 3. Redeploy
docker stack deploy -c docker-stack.qa.yaml catalog
```

---

## Performance Optimization

### Resource Limits

Edit `docker-stack.qa.yaml` to adjust:

```yaml
services:
  backend:
    deploy:
      replicas: 3 # Increase for higher load
      resources:
        limits:
          cpus: "2.0"
          memory: 2048M
```

### Database Optimization

```bash
# Connect to SQL Server
docker exec -it $SQLSERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "YOUR_PASSWORD"

# Check database size
SELECT name, size * 8 / 1024 AS SizeMB FROM sys.master_files;

# Rebuild indexes
EXEC sp_MSforeachtable @command1="DBCC DBREINDEX ('?')";
```

---

## Security Best Practices

### Application Security

- [ ] Strong database password in `.env`
- [ ] Environment variables not in code
- [ ] HTTPS configured (production)
- [ ] Secrets managed via Docker secrets
- [ ] Regular security updates

### Network Security

- [ ] Firewall configured
- [ ] Only necessary ports exposed
- [ ] Internal services on overlay network
- [ ] Rate limiting configured

### Data Security

- [ ] Regular automated backups
- [ ] Backup files encrypted
- [ ] Database connections encrypted
- [ ] File upload validation

---

## Quick Reference

### Common Commands

```bash
# Deploy/Update
cd /opt/apps/catalog && docker stack deploy -c docker-stack.qa.yaml catalog

# View status
docker stack services catalog

# Scale
docker service scale catalog_backend=3

# Logs
docker service logs catalog_backend -f

# Backup
/root/deployment/backup-db.sh

# Restart
docker service update --force catalog_backend
```

### File Locations

- Application: `/opt/apps/catalog/`
- Deployment files: `/root/deployment/`
- Backups: `/opt/backups/catalog/`
- Logs: `docker service logs SERVICE_NAME`

---

**Application deployed and running on Docker Swarm!** 🎉
