using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Modules.EventService.Data;
using Modules.EventService.Repositories;
using Modules.EventService.Services;
using Shared.Kernel.Infrastructure.Database;

namespace Modules.EventService.Services
{
    /// <summary>
    /// Extension methods for registering EventService dependencies.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers EventService services and dependencies.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddEventModule(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                // Register DbContext with PostgreSQL
                services.AddEventsPersistence(configuration);
            }
            else
            {
                // Fallback to in-memory database (for testing)
                services.AddDbContext<EventServiceDbContext>(options =>
                    options.UseInMemoryDatabase("EventServiceDb")
                           .EnableSensitiveDataLogging()
                           .EnableDetailedErrors());
            }

            // Register repositories
            services.AddScoped<IEventRepository, EventRepository>();

            // Register services
            services.AddScoped<IEventService, EventService>();

            return services;
        }

        /// <summary>
        /// Registers EventService persistence layer with PostgreSQL.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddEventsPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var connectionString = ConnectionStringHelper.GetPostgresConnectionString(configuration);
                Console.WriteLine("[EventService] PostgreSQL connection configured successfully");
                
                services.AddDbContext<EventServiceDbContext>(options =>
                    options.UseNpgsql(connectionString, npgOptions =>
                    {
                        npgOptions.MigrationsAssembly(typeof(EventServiceDbContext).Assembly.FullName);
                        npgOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);
                        npgOptions.CommandTimeout(30);
                    }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventService] CRITICAL ERROR: Failed to configure PostgreSQL connection: {ex.Message}");
                throw; // Re-throw to prevent module from being registered with invalid config
            }

            return services;
        }
    }
} 