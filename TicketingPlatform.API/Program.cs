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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Modules.UserService.Controllers.UserController).Assembly)
    .AddApplicationPart(typeof(Modules.EventService.Controllers.EventController).Assembly)
    .AddApplicationPart(typeof(Modules.TicketService.Controllers.TicketController).Assembly)
    .AddApplicationPart(typeof(Modules.PaymentService.Controllers.PaymentController).Assembly);

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
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TicketingPlatform";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TicketingPlatform";

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

// Register module services
builder.Services.AddUserModule();
builder.Services.AddTeamModule(); // Using in-memory database
builder.Services.AddEventModule(); // Using in-memory database
builder.Services.AddTicketModule();
builder.Services.AddPaymentModule();

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

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// // Map module endpoints
// app.MapUserEndpoints();
// app.MapEventEndpoints();
// app.MapTicketEndpoints();
// app.MapPaymentEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithTags("Health");

app.Run();
