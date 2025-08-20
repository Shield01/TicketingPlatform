using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

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
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddTicketModule(this IServiceCollection services)
        {
            // Register TicketService dependencies here
            return services;
        }
    }
} 