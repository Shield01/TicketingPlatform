using Modules.UserService.Services;
using Modules.UserService.Controllers;
using Modules.TeamService.Services;
using Modules.EventService.Services;
using Modules.EventService.Controllers;
using Modules.TicketService.Services;
using Modules.TicketService.Controllers;
using Modules.PaymentService.Services;
using Modules.PaymentService.Controllers;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Shared.Kernel.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Shared.Kernel.Constants;
using Shared.Kernel.Extensions;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Modules.UserService.Controllers.UserController).Assembly)
    .AddApplicationPart(typeof(Modules.EventService.Controllers.EventController).Assembly)
    .AddApplicationPart(typeof(Modules.TicketService.Controllers.TicketController).Assembly)
    .AddApplicationPart(typeof(Modules.PaymentService.Controllers.PaymentController).Assembly);

// Configure CORS
builder.Services.AddConfiguredCors(builder.Configuration, builder.Environment);

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ticketing Platform API",
        Version = "v1",
        Description = "A comprehensive API for managing events, tickets, payments, and user authentication.",
        Contact = new OpenApiContact
        {
            Name = "Ticketing Platform Team",
            Email = "support@ticketingplatform.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Group endpoints by modules
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        return new[] { api.ActionDescriptor.RouteValues["controller"] ?? "Default" };
    });

    // Add XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Include XML documentation from module assemblies
    var userAssembly = typeof(Modules.UserService.Controllers.UserController).Assembly;
    var eventAssembly = typeof(Modules.EventService.Controllers.EventController).Assembly;
    var ticketAssembly = typeof(Modules.TicketService.Controllers.TicketController).Assembly;
    var paymentAssembly = typeof(Modules.PaymentService.Controllers.PaymentController).Assembly;
    
    var assemblies = new[] { userAssembly, eventAssembly, ticketAssembly, paymentAssembly };
    
    foreach (var assembly in assemblies)
    {
        var assemblyXmlFile = $"{assembly.GetName().Name}.xml";
        var assemblyXmlPath = Path.Combine(AppContext.BaseDirectory, assemblyXmlFile);
        if (File.Exists(assemblyXmlPath))
        {
            options.IncludeXmlComments(assemblyXmlPath);
        }
    }

    // Enable annotations
    options.EnableAnnotations();
});

// Configure JWT Authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                   builder.Configuration["Jwt:SecretKey"] ?? 
                   throw new InvalidOperationException("JWT SecretKey not configured");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                builder.Configuration["Jwt:Issuer"] ?? 
                "TicketingPlatform";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                  builder.Configuration["Jwt:Audience"] ?? 
                  "TicketingPlatform";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    // Admin policy - only Admin role
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(RbacConstants.Roles.Admin));
    
    // Organiser policy - Admin or Organiser roles
    options.AddPolicy("OrganiserOrAdmin", policy => 
        policy.RequireRole(RbacConstants.Roles.Admin, RbacConstants.Roles.Organiser));
    
    // Staff policy - Admin, Organiser, or Staff roles
    options.AddPolicy("StaffOrHigher", policy => 
        policy.RequireRole(RbacConstants.Roles.Admin, RbacConstants.Roles.Organiser, RbacConstants.Roles.Staff));
    
    // Authenticated user policy
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// Add health checks with PostgreSQL
Console.WriteLine("=== DATABASE CONNECTION SETUP ===");
string postgresConnectionString;
try
{
    postgresConnectionString = Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.GetPostgresConnectionString(builder.Configuration);
    Console.WriteLine("[STARTUP] PostgreSQL connection string resolved successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"[STARTUP] CRITICAL ERROR: Failed to resolve PostgreSQL connection string: {ex.Message}");
    throw; // Re-throw to prevent app from starting with invalid config
}

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: postgresConnectionString,
        name: "postgresql",
        tags: new[] { "database", "postgres" });

// Register module services with configuration
builder.Services.AddUserModule(builder.Configuration);
builder.Services.AddTeamModule(builder.Configuration);
builder.Services.AddEventModule(builder.Configuration);
builder.Services.AddTicketModule(builder.Configuration);
builder.Services.AddPaymentModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    // Enable Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticketing Platform API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Ticketing Platform API Documentation";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        options.DisplayRequestDuration();
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

// Use CORS middleware (must be before authentication)
app.UseConfiguredCors();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// // Map module endpoints
// app.MapUserEndpoints();
// app.MapEventEndpoints();
// app.MapTicketEndpoints();
// app.MapPaymentEndpoints();

// Map health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration,
                description = entry.Value.Description,
                tags = entry.Value.Tags
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
})
.WithName("HealthCheck")
.WithTags("Health");

app.MapGet("/health/ready", () => Results.Ok(new { status = "Ready", timestamp = DateTime.UtcNow }))
   .WithName("ReadinessCheck")
   .WithTags("Health");

