using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.EventService.Controllers;
using Modules.EventService.DTOs;
using Modules.EventService.Services;
using System.Security.Claims;
using Xunit;

namespace Tests.EventService.Tests
{
    /// <summary>
    /// Unit tests for the EventController class.
    /// </summary>
    public class EventControllerTests
    {
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<ILogger<EventController>> _mockLogger;
        private readonly EventController _controller;

        public EventControllerTests()
        {
            _mockEventService = new Mock<IEventService>();
            _mockLogger = new Mock<ILogger<EventController>>();
            _controller = new EventController(_mockLogger.Object, _mockEventService.Object);
        }

        [Fact]
        public async Task CreateEvent_ValidRequest_ReturnsCreatedResult()
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

            var userId = Guid.NewGuid();
            var expectedResponse = new EventResponse
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
                OrganizerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrganizerName = "John Doe"
            };

            _mockEventService.Setup(s => s.CreateEventAsync(request, userId))
                .ReturnsAsync(expectedResponse);

            // Set up user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.CreateEvent(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<EventResponse>(createdResult.Value);
            Assert.Equal(expectedResponse.Id, response.Id);
            Assert.Equal(expectedResponse.Title, response.Title);
            Assert.Equal(expectedResponse.OrganizerId, response.OrganizerId);

            _mockEventService.Verify(s => s.CreateEventAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task CreateEvent_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            // Set up empty user claims
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.CreateEvent(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            _mockEventService.Verify(s => s.CreateEventAsync(It.IsAny<CreateEventRequest>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateEvent_InvalidUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            // Set up invalid user ID claim
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.CreateEvent(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            _mockEventService.Verify(s => s.CreateEventAsync(It.IsAny<CreateEventRequest>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateEvent_ArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "", // Invalid title
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            var userId = Guid.NewGuid();
            var errorMessage = "Title is required.";

            _mockEventService.Setup(s => s.CreateEventAsync(request, userId))
                .ThrowsAsync(new ArgumentException(errorMessage));

            // Set up user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.CreateEvent(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            var errorProperty = badRequestResult.Value.GetType().GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(badRequestResult.Value);
            Assert.Equal(errorMessage, errorValue);

            _mockEventService.Verify(s => s.CreateEventAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task CreateEvent_GeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new CreateEventRequest
            {
                Title = "Test Event",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            var userId = Guid.NewGuid();

            _mockEventService.Setup(s => s.CreateEventAsync(request, userId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Set up user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.CreateEvent(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.NotNull(statusCodeResult.Value);
            var errorProperty = statusCodeResult.Value.GetType().GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(statusCodeResult.Value);
            Assert.Equal("An error occurred while creating the event.", errorValue);

            _mockEventService.Verify(s => s.CreateEventAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task GetEvent_ExistingEvent_ReturnsOkResult()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var expectedResponse = new EventResponse
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
                OrganizerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrganizerName = "John Doe"
            };

            _mockEventService.Setup(s => s.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPublicEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EventResponse>(okResult.Value);
            Assert.Equal(expectedResponse.Id, response.Id);
            Assert.Equal(expectedResponse.Title, response.Title);

            _mockEventService.Verify(s => s.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetEvent_NonExistingEvent_ReturnsNotFound()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockEventService.Setup(s => s.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync((EventResponse?)null);

            // Act
            var result = await _controller.GetPublicEvent(eventId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _mockEventService.Verify(s => s.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetPublicEvents_ReturnsOkResult()
        {
            // Arrange
            var events = new List<EventViewDTO>
            {
                new EventViewDTO
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 1",
                    Description = "Description 1",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Location 1",
                    Category = "Music",
                    OrganizerName = "John Doe",
                    CreatedAt = DateTime.UtcNow,
                    IsUpcoming = true,
                    DaysUntilEvent = 1
                },
                new EventViewDTO
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 2",
                    Description = "Description 2",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    Location = "Location 2",
                    Category = "Sports",
                    OrganizerName = "Jane Smith",
                    CreatedAt = DateTime.UtcNow,
                    IsUpcoming = true,
                    DaysUntilEvent = 3
                }
            };

            var expectedResponse = new PaginatedEventViewResponse
            {
                Events = events,
                TotalCount = 2,
                Page = 1,
                PageSize = 10,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };

            _mockEventService.Setup(s => s.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPublicEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedEventViewResponse>(okResult.Value);
            Assert.Equal(2, response.Events.Count);
            Assert.Equal(2, response.TotalCount);
            Assert.Equal(1, response.Page);
            Assert.Equal(10, response.PageSize);
            Assert.Equal(1, response.TotalPages);
            Assert.False(response.HasNextPage);
            Assert.False(response.HasPreviousPage);

            _mockEventService.Verify(s => s.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetPublicEvents_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var events = new List<EventViewDTO>
            {
                new EventViewDTO
                {
                    Id = Guid.NewGuid(),
                    Title = "Music Event",
                    Description = "A great music event",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Location = "Concert Hall",
                    Category = "Music",
                    OrganizerName = "John Doe",
                    CreatedAt = DateTime.UtcNow,
                    IsUpcoming = true,
                    DaysUntilEvent = 1
                }
            };

            var expectedResponse = new PaginatedEventViewResponse
            {
                Events = events,
                TotalCount = 1,
                Page = 1,
                PageSize = 10,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };

            _mockEventService.Setup(s => s.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPublicEvents(
                category: "Music",
                eventType: "upcoming",
                searchKeyword: "music",
                page: 1,
                pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedEventViewResponse>(okResult.Value);
            Assert.Single(response.Events);
            Assert.Equal("Music Event", response.Events.First().Title);

            _mockEventService.Verify(s => s.GetFilteredPublicEventsAsync(It.IsAny<EventFilterRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetPublicEvent_ExistingPublishedEvent_ReturnsOkResult()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var expectedResponse = new EventResponse
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
                OrganizerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrganizerName = "John Doe"
            };

            _mockEventService.Setup(s => s.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetPublicEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<EventResponse>(okResult.Value);
            Assert.Equal(expectedResponse.Id, response.Id);
            Assert.Equal(expectedResponse.Title, response.Title);
            Assert.True(response.IsPublished);

            _mockEventService.Verify(s => s.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetPublicEvent_NonExistingEvent_ReturnsNotFound()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockEventService.Setup(s => s.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync((EventResponse?)null);

            // Act
            var result = await _controller.GetPublicEvent(eventId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            var errorProperty = notFoundResult.Value.GetType().GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(notFoundResult.Value);
            Assert.Equal("Event not found or not published.", errorValue);

            _mockEventService.Verify(s => s.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetPublicEvent_UnpublishedEvent_ReturnsNotFound()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockEventService.Setup(s => s.GetPublicEventByIdAsync(eventId))
                .ReturnsAsync((EventResponse?)null);

            // Act
            var result = await _controller.GetPublicEvent(eventId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            var errorProperty = notFoundResult.Value.GetType().GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorValue = errorProperty.GetValue(notFoundResult.Value);
            Assert.Equal("Event not found or not published.", errorValue);

            _mockEventService.Verify(s => s.GetPublicEventByIdAsync(eventId), Times.Once);
        }

        #region GetMyEvents Tests

        [Fact]
        public async Task GetMyEvents_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedResponse = new PaginatedEventsResponse
            {
                Events = new List<EventResponse>
                {
                    new EventResponse
                    {
                        Id = Guid.NewGuid(),
                        Title = "My Test Event",
                        Description = "Test Description",
                        StartDate = DateTime.UtcNow.AddDays(1),
                        EndDate = DateTime.UtcNow.AddDays(2),
                        Location = "Test Location",
                        Status = "Draft",
                        OrganizerId = userId,
                        CreatedAt = DateTime.UtcNow,
                        OrganizerName = "John Doe"
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 20,
                TotalPages = 1,
                HasNextPage = false,
                HasPreviousPage = false
            };

            _mockEventService.Setup(s => s.GetMyEventsAsync(userId, It.IsAny<EventFilterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Set up user claims with extension method support
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetMyEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedEventsResponse>(okResult.Value);
            Assert.Equal(1, response.Events.Count);
            Assert.Equal("My Test Event", response.Events.First().Title);
            Assert.Equal(userId, response.Events.First().OrganizerId);

            _mockEventService.Verify(s => s.GetMyEventsAsync(userId, It.IsAny<EventFilterRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetMyEvents_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange - No user context set

            // Act
            var result = await _controller.GetMyEvents();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated.", unauthorizedResult.Value);

            _mockEventService.Verify(s => s.GetMyEventsAsync(It.IsAny<Guid>(), It.IsAny<EventFilterRequest>()), Times.Never);
        }

        [Fact]
        public async Task GetMyEvents_WithFilters_PassesFiltersCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedResponse = new PaginatedEventsResponse
            {
                Events = new List<EventResponse>(),
                TotalCount = 0,
                Page = 2,
                PageSize = 5,
                TotalPages = 0,
                HasNextPage = false,
                HasPreviousPage = true
            };

            _mockEventService.Setup(s => s.GetMyEventsAsync(userId, It.IsAny<EventFilterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Set up user claims
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetMyEvents(
                status: "Draft,Published",
                category: "Technology",
                eventType: "upcoming",
                q: "test search",
                location: "New York",
                from: DateTime.UtcNow.AddDays(1),
                to: DateTime.UtcNow.AddDays(30),
                page: 2,
                pageSize: 5,
                sortBy: "StartDate",
                sortDir: "asc"
            );

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedEventsResponse>(okResult.Value);

            _mockEventService.Verify(s => s.GetMyEventsAsync(userId, It.Is<EventFilterRequest>(f =>
                f.Status == "Draft,Published" &&
                f.Category == "Technology" &&
                f.EventType == "upcoming" &&
                f.SearchKeyword == "test search" &&
                f.Location == "New York" &&
                f.Page == 2 &&
                f.PageSize == 5 &&
                f.SortBy == "StartDate" &&
                f.SortDirection == "asc"
            )), Times.Once);
        }

        [Fact]
        public async Task GetMyEvents_ServiceThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockEventService.Setup(s => s.GetMyEventsAsync(userId, It.IsAny<EventFilterRequest>()))
                .ThrowsAsync(new ArgumentException("Invalid page size"));

            // Set up user claims
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetMyEvents(pageSize: 101);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);

            _mockEventService.Verify(s => s.GetMyEventsAsync(userId, It.IsAny<EventFilterRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetMyEvents_ServiceThrowsGeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockEventService.Setup(s => s.GetMyEventsAsync(userId, It.IsAny<EventFilterRequest>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Set up user claims
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetMyEvents();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);

            _mockEventService.Verify(s => s.GetMyEventsAsync(userId, It.IsAny<EventFilterRequest>()), Times.Once);
        }

        #endregion
    }
} 