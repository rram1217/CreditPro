#!/bin/bash

echo "Database Backup"
echo "=================="

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

print_success() {
    echo -e "${GREEN} $1${NC}"
}

print_error() {
    echo -e "${RED} $1${NC}"
}

BACKUP_DIR="./backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/creditpro_$TIMESTAMP.sql"

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup PostgreSQL
echo "Backing up PostgreSQL..."
docker exec creditpro-postgres pg_dump -U postgres creditprodb > $BACKUP_FILE

if [ $? -eq 0 ]; then
    print_success "Backup created: $BACKUP_FILE"
    
    # Compress backup
    gzip $BACKUP_FILE
    print_success "Backup compressed: ${BACKUP_FILE}.gz"
    
    # Delete old backups (keep last 7 days)
    find $BACKUP_DIR -name "*.sql.gz" -mtime +7 -delete
    print_success "Old backups cleaned up"
else
    print_error "Backup failed"
    exit 1
fi
