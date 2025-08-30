# PostgreSQL Setup Guide for Ticketing Platform

This guide provides detailed instructions for setting up PostgreSQL with the Ticketing Platform in various environments.

## 🏗️ Architecture Overview

The Ticketing Platform uses a **modular monolith** architecture with PostgreSQL:
- **Single Database:** `ticketing_platform_dev` (development) / `ticketing_platform` (production)
- **Multiple Schemas:** Each module gets its own schema for logical separation
- **Cross-Schema References:** Foreign keys can reference tables across schemas
- **Migration Management:** Per-module EF Core migrations with shared database

### Schema Layout
```
Database: ticketing_platform_dev
├── users schema
│   └── app_users
├── teams schema  
│   ├── app_teams
│   └── app_team_members
├── events schema
│   └── app_events
├── tickets schema
│   ├── app_tickets
│   └── app_ticket_tiers
└── payments schema
    ├── app_payments
    └── app_payment_items
```

## 🐳 Option 1: Docker Compose (Recommended)

### Quick Start
```bash
# Start PostgreSQL container
docker-compose up -d ticketing-db

# Apply all migrations
.\scripts\apply-migrations.ps1

# Start the API
dotnet run --project TicketingPlatform.API
```

### Container Details
- **Image:** `postgres:15-alpine`
- **Database:** `ticketing_platform_dev`
- **Username:** `ticketing_admin`
- **Password:** `ticketing_dev_password`
- **Port:** `5432` (mapped to host)
- **pgAdmin:** Available at `http://localhost:8080`

### Managing the Container
```bash
# View logs
docker-compose logs ticketing-db

# Connect to database
docker-compose exec ticketing-db psql -U ticketing_admin -d ticketing_platform_dev

# Stop and remove
docker-compose down
docker-compose down -v  # Also remove volumes
```

## 🔧 Option 2: Local PostgreSQL Installation

