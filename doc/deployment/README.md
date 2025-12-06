# Deployment Documentation

This folder contains application-specific deployment guides and configuration files for the Catalog application.

## 📋 Contents

### Documentation

- **APP_DEPLOYMENT.md** - Complete application deployment guide
- **Jenkinsfile** - CI/CD pipeline configuration

### Scripts

- **deploy-app.sh** - Optional manual deployment script (fallback)
- **backup-db.sh** - Database backup script

### Configuration Templates

- **.env.template** - Application environment variables template
- **server-config.template.sh** - Server configuration template
- **nginx-qa.conf** - Nginx reverse proxy for Docker Compose setup
- **nginx-swarm.conf** - Nginx reverse proxy for Docker Swarm deployment

---

## Prerequisites

- ✅ Infrastructure must be set up (see `/doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md`)
- ✅ Docker Swarm must be active
- ✅ Jenkins must be running
- ✅ Portainer must be accessible

---

## CI/CD Quick Start

### 1. Configure GitHub Webhook

Set up a webhook in your GitHub repository to notify Jenkins on push/merge to `main`.

### 2. Jenkins Pipeline

Jenkins will automatically:

- Checkout the latest code from GitHub
- Build Docker images
- Deploy the stack using Docker Swarm

No manual file uploads are required for deployment. Manual scripts are available for fallback or maintenance.

### 3. Deploy Application

```bash
cd /root/deployment
chmod +x deploy-app.sh backup-db.sh
./deploy-app.sh
```

---

## Application Architecture

### Services

- **Backend:** .NET 8 Web API (2 replicas)
- **Frontend:** Angular 17 SPA (2 replicas)
- **Database:** SQL Server 2022 (1 replica)
- **Nginx:** Reverse proxy (1 replica)

### Access URLs

- Application: `http://YOUR_SERVER_IP`
- API: `http://YOUR_SERVER_IP/api`
- Health: `http://YOUR_SERVER_IP/health`

---

## CI/CD Pipeline

### Setup Jenkins Pipeline

1. **Create credentials in Jenkins:**

   - `github-credentials` - GitHub PAT
   - `sql-server-password` - Database password
   - `github-webhook-secret` - Webhook secret

2. **Create Jenkins job:**

   - Name: `edm-deployment`
   - Type: Pipeline
   - SCM: Git (your repo)
   - Branch: `*/development`
   - Script Path: `doc/deployment/Jenkinsfile`

3. **Configure GitHub webhook:**
   - URL: `http://YOUR_SERVER_IP:8080/github-webhook/`
   - Content type: `application/json`
   - Secret: (your webhook secret)
   - Events: Pull requests, Pushes

### Pipeline Stages

1. Checkout code
2. Verify branch (development only)
3. Build Docker images (backend + frontend)
4. Run tests (backend + frontend)
5. Security scan (Trivy)
6. Tag images
7. Backup database
8. Deploy to Swarm
9. Health check
10. Run migrations
11. Cleanup old images

---

## Management Operations

### View Logs

```bash
docker service logs edm_backend -f
docker service logs edm_frontend -f
```

### Scale Services

```bash
docker service scale edm_backend=3
docker service scale edm_frontend=2
```

### Backup Database

```bash
/root/deployment/backup-db.sh
```

### Update Application

```bash
cd /opt/apps/edm
docker stack deploy -c docker-stack.qa.yaml edm
```

---

## Security Notes

⚠️ **Never commit these files to version control:**

- `.env`
- `server-config.sh`
- Any file containing passwords, tokens, or server IPs

✅ **Safe to commit:**

- `.env.template`
- `server-config.template.sh`
- All `.md` documentation files
- Scripts without hardcoded credentials

---

## Support

For detailed instructions, see:

- **Full deployment guide:** `APP_DEPLOYMENT.md`
- **Infrastructure setup:** `/doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md`
- **Troubleshooting:** Refer to deployment documentation

---

**Application-specific deployment documentation.** 🚀
