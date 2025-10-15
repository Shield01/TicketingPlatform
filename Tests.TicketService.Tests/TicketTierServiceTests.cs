using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Modules.TicketService.Services;
using Shared.Kernel.Interfaces;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for TicketTierService business logic and validation.
    /// </summary>
    public class TicketTierServiceTests
    {
        private readonly Mock<ITicketTierRepository> _mockRepository;
        private readonly Mock<IEventMinimumPriceService> _mockEventMinimumPriceService;
        private readonly Mock<ILogger<TicketTierService>> _mockLogger;
        private readonly TicketTierService _service;

        public TicketTierServiceTests()
        {
            _mockRepository = new Mock<ITicketTierRepository>();
            _mockEventMinimumPriceService = new Mock<IEventMinimumPriceService>();
            _mockLogger = new Mock<ILogger<TicketTierService>>();
            
            // Setup default behavior - minimum price service succeeds
            _mockEventMinimumPriceService.Setup(x => x.UpdateMinimumPriceIfLowerAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .ReturnsAsync((Guid eventId, decimal price, string currency) => price);
            _mockEventMinimumPriceService.Setup(x => x.RecalculateAndUpdateMinimumPriceAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid eventId) => (decimal?)100m);
            
            _service = new TicketTierService(_mockRepository.Object, _mockEventMinimumPriceService.Object, _mockLogger.Object);
        }

        #region CreateTicketTierAsync Tests

        [Fact]
        public async Task CreateTicketTierAsync_ValidRequest_ReturnsTicketTierResponse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Description = "Premium access",
                Price = 150.00m,
                Currency = "USD",
                MaxQuantity = 50,
                IsAvailable = true
            };

            _mockRepository.Setup(r => r.TierNameExistsForEventAsync(eventId, request.Name, null))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync((TicketTier tier) =>
                {
                    tier.Id = Guid.NewGuid();
                    tier.CreatedAt = DateTime.UtcNow;
                    tier.UpdatedAt = DateTime.UtcNow;
                    return tier;
                });

            // Act
            var result = await _service.CreateTicketTierAsync(eventId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Price, result.Price);
            Assert.Equal(request.Currency, result.Currency);
            Assert.Equal(request.MaxQuantity, result.MaxQuantity);
            Assert.Equal(0, result.SoldQuantity);
            Assert.Equal(request.IsAvailable, result.IsAvailable);
            Assert.True(result.IsActive);

            _mockRepository.Verify(r => r.CreateTicketTierAsync(It.IsAny<TicketTier>()), Times.Once);
        }

        [Fact]
        public async Task CreateTicketTierAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateTicketTierAsync(eventId, null, userId));
        }

        [Fact]
        public async Task CreateTicketTierAsync_EmptyEventId_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(Guid.Empty, request, userId));
        }

        [Fact]
        public async Task CreateTicketTierAsync_EmptyUserId_ThrowsArgumentException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, Guid.Empty));
        }

        [Fact]
        public async Task CreateTicketTierAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            _mockRepository.Setup(r => r.TierNameExistsForEventAsync(eventId, request.Name, null))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));

            Assert.Contains("already exists", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CreateTicketTierAsync_InvalidName_ThrowsArgumentException(string name)
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = name,
                Price = 150.00m,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10.50)]
        public async Task CreateTicketTierAsync_InvalidPrice_ThrowsArgumentException(decimal price)
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = price,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task CreateTicketTierAsync_InvalidMaxQuantity_ThrowsArgumentException(int maxQuantity)
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = maxQuantity
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        [Fact]
        public async Task CreateTicketTierAsync_ValidSaleDates_Success()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(1);
            var endDate = DateTime.UtcNow.AddDays(7);
            
            var request = new CreateTicketTierRequest
            {
                Name = "Early Bird",
                Price = 75.00m,
                MaxQuantity = 100,
                SaleStartDate = startDate,
                SaleEndDate = endDate
            };

            _mockRepository.Setup(r => r.TierNameExistsForEventAsync(eventId, request.Name, null))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.CreateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync((TicketTier tier) =>
                {
                    tier.Id = Guid.NewGuid();
                    return tier;
                });

            // Act
            var result = await _service.CreateTicketTierAsync(eventId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(startDate, result.SaleStartDate);
            Assert.Equal(endDate, result.SaleEndDate);
        }

        [Fact]
        public async Task CreateTicketTierAsync_InvalidSaleDates_ThrowsArgumentException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(7);
            var endDate = DateTime.UtcNow.AddDays(1); // End before start
            
            var request = new CreateTicketTierRequest
            {
                Name = "Early Bird",
                Price = 75.00m,
                MaxQuantity = 100,
                SaleStartDate = startDate,
                SaleEndDate = endDate
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        #endregion

        #region UpdateTicketTierAsync Tests

        [Fact]
        public async Task UpdateTicketTierAsync_ValidRequest_ReturnsUpdatedTicketTier()
        {
            // Arrange
            var tierId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            
            var existingTier = new TicketTier
            {
                Id = tierId,
                EventId = eventId,
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                SoldQuantity = 10,
                IsAvailable = true,
                IsActive = true
            };

            var request = new CreateTicketTierRequest
            {
                Name = "VIP Premium",
                Description = "Updated description",
                Price = 200.00m,
                MaxQuantity = 60 // Can increase since sold quantity is 10
            };

            _mockRepository.Setup(r => r.GetTicketTierByIdAsync(tierId))
                .ReturnsAsync(existingTier);

            _mockRepository.Setup(r => r.TierNameExistsForEventAsync(eventId, request.Name, tierId))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.UpdateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync((TicketTier tier) => tier);

            // Act
            var result = await _service.UpdateTicketTierAsync(tierId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Price, result.Price);
            Assert.Equal(request.MaxQuantity, result.MaxQuantity);
            Assert.Equal(10, result.SoldQuantity); // Should preserve sold quantity

            _mockRepository.Verify(r => r.UpdateTicketTierAsync(It.IsAny<TicketTier>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTicketTierAsync_TierNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var tierId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            _mockRepository.Setup(r => r.GetTicketTierByIdAsync(tierId))
                .ReturnsAsync((TicketTier)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateTicketTierAsync(tierId, request, userId));

            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task UpdateTicketTierAsync_MaxQuantityLessThanSold_ThrowsInvalidOperationException()
        {
            // Arrange
            var tierId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            
            var existingTier = new TicketTier
            {
                Id = tierId,
                EventId = eventId,
                Name = "VIP",
                MaxQuantity = 50,
                SoldQuantity = 30 // 30 tickets already sold
            };

            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 20 // Trying to reduce to less than sold
            };

            _mockRepository.Setup(r => r.GetTicketTierByIdAsync(tierId))
                .ReturnsAsync(existingTier);

            _mockRepository.Setup(r => r.TierNameExistsForEventAsync(eventId, request.Name, tierId))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateTicketTierAsync(tierId, request, userId));

            Assert.Contains("cannot be less than already sold", exception.Message);
        }

        #endregion

        #region GetEventTicketTiersAsync Tests

        [Fact]
        public async Task GetEventTicketTiersAsync_ValidEventId_ReturnsTicketTiers()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "VIP",
                    Price = 150.00m,
                    MaxQuantity = 50,
                    SoldQuantity = 10,
                    IsAvailable = true
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 75.00m,
                    MaxQuantity = 200,
                    SoldQuantity = 50,
                    IsAvailable = true
                }
            };

            _mockRepository.Setup(r => r.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(tiers);

            // Act
            var result = await _service.GetEventTicketTiersAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            var resultList = result.ToList();
            Assert.Equal("VIP", resultList[0].Name);
            Assert.Equal(40, resultList[0].AvailableQuantity); // 50 - 10
            Assert.Equal("Regular", resultList[1].Name);
            Assert.Equal(150, resultList[1].AvailableQuantity); // 200 - 50
        }

        [Fact]
        public async Task GetEventTicketTiersAsync_EmptyEventId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetEventTicketTiersAsync(Guid.Empty));
        }

        #endregion

        #region DeleteTicketTierAsync Tests

        [Fact]
        public async Task DeleteTicketTierAsync_ValidTierId_ReturnsTrue()
        {
            // Arrange
            var tierId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockRepository.Setup(r => r.DeleteTicketTierAsync(tierId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteTicketTierAsync(tierId, userId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteTicketTierAsync(tierId), Times.Once);
        }

        [Fact]
        public async Task DeleteTicketTierAsync_EmptyTierId_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteTicketTierAsync(Guid.Empty, userId));
        }

        [Fact]
        public async Task DeleteTicketTierAsync_EmptyUserId_ThrowsArgumentException()
        {
            // Arrange
            var tierId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteTicketTierAsync(tierId, Guid.Empty));
        }

        #endregion

        #region Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task CreateTicketTierAsync_InvalidCurrency_ThrowsArgumentException(string currency)
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                Currency = currency,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        [Theory]
        [InlineData("US")]
        [InlineData("EURO")]
        [InlineData("")]
        public async Task CreateTicketTierAsync_InvalidCurrencyLength_ThrowsArgumentException(string currency)
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                Currency = currency,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        [Fact]
        public async Task CreateTicketTierAsync_DescriptionTooLong_ThrowsArgumentException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var longDescription = new string('a', 501); // 501 characters, exceeds 500 limit
            
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Description = longDescription,
                Price = 150.00m,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        [Fact]
        public async Task CreateTicketTierAsync_NameTooLong_ThrowsArgumentException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var longName = new string('a', 101); // 101 characters, exceeds 100 limit
            
            var request = new CreateTicketTierRequest
            {
                Name = longName,
                Price = 150.00m,
                MaxQuantity = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateTicketTierAsync(eventId, request, userId));
        }

        #endregion
    }
}
