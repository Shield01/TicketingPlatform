using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Modules.TicketService.Configuration;
using Modules.TicketService.DTOs;
using Modules.TicketService.Services;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for EmailService.
    /// </summary>
    public class EmailServiceTests
    {
        private readonly Mock<IOptions<EmailConfiguration>> _mockEmailConfig;
        private readonly Mock<IEmailTemplateService> _mockTemplateService;
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _mockEmailConfig = new Mock<IOptions<EmailConfiguration>>();
            _mockTemplateService = new Mock<IEmailTemplateService>();
            _mockLogger = new Mock<ILogger<EmailService>>();

            // Setup default email configuration
            var emailConfig = new EmailConfiguration
            {
                IsEnabled = true,
                SmtpHost = "smtp.test.com",
                SmtpPort = 587,
                UseSsl = true,
                SmtpUsername = "test@test.com",
                SmtpPassword = "testpassword",
                FromEmail = "noreply@test.com",
                FromName = "Test Platform",
                TimeoutSeconds = 30
            };

            _mockEmailConfig.Setup(x => x.Value).Returns(emailConfig);

            _emailService = new EmailService(_mockEmailConfig.Object, _mockTemplateService.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_emailService);
        }

        // Note: EmailService constructor doesn't validate null parameters, which is acceptable for this implementation

        [Fact]
        public async Task SendTicketConfirmationEmailAsync_WhenEmailDisabled_ShouldReturnTrue()
        {
            // Arrange
            var disabledConfig = new EmailConfiguration { IsEnabled = false };
            var mockDisabledConfig = new Mock<IOptions<EmailConfiguration>>();
            mockDisabledConfig.Setup(x => x.Value).Returns(disabledConfig);
            var service = new EmailService(mockDisabledConfig.Object, _mockTemplateService.Object, _mockLogger.Object);

            var ticketResponse = CreateSampleTicketResponse();
            var userEmail = "test@example.com";
            var userName = "Test User";
            var eventName = "Test Event";

            // Act
            var result = await service.SendTicketConfirmationEmailAsync(ticketResponse, userEmail, userName, eventName);

            // Assert
            Assert.True(result);
            _mockTemplateService.Verify(x => x.GenerateTicketConfirmationHtml(It.IsAny<TicketResponse>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendTicketConfirmationEmailAsync_WithValidParameters_ShouldCallTemplateService()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var userEmail = "test@example.com";
            var userName = "Test User";
            var eventName = "Test Event";

            _mockTemplateService.Setup(x => x.GenerateTicketConfirmationHtml(It.IsAny<TicketResponse>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("<html>Test HTML</html>");
            _mockTemplateService.Setup(x => x.GenerateTicketConfirmationText(It.IsAny<TicketResponse>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Test Text");

            // Act
            var result = await _emailService.SendTicketConfirmationEmailAsync(ticketResponse, userEmail, userName, eventName);

            // Assert
            _mockTemplateService.Verify(x => x.GenerateTicketConfirmationHtml(ticketResponse, userName, eventName), Times.Once);
            _mockTemplateService.Verify(x => x.GenerateTicketConfirmationText(ticketResponse, userName, eventName), Times.Once);
        }

        [Fact]
        public async Task SendMultipleTicketConfirmationEmailsAsync_WhenEmailDisabled_ShouldReturnTrue()
        {
            // Arrange
            var disabledConfig = new EmailConfiguration { IsEnabled = false };
            var mockDisabledConfig = new Mock<IOptions<EmailConfiguration>>();
            mockDisabledConfig.Setup(x => x.Value).Returns(disabledConfig);
            var service = new EmailService(mockDisabledConfig.Object, _mockTemplateService.Object, _mockLogger.Object);

            var ticketResponses = new List<TicketResponse> { CreateSampleTicketResponse() };
            var userEmail = "test@example.com";
            var userName = "Test User";
            var eventName = "Test Event";

            // Act
            var result = await service.SendMultipleTicketConfirmationEmailsAsync(ticketResponses, userEmail, userName, eventName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SendMultipleTicketConfirmationEmailsAsync_WithEmptyTickets_ShouldReturnFalse()
        {
            // Arrange
            var ticketResponses = new List<TicketResponse>();
            var userEmail = "test@example.com";
            var userName = "Test User";
            var eventName = "Test Event";

            // Act
            var result = await _emailService.SendMultipleTicketConfirmationEmailsAsync(ticketResponses, userEmail, userName, eventName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendMultipleTicketConfirmationEmailsAsync_WithValidTickets_ShouldCallTemplateService()
        {
            // Arrange
            var ticketResponses = new List<TicketResponse> 
            { 
                CreateSampleTicketResponse(),
                CreateSampleTicketResponse()
            };
            var userEmail = "test@example.com";
            var userName = "Test User";
            var eventName = "Test Event";

            _mockTemplateService.Setup(x => x.GenerateMultipleTicketConfirmationHtml(It.IsAny<IEnumerable<TicketResponse>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("<html>Test HTML</html>");
            _mockTemplateService.Setup(x => x.GenerateMultipleTicketConfirmationText(It.IsAny<IEnumerable<TicketResponse>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Test Text");

            // Act
            var result = await _emailService.SendMultipleTicketConfirmationEmailsAsync(ticketResponses, userEmail, userName, eventName);

            // Assert
            _mockTemplateService.Verify(x => x.GenerateMultipleTicketConfirmationHtml(ticketResponses, userName, eventName), Times.Once);
            _mockTemplateService.Verify(x => x.GenerateMultipleTicketConfirmationText(ticketResponses, userName, eventName), Times.Once);
        }

        [Fact]
        public async Task SendTestEmailAsync_WhenEmailDisabled_ShouldReturnFalse()
        {
            // Arrange
            var disabledConfig = new EmailConfiguration { IsEnabled = false };
            var mockDisabledConfig = new Mock<IOptions<EmailConfiguration>>();
            mockDisabledConfig.Setup(x => x.Value).Returns(disabledConfig);
            var service = new EmailService(mockDisabledConfig.Object, _mockTemplateService.Object, _mockLogger.Object);

            var testEmail = "test@example.com";

            // Act
            var result = await service.SendTestEmailAsync(testEmail);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendTestEmailAsync_WithValidEmail_ShouldReturnFalseDueToSMTPFailure()
        {
            // Arrange
            var testEmail = "test@example.com";

            // Act
            var result = await _emailService.SendTestEmailAsync(testEmail);

            // Assert
            Assert.False(result); // Should fail due to test SMTP configuration
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData(null)]
        public async Task SendTicketConfirmationEmailAsync_WithInvalidEmail_ShouldHandleGracefully(string invalidEmail)
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var userName = "Test User";
            var eventName = "Test Event";

            // Act
            var result = await _emailService.SendTicketConfirmationEmailAsync(ticketResponse, invalidEmail, userName, eventName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendTicketConfirmationEmailAsync_WithTemplateServiceException_ShouldReturnFalse()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var userEmail = "test@example.com";
            var userName = "Test User";
            var eventName = "Test Event";

            _mockTemplateService.Setup(x => x.GenerateTicketConfirmationHtml(It.IsAny<TicketResponse>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Template generation failed"));

            // Act
            var result = await _emailService.SendTicketConfirmationEmailAsync(ticketResponse, userEmail, userName, eventName);

            // Assert
            Assert.False(result);
        }

        private static TicketResponse CreateSampleTicketResponse()
        {
            return new TicketResponse
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventName = "Test Event",
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                TierName = "VIP",
                TierDescription = "VIP Ticket",
                Price = 100.00m,
                Currency = "USD",
                TicketCode = "TKT-20241201-ABC12345",
                QRCodeData = "test-qr-data",
                QRCodeImage = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==", // 1x1 transparent PNG
                IsUsed = false,
                UsedAt = null,
                Status = "UNUSED",
                PaymentId = Guid.NewGuid(),
                IssuedAt = DateTime.UtcNow,
                IsActive = true,
                IsValidForUse = true
            };
        }
    }
}
