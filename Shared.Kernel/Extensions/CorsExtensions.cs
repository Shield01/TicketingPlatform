using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Configuration;

namespace Shared.Kernel.Extensions;

/// <summary>
/// Extension methods for configuring CORS in the application
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds CORS services to the DI container with configuration from appsettings and environment variables
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddConfiguredCors(this IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment)
    {
        // Bind CORS configuration from appsettings
        var corsConfig = new CorsConfiguration();
        configuration.GetSection(CorsConfiguration.SectionName).Bind(corsConfig);

        // Override with environment variables if present
        var envOrigins = CorsConfiguration.GetAllowedOriginsFromEnvironment(configuration);
        if (envOrigins.Count > 0)
        {
            corsConfig.AllowedOrigins = envOrigins;
        }

        // Use defaults if configuration is incomplete
        if (!corsConfig.IsValid())
        {
            corsConfig = environment.IsDevelopment() 
                ? CorsConfiguration.GetDevelopmentDefault() 
                : CorsConfiguration.GetProductionDefault();
        }

        // Validate production configuration
        if (environment.IsProduction() && corsConfig.AllowedOrigins.Any(o => o.Contains("localhost")))
        {
            throw new InvalidOperationException(
                "CORS configuration error: localhost origins are not allowed in production environment. " +
                "Please configure allowed origins via CORS_ALLOWED_ORIGINS environment variable or appsettings.Production.json");
        }

        // Add CORS policy
        services.AddCors(options =>
        {
            options.AddPolicy(CorsConfiguration.PolicyName, policy =>
            {
                // Configure origins
                if (corsConfig.AllowedOrigins.Contains("*"))
                {
                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(corsConfig.AllowedOrigins.ToArray());
                }

                // Configure methods
                if (corsConfig.AllowedMethods.Contains("*"))
                {
                    policy.AllowAnyMethod();
                }
                else
                {
                    policy.WithMethods(corsConfig.AllowedMethods.ToArray());
                }

                // Configure headers
                if (corsConfig.AllowedHeaders.Contains("*"))
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders(corsConfig.AllowedHeaders.ToArray());
                }

                // Configure credentials (only if not allowing any origin)
                if (corsConfig.AllowCredentials && !corsConfig.AllowedOrigins.Contains("*"))
                {
                    policy.AllowCredentials();
                }

                // Set max age for preflight requests
                policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsConfig.MaxAge));
            });
        });

        // Log CORS configuration for debugging
        services.AddSingleton<ICorsConfigurationLogger, CorsConfigurationLogger>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ICorsConfigurationLogger>();
        logger?.LogCorsConfiguration(corsConfig, environment);

        return services;
    }

    /// <summary>
    /// Adds the configured CORS middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app)
    {
        return app.UseCors(CorsConfiguration.PolicyName);
    }
}

/// <summary>
/// Interface for CORS configuration logging
/// </summary>
public interface ICorsConfigurationLogger
{
    /// <summary>
    /// Logs the CORS configuration for debugging purposes
    /// </summary>
    /// <param name="corsConfig">The CORS configuration</param>
    /// <param name="environment">The hosting environment</param>
    void LogCorsConfiguration(CorsConfiguration corsConfig, IHostEnvironment environment);
}

/// <summary>
/// Implementation of CORS configuration logger
/// </summary>
public class CorsConfigurationLogger : ICorsConfigurationLogger
{
    private readonly ILogger<CorsConfigurationLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the CORS configuration logger
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public CorsConfigurationLogger(ILogger<CorsConfigurationLogger> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs the CORS configuration for debugging purposes
    /// </summary>
    /// <param name="corsConfig">The CORS configuration</param>
    /// <param name="environment">The hosting environment</param>
    public void LogCorsConfiguration(CorsConfiguration corsConfig, IHostEnvironment environment)
    {
        _logger.LogInformation("=== CORS CONFIGURATION ({Environment}) ===", environment.EnvironmentName);
        _logger.LogInformation("Allowed Origins: {Origins}", string.Join(", ", corsConfig.AllowedOrigins));
        _logger.LogInformation("Allowed Methods: {Methods}", string.Join(", ", corsConfig.AllowedMethods));
        _logger.LogInformation("Allowed Headers: {Headers}", string.Join(", ", corsConfig.AllowedHeaders));
        _logger.LogInformation("Allow Credentials: {AllowCredentials}", corsConfig.AllowCredentials);
        _logger.LogInformation("Max Age: {MaxAge} seconds", corsConfig.MaxAge);
        _logger.LogInformation("=== END CORS CONFIGURATION ===");
    }
}
