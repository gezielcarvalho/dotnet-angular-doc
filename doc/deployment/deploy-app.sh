#!/bin/bash
#####################################################################
# Local Deployment Script for Manual Execution
# File: deploy-local.sh
# Purpose: Deploy application stack locally on server after manual upload
# Usage: Upload via MobaXterm, then run: ./deploy-local.sh
#####################################################################

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"
}

warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

info() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')] INFO:${NC} $1"
}

# Load server configuration if exists
if [ -f "./server-config.sh" ]; then
    source ./server-config.sh
    log "Loaded server configuration"
else
    warning "server-config.sh not found, using defaults"
    APP_DIR="/opt/apps/edm"
fi

# Load environment variables if exists
if [ -f "./.env" ]; then
    source ./.env
    log "Loaded environment variables"
else
    warning ".env file not found, will use defaults from docker-stack.qa.yaml"
fi

log "Starting deployment process..."

# Check if Docker is running
if ! docker info &> /dev/null; then
    error "Docker is not running or not installed!"
    error "Please run setup-swarm-jenkins.sh first"
    exit 1
fi

# Check if Swarm is initialized
if ! docker info | grep -q "Swarm: active"; then
    error "Docker Swarm is not initialized!"
    error "Please run setup-swarm-jenkins.sh first"
    exit 1
fi

# Check if docker-stack.qa.yaml exists
if [ ! -f "docker-stack.qa.yaml" ]; then
    error "docker-stack.qa.yaml not found!"
    error "Please upload it to the current directory first"
    exit 1
fi

# Create application directory if it doesn't exist
if [ ! -d "$APP_DIR" ]; then
    log "Creating application directory: $APP_DIR"
    mkdir -p "$APP_DIR"
fi

# Copy files to application directory
log "Copying files to $APP_DIR..."
cp docker-stack.qa.yaml "$APP_DIR/"
if [ -f ".env" ]; then
    cp .env "$APP_DIR/"
    chmod 600 "$APP_DIR/.env"
fi
if [ -f "server-config.sh" ]; then
    cp server-config.sh "$APP_DIR/"
fi

# Navigate to application directory
cd "$APP_DIR"
log "Changed to application directory: $APP_DIR"

# Check if stack is already deployed
STACK_NAME="edm"
if docker stack ls | grep -q "$STACK_NAME"; then
    log "Stack '$STACK_NAME' already exists, updating..."
    
    # Backup database before update
    log "Creating database backup before update..."
    BACKUP_DIR="/opt/backups/edm"
    mkdir -p "$BACKUP_DIR"
    
    SQLSERVER_CONTAINER=$(docker ps -q -f name=edm_sqlserver 2>/dev/null || echo "")
    if [ ! -z "$SQLSERVER_CONTAINER" ]; then
        BACKUP_FILE="$BACKUP_DIR/pre-update-$(date +%Y%m%d-%H%M%S).bak"
        docker exec $SQLSERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd \
            -S localhost -U SA -P "${SA_PASSWORD}" \
            -Q "BACKUP DATABASE [CatalogDB] TO DISK = '/tmp/backup.bak'" 2>/dev/null || true
        docker cp $SQLSERVER_CONTAINER:/tmp/backup.bak "$BACKUP_FILE" 2>/dev/null || true
        if [ -f "$BACKUP_FILE" ]; then
            log "Database backed up to: $BACKUP_FILE"
        fi
    else
        info "SQL Server container not found, skipping backup"
    fi
else
    log "Deploying new stack '$STACK_NAME'..."
fi

# Deploy stack
log "Deploying stack to Docker Swarm..."
docker stack deploy -c docker-stack.qa.yaml "$STACK_NAME"

# Wait for services to start
log "Waiting for services to initialize..."
sleep 15

# Check service status
log "Checking service status..."
docker stack services "$STACK_NAME"

# Wait for backend to be healthy
log "Waiting for backend service to be healthy..."
MAX_RETRIES=30
RETRY_COUNT=0
while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    if curl -f http://localhost/health &> /dev/null || curl -f http://localhost/api/health &> /dev/null; then
        log "Backend is healthy!"
        break
    fi
    RETRY_COUNT=$((RETRY_COUNT+1))
    if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
        warning "Backend health check timeout after $MAX_RETRIES attempts"
        warning "Check logs with: docker service logs edm_backend"
    else
        sleep 2
    fi
done

# Run database migrations
log "Checking for backend container to run migrations..."
sleep 5
BACKEND_CONTAINER=$(docker ps -q -f name=edm_backend | head -n 1)
if [ ! -z "$BACKEND_CONTAINER" ]; then
    log "Running database migrations..."
    docker exec $BACKEND_CONTAINER dotnet ef database update || {
        warning "Database migration failed or already up to date"
        info "Check migrations with: docker service logs edm_backend"
    }
else
    warning "Backend container not found, skipping migrations"
    info "Run migrations manually later with:"
    info "  docker exec \$(docker ps -q -f name=edm_backend) dotnet ef database update"
fi

# Display final status
echo ""
log "=========================================="
log "Deployment Summary"
log "=========================================="
info "Stack Name: $STACK_NAME"
info "Application Directory: $APP_DIR"
echo ""
log "Running Services:"
docker service ls --filter name=$STACK_NAME
echo ""
log "Service Details:"
docker stack ps $STACK_NAME
echo ""
log "=========================================="
log "Access Points:"
log "=========================================="
info "Application: http://YOUR_SERVER_IP"
info "API: http://YOUR_SERVER_IP/api"
info "Portainer: http://YOUR_SERVER_IP:9000"
info "Jenkins: http://YOUR_SERVER_IP:8080"
log "=========================================="
echo ""
info "Useful commands:"
info "  View logs: docker service logs edm_backend -f"
info "  Scale: docker service scale edm_backend=3"
info "  Update: docker stack deploy -c docker-stack.qa.yaml edm"
info "  Remove: docker stack rm edm"
echo ""
log "Deployment completed successfully!"
