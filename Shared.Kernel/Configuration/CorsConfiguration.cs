using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Kernel.Configuration;

/// <summary>
/// Configuration model for CORS settings
/// </summary>
public class CorsConfiguration
{
    /// <summary>
    /// Configuration section key for CORS settings
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// List of allowed origins for CORS
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();

    /// <summary>
    /// List of allowed HTTP methods for CORS
    /// </summary>
    public List<string> AllowedMethods { get; set; } = new();

    /// <summary>
    /// List of allowed headers for CORS
    /// </summary>
    public List<string> AllowedHeaders { get; set; } = new();

    /// <summary>
    /// Whether to allow credentials in CORS requests
    /// </summary>
    public bool AllowCredentials { get; set; } = true;

    /// <summary>
    /// Maximum age for preflight request caching (in seconds)
    /// </summary>
    public int MaxAge { get; set; } = 3600;

    /// <summary>
    /// CORS policy name for the application
    /// </summary>
    public const string PolicyName = "TicketingPlatformCorsPolicy";

    /// <summary>
    /// Gets the allowed origins from environment variables or configuration
    /// Supports comma-separated values in environment variables
    /// </summary>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>List of allowed origins</returns>
    public static List<string> GetAllowedOriginsFromEnvironment(IConfiguration configuration)
    {
        var origins = new List<string>();

        // Check environment variable first (comma-separated)
        var envOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        if (!string.IsNullOrEmpty(envOrigins))
        {
            origins.AddRange(envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim()));
        }

        // If no environment variable, fall back to configuration
        if (origins.Count == 0)
        {
            var configOrigins = configuration.GetSection($"{SectionName}:AllowedOrigins").Get<List<string>>();
            if (configOrigins != null)
            {
                origins.AddRange(configOrigins);
            }
        }

        return origins;
    }

    /// <summary>
    /// Validates the CORS configuration
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        return AllowedOrigins.Count > 0 && AllowedMethods.Count > 0 && AllowedHeaders.Count > 0;
    }

    /// <summary>
    /// Gets default CORS configuration for development environment
    /// </summary>
    /// <returns>Development CORS configuration</returns>
    public static CorsConfiguration GetDevelopmentDefault()
    {
        return new CorsConfiguration
        {
            AllowedOrigins = new List<string>
            {
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:5173",
                "http://localhost:8080",
                "https://localhost:3000",
                "https://localhost:3001",
                "https://localhost:5173",
                "https://localhost:8080"
            },
            AllowedMethods = new List<string> { "*" },
            AllowedHeaders = new List<string> { "*" },
            AllowCredentials = true,
            MaxAge = 3600
        };
    }

    /// <summary>
    /// Gets default CORS configuration for production environment
    /// </summary>
    /// <returns>Production CORS configuration</returns>
    public static CorsConfiguration GetProductionDefault()
    {
        return new CorsConfiguration
        {
            AllowedOrigins = new List<string>(), // Must be explicitly configured
            AllowedMethods = new List<string> { "GET", "POST", "PUT", "DELETE", "OPTIONS" },
            AllowedHeaders = new List<string> { "Content-Type", "Authorization", "X-Requested-With" },
            AllowCredentials = true,
            MaxAge = 86400
        };
    }
}
