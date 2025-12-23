using Microsoft.Extensions.Logging;
using Modules.PaymentService.Configuration;
using Modules.PaymentService.Infrastructure.DTOs;
using Modules.PaymentService.Infrastructure.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

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

            // HttpClient configured per request to ensure headers are fresh and correct
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

            // Correct Endpoint: POST /payaza-account/api/v1/mainaccounts/merchant/provider/enquiry
            var endpoint = "/payaza-account/api/v1/mainaccounts/merchant/provider/enquiry";

            var payload = new
            {
                service_payload = new
                {
                    currency = "NGN", // Defaulting to NGN as per MVP scope
                    bank_code = bankCode,
                    account_number = accountNumber
                }
            };

            var internalResponse = await ExecuteWithRetryAsync<PayAzaEnquiryResponseInternal>(
                async () => 
                {
                    var request = CreateRequestMessage(HttpMethod.Post, endpoint);
                    request.Content = new StringContent(
                        JsonSerializer.Serialize(payload, _jsonOptions),
                        Encoding.UTF8,
                        "application/json");
                    return await _httpClient.SendAsync(request, cancellationToken);
                },
                "GetAccountDetails",
                cancellationToken);

            // Map to public DTO
            return new PayAzaAccountDetailsResponse
            {
                Success = internalResponse.ResponseCode == 200,
                Message = internalResponse.ResponseMessage,
                Data = internalResponse.ResponseContent != null ? new PayAzaAccountData
                {
                    AccountName = internalResponse.ResponseContent.AccountName ?? string.Empty,
                    AccountNumber = internalResponse.ResponseContent.AccountNumber ?? accountNumber,
                    BankCode = internalResponse.ResponseContent.BankCode ?? bankCode,
                    Currency = "NGN",
                    BankName = internalResponse.ResponseContent.BankName ?? string.Empty
                } : null,
                Error = internalResponse.ResponseCode != 200 ? new PayAzaErrorDetails { Message = internalResponse.ResponseMessage } : null
            };
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

            // Set merchant key if not provided (though new payload uses TransactionPin)
            if (string.IsNullOrWhiteSpace(request.MerchantKey))
                request.MerchantKey = _configuration.MerchantKey;

            _logger.LogInformation("Initiating payout for reference {TransactionReference}, amount {Amount} {Currency}", 
                request.TransactionReference, request.Amount, request.Currency);

            // Correct Endpoint: POST /payout-receptor/payout
            var endpoint = "/payout-receptor/payout";

            // Construct new nested payload
            var payload = new
            {
                transaction_type = "nuban", // Default for NGN
                service_payload = new
                {
                    payout_amount = request.Amount,
                    transaction_pin = int.TryParse(_configuration.TransactionPin, out var pin) ? pin : 0, // Should be parsed from config
                    account_reference = _configuration.AccountReference,
                    currency = request.Currency,
                    country = "NGA", // Default for NGN
                    payout_beneficiaries = new[]
                    {
                        new
                        {
                            credit_amount = request.Amount,
                            account_number = request.AccountNumber,
                            account_name = request.AccountName ?? "Beneficiary",
                            bank_code = request.BankCode,
                            narration = request.Narration,
                            transaction_reference = request.TransactionReference,
                            sender = new
                            {
                                sender_name = "Ticketing Platform", // Generic sender name
                                sender_id = "",
                                sender_phone_number = "",
                                sender_address = ""
                            }
                        }
                    }
                }
            };

            var internalResponse = await ExecuteWithRetryAsync<PayAzaPayoutResponseInternal>(
                async () =>
                {
                    var request = CreateRequestMessage(HttpMethod.Post, endpoint);
                    request.Content = new StringContent(
                        JsonSerializer.Serialize(payload, _jsonOptions),
                        Encoding.UTF8,
                        "application/json");
                    return await _httpClient.SendAsync(request, cancellationToken);
                },
                "InitiatePayout",
                cancellationToken);

            // Map to public DTO
            return new PayAzaPayoutResponse
            {
                Success = internalResponse.ResponseCode == 200,
                Message = internalResponse.ResponseMessage,
                Data = internalResponse.ResponseContent != null ? new PayAzaPayoutData
                {
                    TransactionReference = internalResponse.ResponseContent.TransactionReference ?? request.TransactionReference,
                    Status = internalResponse.ResponseContent.TransactionStatus ?? "Initiated",
                    Amount = internalResponse.ResponseContent.Amount,
                    Fee = 0, // Fee not explicitly returned in the simple internal response structure sometimes
                    CreatedAt = DateTime.UtcNow // Placeholder
                } : null,
                Error = internalResponse.ResponseCode != 200 ? new PayAzaErrorDetails { Message = internalResponse.ResponseMessage } : null
            };
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
                async () =>
                {
                   var request = CreateRequestMessage(HttpMethod.Get, endpoint);
                   return await _httpClient.SendAsync(request, cancellationToken);
                },
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
                var computedSignature = ComputeHmacSha512(payload, secretKey);
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
        /// Creates an HTTP request message with required headers.
        /// </summary>
        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string requestUri)
        {
            // Construct full URI manually to ensure Base URL path segments (e.g. '/live') 
            // are preserved and not overridden by leading slashes in relative paths.
            var baseUrl = _configuration.CurrentBaseUrl.TrimEnd('/');
            var relativePath = requestUri.TrimStart('/');
            var fullUri = new Uri($"{baseUrl}/{relativePath}");

            var request = new HttpRequestMessage(method, fullUri);

            // Add Authorization header
            // Format: "Payaza <Base64EncodedKey>"
            var apiKey = _configuration.CurrentApiKey;
            var encodedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey));
            request.Headers.Authorization = new AuthenticationHeaderValue("Payaza", encodedKey);

            // Add X-TenantID header
            request.Headers.Add("X-TenantID", _configuration.TenantId);

            // Add standard headers
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Add User-Agent if not present (though HttpClient might have default)
            if (!request.Headers.UserAgent.Any())
            {
                request.Headers.UserAgent.ParseAdd("TicketingPlatform-PayAzaClient/1.0");
            }

            return request;
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
        /// Computes HMAC-SHA512 hash for webhook signature validation.
        /// </summary>
        private static string ComputeHmacSha512(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        // Internal Response Classes for PayAza API deserialization

        private class PayAzaEnquiryResponseInternal
        {
            [JsonPropertyName("response_code")]
            public int ResponseCode { get; set; }

            [JsonPropertyName("response_message")]
            public string ResponseMessage { get; set; } = string.Empty;

            [JsonPropertyName("response_content")]
            public PayAzaEnquiryContent? ResponseContent { get; set; }
        }

        private class PayAzaEnquiryContent
        {
            [JsonPropertyName("account_name")]
            public string? AccountName { get; set; }

            [JsonPropertyName("account_number")]
            public string? AccountNumber { get; set; }

            [JsonPropertyName("bank_code")]
            public string? BankCode { get; set; }

            [JsonPropertyName("bank_name")]
            public string? BankName { get; set; }
            
            [JsonPropertyName("transaction_status")]
            public string? TransactionStatus { get; set; }
        }

        private class PayAzaPayoutResponseInternal
        {
            [JsonPropertyName("response_code")]
            public int ResponseCode { get; set; }

            [JsonPropertyName("response_message")]
            public string ResponseMessage { get; set; } = string.Empty;

            [JsonPropertyName("response_content")]
            public PayAzaPayoutContent? ResponseContent { get; set; }
        }

        private class PayAzaPayoutContent
        {
            [JsonPropertyName("transaction_status")]
            public string? TransactionStatus { get; set; }

            [JsonPropertyName("transaction_reference")]
            public string? TransactionReference { get; set; }

            [JsonPropertyName("amount")]
            public decimal Amount { get; set; }

            [JsonPropertyName("response_status")]
            public string? ResponseStatus { get; set; }
        }
    }
}

