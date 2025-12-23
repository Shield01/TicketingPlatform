using Microsoft.Extensions.Logging;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Infrastructure;
using Modules.PaymentService.Infrastructure.DTOs;
using Modules.PaymentService.Infrastructure.Exceptions;
using Modules.PaymentService.Models;
using Modules.PaymentService.Repositories;
using Modules.PaymentService.Services;
using Moq;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for PayoutService.
    /// </summary>
    public class PayoutServiceTests
    {
        private readonly Mock<IPayoutRepository> _mockRepository;
        private readonly Mock<IPayAzaClient> _mockPayAzaClient;
        private readonly Mock<ILogger<PayoutService>> _mockLogger;
        private readonly PayoutService _service;

        public PayoutServiceTests()
        {
            _mockRepository = new Mock<IPayoutRepository>();
            _mockPayAzaClient = new Mock<IPayAzaClient>();
            _mockLogger = new Mock<ILogger<PayoutService>>();
            _service = new PayoutService(_mockRepository.Object, _mockPayAzaClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InitiatePayoutAsync_ValidRequest_CreatesPayoutSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                Narration = "Test payout",
                IsDryRun = false
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<PayoutTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutTransaction p, CancellationToken ct) => p);

            _mockPayAzaClient.Setup(c => c.InitiatePayoutAsync(It.IsAny<PayAzaPayoutRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PayAzaPayoutResponse
                {
                    Success = true,
                    Message = "Payout initiated",
                    Data = new PayAzaPayoutData
                    {
                        TransactionReference = "PAYOUT-TEST-001",
                        Status = "processing",
                        Amount = 50000m,
                        Fee = 150m,
                        CreatedAt = DateTime.UtcNow
                    }
                });

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<PayoutTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutTransaction p, CancellationToken ct) => p);

            // Act
            var result = await _service.InitiatePayoutAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(request.Currency, result.Currency);
            Assert.Equal(request.AccountNumber, result.AccountNumber);
            Assert.Equal(request.AccountName, result.AccountName);
            Assert.False(result.IsDryRun);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<PayoutTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockPayAzaClient.Verify(c => c.InitiatePayoutAsync(It.IsAny<PayAzaPayoutRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task InitiatePayoutAsync_DryRun_DoesNotCallPayAza()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                IsDryRun = true
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<PayoutTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutTransaction p, CancellationToken ct) => p);

            // Act
            var result = await _service.InitiatePayoutAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDryRun);
            _mockPayAzaClient.Verify(c => c.InitiatePayoutAsync(It.IsAny<PayAzaPayoutRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task InitiatePayoutAsync_DuplicateReference_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                TransactionReference = "EXISTING-REF"
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync("EXISTING-REF", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.InitiatePayoutAsync(request, userId));
        }

        [Fact]
        public async Task InitiatePayoutAsync_PayAzaFailure_MarksPayoutAsFailed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                IsDryRun = false
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<PayoutTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutTransaction p, CancellationToken ct) => p);

            _mockPayAzaClient.Setup(c => c.InitiatePayoutAsync(It.IsAny<PayAzaPayoutRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PayAzaPayoutResponse
                {
                    Success = false,
                    Message = "Insufficient funds",
                    Error = new PayAzaErrorDetails
                    {
                        Code = "INSUFFICIENT_FUNDS",
                        Message = "Insufficient funds in merchant account"
                    }
                });

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<PayoutTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutTransaction p, CancellationToken ct) => p);

            // Act
            var result = await _service.InitiatePayoutAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PayoutStatus.FAILED, result.Status);
            Assert.NotNull(result.ErrorMessage);
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<PayoutTransaction>(p => p.Status == PayoutStatus.FAILED), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyAccountAsync_ValidAccount_ReturnsSuccess()
        {
            // Arrange
            var request = new AccountEnquiryRequest
            {
                AccountNumber = "0123456789",
                BankCode = "058"
            };

            _mockPayAzaClient.Setup(c => c.GetAccountDetailsAsync("0123456789", "058", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PayAzaAccountDetailsResponse
                {
                    Success = true,
                    Message = "Account verified",
                    Data = new PayAzaAccountData
                    {
                        AccountNumber = "0123456789",
                        AccountName = "John Doe",
                        BankCode = "058",
                        BankName = "GTBank",
                        Currency = "NGN",
                        Balance = 100000m
                    }
                });

            // Act
            var result = await _service.VerifyAccountAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("John Doe", result.AccountName);
            Assert.Equal("GTBank", result.BankName);
            Assert.Equal("0123456789", result.AccountNumber);
        }

        [Fact]
        public async Task VerifyAccountAsync_InvalidAccount_ReturnsFailure()
        {
            // Arrange
            var request = new AccountEnquiryRequest
            {
                AccountNumber = "9999999999",
                BankCode = "058"
            };

            _mockPayAzaClient.Setup(c => c.GetAccountDetailsAsync("9999999999", "058", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new PayAzaNotFoundException("Account not found"));

            // Act
            var result = await _service.VerifyAccountAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPayoutByIdAsync_ExistingPayout_ReturnsPayout()
        {
            // Arrange
            var payoutId = Guid.NewGuid();
            var payout = new PayoutTransaction
            {
                Id = payoutId,
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-TEST-001",
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                Status = PayoutStatus.COMPLETED
            };

            _mockRepository.Setup(r => r.GetByIdAsync(payoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payout);

            // Act
            var result = await _service.GetPayoutByIdAsync(payoutId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payoutId, result.PayoutId);
            Assert.Equal("PAYOUT-TEST-001", result.TransactionReference);
        }

        [Fact]
        public async Task GetPayoutByIdAsync_NonExistentPayout_ReturnsNull()
        {
            // Arrange
            var payoutId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(payoutId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutTransaction?)null);

            // Act
            var result = await _service.GetPayoutByIdAsync(payoutId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPayoutsByUserIdAsync_MultiplePayouts_ReturnsPaginatedList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var payouts = new List<PayoutTransaction>
            {
                new PayoutTransaction
                {
                    Id = Guid.NewGuid(),
                    InitiatedByUserId = userId,
                    TransactionReference = "PAYOUT-001",
                    Amount = 10000m,
                    Currency = "NGN",
                    AccountNumber = "1111111111",
                    BankCode = "058",
                    AccountName = "User 1",
                    Status = PayoutStatus.COMPLETED
                },
                new PayoutTransaction
                {
                    Id = Guid.NewGuid(),
                    InitiatedByUserId = userId,
                    TransactionReference = "PAYOUT-002",
                    Amount = 20000m,
                    Currency = "NGN",
                    AccountNumber = "2222222222",
                    BankCode = "044",
                    AccountName = "User 2",
                    Status = PayoutStatus.COMPLETED
                }
            };

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId, 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync((payouts, 2));

            // Act
            var (result, totalCount) = await _service.GetPayoutsByUserIdAsync(userId, 1, 20);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, totalCount);
        }

        [Fact]
        public async Task PreviewPayoutAsync_ForcesDryRun()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new InitiatePayoutRequest
            {
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                IsDryRun = false  // Explicitly set to false
            };

            _mockRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<PayoutTransaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PayoutTransaction p, CancellationToken ct) => p);

            // Act
            var result = await _service.PreviewPayoutAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDryRun);  // Should be forced to true
            _mockPayAzaClient.Verify(c => c.InitiatePayoutAsync(It.IsAny<PayAzaPayoutRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAccountDetailsAsync_ReturnsStatistics()
        {
            // Arrange
            var statistics = new PayoutStatistics
            {
                TotalPayouts = 100,
                CompletedPayouts = 80,
                FailedPayouts = 15,
                PendingPayouts = 5,
                TotalAmount = 5000000m,
                TotalFees = 50000m,
                Currency = "NGN"
            };

            _mockRepository.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(statistics);

            _mockRepository.Setup(r => r.GetByUserIdAsync(Guid.Empty, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<PayoutTransaction>(), 0));

            // Act
            var result = await _service.GetAccountDetailsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.TotalPayouts);
            Assert.Equal(80, result.CompletedPayoutsCount);
            Assert.Equal(15, result.FailedPayoutsCount);
            Assert.Equal(5, result.PendingPayoutsCount);
            Assert.Equal(5000000m, result.TotalAmount);
        }
    }
}

