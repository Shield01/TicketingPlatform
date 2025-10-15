using Microsoft.Extensions.Logging;
using Moq;
using Modules.EventService.Models;
using Modules.EventService.Repositories;
using Modules.EventService.Services;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Xunit;

namespace Tests.EventService.Tests
{
    /// <summary>
    /// Unit tests for EventMinimumPriceService.
    /// </summary>
    public class EventMinimumPriceServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<ITicketTierRepository> _mockTicketTierRepository;
        private readonly Mock<ILogger<EventMinimumPriceService>> _mockLogger;
        private readonly EventMinimumPriceService _service;

        public EventMinimumPriceServiceTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockTicketTierRepository = new Mock<ITicketTierRepository>();
            _mockLogger = new Mock<ILogger<EventMinimumPriceService>>();
            
            _service = new EventMinimumPriceService(
                _mockEventRepository.Object,
                _mockTicketTierRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_EventNotFound_ReturnsNull()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Null(result);
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.IsAny<Event>()), Times.Never);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_NoAvailableTiers_SetsMinimumPriceToNull()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = 100m
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);
            _mockTicketTierRepository.Setup(x => x.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(new List<TicketTier>());

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Null(result);
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == null)), Times.Once);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_MultipleAvailableTiers_SetsMinimumToLowestPrice()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = null
            };

            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "VIP",
                    Price = 200m,
                    MaxQuantity = 100,
                    SoldQuantity = 50,
                    IsAvailable = true,
                    IsActive = true
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 100m,
                    MaxQuantity = 500,
                    SoldQuantity = 100,
                    IsAvailable = true,
                    IsActive = true
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 75m,
                    MaxQuantity = 50,
                    SoldQuantity = 10,
                    IsAvailable = true,
                    IsActive = true
                }
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);
            _mockTicketTierRepository.Setup(x => x.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(tiers);

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Equal(75m, result);
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 75m)), Times.Once);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_SoldOutTier_ExcludesFromCalculation()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = 50m
            };

            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 50m,
                    MaxQuantity = 50,
                    SoldQuantity = 50, // Sold out
                    IsAvailable = true,
                    IsActive = true
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 100m,
                    MaxQuantity = 500,
                    SoldQuantity = 100,
                    IsAvailable = true,
                    IsActive = true
                }
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);
            _mockTicketTierRepository.Setup(x => x.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(tiers);

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Equal(100m, result); // Should be Regular tier price, not sold-out Early Bird
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 100m)), Times.Once);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_UnavailableTier_ExcludesFromCalculation()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = 50m
            };

            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 50m,
                    MaxQuantity = 50,
                    SoldQuantity = 10,
                    IsAvailable = false, // Not available
                    IsActive = true
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 100m,
                    MaxQuantity = 500,
                    SoldQuantity = 100,
                    IsAvailable = true,
                    IsActive = true
                }
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);
            _mockTicketTierRepository.Setup(x => x.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(tiers);

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Equal(100m, result); // Should exclude unavailable tier
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 100m)), Times.Once);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_InactiveTier_ExcludesFromCalculation()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = 50m
            };

            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 50m,
                    MaxQuantity = 50,
                    SoldQuantity = 10,
                    IsAvailable = true,
                    IsActive = false // Inactive
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 100m,
                    MaxQuantity = 500,
                    SoldQuantity = 100,
                    IsAvailable = true,
                    IsActive = true
                }
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);
            _mockTicketTierRepository.Setup(x => x.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(tiers);

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Equal(100m, result); // Should exclude inactive tier
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 100m)), Times.Once);
        }

        [Fact]
        public async Task UpdateMinimumPriceIfLowerAsync_EventNotFound_ReturnsNull()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _service.UpdateMinimumPriceIfLowerAsync(eventId, 50m, "USD");

            // Assert
            Assert.Null(result);
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.IsAny<Event>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMinimumPriceIfLowerAsync_NoExistingMinimum_SetsNewPrice()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = null
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);

            // Act
            var result = await _service.UpdateMinimumPriceIfLowerAsync(eventId, 50m, "USD");

            // Assert
            Assert.Equal(50m, result);
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 50m && e.MinimumPriceCurrency == "USD")), Times.Once);
        }

        [Fact]
        public async Task UpdateMinimumPriceIfLowerAsync_NewPriceLower_UpdatesPrice()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = 100m,
                MinimumPriceCurrency = "USD"
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);

            // Act
            var result = await _service.UpdateMinimumPriceIfLowerAsync(eventId, 75m, "NGN");

            // Assert
            Assert.Equal(75m, result);
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 75m && e.MinimumPriceCurrency == "NGN")), Times.Once);
        }

        [Fact]
        public async Task UpdateMinimumPriceIfLowerAsync_NewPriceHigher_DoesNotUpdate()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = 50m,
                MinimumPriceCurrency = "USD"
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);

            // Act
            var result = await _service.UpdateMinimumPriceIfLowerAsync(eventId, 100m, "USD");

            // Assert
            Assert.Equal(50m, result); // Should remain at original price
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.IsAny<Event>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMinimumPriceIfLowerAsync_NewPriceEqual_DoesNotUpdate()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = 50m,
                MinimumPriceCurrency = "USD"
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);

            // Act
            var result = await _service.UpdateMinimumPriceIfLowerAsync(eventId, 50m, "USD");

            // Assert
            Assert.Equal(50m, result); // Should remain at original price
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.IsAny<Event>()), Times.Never);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_SaleNotStartedYet_ExcludesFromCalculation()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = null
            };

            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 50m,
                    MaxQuantity = 50,
                    SoldQuantity = 0,
                    IsAvailable = true,
                    IsActive = true,
                    SaleStartDate = DateTime.UtcNow.AddDays(7) // Starts in the future
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 100m,
                    MaxQuantity = 500,
                    SoldQuantity = 0,
                    IsAvailable = true,
                    IsActive = true
                }
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);
            _mockTicketTierRepository.Setup(x => x.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(tiers);

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Equal(100m, result); // Should exclude tier with future sale start date
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 100m)), Times.Once);
        }

        [Fact]
        public async Task RecalculateAndUpdateMinimumPriceAsync_SaleEnded_ExcludesFromCalculation()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                Title = "Test Event",
                MinimumPrice = null
            };

            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 50m,
                    MaxQuantity = 50,
                    SoldQuantity = 10,
                    IsAvailable = true,
                    IsActive = true,
                    SaleEndDate = DateTime.UtcNow.AddDays(-1) // Ended yesterday
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 100m,
                    MaxQuantity = 500,
                    SoldQuantity = 0,
                    IsAvailable = true,
                    IsActive = true
                }
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(@event);
            _mockTicketTierRepository.Setup(x => x.GetTicketTiersByEventIdAsync(eventId))
                .ReturnsAsync(tiers);

            // Act
            var result = await _service.RecalculateAndUpdateMinimumPriceAsync(eventId);

            // Assert
            Assert.Equal(100m, result); // Should exclude tier with past sale end date
            _mockEventRepository.Verify(x => x.UpdateEventAsync(It.Is<Event>(e => 
                e.Id == eventId && e.MinimumPrice == 100m)), Times.Once);
        }
    }
}

