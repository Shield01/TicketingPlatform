using Microsoft.Extensions.Logging;
using Moq;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Modules.TicketService.Services;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for the TicketIssueService.
    /// </summary>
    public class TicketIssueServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepository;
        private readonly Mock<IQRCodeService> _mockQRCodeService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<Shared.Kernel.Interfaces.IUserInfoService> _mockUserInfoService;
        private readonly Mock<Shared.Kernel.Interfaces.IEventInfoService> _mockEventInfoService;
        private readonly Mock<ILogger<TicketIssueService>> _mockLogger;
        private readonly TicketIssueService _service;

        public TicketIssueServiceTests()
        {
            _mockTicketRepository = new Mock<ITicketRepository>();
            _mockQRCodeService = new Mock<IQRCodeService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockUserInfoService = new Mock<Shared.Kernel.Interfaces.IUserInfoService>();
            _mockEventInfoService = new Mock<Shared.Kernel.Interfaces.IEventInfoService>();
            _mockLogger = new Mock<ILogger<TicketIssueService>>();
            _service = new TicketIssueService(_mockTicketRepository.Object, _mockQRCodeService.Object, _mockEmailService.Object, _mockUserInfoService.Object, _mockEventInfoService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task IssueTicketsAsync_ValidRequest_ShouldIssueTickets()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();
            var ticketTier = CreateTestTicketTier();
            var issuedTickets = new List<Ticket> { CreateTestTicket() };

            _mockTicketRepository.Setup(r => r.ValidatePaymentForTicketIssuanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.ValidateTicketTierCapacityAsync(request.TicketTierId, request.Quantity))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.GetTicketTierAsync(request.TicketTierId))
                .ReturnsAsync(ticketTier);
            _mockTicketRepository.Setup(r => r.TicketCodeExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockTicketRepository.Setup(r => r.IssueMultipleTicketsAsync(It.IsAny<IEnumerable<Ticket>>()))
                .ReturnsAsync(issuedTickets);
            _mockTicketRepository.Setup(r => r.UpdateTicketTierSoldQuantityAsync(request.TicketTierId, request.Quantity))
                .ReturnsAsync(true);
            _mockQRCodeService.Setup(q => q.GenerateJWTLikeQRData(It.IsAny<Ticket>()))
                .Returns("test.qr.data");
            _mockUserInfoService.Setup(u => u.GetUserInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Shared.Kernel.Interfaces.UserInfo { Id = request.UserId, Email = "test@example.com", FirstName = "Test", LastName = "User" });
            _mockEventInfoService.Setup(e => e.GetEventInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Shared.Kernel.Interfaces.EventInfo { Id = request.EventId, Title = "Test Event" });

            // Act
            var result = await _service.IssueTicketsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Quantity, result.TicketsIssued);
            Assert.Equal(request.Price * request.Quantity, result.TotalPrice);
            Assert.Equal(request.Currency, result.Currency);
            Assert.Equal(request.PaymentId, result.PaymentId);

            _mockTicketRepository.Verify(r => r.IssueMultipleTicketsAsync(It.IsAny<IEnumerable<Ticket>>()), Times.Once);
            _mockTicketRepository.Verify(r => r.UpdateTicketTierSoldQuantityAsync(request.TicketTierId, request.Quantity), Times.Once);
        }

        [Fact]
        public async Task IssueTicketsAsync_InvalidPayment_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();

            _mockTicketRepository.Setup(r => r.ValidatePaymentForTicketIssuanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.IssueTicketsAsync(request));
        }

        [Fact]
        public async Task IssueTicketsAsync_InsufficientCapacity_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();

            _mockTicketRepository.Setup(r => r.ValidatePaymentForTicketIssuanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.ValidateTicketTierCapacityAsync(request.TicketTierId, request.Quantity))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.IssueTicketsAsync(request));
            Assert.Contains("Insufficient ticket capacity", exception.Message);
        }

        [Fact]
        public async Task IssueTicketsAsync_TicketTierNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();

            _mockTicketRepository.Setup(r => r.ValidatePaymentForTicketIssuanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.ValidateTicketTierCapacityAsync(request.TicketTierId, request.Quantity))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.GetTicketTierAsync(request.TicketTierId))
                .ReturnsAsync((TicketTier?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.IssueTicketsAsync(request));
            Assert.Contains("Ticket tier", exception.Message);
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task IssueTicketsAsync_NoPaymentIdProvided_ShouldAutoGeneratePaymentId()
        {
            // Arrange
            var request = CreateValidIssueTicketRequest(includePaymentId: false);
            var ticketTier = CreateTestTicketTier();
            var issuedTickets = new List<Ticket> { CreateTestTicket() };

            _mockTicketRepository.Setup(r => r.ValidatePaymentForTicketIssuanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.ValidateTicketTierCapacityAsync(request.TicketTierId, request.Quantity))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.GetTicketTierAsync(request.TicketTierId))
                .ReturnsAsync(ticketTier);
            _mockTicketRepository.Setup(r => r.TicketCodeExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockTicketRepository.Setup(r => r.IssueMultipleTicketsAsync(It.IsAny<IEnumerable<Ticket>>()))
                .ReturnsAsync(issuedTickets);
            _mockTicketRepository.Setup(r => r.UpdateTicketTierSoldQuantityAsync(request.TicketTierId, request.Quantity))
                .ReturnsAsync(true);
            _mockQRCodeService.Setup(q => q.GenerateJWTLikeQRData(It.IsAny<Ticket>()))
                .Returns("test.qr.data");

            // Act
            var result = await _service.IssueTicketsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(request.PaymentId.HasValue);
            Assert.NotEqual(Guid.Empty, request.PaymentId.Value);
            Assert.Equal(request.PaymentId.Value, result.PaymentId);

            // Verify that payment validation was called with the auto-generated GUID
            _mockTicketRepository.Verify(r => r.ValidatePaymentForTicketIssuanceAsync(request.PaymentId.Value), Times.Once);
        }

        [Fact]
        public async Task GetUserTicketsAsync_ValidUserId_ShouldReturnUserTickets()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tickets = new List<Ticket> { CreateTestTicket(userId) };
            var statusCounts = new Dictionary<string, int>
            {
                { Ticket.TicketStatus.Unused, 1 },
                { Ticket.TicketStatus.Used, 0 },
                { Ticket.TicketStatus.Cancelled, 0 }
            };

            _mockTicketRepository.Setup(r => r.GetUserTicketsAsync(userId, 1, 10, null))
                .ReturnsAsync((tickets, 1));
            _mockTicketRepository.Setup(r => r.GetUserTicketStatusCountsAsync(userId))
                .ReturnsAsync(statusCounts);
            _mockTicketRepository.Setup(r => r.GetTicketTierAsync(It.IsAny<Guid>()))
                .ReturnsAsync(CreateTestTicketTier());

            // Act
            var result = await _service.GetUserTicketsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Single(result.Tickets);
            Assert.Equal(1, result.TotalTickets);
            Assert.Equal(1, result.UnusedTickets);
            Assert.Equal(0, result.UsedTickets);
            Assert.Equal(0, result.CancelledTickets);
        }

        [Fact]
        public async Task GetTicketByIdAsync_ValidTicketAndUser_ShouldReturnTicket()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var ticket = CreateTestTicket(userId);
            ticket.Id = ticketId;

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);
            _mockTicketRepository.Setup(r => r.GetTicketTierAsync(ticket.TicketTierId))
                .ReturnsAsync(CreateTestTicketTier());

            // Act
            var result = await _service.GetTicketByIdAsync(ticketId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticketId, result.Id);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task GetTicketByIdAsync_WrongUser_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var ticket = CreateTestTicket(wrongUserId);

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.GetTicketByIdAsync(ticketId, userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task VerifyTicketAsync_ValidUnusedTicket_ShouldMarkAsUsedAndReturnValid()
        {
            // Arrange
            var request = new TicketVerificationRequest { TicketCode = "TKT-20241201-TEST1234" };
            var ticket = CreateTestTicket();
            ticket.TicketCode = request.TicketCode;

            _mockTicketRepository.Setup(r => r.GetTicketByCodeAsync(request.TicketCode))
                .ReturnsAsync(ticket);
            _mockTicketRepository.Setup(r => r.MarkTicketAsUsedAsync(ticket.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.VerifyTicketAsync(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(ticket.Id, result.TicketId);
            Assert.Equal(ticket.EventId, result.EventId);
            Assert.Contains("verified successfully", result.Message);

            _mockTicketRepository.Verify(r => r.MarkTicketAsUsedAsync(ticket.Id), Times.Once);
        }

        [Fact]
        public async Task VerifyTicketAsync_AlreadyUsedTicket_ShouldReturnInvalid()
        {
            // Arrange
            var request = new TicketVerificationRequest { TicketCode = "TKT-20241201-TEST1234" };
            var ticket = CreateTestTicket();
            ticket.TicketCode = request.TicketCode;
            ticket.IsUsed = true;
            ticket.Status = Ticket.TicketStatus.Used;

            _mockTicketRepository.Setup(r => r.GetTicketByCodeAsync(request.TicketCode))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.VerifyTicketAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("not valid for use", result.Message);

            _mockTicketRepository.Verify(r => r.MarkTicketAsUsedAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task VerifyTicketAsync_TicketNotFound_ShouldReturnInvalid()
        {
            // Arrange
            var request = new TicketVerificationRequest { TicketCode = "NONEXISTENT" };

            _mockTicketRepository.Setup(r => r.GetTicketByCodeAsync(request.TicketCode))
                .ReturnsAsync((Ticket?)null);

            // Act
            var result = await _service.VerifyTicketAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Ticket not found.", result.Message);
        }

        [Fact]
        public async Task CancelTicketAsync_ValidUnusedTicket_ShouldCancelTicket()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var ticket = CreateTestTicket(userId);
            ticket.Id = ticketId;

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);
            _mockTicketRepository.Setup(r => r.CancelTicketAsync(ticketId))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(r => r.UpdateTicketTierSoldQuantityAsync(ticket.TicketTierId, -1))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CancelTicketAsync(ticketId, userId);

            // Assert
            Assert.True(result);

            _mockTicketRepository.Verify(r => r.CancelTicketAsync(ticketId), Times.Once);
            _mockTicketRepository.Verify(r => r.UpdateTicketTierSoldQuantityAsync(ticket.TicketTierId, -1), Times.Once);
        }

        [Fact]
        public async Task CancelTicketAsync_WrongUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var ticket = CreateTestTicket(wrongUserId);

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CancelTicketAsync(ticketId, userId));
        }

        [Fact]
        public async Task CancelTicketAsync_AlreadyUsedTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var ticket = CreateTestTicket(userId);
            ticket.IsUsed = true;
            ticket.Status = Ticket.TicketStatus.Used;

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelTicketAsync(ticketId, userId));
            Assert.Contains("already been used", exception.Message);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "Event ID cannot be empty")]
        [InlineData(null, "User ID cannot be empty")]
        public async Task ValidateTicketIssuanceRequestAsync_InvalidRequest_ShouldThrowArgumentException(string? eventIdString, string expectedError)
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();
            if (eventIdString == "00000000-0000-0000-0000-000000000000")
                request.EventId = Guid.Empty;
            else if (eventIdString == null)
                request.UserId = Guid.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateTicketIssuanceRequestAsync(request));
            Assert.Contains(expectedError, exception.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public async Task ValidateTicketIssuanceRequestAsync_InvalidQuantity_ShouldThrowArgumentException(int quantity)
        {
            // Arrange
            var request = CreateValidIssueTicketRequest();
            request.Quantity = quantity;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateTicketIssuanceRequestAsync(request));
            Assert.Contains("Quantity must be between 1 and 10", exception.Message);
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

        private static Ticket CreateTestTicket(Guid? userId = null)
        {
            return new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = Ticket.GenerateTicketCode(),
                Status = Ticket.TicketStatus.Unused,
                IsUsed = false,
                PaymentId = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static TicketTier CreateTestTicketTier()
        {
            return new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "Test Tier",
                Description = "Test Description",
                Price = 50.00m,
                Currency = "USD",
                MaxQuantity = 100,
                SoldQuantity = 0,
                IsAvailable = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        #region QR Code Validation Tests

        [Fact]
        public async Task ValidateQRCodeAsync_WithValidQRCode_ShouldReturnSuccessResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var ticketCode = "TKT-20241201-TEST1234";
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVU5VU0VEIiwiaXNzdWVkQXQiOiIyMDI0LTEyLTAxVDEwOjAwOjAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTA6MDA6MDBaIn0.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };
            var ticket = CreateTestTicket();
            ticket.Id = ticketId;
            ticket.TicketCode = ticketCode;

            var extractedData = new Dictionary<string, string>
            {
                { "ticketId", ticketId.ToString() },
                { "ticketCode", ticketCode },
                { "eventId", ticket.EventId.ToString() },
                { "userId", ticket.UserId.ToString() },
                { "ticketTierId", ticket.TicketTierId.ToString() },
                { "status", ticket.Status }
            };

            _mockQRCodeService.Setup(q => q.ValidateAndExtractQRData(qrCodeData))
                .Returns(extractedData);
            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);
            _mockTicketRepository.Setup(r => r.MarkTicketAsUsedAsync(ticketId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ValidateQRCodeAsync(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(ticketId, result.TicketId);
            Assert.Equal(ticket.EventId, result.EventId);
            Assert.Equal("QR code validated successfully and ticket marked as used.", result.Message);
            
            _mockQRCodeService.Verify(q => q.ValidateAndExtractQRData(qrCodeData), Times.Once);
            _mockTicketRepository.Verify(r => r.GetTicketByIdAsync(ticketId), Times.Once);
            _mockTicketRepository.Verify(r => r.MarkTicketAsUsedAsync(ticketId), Times.Once);
        }

        [Fact]
        public async Task ValidateQRCodeAsync_WithInvalidQRCode_ShouldReturnInvalidResponse()
        {
            // Arrange
            var qrCodeData = "invalid.qr.data";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };

            _mockQRCodeService.Setup(q => q.ValidateAndExtractQRData(qrCodeData))
                .Returns((Dictionary<string, string>?)null);

            // Act
            var result = await _service.ValidateQRCodeAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Invalid QR code data. The QR code may be corrupted, expired, or tampered with.", result.Message);
            
            _mockQRCodeService.Verify(q => q.ValidateAndExtractQRData(qrCodeData), Times.Once);
            _mockTicketRepository.Verify(r => r.GetTicketByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockTicketRepository.Verify(r => r.MarkTicketAsUsedAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ValidateQRCodeAsync_WithMissingTicketId_ShouldReturnInvalidResponse()
        {
            // Arrange
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRDb2RlIjoiVEtULTIwMjQxMjAxLVRFU1QxMjM0In0.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };

            var extractedData = new Dictionary<string, string>
            {
                { "ticketCode", "TKT-20241201-TEST1234" }
                // Missing ticketId
            };

            _mockQRCodeService.Setup(q => q.ValidateAndExtractQRData(qrCodeData))
                .Returns(extractedData);

            // Act
            var result = await _service.ValidateQRCodeAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("QR code does not contain valid ticket information.", result.Message);
            
            _mockQRCodeService.Verify(q => q.ValidateAndExtractQRData(qrCodeData), Times.Once);
            _mockTicketRepository.Verify(r => r.GetTicketByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ValidateQRCodeAsync_WithNonExistentTicket_ShouldReturnInvalidResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var ticketCode = "TKT-20241201-TEST1234";
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVU5VU0VEIiwiaXNzdWVkQXQiOiIyMDI0LTEyLTAxVDEwOjAwOjAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTA6MDA6MDBaIn0.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };

            var extractedData = new Dictionary<string, string>
            {
                { "ticketId", ticketId.ToString() },
                { "ticketCode", ticketCode }
            };

            _mockQRCodeService.Setup(q => q.ValidateAndExtractQRData(qrCodeData))
                .Returns(extractedData);
            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync((Ticket?)null);

            // Act
            var result = await _service.ValidateQRCodeAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Ticket not found. This ticket may have been deleted or the QR code is invalid.", result.Message);
            
            _mockQRCodeService.Verify(q => q.ValidateAndExtractQRData(qrCodeData), Times.Once);
            _mockTicketRepository.Verify(r => r.GetTicketByIdAsync(ticketId), Times.Once);
            _mockTicketRepository.Verify(r => r.MarkTicketAsUsedAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ValidateQRCodeAsync_WithUsedTicket_ShouldReturnInvalidResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var ticketCode = "TKT-20241201-TEST1234";
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVVNFRCIsImlzc3VlZEF0IjoiMjAyNC0xMi0wMVQxMDowMDowMFoiLCJleHAiOiIyMDI1LTEyLTAxVDEwOjAwOjAwWiJ9.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };
            var ticket = CreateTestTicket();
            ticket.Id = ticketId;
            ticket.TicketCode = ticketCode;
            ticket.IsUsed = true;
            ticket.Status = Ticket.TicketStatus.Used;

            var extractedData = new Dictionary<string, string>
            {
                { "ticketId", ticketId.ToString() },
                { "ticketCode", ticketCode }
            };

            _mockQRCodeService.Setup(q => q.ValidateAndExtractQRData(qrCodeData))
                .Returns(extractedData);
            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.ValidateQRCodeAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Ticket cannot be used because it is already used.", result.Message);
            Assert.Equal(ticketId, result.TicketId);
            Assert.Equal(ticket.EventId, result.EventId);
            
            _mockQRCodeService.Verify(q => q.ValidateAndExtractQRData(qrCodeData), Times.Once);
            _mockTicketRepository.Verify(r => r.GetTicketByIdAsync(ticketId), Times.Once);
            _mockTicketRepository.Verify(r => r.MarkTicketAsUsedAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ValidateQRCodeAsync_WithTicketCodeMismatch_ShouldReturnInvalidResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var ticketCode = "TKT-20241201-TEST1234";
            var qrCodeData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IlRJQ0tFVCJ9.eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsInRpY2tldENvZGUiOiJUS1QtMjAyNDEyMDEtVEVTVDEyMzQiLCJldmVudElkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidXNlcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwidGlja2V0VGllcklkIjoiMTIzNDU2NzgtOTBhYi1jZGVmLTEyMzQtNTY3ODkwYWJjZGVmIiwic3RhdHVzIjoiVU5VU0VEIiwiaXNzdWVkQXQiOiIyMDI0LTEyLTAxVDEwOjAwOjAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTA6MDA6MDBaIn0.signature";
            var request = new QRCodeValidationRequest { QRCodeData = qrCodeData };
            var ticket = CreateTestTicket();
            ticket.Id = ticketId;
            ticket.TicketCode = "DIFFERENT-CODE"; // Different from QR code

            var extractedData = new Dictionary<string, string>
            {
                { "ticketId", ticketId.ToString() },
                { "ticketCode", ticketCode }
            };

            _mockQRCodeService.Setup(q => q.ValidateAndExtractQRData(qrCodeData))
                .Returns(extractedData);
            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.ValidateQRCodeAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("QR code data does not match ticket information. This may be a fraudulent QR code.", result.Message);
            
            _mockQRCodeService.Verify(q => q.ValidateAndExtractQRData(qrCodeData), Times.Once);
            _mockTicketRepository.Verify(r => r.GetTicketByIdAsync(ticketId), Times.Once);
            _mockTicketRepository.Verify(r => r.MarkTicketAsUsedAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion
    }
}
