using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.PaymentService.Data;
using Modules.PaymentService.Models;
using Modules.PaymentService.Repositories;
using Moq;
using Xunit;

namespace Tests.PaymentService.Tests
{
    /// <summary>
    /// Unit tests for PayoutRepository.
    /// </summary>
    public class PayoutRepositoryTests : IDisposable
    {
        private readonly PaymentServiceDbContext _context;
        private readonly PayoutRepository _repository;
        private readonly Mock<ILogger<PayoutRepository>> _mockLogger;

        public PayoutRepositoryTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<PaymentServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PaymentServiceDbContext(options);
            _mockLogger = new Mock<ILogger<PayoutRepository>>();
            _repository = new PayoutRepository(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateAsync_ValidPayout_CreatesSuccessfully()
        {
            // Arrange
            var payout = new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-20240115-ABC123",
                Amount = 50000m,
                Currency = "NGN",
                AccountNumber = "0123456789",
                BankCode = "058",
                AccountName = "John Doe",
                Status = PayoutStatus.INITIATED,
                Gateway = "PayAza",
                IsActive = true
            };

            // Act
            var result = await _repository.CreateAsync(payout);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payout.TransactionReference, result.TransactionReference);
            Assert.Equal(payout.Amount, result.Amount);
            Assert.True(result.CreatedAt > DateTime.MinValue);
        }

