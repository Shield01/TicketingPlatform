# TicketingPlatform Backend

A modular, scalable ticketing platform backend built with ASP.NET Core (.NET 8+), designed for event organizers and attendees. This project follows best practices for maintainability, testability, and modular separation.

---

## 🏗️ Architecture Overview

- **Modular Monolith:**
  - Single API entry point (`TicketingPlatform.API`)
  - Four functional modules as class libraries:
    - `Modules.UserService`
    - `Modules.EventService`
    - `Modules.TicketService`
    - `Modules.PaymentService`
  - Shared kernel for cross-cutting concerns: `Shared.Kernel`
  - Each module exposes DI and endpoint extension methods for composition
- **Unit Test Projects:**
  - `Tests.UserService.Tests`, `Tests.EventService.Tests`, etc. (xUnit)

---

## 📁 Project Structure

```
/Modules.UserService
  /Controllers
  /Services
  /Repositories
  /Models
  /DTOs
/Modules.EventService
  ...
/Modules.TicketService
  ...
/Modules.PaymentService
  ...
/Shared.Kernel
  /Enums
  /Interfaces
  /BaseEntities
  /ResultTypes
/TicketingPlatform.API
/Tests.UserService.Tests
/Tests.EventService.Tests
/Tests.TicketService.Tests
/Tests.PaymentService.Tests
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK or later](https://dotnet.microsoft.com/download)
- [PostgreSQL 15+](https://www.postgresql.org/download/) (local installation or cloud)
- [Docker & Docker Compose](https://www.docker.com/) (recommended for local development)
- [Entity Framework Core CLI Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)

### 🐳 Quick Start with Docker (Recommended)

1. **Start PostgreSQL with Docker Compose:**
```sh
docker-compose up -d ticketing-db
```

2. **Apply database migrations:**
```sh
.\scripts\apply-migrations.ps1
```

3. **Build and run the API:**
```sh
dotnet build TicketingPlatform.sln
dotnet run --project TicketingPlatform.API
```

The API will be available at `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger`.

### 🔧 Manual Setup (Local PostgreSQL)

1. **Install and configure PostgreSQL:**
   - Install PostgreSQL 15+ locally
   - Create a database: `ticketing_platform_dev`
   - Create a user with appropriate permissions

2. **Configure connection string:**
   - Copy `env.example.txt` to `.env`
   - Set `POSTGRES_CONNECTION` with your local database details:
   ```
   POSTGRES_CONNECTION=Host=localhost;Port=5432;Database=ticketing_platform_dev;Username=your_user;Password=your_password;Sslmode=Disable
   ```

3. **Apply migrations and run:**
```sh
.\scripts\apply-migrations.ps1
dotnet run --project TicketingPlatform.API
```

### 🧪 Run Tests
```sh
# Unit tests only
dotnet test --filter "Category!=Integration"

# All tests (including integration tests with PostgreSQL)
dotnet test TicketingPlatform.sln

# Integration tests only
dotnet test Tests.Integration.Tests
```

---

## 🗄️ Database Architecture

### Multi-Schema PostgreSQL Design
The platform uses PostgreSQL with a **single database, multiple schemas** approach:

| Schema | Module | Tables |
|--------|--------|--------|
| `users` | UserService | `app_users` |
| `teams` | TeamService | `app_teams`, `app_team_members` |
| `events` | EventService | `app_events` |
| `tickets` | TicketService | `app_tickets`, `app_ticket_tiers` |
| `payments` | PaymentService | `app_payments`, `app_payment_items` |

### Connection String Configuration
The platform supports multiple connection string formats:

**Environment Variables (Priority Order):**
1. `POSTGRES_CONNECTION` - Direct Npgsql connection string
2. `DATABASE_URL` - Postgres URL format (Supabase/Heroku style)
3. `ConnectionStrings:Postgres` - Configuration file

**Examples:**
```sh
# Direct connection string
POSTGRES_CONNECTION=Host=localhost;Port=5432;Database=ticketing_dev;Username=admin;Password=pass;Sslmode=Disable

# Database URL (Supabase)
DATABASE_URL=postgres://user:password@db.supabase.co:5432/postgres

# Configuration file (appsettings.json)
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5432;Database=ticketing_dev;Username=admin;Password=pass"
}
```

### Database Migration Management

**Create Migrations:**
```sh
.\scripts\create-migrations.ps1 "MigrationName"
```

**Apply Migrations:**
```sh
.\scripts\apply-migrations.ps1
```

**Reset Database (Development Only):**
```sh
.\scripts\reset-database.ps1 -Confirm
```

**Manual Migration (Per Module):**
```sh
cd Modules.UserService
dotnet ef migrations add InitialCreate --context UserDbContext --startup-project ../TicketingPlatform.API
dotnet ef database update --context UserDbContext --startup-project ../TicketingPlatform.API
```

---

## 🏥 Health Monitoring

### Health Check Endpoints
- `GET /health` - Comprehensive health check with database connectivity
- `GET /health/ready` - Simple readiness probe

**Sample Health Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "duration": "00:00:00.0234567",
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "tags": ["database", "postgres"]
    }
  ]
}
```

