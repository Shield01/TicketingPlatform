using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Modules.TeamService.Data;
using Modules.TeamService.Services;
using Modules.TeamService.Repositories;
using Shared.Kernel.Infrastructure.Database;

namespace Modules.TeamService.Services
{
    /// <summary>
    /// Extension methods for registering TeamService dependencies.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TeamService services and dependencies.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddTeamModule(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                // Register DbContext with PostgreSQL
                services.AddTeamsPersistence(configuration);
            }
            else
            {
                // Fallback to in-memory database (for testing)
                services.AddDbContext<TeamServiceDbContext>(options =>
                    options.UseInMemoryDatabase("TeamServiceDb"));
            }

            // Register repositories
            services.AddScoped<ITeamRepository, TeamRepository>();

            // Register services
            services.AddScoped<ITeamService, TeamService>();

            return services;
        }

        /// <summary>
        /// Registers TeamService persistence layer with PostgreSQL.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddTeamsPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = ConnectionStringHelper.GetPostgresConnectionString(configuration);
            
            services.AddDbContext<TeamServiceDbContext>(options =>
                options.UseNpgsql(connectionString, npgOptions =>
                {
                    npgOptions.MigrationsAssembly(typeof(TeamServiceDbContext).Assembly.FullName);
                    npgOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

            return services;
        }
    }
}