using Modules.TicketService.Models;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for the Ticket model.
    /// </summary>
    public class TicketModelTests
    {
        [Fact]
        public void GenerateTicketCode_ShouldReturnValidFormat()
        {
            // Act
            var ticketCode = Ticket.GenerateTicketCode();

            // Assert
            Assert.NotNull(ticketCode);
            Assert.StartsWith("TKT-", ticketCode);
            Assert.Contains(DateTime.UtcNow.ToString("yyyyMMdd"), ticketCode);
            Assert.Equal(21, ticketCode.Length); // TKT-YYYYMMDD-XXXXXXXX format
        }

        [Fact]
        public void GenerateTicketCode_ShouldReturnUniqueValues()
        {
            // Act
            var code1 = Ticket.GenerateTicketCode();
            var code2 = Ticket.GenerateTicketCode();

            // Assert
            Assert.NotEqual(code1, code2);
        }

        [Fact]
        public void GenerateQRCodeData_ShouldContainTicketInformation()
        {
            // Arrange
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                TicketCode = "TKT-20241201-TEST1234",
                Status = Ticket.TicketStatus.Unused,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var qrData = ticket.GenerateQRCodeData();

            // Assert
            Assert.NotNull(qrData);
            Assert.Contains($"TICKET:{ticket.TicketCode}", qrData);
            Assert.Contains($"EVENT:{ticket.EventId}", qrData);
            Assert.Contains($"USER:{ticket.UserId}", qrData);
            Assert.Contains($"TIER:{ticket.TicketTierId}", qrData);
            Assert.Contains($"STATUS:{ticket.Status}", qrData);
            Assert.Contains("HASH:", qrData);
        }

        [Fact]
        public void MarkAsUsed_ShouldUpdateTicketProperties()
        {
            // Arrange
            var ticket = new Ticket
            {
                IsUsed = false,
                UsedAt = null,
                Status = Ticket.TicketStatus.Unused
            };

            // Act
            ticket.MarkAsUsed();

            // Assert
            Assert.True(ticket.IsUsed);
            Assert.NotNull(ticket.UsedAt);
            Assert.Equal(Ticket.TicketStatus.Used, ticket.Status);
            Assert.True(ticket.UpdatedAt > DateTime.UtcNow.AddSeconds(-5)); // Recent update
        }

        [Fact]
        public void Cancel_ShouldUpdateStatusAndTimestamp()
        {
            // Arrange
            var ticket = new Ticket
            {
                Status = Ticket.TicketStatus.Unused,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            ticket.Cancel();

            // Assert
            Assert.Equal(Ticket.TicketStatus.Cancelled, ticket.Status);
            Assert.True(ticket.UpdatedAt > DateTime.UtcNow.AddSeconds(-5)); // Recent update
        }

        [Theory]
        [InlineData(Ticket.TicketStatus.Unused, false, true, true)]
        [InlineData(Ticket.TicketStatus.Used, false, true, false)]
        [InlineData(Ticket.TicketStatus.Cancelled, false, true, false)]
        [InlineData(Ticket.TicketStatus.Unused, true, true, false)]
        [InlineData(Ticket.TicketStatus.Unused, false, false, false)]
        public void IsValidForUse_ShouldReturnCorrectResult(string status, bool isUsed, bool isActive, bool expectedResult)
        {
            // Arrange
            var ticket = new Ticket
            {
                Status = status,
                IsUsed = isUsed,
                IsActive = isActive
            };

            // Act
            var result = ticket.IsValidForUse();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ValidateQRCode_ShouldReturnTrue_ForValidQRCode()
        {
            // Arrange
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                TicketCode = "TKT-20241201-TEST1234",
                Status = Ticket.TicketStatus.Unused,
                CreatedAt = DateTime.UtcNow
            };

            var qrData = ticket.GenerateQRCodeData();
            ticket.QRCodeData = qrData;

            // Act
            var result = ticket.ValidateQRCode(qrData);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateQRCode_ShouldReturnFalse_ForInvalidQRCode()
        {
            // Arrange
            var ticket = new Ticket
            {
                QRCodeData = "TICKET:TKT-20241201-TEST1234|EVENT:123|HASH:validhash"
            };

            // Act
            var result = ticket.ValidateQRCode("TICKET:TKT-20241201-FAKE1234|EVENT:123|HASH:invalidhash");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateQRCode_ShouldReturnFalse_ForEmptyQRCode()
        {
            // Arrange
            var ticket = new Ticket();

            // Act
            var result = ticket.ValidateQRCode("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TicketStatus_Constants_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal("UNUSED", Ticket.TicketStatus.Unused);
            Assert.Equal("USED", Ticket.TicketStatus.Used);
            Assert.Equal("CANCELLED", Ticket.TicketStatus.Cancelled);
            Assert.Equal("EXPIRED", Ticket.TicketStatus.Expired);
        }

        [Fact]
        public void NewTicket_ShouldHaveDefaultValues()
        {
            // Act
            var ticket = new Ticket();

            // Assert
            Assert.Equal("USD", ticket.Currency);
            Assert.Equal(Ticket.TicketStatus.Unused, ticket.Status);
            Assert.False(ticket.IsUsed);
            Assert.True(ticket.IsActive);
            Assert.True(ticket.CreatedAt > DateTime.UtcNow.AddSeconds(-5)); // Recent creation
            Assert.True(ticket.UpdatedAt > DateTime.UtcNow.AddSeconds(-5)); // Recent update
        }
    }
}
