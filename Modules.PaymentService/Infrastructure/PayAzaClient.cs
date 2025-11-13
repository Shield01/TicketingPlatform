using Microsoft.Extensions.Logging;
using Modules.PaymentService.Configuration;
using Modules.PaymentService.Infrastructure.DTOs;
using Modules.PaymentService.Infrastructure.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Modules.PaymentService.Infrastructure
{
    /// <summary>
    /// Client for interacting with the PayAza payment gateway API.
    /// </summary>
    public class PayAzaClient : IPayAzaClient
    {
        private readonly HttpClient _httpClient;
        private readonly PayAzaConfiguration _configuration;
        private readonly ILogger<PayAzaClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="configuration">The PayAza configuration.</param>
        /// <param name="logger">The logger.</param>
        public PayAzaClient(
            HttpClient httpClient, 
            PayAzaConfiguration configuration, 
            ILogger<PayAzaClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            // Configure HttpClient
            ConfigureHttpClient();

            _logger.LogInformation("PayAzaClient initialized in {Mode} mode", _configuration.Mode);
        }

        /// <summary>
        /// Gets account details from PayAza.
        /// </summary>
        public async Task<PayAzaAccountDetailsResponse> GetAccountDetailsAsync(
            string accountNumber, 
            string bankCode, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number cannot be null or empty.", nameof(accountNumber));

            if (string.IsNullOrWhiteSpace(bankCode))
                throw new ArgumentException("Bank code cannot be null or empty.", nameof(bankCode));

            _logger.LogInformation("Getting account details for account {AccountNumber} at bank {BankCode}", 
                accountNumber, bankCode);

            var endpoint = $"/api/account/verify?account_number={accountNumber}&bank_code={bankCode}";

            return await ExecuteWithRetryAsync<PayAzaAccountDetailsResponse>(
                () => _httpClient.GetAsync(endpoint, cancellationToken),
                "GetAccountDetails",
                cancellationToken);
        }

        /// <summary>
        /// Initiates a payout transaction through PayAza.
        /// </summary>
        public async Task<PayAzaPayoutResponse> InitiatePayoutAsync(
            PayAzaPayoutRequest request, 
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Set merchant key if not provided
            if (string.IsNullOrWhiteSpace(request.MerchantKey))
                request.MerchantKey = _configuration.MerchantKey;

            _logger.LogInformation("Initiating payout for reference {TransactionReference}, amount {Amount} {Currency}", 
                request.TransactionReference, request.Amount, request.Currency);

            var endpoint = "/api/payout/initiate";
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            return await ExecuteWithRetryAsync<PayAzaPayoutResponse>(
                () => _httpClient.PostAsync(endpoint, content, cancellationToken),
                "InitiatePayout",
                cancellationToken);
        }

        /// <summary>
        /// Gets the status of a transaction from PayAza.
        /// </summary>
        public async Task<PayAzaTransactionStatusResponse> GetTransactionStatusAsync(
            string transactionReference, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transactionReference))
                throw new ArgumentException("Transaction reference cannot be null or empty.", nameof(transactionReference));

            _logger.LogInformation("Getting transaction status for reference {TransactionReference}", transactionReference);

            var endpoint = $"/api/transaction/status/{transactionReference}";

            return await ExecuteWithRetryAsync<PayAzaTransactionStatusResponse>(
                () => _httpClient.GetAsync(endpoint, cancellationToken),
                "GetTransactionStatus",
                cancellationToken);
        }

        /// <summary>
        /// Validates PayAza webhook signature.
        /// </summary>
        public bool ValidateWebhookSignature(string payload, string signature)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Payload cannot be null or empty.", nameof(payload));

            if (string.IsNullOrWhiteSpace(signature))
                throw new ArgumentException("Signature cannot be null or empty.", nameof(signature));

            try
            {
                var secretKey = _configuration.CurrentSecretKey;
                var computedSignature = ComputeHmacSha256(payload, secretKey);
                var isValid = signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);

                _logger.LogInformation("Webhook signature validation result: {IsValid}", isValid);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

        /// <summary>
        /// Gets the current configuration mode.
        /// </summary>
        public string GetCurrentMode()
        {
            return _configuration.Mode;
        }

        /// <summary>
        /// Checks if the client is configured and ready to use.
        /// </summary>
        public bool IsConfigured()
        {
            return _configuration.IsValid();
        }

        /// <summary>
        /// Configures the HttpClient with base address and default headers.
        /// </summary>
        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_configuration.CurrentBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);

            // Clear existing headers
            _httpClient.DefaultRequestHeaders.Clear();

            // Add Authorization header with Base64-encoded API key
            var authHeaderValue = _configuration.GetAuthorizationHeaderValue();
            _httpClient.DefaultRequestHeaders.Add("Authorization", authHeaderValue);

            // Add X-TenantID header
            _httpClient.DefaultRequestHeaders.Add("X-TenantID", _configuration.TenantId);

            // Add standard headers
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TicketingPlatform-PayAzaClient/1.0");

            _logger.LogDebug("HttpClient configured with base URL: {BaseUrl}, TenantID: {TenantId}", 
                _configuration.CurrentBaseUrl, _configuration.TenantId);
        }

        /// <summary>
        /// Executes an HTTP request with exponential backoff retry logic.
        /// </summary>
        private async Task<TResponse> ExecuteWithRetryAsync<TResponse>(
            Func<Task<HttpResponseMessage>> httpRequestFunc,
            string operationName,
            CancellationToken cancellationToken) where TResponse : class
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < _configuration.MaxRetryAttempts)
            {
                attempt++;

                try
                {
                    _logger.LogDebug("Executing {OperationName}, attempt {Attempt}/{MaxAttempts}", 
                        operationName, attempt, _configuration.MaxRetryAttempts);

                    var response = await httpRequestFunc();
                    
                    // If successful, parse and return response
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        var result = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);

                        _logger.LogInformation("{OperationName} completed successfully", operationName);

                        return result ?? throw new PayAzaException($"Failed to deserialize response for {operationName}");
                    }

                    // Handle different HTTP status codes
                    await HandleErrorResponseAsync(response, operationName, cancellationToken);

                    // If we reach here, it means we should retry (5xx errors)
                    if (response.StatusCode >= HttpStatusCode.InternalServerError && attempt < _configuration.MaxRetryAttempts)
                    {
                        var delay = CalculateBackoffDelay(attempt);
                        _logger.LogWarning("Server error ({StatusCode}) on {OperationName}, retrying in {Delay}ms", 
                            (int)response.StatusCode, operationName, delay);
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }

                    // Non-retryable error
                    throw new PayAzaServerException(
                        $"{operationName} failed with status {response.StatusCode}", 
                        (int)response.StatusCode);
                }
                catch (PayAzaException)
                {
                    // Re-throw PayAza-specific exceptions
                    throw;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("{OperationName} was cancelled", operationName);
                    throw;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "HTTP request exception on {OperationName}, attempt {Attempt}/{MaxAttempts}", 
                        operationName, attempt, _configuration.MaxRetryAttempts);

                    if (attempt < _configuration.MaxRetryAttempts)
                    {
                        var delay = CalculateBackoffDelay(attempt);
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogError(ex, "Unexpected error on {OperationName}", operationName);
                    throw new PayAzaException($"{operationName} failed: {ex.Message}", ex);
                }
            }

            // All retries exhausted
            throw new PayAzaException(
                $"{operationName} failed after {_configuration.MaxRetryAttempts} attempts", 
                lastException ?? new Exception("Unknown error"));
        }

        /// <summary>
        /// Handles error responses from PayAza API.
        /// </summary>
        private async Task HandleErrorResponseAsync(
            HttpResponseMessage response, 
            string operationName, 
            CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogWarning("{OperationName} returned error status {StatusCode}: {Content}", 
                operationName, (int)response.StatusCode, content);

            // Try to parse error details
            PayAzaErrorDetails? errorDetails = null;
            try
            {
                var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                if (errorResponse != null && errorResponse.ContainsKey("error"))
                {
                    var errorJson = errorResponse["error"].ToString();
                    errorDetails = JsonSerializer.Deserialize<PayAzaErrorDetails>(errorJson!, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to parse error details from response");
            }

            var errorMessage = errorDetails?.Message ?? $"{operationName} failed with status {response.StatusCode}";
            var errorCode = errorDetails?.Code ?? response.StatusCode.ToString();

            // Throw specific exceptions based on status code
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    throw new PayAzaAuthenticationException(errorMessage);

                case HttpStatusCode.BadRequest:
                    throw new PayAzaValidationException(errorMessage, errorDetails?.Details ?? new Dictionary<string, string[]>());

                case HttpStatusCode.NotFound:
                    throw new PayAzaNotFoundException(errorMessage);

                case HttpStatusCode.TooManyRequests:
                    var resetTime = response.Headers.RetryAfter?.Date?.DateTime;
                    throw new PayAzaRateLimitException(errorMessage, resetTime);

                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                    // These will trigger retry logic
                    return;

                default:
                    throw new PayAzaException(errorMessage, errorCode, (int)response.StatusCode);
            }
        }

        /// <summary>
        /// Calculates the backoff delay for retry attempts using exponential backoff.
        /// </summary>
        private int CalculateBackoffDelay(int attempt)
        {
            // Exponential backoff: initial_delay * 2^(attempt-1)
            var delay = _configuration.InitialBackoffDelayMs * Math.Pow(2, attempt - 1);
            
            // Add jitter to prevent thundering herd
            var jitter = new Random().Next(0, (int)(delay * 0.1));
            
            return (int)delay + jitter;
        }

        /// <summary>
        /// Computes HMAC-SHA256 hash for webhook signature validation.
        /// </summary>
        private static string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}

