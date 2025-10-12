using Microsoft.Extensions.Logging;
using Modules.EventService.DTOs;
using Modules.EventService.Models;
using Modules.EventService.Repositories;
using Modules.EventService.Services;
using Modules.TeamService.Services;
using Modules.TicketService.Services;
using Modules.UserService.Services;
using Moq;
using Xunit;
using EventService = Modules.EventService.Services.EventService;

namespace Tests.EventService.Tests
{
    /// <summary>
    /// Unit tests for the EventService class.
    /// </summary>
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<ILogger<Modules.EventService.Services.EventService>> _mockLogger;
        private readonly Modules.EventService.Services.EventService _eventService;
        private readonly Mock<ITeamService> _mockTeamService;
        private readonly Mock<ITicketTierService> _mockTicketTierService;

        public EventServiceTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockLogger = new Mock<ILogger<Modules.EventService.Services.EventService>>();
            _mockTeamService = new Mock<ITeamService>();
            _mockTicketTierService = new Mock<ITicketTierService>();
            
            // Setup default mock behavior for ticket tiers (return empty list)
            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Modules.TicketService.DTOs.TicketTierResponse>());
            
            _eventService = new Modules.EventService.Services.EventService(_mockEventRepository.Object, _mockTeamService.Object, _mockTicketTierService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateEventAsync_ValidRequest_ReturnsEventResponse()
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location",
                Category = "Test Category",
                ImageURL = "https://example.com/image.jpg",
                IsPublic = true
            };

            var organizerId = Guid.NewGuid();
            var expectedEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                Category = request.Category,
                ImageURL = request.ImageURL,
                IsPublic = request.IsPublic,
                Status = "Draft",
                OrganizerId = organizerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockEventRepository.Setup(r => r.CreateEventAsync(It.IsAny<Event>()))
                .ReturnsAsync(expectedEvent);

            // Act
            var result = await _eventService.CreateEventAsync(request, organizerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEvent.Id, result.Id);
            Assert.Equal(expectedEvent.Title, result.Title);
            Assert.Equal(expectedEvent.Description, result.Description);
            Assert.Equal(expectedEvent.StartDate, result.StartDate);
            Assert.Equal(expectedEvent.EndDate, result.EndDate);
            Assert.Equal(expectedEvent.Location, result.Location);
            Assert.Equal(expectedEvent.Category, result.Category);
            Assert.Equal(expectedEvent.ImageURL, result.ImageURL);
            Assert.Equal(expectedEvent.IsPublic, result.IsPublic);
            Assert.Equal(expectedEvent.Status, result.Status);
            Assert.Equal(expectedEvent.OrganizerId, result.OrganizerId);

            _mockEventRepository.Verify(r => r.CreateEventAsync(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task GetFilteredPublicEventsAsync_WithTicketTiers_CalculatesMinimumPrice()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    Title = "Test Event",
                    Description = "Test Description",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Test Location",
                    Category = "Test Category",
                    ImageURL = "https://example.com/image.jpg",
                    IsPublic = true,
                    IsPublished = true,
                    Status = "Published",
                    OrganizerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            var ticketTiers = new List<Modules.TicketService.DTOs.TicketTierResponse>
            {
                new Modules.TicketService.DTOs.TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "VIP",
                    Price = 100.00m,
                    Currency = "USD",
                    MaxQuantity = 10,
                    SoldQuantity = 0,
                    IsAvailable = true
                },
                new Modules.TicketService.DTOs.TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 50.00m,
                    Currency = "USD",
                    MaxQuantity = 100,
                    SoldQuantity = 0,
                    IsAvailable = true
                },
                new Modules.TicketService.DTOs.TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 25.00m,
                    Currency = "USD",
                    MaxQuantity = 50,
                    SoldQuantity = 50, // Sold out
                    IsAvailable = false
                }
            };

            var filter = new EventFilterRequest { Page = 1, PageSize = 10 };

            _mockEventRepository.Setup(r => r.GetFilteredPublicEventsAsync(filter))
                .ReturnsAsync((events, 1));

            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(eventId))
                .ReturnsAsync(ticketTiers);

            // Act
            var result = await _eventService.GetFilteredPublicEventsAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Events);
            
            var eventView = result.Events.First();
            Assert.Equal(50.00m, eventView.MinimumTicketPrice); // Should be Regular price since Early Bird is sold out
            Assert.Equal("USD", eventView.MinimumTicketPriceCurrency);
            Assert.Equal("https://example.com/image.jpg", eventView.ImageURL);
        }

        [Fact]
        public async Task GetFilteredPublicEventsAsync_NoAvailableTickets_NullMinimumPrice()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    Title = "Test Event",
                    Description = "Test Description",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Test Location",
                    Category = "Test Category",
                    ImageURL = "https://example.com/image.jpg",
                    IsPublic = true,
                    IsPublished = true,
                    Status = "Published",
                    OrganizerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            var ticketTiers = new List<Modules.TicketService.DTOs.TicketTierResponse>
            {
                new Modules.TicketService.DTOs.TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 50.00m,
                    Currency = "USD",
                    MaxQuantity = 100,
                    SoldQuantity = 100, // Sold out
                    IsAvailable = false
                }
            };

            var filter = new EventFilterRequest { Page = 1, PageSize = 10 };

            _mockEventRepository.Setup(r => r.GetFilteredPublicEventsAsync(filter))
                .ReturnsAsync((events, 1));

            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(eventId))
                .ReturnsAsync(ticketTiers);

            // Act
            var result = await _eventService.GetFilteredPublicEventsAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Events);
            
            var eventView = result.Events.First();
            Assert.Null(eventView.MinimumTicketPrice); // No available tickets
            Assert.Null(eventView.MinimumTicketPriceCurrency);
        }
    }
}
