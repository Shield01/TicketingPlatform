using Microsoft.Extensions.Logging;
using Modules.PaymentService.Configuration;
using Modules.PaymentService.Constants;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Models;
using Modules.PaymentService.Repositories;
using Moq;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for PaymentService.
    /// </summary>
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockRepository;
        private readonly PayAzaConfiguration _configuration;
        private readonly Mock<ILogger<Modules.PaymentService.Services.PaymentService>> _mockLogger;
        private readonly Modules.PaymentService.Services.PaymentService _service;

        public PaymentServiceTests()
        {
            _mockRepository = new Mock<IPaymentRepository>();
            _configuration = new PayAzaConfiguration
            {
                ApiKeyTest = "test_key",
                SecretKeyTest = "test_secret",
                Mode = "test",
                MerchantKey = "merchant_key_123",
                BaseUrlTest = "https://api.payaza.africa/live",
                BaseUrlLive = "https://api.payaza.africa/live"
            };
            _mockLogger = new Mock<ILogger<Modules.PaymentService.Services.PaymentService>>();
            _service = new Modules.PaymentService.Services.PaymentService(_mockRepository.Object, _configuration, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateSessionAsync_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new CreateSessionRequest
            {
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Quantity = 2,
                Amount = 10000m,
                Currency = "NGN",
                CustomerEmail = "test@example.com",
                CustomerName = "Test User",
                CustomerPhone = "+2348012345678"
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), default))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken ct) => p);

            // Act
            var result = await _service.CreateSessionAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.PaymentId);
            Assert.NotEmpty(result.TransactionReference);
            Assert.Contains("EVT-", result.TransactionReference);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(request.Currency, result.Currency);
            Assert.Equal(PaymentStatus.PendingRedirect, result.Status);
            Assert.Equal("PayAza", result.Gateway);
            Assert.NotEmpty(result.RedirectUrl);
            Assert.Contains("checkout-test.payaza.africa", result.RedirectUrl);
            Assert.Contains(result.TransactionReference, result.RedirectUrl);

            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Payment>(), default), Times.Once);
        }

        [Fact]
        public async Task CreateSessionAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateSessionAsync(null!));
        }

        [Fact]
        public async Task CreateSessionAsync_DuplicateReference_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateSessionRequest
            {
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Quantity = 1,
                Amount = 5000m,
                Currency = "NGN",
                CustomerEmail = "test@example.com",
                CustomerName = "Test User"
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), default))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateSessionAsync(request));
            Assert.Contains("Duplicate transaction reference", exception.Message);
        }

        [Fact]
        public async Task CreateSessionAsync_WithOptionalUrls_IncludesInRedirectUrl()
        {
            // Arrange
            var request = new CreateSessionRequest
            {
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Quantity = 1,
                Amount = 5000m,
                Currency = "NGN",
                CustomerEmail = "test@example.com",
                CustomerName = "Test User",
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), default))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken ct) => p);

            // Act
            var result = await _service.CreateSessionAsync(request);

            // Assert
            Assert.Contains("success_url=", result.RedirectUrl);
            Assert.Contains("cancel_url=", result.RedirectUrl);
        }

        [Fact]
        public async Task HandleWebRedirectCallbackAsync_SuccessfulPayment_UpdatesStatusToCompleted()
        {
            // Arrange
            var transactionReference = "PAY-20240115-ABC123";
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = transactionReference,
                Gateway = "PayAza",
                Amount = 5000m,
                Currency = "NGN",
                Status = PaymentStatus.PendingRedirect,
                IsActive = true
            };

            var callbackRequest = new WebRedirectCallbackRequest
            {
                TransactionReference = transactionReference,
                Status = "success",
                GatewayTransactionId = "GW_TXN_123",
                PaymentMethod = "card"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync(transactionReference, default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken ct) => p);

            // Act
            var result = await _service.HandleWebRedirectCallbackAsync(callbackRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(PaymentStatus.Completed, result.Status);
            Assert.Equal(payment.Id, result.PaymentId);

            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Payment>(p => 
                p.Status == PaymentStatus.Completed && 
                p.TransactionId == "GW_TXN_123" &&
                p.CompletedAt != null), default), Times.Once);
        }

        [Fact]
        public async Task HandleWebRedirectCallbackAsync_FailedPayment_UpdatesStatusToFailed()
        {
            // Arrange
            var transactionReference = "PAY-20240115-FAIL001";
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = transactionReference,
                Gateway = "PayAza",
                Amount = 5000m,
                Currency = "NGN",
                Status = PaymentStatus.PendingRedirect,
                IsActive = true
            };

            var callbackRequest = new WebRedirectCallbackRequest
            {
                TransactionReference = transactionReference,
                Status = "failed",
                GatewayTransactionId = "GW_TXN_FAIL",
                PaymentMethod = "card"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync(transactionReference, default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken ct) => p);

            // Act
            var result = await _service.HandleWebRedirectCallbackAsync(callbackRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(PaymentStatus.Failed, result.Status);

            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Payment>(p => 
                p.Status == PaymentStatus.Failed), default), Times.Once);
        }

        [Fact]
        public async Task HandleWebRedirectCallbackAsync_PaymentNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var callbackRequest = new WebRedirectCallbackRequest
            {
                TransactionReference = "NONEXISTENT-REF",
                Status = "success"
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync(It.IsAny<string>(), default))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _service.HandleWebRedirectCallbackAsync(callbackRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(Guid.Empty, result.PaymentId);
            Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);

            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), default), Times.Never);
        }

        [Fact]
        public async Task HandleWebRedirectCallbackAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _service.HandleWebRedirectCallbackAsync(null!));
        }

        [Fact]
        public async Task HandleWebRedirectCallbackAsync_WithMetadata_StoresMetadataAsJson()
        {
            // Arrange
            var transactionReference = "PAY-20240115-META001";
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = transactionReference,
                Gateway = "PayAza",
                Amount = 5000m,
                Currency = "NGN",
                Status = PaymentStatus.PendingRedirect,
                IsActive = true
            };

            var callbackRequest = new WebRedirectCallbackRequest
            {
                TransactionReference = transactionReference,
                Status = "success",
                Metadata = new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                }
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync(transactionReference, default))
                .ReturnsAsync(payment);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), default))
                .ReturnsAsync((Payment p, CancellationToken ct) => p);

            // Act
            var result = await _service.HandleWebRedirectCallbackAsync(callbackRequest);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Payment>(p => 
                !string.IsNullOrEmpty(p.GatewayMetadata)), default), Times.Once);
        }

        [Fact]
        public async Task GetPaymentStatusAsync_ExistingPayment_ReturnsPaymentDetails()
        {
            // Arrange
            var transactionReference = "PAY-20240115-STATUS001";
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = transactionReference,
                Gateway = "PayAza",
                Amount = 5000m,
                Currency = "NGN",
                Status = PaymentStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockRepository.Setup(r => r.GetByReferenceAsync(transactionReference, default))
                .ReturnsAsync(payment);

            // Act
            var result = await _service.GetPaymentStatusAsync(transactionReference);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payment.Id, result.Id);
            Assert.Equal(payment.UserId, result.UserId);
            Assert.Equal(payment.EventId, result.EventId);
            Assert.Equal(payment.Amount, result.Amount);
            Assert.Equal(payment.Currency, result.Currency);
            Assert.Equal(payment.Status, result.Status);
            Assert.Equal(payment.PaymentReference, result.Reference);
        }

        [Fact]
        public async Task GetPaymentStatusAsync_NonExistentPayment_ReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByReferenceAsync(It.IsAny<string>(), default))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _service.GetPaymentStatusAsync("NONEXISTENT-REF");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPaymentStatusAsync_EmptyReference_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetPaymentStatusAsync(string.Empty));
        }
    }
}

