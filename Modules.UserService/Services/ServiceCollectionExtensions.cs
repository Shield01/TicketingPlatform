using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.UserService.Repositories;
using Modules.UserService.Services;

namespace Modules.UserService.Services
{
    /// <summary>
    /// Extension methods for registering UserService dependencies.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers UserService services and dependencies.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddUserModule(this IServiceCollection services)
        {
            // Register DbContext
            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("UserServiceDb"));

            // Register Repository
            services.AddScoped<IUserRepository, UserRepository>();

            // Register Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
} 