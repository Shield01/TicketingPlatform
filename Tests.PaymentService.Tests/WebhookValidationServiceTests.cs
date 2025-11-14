using Microsoft.Extensions.Logging;
using Moq;
using Modules.PaymentService.Configuration;
using Modules.PaymentService.Services;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for WebhookValidationService.
    /// </summary>
    public class WebhookValidationServiceTests
    {
        private readonly Mock<ILogger<WebhookValidationService>> _mockLogger;
        private readonly PayAzaConfiguration _configuration;
        private readonly WebhookValidationService _service;

        public WebhookValidationServiceTests()
        {
            _mockLogger = new Mock<ILogger<WebhookValidationService>>();
            _configuration = new PayAzaConfiguration
            {
                SecretKeyTest = "test-secret-key-12345",
                SecretKeyLive = "live-secret-key-67890",
                Mode = "test"
            };
            _service = new WebhookValidationService(_configuration, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => 
                new WebhookValidationService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => 
                new WebhookValidationService(_configuration, null!));
        }

        [Fact]
        public void ComputeSignature_WithValidPayload_ReturnsBase64EncodedSignature()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";

            // Act
            var signature = _service.ComputeSignature(payload);

            // Assert
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            
            // Verify it's valid Base64
            var bytes = Convert.FromBase64String(signature);
            Assert.NotEmpty(bytes);
            
            // HMAC SHA512 produces 64 bytes = 88 Base64 characters (with padding)
            Assert.True(signature.Length >= 86);
        }

        [Fact]
        public void ComputeSignature_WithNullPayload_ThrowsArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => _service.ComputeSignature(null!));
        }

        [Fact]
        public void ComputeSignature_WithEmptyPayload_ThrowsArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => _service.ComputeSignature(string.Empty));
        }

        [Fact]
        public void ComputeSignature_WithSamePayload_ProducesSameSignature()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";

            // Act
            var signature1 = _service.ComputeSignature(payload);
            var signature2 = _service.ComputeSignature(payload);

            // Assert
            Assert.Equal(signature1, signature2);
        }

        [Fact]
        public void ComputeSignature_WithDifferentPayloads_ProducesDifferentSignatures()
        {
            // Arrange
            var payload1 = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";
            var payload2 = "{\"transaction_reference\":\"TXN-456\",\"status\":\"failed\"}";

            // Act
            var signature1 = _service.ComputeSignature(payload1);
            var signature2 = _service.ComputeSignature(payload2);

            // Assert
            Assert.NotEqual(signature1, signature2);
        }

        [Fact]
        public void ValidateSignature_WithValidSignature_ReturnsTrue()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";
            var validSignature = _service.ComputeSignature(payload);

            // Act
            var result = _service.ValidateSignature(payload, validSignature);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateSignature_WithInvalidSignature_ReturnsFalse()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";
            var invalidSignature = "invalid-signature-base64-encoded";

            // Act
            var result = _service.ValidateSignature(payload, invalidSignature);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_WithModifiedPayload_ReturnsFalse()
        {
            // Arrange
            var originalPayload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";
            var modifiedPayload = "{\"transaction_reference\":\"TXN-456\",\"status\":\"success\"}";
            var signature = _service.ComputeSignature(originalPayload);

            // Act
            var result = _service.ValidateSignature(modifiedPayload, signature);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_WithNullPayload_ReturnsFalse()
        {
            // Arrange
            var signature = "some-signature";

            // Act
            var result = _service.ValidateSignature(null!, signature);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_WithEmptyPayload_ReturnsFalse()
        {
            // Arrange
            var signature = "some-signature";

            // Act
            var result = _service.ValidateSignature(string.Empty, signature);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_WithNullSignature_ReturnsFalse()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\"}";

            // Act
            var result = _service.ValidateSignature(payload, null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_WithEmptySignature_ReturnsFalse()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\"}";

            // Act
            var result = _service.ValidateSignature(payload, string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_IsCaseSensitive()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";
            var validSignature = _service.ComputeSignature(payload);
            var lowercaseSignature = validSignature.ToLower();

            // Act & Assert - Should not match if signature has different case
            // Note: Base64 is case-sensitive
            if (validSignature != lowercaseSignature)
            {
                var result = _service.ValidateSignature(payload, lowercaseSignature);
                Assert.False(result);
            }
        }

        [Fact]
        public void ValidateSignature_WithDifferentSecretKey_ReturnsFalse()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";
            
            // Create service with different secret key
            var differentConfig = new PayAzaConfiguration
            {
                SecretKeyTest = "different-secret-key",
                Mode = "test"
            };
            var differentService = new WebhookValidationService(differentConfig, _mockLogger.Object);
            
            // Compute signature with original service
            var signature = _service.ComputeSignature(payload);

            // Act - Validate with service using different key
            var result = differentService.ValidateSignature(payload, signature);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_WithWhitespaceInPayload_AffectsSignature()
        {
            // Arrange
            var payload1 = "{\"transaction_reference\":\"TXN-123\",\"status\":\"success\"}";
            var payload2 = "{ \"transaction_reference\": \"TXN-123\", \"status\": \"success\" }";
            var signature = _service.ComputeSignature(payload1);

            // Act
            var result = _service.ValidateSignature(payload2, signature);

            // Assert - Whitespace changes the signature
            Assert.False(result);
        }

        [Fact]
        public void ValidateSignature_WithComplexPayload_WorksCorrectly()
        {
            // Arrange
            var payload = @"{
                ""event"": ""collection.success"",
                ""transaction_reference"": ""EVT-123E4567-20240115-ABCD1234"",
                ""transaction_id"": ""PAYAZA_TXN_987654321"",
                ""status"": ""success"",
                ""amount"": 10000.00,
                ""currency"": ""NGN"",
                ""payment_method"": ""card"",
                ""fee"": 150.00,
                ""created_at"": ""2024-01-15T12:00:00Z"",
                ""completed_at"": ""2024-01-15T12:05:00Z"",
                ""customer_email"": ""customer@example.com"",
                ""metadata"": {
                    ""card_type"": ""visa"",
                    ""last4"": ""1234""
                }
            }";

            // Act
            var signature = _service.ComputeSignature(payload);
            var result = _service.ValidateSignature(payload, signature);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("live")]
        public void ValidateSignature_WorksInBothModes(string mode)
        {
            // Arrange
            var config = new PayAzaConfiguration
            {
                SecretKeyTest = "test-secret-key",
                SecretKeyLive = "live-secret-key",
                Mode = mode
            };
            var service = new WebhookValidationService(config, _mockLogger.Object);
            var payload = "{\"test\":\"data\"}";

            // Act
            var signature = service.ComputeSignature(payload);
            var result = service.ValidateSignature(payload, signature);

            // Assert
            Assert.True(result);
        }
    }
}

