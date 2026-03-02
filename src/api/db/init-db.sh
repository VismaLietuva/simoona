#!/bin/bash
# Start SQL Server in the background
/opt/mssql/bin/sqlservr &
PID=$!

# Wait until SQL Server is ready to accept connections
until /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" -b -No 2>/dev/null; do
    echo "Waiting for SQL Server to be ready..."
    sleep 3
done

echo "SQL Server ready - creating databases if they do not exist"
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -No -Q "
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SimoonaDB')
    CREATE DATABASE [SimoonaDB];
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SimoonaDBJobs')
    CREATE DATABASE [SimoonaDBJobs];
"
echo "Database initialisation complete"

# Hand off to the SQL Server process
wait $PID
