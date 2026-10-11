# Historical documentation reorganization summary

> Historical record only. The material described below predates the standard
> `docs/` structure and must not be used as current deployment guidance.

**Date:** November 27, 2025  
**Purpose:** Separate generic infrastructure from application-specific deployment

---

## Overview

The documentation has been reorganized into two distinct folders:

1. **`/doc/infrastructure/`** - Generic infrastructure setup (Docker Swarm, Jenkins, Portainer)
2. **`/doc/deployment/`** - Application-specific deployment (Catalog app)

This separation allows the infrastructure setup to be **reusable for any application** while keeping app-specific configurations separate and secure.

---

## Folder Structure

### `/doc/infrastructure/` - Generic Infrastructure

**Purpose:** Set up Docker Swarm, Jenkins, and Portainer  
**Reusable:** ✅ Yes - works for any application  
**Contains:**

```
infrastructure/
├── D001_INFRASTRUCTURE_SETUP.md    ⭐ Main setup guide
├── setup-swarm-jenkins.sh          Infrastructure automation script
├── nginx-swarm.conf                Example Nginx for Swarm
├── nginx-qa.conf                   Example Nginx for Compose
├── .gitignore                      Prevents committing secrets
└── README.md                       Overview and quick start

Legacy (Reference):
├── D007_QA_deployment_plan.md
├── D008_MOBAXTERM_DEPLOYMENT.md
├── DEPLOYMENT_CHECKLIST.md
├── DEPLOYMENT_SUMMARY.md
├── jenkins-setup.md
└── deploy-qa.sh
```

**What it sets up:**

- ✅ Docker Engine + Compose plugin
- ✅ Docker Swarm (single-node mode)
- ✅ Portainer CE (port 9000/9443)
- ✅ Jenkins LTS (port 8080)
- ✅ UFW firewall

**No application-specific info:**

- ❌ No GitHub webhooks
- ❌ No repository URLs
- ❌ No app credentials
- ❌ No database configs
- ❌ No Jenkinsfile

---

### `/doc/deployment/` - Application Deployment

**Purpose:** Deploy the Catalog application  
**Application-specific:** ✅ Yes - Catalog .NET + Angular app  
**Contains:**

```
deployment/
├── APP_DEPLOYMENT.md              ⭐ Application deployment guide
├── Jenkinsfile                    CI/CD pipeline for Catalog app
├── deploy-app.sh                  Application deployment script
├── backup-db.sh                   Database backup script
├── .env.template                  Environment variables template
├── server-config.template.sh      Server config template
├── .gitignore                     Prevents committing secrets
└── README.md                      Overview and quick start
```

**What it deploys:**

- Backend (.NET 8 API)
- Frontend (Angular 17)
- Database (SQL Server 2022)
- Nginx (reverse proxy)

**Application-specific configs:**

- GitHub webhook setup
- Repository URL
- Database credentials
- Jenkins pipeline
- Stack file reference

---

## Security Improvements

### Templates Instead of Actual Values

**Old Approach:**

```bash
# Hard-coded in scripts
GITHUB_REPO="https://github.com/gezielcarvalho/dotnet-angular-doc.git"
SA_PASSWORD="<example-only; never reuse or commit a real password>"
SERVER_IP="212.227.243.129"
```

**New Approach:**

```bash
# Templates committed to git
.env.template               ← Safe to commit
server-config.template.sh   ← Safe to commit

# Actual values (gitignored)
.env                        ← Never committed
server-config.sh            ← Never committed
```

### .gitignore Protection

Both folders have `.gitignore` files:

```
# .gitignore
.env
server-config.sh
*.log
*.bak
.DS_Store
secrets/
```

---

## Migration Guide

### For Infrastructure Setup (Any Project)

**Use:** `/doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md`

**Steps:**

1. Connect to server via MobaXterm
2. Upload `setup-swarm-jenkins.sh`
3. Run the script
4. Access Portainer (port 9000) and Jenkins (port 8080)

**Result:** Docker Swarm + Jenkins + Portainer ready for any application

