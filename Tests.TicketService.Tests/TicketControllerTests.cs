using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.TicketService.Controllers;
using Modules.TicketService.DTOs;
using Modules.TicketService.Services;
using Shared.Kernel.Extensions;
using System.Security.Claims;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for the TicketController.
    /// </summary>
    public class TicketControllerTests
    {
        private readonly Mock<ILogger<TicketController>> _mockLogger;
        private readonly Mock<ITicketTierService> _mockTicketTierService;
        private readonly Mock<ITicketIssueService> _mockTicketIssueService;
        private readonly Mock<IQRCodeService> _mockQRCodeService;
        private readonly TicketController _controller;

        public TicketControllerTests()
        {
            _mockLogger = new Mock<ILogger<TicketController>>();
            _mockTicketTierService = new Mock<ITicketTierService>();
            _mockTicketIssueService = new Mock<ITicketIssueService>();
            _mockQRCodeService = new Mock<IQRCodeService>();
            _controller = new TicketController(_mockLogger.Object, _mockTicketTierService.Object, _mockTicketIssueService.Object, _mockQRCodeService.Object);

            // Setup HttpContext with user claims
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test", nameType: "UserId", roleType: "Role");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public async Task IssueTickets_ValidRequest_ShouldReturnCreated()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();
            var response = CreateIssueTicketResponse();

            _mockTicketIssueService.Setup(s => s.IssueTicketsAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.IssueTickets(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedResponse = Assert.IsType<IssueTicketResponse>(createdResult.Value);
            Assert.Equal(response.TicketsIssued, returnedResponse.TicketsIssued);
            Assert.Equal(response.PaymentId, returnedResponse.PaymentId);
        }

        [Fact]
        public async Task IssueTickets_InvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();
            _mockTicketIssueService.Setup(s => s.IssueTicketsAsync(request))
                .ThrowsAsync(new ArgumentException("Invalid request"));

            // Act
            var result = await _controller.IssueTickets(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task IssueTickets_InsufficientCapacity_ShouldReturnConflict()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();
            _mockTicketIssueService.Setup(s => s.IssueTicketsAsync(request))
                .ThrowsAsync(new InvalidOperationException("Insufficient capacity"));

            // Act
            var result = await _controller.IssueTickets(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var errorResponse = conflictResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task IssueTickets_UnexpectedError_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();
            _mockTicketIssueService.Setup(s => s.IssueTicketsAsync(request))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.IssueTickets(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task IssueTickets_NoPaymentIdProvided_ShouldStillSucceed()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest(includePaymentId: false);
            var response = CreateIssueTicketResponse();

            _mockTicketIssueService.Setup(s => s.IssueTicketsAsync(It.IsAny<IssueTicketRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.IssueTickets(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedResponse = Assert.IsType<IssueTicketResponse>(createdResult.Value);
            Assert.Equal(response.TicketsIssued, returnedResponse.TicketsIssued);

            // Verify the service was called (PaymentId auto-generation happens in service layer)
            _mockTicketIssueService.Verify(s => s.IssueTicketsAsync(It.IsAny<IssueTicketRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetUserTickets_ValidRequest_ShouldReturnOk()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var response = CreateUserTicketsResponse(userId);

            _mockTicketIssueService.Setup(s => s.GetUserTicketsAsync(userId, 1, 10, null))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserTickets();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsType<UserTicketsResponse>(okResult.Value);
            Assert.Equal(userId, returnedResponse.UserId);
            Assert.Equal(response.TotalTickets, returnedResponse.TotalTickets);
        }

        [Fact]
        public async Task GetUserTickets_WithPagination_ShouldPassParameters()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var page = 2;
            var pageSize = 5;
            var status = "UNUSED";
            var response = CreateUserTicketsResponse(userId);

            _mockTicketIssueService.Setup(s => s.GetUserTicketsAsync(userId, page, pageSize, status))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserTickets(page, pageSize, status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockTicketIssueService.Verify(s => s.GetUserTicketsAsync(userId, page, pageSize, status), Times.Once);
        }

        [Fact]
        public async Task GetUserTickets_ServiceError_ShouldReturnInternalServerError()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            _mockTicketIssueService.Setup(s => s.GetUserTicketsAsync(userId, 1, 10, null))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.GetUserTickets();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetTicketById_ValidRequest_ShouldReturnOk()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var ticketId = Guid.NewGuid();
            var ticketResponse = CreateTicketResponse(ticketId, userId);

            _mockTicketIssueService.Setup(s => s.GetTicketByIdAsync(ticketId, userId))
                .ReturnsAsync(ticketResponse);

            // Act
            var result = await _controller.GetTicketById(ticketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTicket = Assert.IsType<TicketResponse>(okResult.Value);
            Assert.Equal(ticketId, returnedTicket.Id);
            Assert.Equal(userId, returnedTicket.UserId);
        }

        [Fact]
        public async Task GetTicketById_TicketNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var ticketId = Guid.NewGuid();

            _mockTicketIssueService.Setup(s => s.GetTicketByIdAsync(ticketId, userId))
                .ReturnsAsync((TicketResponse?)null);

            // Act
            var result = await _controller.GetTicketById(ticketId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = notFoundResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task CancelTicket_ValidRequest_ShouldReturnOk()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var ticketId = Guid.NewGuid();

            _mockTicketIssueService.Setup(s => s.CancelTicketAsync(ticketId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CancelTicket(ticketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var successResponse = okResult.Value;
            Assert.NotNull(successResponse);
        }

        [Fact]
        public async Task CancelTicket_CancellationFailed_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var ticketId = Guid.NewGuid();

            _mockTicketIssueService.Setup(s => s.CancelTicketAsync(ticketId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CancelTicket(ticketId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task CancelTicket_UnauthorizedAccess_ShouldReturnNotFound()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var ticketId = Guid.NewGuid();

            _mockTicketIssueService.Setup(s => s.CancelTicketAsync(ticketId, userId))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            // Act
            var result = await _controller.CancelTicket(ticketId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = notFoundResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task CancelTicket_InvalidOperation_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var ticketId = Guid.NewGuid();

            _mockTicketIssueService.Setup(s => s.CancelTicketAsync(ticketId, userId))
                .ThrowsAsync(new InvalidOperationException("Cannot cancel used ticket"));

            // Act
            var result = await _controller.CancelTicket(ticketId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task VerifyTicket_ValidTicket_ShouldReturnOk()
        {
            // Arrange
            var request = new TicketVerificationRequest { TicketCode = "TKT-20241201-TEST1234" };
            var response = new TicketVerificationResponse
            {
                IsValid = true,
                TicketId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventName = "Test Event",
                Message = "Ticket verified successfully"
            };

            _mockTicketIssueService.Setup(s => s.VerifyTicketAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.VerifyTicket(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsType<TicketVerificationResponse>(okResult.Value);
            Assert.True(returnedResponse.IsValid);
            Assert.Equal(response.TicketId, returnedResponse.TicketId);
        }

        [Fact]
        public async Task VerifyTicket_InvalidTicket_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new TicketVerificationRequest { TicketCode = "INVALID" };
            var response = new TicketVerificationResponse
            {
                IsValid = false,
                Message = "Ticket not found"
            };

            _mockTicketIssueService.Setup(s => s.VerifyTicketAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.VerifyTicket(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedResponse = Assert.IsType<TicketVerificationResponse>(badRequestResult.Value);
            Assert.False(returnedResponse.IsValid);
        }

        [Fact]
        public async Task VerifyTicket_ServiceError_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = new TicketVerificationRequest { TicketCode = "TKT-20241201-TEST1234" };

            _mockTicketIssueService.Setup(s => s.VerifyTicketAsync(request))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.VerifyTicket(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task CreateTicketTiers_AllTiersSuccessful_ShouldReturnCreated()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var request = CreateTicketTiersRequest();
            var expectedResponses = new List<TicketTierResponse>
            {
                CreateTicketTierResponse("VIP"),
                CreateTicketTierResponse("Regular")
            };

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(
                It.IsAny<Guid>(), It.IsAny<CreateTicketTierRequest>(), userId))
                .ReturnsAsync((Guid eventId, CreateTicketTierRequest req, Guid userId) => 
                    expectedResponses.First(r => r.Name == req.Name));

            // Act
            var result = await _controller.CreateTicketTiers(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedTiers = Assert.IsType<List<TicketTierResponse>>(createdResult.Value);
            Assert.Equal(2, returnedTiers.Count);
            Assert.Contains(returnedTiers, t => t.Name == "VIP");
            Assert.Contains(returnedTiers, t => t.Name == "Regular");

            _mockTicketTierService.Verify(s => s.CreateTicketTierAsync(
                It.IsAny<Guid>(), It.IsAny<CreateTicketTierRequest>(), userId), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateTicketTiers_PartialSuccess_ShouldReturnOkWithDetails()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var request = CreateTicketTiersRequest();
            var successResponse = CreateTicketTierResponse("VIP");

            _mockTicketTierService.SetupSequence(s => s.CreateTicketTierAsync(
                It.IsAny<Guid>(), It.IsAny<CreateTicketTierRequest>(), userId))
                .ReturnsAsync(successResponse)
                .ThrowsAsync(new InvalidOperationException("Tier name already exists"));

            // Act
            var result = await _controller.CreateTicketTiers(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);

            // Use dynamic to access anonymous type properties
            dynamic dynamicResponse = response!;
            var message = dynamicResponse.message as string;
            var createdTiers = dynamicResponse.createdTiers as List<TicketTierResponse>;
            var errors = dynamicResponse.errors as List<string>;

            Assert.Contains("Partially created 1 out of 2", message);
            Assert.Single(createdTiers);
            Assert.Single(errors);
            Assert.Equal("VIP", createdTiers!.First().Name);

            _mockTicketTierService.Verify(s => s.CreateTicketTierAsync(
                It.IsAny<Guid>(), It.IsAny<CreateTicketTierRequest>(), userId), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateTicketTiers_AllTiersFail_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var request = CreateTicketTiersRequest();

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(
                It.IsAny<Guid>(), It.IsAny<CreateTicketTierRequest>(), userId))
                .ThrowsAsync(new ArgumentException("Invalid tier data"));

            // Act
            var result = await _controller.CreateTicketTiers(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value;
            Assert.NotNull(errorResponse);

            _mockTicketTierService.Verify(s => s.CreateTicketTierAsync(
                It.IsAny<Guid>(), It.IsAny<CreateTicketTierRequest>(), userId), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateTicketTiers_UnexpectedError_ShouldReturnInternalServerError()
        {
            // Arrange
            var userId = GetUserIdFromContext();
            var request = CreateTicketTiersRequest();

            _mockTicketTierService.Setup(s => s.CreateTicketTierAsync(
                It.IsAny<Guid>(), It.IsAny<CreateTicketTierRequest>(), userId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CreateTicketTiers(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        private Guid GetUserIdFromContext()
        {
            var userIdClaim = _controller.HttpContext.User.FindFirst("UserId");
            return Guid.Parse(userIdClaim!.Value);
        }

        private static IssueTicketRequest CreateValidIssueTicketRequest(bool includePaymentId = true)
        {
            return new IssueTicketRequest
            {
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                PaymentId = includePaymentId ? Guid.NewGuid() : null,
                Quantity = 1
            };
        }

        private static IssueTicketResponse CreateIssueTicketResponse()
        {
            return new IssueTicketResponse
            {
                Tickets = new List<TicketResponse> { CreateTicketResponse() },
                TicketsIssued = 1,
                TotalPrice = 50.00m,
                Currency = "USD",
                PaymentId = Guid.NewGuid(),
                IssuedAt = DateTime.UtcNow,
                Message = "Tickets issued successfully"
            };
        }

        private static UserTicketsResponse CreateUserTicketsResponse(Guid userId)
        {
            return new UserTicketsResponse
            {
                UserId = userId,
                Tickets = new List<TicketResponse> { CreateTicketResponse(userId: userId) },
                TotalTickets = 1,
                UnusedTickets = 1,
                UsedTickets = 0,
                CancelledTickets = 0,
                Page = 1,
                PageSize = 10
            };
        }

        private static TicketResponse CreateTicketResponse(Guid? id = null, Guid? userId = null)
        {
            return new TicketResponse
            {
                Id = id ?? Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventName = "Test Event",
                UserId = userId ?? Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                TierName = "Test Tier",
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241201-TEST1234",
                Status = "UNUSED",
                IssuedAt = DateTime.UtcNow,
                IsActive = true,
                IsValidForUse = true
            };
        }

        private static CreateTicketTiersRequest CreateTicketTiersRequest()
        {
            return new CreateTicketTiersRequest
            {
                EventId = Guid.NewGuid(),
                Tiers = new List<TicketTierRequest>
                {
                    new TicketTierRequest
                    {
                        Name = "VIP",
                        Description = "VIP access with premium benefits",
                        Price = 150.00m,
                        Quantity = 50
                    },
                    new TicketTierRequest
                    {
                        Name = "Regular",
                        Description = "Standard event access",
                        Price = 75.00m,
                        Quantity = 200
                    }
                }
            };
        }

        private static TicketTierResponse CreateTicketTierResponse(string name)
        {
            return new TicketTierResponse
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = name,
                Description = $"{name} tier description",
                Price = name == "VIP" ? 150.00m : 75.00m,
                Currency = "USD",
                MaxQuantity = name == "VIP" ? 50 : 200,
                SoldQuantity = 0,
                IsAvailable = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        #region QR Code Tests

        [Fact]
        public async Task GetTicketQRCode_WithValidTicket_ReturnsQRCodeResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ticketResponse = CreateTicketResponse(ticketId, userId);

            _mockTicketIssueService
                .Setup(x => x.GetTicketByIdAsync(ticketId, userId))
                .ReturnsAsync(ticketResponse);

            // Act
            var result = await _controller.GetTicketQRCode(ticketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var qrResponse = Assert.IsType<QRCodeResponse>(okResult.Value);
            
            Assert.Equal(ticketId, qrResponse.TicketId);
            Assert.Equal(ticketResponse.TicketCode, qrResponse.TicketCode);
            Assert.Equal(ticketResponse.QRCodeData, qrResponse.QRCodeData);
            Assert.Equal(ticketResponse.QRCodeImage, qrResponse.QRCodeImage);
            Assert.Equal("image/png", qrResponse.ImageMimeType);
            Assert.Equal(512, qrResponse.ImageSize);
            Assert.Equal(ticketResponse.IsValidForUse, qrResponse.IsValidForUse);
            Assert.Equal(ticketResponse.Status, qrResponse.Status);
        }

        [Fact]
        public async Task GetTicketQRCode_WithNonExistentTicket_ReturnsNotFound()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockTicketIssueService
                .Setup(x => x.GetTicketByIdAsync(ticketId, userId))
                .ReturnsAsync((TicketResponse?)null);

            // Act
            var result = await _controller.GetTicketQRCode(ticketId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = notFoundResult.Value;
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public async Task GetTicketQRCode_WithUnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var controller = new TicketController(_mockLogger.Object, _mockTicketTierService.Object, _mockTicketIssueService.Object, _mockQRCodeService.Object);
            
            // No HttpContext setup - should return unauthorized

            // Act
            var result = await controller.GetTicketQRCode(ticketId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetTicketQRCode_WithServiceException_ReturnsInternalServerError()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockTicketIssueService
                .Setup(x => x.GetTicketByIdAsync(ticketId, userId))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.GetTicketQRCode(ticketId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        private static TicketResponse CreateTicketResponse(Guid ticketId, Guid userId)
        {
            return new TicketResponse
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                EventName = "Test Event",
                UserId = userId,
                TicketTierId = Guid.NewGuid(),
                TierName = "VIP",
                TierDescription = "VIP tier",
                Price = 150.00m,
                Currency = "USD",
                TicketCode = "TKT-20241201-TEST1234",
                QRCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVU5VU0VEIiwiaXNzdWVkQXQiOiIyMDI0LTEyLTAxVDEwOjAwOjAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTA6MDA6MDBaIn0.signature",
                QRCodeImage = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
                IsUsed = false,
                UsedAt = null,
                Status = "UNUSED",
                PaymentId = Guid.NewGuid(),
                IssuedAt = DateTime.UtcNow,
                IsActive = true,
                IsValidForUse = true
            };
        }

        #endregion

        #region QR Code Validation Tests

        [Fact]
        public async Task ValidateQRCode_WithValidQRCode_ReturnsSuccessResponse()
        {
            // Arrange
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVU5VU0VEIiwiaXNzdWVkQXQiOiIyMDI0LTEyLTAxVDEwOjAwOjAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTA6MDA6MDBaIn0.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };
            var expectedResponse = new TicketVerificationResponse
            {
                IsValid = true,
                TicketId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventName = "Test Event",
                TicketTier = "VIP",
                AttendeeName = "John Doe",
                VerifiedAt = DateTime.UtcNow,
                Message = "QR code validated successfully and ticket marked as used."
            };

            _mockTicketIssueService
                .Setup(x => x.ValidateQRCodeAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ValidateQRCode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TicketVerificationResponse>(okResult.Value);
            
            Assert.True(response.IsValid);
            Assert.Equal(expectedResponse.TicketId, response.TicketId);
            Assert.Equal(expectedResponse.EventId, response.EventId);
            Assert.Equal(expectedResponse.EventName, response.EventName);
            Assert.Equal(expectedResponse.TicketTier, response.TicketTier);
            Assert.Equal(expectedResponse.AttendeeName, response.AttendeeName);
            Assert.Equal("QR code validated successfully and ticket marked as used.", response.Message);
        }

        [Fact]
        public async Task ValidateQRCode_WithInvalidQRCode_ReturnsBadRequest()
        {
            // Arrange
            var qrCodeData = "invalid.qr.data";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };
            var expectedResponse = new TicketVerificationResponse
            {
                IsValid = false,
                Message = "Invalid QR code data. The QR code may be corrupted, expired, or tampered with."
            };

            _mockTicketIssueService
                .Setup(x => x.ValidateQRCodeAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ValidateQRCode(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<TicketVerificationResponse>(badRequestResult.Value);
            
            Assert.False(response.IsValid);
            Assert.Equal("Invalid QR code data. The QR code may be corrupted, expired, or tampered with.", response.Message);
        }

        [Fact]
        public async Task ValidateQRCode_WithUsedTicket_ReturnsBadRequest()
        {
            // Arrange
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVVNFRCIsImlzc3VlZEF0IjoiMjAyNC0xMi0wMVQxMDowMDowMFoiLCJleHAiOiIyMDI1LTEyLTAxVDEwOjAwOjAwWiJ9.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };
            var expectedResponse = new TicketVerificationResponse
            {
                IsValid = false,
                TicketId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventName = "Test Event",
                TicketTier = "VIP",
                AttendeeName = "John Doe",
                VerifiedAt = DateTime.UtcNow,
                Message = "Ticket cannot be used because it is already used."
            };

            _mockTicketIssueService
                .Setup(x => x.ValidateQRCodeAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ValidateQRCode(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<TicketVerificationResponse>(badRequestResult.Value);
            
            Assert.False(response.IsValid);
            Assert.Equal("Ticket cannot be used because it is already used.", response.Message);
        }

        [Fact]
        public async Task ValidateQRCode_WithUnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVU5VU0VEIiwiaXNzdWVkQXQiOiIyMDI0LTEyLTAxVDEwOjAwOjAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTA6MDA6MDBaIn0.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };
            var controller = new TicketController(_mockLogger.Object, _mockTicketTierService.Object, _mockTicketIssueService.Object, _mockQRCodeService.Object);
            
            // No HttpContext setup - should return unauthorized

            // Act
            var result = await controller.ValidateQRCode(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task ValidateQRCode_WithServiceException_ReturnsInternalServerError()
        {
            // Arrange
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVU5VU0VEIiwiaXNzdWVkQXQiOiIyMDI0LTEyLTAxVDEwOjAwOjAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTA6MDA6MDBaIn0.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };

            _mockTicketIssueService
                .Setup(x => x.ValidateQRCodeAsync(request))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.ValidateQRCode(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion
    }
}