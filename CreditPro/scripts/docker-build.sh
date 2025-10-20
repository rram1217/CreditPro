#!/bin/bash

echo "Building CreditPro Docker Image"
echo "==================================="

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_success() {
    echo -e "${GREEN} $1${NC}"
}

print_info() {
    echo -e "${YELLOW} $1${NC}"
}

print_error() {
    echo -e "${RED} $1${NC}"
}

VERSION=${1:-latest}

print_info "Building Docker image with tag: creditpro:$VERSION"

# Build the image
docker build -t creditpro:$VERSION -f Dockerfile .

if [ $? -eq 0 ]; then
    print_success "Docker image built successfully!"
    echo ""
    echo "To run the container:"
    echo "  docker run -p 8080:8080 creditpro:$VERSION"
    echo ""
    echo "To run with docker-compose:"
    echo "  docker-compose up -d"
else
    print_error "Docker build failed!"
    exit 1
fi
