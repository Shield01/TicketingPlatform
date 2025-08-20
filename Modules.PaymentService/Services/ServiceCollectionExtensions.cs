using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Extension methods for registering PaymentService dependencies.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers PaymentService services and dependencies.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddPaymentModule(this IServiceCollection services)
        {
            // Register PaymentService dependencies here
            return services;
        }
    }
} 