---

### For Application Deployment (Catalog App)

**Prerequisites:** Infrastructure must be set up first!

**Use:** `/doc/deployment/APP_DEPLOYMENT.md`

**Steps:**

1. Copy templates and customize:

   ```bash
   cp doc/deployment/.env.template .env
   cp doc/deployment/server-config.template.sh server-config.sh
   # Edit with your actual values
   ```

2. Upload files to server:

   - `docker-stack.qa.yaml` (from root)
   - `.env` (customized)
   - `server-config.sh` (customized)
   - `deploy-app.sh`
   - `backup-db.sh`

3. Run deployment:

   ```bash
   ./deploy-app.sh
   ```

4. Configure Jenkins CI/CD:
   - Create credentials
   - Create pipeline job
   - Configure GitHub webhook

**Result:** Catalog application running on Docker Swarm with automated CI/CD

---

## Key Differences

| Aspect          | Infrastructure            | Deployment                    |
| --------------- | ------------------------- | ----------------------------- |
| **Purpose**     | Setup core services       | Deploy specific app           |
| **Reusable**    | ✅ Any project            | ❌ Catalog app only           |
| **Credentials** | Portainer, Jenkins admin  | Database, GitHub, app secrets |
| **GitHub**      | No integration            | Webhook + pipeline            |
| **Services**    | Swarm, Jenkins, Portainer | Backend, Frontend, DB, Nginx  |
| **Run Once**    | ✅ Per server             | ❌ Per deployment             |

---

## Workflow

### First Time Setup

```
1. Infrastructure Setup (Once per server)
   ↓
   /doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md
   ↓
   Docker Swarm + Jenkins + Portainer running
   ↓
2. Application Deployment (Per app)
   ↓
   /doc/deployment/APP_DEPLOYMENT.md
   ↓
   Catalog app running on infrastructure
```

### Ongoing Operations

**Infrastructure management:**

- Use Portainer (port 9000)
- Manage Jenkins (port 8080)
- `docker service ls`

**Application updates:**

- Merge PR to `development` → Jenkins auto-deploys
- Manual: `docker stack deploy -c docker-stack.qa.yaml edm`

---

## Files Moved

### Moved to `/doc/deployment/`

- ✅ `deploy-local.sh` → `deploy-app.sh`
- ✅ `backup-db.sh`
- ✅ `.env.template`
- ✅ `server-config.template.sh`
- ✅ `Jenkinsfile` (from root)

### Stayed in `/doc/infrastructure/`

- ✅ `setup-swarm-jenkins.sh` (infrastructure only)
- ✅ `nginx-*.conf` (examples)
- ✅ Legacy docs (reference)

### Created New

- ✅ `/doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md`
- ✅ `/doc/deployment/APP_DEPLOYMENT.md`
- ✅ `/doc/deployment/README.md`
- ✅ Both folders: `.gitignore`

---

## Benefits

### 🔒 Security

- Templates safe to commit
- Actual credentials never in git
- Separate concerns (infra vs app)

### 🔄 Reusability

- Infrastructure setup works for any app
- Easy to deploy multiple apps on same server

### 📖 Clarity

- Clear separation of concerns
- Easier to find relevant docs
- Better onboarding for new team members

### 🛡️ Best Practices

- Credentials in gitignored files
- Environment-specific configs separated
- Infrastructure as reusable component

---

## Quick Reference

### Setup New Server

👉 `/doc/infrastructure/D001_INFRASTRUCTURE_SETUP.md`

### Deploy Catalog App

👉 `/doc/deployment/APP_DEPLOYMENT.md`

### Customize Configs

```bash
# Application config
cp doc/deployment/.env.template .env
cp doc/deployment/server-config.template.sh server-config.sh
```

### Access Services

- **Portainer:** http://YOUR_SERVER_IP:9000
- **Jenkins:** http://YOUR_SERVER_IP:8080
- **Application:** http://YOUR_SERVER_IP

---

**Infrastructure and application deployment now properly separated!** 🎉
