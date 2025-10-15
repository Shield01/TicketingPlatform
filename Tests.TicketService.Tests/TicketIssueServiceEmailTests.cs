using Microsoft.Extensions.Logging;
using Moq;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Modules.TicketService.Services;
using Shared.Kernel.Interfaces;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for TicketIssueService email integration.
    /// </summary>
    public class TicketIssueServiceEmailTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepository;
        private readonly Mock<IQRCodeService> _mockQRCodeService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IUserInfoService> _mockUserInfoService;
        private readonly Mock<IEventInfoService> _mockEventInfoService;
        private readonly Mock<IEventMinimumPriceService> _mockEventMinimumPriceService;
        private readonly Mock<ILogger<TicketIssueService>> _mockLogger;
        private readonly TicketIssueService _ticketIssueService;

        public TicketIssueServiceEmailTests()
        {
            _mockTicketRepository = new Mock<ITicketRepository>();
            _mockQRCodeService = new Mock<IQRCodeService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockUserInfoService = new Mock<IUserInfoService>();
            _mockEventInfoService = new Mock<IEventInfoService>();
            _mockEventMinimumPriceService = new Mock<IEventMinimumPriceService>();
            _mockLogger = new Mock<ILogger<TicketIssueService>>();

            // Setup default behavior for minimum price service
            _mockEventMinimumPriceService.Setup(x => x.RecalculateAndUpdateMinimumPriceAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid eventId) => (decimal?)100m);

            _ticketIssueService = new TicketIssueService(
                _mockTicketRepository.Object,
                _mockQRCodeService.Object,
                _mockEmailService.Object,
                _mockUserInfoService.Object,
                _mockEventInfoService.Object,
                _mockEventMinimumPriceService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithEmailService_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_ticketIssueService);
        }

        [Fact]
        public async Task IssueTicketsAsync_WithValidRequest_ShouldTriggerEmailSending()
        {
            // Arrange
            var request = CreateSampleIssueTicketRequest();
            var ticketTier = CreateSampleTicketTier();
            var issuedTickets = CreateSampleIssuedTickets(request);

            SetupMockRepositoryForSuccessfulIssuance(request, ticketTier, issuedTickets);
            SetupMockQRCodeService();
            SetupMockEmailService();

            // Act
            var result = await _ticketIssueService.IssueTicketsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Quantity, result.TicketsIssued);
            
            // Wait a bit for the fire-and-forget email task to start
            await Task.Delay(500);
            
            // Verify email service was called (fire-and-forget task)
            _mockEmailService.Verify(
                x => x.SendMultipleTicketConfirmationEmailsAsync(
                    It.IsAny<IEnumerable<TicketResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task IssueTicketsAsync_WithSingleTicket_ShouldTriggerSingleTicketEmail()
        {
            // Arrange
            var request = CreateSampleIssueTicketRequest();
            request.Quantity = 1; // Single ticket
            var ticketTier = CreateSampleTicketTier();
            var issuedTickets = CreateSampleIssuedTickets(request);

            SetupMockRepositoryForSuccessfulIssuance(request, ticketTier, issuedTickets);
            SetupMockQRCodeService();
            SetupMockEmailService();

            // Act
            var result = await _ticketIssueService.IssueTicketsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TicketsIssued);
            
            // Wait a bit for the fire-and-forget email task to start
            await Task.Delay(500);
            
            // Verify email service was called for single ticket
            _mockEmailService.Verify(
                x => x.SendTicketConfirmationEmailAsync(
                    It.IsAny<TicketResponse>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task IssueTicketsAsync_WithEmailServiceFailure_ShouldNotAffectTicketIssuance()
        {
            // Arrange
            var request = CreateSampleIssueTicketRequest();
            var ticketTier = CreateSampleTicketTier();
            var issuedTickets = CreateSampleIssuedTickets(request);

            SetupMockRepositoryForSuccessfulIssuance(request, ticketTier, issuedTickets);
            SetupMockQRCodeService();
            
            // Setup email service to fail
            _mockEmailService.Setup(x => x.SendMultipleTicketConfirmationEmailsAsync(
                It.IsAny<IEnumerable<TicketResponse>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _ticketIssueService.IssueTicketsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Quantity, result.TicketsIssued);
            Assert.True(result.TicketsIssued > 0); // Ticket issuance should succeed even if email fails
        }

        [Fact]
        public async Task IssueTicketsAsync_WithEmailServiceException_ShouldNotAffectTicketIssuance()
        {
            // Arrange
            var request = CreateSampleIssueTicketRequest();
            var ticketTier = CreateSampleTicketTier();
            var issuedTickets = CreateSampleIssuedTickets(request);

            SetupMockRepositoryForSuccessfulIssuance(request, ticketTier, issuedTickets);
            SetupMockQRCodeService();
            
            // Setup email service to throw exception
            _mockEmailService.Setup(x => x.SendMultipleTicketConfirmationEmailsAsync(
                It.IsAny<IEnumerable<TicketResponse>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service error"));

            // Act
            var result = await _ticketIssueService.IssueTicketsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Quantity, result.TicketsIssued);
            Assert.True(result.TicketsIssued > 0); // Ticket issuance should succeed even if email fails
        }

        [Fact]
        public async Task IssueTicketsAsync_WithTicketIssuanceFailure_ShouldNotTriggerEmail()
        {
            // Arrange
            var request = CreateSampleIssueTicketRequest();
            var ticketTier = CreateSampleTicketTier();

            // Setup repository to fail ticket issuance
            _mockTicketRepository.Setup(x => x.ValidatePaymentForTicketIssuanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _ticketIssueService.IssueTicketsAsync(request));

            // Verify email service was never called
            _mockEmailService.Verify(
                x => x.SendMultipleTicketConfirmationEmailsAsync(
                    It.IsAny<IEnumerable<TicketResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        // Note: SendTicketConfirmationEmailAsync is a private method, so we test it indirectly through IssueTicketsAsync

        [Fact]
        public async Task SendTicketConfirmationEmailAsync_WithValidTickets_ShouldUseRealUserData()
        {
            // Arrange
            var request = CreateSampleIssueTicketRequest();
            var ticketTier = CreateSampleTicketTier();
            var issuedTickets = CreateSampleIssuedTickets(request);

            SetupMockRepositoryForSuccessfulIssuance(request, ticketTier, issuedTickets);
            SetupMockQRCodeService();
            SetupMockEmailService();

            // Act
            var result = await _ticketIssueService.IssueTicketsAsync(request);

            // Assert
            Assert.NotNull(result);
            
            // Wait a bit for the fire-and-forget email task to start
            await Task.Delay(500);
            
            // Verify email service was called with real user data from UserInfoService
            _mockEmailService.Verify(
                x => x.SendMultipleTicketConfirmationEmailsAsync(
                    It.IsAny<IEnumerable<TicketResponse>>(),
                    "test@example.com", // Real email from UserInfoService mock
                    "Test User", // Real name from UserInfoService mock
                    "Test Event"), // Real event name from EventInfoService mock
                Times.Once);
        }

        private void SetupMockRepositoryForSuccessfulIssuance(IssueTicketRequest request, TicketTier ticketTier, List<Ticket> issuedTickets)
        {
            _mockTicketRepository.Setup(x => x.ValidatePaymentForTicketIssuanceAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(x => x.ValidateTicketTierCapacityAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(x => x.GetTicketTierAsync(It.IsAny<Guid>()))
                .ReturnsAsync(ticketTier);
            _mockTicketRepository.Setup(x => x.IssueMultipleTicketsAsync(It.IsAny<List<Ticket>>()))
                .ReturnsAsync(issuedTickets);
            _mockTicketRepository.Setup(x => x.UpdateTicketTierSoldQuantityAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockTicketRepository.Setup(x => x.TicketCodeExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            
            // Setup new service mocks
            _mockUserInfoService.Setup(u => u.GetUserInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Shared.Kernel.Interfaces.UserInfo { Id = request.UserId, Email = "test@example.com", FirstName = "Test", LastName = "User" });
            _mockEventInfoService.Setup(e => e.GetEventInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Shared.Kernel.Interfaces.EventInfo { Id = request.EventId, Title = "Test Event" });
        }

        private void SetupMockQRCodeService()
        {
            _mockQRCodeService.Setup(x => x.GenerateJWTLikeQRData(It.IsAny<Ticket>()))
                .Returns("test-qr-data");
            _mockQRCodeService.Setup(x => x.GenerateQRCodeImage(It.IsAny<string>()))
                .Returns("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
        }

        private void SetupMockEmailService()
        {
            _mockEmailService.Setup(x => x.SendTicketConfirmationEmailAsync(
                It.IsAny<TicketResponse>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockEmailService.Setup(x => x.SendMultipleTicketConfirmationEmailsAsync(
                It.IsAny<IEnumerable<TicketResponse>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(true);
        }

        private static IssueTicketRequest CreateSampleIssueTicketRequest()
        {
            return new IssueTicketRequest
            {
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 100.00m,
                Currency = "USD",
                Quantity = 2,
                PaymentId = Guid.NewGuid()
            };
        }

        private static TicketTier CreateSampleTicketTier()
        {
            return new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "VIP",
                Description = "VIP Ticket",
                Price = 100.00m,
                Currency = "USD",
                MaxQuantity = 100,
                SoldQuantity = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static List<Ticket> CreateSampleIssuedTickets(IssueTicketRequest request)
        {
            var tickets = new List<Ticket>();
            for (int i = 0; i < request.Quantity; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    EventId = request.EventId,
                    UserId = request.UserId,
                    TicketTierId = request.TicketTierId,
                    Price = request.Price,
                    Currency = request.Currency,
                    TicketCode = $"TKT-20241201-ABC{i:D5}",
                    QRCodeData = "test-qr-data",
                    Status = Ticket.TicketStatus.Unused,
                    PaymentId = request.PaymentId,
                    IsUsed = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            return tickets;
        }
    }
}
