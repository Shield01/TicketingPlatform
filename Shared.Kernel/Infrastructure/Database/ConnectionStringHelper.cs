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
            string? connectionString = null;
            string source = "None";

            // Priority 1: Direct POSTGRES_CONNECTION environment variable
            var directConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
            if (!string.IsNullOrEmpty(directConnection))
            {
                connectionString = directConnection;
                source = "POSTGRES_CONNECTION environment variable";
                Console.WriteLine($"[ConnectionStringHelper] Using connection from: {source}");
                Console.WriteLine($"[ConnectionStringHelper] Masked connection: {MaskConnectionString(connectionString)}");
                return connectionString;
            }

            // Priority 2: DATABASE_URL environment variable (Supabase/Heroku style)
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                connectionString = ConvertDatabaseUrlToConnectionString(databaseUrl);
                source = "DATABASE_URL environment variable (converted from URI)";
                Console.WriteLine($"[ConnectionStringHelper] Using connection from: {source}");
                Console.WriteLine($"[ConnectionStringHelper] Original DATABASE_URL: {MaskUrl(databaseUrl)}");
                Console.WriteLine($"[ConnectionStringHelper] Converted connection: {MaskConnectionString(connectionString)}");
                return connectionString;
            }

            // Priority 3: Configuration from appsettings.json
            var configConnection = configuration.GetConnectionString("Postgres");
            if (!string.IsNullOrEmpty(configConnection))
            {
                // If it's already a DATABASE_URL format, convert it
                if (configConnection.StartsWith("postgres://") || configConnection.StartsWith("postgresql://"))
                {
                    connectionString = ConvertDatabaseUrlToConnectionString(configConnection);
                    source = "appsettings.json Postgres connection string (converted from URI)";
                    Console.WriteLine($"[ConnectionStringHelper] Using connection from: {source}");
                    Console.WriteLine($"[ConnectionStringHelper] Original DATABASE_URL: {MaskUrl(configConnection)}");
                    Console.WriteLine($"[ConnectionStringHelper] Converted connection: {MaskConnectionString(connectionString)}");
                }
                else
                {
                    connectionString = configConnection;
                    source = "appsettings.json Postgres connection string";
                    Console.WriteLine($"[ConnectionStringHelper] Using connection from: {source}");
                    Console.WriteLine($"[ConnectionStringHelper] Masked connection: {MaskConnectionString(connectionString)}");
                    
                    // Apply Supabase optimizations if this is a Supabase connection
                    if (connectionString.Contains("supabase.com"))
                    {
                        connectionString = ApplySupabaseOptimizationsToConnectionString(connectionString);
                    }
                }
                return connectionString;
            }

            Console.WriteLine("[ConnectionStringHelper] ERROR: No connection string found!");
            Console.WriteLine("[ConnectionStringHelper] Checked sources:");
            Console.WriteLine("  1. POSTGRES_CONNECTION environment variable: Not set");
            Console.WriteLine("  2. DATABASE_URL environment variable: Not set");
            Console.WriteLine("  3. appsettings.json 'Postgres' connection string: Not set");

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

                // Apply Supabase-specific optimizations
                if (IsSupabaseConnection(uri.Host))
                {
                    ApplySupabaseConnectionSettings(builder, uri);
                }

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
        /// Masks sensitive information in a connection string for logging purposes.
        /// </summary>
        /// <param name="connectionString">The connection string to mask.</param>
        /// <returns>A masked version of the connection string.</returns>
        public static string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "[EMPTY]";

            // If this is a DATABASE_URL format, use the URL masking method
            if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
            {
                return MaskUrl(connectionString);
            }

            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                
                // Mask password
                if (!string.IsNullOrEmpty(builder.Password))
                {
                    builder.Password = "***MASKED***";
                }
                
                return builder.ConnectionString;
            }
            catch
            {
                // If parsing fails, do basic masking
                var password = GetPasswordFromConnectionString(connectionString);
                if (!string.IsNullOrEmpty(password))
                {
                    return connectionString.Replace(password, "***MASKED***");
                }
                return connectionString;
            }
        }

        /// <summary>
        /// Masks sensitive information in a DATABASE_URL for logging purposes.
        /// </summary>
        /// <param name="databaseUrl">The database URL to mask.</param>
        /// <returns>A masked version of the database URL.</returns>
        public static string MaskUrl(string databaseUrl)
        {
            if (string.IsNullOrEmpty(databaseUrl))
                return "[EMPTY]";

            try
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo?.Split(':');
                
                if (userInfo != null && userInfo.Length == 2)
                {
                    var maskedUserInfo = $"{userInfo[0]}:***MASKED***";
                    return databaseUrl.Replace(uri.UserInfo, maskedUserInfo);
                }
                
                return databaseUrl;
            }
            catch
            {
                // Basic masking if URI parsing fails
                var atIndex = databaseUrl.IndexOf('@');
                var colonIndex = databaseUrl.LastIndexOf(':', atIndex);
                
                if (colonIndex > 0 && atIndex > colonIndex)
                {
                    var before = databaseUrl.Substring(0, colonIndex + 1);
                    var after = databaseUrl.Substring(atIndex);
                    return before + "***MASKED***" + after;
                }
                
                return databaseUrl;
            }
        }

        /// <summary>
        /// Extracts password from a connection string for masking purposes.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The password if found, empty string otherwise.</returns>
        private static string GetPasswordFromConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "";

            // Handle DATABASE_URL format (postgres://user:password@host:port/database)
            if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
            {
                try
                {
                    var uri = new Uri(connectionString);
                    var userInfo = uri.UserInfo?.Split(':');
                    if (userInfo != null && userInfo.Length == 2)
                    {
                        return Uri.UnescapeDataString(userInfo[1]);
                    }
                }
                catch
                {
                    // If URI parsing fails, try basic pattern matching
                    var atIndex = connectionString.IndexOf('@');
                    var colonIndex = connectionString.LastIndexOf(':', atIndex);
                    
                    if (colonIndex > 0 && atIndex > colonIndex)
                    {
                        return connectionString.Substring(colonIndex + 1, atIndex - colonIndex - 1);
                    }
                }
                
                return "";
            }

            // Handle standard connection string format (Host=...;Password=...;...)
            var patterns = new[] { "Password=", "password=", "Pwd=", "pwd=" };
            
            foreach (var pattern in patterns)
            {
                var startIndex = connectionString.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (startIndex >= 0)
                {
                    startIndex += pattern.Length;
                    var endIndex = connectionString.IndexOf(';', startIndex);
                    if (endIndex == -1) endIndex = connectionString.Length;
                    
                    return connectionString.Substring(startIndex, endIndex - startIndex);
                }
            }
            
            return "";
        }

        /// <summary>
        /// Determines if the connection is to a Supabase database.
        /// </summary>
        /// <param name="host">The database host.</param>
        /// <returns>True if this is a Supabase connection.</returns>
        private static bool IsSupabaseConnection(string host)
        {
            return !string.IsNullOrEmpty(host) && host.Contains("supabase.com");
        }

        /// <summary>
        /// Applies Supabase-specific connection settings and optimizations.
        /// </summary>
        /// <param name="builder">The connection string builder.</param>
        /// <param name="uri">The original database URI.</param>
        private static void ApplySupabaseConnectionSettings(NpgsqlConnectionStringBuilder builder, Uri uri)
        {
            Console.WriteLine("[ConnectionStringHelper] Applying Supabase-specific optimizations");
            
            // Supabase-specific SSL settings
            builder.SslMode = SslMode.Require;
            builder.TrustServerCertificate = false; // Supabase has valid certificates
            
            // Connection pool settings optimized for Supabase
            builder.MinPoolSize = 0;
            builder.MaxPoolSize = 10; // Conservative for pooler connections
            builder.ConnectionIdleLifetime = 300; // 5 minutes
            builder.ConnectionPruningInterval = 10;
            
            // Timeout settings for Supabase (they can be slower than local)
            builder.CommandTimeout = 60;  // Increased from 30
            builder.Timeout = 30;         // Increased from 15
            builder.KeepAlive = 30;
            
            // Handle Supabase connection pooler vs direct connection
            if (uri.Port == 6543)
            {
                Console.WriteLine("[ConnectionStringHelper] Using Supabase connection pooler (port 6543)");
                // Pooler-specific settings
                builder.Pooling = false; // Disable client-side pooling when using Supabase pooler
                builder.Multiplexing = false; // Disable multiplexing for pooler
            }
            else if (uri.Port == 5432)
            {
                Console.WriteLine("[ConnectionStringHelper] Using Supabase direct connection (port 5432)");
                // Direct connection settings
                builder.Pooling = true;
                builder.Multiplexing = true;
            }
            
            // Add pgbouncer compatibility settings for Supabase pooler
            if (uri.Port == 6543)
            {
                builder.IncludeErrorDetail = true;
                builder.LogParameters = false; // Reduce overhead for pooler
            }
            
            Console.WriteLine($"[ConnectionStringHelper] Applied Supabase settings: SSL={builder.SslMode}, Pool={builder.Pooling}, Timeout={builder.Timeout}s");
        }

        /// <summary>
        /// Creates an alternative connection string for Supabase fallback.
        /// </summary>
        /// <param name="originalConnectionString">The original connection string (either DATABASE_URL or standard format).</param>
        /// <returns>A fallback connection string or null if not applicable.</returns>
        public static string? CreateSupabaseFallbackConnection(string originalConnectionString)
        {
            if (string.IsNullOrEmpty(originalConnectionString) || !originalConnectionString.Contains("supabase.com"))
                return null;
                
            try
            {
                // Handle DATABASE_URL format (postgres://...)
                if (originalConnectionString.StartsWith("postgres://") || originalConnectionString.StartsWith("postgresql://"))
                {
                    return CreateSupabaseFallbackFromUrl(originalConnectionString);
                }
                // Handle standard connection string format (Server=...;Port=...;...)
                else if (originalConnectionString.Contains("Server=") || originalConnectionString.Contains("Host="))
                {
                    return CreateSupabaseFallbackFromConnectionString(originalConnectionString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConnectionStringHelper] Failed to create Supabase fallback: {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// Creates a fallback connection string from a DATABASE_URL format.
        /// </summary>
        /// <param name="originalDatabaseUrl">The original DATABASE_URL.</param>
        /// <returns>A fallback connection string or null.</returns>
        private static string? CreateSupabaseFallbackFromUrl(string originalDatabaseUrl)
        {
            var uri = new Uri(originalDatabaseUrl);
            
            // If using pooler (6543), try direct connection (5432)
            if (uri.Port == 6543)
            {
                var fallbackUrl = originalDatabaseUrl.Replace(":6543", ":5432");
                Console.WriteLine($"[ConnectionStringHelper] Creating Supabase fallback: pooler -> direct connection");
                return ConvertDatabaseUrlToConnectionString(fallbackUrl);
            }
            // If using direct (5432), try pooler (6543)
            else if (uri.Port == 5432)
            {
                var fallbackUrl = originalDatabaseUrl.Replace(":5432", ":6543");
                Console.WriteLine($"[ConnectionStringHelper] Creating Supabase fallback: direct -> pooler connection");
                return ConvertDatabaseUrlToConnectionString(fallbackUrl);
            }
            
            return null;
        }

        /// <summary>
        /// Creates a fallback connection string from a standard connection string format.
        /// </summary>
        /// <param name="originalConnectionString">The original connection string.</param>
        /// <returns>A fallback connection string or null.</returns>
        private static string? CreateSupabaseFallbackFromConnectionString(string originalConnectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(originalConnectionString);
                
                // If using pooler (6543), try direct connection (5432)
                if (builder.Port == 6543)
                {
                    Console.WriteLine($"[ConnectionStringHelper] Creating Supabase fallback: pooler (6543) -> direct (5432)");
                    builder.Port = 5432;
                    
                    // Apply direct connection settings
                    builder.Pooling = true;
                    builder.Multiplexing = true;
                    builder.SslMode = SslMode.Require;
                    builder.TrustServerCertificate = false;
                    
                    return builder.ConnectionString;
                }
                // If using direct (5432), try pooler (6543)
                else if (builder.Port == 5432)
                {
                    Console.WriteLine($"[ConnectionStringHelper] Creating Supabase fallback: direct (5432) -> pooler (6543)");
                    builder.Port = 6543;
                    
                    // Apply pooler settings
                    builder.Pooling = false;
                    builder.Multiplexing = false;
                    builder.SslMode = SslMode.Require;
                    builder.TrustServerCertificate = false;
                    
                    return builder.ConnectionString;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConnectionStringHelper] Error parsing connection string for fallback: {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// Applies Supabase optimizations to a standard connection string.
        /// </summary>
        /// <param name="connectionString">The original connection string.</param>
        /// <returns>The optimized connection string.</returns>
        private static string ApplySupabaseOptimizationsToConnectionString(string connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                
                Console.WriteLine("[ConnectionStringHelper] Applying Supabase-specific optimizations to connection string");
                
                // Supabase-specific SSL settings
                builder.SslMode = SslMode.Require;
                builder.TrustServerCertificate = false; // Supabase has valid certificates
                
                // Connection pool settings optimized for Supabase
                builder.MinPoolSize = 0;
                builder.MaxPoolSize = 10; // Conservative for pooler connections
                builder.ConnectionIdleLifetime = 300; // 5 minutes
                builder.ConnectionPruningInterval = 10;
                
                // Timeout settings for Supabase (they can be slower than local)
                builder.CommandTimeout = 60;  // Increased from 30
                builder.Timeout = 30;         // Increased from 15
                builder.KeepAlive = 30;
                
                // Handle Supabase connection pooler vs direct connection
                if (builder.Port == 6543)
                {
                    Console.WriteLine("[ConnectionStringHelper] Using Supabase connection pooler (port 6543)");
                    // Pooler-specific settings
                    builder.Pooling = false; // Disable client-side pooling when using Supabase pooler
                    builder.Multiplexing = false; // Disable multiplexing for pooler
                    builder.IncludeErrorDetail = true;
                    builder.LogParameters = false; // Reduce overhead for pooler
                }
                else if (builder.Port == 5432)
                {
                    Console.WriteLine("[ConnectionStringHelper] Using Supabase direct connection (port 5432)");
                    // Direct connection settings
                    builder.Pooling = true;
                    builder.Multiplexing = true;
                }
                
                Console.WriteLine($"[ConnectionStringHelper] Applied Supabase settings: SSL={builder.SslMode}, Pool={builder.Pooling}, Timeout={builder.Timeout}s");
                
                return builder.ConnectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConnectionStringHelper] Error applying Supabase optimizations: {ex.Message}");
                return connectionString; // Return original if optimization fails
            }
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
