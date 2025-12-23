using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Modules.PaymentService.Configuration;
using Modules.PaymentService.Infrastructure;
using Modules.PaymentService.Infrastructure.DTOs;
using Modules.PaymentService.Infrastructure.Exceptions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for PayAzaClient.
    /// </summary>
    public class PayAzaClientTests
    {
        private readonly Mock<ILogger<PayAzaClient>> _loggerMock;
        private readonly PayAzaConfiguration _testConfig;

        public PayAzaClientTests()
        {
            _loggerMock = new Mock<ILogger<PayAzaClient>>();
            _testConfig = new PayAzaConfiguration
            {
                ApiKeyTest = "test-api-key",
                ApiKeyLive = "live-api-key",
                SecretKeyTest = "test-secret-key",
                SecretKeyLive = "live-secret-key",
                Mode = "test",
                MerchantKey = "merchant-123",
                BaseUrlTest = "https://api.payaza.africa/live",
                BaseUrlLive = "https://api.payaza.africa/live",
                TimeoutSeconds = 30,
                MaxRetryAttempts = 3,
                InitialBackoffDelayMs = 100
            };
        }

        [Fact]
        public async Task GetAccountDetailsAsync_Success_ReturnsAccountDetails()
        {
            // Arrange
            var response = new
            {
                response_code = 200,
                response_message = "Account details retrieved successfully",
                response_content = new
                {
                    account_number = "1234567890",
                    account_name = "John Doe",
                    bank_name = "Test Bank",
                    bank_code = "123",
                    currency = "NGN"
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, response);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act
            var result = await client.GetAccountDetailsAsync("1234567890", "123");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("John Doe", result.Data?.AccountName);
        }

        [Fact]
        public async Task GetAccountDetailsAsync_InvalidAccountNumber_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaAccountDetailsResponse());
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.GetAccountDetailsAsync("", "123"));
        }

        [Fact]
        public async Task InitiatePayoutAsync_Success_ReturnsPayoutResponse()
        {
            // Arrange
            var request = new PayAzaPayoutRequest
            {
                TransactionReference = "TXN-20240115-ABC123",
                Amount = 5000.00m,
                Currency = "NGN",
                AccountNumber = "1234567890",
                BankCode = "123",
                AccountName = "John Doe",
                Narration = "Payout for event tickets"
            };

            var response = new
            {
                response_code = 200,
                response_message = "Payout initiated successfully",
                response_content = new
                {
                    transaction_reference = request.TransactionReference,
                    transaction_status = "pending",
                    amount = request.Amount,
                    response_status = "success"
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, response);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act
            var result = await client.InitiatePayoutAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("pending", result.Data?.Status);
            Assert.Equal(5000.00m, result.Data?.Amount);
        }

        [Fact]
        public async Task InitiatePayoutAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaPayoutResponse());
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                client.InitiatePayoutAsync(null!));
        }

        [Fact]
        public async Task GetTransactionStatusAsync_Success_ReturnsTransactionStatus()
        {
            // Arrange
            var response = new PayAzaTransactionStatusResponse
            {
                Success = true,
                Message = "Transaction status retrieved",
                Data = new PayAzaTransactionData
                {
                    TransactionReference = "TXN-20240115-ABC123",
                    Status = "successful",
                    Amount = 5000.00m,
                    Currency = "NGN",
                    Fee = 50.00m,
                    Type = "payout",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow.AddMinutes(5)
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, response);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act
            var result = await client.GetTransactionStatusAsync("TXN-20240115-ABC123");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("successful", result.Data?.Status);
        }

        [Fact]
        public async Task GetTransactionStatusAsync_EmptyReference_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaTransactionStatusResponse());
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.GetTransactionStatusAsync(""));
        }

        [Fact]
        public async Task PayAzaClient_UnauthorizedResponse_ThrowsAuthenticationException()
        {
            // Arrange
            var errorResponse = new
            {
                success = false,
                message = "Unauthorized",
                error = new
                {
                    code = "AUTH_ERROR",
                    message = "Invalid API key"
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized, errorResponse);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<PayAzaAuthenticationException>(() =>
                client.GetAccountDetailsAsync("1234567890", "123"));
        }

        [Fact]
        public async Task PayAzaClient_BadRequest_ThrowsValidationException()
        {
            // Arrange
            var errorResponse = new
            {
                success = false,
                message = "Validation failed",
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "Invalid account number",
                    details = new Dictionary<string, string[]>
                    {
                        ["account_number"] = new[] { "Account number must be 10 digits" }
                    }
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorResponse);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<PayAzaValidationException>(() =>
                client.GetAccountDetailsAsync("12345", "123"));
        }

        [Fact]
        public async Task PayAzaClient_NotFound_ThrowsNotFoundException()
        {
            // Arrange
            var errorResponse = new
            {
                success = false,
                message = "Transaction not found",
                error = new
                {
                    code = "NOT_FOUND",
                    message = "Transaction does not exist"
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound, errorResponse);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<PayAzaNotFoundException>(() =>
                client.GetTransactionStatusAsync("INVALID-REF"));
        }

        [Fact]
        public async Task PayAzaClient_RateLimitExceeded_ThrowsRateLimitException()
        {
            // Arrange
            var errorResponse = new
            {
                success = false,
                message = "Rate limit exceeded",
                error = new
                {
                    code = "RATE_LIMIT_EXCEEDED",
                    message = "Too many requests"
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.TooManyRequests, errorResponse);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<PayAzaRateLimitException>(() =>
                client.GetAccountDetailsAsync("1234567890", "123"));
        }

        [Fact]
        public async Task PayAzaClient_ServerError_RetriesAndThrowsServerException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient<object?>(HttpStatusCode.InternalServerError, null);
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<PayAzaServerException>(() =>
                client.GetAccountDetailsAsync("1234567890", "123"));
        }

        [Fact]
        public void ValidateWebhookSignature_ValidSignature_ReturnsTrue()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaAccountDetailsResponse());
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"successful\"}";
            var secretKey = _testConfig.CurrentSecretKey;
            
            // Compute expected signature
            using var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToHexString(hashBytes).ToLower();

            // Act
            var isValid = client.ValidateWebhookSignature(payload, expectedSignature);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateWebhookSignature_InvalidSignature_ReturnsFalse()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaAccountDetailsResponse());
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"successful\"}";
            var invalidSignature = "invalid-signature";

            // Act
            var isValid = client.ValidateWebhookSignature(payload, invalidSignature);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void GetCurrentMode_TestMode_ReturnsTest()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaAccountDetailsResponse());
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act
            var mode = client.GetCurrentMode();

            // Assert
            Assert.Equal("test", mode);
        }

        [Fact]
        public void IsConfigured_ValidConfiguration_ReturnsTrue()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaAccountDetailsResponse());
            var client = new PayAzaClient(httpClient, _testConfig, _loggerMock.Object);

            // Act
            var isConfigured = client.IsConfigured();

            // Assert
            Assert.True(isConfigured);
        }

        [Fact]
        public void IsConfigured_InvalidConfiguration_ReturnsFalse()
        {
            // Arrange
            var invalidConfig = new PayAzaConfiguration
            {
                Mode = "test",
                MerchantKey = ""  // Invalid - missing merchant key
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new PayAzaAccountDetailsResponse());
            var client = new PayAzaClient(httpClient, invalidConfig, _loggerMock.Object);

            // Act
            var isConfigured = client.IsConfigured();

            // Assert
            Assert.False(isConfigured);
        }

        [Fact]
        public void PayAzaConfiguration_GetAuthorizationHeaderValue_ReturnsBase64EncodedKey()
        {
            // Arrange
            var config = new PayAzaConfiguration
            {
                ApiKeyTest = "test-api-key",
                Mode = "test"
            };

            // Act
            var authHeader = config.GetAuthorizationHeaderValue();

            // Assert
            Assert.StartsWith("Payaza ", authHeader);
            var base64Part = authHeader.Replace("Payaza ", "");
            var decodedKey = Encoding.UTF8.GetString(Convert.FromBase64String(base64Part));
            Assert.Equal("test-api-key", decodedKey);
        }

        [Fact]
        public void PayAzaConfiguration_LiveMode_UsesLiveCredentials()
        {
            // Arrange
            var config = new PayAzaConfiguration
            {
                ApiKeyTest = "test-api-key",
                ApiKeyLive = "live-api-key",
                BaseUrlTest = "https://test.com",
                BaseUrlLive = "https://live.com",
                Mode = "live"
            };

            // Act & Assert
            Assert.Equal("live-api-key", config.CurrentApiKey);
            Assert.Equal("https://live.com", config.CurrentBaseUrl);
            Assert.True(config.IsLiveMode);
            Assert.False(config.IsTestMode);
        }

        /// <summary>
        /// Creates a mock HttpClient with predefined response.
        /// </summary>
        private HttpClient CreateMockHttpClient<T>(HttpStatusCode statusCode, T responseContent)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseContent != null 
                    ? new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
                    : new StringContent("", Encoding.UTF8, "application/json")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.payaza.africa/live")
            };
        }
    }
}

