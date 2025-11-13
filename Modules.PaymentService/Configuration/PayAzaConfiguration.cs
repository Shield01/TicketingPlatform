using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.Configuration
{
    /// <summary>
    /// Configuration settings for PayAza payment gateway integration.
    /// </summary>
    public class PayAzaConfiguration
    {
        /// <summary>
        /// The configuration section name in appsettings.json.
        /// </summary>
        public const string SectionName = "PayAza";

        /// <summary>
        /// PayAza API key for test environment.
        /// </summary>
        [Required]
        public string ApiKeyTest { get; set; } = string.Empty;

        /// <summary>
        /// PayAza API key for live environment.
        /// </summary>
        [Required]
        public string ApiKeyLive { get; set; } = string.Empty;

        /// <summary>
        /// PayAza secret key for test environment.
        /// </summary>
        [Required]
        public string SecretKeyTest { get; set; } = string.Empty;

        /// <summary>
        /// PayAza secret key for live environment.
        /// </summary>
        [Required]
        public string SecretKeyLive { get; set; } = string.Empty;

        /// <summary>
        /// PayAza mode: "test" or "live".
        /// </summary>
        [Required]
        public string Mode { get; set; } = "test";

        /// <summary>
        /// PayAza merchant key for identifying the merchant.
        /// </summary>
        [Required]
        public string MerchantKey { get; set; } = string.Empty;

        /// <summary>
        /// PayAza base URL for test environment.
        /// </summary>
        [Required]
        public string BaseUrlTest { get; set; } = "https://api-test.payaza.africa";

        /// <summary>
        /// PayAza base URL for live environment.
        /// </summary>
        [Required]
        public string BaseUrlLive { get; set; } = "https://api.payaza.africa";

        /// <summary>
        /// Timeout in seconds for API requests.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum number of retry attempts for failed requests.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Initial delay in milliseconds for exponential backoff.
        /// </summary>
        public int InitialBackoffDelayMs { get; set; } = 1000;

        /// <summary>
        /// Gets the current API key based on the configured mode.
        /// </summary>
        public string CurrentApiKey => IsLiveMode ? ApiKeyLive : ApiKeyTest;

        /// <summary>
        /// Gets the current secret key based on the configured mode.
        /// </summary>
        public string CurrentSecretKey => IsLiveMode ? SecretKeyLive : SecretKeyTest;

        /// <summary>
        /// Gets the current base URL based on the configured mode.
        /// </summary>
        public string CurrentBaseUrl => IsLiveMode ? BaseUrlLive : BaseUrlTest;

        /// <summary>
        /// Indicates whether the client is in live mode.
        /// </summary>
        public bool IsLiveMode => Mode.Equals("live", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether the client is in test mode.
        /// </summary>
        public bool IsTestMode => Mode.Equals("test", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the X-TenantID header value based on the current mode.
        /// </summary>
        public string TenantId => IsLiveMode ? "live" : "test";

        /// <summary>
        /// Validates the configuration settings.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(MerchantKey))
                return false;

            if (!IsTestMode && !IsLiveMode)
                return false;

            if (IsLiveMode)
            {
                return !string.IsNullOrWhiteSpace(ApiKeyLive) &&
                       !string.IsNullOrWhiteSpace(SecretKeyLive) &&
                       !string.IsNullOrWhiteSpace(BaseUrlLive);
            }

            return !string.IsNullOrWhiteSpace(ApiKeyTest) &&
                   !string.IsNullOrWhiteSpace(SecretKeyTest) &&
                   !string.IsNullOrWhiteSpace(BaseUrlTest);
        }

        /// <summary>
        /// Gets the authorization header value (Base64 encoded API key).
        /// </summary>
        /// <returns>The authorization header value.</returns>
        public string GetAuthorizationHeaderValue()
        {
            var apiKey = CurrentApiKey;
            var encodedKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(apiKey));
            return $"Payaza {encodedKey}";
        }
    }
}

