using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.TicketService.Data;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Xunit;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for the TicketRepository.
    /// </summary>
    public class TicketRepositoryTests : IDisposable
    {
        private readonly TicketServiceDbContext _context;
        private readonly Mock<ILogger<TicketRepository>> _mockLogger;
        private readonly TicketRepository _repository;

        public TicketRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TicketServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TicketServiceDbContext(options);
            _mockLogger = new Mock<ILogger<TicketRepository>>();
            _repository = new TicketRepository(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task IssueTicketAsync_ShouldCreateTicket()
        {
            // Arrange
            var ticket = CreateTestTicket();

            // Act
            var result = await _repository.IssueTicketAsync(ticket);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticket.Id, result.Id);
            Assert.Equal(ticket.TicketCode, result.TicketCode);

            var ticketInDb = await _context.Tickets.FindAsync(ticket.Id);
            Assert.NotNull(ticketInDb);
        }

        [Fact]
        public async Task IssueMultipleTicketsAsync_ShouldCreateAllTickets()
        {
            // Arrange
            var tickets = new List<Ticket>
            {
                CreateTestTicket(),
                CreateTestTicket(),
                CreateTestTicket()
            };

            // Act
            var result = await _repository.IssueMultipleTicketsAsync(tickets);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());

            var ticketsInDb = await _context.Tickets.CountAsync();
            Assert.Equal(3, ticketsInDb);
        }

        [Fact]
        public async Task GetTicketByIdAsync_ShouldReturnTicket_WhenExists()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier();
            await _context.TicketTiers.AddAsync(ticketTier);
            
            var ticket = CreateTestTicket(ticketTierId: ticketTier.Id);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketByIdAsync(ticket.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticket.Id, result.Id);
            Assert.Equal(ticket.TicketCode, result.TicketCode);
        }

        [Fact]
        public async Task GetTicketByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetTicketByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTicketByCodeAsync_ShouldReturnTicket_WhenExists()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier();
            await _context.TicketTiers.AddAsync(ticketTier);
            
            var ticket = CreateTestTicket(ticketTierId: ticketTier.Id);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketByCodeAsync(ticket.TicketCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticket.Id, result.Id);
            Assert.Equal(ticket.TicketCode, result.TicketCode);
        }

        [Fact]
        public async Task GetTicketByCodeAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetTicketByCodeAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserTicketsAsync_ShouldReturnUserTickets_WithPagination()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var ticketTier1 = CreateTestTicketTier();
            var ticketTier2 = CreateTestTicketTier();
            await _context.TicketTiers.AddRangeAsync(ticketTier1, ticketTier2);

            var userTickets = new List<Ticket>
            {
                CreateTestTicket(userId: userId, ticketTierId: ticketTier1.Id),
                CreateTestTicket(userId: userId, ticketTierId: ticketTier1.Id),
                CreateTestTicket(userId: userId, ticketTierId: ticketTier2.Id)
            };

            var otherUserTicket = CreateTestTicket(userId: otherUserId, ticketTierId: ticketTier1.Id);

            await _context.Tickets.AddRangeAsync(userTickets);
            await _context.Tickets.AddAsync(otherUserTicket);
            await _context.SaveChangesAsync();

            // Act
            var (tickets, totalCount) = await _repository.GetUserTicketsAsync(userId, page: 1, pageSize: 2);

            // Assert
            Assert.Equal(2, tickets.Count());
            Assert.Equal(3, totalCount);
            Assert.All(tickets, t => Assert.Equal(userId, t.UserId));
        }

        [Fact]
        public async Task GetUserTicketsAsync_ShouldFilterByStatus()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ticketTier = CreateTestTicketTier();
            await _context.TicketTiers.AddAsync(ticketTier);

            var tickets = new List<Ticket>
            {
                CreateTestTicket(userId: userId, ticketTierId: ticketTier.Id, status: Ticket.TicketStatus.Unused),
                CreateTestTicket(userId: userId, ticketTierId: ticketTier.Id, status: Ticket.TicketStatus.Used),
                CreateTestTicket(userId: userId, ticketTierId: ticketTier.Id, status: Ticket.TicketStatus.Unused)
            };

            await _context.Tickets.AddRangeAsync(tickets);
            await _context.SaveChangesAsync();

            // Act
            var (unusedTickets, unusedCount) = await _repository.GetUserTicketsAsync(userId, status: Ticket.TicketStatus.Unused);
            var (usedTickets, usedCount) = await _repository.GetUserTicketsAsync(userId, status: Ticket.TicketStatus.Used);

            // Assert
            Assert.Equal(2, unusedTickets.Count());
            Assert.Equal(2, unusedCount);
            Assert.Single(usedTickets);
            Assert.Equal(1, usedCount);
        }

        [Fact]
        public async Task UpdateTicketAsync_ShouldUpdateTicket()
        {
            // Arrange
            var ticket = CreateTestTicket();
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            ticket.Status = Ticket.TicketStatus.Used;

            // Act
            var result = await _repository.UpdateTicketAsync(ticket);

            // Assert
            Assert.Equal(Ticket.TicketStatus.Used, result.Status);

            var updatedTicket = await _context.Tickets.FindAsync(ticket.Id);
            Assert.Equal(Ticket.TicketStatus.Used, updatedTicket!.Status);
        }

        [Fact]
        public async Task MarkTicketAsUsedAsync_ShouldMarkAsUsed_WhenValid()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier();
            await _context.TicketTiers.AddAsync(ticketTier);
            
            var ticket = CreateTestTicket(ticketTierId: ticketTier.Id, status: Ticket.TicketStatus.Unused);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.MarkTicketAsUsedAsync(ticket.Id);

            // Assert
            Assert.True(result);

            var updatedTicket = await _context.Tickets.FindAsync(ticket.Id);
            Assert.True(updatedTicket!.IsUsed);
            Assert.Equal(Ticket.TicketStatus.Used, updatedTicket.Status);
            Assert.NotNull(updatedTicket.UsedAt);
        }

        [Fact]
        public async Task MarkTicketAsUsedAsync_ShouldReturnFalse_WhenAlreadyUsed()
        {
            // Arrange
            var ticket = CreateTestTicket(status: Ticket.TicketStatus.Used, isUsed: true);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.MarkTicketAsUsedAsync(ticket.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CancelTicketAsync_ShouldCancelTicket_WhenValid()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier();
            await _context.TicketTiers.AddAsync(ticketTier);
            
            var ticket = CreateTestTicket(ticketTierId: ticketTier.Id);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CancelTicketAsync(ticket.Id);

            // Assert
            Assert.True(result);

            var updatedTicket = await _context.Tickets.FindAsync(ticket.Id);
            Assert.Equal(Ticket.TicketStatus.Cancelled, updatedTicket!.Status);
        }

        [Fact]
        public async Task TicketCodeExistsAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            var ticket = CreateTestTicket();
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.TicketCodeExistsAsync(ticket.TicketCode);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task TicketCodeExistsAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Act
            var result = await _repository.TicketCodeExistsAsync("NONEXISTENT");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserTicketStatusCountsAsync_ShouldReturnCorrectCounts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tickets = new List<Ticket>
            {
                CreateTestTicket(userId: userId, status: Ticket.TicketStatus.Unused),
                CreateTestTicket(userId: userId, status: Ticket.TicketStatus.Unused),
                CreateTestTicket(userId: userId, status: Ticket.TicketStatus.Used),
                CreateTestTicket(userId: userId, status: Ticket.TicketStatus.Cancelled)
            };

            await _context.Tickets.AddRangeAsync(tickets);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserTicketStatusCountsAsync(userId);

            // Assert
            Assert.Equal(2, result[Ticket.TicketStatus.Unused]);
            Assert.Equal(1, result[Ticket.TicketStatus.Used]);
            Assert.Equal(1, result[Ticket.TicketStatus.Cancelled]);
        }

        [Fact]
        public async Task ValidateTicketTierCapacityAsync_ShouldReturnTrue_WhenCapacityAvailable()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier(maxQuantity: 10, soldQuantity: 5);
            await _context.TicketTiers.AddAsync(ticketTier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ValidateTicketTierCapacityAsync(ticketTier.Id, 3);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTicketTierCapacityAsync_ShouldReturnFalse_WhenInsufficientCapacity()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier(maxQuantity: 10, soldQuantity: 8);
            await _context.TicketTiers.AddAsync(ticketTier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ValidateTicketTierCapacityAsync(ticketTier.Id, 5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTicketTierSoldQuantityAsync_ShouldUpdateQuantity()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier(maxQuantity: 10, soldQuantity: 5);
            await _context.TicketTiers.AddAsync(ticketTier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.UpdateTicketTierSoldQuantityAsync(ticketTier.Id, 3);

            // Assert
            Assert.True(result);

            var updatedTier = await _context.TicketTiers.FindAsync(ticketTier.Id);
            Assert.Equal(8, updatedTier!.SoldQuantity);
        }

        [Fact]
        public async Task UpdateTicketTierSoldQuantityAsync_ShouldNotAllowNegativeQuantity()
        {
            // Arrange
            var ticketTier = CreateTestTicketTier(maxQuantity: 10, soldQuantity: 2);
            await _context.TicketTiers.AddAsync(ticketTier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.UpdateTicketTierSoldQuantityAsync(ticketTier.Id, -5);

            // Assert
            Assert.True(result);

            var updatedTier = await _context.TicketTiers.FindAsync(ticketTier.Id);
            Assert.Equal(0, updatedTier!.SoldQuantity); // Should not go below 0
        }

        private static Ticket CreateTestTicket(
            Guid? userId = null,
            Guid? eventId = null,
            Guid? ticketTierId = null,
            string? status = null,
            bool isUsed = false)
        {
            return new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = eventId ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                TicketTierId = ticketTierId ?? Guid.NewGuid(),
                Price = 50.00m,
                Currency = "USD",
                TicketCode = Ticket.GenerateTicketCode(),
                Status = status ?? Ticket.TicketStatus.Unused,
                IsUsed = isUsed,
                PaymentId = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static TicketTier CreateTestTicketTier(int maxQuantity = 100, int soldQuantity = 0)
        {
            return new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "Test Tier",
                Description = "Test Description",
                Price = 50.00m,
                Currency = "USD",
                MaxQuantity = maxQuantity,
                SoldQuantity = soldQuantity,
                IsAvailable = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
