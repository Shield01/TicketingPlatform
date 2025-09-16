using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.TicketService.Data;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Moq;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for TicketAuditLogRepository.
    /// </summary>
    public class TicketAuditLogRepositoryTests : IDisposable
    {
        private readonly TicketServiceDbContext _context;
        private readonly TicketAuditLogRepository _repository;
        private readonly Mock<ILogger<TicketAuditLogRepository>> _mockLogger;

        public TicketAuditLogRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TicketServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TicketServiceDbContext(options);
            _mockLogger = new Mock<ILogger<TicketAuditLogRepository>>();
            _repository = new TicketAuditLogRepository(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreateAuditLogAsync_ValidAuditLog_ReturnsCreatedAuditLog()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            
            // Create and add a ticket first
            var ticket = new Ticket
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Unused
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var auditLog = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                PreviousStatus = Ticket.TicketStatus.Unused,
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Manual override for testing purposes",
                WasForced = false
            };

            // Act
            var result = await _repository.CreateAuditLogAsync(auditLog);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(auditLog.Id, result.Id);
            Assert.Equal(auditLog.TicketId, result.TicketId);
            Assert.Equal(auditLog.ActionType, result.ActionType);
            Assert.Equal(auditLog.Reason, result.Reason);

            // Verify it was saved to the database
            var savedAuditLog = await _context.TicketAuditLogs.FindAsync(auditLog.Id);
            Assert.NotNull(savedAuditLog);
            Assert.Equal(auditLog.TicketId, savedAuditLog.TicketId);
        }

        [Fact]
        public async Task GetTicketAuditLogsAsync_ExistingTicket_ReturnsAuditLogs()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();

            // Create and add a ticket first
            var ticket = new Ticket
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Unused
            };
            _context.Tickets.Add(ticket);

            var auditLog1 = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                PreviousStatus = Ticket.TicketStatus.Unused,
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "First override",
                PerformedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var auditLog2 = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.Reset,
                PreviousStatus = Ticket.TicketStatus.Used,
                NewStatus = Ticket.TicketStatus.Unused,
                Reason = "Reset for customer",
                PerformedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _context.TicketAuditLogs.AddRange(auditLog1, auditLog2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketAuditLogsAsync(ticketId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            // Should be ordered by PerformedAt descending (most recent first)
            Assert.Equal(auditLog2.Id, result[0].Id);
            Assert.Equal(auditLog1.Id, result[1].Id);
        }

        [Fact]
        public async Task GetTicketAuditLogsAsync_NonExistentTicket_ReturnsEmptyList()
        {
            // Arrange
            var nonExistentTicketId = Guid.NewGuid();

            // Act
            var result = await _repository.GetTicketAuditLogsAsync(nonExistentTicketId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserAuditLogsAsync_ExistingUser_ReturnsPagedResults()
        {
            // Arrange
            var operatorUserId = Guid.NewGuid();
            var ticketId1 = Guid.NewGuid();
            var ticketId2 = Guid.NewGuid();

            // Create tickets first
            var ticket1 = new Ticket
            {
                Id = ticketId1,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Unused
            };

            var ticket2 = new Ticket
            {
                Id = ticketId2,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 75.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-IJKLMNOP",
                Status = Ticket.TicketStatus.Unused
            };

            _context.Tickets.AddRange(ticket1, ticket2);

            var auditLog1 = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId1,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                PreviousStatus = Ticket.TicketStatus.Unused,
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Override 1",
                PerformedAt = DateTime.UtcNow.AddMinutes(-20)
            };

            var auditLog2 = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId2,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.Reset,
                PreviousStatus = Ticket.TicketStatus.Used,
                NewStatus = Ticket.TicketStatus.Unused,
                Reason = "Override 2",
                PerformedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var auditLog3 = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId1,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.ForceRedeem,
                PreviousStatus = Ticket.TicketStatus.Unused,
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Override 3",
                PerformedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _context.TicketAuditLogs.AddRange(auditLog1, auditLog2, auditLog3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserAuditLogsAsync(operatorUserId, page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            // Should be ordered by PerformedAt descending (most recent first)
            Assert.Equal(auditLog3.Id, result[0].Id);
            Assert.Equal(auditLog2.Id, result[1].Id);
        }

        [Fact]
        public async Task GetTicketAuditLogCountAsync_ExistingTicket_ReturnsCorrectCount()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();

            // Create and add a ticket first
            var ticket = new Ticket
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Unused
            };
            _context.Tickets.Add(ticket);

            // Add multiple audit logs for the same ticket
            for (int i = 0; i < 5; i++)
            {
                var auditLog = new TicketAuditLog
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    PerformedByUserId = operatorUserId,
                    ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                    PreviousStatus = Ticket.TicketStatus.Unused,
                    NewStatus = Ticket.TicketStatus.Used,
                    Reason = $"Override {i + 1}",
                    PerformedAt = DateTime.UtcNow.AddMinutes(-i)
                };
                _context.TicketAuditLogs.Add(auditLog);
            }

            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketAuditLogCountAsync(ticketId);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task GetAuditLogsByDateRangeAsync_WithinRange_ReturnsFilteredResults()
        {
            // Arrange
            var operatorUserId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-2);
            var endDate = DateTime.UtcNow;

            // Create and add a ticket first
            var ticket = new Ticket
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Unused
            };
            _context.Tickets.Add(ticket);

            // Audit log within range
            var auditLogInRange = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                PreviousStatus = Ticket.TicketStatus.Unused,
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Within range",
                PerformedAt = DateTime.UtcNow.AddDays(-1)
            };

            // Audit log outside range
            var auditLogOutsideRange = new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                PerformedByUserId = operatorUserId,
                ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                PreviousStatus = Ticket.TicketStatus.Unused,
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Outside range",
                PerformedAt = DateTime.UtcNow.AddDays(-5)
            };

            _context.TicketAuditLogs.AddRange(auditLogInRange, auditLogOutsideRange);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAuditLogsByDateRangeAsync(startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(auditLogInRange.Id, result[0].Id);
        }
    }
}
