#!/bin/bash
#####################################################################
# Database Backup Script
# File: backup-db.sh
# Purpose: Backup SQL Server database
# Upload via MobaXterm and run manually
#####################################################################

set -e

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() { echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"; }
error() { echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"; }
warning() { echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"; }

# Configuration
BACKUP_DIR="/opt/backups/edm"
STACK_NAME="edm"
APP_DIR="/opt/apps/edm"

# Load environment variables
if [ -f "$APP_DIR/.env" ]; then
    source "$APP_DIR/.env"
elif [ -f "./.env" ]; then
    source ./.env
else
    error "No .env file found!"
    exit 1
fi

# Create backup directory
mkdir -p "$BACKUP_DIR"

log "Starting database backup..."

# Find SQL Server container
SQLSERVER_CONTAINER=$(docker ps -q -f name=${STACK_NAME}_sqlserver 2>/dev/null || echo "")

if [ -z "$SQLSERVER_CONTAINER" ]; then
    error "SQL Server container not found!"
    error "Is the stack running? Check with: docker service ls"
    exit 1
fi

# Generate backup filename
BACKUP_FILE="$BACKUP_DIR/edm-db-$(date +%Y%m%d-%H%M%S).bak"

# Create backup inside container
log "Creating backup in container..."
docker exec $SQLSERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U SA -P "${SA_PASSWORD}" \
    -Q "BACKUP DATABASE [CatalogDB] TO DISK = '/tmp/backup.bak' WITH INIT, COMPRESSION" || {
    error "Backup failed!"
    exit 1
}

# Copy backup to host
log "Copying backup to host..."
docker cp $SQLSERVER_CONTAINER:/tmp/backup.bak "$BACKUP_FILE" || {
    error "Failed to copy backup from container!"
    exit 1
}

# Verify backup file
if [ -f "$BACKUP_FILE" ]; then
    BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    log "Backup completed successfully!"
    log "Location: $BACKUP_FILE"
    log "Size: $BACKUP_SIZE"
    
    # List recent backups
    echo ""
    log "Recent backups:"
    ls -lh "$BACKUP_DIR" | tail -n 5
    
    echo ""
    log "Download this backup using MobaXterm SFTP:"
    log "  Navigate to: $BACKUP_DIR"
    log "  Right-click on: $(basename $BACKUP_FILE)"
    log "  Select: Download"
else
    error "Backup file not found!"
    exit 1
fi

# Clean old backups (keep last 10)
log "Cleaning old backups (keeping last 10)..."
cd "$BACKUP_DIR"
ls -t edm-db-*.bak 2>/dev/null | tail -n +11 | xargs -r rm -f
log "Cleanup completed"

log "Backup process finished!"
