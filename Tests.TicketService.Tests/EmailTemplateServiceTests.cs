using Modules.TicketService.DTOs;
using Modules.TicketService.Services;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for EmailTemplateService.
    /// </summary>
    public class EmailTemplateServiceTests
    {
        private readonly EmailTemplateService _templateService;

        public EmailTemplateServiceTests()
        {
            _templateService = new EmailTemplateService();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_templateService);
        }

        [Fact]
        public void GenerateTicketConfirmationHtml_WithValidParameters_ShouldReturnValidHtml()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationHtml(ticketResponse, userName, eventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("<!DOCTYPE html>", result);
            Assert.Contains("<html", result);
            Assert.Contains("</html>", result);
            Assert.Contains(userName, result);
            Assert.Contains(eventName, result);
            Assert.Contains(ticketResponse.TicketCode, result);
            Assert.Contains(ticketResponse.TierName, result);
            Assert.Contains(ticketResponse.Price.ToString("F2"), result);
            Assert.Contains(ticketResponse.Currency, result);
        }

        [Fact]
        public void GenerateTicketConfirmationHtml_WithQRCode_ShouldIncludeQRCodeImage()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationHtml(ticketResponse, userName, eventName);

            // Assert
            Assert.Contains("data:image/png;base64,", result);
            Assert.Contains(ticketResponse.QRCodeImage!, result);
            Assert.Contains("Present this QR code at the event entrance", result);
        }

        [Fact]
        public void GenerateTicketConfirmationHtml_WithoutQRCode_ShouldNotIncludeQRCodeImage()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            ticketResponse.QRCodeImage = null;
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationHtml(ticketResponse, userName, eventName);

            // Assert
            Assert.DoesNotContain("data:image/png;base64,", result);
            Assert.DoesNotContain("Present this QR code at the event entrance", result);
        }

        [Fact]
        public void GenerateTicketConfirmationHtml_WithTierDescription_ShouldIncludeDescription()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            ticketResponse.TierDescription = "Premium access with VIP benefits";
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationHtml(ticketResponse, userName, eventName);

            // Assert
            Assert.Contains("Description:", result);
            Assert.Contains(ticketResponse.TierDescription, result);
        }

        [Fact]
        public void GenerateTicketConfirmationHtml_WithoutTierDescription_ShouldNotIncludeDescription()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            ticketResponse.TierDescription = null;
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationHtml(ticketResponse, userName, eventName);

            // Assert
            Assert.DoesNotContain("Description:", result);
        }

        [Fact]
        public void GenerateMultipleTicketConfirmationHtml_WithValidParameters_ShouldReturnValidHtml()
        {
            // Arrange
            var ticketResponses = new List<TicketResponse>
            {
                CreateSampleTicketResponse(),
                CreateSampleTicketResponse()
            };
            ticketResponses[1].TicketCode = "TKT-20241201-DEF67890";
            ticketResponses[1].Price = 50.00m;
            
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateMultipleTicketConfirmationHtml(ticketResponses, userName, eventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("<!DOCTYPE html>", result);
            Assert.Contains("<html", result);
            Assert.Contains("</html>", result);
            Assert.Contains(userName, result);
            Assert.Contains(eventName, result);
            Assert.Contains("2 Tickets Confirmed", result);
            Assert.Contains(ticketResponses[0].TicketCode, result);
            Assert.Contains(ticketResponses[1].TicketCode, result);
            Assert.Contains("150.00", result); // Total price
        }

        [Fact]
        public void GenerateMultipleTicketConfirmationHtml_WithEmptyList_ShouldReturnValidHtml()
        {
            // Arrange
            var ticketResponses = new List<TicketResponse>();
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateMultipleTicketConfirmationHtml(ticketResponses, userName, eventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("0.00", result); // Total price should be 0
        }

        [Fact]
        public void GenerateTicketConfirmationText_WithValidParameters_ShouldReturnValidText()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationText(ticketResponse, userName, eventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("TICKET CONFIRMATION", result);
            Assert.Contains(userName, result);
            Assert.Contains(eventName, result);
            Assert.Contains(ticketResponse.TicketCode, result);
            Assert.Contains(ticketResponse.TierName, result);
            Assert.Contains(ticketResponse.Price.ToString("F2"), result);
            Assert.Contains(ticketResponse.Currency, result);
            Assert.Contains("IMPORTANT INFORMATION:", result);
        }

        [Fact]
        public void GenerateTicketConfirmationText_WithTierDescription_ShouldIncludeDescription()
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            ticketResponse.TierDescription = "Premium access with VIP benefits";
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationText(ticketResponse, userName, eventName);

            // Assert
            Assert.Contains("Description:", result);
            Assert.Contains(ticketResponse.TierDescription, result);
        }

        [Fact]
        public void GenerateMultipleTicketConfirmationText_WithValidParameters_ShouldReturnValidText()
        {
            // Arrange
            var ticketResponses = new List<TicketResponse>
            {
                CreateSampleTicketResponse(),
                CreateSampleTicketResponse()
            };
            ticketResponses[1].TicketCode = "TKT-20241201-DEF67890";
            ticketResponses[1].Price = 50.00m;
            
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateMultipleTicketConfirmationText(ticketResponses, userName, eventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("MULTIPLE TICKETS CONFIRMATION", result);
            Assert.Contains(userName, result);
            Assert.Contains(eventName, result);
            Assert.Contains("2 ticket(s)", result);
            Assert.Contains(ticketResponses[0].TicketCode, result);
            Assert.Contains(ticketResponses[1].TicketCode, result);
            Assert.Contains("150.00", result); // Total price
            Assert.Contains("TICKET #1:", result);
            Assert.Contains("TICKET #2:", result);
        }

        [Fact]
        public void GenerateMultipleTicketConfirmationText_WithEmptyList_ShouldReturnValidText()
        {
            // Arrange
            var ticketResponses = new List<TicketResponse>();
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateMultipleTicketConfirmationText(ticketResponses, userName, eventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("0.00", result); // Total price should be 0
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void GenerateTicketConfirmationHtml_WithInvalidUserName_ShouldHandleGracefully(string invalidUserName)
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var eventName = "Tech Conference 2024";

            // Act
            var result = _templateService.GenerateTicketConfirmationHtml(ticketResponse, invalidUserName, eventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("<!DOCTYPE html>", result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void GenerateTicketConfirmationHtml_WithInvalidEventName_ShouldHandleGracefully(string invalidEventName)
        {
            // Arrange
            var ticketResponse = CreateSampleTicketResponse();
            var userName = "John Doe";

            // Act
            var result = _templateService.GenerateTicketConfirmationHtml(ticketResponse, userName, invalidEventName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("<!DOCTYPE html>", result);
        }

        [Fact]
        public void GenerateTicketConfirmationHtml_WithNullTicketResponse_ShouldThrowException()
        {
            // Arrange
            TicketResponse? ticketResponse = null;
            var userName = "John Doe";
            var eventName = "Tech Conference 2024";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => 
                _templateService.GenerateTicketConfirmationHtml(ticketResponse!, userName, eventName));
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
