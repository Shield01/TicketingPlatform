using Microsoft.Extensions.Logging;
using Moq;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Modules.TicketService.Services;
using Shared.Kernel.Interfaces;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Integration tests for TicketTierService and EventMinimumPriceService interaction.
    /// </summary>
    public class TicketTierMinimumPriceIntegrationTests
    {
        private readonly Mock<ITicketTierRepository> _mockTicketTierRepository;
        private readonly Mock<IEventMinimumPriceService> _mockEventMinimumPriceService;
        private readonly Mock<ILogger<TicketTierService>> _mockLogger;
        private readonly TicketTierService _service;

        public TicketTierMinimumPriceIntegrationTests()
        {
            _mockTicketTierRepository = new Mock<ITicketTierRepository>();
            _mockEventMinimumPriceService = new Mock<IEventMinimumPriceService>();
            _mockLogger = new Mock<ILogger<TicketTierService>>();

            _service = new TicketTierService(
                _mockTicketTierRepository.Object,
                _mockEventMinimumPriceService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateTicketTierAsync_AvailableTier_UpdatesEventMinimumPrice()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "Early Bird",
                Description = "Early bird special",
                Price = 50m,
                Currency = "USD",
                MaxQuantity = 100,
                IsAvailable = true
            };

            var createdTier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                MaxQuantity = request.MaxQuantity,
                IsAvailable = request.IsAvailable,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockTicketTierRepository.Setup(x => x.TierNameExistsForEventAsync(eventId, request.Name, null))
                .ReturnsAsync(false);
            _mockTicketTierRepository.Setup(x => x.CreateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync(createdTier);
            _mockEventMinimumPriceService.Setup(x => x.UpdateMinimumPriceIfLowerAsync(eventId, request.Price))
                .ReturnsAsync(request.Price);

            // Act
            var result = await _service.CreateTicketTierAsync(eventId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdTier.Id, result.Id);
            _mockEventMinimumPriceService.Verify(
                x => x.UpdateMinimumPriceIfLowerAsync(eventId, request.Price), 
                Times.Once);
        }

        [Fact]
        public async Task CreateTicketTierAsync_UnavailableTier_DoesNotUpdateEventMinimumPrice()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Description = "VIP tickets",
                Price = 200m,
                Currency = "USD",
                MaxQuantity = 50,
                IsAvailable = false // Not available
            };

            var createdTier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                MaxQuantity = request.MaxQuantity,
                IsAvailable = request.IsAvailable,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockTicketTierRepository.Setup(x => x.TierNameExistsForEventAsync(eventId, request.Name, null))
                .ReturnsAsync(false);
            _mockTicketTierRepository.Setup(x => x.CreateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync(createdTier);

            // Act
            var result = await _service.CreateTicketTierAsync(eventId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdTier.Id, result.Id);
            _mockEventMinimumPriceService.Verify(
                x => x.UpdateMinimumPriceIfLowerAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), 
                Times.Never);
        }

        [Fact]
        public async Task UpdateTicketTierAsync_RecalculatesEventMinimumPrice()
        {
            // Arrange
            var tierId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingTier = new TicketTier
            {
                Id = tierId,
                EventId = eventId,
                Name = "Regular",
                Price = 100m,
                Currency = "USD",
                MaxQuantity = 500,
                SoldQuantity = 100,
                IsAvailable = true,
                IsActive = true
            };

            var request = new CreateTicketTierRequest
            {
                Name = "Regular",
                Description = "Updated description",
                Price = 90m, // Price changed
                Currency = "USD",
                MaxQuantity = 500,
                IsAvailable = true
            };

            _mockTicketTierRepository.Setup(x => x.GetTicketTierByIdAsync(tierId))
                .ReturnsAsync(existingTier);
            _mockTicketTierRepository.Setup(x => x.TierNameExistsForEventAsync(eventId, request.Name, tierId))
                .ReturnsAsync(false);
            _mockTicketTierRepository.Setup(x => x.UpdateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync(existingTier);
            _mockEventMinimumPriceService.Setup(x => x.RecalculateAndUpdateMinimumPriceAsync(eventId))
                .ReturnsAsync(90m);

            // Act
            var result = await _service.UpdateTicketTierAsync(tierId, request, userId);

            // Assert
            Assert.NotNull(result);
            _mockEventMinimumPriceService.Verify(
                x => x.RecalculateAndUpdateMinimumPriceAsync(eventId), 
                Times.Once);
        }

        [Fact]
        public async Task DeleteTicketTierAsync_RecalculatesEventMinimumPrice()
        {
            // Arrange
            var tierId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingTier = new TicketTier
            {
                Id = tierId,
                EventId = eventId,
                Name = "Early Bird",
                Price = 50m,
                Currency = "USD",
                MaxQuantity = 100,
                SoldQuantity = 50,
                IsAvailable = true,
                IsActive = true
            };

            _mockTicketTierRepository.Setup(x => x.GetTicketTierByIdAsync(tierId))
                .ReturnsAsync(existingTier);
            _mockTicketTierRepository.Setup(x => x.DeleteTicketTierAsync(tierId))
                .ReturnsAsync(true);
            _mockEventMinimumPriceService.Setup(x => x.RecalculateAndUpdateMinimumPriceAsync(eventId))
                .ReturnsAsync(100m);

            // Act
            var result = await _service.DeleteTicketTierAsync(tierId, userId);

            // Assert
            Assert.True(result);
            _mockEventMinimumPriceService.Verify(
                x => x.RecalculateAndUpdateMinimumPriceAsync(eventId), 
                Times.Once);
        }

        [Fact]
        public async Task CreateTicketTierAsync_MinimumPriceServiceFailure_TierStillCreated()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "Regular",
                Description = "Regular tickets",
                Price = 100m,
                Currency = "USD",
                MaxQuantity = 500,
                IsAvailable = true
            };

            var createdTier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = request.Name,
                Price = request.Price,
                Currency = request.Currency,
                MaxQuantity = request.MaxQuantity,
                IsAvailable = request.IsAvailable,
                IsActive = true
            };

            _mockTicketTierRepository.Setup(x => x.TierNameExistsForEventAsync(eventId, request.Name, null))
                .ReturnsAsync(false);
            _mockTicketTierRepository.Setup(x => x.CreateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync(createdTier);
            _mockEventMinimumPriceService.Setup(x => x.UpdateMinimumPriceIfLowerAsync(eventId, request.Price))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.CreateTicketTierAsync(eventId, request, userId);

            // Assert - Tier should still be created even if minimum price update fails
            Assert.NotNull(result);
            Assert.Equal(createdTier.Id, result.Id);
            _mockTicketTierRepository.Verify(x => x.CreateTicketTierAsync(It.IsAny<TicketTier>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTicketTierAsync_MinimumPriceServiceFailure_TierStillUpdated()
        {
            // Arrange
            var tierId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingTier = new TicketTier
            {
                Id = tierId,
                EventId = eventId,
                Name = "Regular",
                Price = 100m,
                Currency = "USD",
                MaxQuantity = 500,
                SoldQuantity = 50,
                IsAvailable = true,
                IsActive = true
            };

            var request = new CreateTicketTierRequest
            {
                Name = "Regular",
                Price = 95m,
                Currency = "USD",
                MaxQuantity = 500,
                IsAvailable = true
            };

            _mockTicketTierRepository.Setup(x => x.GetTicketTierByIdAsync(tierId))
                .ReturnsAsync(existingTier);
            _mockTicketTierRepository.Setup(x => x.TierNameExistsForEventAsync(eventId, request.Name, tierId))
                .ReturnsAsync(false);
            _mockTicketTierRepository.Setup(x => x.UpdateTicketTierAsync(It.IsAny<TicketTier>()))
                .ReturnsAsync(existingTier);
            _mockEventMinimumPriceService.Setup(x => x.RecalculateAndUpdateMinimumPriceAsync(eventId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UpdateTicketTierAsync(tierId, request, userId);

            // Assert - Tier should still be updated even if minimum price recalculation fails
            Assert.NotNull(result);
            _mockTicketTierRepository.Verify(x => x.UpdateTicketTierAsync(It.IsAny<TicketTier>()), Times.Once);
        }
    }
}