        [Fact]
        public async Task CreateAsync_NullPayout_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync(null!));
        }

        [Fact]
        public async Task GetByIdAsync_ExistingPayout_ReturnsPayout()
        {
            // Arrange
            var payout = new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-20240115-XYZ789",
                Amount = 30000m,
                Currency = "NGN",
                AccountNumber = "9876543210",
                BankCode = "044",
                AccountName = "Jane Smith",
                Status = PayoutStatus.COMPLETED,
                IsActive = true
            };
            await _repository.CreateAsync(payout);

            // Act
            var result = await _repository.GetByIdAsync(payout.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payout.Id, result.Id);
            Assert.Equal(payout.TransactionReference, result.TransactionReference);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentPayout_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByReferenceAsync_ExistingReference_ReturnsPayout()
        {
            // Arrange
            var payout = new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-20240115-DEF456",
                Amount = 25000m,
                Currency = "NGN",
                AccountNumber = "1111222233",
                BankCode = "033",
                AccountName = "Test User",
                Status = PayoutStatus.PROCESSING,
                IsActive = true
            };
            await _repository.CreateAsync(payout);

            // Act
            var result = await _repository.GetByReferenceAsync("PAYOUT-20240115-DEF456");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payout.TransactionReference, result.TransactionReference);
        }

        [Fact]
        public async Task GetByReferenceAsync_NonExistentReference_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByReferenceAsync("NON-EXISTENT-REF");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ExistingPayout_UpdatesSuccessfully()
        {
            // Arrange
            var payout = new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-20240115-GHI789",
                Amount = 15000m,
                Currency = "NGN",
                AccountNumber = "4444555566",
                BankCode = "011",
                AccountName = "Update Test",
                Status = PayoutStatus.INITIATED,
                IsActive = true
            };
            await _repository.CreateAsync(payout);

            // Act
            payout.Status = PayoutStatus.COMPLETED;
            payout.GatewayFee = 150m;
            var result = await _repository.UpdateAsync(payout);

            // Assert
            Assert.Equal(PayoutStatus.COMPLETED, result.Status);
            Assert.Equal(150m, result.GatewayFee);
        }

        [Fact]
        public async Task ReferenceExistsAsync_ExistingReference_ReturnsTrue()
        {
            // Arrange
            var payout = new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-UNIQUE-REF-001",
                Amount = 10000m,
                Currency = "NGN",
                AccountNumber = "7777888899",
                BankCode = "057",
                AccountName = "Unique Test",
                Status = PayoutStatus.INITIATED,
                IsActive = true
            };
            await _repository.CreateAsync(payout);

            // Act
            var result = await _repository.ReferenceExistsAsync("PAYOUT-UNIQUE-REF-001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ReferenceExistsAsync_NonExistentReference_ReturnsFalse()
        {
            // Act
            var result = await _repository.ReferenceExistsAsync("NON-EXISTENT-REF");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ExistingPayouts_ReturnsPaginatedList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            for (int i = 0; i < 5; i++)
            {
                await _repository.CreateAsync(new PayoutTransaction
                {
                    Id = Guid.NewGuid(),
                    InitiatedByUserId = userId,
                    TransactionReference = $"PAYOUT-USER-{i}",
                    Amount = 1000m * (i + 1),
                    Currency = "NGN",
                    AccountNumber = "1234567890",
                    BankCode = "058",
                    AccountName = "Test User",
                    Status = PayoutStatus.COMPLETED,
                    IsActive = true
                });
            }

            // Act
            var (payouts, totalCount) = await _repository.GetByUserIdAsync(userId, page: 1, pageSize: 3);

            // Assert
            Assert.Equal(3, payouts.Count);
            Assert.Equal(5, totalCount);
        }

        [Fact]
        public async Task GetByStatusAsync_MultipleStatuses_ReturnsFilteredPayouts()
        {
            // Arrange
            await _repository.CreateAsync(new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-INITIATED-001",
                Amount = 5000m,
                Currency = "NGN",
                AccountNumber = "1111111111",
                BankCode = "058",
                AccountName = "User 1",
                Status = PayoutStatus.INITIATED,
                IsActive = true
            });

            await _repository.CreateAsync(new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-PROCESSING-001",
                Amount = 7000m,
                Currency = "NGN",
                AccountNumber = "2222222222",
                BankCode = "044",
                AccountName = "User 2",
                Status = PayoutStatus.PROCESSING,
                IsActive = true
            });

            await _repository.CreateAsync(new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-COMPLETED-001",
                Amount = 9000m,
                Currency = "NGN",
                AccountNumber = "3333333333",
                BankCode = "033",
                AccountName = "User 3",
                Status = PayoutStatus.COMPLETED,
                IsActive = true
            });

            // Act
            var result = await _repository.GetByStatusAsync(new[] { PayoutStatus.INITIATED, PayoutStatus.PROCESSING });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Status == PayoutStatus.INITIATED);
            Assert.Contains(result, p => p.Status == PayoutStatus.PROCESSING);
        }

        [Fact]
        public async Task GetStatisticsAsync_MultiplePayouts_ReturnsCorrectStatistics()
        {
            // Arrange
            await _repository.CreateAsync(new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-STAT-001",
                Amount = 10000m,
                Currency = "NGN",
                AccountNumber = "1111111111",
                BankCode = "058",
                AccountName = "Stat User 1",
                Status = PayoutStatus.COMPLETED,
                GatewayFee = 100m,
                IsDryRun = false,
                IsActive = true
            });

            await _repository.CreateAsync(new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-STAT-002",
                Amount = 20000m,
                Currency = "NGN",
                AccountNumber = "2222222222",
                BankCode = "044",
                AccountName = "Stat User 2",
                Status = PayoutStatus.FAILED,
                IsDryRun = false,
                IsActive = true
            });

            await _repository.CreateAsync(new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = Guid.NewGuid(),
                TransactionReference = "PAYOUT-STAT-003",
                Amount = 15000m,
                Currency = "NGN",
                AccountNumber = "3333333333",
                BankCode = "033",
                AccountName = "Stat User 3",
                Status = PayoutStatus.PROCESSING,
                IsDryRun = false,
                IsActive = true
            });

            // Act
            var statistics = await _repository.GetStatisticsAsync();

            // Assert
            Assert.Equal(3, statistics.TotalPayouts);
            Assert.Equal(1, statistics.CompletedPayouts);
            Assert.Equal(1, statistics.FailedPayouts);
            Assert.Equal(1, statistics.PendingPayouts);
            Assert.Equal(10000m, statistics.TotalAmount);
            Assert.Equal(100m, statistics.TotalFees);
        }
    }
}

