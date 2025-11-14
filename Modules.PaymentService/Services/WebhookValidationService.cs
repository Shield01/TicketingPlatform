using Microsoft.Extensions.Logging;
using Modules.PaymentService.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service implementation for webhook signature validation using HMAC SHA512.
    /// </summary>
    public class WebhookValidationService : IWebhookValidationService
    {
        private readonly PayAzaConfiguration _configuration;
        private readonly ILogger<WebhookValidationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookValidationService"/> class.
        /// </summary>
        /// <param name="configuration">The PayAza configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public WebhookValidationService(
            PayAzaConfiguration configuration,
            ILogger<WebhookValidationService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool ValidateSignature(string payload, string signature)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                _logger.LogWarning("Webhook validation failed: Payload is null or empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogWarning("Webhook validation failed: Signature is null or empty");
                return false;
            }

            try
            {
                var computedSignature = ComputeSignature(payload);
                var isValid = signature.Equals(computedSignature, StringComparison.Ordinal);

                if (!isValid)
                {
                    _logger.LogWarning(
                        "Webhook signature validation failed. Expected signature does not match provided signature");
                }
                else
                {
                    _logger.LogInformation("Webhook signature validated successfully");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

        /// <inheritdoc/>
        public string ComputeSignature(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Payload cannot be null or empty", nameof(payload));

            var secretKey = _configuration.CurrentSecretKey;
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            
            // Return Base64-encoded signature as per PayAza specification
            return Convert.ToBase64String(hashBytes);
        }
    }
}

