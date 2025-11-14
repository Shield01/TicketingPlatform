using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.PaymentService.Constants;
using Modules.PaymentService.Controllers;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Services;
using Moq;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for PaymentController.
    /// </summary>
    public class PaymentControllerTests
    {
        private readonly Mock<ILogger<PaymentController>> _mockLogger;
        private readonly Mock<IPaymentService> _mockService;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _mockLogger = new Mock<ILogger<PaymentController>>();
            _mockService = new Mock<IPaymentService>();
            _controller = new PaymentController(_mockLogger.Object, _mockService.Object);
        }

        [Fact]
        public async Task CreateSession_ValidRequest_ReturnsOkResult()
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
                CustomerName = "Test User"
            };

            var expectedResponse = new CreateSessionResponse
            {
                PaymentId = Guid.NewGuid(),
                TransactionReference = "PAY-20240115-ABC123",
                RedirectUrl = "https://checkout-test.payaza.africa?transaction_reference=PAY-20240115-ABC123",
                Amount = 10000m,
                Currency = "NGN",
                Status = PaymentStatus.PendingRedirect,
                Gateway = "PayAza",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow
            };

            _mockService.Setup(s => s.CreateSessionAsync(request, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateSession(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CreateSessionResponse>(okResult.Value);
            Assert.Equal(expectedResponse.PaymentId, response.PaymentId);
            Assert.Equal(expectedResponse.TransactionReference, response.TransactionReference);
            Assert.Equal(expectedResponse.RedirectUrl, response.RedirectUrl);

            _mockService.Verify(s => s.CreateSessionAsync(request, default), Times.Once);
        }

        [Fact]
        public async Task CreateSession_DuplicateReference_ReturnsConflict()
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

            _mockService.Setup(s => s.CreateSessionAsync(request, default))
                .ThrowsAsync(new InvalidOperationException("Duplicate transaction reference"));

            // Act
            var result = await _controller.CreateSession(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflictResult.Value);
        }

        [Fact]
        public async Task CreateSession_ServiceThrowsException_ReturnsInternalServerError()
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

            _mockService.Setup(s => s.CreateSessionAsync(request, default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateSession(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task HandleWebRedirectCallback_SuccessfulPayment_ReturnsOkResult()
        {
            // Arrange
            var request = new WebRedirectCallbackRequest
            {
                TransactionReference = "PAY-20240115-SUCCESS",
                Status = "success",
                GatewayTransactionId = "GW_TXN_123",
                PaymentMethod = "card"
            };

            var expectedResponse = new WebRedirectCallbackResponse
            {
                PaymentId = Guid.NewGuid(),
                TransactionReference = request.TransactionReference,
                Status = PaymentStatus.Completed,
                Message = "Payment completed successfully",
                Success = true
            };

            _mockService.Setup(s => s.HandleWebRedirectCallbackAsync(request, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.HandleWebRedirectCallback(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<WebRedirectCallbackResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(PaymentStatus.Completed, response.Status);

            _mockService.Verify(s => s.HandleWebRedirectCallbackAsync(request, default), Times.Once);
        }

        [Fact]
        public async Task HandleWebRedirectCallback_PaymentNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new WebRedirectCallbackRequest
            {
                TransactionReference = "NONEXISTENT-REF",
                Status = "success"
            };

            var expectedResponse = new WebRedirectCallbackResponse
            {
                PaymentId = Guid.Empty,
                TransactionReference = request.TransactionReference,
                Status = PaymentStatus.Failed,
                Message = "Payment not found",
                Success = false
            };

            _mockService.Setup(s => s.HandleWebRedirectCallbackAsync(request, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.HandleWebRedirectCallback(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<WebRedirectCallbackResponse>(notFoundResult.Value);
            Assert.False(response.Success);
            Assert.Equal(Guid.Empty, response.PaymentId);
        }

        [Fact]
        public async Task HandleWebRedirectCallback_FailedPayment_ReturnsOkWithFailureStatus()
        {
            // Arrange
            var request = new WebRedirectCallbackRequest
            {
                TransactionReference = "PAY-20240115-FAILED",
                Status = "failed",
                GatewayTransactionId = "GW_TXN_FAIL"
            };

            var expectedResponse = new WebRedirectCallbackResponse
            {
                PaymentId = Guid.NewGuid(),
                TransactionReference = request.TransactionReference,
                Status = PaymentStatus.Failed,
                Message = "Payment failed",
                Success = false
            };

            _mockService.Setup(s => s.HandleWebRedirectCallbackAsync(request, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.HandleWebRedirectCallback(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<WebRedirectCallbackResponse>(okResult.Value);
            Assert.False(response.Success);
            Assert.Equal(PaymentStatus.Failed, response.Status);
        }

        [Fact]
        public async Task HandleWebRedirectCallback_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new WebRedirectCallbackRequest
            {
                TransactionReference = "PAY-20240115-ERROR",
                Status = "success"
            };

            _mockService.Setup(s => s.HandleWebRedirectCallbackAsync(request, default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.HandleWebRedirectCallback(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task HandleWebRedirectCallback_CancelledPayment_ReturnsOkWithCancelledStatus()
        {
            // Arrange
            var request = new WebRedirectCallbackRequest
            {
                TransactionReference = "PAY-20240115-CANCELLED",
                Status = "cancelled"
            };

            var expectedResponse = new WebRedirectCallbackResponse
            {
                PaymentId = Guid.NewGuid(),
                TransactionReference = request.TransactionReference,
                Status = PaymentStatus.Cancelled,
                Message = "Payment was cancelled",
                Success = false
            };

            _mockService.Setup(s => s.HandleWebRedirectCallbackAsync(request, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.HandleWebRedirectCallback(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<WebRedirectCallbackResponse>(okResult.Value);
            Assert.False(response.Success);
            Assert.Equal(PaymentStatus.Cancelled, response.Status);
        }
    }
}

