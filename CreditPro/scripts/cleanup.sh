#!/bin/bash

echo "Cleaning up CreditPro..."
echo "==========================="

# Colors
GREEN='\033[0;32m'
NC='\033[0m'

print_success() {
    echo -e "${GREEN} $1${NC}"
}

# Stop and remove containers
echo "Stopping Docker containers..."
docker-compose down -v
print_success "Docker containers stopped and removed"

# Clean build artifacts
echo "Cleaning build artifacts..."
cd CreditPro
dotnet clean
cd ..

cd CreditPro.Tests
dotnet clean
cd ..
print_success "Build artifacts cleaned"

# Remove bin and obj directories
echo "Removing bin and obj directories..."
find . -type d -name "bin" -o -name "obj" | xargs rm -rf
print_success "bin and obj directories removed"

echo ""
print_success "Cleanup complete!"
