using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Modules.TeamService.Data;
using Modules.TeamService.Repositories;
using Modules.TeamService.Services;

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
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddTeamModule(this IServiceCollection services)
        {
            // Register DbContext with in-memory database for development
            services.AddDbContext<TeamServiceDbContext>(options =>
                options.UseInMemoryDatabase("TeamServiceDb"));

            // Register repositories
            services.AddScoped<ITeamRepository, TeamRepository>();

            // Register services
            services.AddScoped<ITeamService, TeamService>();

            return services;
        }
    }
} 