using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.TicketService.Controllers;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Services;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for TicketController override functionality.
    /// </summary>
    public class TicketControllerOverrideTests
    {
        private readonly Mock<ILogger<TicketController>> _mockLogger;
        private readonly Mock<ITicketTierService> _mockTicketTierService;
        private readonly Mock<ITicketIssueService> _mockTicketIssueService;
        private readonly Mock<IQRCodeService> _mockQRCodeService;
        private readonly Mock<ITicketOverrideService> _mockTicketOverrideService;
        private readonly TicketController _controller;

        public TicketControllerOverrideTests()
        {
            _mockLogger = new Mock<ILogger<TicketController>>();
            _mockTicketTierService = new Mock<ITicketTierService>();
            _mockTicketIssueService = new Mock<ITicketIssueService>();
            _mockQRCodeService = new Mock<IQRCodeService>();
            _mockTicketOverrideService = new Mock<ITicketOverrideService>();

            _controller = new TicketController(
                _mockLogger.Object,
                _mockTicketTierService.Object,
                _mockTicketIssueService.Object,
                _mockQRCodeService.Object,
                _mockTicketOverrideService.Object);

            // Setup HttpContext with authenticated user
            var httpContext = new DefaultHttpContext();
            var operatorUserId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim("UserId", operatorUserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            httpContext.User = principal;

            // Mock IP address and user agent
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            httpContext.Request.Headers["User-Agent"] = "Test User Agent";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task OverrideTicketStatus_ValidRequest_ReturnsOkWithTicketResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.Parse(_controller.HttpContext.User.FindFirst("UserId")!.Value);
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Manual override for scanning issue",
                ForceOverride = false
            };

            var ticketResponse = new TicketResponse
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                EventName = "Test Event",
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                TierName = "VIP",
                Price = 100.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Used,
                IsUsed = true,
                IsValidForUse = false,
                IssuedAt = DateTime.UtcNow.AddHours(-1),
                IsActive = true
            };

            _mockTicketOverrideService.Setup(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                "127.0.0.1",
                "Test User Agent"))
                .ReturnsAsync(ticketResponse);

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTicket = Assert.IsType<TicketResponse>(okResult.Value);
            Assert.Equal(ticketResponse.Id, returnedTicket.Id);
            Assert.Equal(ticketResponse.Status, returnedTicket.Status);
            Assert.Equal(ticketResponse.IsUsed, returnedTicket.IsUsed);

            // Verify service was called with correct parameters
            _mockTicketOverrideService.Verify(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                "127.0.0.1",
                "Test User Agent"), Times.Once);
        }

        [Fact]
        public async Task OverrideTicketStatus_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.Parse(_controller.HttpContext.User.FindFirst("UserId")!.Value);
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Manual override",
                ForceOverride = false
            };

            _mockTicketOverrideService.Setup(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync((TicketResponse?)null);

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = notFoundResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task OverrideTicketStatus_NoUserIdInClaims_ReturnsUnauthorized()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Manual override",
                ForceOverride = false
            };

            // Setup HttpContext without UserId claim
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin")
                // Missing UserId claim
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            httpContext.User = principal;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated.", unauthorizedResult.Value);

            // Verify service was not called
            _mockTicketOverrideService.Verify(s => s.OverrideTicketStatusAsync(
                It.IsAny<Guid>(),
                It.IsAny<TicketOverrideRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task OverrideTicketStatus_ServiceThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.Parse(_controller.HttpContext.User.FindFirst("UserId")!.Value);
            var request = new TicketOverrideRequest
            {
                NewStatus = "INVALID_STATUS",
                Reason = "Test reason",
                ForceOverride = false
            };

            _mockTicketOverrideService.Setup(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid status provided"));

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task OverrideTicketStatus_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.Parse(_controller.HttpContext.User.FindFirst("UserId")!.Value);
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Test reason",
                ForceOverride = false
            };

            _mockTicketOverrideService.Setup(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task OverrideTicketStatus_ForceOverrideTrue_PassesCorrectParameter()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.Parse(_controller.HttpContext.User.FindFirst("UserId")!.Value);
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Emergency override",
                ForceOverride = true
            };

            var ticketResponse = new TicketResponse
            {
                Id = ticketId,
                Status = Ticket.TicketStatus.Used,
                IsUsed = true
            };

            _mockTicketOverrideService.Setup(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(ticketResponse);

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify service was called with ForceOverride = true
            _mockTicketOverrideService.Verify(s => s.OverrideTicketStatusAsync(
                ticketId,
                It.Is<TicketOverrideRequest>(r => r.ForceOverride == true),
                operatorUserId,
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketAuditLog_ValidTicketId_ReturnsOkWithAuditLogs()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var auditLogs = new List<TicketAuditLogResponse>
            {
                new TicketAuditLogResponse
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    TicketCode = "TKT-20241216-ABCDEFGH",
                    PerformedByUserId = Guid.NewGuid(),
                    ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                    PreviousStatus = Ticket.TicketStatus.Unused,
                    NewStatus = Ticket.TicketStatus.Used,
                    Reason = "Manual override",
                    WasForced = false,
                    IpAddress = "127.0.0.1",
                    PerformedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new TicketAuditLogResponse
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    TicketCode = "TKT-20241216-ABCDEFGH",
                    PerformedByUserId = Guid.NewGuid(),
                    ActionType = TicketAuditLog.ActionTypes.Reset,
                    PreviousStatus = Ticket.TicketStatus.Used,
                    NewStatus = Ticket.TicketStatus.Unused,
                    Reason = "Customer request",
                    WasForced = false,
                    IpAddress = "127.0.0.1",
                    PerformedAt = DateTime.UtcNow.AddMinutes(-5)
                }
            };

            _mockTicketOverrideService.Setup(s => s.GetTicketAuditLogAsync(ticketId))
                .ReturnsAsync(auditLogs);

            // Act
            var result = await _controller.GetTicketAuditLog(ticketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAuditLogs = Assert.IsType<List<TicketAuditLogResponse>>(okResult.Value);
            Assert.Equal(2, returnedAuditLogs.Count);
            Assert.Equal(auditLogs[0].Id, returnedAuditLogs[0].Id);
            Assert.Equal(auditLogs[1].Id, returnedAuditLogs[1].Id);

            // Verify service was called
            _mockTicketOverrideService.Verify(s => s.GetTicketAuditLogAsync(ticketId), Times.Once);
        }

        [Fact]
        public async Task GetTicketAuditLog_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var ticketId = Guid.NewGuid();

            _mockTicketOverrideService.Setup(s => s.GetTicketAuditLogAsync(ticketId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetTicketAuditLog(ticketId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetTicketAuditLog_EmptyAuditLog_ReturnsOkWithEmptyList()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var emptyAuditLogs = new List<TicketAuditLogResponse>();

            _mockTicketOverrideService.Setup(s => s.GetTicketAuditLogAsync(ticketId))
                .ReturnsAsync(emptyAuditLogs);

            // Act
            var result = await _controller.GetTicketAuditLog(ticketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAuditLogs = Assert.IsType<List<TicketAuditLogResponse>>(okResult.Value);
            Assert.Empty(returnedAuditLogs);
        }

        [Theory]
        [InlineData("UNUSED")]
        [InlineData("USED")]
        [InlineData("CANCELLED")]
        [InlineData("EXPIRED")]
        public async Task OverrideTicketStatus_AllValidStatuses_AcceptsRequest(string validStatus)
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.Parse(_controller.HttpContext.User.FindFirst("UserId")!.Value);
            var request = new TicketOverrideRequest
            {
                NewStatus = validStatus,
                Reason = $"Override to {validStatus}",
                ForceOverride = false
            };

            var ticketResponse = new TicketResponse
            {
                Id = ticketId,
                Status = validStatus
            };

            _mockTicketOverrideService.Setup(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(ticketResponse);

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTicket = Assert.IsType<TicketResponse>(okResult.Value);
            Assert.Equal(validStatus, returnedTicket.Status);
        }

        [Fact]
        public async Task OverrideTicketStatus_CapturesIpAddressAndUserAgent_PassesToService()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.Parse(_controller.HttpContext.User.FindFirst("UserId")!.Value);
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Test IP and User Agent capture",
                ForceOverride = false
            };

            var expectedIpAddress = "192.168.1.100";
            var expectedUserAgent = "Custom Test Agent/1.0";

            // Setup HttpContext with specific IP and User Agent
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("UserId", operatorUserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            httpContext.User = principal;
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(expectedIpAddress);
            httpContext.Request.Headers["User-Agent"] = expectedUserAgent;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var ticketResponse = new TicketResponse { Id = ticketId, Status = Ticket.TicketStatus.Used };

            _mockTicketOverrideService.Setup(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                expectedIpAddress,
                expectedUserAgent))
                .ReturnsAsync(ticketResponse);

            // Act
            var result = await _controller.OverrideTicketStatus(ticketId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify service was called with correct IP and User Agent
            _mockTicketOverrideService.Verify(s => s.OverrideTicketStatusAsync(
                ticketId,
                request,
                operatorUserId,
                expectedIpAddress,
                expectedUserAgent), Times.Once);
        }
    }
}
