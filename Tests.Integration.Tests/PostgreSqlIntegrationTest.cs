using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.UserService.Repositories;
using Modules.UserService.Models;
using Modules.EventService.Data;
using Modules.EventService.Models;
using Modules.TeamService.Data;
using Modules.TeamService.Models;
using Modules.TicketService.Data;
using Modules.PaymentService.Data;
using Shared.Kernel.Infrastructure.Database;
using System.Text.Json;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Tests.Integration.Tests
{
    /// <summary>
    /// Integration tests using real PostgreSQL database with Testcontainers.
    /// </summary>
    public class PostgreSqlIntegrationTest : IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;
        private readonly PostgreSqlContainer _postgresContainer;
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client;
        private string _connectionString = string.Empty;

        public PostgreSqlIntegrationTest(ITestOutputHelper output)
        {
            _output = output;
            
            // Configure PostgreSQL test container
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("ticketing_platform_test")
                .WithUsername("test_user")
                .WithPassword("test_password")
                .WithPortBinding(0, 5432) // Random host port
                .WithEnvironment("POSTGRES_INITDB_ARGS", "--encoding=UTF8 --lc-collate=en_US.UTF-8 --lc-ctype=en_US.UTF-8")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready", "-h", "localhost", "-U", "test_user"))
                .Build();
        }

        public async Task InitializeAsync()
        {
            _output.WriteLine("Starting PostgreSQL container...");
            await _postgresContainer.StartAsync();
            
            _connectionString = _postgresContainer.GetConnectionString();
            _output.WriteLine($"PostgreSQL container started. Connection string: {_connectionString}");

            // Initialize schemas
            await InitializeDatabaseSchemasAsync();

            // Create test web application factory
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:Postgres"] = _connectionString,
                            ["POSTGRES_CONNECTION"] = _connectionString
                        });
                    });
                    
                    builder.ConfigureServices(services =>
                    {
                        // Override the connection string for all modules
                        Environment.SetEnvironmentVariable("POSTGRES_CONNECTION", _connectionString);
                    });

                    builder.ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Information);
                    });
                });

            _client = _factory.CreateClient();
            
            // Apply migrations to test database
            await ApplyMigrationsAsync();
            
            _output.WriteLine("Test environment initialized successfully");
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
            
            if (_postgresContainer != null)
            {
                _output.WriteLine("Stopping PostgreSQL container...");
                await _postgresContainer.StopAsync();
                await _postgresContainer.DisposeAsync();
                _output.WriteLine("PostgreSQL container stopped and disposed");
            }
        }

        private async Task InitializeDatabaseSchemasAsync()
        {
            using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var schemas = new[] { "users", "events", "teams", "tickets", "payments" };
            
            foreach (var schema in schemas)
            {
                var createSchemaCommand = connection.CreateCommand();
                createSchemaCommand.CommandText = $"CREATE SCHEMA IF NOT EXISTS {schema};";
                await createSchemaCommand.ExecuteNonQueryAsync();
                _output.WriteLine($"Created schema: {schema}");
            }

            await connection.CloseAsync();
        }

        private async Task ApplyMigrationsAsync()
        {
            using var scope = _factory!.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // Apply migrations for all contexts
                var userContext = services.GetRequiredService<UserDbContext>();
                await userContext.Database.MigrateAsync();
                _output.WriteLine("Applied UserService migrations");

                var eventContext = services.GetRequiredService<EventServiceDbContext>();
                await eventContext.Database.MigrateAsync();
                _output.WriteLine("Applied EventService migrations");

                var teamContext = services.GetRequiredService<TeamServiceDbContext>();
                await teamContext.Database.MigrateAsync();
                _output.WriteLine("Applied TeamService migrations");

                var ticketContext = services.GetRequiredService<TicketServiceDbContext>();
                await ticketContext.Database.MigrateAsync();
                _output.WriteLine("Applied TicketService migrations");

                var paymentContext = services.GetRequiredService<PaymentServiceDbContext>();
                await paymentContext.Database.MigrateAsync();
                _output.WriteLine("Applied PaymentService migrations");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error applying migrations: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnHealthy()
        {
            // Act
            var response = await _client!.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            _output.WriteLine($"Health check response: {content}");
            
            var healthResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var status = healthResponse.GetProperty("status").GetString();
            
            Assert.Equal("Healthy", status);
            Assert.True(healthResponse.GetProperty("checks").EnumerateArray().Any(check => 
                check.GetProperty("name").GetString() == "postgresql"));
        }

        [Fact]
        public async Task Database_ShouldSupportCrossSchemaOperations()
        {
            using var scope = _factory!.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Arrange - Get contexts
            var userContext = services.GetRequiredService<UserDbContext>();
            var teamContext = services.GetRequiredService<TeamServiceDbContext>();
            var eventContext = services.GetRequiredService<EventServiceDbContext>();

            // Create a user in users schema
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FirstName = "John",
                LastName = "Doe",
                Role = "Organiser"
            };

            userContext.Users.Add(user);
            await userContext.SaveChangesAsync();

            // Create a team in teams schema
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                Description = "A test team",
                TeamLeaderId = user.Id
            };

            teamContext.Teams.Add(team);
            await teamContext.SaveChangesAsync();

            // Create an event in events schema that references the user and team
            var eventEntity = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Test Event",
                Description = "A test event",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(30).AddHours(2),
                Location = "Test Venue",
                OrganizerId = user.Id,
                TeamId = team.Id,
                IsPublic = true,
                IsPublished = true
            };

            eventContext.Events.Add(eventEntity);
            await eventContext.SaveChangesAsync();

            // Act & Assert - Verify cross-schema references work
            var savedEvent = await eventContext.Events
                .Include(e => e.Organizer)
                .Include(e => e.Team)
                .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);

            Assert.NotNull(savedEvent);
            Assert.Equal("Test Event", savedEvent.Title);
            Assert.Equal(user.Id, savedEvent.OrganizerId);
            Assert.Equal(team.Id, savedEvent.TeamId);

            _output.WriteLine($"Successfully created cross-schema references:");
            _output.WriteLine($"- User ID: {user.Id} in 'users' schema");
            _output.WriteLine($"- Team ID: {team.Id} in 'teams' schema");
            _output.WriteLine($"- Event ID: {eventEntity.Id} in 'events' schema");
        }

        [Fact]
        public async Task UserRegistration_ShouldWorkWithPostgreSQL()
        {
            // Arrange
            var registrationData = new
            {
                email = "integration.test@example.com",
                password = "SecurePassword123!",
                firstName = "Integration",
                lastName = "Test"
            };

            var json = JsonSerializer.Serialize(registrationData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("/api/users/register", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _output.WriteLine($"Registration response: {responseContent}");

            var registrationResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            Assert.True(registrationResponse.GetProperty("userId").TryGetGuid(out var userId));
            Assert.NotEqual(Guid.Empty, userId);

            // Verify in database
            using var scope = _factory!.Services.CreateScope();
            var userContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            
            var user = await userContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            Assert.NotNull(user);
            Assert.Equal("integration.test@example.com", user.Email);
            Assert.Equal("Integration", user.FirstName);
            Assert.Equal("Test", user.LastName);
        }

        [Fact]
        public async Task Database_ShouldPersistDataCorrectly()
        {
            using var scope = _factory!.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Test each module's database operations
            await TestUserModule(services);
            await TestTeamModule(services);
            await TestEventModule(services);
            await TestTicketModule(services);
            await TestPaymentModule(services);

            _output.WriteLine("All module database operations completed successfully");
        }

        private async Task TestUserModule(IServiceProvider services)
        {
            var context = services.GetRequiredService<UserDbContext>();

            var user = new User
            {
                Email = "module.test@example.com",
                PasswordHash = "hashed",
                FirstName = "Module",
                LastName = "Test",
                Role = "Admin"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "module.test@example.com");
            Assert.NotNull(savedUser);
            Assert.Equal("Admin", savedUser.Role);
            
            _output.WriteLine($"✅ UserModule test passed - User ID: {savedUser.Id}");
        }

        private async Task TestTeamModule(IServiceProvider services)
        {
            var context = services.GetRequiredService<TeamServiceDbContext>();

            var team = new Team
            {
                Name = "Module Test Team",
                Description = "Test team for module verification",
                TeamLeaderId = Guid.NewGuid() // Dummy ID for test
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var savedTeam = await context.Teams.FirstOrDefaultAsync(t => t.Name == "Module Test Team");
            Assert.NotNull(savedTeam);
            
            _output.WriteLine($"✅ TeamModule test passed - Team ID: {savedTeam.Id}");
        }

        private async Task TestEventModule(IServiceProvider services)
        {
            var context = services.GetRequiredService<EventServiceDbContext>();

            var eventEntity = new Event
            {
                Title = "Module Test Event",
                Description = "Test event for module verification",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Test Location",
                OrganizerId = Guid.NewGuid() // Dummy ID for test
            };

            context.Events.Add(eventEntity);
            await context.SaveChangesAsync();

            var savedEvent = await context.Events.FirstOrDefaultAsync(e => e.Title == "Module Test Event");
            Assert.NotNull(savedEvent);
            
            _output.WriteLine($"✅ EventModule test passed - Event ID: {savedEvent.Id}");
        }

        private async Task TestTicketModule(IServiceProvider services)
        {
            var context = services.GetRequiredService<TicketServiceDbContext>();

            var ticketTier = new Modules.TicketService.Models.TicketTier
            {
                EventId = Guid.NewGuid(),
                Name = "Test Tier",
                Price = 100.00m,
                MaxQuantity = 50
            };

            context.TicketTiers.Add(ticketTier);
            await context.SaveChangesAsync();

            var savedTier = await context.TicketTiers.FirstOrDefaultAsync(t => t.Name == "Test Tier");
            Assert.NotNull(savedTier);
            
            _output.WriteLine($"✅ TicketModule test passed - TicketTier ID: {savedTier.Id}");
        }

        private async Task TestPaymentModule(IServiceProvider services)
        {
            var context = services.GetRequiredService<PaymentServiceDbContext>();

            var payment = new Modules.PaymentService.Models.Payment
            {
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = "TEST_REF_123",
                Gateway = "Test Gateway",
                Amount = 250.00m,
                Currency = "USD",
                Status = "Completed"
            };

            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            var savedPayment = await context.Payments.FirstOrDefaultAsync(p => p.PaymentReference == "TEST_REF_123");
            Assert.NotNull(savedPayment);
            
            _output.WriteLine($"✅ PaymentModule test passed - Payment ID: {savedPayment.Id}");
        }
    }
}
