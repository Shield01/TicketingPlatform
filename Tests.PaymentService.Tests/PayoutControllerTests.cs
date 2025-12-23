using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.PaymentService.Controllers;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Models;
using Modules.PaymentService.Services;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for PayoutController.
    /// </summary>
    public class PayoutControllerTests
    {
        private readonly Mock<ILogger<PayoutController>> _mockLogger;
        private readonly Mock<IPayoutService> _mockService;
        private readonly PayoutController _controller;
        private readonly Guid _testUserId;

        public PayoutControllerTests()
        {
            _mockLogger = new Mock<ILogger<PayoutController>>();
            _mockService = new Mock<IPayoutService>();
            _controller = new PayoutController(_mockLogger.Object, _mockService.Object);
            _testUserId = Guid.NewGuid();

            // Setup HttpContext with user claims
            var claims = new List<Claim>
            {
                new Claim("UserId", _testUserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task InitiatePayout_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                Narration = "Test payout"
            };

            var expectedResponse = new PayoutResponse
            {
                PayoutId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-TEST-001",
                Amount = 50000m,
                Currency = "NGN",
                Status = PayoutStatus.INITIATED,
                AccountNumber = "0123456789",
                AccountName = "John Doe",
                BankCode = "058",
                Message = "Payout initiated successfully."
            };

            _mockService.Setup(s => s.InitiatePayoutAsync(request, _testUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.InitiatePayout(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PayoutResponse>(okResult.Value);
            Assert.Equal(expectedResponse.TransactionReference, response.TransactionReference);
            Assert.Equal(expectedResponse.Amount, response.Amount);
        }

        [Fact]
        public async Task InitiatePayout_DuplicateReference_ReturnsConflict()
        {
            // Arrange
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                TransactionReference = "EXISTING-REF"
            };

            _mockService.Setup(s => s.InitiatePayoutAsync(request, _testUserId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Duplicate transaction reference detected"));

            // Act
            var result = await _controller.InitiatePayout(request);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task InitiatePayout_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var request = new InitiatePayoutRequest
            {
                Amount = -1000m,  // Invalid amount
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe"
            };

            _mockService.Setup(s => s.InitiatePayoutAsync(request, _testUserId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Amount must be greater than zero."));

            // Act
            var result = await _controller.InitiatePayout(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task VerifyAccount_ValidAccount_ReturnsOk()
        {
            // Arrange
            var request = new AccountEnquiryRequest
            {
                AccountNumber = "0123456789",
                BankCode = "058"
            };

            var expectedResponse = new AccountEnquiryResponse
            {
                Success = true,
                AccountNumber = "0123456789",
                AccountName = "John Doe",
                BankCode = "058",
                BankName = "GTBank",
                Currency = "NGN",
                Message = "Account verified successfully."
            };

            _mockService.Setup(s => s.VerifyAccountAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.VerifyAccount(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AccountEnquiryResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("John Doe", response.AccountName);
        }

        [Fact]
        public async Task VerifyAccount_InvalidAccount_ReturnsBadRequest()
        {
            // Arrange
            var request = new AccountEnquiryRequest
            {
                AccountNumber = "9999999999",
                BankCode = "058"
            };

            var expectedResponse = new AccountEnquiryResponse
            {
                Success = false,
                AccountNumber = "9999999999",
                BankCode = "058",
                Message = "Account verification failed.",
                ErrorMessage = "Invalid account number"
            };

            _mockService.Setup(s => s.VerifyAccountAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.VerifyAccount(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PreviewPayout_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe"
            };

            var expectedResponse = new PayoutResponse
            {
                PayoutId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-PREVIEW-001",
                Amount = 50000m,
                Currency = "NGN",
                Status = PayoutStatus.INITIATED,
                AccountNumber = "0123456789",
                AccountName = "John Doe",
                BankCode = "058",
                IsDryRun = true,
                Message = "This is a preview/dry-run payout (not executed)."
            };

            _mockService.Setup(s => s.PreviewPayoutAsync(request, _testUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.PreviewPayout(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PayoutResponse>(okResult.Value);
            Assert.True(response.IsDryRun);
        }

        [Fact]
        public async Task GetPayoutById_ExistingPayout_ReturnsOk()
        {
            // Arrange
            var payoutId = Guid.NewGuid();
            var expectedResponse = new PayoutResponse
            {
                PayoutId = payoutId,
                TransactionReference = "PAYOUT-TEST-001",
                Amount = 50000m,
                Currency = "NGN",
                Status = PayoutStatus.COMPLETED,
                AccountNumber = "0123456789",
                AccountName = "John Doe",
                BankCode = "058"
            };

            _mockService.Setup(s => s.GetPayoutByIdAsync(payoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPayoutById(payoutId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PayoutResponse>(okResult.Value);
            Assert.Equal(payoutId, response.PayoutId);
        }

        [Fact]
        public async Task GetPayoutById_NonExistentPayout_ReturnsNotFound()
        {
            // Arrange
            var payoutId = Guid.NewGuid();
            _mockService.Setup(s => s.GetPayoutByIdAsync(payoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutResponse?)null);

            // Act
            var result = await _controller.GetPayoutById(payoutId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetMyPayouts_ValidRequest_ReturnsOk()
        {
            // Arrange
            var payouts = new List<PayoutResponse>
            {
                new PayoutResponse
                {
                    PayoutId = Guid.NewGuid(),
                    TransactionReference = "PAYOUT-001",
                    Amount = 10000m,
                    Currency = "NGN",
                    Status = PayoutStatus.COMPLETED,
                    AccountNumber = "1111111111",
                    AccountName = "User 1",
                    BankCode = "058"
                },
                new PayoutResponse
                {
                    PayoutId = Guid.NewGuid(),
                    TransactionReference = "PAYOUT-002",
                    Amount = 20000m,
                    Currency = "NGN",
                    Status = PayoutStatus.COMPLETED,
                    AccountNumber = "2222222222",
                    AccountName = "User 2",
                    BankCode = "044"
                }
            };

            _mockService.Setup(s => s.GetPayoutsByUserIdAsync(_testUserId, 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync((payouts, 2));

            // Act
            var result = await _controller.GetMyPayouts(1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetMyPayouts_ExceedsMaxPageSize_LimitsTo100()
        {
            // Arrange
            _mockService.Setup(s => s.GetPayoutsByUserIdAsync(_testUserId, 1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<PayoutResponse>(), 0));

            // Act
            var result = await _controller.GetMyPayouts(1, 200);  // Request 200, should be capped to 100

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockService.Verify(s => s.GetPayoutsByUserIdAsync(_testUserId, 1, 100, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountDetails_ValidRequest_ReturnsOk()
        {
            // Arrange
            var expectedResponse = new AccountDetailsResponse
            {
                TotalPayouts = 100,
                TotalAmount = 5000000m,
                Currency = "NGN",
                PendingPayoutsCount = 5,
                CompletedPayoutsCount = 80,
                FailedPayoutsCount = 15,
                RecentPayouts = new List<PayoutResponse>()
            };

            _mockService.Setup(s => s.GetAccountDetailsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAccountDetails();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AccountDetailsResponse>(okResult.Value);
            Assert.Equal(100, response.TotalPayouts);
            Assert.Equal(5000000m, response.TotalAmount);
        }
    }
}

