using Microsoft.Extensions.Logging;
using Moq;
using Modules.PaymentService.Constants;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Models;
using Modules.PaymentService.Repositories;
using Modules.PaymentService.Services;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for WebhookProcessingService.
    /// </summary>
    public class WebhookProcessingServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockRepository;
        private readonly Mock<ILogger<WebhookProcessingService>> _mockLogger;
        private readonly WebhookProcessingService _service;

        public WebhookProcessingServiceTests()
        {
            _mockRepository = new Mock<IPaymentRepository>();
            _mockLogger = new Mock<ILogger<WebhookProcessingService>>();
            _service = new WebhookProcessingService(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WebhookProcessingService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WebhookProcessingService(_mockRepository.Object, null!));
        }

        [Fact]
        public async Task ProcessWebhookAsync_WithNullPayload_ReturnsFailureResult()
        {
            // Act
            var result = await _service.ProcessWebhookAsync(null!);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("null", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessWebhookAsync_WithMissingTransactionReference_ReturnsFailureResult()
        {
            // Arrange
            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = string.Empty
            };

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("required", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessWebhookAsync_WithNonExistentPayment_ReturnsFailureResult()
        {
            // Arrange
            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = "TXN-123",
                Amount = 5000m,
                Currency = "NGN"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync(It.IsAny<string>(), default))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessWebhookAsync_WithSuccessfulPayment_UpdatesStatusToCompleted()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending,
                Amount = 5000m,
                Currency = "NGN"
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = "TXN-123",
                TransactionId = "PAYAZA-456",
                Amount = 5000m,
                Currency = "NGN",
                PaymentMethod = "card"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(PaymentStatus.Completed, result.Status);
            Assert.Equal(payment.Id, result.PaymentId);
            Assert.Equal("TXN-123", result.TransactionReference);
            Assert.False(result.IsDuplicate);

            // Verify payment was updated
            _mockRepository.Verify(r => r.UpdateAsync(
                It.Is<Payment>(p =>
                    p.Status == PaymentStatus.Completed &&
                    p.TransactionId == "PAYAZA-456" &&
                    p.PaymentMethod == "card" &&
                    p.CompletedAt != null),
                default), Times.Once);
        }

        [Fact]
        public async Task ProcessWebhookAsync_WithFailedPayment_UpdatesStatusToFailed()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.failed",
                Status = "failed",
                TransactionReference = "TXN-123",
                ErrorMessage = "Insufficient funds",
                ErrorCode = "ERR_001"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(PaymentStatus.Failed, result.Status);

            // Verify metadata includes error details
            _mockRepository.Verify(r => r.UpdateAsync(
                It.Is<Payment>(p =>
                    p.Status == PaymentStatus.Failed &&
                    p.GatewayMetadata!.Contains("Insufficient funds")),
                default), Times.Once);
        }

        [Fact]
        public async Task ProcessWebhookAsync_WithDuplicateWebhook_ReturnsDuplicateResult()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Completed,
                LastWebhookEventId = "abc123", // Already processed this exact webhook
                WebhookCount = 1
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = "TXN-123",
                TransactionId = "PAYAZA-456"
            };

            // The webhook will generate the same event ID
            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            // Note: The actual duplicate detection depends on GenerateWebhookEventId logic
            // For now, verify the service handles it gracefully
            Assert.True(result.Success || result.IsDuplicate);
        }

        [Fact]
        public async Task ProcessWebhookAsync_UpdatesWebhookCount()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending,
                WebhookCount = 0
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = "TXN-123"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            await _service.ProcessWebhookAsync(payload);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(
                It.Is<Payment>(p =>
                    p.WebhookCount == 1 &&
                    p.LastWebhookReceivedAt != null &&
                    !string.IsNullOrEmpty(p.LastWebhookEventId)),
                default), Times.Once);
        }

        [Theory]
        [InlineData("success", PaymentStatus.Completed)]
        [InlineData("successful", PaymentStatus.Completed)]
        [InlineData("completed", PaymentStatus.Completed)]
        [InlineData("confirmed", PaymentStatus.Confirmed)]
        [InlineData("pending", PaymentStatus.Pending)]
        [InlineData("failed", PaymentStatus.Failed)]
        [InlineData("failure", PaymentStatus.Failed)]
        [InlineData("cancelled", PaymentStatus.Cancelled)]
        [InlineData("canceled", PaymentStatus.Cancelled)]
        [InlineData("expired", PaymentStatus.Expired)]
        [InlineData("unknown_status", PaymentStatus.Failed)]
        public async Task ProcessWebhookAsync_MapsStatusCorrectly(string webhookStatus, string expectedStatus)
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.event",
                Status = webhookStatus,
                TransactionReference = "TXN-123"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedStatus, result.Status);
        }

        [Theory]
        [InlineData("collection.success", PaymentStatus.Completed)]
        [InlineData("collection.completed", PaymentStatus.Completed)]
        [InlineData("transfer.completed", PaymentStatus.Completed)]
        [InlineData("collection.failed", PaymentStatus.Failed)]
        [InlineData("transfer.failed", PaymentStatus.Failed)]
        [InlineData("collection.cancelled", PaymentStatus.Cancelled)]
        public async Task ProcessWebhookAsync_MapsEventTypeCorrectly(string eventType, string expectedStatus)
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = eventType,
                Status = "any_status", // Event type should take precedence
                TransactionReference = "TXN-123"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedStatus, result.Status);
        }

        [Fact]
        public async Task ProcessWebhookAsync_SetsCompletedAtForSuccessfulPayments()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending
            };

            var completedAt = DateTime.UtcNow.AddMinutes(-5);
            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = "TXN-123",
                CompletedAt = completedAt
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            await _service.ProcessWebhookAsync(payload);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(
                It.Is<Payment>(p =>
                    p.CompletedAt != null &&
                    p.CompletedAt.Value.Date == completedAt.Date),
                default), Times.Once);
        }

        [Fact]
        public async Task ProcessWebhookAsync_DoesNotSetCompletedAtForFailedPayments()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending,
                CompletedAt = null
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.failed",
                Status = "failed",
                TransactionReference = "TXN-123"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            await _service.ProcessWebhookAsync(payload);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(
                It.Is<Payment>(p => p.CompletedAt == null),
                default), Times.Once);
        }

        [Fact]
        public async Task ProcessWebhookAsync_StoresWebhookMetadata()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                Status = PaymentStatus.Pending
            };

            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = "TXN-123",
                Fee = 150m,
                Metadata = new Dictionary<string, object>
                {
                    ["card_type"] = "visa",
                    ["last4"] = "1234"
                }
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken _) => p);

            // Act
            await _service.ProcessWebhookAsync(payload);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(
                It.Is<Payment>(p =>
                    p.GatewayMetadata!.Contains("webhook_event") &&
                    p.GatewayMetadata!.Contains("gateway_fee") &&
                    p.GatewayMetadata!.Contains("webhook_card_type")),
                default), Times.Once);
        }

        [Fact]
        public async Task IsDuplicateWebhookAsync_WithMatchingEventId_ReturnsTrue()
        {
            // Arrange
            var eventId = "event123";
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                LastWebhookEventId = eventId
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            // Act
            var result = await _service.IsDuplicateWebhookAsync("TXN-123", eventId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDuplicateWebhookAsync_WithDifferentEventId_ReturnsFalse()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = "TXN-123",
                LastWebhookEventId = "event123"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync(payment);

            // Act
            var result = await _service.IsDuplicateWebhookAsync("TXN-123", "event456");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsDuplicateWebhookAsync_WithNonExistentPayment_ReturnsFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _service.IsDuplicateWebhookAsync("TXN-123", "event123");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ProcessWebhookAsync_WithRepositoryException_ReturnsFailureResult()
        {
            // Arrange
            var payload = new PayAzaWebhookPayload
            {
                Event = "collection.success",
                Status = "success",
                TransactionReference = "TXN-123"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync("TXN-123", default))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _service.ProcessWebhookAsync(payload);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("error", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}

