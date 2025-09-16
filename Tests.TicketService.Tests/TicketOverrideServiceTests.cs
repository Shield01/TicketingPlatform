using Microsoft.Extensions.Logging;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Modules.TicketService.Services;
using Shared.Kernel.Interfaces;
using Moq;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for TicketOverrideService.
    /// </summary>
    public class TicketOverrideServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepository;
        private readonly Mock<ITicketAuditLogRepository> _mockAuditLogRepository;
        private readonly Mock<IUserInfoService> _mockUserInfoService;
        private readonly Mock<IEventInfoService> _mockEventInfoService;
        private readonly Mock<ILogger<TicketOverrideService>> _mockLogger;
        private readonly TicketOverrideService _service;

        public TicketOverrideServiceTests()
        {
            _mockTicketRepository = new Mock<ITicketRepository>();
            _mockAuditLogRepository = new Mock<ITicketAuditLogRepository>();
            _mockUserInfoService = new Mock<IUserInfoService>();
            _mockEventInfoService = new Mock<IEventInfoService>();
            _mockLogger = new Mock<ILogger<TicketOverrideService>>();
            
            _service = new TicketOverrideService(
                _mockTicketRepository.Object,
                _mockAuditLogRepository.Object,
                _mockUserInfoService.Object,
                _mockEventInfoService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task OverrideTicketStatusAsync_ValidRequest_ReturnsUpdatedTicket()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Manual override for scanning issue",
                ForceOverride = false
            };

            var originalTicket = new Ticket
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Unused,
                IsUsed = false
            };

            var updatedTicket = new Ticket
            {
                Id = ticketId,
                EventId = originalTicket.EventId,
                UserId = originalTicket.UserId,
                TicketTierId = originalTicket.TicketTierId,
                Price = originalTicket.Price,
                Currency = originalTicket.Currency,
                TicketCode = originalTicket.TicketCode,
                Status = Ticket.TicketStatus.Used,
                IsUsed = true,
                UsedAt = DateTime.UtcNow
            };

            var userInfo = new UserInfo { Id = originalTicket.UserId, Email = "user@example.com", FirstName = "John", LastName = "Doe" };
            var eventInfo = new EventInfo { Id = originalTicket.EventId, Title = "Test Event" };

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(originalTicket);
            _mockTicketRepository.Setup(r => r.OverrideTicketStatusAsync(ticketId, request.NewStatus, request.ForceOverride))
                .ReturnsAsync(updatedTicket);
            _mockAuditLogRepository.Setup(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()))
                .ReturnsAsync((TicketAuditLog log) => log);
            _mockUserInfoService.Setup(s => s.GetUserInfoAsync(originalTicket.UserId))
                .ReturnsAsync(userInfo);
            _mockEventInfoService.Setup(s => s.GetEventInfoAsync(originalTicket.EventId))
                .ReturnsAsync(eventInfo);

            // Act
            var result = await _service.OverrideTicketStatusAsync(ticketId, request, operatorUserId, "127.0.0.1", "Test User Agent");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticketId, result.Id);
            Assert.Equal(Ticket.TicketStatus.Used, result.Status);
            Assert.True(result.IsUsed);

            // Verify audit log was created
            _mockAuditLogRepository.Verify(r => r.CreateAuditLogAsync(It.Is<TicketAuditLog>(log =>
                log.TicketId == ticketId &&
                log.PerformedByUserId == operatorUserId &&
                log.PreviousStatus == Ticket.TicketStatus.Unused &&
                log.NewStatus == Ticket.TicketStatus.Used &&
                log.Reason == request.Reason &&
                log.WasForced == request.ForceOverride &&
                log.IpAddress == "127.0.0.1" &&
                log.UserAgent == "Test User Agent"
            )), Times.Once);
        }

        [Fact]
        public async Task OverrideTicketStatusAsync_InvalidStatus_ReturnsNull()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = "INVALID_STATUS",
                Reason = "Test reason",
                ForceOverride = false
            };

            // Act
            var result = await _service.OverrideTicketStatusAsync(ticketId, request, operatorUserId);

            // Assert
            Assert.Null(result);

            // Verify no repository calls were made
            _mockTicketRepository.Verify(r => r.GetTicketByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockAuditLogRepository.Verify(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()), Times.Never);
        }

        [Fact]
        public async Task OverrideTicketStatusAsync_NonExistentTicket_ReturnsNull()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Test reason",
                ForceOverride = false
            };

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync((Ticket?)null);

            // Act
            var result = await _service.OverrideTicketStatusAsync(ticketId, request, operatorUserId);

            // Assert
            Assert.Null(result);

            // Verify no audit log was created
            _mockAuditLogRepository.Verify(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()), Times.Never);
        }

        [Fact]
        public async Task OverrideTicketStatusAsync_RepositoryOverrideFails_ReturnsNull()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Test reason",
                ForceOverride = false
            };

            var originalTicket = new Ticket
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

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(originalTicket);
            _mockTicketRepository.Setup(r => r.OverrideTicketStatusAsync(ticketId, request.NewStatus, request.ForceOverride))
                .ReturnsAsync((Ticket?)null);

            // Act
            var result = await _service.OverrideTicketStatusAsync(ticketId, request, operatorUserId);

            // Assert
            Assert.Null(result);

            // Verify no audit log was created
            _mockAuditLogRepository.Verify(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()), Times.Never);
        }

        [Theory]
        [InlineData(Ticket.TicketStatus.Unused)]
        [InlineData(Ticket.TicketStatus.Used)]
        [InlineData(Ticket.TicketStatus.Cancelled)]
        [InlineData(Ticket.TicketStatus.Expired)]
        public async Task OverrideTicketStatusAsync_ValidStatuses_AcceptsAllValidStatuses(string validStatus)
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = validStatus,
                Reason = "Test reason",
                ForceOverride = false
            };

            var originalTicket = new Ticket
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

            var updatedTicket = new Ticket
            {
                Id = ticketId,
                EventId = originalTicket.EventId,
                UserId = originalTicket.UserId,
                TicketTierId = originalTicket.TicketTierId,
                Price = originalTicket.Price,
                Currency = originalTicket.Currency,
                TicketCode = originalTicket.TicketCode,
                Status = validStatus
            };

            var userInfo = new UserInfo { Id = originalTicket.UserId, Email = "user@example.com", FirstName = "John", LastName = "Doe" };
            var eventInfo = new EventInfo { Id = originalTicket.EventId, Title = "Test Event" };

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(originalTicket);
            _mockTicketRepository.Setup(r => r.OverrideTicketStatusAsync(ticketId, validStatus, request.ForceOverride))
                .ReturnsAsync(updatedTicket);
            _mockAuditLogRepository.Setup(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()))
                .ReturnsAsync((TicketAuditLog log) => log);
            _mockUserInfoService.Setup(s => s.GetUserInfoAsync(originalTicket.UserId))
                .ReturnsAsync(userInfo);
            _mockEventInfoService.Setup(s => s.GetEventInfoAsync(originalTicket.EventId))
                .ReturnsAsync(eventInfo);

            // Act
            var result = await _service.OverrideTicketStatusAsync(ticketId, request, operatorUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(validStatus, result.Status);

            // Verify audit log was created
            _mockAuditLogRepository.Verify(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()), Times.Once);
        }

        [Fact]
        public async Task OverrideTicketStatusAsync_ForceOverrideTrue_CreatesAuditLogWithForceFlag()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = Ticket.TicketStatus.Used,
                Reason = "Force override for emergency",
                ForceOverride = true
            };

            var originalTicket = new Ticket
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = Ticket.TicketStatus.Cancelled // From cancelled to used - normally invalid
            };

            var updatedTicket = new Ticket
            {
                Id = ticketId,
                EventId = originalTicket.EventId,
                UserId = originalTicket.UserId,
                TicketTierId = originalTicket.TicketTierId,
                Price = originalTicket.Price,
                Currency = originalTicket.Currency,
                TicketCode = originalTicket.TicketCode,
                Status = Ticket.TicketStatus.Used
            };

            var userInfo = new UserInfo { Id = originalTicket.UserId, Email = "user@example.com", FirstName = "John", LastName = "Doe" };
            var eventInfo = new EventInfo { Id = originalTicket.EventId, Title = "Test Event" };

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(originalTicket);
            _mockTicketRepository.Setup(r => r.OverrideTicketStatusAsync(ticketId, request.NewStatus, request.ForceOverride))
                .ReturnsAsync(updatedTicket);
            _mockAuditLogRepository.Setup(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()))
                .ReturnsAsync((TicketAuditLog log) => log);
            _mockUserInfoService.Setup(s => s.GetUserInfoAsync(originalTicket.UserId))
                .ReturnsAsync(userInfo);
            _mockEventInfoService.Setup(s => s.GetEventInfoAsync(originalTicket.EventId))
                .ReturnsAsync(eventInfo);

            // Act
            var result = await _service.OverrideTicketStatusAsync(ticketId, request, operatorUserId);

            // Assert
            Assert.NotNull(result);

            // Verify audit log was created with force flag
            _mockAuditLogRepository.Verify(r => r.CreateAuditLogAsync(It.Is<TicketAuditLog>(log =>
                log.WasForced == true &&
                log.ActionType == TicketAuditLog.ActionTypes.ForceRedeem
            )), Times.Once);
        }

        [Fact]
        public async Task GetTicketAuditLogAsync_ExistingTicket_ReturnsAuditLogs()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var auditLogs = new List<TicketAuditLog>
            {
                new TicketAuditLog
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    PerformedByUserId = Guid.NewGuid(),
                    ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                    PreviousStatus = Ticket.TicketStatus.Unused,
                    NewStatus = Ticket.TicketStatus.Used,
                    Reason = "Test reason 1",
                    PerformedAt = DateTime.UtcNow.AddMinutes(-10),
                    Ticket = new Ticket { TicketCode = "TKT-20241216-ABCDEFGH" }
                },
                new TicketAuditLog
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    PerformedByUserId = Guid.NewGuid(),
                    ActionType = TicketAuditLog.ActionTypes.Reset,
                    PreviousStatus = Ticket.TicketStatus.Used,
                    NewStatus = Ticket.TicketStatus.Unused,
                    Reason = "Test reason 2",
                    PerformedAt = DateTime.UtcNow.AddMinutes(-5),
                    Ticket = new Ticket { TicketCode = "TKT-20241216-ABCDEFGH" }
                }
            };

            _mockAuditLogRepository.Setup(r => r.GetTicketAuditLogsAsync(ticketId))
                .ReturnsAsync(auditLogs);

            // Act
            var result = await _service.GetTicketAuditLogAsync(ticketId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(auditLogs[0].Id, result[0].Id);
            Assert.Equal(auditLogs[1].Id, result[1].Id);
        }

        [Fact]
        public async Task GetOperatorAuditLogsAsync_ExistingOperator_ReturnsPagedResults()
        {
            // Arrange
            var operatorUserId = Guid.NewGuid();
            var auditLogs = new List<TicketAuditLog>
            {
                new TicketAuditLog
                {
                    Id = Guid.NewGuid(),
                    TicketId = Guid.NewGuid(),
                    PerformedByUserId = operatorUserId,
                    ActionType = TicketAuditLog.ActionTypes.StatusOverride,
                    PreviousStatus = Ticket.TicketStatus.Unused,
                    NewStatus = Ticket.TicketStatus.Used,
                    Reason = "Test reason",
                    PerformedAt = DateTime.UtcNow,
                    Ticket = new Ticket { TicketCode = "TKT-20241216-ABCDEFGH" }
                }
            };

            _mockAuditLogRepository.Setup(r => r.GetUserAuditLogsAsync(operatorUserId, 1, 50))
                .ReturnsAsync(auditLogs);

            // Act
            var result = await _service.GetOperatorAuditLogsAsync(operatorUserId, 1, 50);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(auditLogs[0].Id, result[0].Id);
        }

        [Theory]
        [InlineData(Ticket.TicketStatus.Unused, Ticket.TicketStatus.Used, TicketAuditLog.ActionTypes.ForceRedeem)]
        [InlineData(Ticket.TicketStatus.Used, Ticket.TicketStatus.Unused, TicketAuditLog.ActionTypes.Reset)]
        [InlineData(Ticket.TicketStatus.Unused, Ticket.TicketStatus.Cancelled, TicketAuditLog.ActionTypes.AdminCancel)]
        [InlineData(Ticket.TicketStatus.Used, Ticket.TicketStatus.Expired, TicketAuditLog.ActionTypes.StatusOverride)]
        public async Task OverrideTicketStatusAsync_DifferentStatusTransitions_CreatesCorrectActionType(
            string previousStatus, string newStatus, string expectedActionType)
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var operatorUserId = Guid.NewGuid();
            var request = new TicketOverrideRequest
            {
                NewStatus = newStatus,
                Reason = "Test action type determination",
                ForceOverride = false
            };

            var originalTicket = new Ticket
            {
                Id = ticketId,
                EventId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TicketTierId = Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = "TKT-20241216-ABCDEFGH",
                Status = previousStatus
            };

            var updatedTicket = new Ticket
            {
                Id = ticketId,
                EventId = originalTicket.EventId,
                UserId = originalTicket.UserId,
                TicketTierId = originalTicket.TicketTierId,
                Price = originalTicket.Price,
                Currency = originalTicket.Currency,
                TicketCode = originalTicket.TicketCode,
                Status = newStatus
            };

            var userInfo = new UserInfo { Id = originalTicket.UserId, Email = "user@example.com", FirstName = "John", LastName = "Doe" };
            var eventInfo = new EventInfo { Id = originalTicket.EventId, Title = "Test Event" };

            _mockTicketRepository.Setup(r => r.GetTicketByIdAsync(ticketId))
                .ReturnsAsync(originalTicket);
            _mockTicketRepository.Setup(r => r.OverrideTicketStatusAsync(ticketId, newStatus, request.ForceOverride))
                .ReturnsAsync(updatedTicket);
            _mockAuditLogRepository.Setup(r => r.CreateAuditLogAsync(It.IsAny<TicketAuditLog>()))
                .ReturnsAsync((TicketAuditLog log) => log);
            _mockUserInfoService.Setup(s => s.GetUserInfoAsync(originalTicket.UserId))
                .ReturnsAsync(userInfo);
            _mockEventInfoService.Setup(s => s.GetEventInfoAsync(originalTicket.EventId))
                .ReturnsAsync(eventInfo);

            // Act
            var result = await _service.OverrideTicketStatusAsync(ticketId, request, operatorUserId);

            // Assert
            Assert.NotNull(result);

            // Verify audit log was created with correct action type
            _mockAuditLogRepository.Verify(r => r.CreateAuditLogAsync(It.Is<TicketAuditLog>(log =>
                log.ActionType == expectedActionType
            )), Times.Once);
        }
    }
}
