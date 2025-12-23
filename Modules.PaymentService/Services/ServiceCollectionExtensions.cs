using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Modules.PaymentService.Data;
using Modules.PaymentService.Configuration;
using Modules.PaymentService.Infrastructure;
using Shared.Kernel.Infrastructure.Database;
using Polly;
using Polly.Extensions.Http;

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
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                // Register DbContext with PostgreSQL
                services.AddPaymentsPersistence(configuration);
                
                // Register PayAza client
                services.AddPayAzaClient(configuration);
            }
            else
            {
                // Fallback to in-memory database (for testing)
                services.AddDbContext<PaymentServiceDbContext>(options =>
                    options.UseInMemoryDatabase("PaymentServiceDb"));
            }

            // Register repositories
            services.AddScoped<Repositories.IPaymentRepository, Repositories.PaymentRepository>();
            services.AddScoped<Repositories.IPayoutRepository, Repositories.PayoutRepository>();

            // Register services
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPayoutService, PayoutService>();
            services.AddScoped<IWebhookValidationService, WebhookValidationService>();
            services.AddScoped<IWebhookProcessingService, WebhookProcessingService>();

            // Register background services for TSQ (Transaction Status Query)
            services.AddHostedService<TransactionStatusQueryService>();

            return services;
        }

        /// <summary>
        /// Registers PaymentService persistence layer with PostgreSQL.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddPaymentsPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = ConnectionStringHelper.GetPostgresConnectionString(configuration);
            
            services.AddDbContext<PaymentServiceDbContext>(options =>
                options.UseNpgsql(connectionString, npgOptions =>
                {
                    npgOptions.MigrationsAssembly(typeof(PaymentServiceDbContext).Assembly.FullName);
                    npgOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

            return services;
        }

        /// <summary>
        /// Registers PayAza client with dependency injection.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The IServiceCollection instance.</returns>
        public static IServiceCollection AddPayAzaClient(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind PayAza configuration
            var payAzaConfig = new PayAzaConfiguration();
            configuration.GetSection(PayAzaConfiguration.SectionName).Bind(payAzaConfig);

            // Override with environment variables if present
            OverrideWithEnvironmentVariables(payAzaConfig);

            // Validate configuration
            if (!payAzaConfig.IsValid())
            {
                throw new InvalidOperationException(
                    $"PayAza configuration is invalid. Please check {PayAzaConfiguration.SectionName} section in appsettings.json or environment variables.");
            }

            // Register configuration as singleton
            services.AddSingleton(payAzaConfig);

            // Register HttpClient with Polly retry policy
            services.AddHttpClient<IPayAzaClient, PayAzaClient>(client =>
            {
                client.BaseAddress = new Uri(payAzaConfig.CurrentBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(payAzaConfig.TimeoutSeconds);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

            return services;
        }

        /// <summary>
        /// Overrides PayAza configuration with environment variables.
        /// </summary>
        private static void OverrideWithEnvironmentVariables(PayAzaConfiguration config)
        {
            config.ApiKeyTest = Environment.GetEnvironmentVariable("PAYAZA_API_KEY_TEST") ?? config.ApiKeyTest;
            config.ApiKeyLive = Environment.GetEnvironmentVariable("PAYAZA_API_KEY_LIVE") ?? config.ApiKeyLive;
            config.SecretKeyTest = Environment.GetEnvironmentVariable("PAYAZA_SECRET_KEY_TEST") ?? config.SecretKeyTest;
            config.SecretKeyLive = Environment.GetEnvironmentVariable("PAYAZA_SECRET_KEY_LIVE") ?? config.SecretKeyLive;
            config.Mode = Environment.GetEnvironmentVariable("PAYAZA_MODE") ?? config.Mode;
            config.MerchantKey = Environment.GetEnvironmentVariable("PAYAZA_MERCHANT_KEY") ?? config.MerchantKey;
            config.BaseUrlTest = Environment.GetEnvironmentVariable("PAYAZA_BASE_URL_TEST") ?? config.BaseUrlTest;
            config.BaseUrlLive = Environment.GetEnvironmentVariable("PAYAZA_BASE_URL_LIVE") ?? config.BaseUrlLive;
        }

        /// <summary>
        /// Gets the Polly retry policy for HTTP requests.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => (int)msg.StatusCode >= 500)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Logging would happen here in the PayAzaClient
                    });
        }
    }
} 