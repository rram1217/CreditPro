#!/bin/bash

echo "Starting CreditPro in Development Mode"
echo "=========================================="

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_success() {
    echo -e "${GREEN} $1${NC}"
}

print_info() {
    echo -e "${YELLOW} $1${NC}"
}

# Check if databases are running
if ! docker ps | grep -q creditpro-postgres; then
    print_info "Starting databases..."
    docker-compose up -d postgres dynamodb-local dynamodb-admin
    sleep 5
fi

print_success "Databases are running"

# Navigate to project
cd CreditPro

# Run with hot reload
print_info "Starting application with hot reload..."
print_info "Press Ctrl+C to stop"
echo ""

dotnet watch run