### Windows Installation
1. Download PostgreSQL from [postgresql.org](https://www.postgresql.org/download/windows/)
2. Run installer with default settings
3. Remember the password for `postgres` user
4. Add PostgreSQL bin directory to PATH

### macOS Installation
```bash
# Using Homebrew
brew install postgresql@15
brew services start postgresql@15

# Using MacPorts
sudo port install postgresql15-server
sudo port load postgresql15-server
```

### Linux Installation (Ubuntu/Debian)
```bash
# Install PostgreSQL
sudo apt update
sudo apt install postgresql postgresql-contrib

# Start service
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

### Database Setup
```sql
-- Connect as postgres superuser
psql -U postgres

-- Create database and user
CREATE DATABASE ticketing_platform_dev;
CREATE USER ticketing_admin WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE ticketing_platform_dev TO ticketing_admin;

-- Create schemas
\c ticketing_platform_dev
CREATE SCHEMA users;
CREATE SCHEMA teams;
CREATE SCHEMA events;
CREATE SCHEMA tickets;
CREATE SCHEMA payments;

-- Grant schema permissions
GRANT ALL ON SCHEMA users TO ticketing_admin;
GRANT ALL ON SCHEMA teams TO ticketing_admin;
GRANT ALL ON SCHEMA events TO ticketing_admin;
GRANT ALL ON SCHEMA tickets TO ticketing_admin;
GRANT ALL ON SCHEMA payments TO ticketing_admin;
```

## ☁️ Option 3: Cloud PostgreSQL (Production)

### Supabase Setup
1. Create account at [supabase.com](https://supabase.com)
2. Create a new project
3. Go to Settings > Database
4. Copy the connection string
5. Set environment variable:
   ```bash
   DATABASE_URL=postgres://postgres.xyz:[password]@aws-0-us-west-1.pooler.supabase.com:5432/postgres
   ```

### Azure Database for PostgreSQL
1. Create Azure Database for PostgreSQL resource
2. Configure firewall rules
3. Create connection string:
   ```bash
   POSTGRES_CONNECTION=Host=myserver.postgres.database.azure.com;Port=5432;Database=ticketing_platform;Username=myadmin@myserver;Password=mypassword;Sslmode=Require
   ```

### AWS RDS PostgreSQL
1. Launch RDS PostgreSQL instance
2. Configure security groups
3. Create connection string:
   ```bash
   POSTGRES_CONNECTION=Host=myinstance.123456789012.us-west-2.rds.amazonaws.com;Port=5432;Database=ticketing_platform;Username=postgres;Password=mypassword;Sslmode=Require
   ```

### Railway
1. Create Railway account and project
2. Add PostgreSQL service
3. Use provided DATABASE_URL:
   ```bash
   DATABASE_URL=postgres://postgres:password@containers-us-west-xyz.railway.app:1234/railway
   ```

## 🔧 Configuration Management

### Environment Variables (Priority Order)
1. `POSTGRES_CONNECTION` - Full Npgsql connection string
2. `DATABASE_URL` - PostgreSQL URL format
3. `ConnectionStrings:Postgres` - From appsettings.json

### Connection String Examples
```bash
# Local development
POSTGRES_CONNECTION=Host=localhost;Port=5432;Database=ticketing_platform_dev;Username=ticketing_admin;Password=pass;Sslmode=Disable

# Docker Compose
POSTGRES_CONNECTION=Host=ticketing-db;Port=5432;Database=ticketing_platform_dev;Username=ticketing_admin;Password=ticketing_dev_password;Sslmode=Disable

# Production with SSL
POSTGRES_CONNECTION=Host=prod-server;Port=5432;Database=ticketing_platform;Username=app_user;Password=secure_pass;Sslmode=Require;TrustServerCertificate=false

# Supabase URL format
DATABASE_URL=postgres://postgres.abcdefg:password@aws-0-us-west-1.pooler.supabase.com:5432/postgres
```

### Environment Files
```bash
# Development (.env)
ASPNETCORE_ENVIRONMENT=Development
POSTGRES_CONNECTION=Host=localhost;Port=5432;Database=ticketing_platform_dev;Username=ticketing_admin;Password=dev_password;Sslmode=Disable

# Production (.env)
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=postgres://user:password@prod-host:5432/database
JWT_SECRET_KEY=production-secret-key-minimum-32-characters
```

## 🗄️ Migration Management

### Automated Scripts
```bash
# Create migrations for all modules
.\scripts\create-migrations.ps1 "InitialCreate"

# Apply all migrations
.\scripts\apply-migrations.ps1

# Reset database (development only)
.\scripts\reset-database.ps1 -Confirm
```

### Manual Migration Commands
```bash
# Create migration for specific module
cd Modules.UserService
dotnet ef migrations add InitialCreate --context UserDbContext --startup-project ../TicketingPlatform.API --output-dir Data/Migrations

# Apply migrations
dotnet ef database update --context UserDbContext --startup-project ../TicketingPlatform.API

# Generate SQL script
dotnet ef migrations script --context UserDbContext --startup-project ../TicketingPlatform.API --output migration.sql
```

### CI/CD Migration Strategy
```bash
# In deployment pipeline
export POSTGRES_CONNECTION="production-connection-string"
.\scripts\apply-migrations.ps1 -Environment Production
```

## 🔍 Troubleshooting

### Common Issues

**Connection refused (local PostgreSQL)**
```bash
# Check if PostgreSQL is running
sudo systemctl status postgresql  # Linux
brew services list | grep postgresql  # macOS

# Check if port is open
netstat -an | grep 5432
```

**Authentication failed**
```sql
-- Reset user password
ALTER USER ticketing_admin PASSWORD 'new_password';

-- Check user privileges
\du ticketing_admin
```

**Schema permission errors**
```sql
-- Grant all privileges
GRANT ALL PRIVILEGES ON DATABASE ticketing_platform_dev TO ticketing_admin;
GRANT ALL ON SCHEMA users TO ticketing_admin;
-- Repeat for other schemas
```

**Migration failures**
```bash
# Check migration status
dotnet ef migrations list --context UserDbContext --startup-project TicketingPlatform.API

# Revert to specific migration
dotnet ef database update SpecificMigration --context UserDbContext --startup-project TicketingPlatform.API

# Remove last migration
dotnet ef migrations remove --context UserDbContext --startup-project TicketingPlatform.API
```

### Performance Tuning

**Connection Pooling (Automatic with Npgsql)**
```csharp
// Connection string parameters
Host=localhost;Port=5432;Database=ticketing_dev;Username=admin;Password=pass;
Maximum Pool Size=100;Minimum Pool Size=5;Connection Lifetime=0;Command Timeout=30
```

**Query Performance**
```sql
-- Enable query logging (development only)
ALTER SYSTEM SET log_statement = 'all';
SELECT pg_reload_conf();

-- Monitor slow queries
SELECT query, mean_time, calls FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 10;
```

**Index Monitoring**
```sql
-- Check unused indexes
SELECT * FROM pg_stat_user_indexes WHERE idx_scan = 0;

-- Check missing indexes (requires pg_stat_statements)
SELECT schemaname, tablename, attname, n_distinct, correlation 
FROM pg_stats 
WHERE schemaname IN ('users', 'events', 'teams', 'tickets', 'payments');
```

## 🧪 Testing with PostgreSQL

### Integration Tests
The platform includes comprehensive integration tests using Testcontainers:

```bash
# Run integration tests
dotnet test Tests.Integration.Tests

# View test logs
dotnet test Tests.Integration.Tests --logger "console;verbosity=detailed"
```

### Test Database Isolation
- Each test run uses a fresh PostgreSQL container
- Database schemas are created automatically
- All modules are tested with cross-schema relationships
- Tests verify data persistence and query performance

### Manual Testing
```bash
# Start test environment
docker-compose up -d ticketing-db
.\scripts\apply-migrations.ps1

# Run API tests
.\test_registration.ps1
.\test_login.ps1
.\test_swagger_auth.ps1

# Check health endpoint
curl http://localhost:5000/health
```

This setup provides a robust, scalable PostgreSQL foundation for the Ticketing Platform with proper separation of concerns and production-ready configuration.
