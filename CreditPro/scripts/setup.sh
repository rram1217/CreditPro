#!/bin/bash

echo "Setting up CreditPro Microservice..."
echo "========================================"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_success() {
    echo -e "${GREEN} $1${NC}"
}

print_error() {
    echo -e "${RED} $1${NC}"
}

print_info() {
    echo -e "${YELLOW} $1${NC}"
}

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker and try again."
    exit 1
fi
print_success "Docker is running"

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK is not installed. Please install .NET 8 SDK."
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
print_success ".NET SDK $DOTNET_VERSION is installed"

# Setup environment file
if [ ! -f .env ]; then
    cat > .env << EOF
# PostgreSQL Configuration
POSTGRES_DB=creditprodb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres123
POSTGRES_PORT=5432

# DynamoDB Configuration
AWS_SERVICE_URL=http://localhost:8000
AWS_REGION=us-east-1
AWS_TABLE_NAME=CreditProAuditEvents

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=creditprodb;Username=postgres;Password=postgres123
EOF
    print_success "Created .env file"
else
    print_info ".env file already exists"
fi

# Start databases
echo ""
print_info "Starting databases..."
docker-compose up -d postgres dynamodb-local dynamodb-admin

# Wait for databases
echo ""
print_info "Waiting for databases to be ready..."
sleep 5

# Check PostgreSQL
until docker exec creditpro-postgres pg_isready -U postgres &> /dev/null; do
    echo "Waiting for PostgreSQL..."
    sleep 2
done
print_success "PostgreSQL is ready"

# Check DynamoDB
until curl -s http://localhost:8000 > /dev/null; do
    echo "Waiting for DynamoDB..."
    sleep 2
done
print_success "DynamoDB is ready"

# Restore NuGet packages
echo ""
print_info "Restoring NuGet packages..."
cd CreditPro
dotnet restore
if [ $? -eq 0 ]; then
    print_success "NuGet packages restored"
else
    print_error "Failed to restore NuGet packages"
    exit 1
fi

# Build solution
echo ""
print_info "Building solution..."
dotnet build --no-restore
if [ $? -eq 0 ]; then
    print_success "Solution built successfully"
else
    print_error "Build failed"
    exit 1
fi

# Run migrations
echo ""
print_info "Running database migrations..."
dotnet ef database update
if [ $? -eq 0 ]; then
    print_success "Migrations applied successfully"
else
    print_error "Failed to apply migrations"
    cd ..
    exit 1
fi
cd ..

# Run tests
echo ""
print_info "Running tests..."
cd CreditPro.Tests
dotnet test --no-build --verbosity quiet
if [ $? -eq 0 ]; then
    print_success "All tests passed"
else
    print_error "Some tests failed"
fi
cd ..

echo ""
echo "========================================"
print_success "Development environment setup complete!"
echo ""
echo "Available services:"
echo "  - API:             https://localhost:7001"
echo "  - Swagger:         https://localhost:7001/swagger"
echo "  - PostgreSQL:      localhost:5432"
echo "  - DynamoDB:        localhost:8000"
echo "  - DynamoDB Admin:  http://localhost:8001"
echo ""
echo "To start the API:"
echo "  cd CreditPro"
echo "  dotnet run"
echo ""
echo "To start with hot reload:"
echo "  cd CreditPro"
echo "  dotnet watch run"
echo ""
