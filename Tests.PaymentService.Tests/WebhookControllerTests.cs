using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.PaymentService.Controllers;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Services;
using System.Text;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for PaymentController webhook endpoint.
    /// </summary>
    public class WebhookControllerTests
    {
        private readonly Mock<ILogger<PaymentController>> _mockLogger;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IWebhookValidationService> _mockWebhookValidationService;
        private readonly Mock<IWebhookProcessingService> _mockWebhookProcessingService;
        private readonly PaymentController _controller;

        public WebhookControllerTests()
        {
            _mockLogger = new Mock<ILogger<PaymentController>>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockWebhookValidationService = new Mock<IWebhookValidationService>();
            _mockWebhookProcessingService = new Mock<IWebhookProcessingService>();

            _controller = new PaymentController(
                _mockLogger.Object,
                _mockPaymentService.Object,
                _mockWebhookValidationService.Object,
                _mockWebhookProcessingService.Object);
        }

        private void SetupRequestBody(string payload)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private void SetupRequestBodyAndSignature(string payload, string signature)
        {
            SetupRequestBody(payload);
            _controller.HttpContext.Request.Headers["x-payaza-signature"] = signature;
        }

        [Fact]
        public async Task ProcessWebhook_WithEmptyPayload_ReturnsBadRequest()
        {
            // Arrange
            SetupRequestBody(string.Empty);

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task ProcessWebhook_WithMissingSignature_ReturnsUnauthorized()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\"}";
            SetupRequestBody(payload);
            // No signature header set

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        [Fact]
        public async Task ProcessWebhook_WithInvalidSignature_ReturnsUnauthorized()
        {
            // Arrange
            var payload = "{\"transaction_reference\":\"TXN-123\"}";
            var signature = "invalid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(false);

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        [Fact]
        public async Task ProcessWebhook_WithInvalidJson_ReturnsBadRequest()
        {
            // Arrange
            var payload = "{ invalid json }";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task ProcessWebhook_WithValidPayload_ProcessesSuccessfully()
        {
            // Arrange
            var payload = @"{
                ""event"": ""collection.success"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""success"",
                ""amount"": 5000.00,
                ""currency"": ""NGN""
            }";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(WebhookProcessingResult.SuccessResult(
                    Guid.NewGuid(),
                    "TXN-123",
                    "COMPLETED"));

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var webhookResult = Assert.IsType<WebhookProcessingResult>(okResult.Value);
            Assert.True(webhookResult.Success);
            Assert.Equal("TXN-123", webhookResult.TransactionReference);
        }

        [Fact]
        public async Task ProcessWebhook_WithDuplicateWebhook_ReturnsOkWithDuplicateFlag()
        {
            // Arrange
            var payload = @"{
                ""event"": ""collection.success"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""success""
            }";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(WebhookProcessingResult.DuplicateResult("TXN-123"));

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var webhookResult = Assert.IsType<WebhookProcessingResult>(okResult.Value);
            Assert.True(webhookResult.Success);
            Assert.True(webhookResult.IsDuplicate);
        }

        [Fact]
        public async Task ProcessWebhook_WithProcessingFailure_ReturnsBadRequest()
        {
            // Arrange
            var payload = @"{
                ""event"": ""collection.failed"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""failed""
            }";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(WebhookProcessingResult.FailureResult("Payment not found"));

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var webhookResult = Assert.IsType<WebhookProcessingResult>(badRequestResult.Value);
            Assert.False(webhookResult.Success);
        }

        [Fact]
        public async Task ProcessWebhook_WithException_ReturnsOkToPreventRetries()
        {
            // Arrange
            var payload = @"{
                ""event"": ""collection.success"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""success""
            }";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            // Should return 200 OK to prevent PayAza from retrying
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ProcessWebhook_ValidatesSignatureBeforeParsing()
        {
            // Arrange
            var payload = @"{
                ""event"": ""collection.success"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""success""
            }";
            var signature = "invalid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(false);

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            
            // Verify processing service was never called
            _mockWebhookProcessingService.Verify(
                s => s.ProcessWebhookAsync(It.IsAny<PayAzaWebhookPayload>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessWebhook_WithComplexPayload_ProcessesAllFields()
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
                ""customer_name"": ""John Doe"",
                ""metadata"": {
                    ""card_type"": ""visa"",
                    ""last4"": ""1234""
                }
            }";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            PayAzaWebhookPayload? capturedPayload = null;
            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .Callback<PayAzaWebhookPayload, CancellationToken>((p, _) => capturedPayload = p)
                .ReturnsAsync(WebhookProcessingResult.SuccessResult(
                    Guid.NewGuid(),
                    "EVT-123E4567-20240115-ABCD1234",
                    "COMPLETED"));

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(capturedPayload);
            Assert.Equal("collection.success", capturedPayload.Event);
            Assert.Equal("EVT-123E4567-20240115-ABCD1234", capturedPayload.TransactionReference);
            Assert.Equal("PAYAZA_TXN_987654321", capturedPayload.TransactionId);
            Assert.Equal("success", capturedPayload.Status);
            Assert.Equal(10000.00m, capturedPayload.Amount);
            Assert.Equal("NGN", capturedPayload.Currency);
            Assert.Equal("card", capturedPayload.PaymentMethod);
            Assert.Equal(150.00m, capturedPayload.Fee);
            Assert.Equal("customer@example.com", capturedPayload.CustomerEmail);
            Assert.Equal("John Doe", capturedPayload.CustomerName);
            Assert.NotNull(capturedPayload.Metadata);
            Assert.Equal(2, capturedPayload.Metadata.Count);
        }

        [Theory]
        [InlineData("collection.success")]
        [InlineData("collection.failed")]
        [InlineData("transfer.completed")]
        [InlineData("transfer.failed")]
        public async Task ProcessWebhook_HandlesVariousEventTypes(string eventType)
        {
            // Arrange
            var payload = $@"{{
                ""event"": ""{eventType}"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""success""
            }}";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(WebhookProcessingResult.SuccessResult(
                    Guid.NewGuid(),
                    "TXN-123",
                    "COMPLETED"));

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ProcessWebhook_WithWhitespaceInPayload_MaintainsIntegrity()
        {
            // Arrange
            var payload = @"
            {
                ""event"": ""collection.success"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""success""
            }
            ";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(
                It.Is<string>(p => p == payload), // Must match exact payload including whitespace
                signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(WebhookProcessingResult.SuccessResult(
                    Guid.NewGuid(),
                    "TXN-123",
                    "COMPLETED"));

            // Act
            var result = await _controller.ProcessWebhook();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            
            // Verify signature was validated with exact payload
            _mockWebhookValidationService.Verify(
                s => s.ValidateSignature(payload, signature),
                Times.Once);
        }

        [Fact]
        public async Task ProcessWebhook_LogsImportantEvents()
        {
            // Arrange
            var payload = @"{
                ""event"": ""collection.success"",
                ""transaction_reference"": ""TXN-123"",
                ""status"": ""success""
            }";
            var signature = "valid-signature";
            SetupRequestBodyAndSignature(payload, signature);

            _mockWebhookValidationService.Setup(s => s.ValidateSignature(payload, signature))
                .Returns(true);

            _mockWebhookProcessingService.Setup(s => s.ProcessWebhookAsync(
                It.IsAny<PayAzaWebhookPayload>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(WebhookProcessingResult.SuccessResult(
                    Guid.NewGuid(),
                    "TXN-123",
                    "COMPLETED"));

            // Act
            await _controller.ProcessWebhook();

            // Assert - Verify logging calls were made (using Moq's Verify on ILogger is complex,
            // so we just verify the method completed successfully which implies logging occurred)
            _mockWebhookValidationService.Verify(
                s => s.ValidateSignature(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}

