#!/bin/bash

echo "Database Migration Tool"
echo "=========================="

ACTION=$1
MIGRATION_NAME=$2

if [ -z "$ACTION" ]; then
    echo "Usage: ./scripts/migrate.sh [add|update|rollback|list|script] [migration-name]"
    echo ""
    echo "Commands:"
    echo "  add <name>       - Create a new migration"
    echo "  update           - Apply pending migrations"
    echo "  rollback [name]  - Rollback to a specific migration or all"
    echo "  list             - List all migrations"
    echo "  script [file]    - Generate SQL script"
    exit 1
fi

cd CreditPro

case $ACTION in
    add)
        if [ -z "$MIGRATION_NAME" ]; then
            echo "Error: Migration name required"
            echo "Usage: ./scripts/migrate.sh add MigrationName"
            exit 1
        fi
        echo "Creating migration: $MIGRATION_NAME"
        dotnet ef migrations add $MIGRATION_NAME
        ;;
    
    update)
        echo "Applying migrations..."
        dotnet ef database update
        ;;
    
    rollback)
        if [ -z "$MIGRATION_NAME" ]; then
            echo "Rolling back all migrations..."
            dotnet ef database update 0
        else
            echo "Rolling back to: $MIGRATION_NAME"
            dotnet ef database update $MIGRATION_NAME
        fi
        ;;
    
    list)
        echo "Listing migrations..."
        dotnet ef migrations list
        ;;
    
    script)
        OUTPUT_FILE=${MIGRATION_NAME:-"migration.sql"}
        echo "Generating SQL script: $OUTPUT_FILE"
        dotnet ef migrations script --output ../scripts/$OUTPUT_FILE
        echo "Script saved to: scripts/$OUTPUT_FILE"
        ;;
    
    *)
        echo "Unknown action: $ACTION"
        echo "Available actions: add, update, rollback, list, script"
        exit 1
        ;;
esac

cd ..