// Debug endpoint for connection string investigation (only for development/staging)
app.MapGet("/debug/connection", (IConfiguration configuration) =>
{
    try
    {
        // Get all environment variables related to database
        var postgresConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        var configConnection = configuration.GetConnectionString("Postgres");
        
        // Get the actual connection string that would be used
        string actualConnectionString;
        string source;
        
        try
        {
            actualConnectionString = Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.GetPostgresConnectionString(configuration);
            source = "Successfully resolved";
        }
        catch (Exception ex)
        {
            actualConnectionString = $"ERROR: {ex.Message}";
            source = "Failed to resolve";
        }

        // Create Supabase fallback if available
        string? fallbackConnection = null;
        var originalUrl = databaseUrl ?? configConnection;
        if (!string.IsNullOrEmpty(originalUrl) && originalUrl.Contains("supabase.com"))
        {
            fallbackConnection = Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.CreateSupabaseFallbackConnection(originalUrl);
        }
        
        var debugInfo = new
        {
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            connectionSources = new
            {
                postgresConnection_EnvVar = string.IsNullOrEmpty(postgresConnection) ? "[NOT SET]" : Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskConnectionString(postgresConnection),
                databaseUrl_EnvVar = string.IsNullOrEmpty(databaseUrl) ? "[NOT SET]" : Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskUrl(databaseUrl),
                appsettingsPostgres = string.IsNullOrEmpty(configConnection) ? "[NOT SET]" : 
                    configConnection.StartsWith("postgres://") ? 
                        Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskUrl(configConnection) : 
                        Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskConnectionString(configConnection)
            },
            resolved = new
            {
                source = source,
                connectionString = actualConnectionString.StartsWith("ERROR:") ? actualConnectionString : Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskConnectionString(actualConnectionString),
                supabaseFallback = string.IsNullOrEmpty(fallbackConnection) ? "[NOT AVAILABLE]" : Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskConnectionString(fallbackConnection)
            },
            allEnvironmentVariables = Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .Where(x => x.Key.ToString()!.ToUpper().Contains("DATABASE") || 
                           x.Key.ToString()!.ToUpper().Contains("POSTGRES") ||
                           x.Key.ToString()!.ToUpper().Contains("CONNECTION") ||
                           x.Key.ToString()!.ToUpper().Contains("ASPNETCORE") ||
                           x.Key.ToString()!.ToUpper().Contains("RENDER") ||
                           x.Key.ToString()!.ToUpper().Contains("PORT"))
                .ToDictionary(x => x.Key.ToString()!, x => 
                {
                    var value = x.Value?.ToString() ?? "";
                    if (value.StartsWith("postgres://"))
                        return Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskUrl(value);
                    else if (value.Contains("Password=") || value.Contains("password="))
                        return Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.MaskConnectionString(value);
                    else
                        return value;
                }),
            deploymentInfo = new
            {
                isRender = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER")),
                renderServiceId = Environment.GetEnvironmentVariable("RENDER_SERVICE_ID"),
                renderServiceName = Environment.GetEnvironmentVariable("RENDER_SERVICE_NAME"),
                port = Environment.GetEnvironmentVariable("PORT"),
                aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
            }
        };
        
        return Results.Ok(debugInfo);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Debug endpoint error: {ex.Message}");
    }
})
.WithName("ConnectionDebug")
.WithTags("Debug")
.WithOpenApi(operation => new(operation)
{
    Summary = "Debug connection string resolution",
    Description = "Shows how the database connection string is being resolved (passwords masked)"
});

// Test connection endpoint for Supabase debugging
app.MapGet("/debug/test-connection", async (IConfiguration configuration) =>
{
    try
    {
        var connectionString = Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.GetPostgresConnectionString(configuration);
        
        using var connection = new Npgsql.NpgsqlConnection(connectionString);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await connection.OpenAsync();
        stopwatch.Stop();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 'Connection successful!' as message";
        var testQuery = (string)(await command.ExecuteScalarAsync() ?? "");
        
        return Results.Ok(new
        {
            status = "SUCCESS",
            message = testQuery,
            connectionTime = $"{stopwatch.ElapsedMilliseconds}ms",
            serverVersion = connection.ServerVersion,
            database = connection.Database,
            host = connection.Host,
            port = connection.Port,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        // Try fallback connection for Supabase
        var originalUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? configuration.GetConnectionString("Postgres");
        if (!string.IsNullOrEmpty(originalUrl) && originalUrl.Contains("supabase.com"))
        {
            try
            {
                var fallbackConnection = Shared.Kernel.Infrastructure.Database.ConnectionStringHelper.CreateSupabaseFallbackConnection(originalUrl);
                if (!string.IsNullOrEmpty(fallbackConnection))
                {
                    using var connection = new Npgsql.NpgsqlConnection(fallbackConnection);
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    await connection.OpenAsync();
                    stopwatch.Stop();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT 'Fallback connection successful!' as message";
                    var testQuery = (string)(await command.ExecuteScalarAsync() ?? "");
                    
                    return Results.Ok(new
                    {
                        status = "SUCCESS_FALLBACK",
                        message = testQuery,
                        connectionTime = $"{stopwatch.ElapsedMilliseconds}ms",
                        serverVersion = connection.ServerVersion,
                        database = connection.Database,
                        host = connection.Host,
                        port = connection.Port,
                        originalError = ex.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception fallbackEx)
            {
                return Results.Problem($"Both primary and fallback connections failed. Primary: {ex.Message}. Fallback: {fallbackEx.Message}");
            }
        }
        
        return Results.Problem($"Connection failed: {ex.Message}");
    }
})
.WithName("TestConnection")
.WithTags("Debug")
.WithOpenApi(operation => new(operation)
{
    Summary = "Test database connection",
    Description = "Attempts to connect to the database and run a simple query, with automatic Supabase fallback"
});

app.Run();
