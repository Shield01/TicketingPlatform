using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Modules.TicketService.Data;
using Shared.Kernel.Infrastructure.Database;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Extension methods for registering TicketService dependencies.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TicketService services and dependencies.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddTicketModule(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                // Register DbContext with PostgreSQL
                services.AddTicketsPersistence(configuration);
            }
            else
            {
                // Fallback to in-memory database (for testing)
                services.AddDbContext<TicketServiceDbContext>(options =>
                    options.UseInMemoryDatabase("TicketServiceDb"));
            }

            // TODO: Register repositories and services when implemented
            // services.AddScoped<ITicketRepository, TicketRepository>();
            // services.AddScoped<ITicketService, TicketService>();

            return services;
        }

        /// <summary>
        /// Registers TicketService persistence layer with PostgreSQL.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddTicketsPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = ConnectionStringHelper.GetPostgresConnectionString(configuration);
            
            services.AddDbContext<TicketServiceDbContext>(options =>
                options.UseNpgsql(connectionString, npgOptions =>
                {
                    npgOptions.MigrationsAssembly(typeof(TicketServiceDbContext).Assembly.FullName);
                    npgOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

            return services;
        }
    }
} 