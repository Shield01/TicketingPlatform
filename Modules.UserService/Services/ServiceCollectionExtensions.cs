using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Modules.UserService.Repositories;
using Modules.UserService.Services;
using Shared.Kernel.Infrastructure.Database;

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
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                // Register DbContext with PostgreSQL
                services.AddUserPersistence(configuration);
            }
            else
            {
                // Fallback to in-memory database (for testing)
                services.AddDbContext<UserDbContext>(options =>
                    options.UseInMemoryDatabase("UserServiceDb"));
            }

            // Register Repository
            services.AddScoped<IUserRepository, UserRepository>();

            // Register Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<Shared.Kernel.Interfaces.IUserInfoService, UserInfoService>();

            return services;
        }

        /// <summary>
        /// Registers UserService persistence layer with PostgreSQL.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddUserPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var connectionString = ConnectionStringHelper.GetPostgresConnectionString(configuration);
                Console.WriteLine("[UserService] PostgreSQL connection configured successfully");
                
                // Add Supabase fallback connection if available
                var fallbackConnectionString = TryGetSupabaseFallback(configuration);
                
                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseNpgsql(connectionString, npgOptions =>
                    {
                        npgOptions.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName);
                        npgOptions.EnableRetryOnFailure(
                            maxRetryCount: 5, // Increased for Supabase
                            maxRetryDelay: TimeSpan.FromSeconds(30), // Increased for remote connections
                            errorCodesToAdd: new[] { "XX000", "08006", "08001" }); // Include Supabase-specific error codes
                        npgOptions.CommandTimeout(120); // Increased for remote connections
                    });
                    
                    // Add connection resilience logging
                    options.LogTo(Console.WriteLine, LogLevel.Warning);
                    options.EnableSensitiveDataLogging(false);
                    options.EnableDetailedErrors(true);
                });
                
                if (!string.IsNullOrEmpty(fallbackConnectionString))
                {
                    Console.WriteLine("[UserService] Supabase fallback connection available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserService] CRITICAL ERROR: Failed to configure PostgreSQL connection: {ex.Message}");
                throw; // Re-throw to prevent module from being registered with invalid config
            }

            return services;
        }
        
        /// <summary>
        /// Attempts to get a Supabase fallback connection string.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>A fallback connection string or null.</returns>
        private static string? TryGetSupabaseFallback(IConfiguration configuration)
        {
            try
            {
                // Try to get the original DATABASE_URL to create a fallback
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                                 configuration.GetConnectionString("Postgres");
                                 
                if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.Contains("supabase.com"))
                {
                    return ConnectionStringHelper.CreateSupabaseFallbackConnection(databaseUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserService] Could not create Supabase fallback: {ex.Message}");
            }
            
            return null;
        }
    }
} 