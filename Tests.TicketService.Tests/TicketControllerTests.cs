using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Modules.TicketService.Controllers;
using Modules.TicketService.DTOs;
using Modules.TicketService.Services;
using Shared.Kernel.Constants;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for TicketController API endpoints.
    /// </summary>
    public class TicketControllerTests
    {
        private readonly Mock<ILogger<TicketController>> _mockLogger;
        private readonly Mock<ITicketTierService> _mockTicketTierService;
        private readonly TicketController _controller;

        public TicketControllerTests()
        {
            _mockLogger = new Mock<ILogger<TicketController>>();
            _mockTicketTierService = new Mock<ITicketTierService>();
            _controller = new TicketController(_mockLogger.Object, _mockTicketTierService.Object);

            // Setup default user context
            SetupUserContext(Guid.NewGuid());
        }

        private void SetupUserContext(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, userId.ToString()),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Organiser)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        #region CreateEventTicketTier Tests

        [Fact]
        public async Task CreateEventTicketTier_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            SetupUserContext(userId);

            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Description = "Premium access",
                Price = 150.00m,
                Currency = "USD",
                MaxQuantity = 50,
                IsAvailable = true
            };

            var expectedResponse = new TicketTierResponse
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                MaxQuantity = request.MaxQuantity,
                SoldQuantity = 0,
                IsAvailable = request.IsAvailable,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(eventId, request, userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateEventTicketTier(eventId, request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetEventTickets), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.True(createdResult.RouteValues.ContainsKey("eventId"));
            Assert.Equal(eventId, createdResult.RouteValues["eventId"]);

            var response = Assert.IsType<TicketTierResponse>(createdResult.Value);
            Assert.Equal(expectedResponse.Id, response.Id);
            Assert.Equal(expectedResponse.Name, response.Name);
            Assert.Equal(expectedResponse.Price, response.Price);

            _mockTicketTierService.Verify(s => s.CreateTicketTierAsync(eventId, request, userId), Times.Once);
        }

        [Fact]
        public async Task CreateEventTicketTier_NoUserId_ReturnsUnauthorized()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            // Setup controller with no user claims
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.CreateEventTicketTier(eventId, request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task CreateEventTicketTier_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            SetupUserContext(userId);

            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(eventId, request, userId))
                .ThrowsAsync(new ArgumentException("Price must be greater than 0."));

            // Act
            var result = await _controller.CreateEventTicketTier(eventId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            
            var error = badRequestResult.Value.GetType().GetProperty("error")?.GetValue(badRequestResult.Value)?.ToString();
            Assert.Equal("Price must be greater than 0.", error);
        }

        [Fact]
        public async Task CreateEventTicketTier_DuplicateName_ReturnsConflict()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            SetupUserContext(userId);

            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(eventId, request, userId))
                .ThrowsAsync(new InvalidOperationException("A ticket tier with the name 'VIP' already exists for this event."));

            // Act
            var result = await _controller.CreateEventTicketTier(eventId, request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflictResult.Value);
            
            var error = conflictResult.Value.GetType().GetProperty("error")?.GetValue(conflictResult.Value)?.ToString();
            Assert.Contains("already exists", error);
        }

        [Fact]
        public async Task CreateEventTicketTier_UnauthorizedAccess_ReturnsForbid()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            SetupUserContext(userId);

            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(eventId, request, userId))
                .ThrowsAsync(new UnauthorizedAccessException("User not authorized to create ticket tiers for this event."));

            // Act
            var result = await _controller.CreateEventTicketTier(eventId, request);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
            Assert.NotNull(forbidResult);
        }

        [Fact]
        public async Task CreateEventTicketTier_InternalError_ReturnsInternalServerError()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            SetupUserContext(userId);

            var request = new CreateTicketTierRequest
            {
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50
            };

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(eventId, request, userId))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act
            var result = await _controller.CreateEventTicketTier(eventId, request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.NotNull(statusResult.Value);
            
            var error = statusResult.Value.GetType().GetProperty("error")?.GetValue(statusResult.Value)?.ToString();
            Assert.Equal("An error occurred while creating the ticket tier.", error);
        }

        #endregion

        #region GetEventTickets Tests

        [Fact]
        public async Task GetEventTickets_ValidEventId_ReturnsOkWithTickets()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var expectedTiers = new List<TicketTierResponse>
            {
                new TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "VIP",
                    Price = 150.00m,
                    MaxQuantity = 50,
                    SoldQuantity = 10,
                    IsAvailable = true
                },
                new TicketTierResponse
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

            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(eventId))
                .ReturnsAsync(expectedTiers);

            // Act
            var result = await _controller.GetEventTickets(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tiers = Assert.IsAssignableFrom<IEnumerable<TicketTierResponse>>(okResult.Value);
            
            var tiersList = tiers.ToList();
            Assert.Equal(2, tiersList.Count);
            Assert.Equal("VIP", tiersList[0].Name);
            Assert.Equal("Regular", tiersList[1].Name);

            _mockTicketTierService.Verify(s => s.GetEventTicketTiersAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetEventTickets_InvalidEventId_ReturnsBadRequest()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(eventId))
                .ThrowsAsync(new ArgumentException("Event ID cannot be empty."));

            // Act
            var result = await _controller.GetEventTickets(eventId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            
            var error = badRequestResult.Value.GetType().GetProperty("error")?.GetValue(badRequestResult.Value)?.ToString();
            Assert.Equal("Event ID cannot be empty.", error);
        }

        [Fact]
        public async Task GetEventTickets_ServiceError_ReturnsInternalServerError()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(eventId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetEventTickets(eventId);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.NotNull(statusResult.Value);
            
            var error = statusResult.Value.GetType().GetProperty("error")?.GetValue(statusResult.Value)?.ToString();
            Assert.Equal("An error occurred while retrieving ticket tiers.", error);
        }

        [Fact]
        public async Task GetEventTickets_NoTiers_ReturnsEmptyList()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var emptyTiers = new List<TicketTierResponse>();

            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(eventId))
                .ReturnsAsync(emptyTiers);

            // Act
            var result = await _controller.GetEventTickets(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tiers = Assert.IsAssignableFrom<IEnumerable<TicketTierResponse>>(okResult.Value);
            Assert.Empty(tiers);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CreateAndGetTicketTier_Integration_Success()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            SetupUserContext(userId);

            var createRequest = new CreateTicketTierRequest
            {
                Name = "Early Bird",
                Description = "Limited time offer",
                Price = 50.00m,
                Currency = "USD",
                MaxQuantity = 100,
                IsAvailable = true
            };

            var createdTier = new TicketTierResponse
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = createRequest.Name,
                Description = createRequest.Description,
                Price = createRequest.Price,
                Currency = createRequest.Currency,
                MaxQuantity = createRequest.MaxQuantity,
                SoldQuantity = 0,
                IsAvailable = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var getTiers = new List<TicketTierResponse> { createdTier };

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(eventId, createRequest, userId))
                .ReturnsAsync(createdTier);

            _mockTicketTierService.Setup(s => s.GetEventTicketTiersAsync(eventId))
                .ReturnsAsync(getTiers);

            // Act - Create tier
            var createResult = await _controller.CreateEventTicketTier(eventId, createRequest);

            // Assert - Create
            var createdResult = Assert.IsType<CreatedAtActionResult>(createResult);
            Assert.NotNull(createdResult.Value);

            // Act - Get tiers
            var getResult = await _controller.GetEventTickets(eventId);

            // Assert - Get
            var okResult = Assert.IsType<OkObjectResult>(getResult);
            var tiers = Assert.IsAssignableFrom<IEnumerable<TicketTierResponse>>(okResult.Value);
            var tiersList = tiers.ToList();
            
            Assert.Single(tiersList);
            Assert.Equal(createdTier.Name, tiersList[0].Name);
            Assert.Equal(createdTier.Price, tiersList[0].Price);
        }

        #endregion

        #region Helper Methods

        private void SetupInvalidUserContext()
        {
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        #endregion
    }
}
