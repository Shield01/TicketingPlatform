using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Specialized;
using System.Text;

namespace Shared.Kernel.Infrastructure.Database
{
    /// <summary>
    /// Helper class for managing PostgreSQL connection strings across different environments.
    /// Supports both direct connection strings and DATABASE_URL format (Supabase/Heroku style).
    /// </summary>
    public static class ConnectionStringHelper
    {
        /// <summary>
        /// Gets the PostgreSQL connection string from environment variables or configuration.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>A properly formatted Npgsql connection string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no valid connection string is found.</exception>
        public static string GetPostgresConnectionString(IConfiguration configuration)
        {
            // Priority 1: Direct POSTGRES_CONNECTION environment variable
            var directConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
            if (!string.IsNullOrEmpty(directConnection))
            {
                return directConnection;
            }

            // Priority 2: DATABASE_URL environment variable (Supabase/Heroku style)
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                return ConvertDatabaseUrlToConnectionString(databaseUrl);
            }

            // Priority 3: Configuration from appsettings.json
            var configConnection = configuration.GetConnectionString("Postgres");
            if (!string.IsNullOrEmpty(configConnection))
            {
                // If it's already a DATABASE_URL format, convert it
                if (configConnection.StartsWith("postgres://"))
                {
                    return ConvertDatabaseUrlToConnectionString(configConnection);
                }
                return configConnection;
            }

            throw new InvalidOperationException(
                "No PostgreSQL connection string found. Please set POSTGRES_CONNECTION, DATABASE_URL environment variable, " +
                "or add 'Postgres' connection string to appsettings.json");
        }

        /// <summary>
        /// Converts a DATABASE_URL format connection string to Npgsql format.
        /// </summary>
        /// <param name="databaseUrl">The DATABASE_URL format string (postgres://user:password@host:port/database).</param>
        /// <returns>A properly formatted Npgsql connection string.</returns>
        /// <exception cref="ArgumentException">Thrown when the DATABASE_URL format is invalid.</exception>
        public static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
        {
            if (string.IsNullOrEmpty(databaseUrl))
            {
                throw new ArgumentException("Database URL cannot be null or empty.", nameof(databaseUrl));
            }

            try
            {
                var uri = new Uri(databaseUrl);
                
                if (uri.Scheme != "postgres" && uri.Scheme != "postgresql")
                {
                    throw new ArgumentException($"Invalid database URL scheme: {uri.Scheme}. Expected 'postgres' or 'postgresql'.");
                }

                var userInfo = uri.UserInfo?.Split(':');
                if (userInfo == null || userInfo.Length != 2)
                {
                    throw new ArgumentException("Database URL must contain username and password in format postgres://user:password@host:port/database");
                }

                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Username = Uri.UnescapeDataString(userInfo[0]),
                    Password = Uri.UnescapeDataString(userInfo[1]),
                    Database = uri.AbsolutePath.TrimStart('/'),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true,
                    CommandTimeout = 30,
                    Timeout = 15
                };

                // Add additional query parameters if present
                var query = uri.Query;
                if (!string.IsNullOrEmpty(query))
                {
                    var queryParams = ParseQueryString(query);
                    
                    // Handle common SSL parameters
                    if (queryParams.ContainsKey("sslmode"))
                    {
                        if (Enum.TryParse<SslMode>(queryParams["sslmode"], true, out var sslMode))
                        {
                            builder.SslMode = sslMode;
                        }
                    }

                    if (queryParams.ContainsKey("trust_server_certificate"))
                    {
                        if (bool.TryParse(queryParams["trust_server_certificate"], out var trustCert))
                        {
                            builder.TrustServerCertificate = trustCert;
                        }
                    }
                }

                return builder.ConnectionString;
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException($"Invalid DATABASE_URL format: {ex.Message}", nameof(databaseUrl), ex);
            }
        }

        /// <summary>
        /// Validates that a connection string can be used to create a valid NpgsqlConnection.
        /// </summary>
        /// <param name="connectionString">The connection string to validate.</param>
        /// <returns>True if the connection string is valid, false otherwise.</returns>
        public static bool IsValidConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }

            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                return !string.IsNullOrEmpty(builder.Host) && 
                       !string.IsNullOrEmpty(builder.Database) && 
                       !string.IsNullOrEmpty(builder.Username);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the schema prefix for multi-tenant or environment-based schema separation.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="defaultPrefix">The default prefix to use if none is configured.</param>
        /// <returns>The schema prefix string.</returns>
        public static string GetSchemaPrefix(IConfiguration configuration, string defaultPrefix = "")
        {
            var prefix = Environment.GetEnvironmentVariable("DB_SCHEMA_PREFIX") ?? 
                         configuration["Database:SchemaPrefix"] ?? 
                         defaultPrefix;
            
            return string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";
        }

        /// <summary>
        /// Simple query string parser for URL parameters.
        /// </summary>
        /// <param name="query">The query string to parse.</param>
        /// <returns>A dictionary of key-value pairs.</returns>
        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (string.IsNullOrEmpty(query))
                return result;

            // Remove leading '?' if present
            if (query.StartsWith("?"))
                query = query.Substring(1);

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = Uri.UnescapeDataString(keyValue[1]);
                    result[key] = value;
                }
            }

            return result;
        }
    }
}