---

## 🐳 Docker & Deployment

### Local Development with Docker
```sh
# Start database only
docker-compose up -d ticketing-db

# Start database with pgAdmin
docker-compose up -d ticketing-db ticketing-pgadmin

# Full stack (database + API)
docker-compose --profile full-stack up -d
```

### Production Deployment

**Environment Variables for Production:**
```sh
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=postgres://user:password@hostname:port/database
JWT_SECRET_KEY=your-production-secret-key-at-least-32-characters
```

**Supabase Setup:**
1. Create a Supabase project
2. Get the connection string from Settings > Database
3. Set `DATABASE_URL` environment variable
4. Run migrations: `.\scripts\apply-migrations.ps1 -Environment Production`

**Other Cloud Providers:**
- **Azure PostgreSQL:** Use connection string format with `Sslmode=Require`
- **AWS RDS:** Include region and SSL settings
- **Railway/Render:** Use provided `DATABASE_URL`

---

## 🧩 Module Composition
- Each module exposes:
  - `ServiceCollectionExtensions.cs` for DI registration (e.g., `services.AddUserModule()`)
  - `EndpointMapper.cs` for endpoint registration (e.g., `app.MapUserEndpoints()`)
- Compose modules in `TicketingPlatform.API/Program.cs`:
  ```csharp
  builder.Services.AddUserModule();
  app.MapUserEndpoints();
  // ...repeat for other modules
  ```

---

## 🧪 Testing

### Test Projects Structure
```
/Tests.UserService.Tests          # Unit tests for User module
/Tests.EventService.Tests         # Unit tests for Event module  
/Tests.TeamService.Tests          # Unit tests for Team module
/Tests.TicketService.Tests        # Unit tests for Ticket module
/Tests.PaymentService.Tests       # Unit tests for Payment module
/Tests.Integration.Tests          # Integration tests with real PostgreSQL
```

### Test Categories
- **Unit Tests:** Fast, isolated tests using in-memory databases
- **Integration Tests:** End-to-end tests using real PostgreSQL with Testcontainers
- **API Tests:** HTTP endpoint tests with full authentication

### Running Tests
```sh
# Run all tests
dotnet test

# Unit tests only (fast)
dotnet test --filter "Category!=Integration"

# Integration tests only (requires Docker)
dotnet test Tests.Integration.Tests

# Test with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Database Testing Strategy
- **Unit Tests:** Use EF Core InMemory provider for fast, isolated tests
- **Integration Tests:** Use PostgreSQL Testcontainers for realistic database testing
- **CI/CD:** Automated testing with ephemeral PostgreSQL containers

### Test Data Management
- Each test module creates isolated test data
- Cross-schema relationship testing in integration tests
- Automatic cleanup between test runs

---

## 🛡️ Best Practices

### Development Guidelines
- **Documentation:** 100% XML documentation on all public methods and classes
- **Testing:** 95-100% test coverage across all modules
- **Database:** Use migrations for all schema changes
- **Security:** JWT authentication with role-based authorization
- **Logging:** Structured logging with correlation IDs
- **Error Handling:** Consistent error responses across all APIs

### Database Best Practices
- **Schema Separation:** Each module owns its schema and tables
- **Migrations:** Version-controlled, incremental database changes
- **Indexing:** Strategic indexes on foreign keys and frequently queried columns
- **Transactions:** Use DbContext transaction scope for data consistency
- **Connection Pooling:** Leveraged automatically by Npgsql

### Security Guidelines
- **Environment Variables:** Never commit secrets to source control
- **Connection Strings:** Use environment variables in production
- **SSL/TLS:** Enable SSL mode for production databases
- **RBAC:** Implement role-based access control for all sensitive operations
- **Health Checks:** Monitor database connectivity and application health

---

## 🤝 Contributing
1. Fork the repo and create a feature branch.
2. Follow the modular structure and naming conventions.
3. Write unit tests for all new code.
4. Document all public methods and APIs.
5. Submit a pull request with a clear description.

---

## 📚 References
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [xUnit Documentation](https://xunit.net/docs/)
- [Swagger/OpenAPI](https://swagger.io/docs/)

---

## 📄 License
MIT 