
echo "Running CreditPro Tests..."
echo "============================="

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

cd CreditPro.Tests

# Run tests with coverage
echo "Running tests with coverage..."
dotnet test /p:CollectCoverage=true \
            /p:CoverletOutputFormat=opencover \
            /p:CoverletOutput=./coverage/ \
            --logger "console;verbosity=normal"

# Check if tests passed
if [ $? -eq 0 ]; then
    echo ""
    print_success "All tests passed!"
    
    # Show coverage summary
    if [ -f "./coverage/coverage.opencover.xml" ]; then
        echo ""
        echo "Coverage report generated at: ./coverage/coverage.opencover.xml"
    fi
else
    echo ""
    print_error "Some tests failed!"
    exit 1
fi

cd ..
