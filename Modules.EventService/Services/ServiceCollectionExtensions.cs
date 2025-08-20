using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.EventService.Data;
using Modules.EventService.Repositories;
using Modules.EventService.Services;

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
        /// <param name="connectionString">The database connection string (optional for in-memory).</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddEventModule(this IServiceCollection services, string? connectionString = null)
        {
            // Register DbContext with in-memory database for development
            // Use a static database name to ensure all context instances share the same data
            services.AddDbContext<EventServiceDbContext>(options =>
                options.UseInMemoryDatabase("EventServiceDb")
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors());

            // Register repositories
            services.AddScoped<IEventRepository, EventRepository>();

            // Register services
            services.AddScoped<IEventService, EventService>();

            return services;
        }
    }
} 