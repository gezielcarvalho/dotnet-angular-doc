#!/bin/bash
# Server Configuration Template
# Copy this file, rename to server-config.sh, and customize with your values
# DO NOT commit server-config.sh to version control!

# Server IP address
export SERVER_IP="YOUR_SERVER_IP_HERE"

# Deployment user (created during setup)
export DEPLOY_USER="deploy"

# Application directory
export APP_DIR="/opt/apps/edm"

# GitHub repository URL
export GITHUB_REPO="https://github.com/YOUR_USERNAME/YOUR_REPO.git"

# Backup directory
export BACKUP_DIR="/opt/backups/edm"

# Stack name
export STACK_NAME="edm"
