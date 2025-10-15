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
            Assert.Equal(expectedEvent.IsPublic, result.IsPublic);
            Assert.Equal(expectedEvent.Status, result.Status);
            Assert.Equal(expectedEvent.OrganizerId, result.OrganizerId);

            _mockEventRepository.Verify(r => r.CreateEventAsync(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task CreateEventAsync_NullRequest_ThrowsArgumentException()
        {
            // Arrange
            CreateEventRequest? request = null;
            var organizerId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventAsync(request!, organizerId));
            Assert.Equal("Request cannot be null.", exception.Message);
        }

        [Theory]
        [InlineData("", "Title is required.")]
        [InlineData(null, "Title is required.")]
        [InlineData("   ", "Title is required.")]
        public async Task CreateEventAsync_InvalidTitle_ThrowsArgumentException(string? title, string expectedError)
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = title,
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            var organizerId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventAsync(request, organizerId));
            Assert.Equal(expectedError, exception.Message);
        }

        [Theory]
        [InlineData("", "Description is required.")]
        [InlineData(null, "Description is required.")]
        [InlineData("   ", "Description is required.")]
        public async Task CreateEventAsync_InvalidDescription_ThrowsArgumentException(string? description, string expectedError)
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = description,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            var organizerId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventAsync(request, organizerId));
            Assert.Equal(expectedError, exception.Message);
        }

        [Theory]
        [InlineData("", "Location is required.")]
        [InlineData(null, "Location is required.")]
        [InlineData("   ", "Location is required.")]
        public async Task CreateEventAsync_InvalidLocation_ThrowsArgumentException(string? location, string expectedError)
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = location
            };

            var organizerId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventAsync(request, organizerId));
            Assert.Equal(expectedError, exception.Message);
        }

        [Fact]
        public async Task CreateEventAsync_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1),
                Location = "Test Location"
            };

            var organizerId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventAsync(request, organizerId));
            Assert.Equal("End date must be after start date.", exception.Message);
        }

        [Fact]
        public async Task CreateEventAsync_StartDateInPast_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                Location = "Test Location"
            };

            var organizerId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _eventService.CreateEventAsync(request, organizerId));
            Assert.Equal("Event cannot be created in the past.", exception.Message);
        }

        [Fact]
        public async Task GetEventByIdAsync_ExistingEvent_ReturnsEventResponse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var organizerId = Guid.NewGuid();
            var expectedEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location",
                Category = "Test Category",
                IsPublic = true,
                Status = "Draft",
                OrganizerId = organizerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Organizer = new Modules.UserService.Models.User
                {
                    Id = organizerId,
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(expectedEvent);

            // Act
            var result = await _eventService.GetEventByIdAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEvent.Id, result.Id);
            Assert.Equal(expectedEvent.Title, result.Title);
            Assert.Equal("John Doe", result.OrganizerName);

            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetEventByIdAsync_NonExistingEvent_ReturnsNull()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _eventService.GetEventByIdAsync(eventId);

            // Assert
            Assert.Null(result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetEventsByOrganizerAsync_ReturnsEventResponses()
        {
            // Arrange
            var organizerId = Guid.NewGuid();
            var events = new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 1",
                    Description = "Description 1",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Location 1",
                    OrganizerId = organizerId,
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 2",
                    Description = "Description 2",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    Location = "Location 2",
                    OrganizerId = organizerId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockEventRepository.Setup(r => r.GetEventsByOrganizerAsync(organizerId))
                .ReturnsAsync(events);

            // Act
            var result = await _eventService.GetEventsByOrganizerAsync(organizerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, eventResponse => Assert.Equal(organizerId, eventResponse.OrganizerId));

            _mockEventRepository.Verify(r => r.GetEventsByOrganizerAsync(organizerId), Times.Once);
        }

        [Fact]
        public async Task GetPublicEventsAsync_ReturnsPublicEventResponses()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Public Event 1",
                    Description = "Description 1",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Location 1",
                    IsPublic = true,
                    Status = "Published",
                    OrganizerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Public Event 2",
                    Description = "Description 2",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    Location = "Location 2",
                    IsPublic = true,
                    Status = "Published",
                    OrganizerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockEventRepository.Setup(r => r.GetPublicEventsAsync())
                .ReturnsAsync(events);

            // Act
            var result = await _eventService.GetPublicEventsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, eventResponse => Assert.True(eventResponse.IsPublic));

            _mockEventRepository.Verify(r => r.GetPublicEventsAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_ValidRequest_ReturnsUpdatedEventResponse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateEventRequest
            {
                Title = "Updated Event",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Updated Location",
                Category = "Updated Category",
                IsPublic = false
            };

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Original Event",
                Description = "Original Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Original Location",
                Category = "Original Category",
                IsPublic = true,
                Status = "Draft",
                OrganizerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var updatedEvent = new Event
            {
                Id = eventId,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                Category = request.Category,
                IsPublic = request.IsPublic,
                Status = "Draft",
                OrganizerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.UpdateEventAsync(It.IsAny<Event>()))
                .ReturnsAsync(updatedEvent);

            // Act
            var result = await _eventService.UpdateEventAsync(eventId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Location, result.Location);
            Assert.Equal(request.Category, result.Category);
            Assert.Equal(request.IsPublic, result.IsPublic);

            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_UserNotOrganizer_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organizerId = Guid.NewGuid(); // Different from userId
            var request = new UpdateEventRequest
            {
                Title = "Updated Event",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Updated Location"
            };

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Original Event",
                OrganizerId = organizerId,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _eventService.UpdateEventAsync(eventId, request, userId, false)); // Not admin
            Assert.Contains("not authorized", exception.Message);

            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteEventAsync_UserIsOrganizer_ReturnsTrue()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(-1), // Recent event, no tickets
                IsPublished = false,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.DeleteEventAsync(eventId))
                .ReturnsAsync(true);

            // Act
            var result = await _eventService.DeleteEventAsync(eventId, userId, false);

            // Assert
            Assert.True(result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Exactly(2)); // Called twice: auth check + ticket check
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.DeleteEventAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task DeleteEventAsync_UserNotOrganizer_ReturnsFalse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organizerId = Guid.NewGuid(); // Different from userId

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = organizerId,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                IsPublished = false,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _eventService.DeleteEventAsync(eventId, userId, false); // Not admin

            // Assert
            Assert.False(result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.DeleteEventAsync(eventId), Times.Never);
        }

        [Theory]
        [InlineData(null, false, "Request cannot be null.")]
        [InlineData("", false, "Title is required.")]
        [InlineData("   ", false, "Title is required.")]
        [InlineData("Valid Title", true, null)]
        public void ValidateCreateEventRequest_ReturnsExpectedResult(string? title, bool expectedIsValid, string? expectedErrorMessage)
        {
            // Arrange
            var request = title == null ? null : new CreateEventRequest
            {
                Title = title,
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            // Act
            var (isValid, errorMessage) = _eventService.ValidateCreateEventRequest(request!);

            // Assert
            Assert.Equal(expectedIsValid, isValid);
            Assert.Equal(expectedErrorMessage, errorMessage);
        }

        [Fact]
        public async Task GetPublicEventByIdAsync_ExistingPublishedEvent_ReturnsEventResponse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var organizerId = Guid.NewGuid();
            var expectedEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location",
                Category = "Test Category",
                IsPublic = true,
                IsPublished = true,
                Status = "Published",
                OrganizerId = organizerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Organizer = new Modules.UserService.Models.User
                {
                    Id = organizerId,
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _mockEventRepository.Setup(r => r.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync(expectedEvent);

            // Act
            var result = await _eventService.GetPublicEventByIdAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEvent.Id, result.Id);
            Assert.Equal(expectedEvent.Title, result.Title);
            Assert.True(result.IsPublished);
            Assert.Equal("John Doe", result.OrganizerName);

            _mockEventRepository.Verify(r => r.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetPublicEventByIdAsync_NonExistingEvent_ReturnsNull()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockEventRepository.Setup(r => r.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _eventService.GetPublicEventByIdAsync(eventId);

            // Assert
            Assert.Null(result);
            _mockEventRepository.Verify(r => r.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetPublicEventByIdAsync_UnpublishedEvent_ReturnsNull()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockEventRepository.Setup(r => r.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _eventService.GetPublicEventByIdAsync(eventId);

            // Assert
            Assert.Null(result);
            _mockEventRepository.Verify(r => r.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetFilteredPublicEventsAsync_ValidFilter_ReturnsPaginatedResponse()
        {
            // Arrange
            var filter = new EventFilterRequest
            {
                Status = "Published",
                Category = "Music",
                EventType = "upcoming",
                SearchKeyword = "concert",
                Location = "Concert Hall",
                Page = 1,
                PageSize = 10,
                SortBy = "startdate",
                SortDirection = "asc"
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Music Concert",
                    Description = "A great concert",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Concert Hall",
                    Category = "Music",
                    IsPublic = true,
                    IsPublished = true,
                    Status = "Published",
                    OrganizerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Organizer = new Modules.UserService.Models.User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "John",
                        LastName = "Doe"
                    }
                }
            };

            _mockEventRepository.Setup(r => r.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()))
                .ReturnsAsync((events, 1));

            // Act
            var result = await _eventService.GetFilteredPublicEventsAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Events);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.TotalPages);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);

            var eventView = result.Events.First();
            Assert.Equal("Music Concert", eventView.Title);
            Assert.True(eventView.IsUpcoming);
            Assert.True(eventView.DaysUntilEvent >= 0); // Should be 0 or more days until event

            _mockEventRepository.Verify(r => r.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetFilteredPublicEventsAsync_WithPagination_ReturnsCorrectPagination()
        {
            // Arrange
            var filter = new EventFilterRequest
            {
                Page = 2,
                PageSize = 5
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 1",
                    Description = "Description 1",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Location 1",
                    IsPublic = true,
                    IsPublished = true,
                    Status = "Published",
                    OrganizerId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockEventRepository.Setup(r => r.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()))
                .ReturnsAsync((events, 12)); // Total 12 events, page 2 of 5 per page

            // Act
            var result = await _eventService.GetFilteredPublicEventsAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Events);
            Assert.Equal(12, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(3, result.TotalPages); // 12 total / 5 per page = 3 pages
            Assert.True(result.HasNextPage);
            Assert.True(result.HasPreviousPage);

            _mockEventRepository.Verify(r => r.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetFilteredPublicEventsAsync_NormalizesFilterParameters()
        {
            // Arrange
            var filter = new EventFilterRequest
            {
                Page = -1, // Invalid page
                PageSize = 200, // Invalid page size
                SortDirection = "invalid" // Invalid sort direction
            };

            var events = new List<Event>();
            _mockEventRepository.Setup(r => r.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()))
                .ReturnsAsync((events, 0));

            // Act
            var result = await _eventService.GetFilteredPublicEventsAsync(filter);

            // Assert
            Assert.NotNull(result);
            // The service should normalize the filter parameters
            _mockEventRepository.Verify(r => r.GetFilteredPublicEventsAsync(It.Is<EventFilterRequest>(f => 
                f.Page == 1 && f.PageSize == 100 && f.SortDirection == "asc")), Times.Once);
        }

        #region Update Event Tests

        [Fact]
        public async Task UpdateEventAsync_ValidRequest_AsOrganizer_ReturnsUpdatedEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateEventRequest
            {
                Title = "Updated Event",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(3),
                Location = "Updated Location",
                Category = "Updated Category",
                IsPublic = false,
                Status = "Published"
            };

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Original Event",
                Description = "Original Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Original Location",
                Category = "Original Category",
                IsPublic = true,
                Status = "Draft",
                OrganizerId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedEvent = new Event
            {
                Id = eventId,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                Category = request.Category,
                IsPublic = request.IsPublic,
                Status = request.Status,
                OrganizerId = userId,
                CreatedAt = existingEvent.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.UpdateEventAsync(It.IsAny<Event>()))
                .ReturnsAsync(updatedEvent);

            // Act
            var result = await _eventService.UpdateEventAsync(eventId, request, userId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.StartDate, result.StartDate);
            Assert.Equal(request.EndDate, result.EndDate);
            Assert.Equal(request.Location, result.Location);
            Assert.Equal(request.Category, result.Category);
            Assert.Equal(request.IsPublic, result.IsPublic);
            Assert.Equal(request.Status, result.Status);

            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_ValidRequest_AsAdmin_ReturnsUpdatedEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organizerId = Guid.NewGuid(); // Different from userId
            var request = new UpdateEventRequest
            {
                Title = "Admin Updated Event",
                Description = "Admin Updated Description",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(3),
                Location = "Admin Updated Location",
                Category = "Admin Category",
                IsPublic = true,
                Status = "Published"
            };

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Original Event",
                Description = "Original Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Original Location",
                OrganizerId = organizerId, // Different organizer
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedEvent = new Event
            {
                Id = eventId,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                Category = request.Category,
                IsPublic = request.IsPublic,
                Status = request.Status,
                OrganizerId = organizerId,
                CreatedAt = existingEvent.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(false); // User is not organizer
            _mockEventRepository.Setup(r => r.UpdateEventAsync(It.IsAny<Event>()))
                .ReturnsAsync(updatedEvent);

            // Act
            var result = await _eventService.UpdateEventAsync(eventId, request, userId, true); // Admin override

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);

            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_EventNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateEventRequest
            {
                Title = "Updated Event",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Updated Location"
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _eventService.UpdateEventAsync(eventId, request, userId, false));

            Assert.Contains("not found", exception.Message);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_NotOrganizerNotAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organizerId = Guid.NewGuid(); // Different from userId
            var request = new UpdateEventRequest
            {
                Title = "Updated Event",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Updated Location"
            };

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Original Event",
                OrganizerId = organizerId,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _eventService.UpdateEventAsync(eventId, request, userId, false)); // Not admin

            Assert.Contains("not authorized", exception.Message);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_InvalidDates_ThrowsArgumentException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateEventRequest
            {
                Title = "Updated Event",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1), // End date before start date
                Location = "Updated Location"
            };

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Original Event",
                OrganizerId = userId,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _eventService.UpdateEventAsync(eventId, request, userId, false));

            Assert.Contains("End date must be after start date", exception.Message);
        }

        #endregion

        #region Delete Event Tests

        [Fact]
        public async Task DeleteEventAsync_ValidRequest_AsOrganizer_NoTickets_ReturnsTrue()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(-1), // Recent event, no tickets
                IsPublished = false,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.DeleteEventAsync(eventId))
                .ReturnsAsync(true);

            // Act
            var result = await _eventService.DeleteEventAsync(eventId, userId, false);

            // Assert
            Assert.True(result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Exactly(2)); // Called twice: auth check + ticket check
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.DeleteEventAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task DeleteEventAsync_ValidRequest_AsAdmin_NoTickets_ReturnsTrue()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organizerId = Guid.NewGuid(); // Different from userId

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = organizerId, // Different organizer
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                IsPublished = false,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(false); // User is not organizer
            _mockEventRepository.Setup(r => r.DeleteEventAsync(eventId))
                .ReturnsAsync(true);

            // Act
            var result = await _eventService.DeleteEventAsync(eventId, userId, true); // Admin override

            // Assert
            Assert.True(result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Exactly(2)); // Called twice: auth check + ticket check
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.DeleteEventAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task DeleteEventAsync_EventNotFound_ReturnsFalse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _eventService.DeleteEventAsync(eventId, userId, false);

            // Assert
            Assert.False(result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _mockEventRepository.Verify(r => r.DeleteEventAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteEventAsync_NotOrganizerNotAdmin_ReturnsFalse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organizerId = Guid.NewGuid(); // Different from userId

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = organizerId,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                IsPublished = false,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _eventService.DeleteEventAsync(eventId, userId, false); // Not admin

            // Assert
            Assert.False(result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.DeleteEventAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteEventAsync_HasTickets_ThrowsInvalidOperationException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingEvent = new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-2), // Old event
                IsPublished = true, // Published event
                StartDate = DateTime.UtcNow.AddDays(1), // Future event
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockEventRepository.Setup(r => r.GetEventByIdAsync(eventId))
                .ReturnsAsync(existingEvent);
            _mockEventRepository.Setup(r => r.IsUserOrganizerAsync(eventId, userId))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _eventService.DeleteEventAsync(eventId, userId, false));

            Assert.Contains("tickets have been issued", exception.Message);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(eventId), Times.Exactly(2)); // Called twice: once for auth, once for ticket check
            _mockEventRepository.Verify(r => r.IsUserOrganizerAsync(eventId, userId), Times.Once);
            _mockEventRepository.Verify(r => r.DeleteEventAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region GetMyEventsAsync Tests

        [Fact]
        public async Task GetMyEventsAsync_ValidFilter_ReturnsPaginatedResponse()
        {
            // Arrange
            var organizerId = Guid.NewGuid();
            var filter = new EventFilterRequest
            {
                Page = 1,
                PageSize = 10,
                Status = "Draft,Published",
                SearchKeyword = "test",
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Event 1",
                    Description = "Test Description 1",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Test Location 1",
                    Status = "Draft",
                    OrganizerId = organizerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Event 2",
                    Description = "Test Description 2",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    Location = "Test Location 2",
                    Status = "Published",
                    OrganizerId = organizerId,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
                    IsActive = true
                }
            };

            _mockEventRepository.Setup(r => r.GetFilteredEventsByOrganizerAsync(organizerId, filter))
                .ReturnsAsync((events, 2));

            // Act
            var result = await _eventService.GetMyEventsAsync(organizerId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Events.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.TotalPages);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);

            // Verify events are returned in the correct order
            Assert.Equal("Test Event 1", result.Events.First().Title);
            Assert.Equal("Test Event 2", result.Events.Last().Title);

            _mockEventRepository.Verify(r => r.GetFilteredEventsByOrganizerAsync(organizerId, filter), Times.Once);
        }

        [Fact]
        public async Task GetMyEventsAsync_NoEvents_ReturnsEmptyResponse()
        {
            // Arrange
            var organizerId = Guid.NewGuid();
            var filter = new EventFilterRequest
            {
                Page = 1,
                PageSize = 10
            };

            _mockEventRepository.Setup(r => r.GetFilteredEventsByOrganizerAsync(organizerId, filter))
                .ReturnsAsync((new List<Event>(), 0));

            // Act
            var result = await _eventService.GetMyEventsAsync(organizerId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Events);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(0, result.TotalPages);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);

            _mockEventRepository.Verify(r => r.GetFilteredEventsByOrganizerAsync(organizerId, filter), Times.Once);
        }

        [Fact]
        public async Task GetMyEventsAsync_InvalidFilter_ThrowsArgumentException()
        {
            // Arrange
            var organizerId = Guid.NewGuid();
            var filter = new EventFilterRequest
            {
                Page = 0, // Invalid page
                PageSize = 10
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _eventService.GetMyEventsAsync(organizerId, filter));
            Assert.Contains("Page must be greater than 0", exception.Message);

            _mockEventRepository.Verify(r => r.GetFilteredEventsByOrganizerAsync(It.IsAny<Guid>(), It.IsAny<EventFilterRequest>()), Times.Never);
        }

        #endregion
    }
} 