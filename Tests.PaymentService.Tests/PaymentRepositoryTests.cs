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
    /// Unit tests for PaymentRepository.
    /// </summary>
    public class PaymentRepositoryTests : IDisposable
    {
        private readonly PaymentServiceDbContext _context;
        private readonly PaymentRepository _repository;
        private readonly Mock<ILogger<PaymentRepository>> _mockLogger;

        public PaymentRepositoryTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<PaymentServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PaymentServiceDbContext(options);
            _mockLogger = new Mock<ILogger<PaymentRepository>>();
            _repository = new PaymentRepository(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateAsync_ValidPayment_CreatesSuccessfully()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = "PAY-20240115-ABC123",
                Gateway = "PayAza",
                Amount = 5000m,
                Currency = "NGN",
                Status = "PENDING_REDIRECT",
                IsActive = true
            };

            // Act
            var result = await _repository.CreateAsync(payment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payment.PaymentReference, result.PaymentReference);
            Assert.Equal(payment.Amount, result.Amount);
            Assert.True(result.CreatedAt > DateTime.MinValue);
        }

        [Fact]
        public async Task CreateAsync_NullPayment_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync(null!));
        }

        [Fact]
        public async Task GetByIdAsync_ExistingPayment_ReturnsPayment()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = "PAY-20240115-XYZ789",
                Gateway = "PayAza",
                Amount = 3000m,
                Currency = "NGN",
                Status = "PENDING",
                IsActive = true
            };
            await _repository.CreateAsync(payment);

            // Act
            var result = await _repository.GetByIdAsync(payment.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payment.Id, result.Id);
            Assert.Equal(payment.PaymentReference, result.PaymentReference);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentPayment_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByReferenceAsync_ExistingReference_ReturnsPayment()
        {
            // Arrange
            var reference = "PAY-20240115-REF001";
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = reference,
                Gateway = "PayAza",
                Amount = 2500m,
                Currency = "NGN",
                Status = "COMPLETED",
                IsActive = true
            };
            await _repository.CreateAsync(payment);

            // Act
            var result = await _repository.GetByReferenceAsync(reference);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reference, result.PaymentReference);
        }

        [Fact]
        public async Task GetByReferenceAsync_NonExistentReference_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByReferenceAsync("NONEXISTENT-REF");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByReferenceAsync_EmptyReference_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByReferenceAsync(string.Empty));
        }

        [Fact]
        public async Task UpdateAsync_ValidPayment_UpdatesSuccessfully()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = "PAY-20240115-UPD001",
                Gateway = "PayAza",
                Amount = 4000m,
                Currency = "NGN",
                Status = "PENDING",
                IsActive = true
            };
            await _repository.CreateAsync(payment);

            // Act
            payment.Status = "COMPLETED";
            payment.CompletedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(payment);

            // Assert
            Assert.Equal("COMPLETED", result.Status);
            Assert.NotNull(result.CompletedAt);
            Assert.True(result.UpdatedAt > result.CreatedAt);
        }

        [Fact]
        public async Task UpdateAsync_NullPayment_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null!));
        }

        [Fact]
        public async Task ReferenceExistsAsync_ExistingReference_ReturnsTrue()
        {
            // Arrange
            var reference = "PAY-20240115-EXISTS";
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                PaymentReference = reference,
                Gateway = "PayAza",
                Amount = 1500m,
                Currency = "NGN",
                Status = "PENDING",
                IsActive = true
            };
            await _repository.CreateAsync(payment);

            // Act
            var result = await _repository.ReferenceExistsAsync(reference);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ReferenceExistsAsync_NonExistentReference_ReturnsFalse()
        {
            // Act
            var result = await _repository.ReferenceExistsAsync("NONEXISTENT-REF");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ExistingUser_ReturnsPayments()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var payment1 = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventId = Guid.NewGuid(),
                PaymentReference = "PAY-20240115-USER01",
                Gateway = "PayAza",
                Amount = 2000m,
                Currency = "NGN",
                Status = "COMPLETED",
                IsActive = true
            };
            var payment2 = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventId = Guid.NewGuid(),
                PaymentReference = "PAY-20240115-USER02",
                Gateway = "PayAza",
                Amount = 3000m,
                Currency = "NGN",
                Status = "PENDING",
                IsActive = true
            };
            await _repository.CreateAsync(payment1);
            await _repository.CreateAsync(payment2);

            // Act
            var (payments, totalCount) = await _repository.GetByUserIdAsync(userId, 1, 10);

            // Assert
            Assert.Equal(2, totalCount);
            Assert.Equal(2, payments.Count());
        }

        [Fact]
        public async Task GetByUserIdAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            for (int i = 0; i < 15; i++)
            {
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EventId = Guid.NewGuid(),
                    PaymentReference = $"PAY-20240115-PAGE{i:D2}",
                    Gateway = "PayAza",
                    Amount = 1000m * (i + 1),
                    Currency = "NGN",
                    Status = "COMPLETED",
                    IsActive = true
                };
                await _repository.CreateAsync(payment);
            }

            // Act
            var (payments, totalCount) = await _repository.GetByUserIdAsync(userId, 2, 10);

            // Assert
            Assert.Equal(15, totalCount);
            Assert.Equal(5, payments.Count()); // Second page should have 5 items
        }

        [Fact]
        public async Task GetByEventIdAsync_ExistingEvent_ReturnsPayments()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var payment1 = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = eventId,
                PaymentReference = "PAY-20240115-EVENT01",
                Gateway = "PayAza",
                Amount = 2500m,
                Currency = "NGN",
                Status = "COMPLETED",
                IsActive = true
            };
            var payment2 = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = eventId,
                PaymentReference = "PAY-20240115-EVENT02",
                Gateway = "PayAza",
                Amount = 3500m,
                Currency = "NGN",
                Status = "PENDING",
                IsActive = true
            };
            await _repository.CreateAsync(payment1);
            await _repository.CreateAsync(payment2);

            // Act
            var payments = await _repository.GetByEventIdAsync(eventId);

            // Assert
            Assert.Equal(2, payments.Count());
        }

        [Fact]
        public async Task GetByEventIdAsync_NonExistentEvent_ReturnsEmptyList()
        {
            // Act
            var payments = await _repository.GetByEventIdAsync(Guid.NewGuid());

            // Assert
            Assert.Empty(payments);
        }
    }
}

