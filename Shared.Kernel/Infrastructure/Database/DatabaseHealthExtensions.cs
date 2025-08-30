using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shared.Kernel.Infrastructure.Database
{
    /// <summary>
    /// Extension methods for adding database health checks.
    /// </summary>
    public static class DatabaseHealthExtensions
    {
        /// <summary>
        /// Adds PostgreSQL health check to the service collection.
        /// </summary>
        /// <param name="builder">The health checks builder.</param>
        /// <param name="connectionString">The PostgreSQL connection string.</param>
        /// <param name="name">The name of the health check.</param>
        /// <param name="failureStatus">The failure status to report.</param>
        /// <param name="tags">Tags to associate with the health check.</param>
        /// <returns>The health checks builder for chaining.</returns>
        public static IHealthChecksBuilder AddPostgreSQL(
            this IHealthChecksBuilder builder,
            string connectionString,
            string name = "postgresql",
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null)
        {
            return builder.AddNpgSql(
                connectionString,
                healthQuery: "SELECT 1;",
                name: name,
                failureStatus: failureStatus,
                tags: tags);
        }

        /// <summary>
        /// Adds PostgreSQL health check with custom query to the service collection.
        /// </summary>
        /// <param name="builder">The health checks builder.</param>
        /// <param name="connectionString">The PostgreSQL connection string.</param>
        /// <param name="healthQuery">The custom health check query.</param>
        /// <param name="name">The name of the health check.</param>
        /// <param name="failureStatus">The failure status to report.</param>
        /// <param name="tags">Tags to associate with the health check.</param>
        /// <returns>The health checks builder for chaining.</returns>
        public static IHealthChecksBuilder AddPostgreSQLWithQuery(
            this IHealthChecksBuilder builder,
            string connectionString,
            string healthQuery,
            string name = "postgresql",
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null)
        {
            return builder.AddNpgSql(
                connectionString,
                healthQuery: healthQuery,
                name: name,
                failureStatus: failureStatus,
                tags: tags);
        }
    }
}
