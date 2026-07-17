#!/bin/bash
set -e

# Create multiple databases from POSTGRES_MULTIPLE_DATABASES env var
if [ -n "$POSTGRES_MULTIPLE_DATABASES" ]; then
  IFS=',' read -ra DB_ARRAY <<< "$POSTGRES_MULTIPLE_DATABASES"
  for db in "${DB_ARRAY[@]}"; do
    echo "  Creating database '$db'"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" \
      -c "CREATE DATABASE $db;"
  done
  echo "  All databases created successfully"
fi
