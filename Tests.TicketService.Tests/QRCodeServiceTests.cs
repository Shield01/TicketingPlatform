using Microsoft.Extensions.Logging;
using Moq;
using Modules.TicketService.Models;
using Modules.TicketService.Services;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for QRCodeService functionality.
    /// </summary>
    public class QRCodeServiceTests
    {
        private readonly Mock<ILogger<QRCodeService>> _mockLogger;
        private readonly QRCodeService _qrCodeService;

        public QRCodeServiceTests()
        {
            _mockLogger = new Mock<ILogger<QRCodeService>>();
            _qrCodeService = new QRCodeService(_mockLogger.Object);
        }

        [Fact]
        public void GenerateJWTLikeQRData_WithValidTicket_ReturnsValidJWTStructure()
        {
            // Arrange
            var ticket = CreateTestTicket();

            // Act
            var qrData = _qrCodeService.GenerateJWTLikeQRData(ticket);

            // Assert
            Assert.NotNull(qrData);
            Assert.NotEmpty(qrData);
            
            // JWT structure should have 3 parts separated by dots
            var parts = qrData.Split('.');
            Assert.Equal(3, parts.Length);
            Assert.NotNull(parts[0]); // Header
            Assert.NotNull(parts[1]); // Payload
            Assert.NotNull(parts[2]); // Signature
        }

        [Fact]
        public void GenerateJWTLikeQRData_WithValidTicket_ContainsTicketInformation()
        {
            // Arrange
            var ticket = CreateTestTicket();

            // Act
            var qrData = _qrCodeService.GenerateJWTLikeQRData(ticket);

            // Assert
            var parts = qrData.Split('.');
            var payload = parts[1];
            
            // Decode base64 URL payload
            var payloadJson = Base64UrlDecode(payload);
            Assert.Contains(ticket.Id.ToString(), payloadJson);
            Assert.Contains(ticket.TicketCode, payloadJson);
            Assert.Contains(ticket.EventId.ToString(), payloadJson);
            Assert.Contains(ticket.UserId.ToString(), payloadJson);
            Assert.Contains(ticket.TicketTierId.ToString(), payloadJson);
            Assert.Contains(ticket.Status, payloadJson);
        }

        [Fact]
        public void ValidateAndExtractQRData_WithValidQRData_ReturnsTicketInformation()
        {
            // Arrange
            var ticket = CreateTestTicket();
            var qrData = _qrCodeService.GenerateJWTLikeQRData(ticket);

            // Act
            var extractedData = _qrCodeService.ValidateAndExtractQRData(qrData);

            // Assert
            Assert.NotNull(extractedData);
            Assert.Equal(ticket.Id.ToString(), extractedData["ticketId"]);
            Assert.Equal(ticket.TicketCode, extractedData["ticketCode"]);
            Assert.Equal(ticket.EventId.ToString(), extractedData["eventId"]);
            Assert.Equal(ticket.UserId.ToString(), extractedData["userId"]);
            Assert.Equal(ticket.TicketTierId.ToString(), extractedData["ticketTierId"]);
            Assert.Equal(ticket.Status, extractedData["status"]);
        }

        [Fact]
        public void ValidateAndExtractQRData_WithInvalidQRData_ReturnsNull()
        {
            // Arrange
            var invalidQrData = "invalid.qr.data";

            // Act
            var extractedData = _qrCodeService.ValidateAndExtractQRData(invalidQrData);

            // Assert
            Assert.Null(extractedData);
        }

        [Fact]
        public void ValidateAndExtractQRData_WithEmptyQRData_ReturnsNull()
        {
            // Arrange
            var emptyQrData = string.Empty;

            // Act
            var extractedData = _qrCodeService.ValidateAndExtractQRData(emptyQrData);

            // Assert
            Assert.Null(extractedData);
        }

        [Fact]
        public void ValidateAndExtractQRData_WithNullQRData_ReturnsNull()
        {
            // Arrange
            string? nullQrData = null;

            // Act
            var extractedData = _qrCodeService.ValidateAndExtractQRData(nullQrData!);

            // Assert
            Assert.Null(extractedData);
        }

        [Fact]
        public void GenerateQRCodeImage_WithValidTicket_ReturnsBase64String()
        {
            // Arrange
            var ticket = CreateTestTicket();

            // Act
            var qrCodeImage = _qrCodeService.GenerateQRCodeImage(ticket);

            // Assert
            Assert.NotNull(qrCodeImage);
            Assert.NotEmpty(qrCodeImage);
            
            // Should be a valid base64 string
            var bytes = Convert.FromBase64String(qrCodeImage);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void GenerateQRCodeImage_WithValidQRData_ReturnsBase64String()
        {
            // Arrange
            var qrData = "test.qr.data";

            // Act
            var qrCodeImage = _qrCodeService.GenerateQRCodeImage(qrData);

            // Assert
            Assert.NotNull(qrCodeImage);
            Assert.NotEmpty(qrCodeImage);
            
            // Should be a valid base64 string
            var bytes = Convert.FromBase64String(qrCodeImage);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void GenerateQRCodeBytes_WithValidTicket_ReturnsByteArray()
        {
            // Arrange
            var ticket = CreateTestTicket();

            // Act
            var qrCodeBytes = _qrCodeService.GenerateQRCodeBytes(ticket);

            // Assert
            Assert.NotNull(qrCodeBytes);
            Assert.True(qrCodeBytes.Length > 0);
        }

        [Fact]
        public void GenerateQRCodeBytes_WithValidQRData_ReturnsByteArray()
        {
            // Arrange
            var qrData = "test.qr.data";

            // Act
            var qrCodeBytes = _qrCodeService.GenerateQRCodeBytes(qrData);

            // Assert
            Assert.NotNull(qrCodeBytes);
            Assert.True(qrCodeBytes.Length > 0);
        }

        [Fact]
        public void GenerateJWTLikeQRData_WithDifferentTickets_ReturnsDifferentQRData()
        {
            // Arrange
            var ticket1 = CreateTestTicket();
            var ticket2 = CreateTestTicket();
            ticket2.Id = Guid.NewGuid();
            ticket2.TicketCode = "TKT-20241201-DIFFERENT";

            // Act
            var qrData1 = _qrCodeService.GenerateJWTLikeQRData(ticket1);
            var qrData2 = _qrCodeService.GenerateJWTLikeQRData(ticket2);

            // Assert
            Assert.NotEqual(qrData1, qrData2);
        }

        [Fact]
        public void ValidateAndExtractQRData_WithExpiredTicket_ReturnsNull()
        {
            // Arrange
            var ticket = CreateTestTicket();
            ticket.CreatedAt = DateTime.UtcNow.AddYears(-2); // Expired ticket
            var qrData = _qrCodeService.GenerateJWTLikeQRData(ticket);

            // Act
            var extractedData = _qrCodeService.ValidateAndExtractQRData(qrData);

            // Assert
            Assert.Null(extractedData);
        }

        [Fact]
        public void ValidateAndExtractQRData_WithTamperedQRData_ReturnsNull()
        {
            // Arrange
            var ticket = CreateTestTicket();
            var qrData = _qrCodeService.GenerateJWTLikeQRData(ticket);
            
            // Tamper with the signature
            var parts = qrData.Split('.');
            parts[2] = "tampered_signature";
            var tamperedQrData = string.Join(".", parts);

            // Act
            var extractedData = _qrCodeService.ValidateAndExtractQRData(tamperedQrData);

            // Assert
            Assert.Null(extractedData);
        }

        [Fact]
        public void GenerateJWTLikeQRData_WithNullTicket_ThrowsException()
        {
            // Arrange
            Ticket? nullTicket = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _qrCodeService.GenerateJWTLikeQRData(nullTicket!));
        }

        [Fact]
        public void GenerateQRCodeImage_WithNullTicket_ThrowsException()
        {
            // Arrange
            Ticket? nullTicket = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _qrCodeService.GenerateQRCodeImage(nullTicket!));
        }

        [Fact]
        public void GenerateQRCodeBytes_WithNullTicket_ThrowsException()
        {
            // Arrange
            Ticket? nullTicket = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _qrCodeService.GenerateQRCodeBytes(nullTicket!));
        }

        /// <summary>
        /// Creates a test ticket for unit testing.
        /// </summary>
        /// <returns>A test ticket instance.</returns>
        private static Ticket CreateTestTicket()
        {
            return new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241201-TEST1234",
                Status = Ticket.TicketStatus.Unused,
                IsUsed = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Base64 URL decodes a string.
        /// </summary>
        /// <param name="input">The string to decode.</param>
        /// <returns>Decoded string.</returns>
        private static string Base64UrlDecode(string input)
        {
            var base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
