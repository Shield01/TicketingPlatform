using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Modules.TicketService.Data;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;

namespace Tests.TicketService.Tests
{
    /// <summary>
    /// Unit tests for TicketTierRepository data access layer functionality.
    /// </summary>
    public class TicketTierRepositoryTests : IDisposable
    {
        private readonly TicketServiceDbContext _context;
        private readonly TicketTierRepository _repository;

        public TicketTierRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TicketServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TicketServiceDbContext(options);
            _repository = new TicketTierRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region CreateTicketTierAsync Tests

        [Fact]
        public async Task CreateTicketTierAsync_ValidTier_ReturnsSavedTier()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var tier = new TicketTier
            {
                EventId = eventId,
                Name = "VIP",
                Description = "Premium access",
                Price = 150.00m,
                Currency = "USD",
                MaxQuantity = 50,
                IsAvailable = true,
                IsActive = true
            };

            // Act
            var result = await _repository.CreateTicketTierAsync(tier);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal("VIP", result.Name);
            Assert.Equal("Premium access", result.Description);
            Assert.Equal(150.00m, result.Price);
            Assert.Equal("USD", result.Currency);
            Assert.Equal(50, result.MaxQuantity);
            Assert.Equal(0, result.SoldQuantity);
            Assert.True(result.IsAvailable);
            Assert.True(result.IsActive);

            // Verify it's saved in database
            var saved = await _context.TicketTiers.FindAsync(result.Id);
            Assert.NotNull(saved);
            Assert.Equal(result.Name, saved.Name);
        }

        [Fact]
        public async Task CreateTicketTierAsync_NullTier_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.CreateTicketTierAsync(null));
        }

        #endregion

        #region GetTicketTierByIdAsync Tests

        [Fact]
        public async Task GetTicketTierByIdAsync_ExistingTier_ReturnsTier()
        {
            // Arrange
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketTierByIdAsync(tier.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tier.Id, result.Id);
            Assert.Equal(tier.Name, result.Name);
        }

        [Fact]
        public async Task GetTicketTierByIdAsync_NonExistentTier_ReturnsNull()
        {
            // Act
            var result = await _repository.GetTicketTierByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTicketTierByIdAsync_InactiveTier_ReturnsNull()
        {
            // Arrange
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = false // Inactive tier
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketTierByIdAsync(tier.Id);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetTicketTiersByEventIdAsync Tests

        [Fact]
        public async Task GetTicketTiersByEventIdAsync_ExistingTiers_ReturnsOrderedTiers()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var tiers = new List<TicketTier>
            {
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "VIP",
                    Price = 200.00m,
                    MaxQuantity = 20,
                    IsActive = true
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Price = 75.00m,
                    MaxQuantity = 100,
                    IsActive = true
                },
                new TicketTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Price = 50.00m,
                    MaxQuantity = 50,
                    IsActive = true
                }
            };

            _context.TicketTiers.AddRange(tiers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketTiersByEventIdAsync(eventId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);

            // Should be ordered by price (ascending)
            Assert.Equal("Early Bird", resultList[0].Name);
            Assert.Equal(50.00m, resultList[0].Price);
            Assert.Equal("Regular", resultList[1].Name);
            Assert.Equal(75.00m, resultList[1].Price);
            Assert.Equal("VIP", resultList[2].Name);
            Assert.Equal(200.00m, resultList[2].Price);
        }

        [Fact]
        public async Task GetTicketTiersByEventIdAsync_NoTiers_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetTicketTiersByEventIdAsync(Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTicketTiersByEventIdAsync_OnlyActiveTiers_ReturnsActiveTiers()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var activeTier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true
            };

            var inactiveTier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = "Removed",
                Price = 100.00m,
                MaxQuantity = 30,
                IsActive = false
            };

            _context.TicketTiers.AddRange(activeTier, inactiveTier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTicketTiersByEventIdAsync(eventId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("VIP", resultList[0].Name);
        }

        #endregion

        #region TierNameExistsForEventAsync Tests

        [Fact]
        public async Task TierNameExistsForEventAsync_ExistingName_ReturnsTrue()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.TierNameExistsForEventAsync(eventId, "VIP");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task TierNameExistsForEventAsync_NonExistentName_ReturnsFalse()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            // Act
            var result = await _repository.TierNameExistsForEventAsync(eventId, "NonExistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task TierNameExistsForEventAsync_DifferentEvent_ReturnsFalse()
        {
            // Arrange
            var eventId1 = Guid.NewGuid();
            var eventId2 = Guid.NewGuid();
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId1,
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.TierNameExistsForEventAsync(eventId2, "VIP");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task TierNameExistsForEventAsync_ExcludeId_ReturnsFalse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var tierId = Guid.NewGuid();
            var tier = new TicketTier
            {
                Id = tierId,
                EventId = eventId,
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Act - Exclude the same tier from the check
            var result = await _repository.TierNameExistsForEventAsync(eventId, "VIP", tierId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task TierNameExistsForEventAsync_InactiveTier_ReturnsFalse()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = false // Inactive tier
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.TierNameExistsForEventAsync(eventId, "VIP");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region UpdateTicketTierAsync Tests

        [Fact]
        public async Task UpdateTicketTierAsync_ValidTier_ReturnsUpdatedTier()
        {
            // Arrange
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "VIP",
                Description = "Original description",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Modify the tier
            tier.Name = "VIP Premium";
            tier.Description = "Updated description";
            tier.Price = 200.00m;

            var originalUpdatedAt = tier.UpdatedAt;

            // Act
            var result = await _repository.UpdateTicketTierAsync(tier);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VIP Premium", result.Name);
            Assert.Equal("Updated description", result.Description);
            Assert.Equal(200.00m, result.Price);
            Assert.True(result.UpdatedAt > originalUpdatedAt);

            // Verify changes are saved in database
            var saved = await _context.TicketTiers.FindAsync(tier.Id);
            Assert.NotNull(saved);
            Assert.Equal("VIP Premium", saved.Name);
            Assert.Equal("Updated description", saved.Description);
            Assert.Equal(200.00m, saved.Price);
        }

        [Fact]
        public async Task UpdateTicketTierAsync_NullTier_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.UpdateTicketTierAsync(null));
        }

        #endregion

        #region DeleteTicketTierAsync Tests

        [Fact]
        public async Task DeleteTicketTierAsync_TierWithNoTickets_HardDeletes()
        {
            // Arrange
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true
            };

            _context.TicketTiers.Add(tier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteTicketTierAsync(tier.Id);

            // Assert
            Assert.True(result);

            // Verify tier is completely removed from database
            var deleted = await _context.TicketTiers.FindAsync(tier.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteTicketTierAsync_TierWithTickets_SoftDeletes()
        {
            // Arrange
            var tier = new TicketTier
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Name = "VIP",
                Price = 150.00m,
                MaxQuantity = 50,
                IsActive = true
            };

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = tier.EventId,
                UserId = Guid.NewGuid(),
                TicketTierId = tier.Id,
                Price = tier.Price,
                Currency = "USD",
                TicketCode = "TEST123",
                Status = "Active",
                IsActive = true
            };

            _context.TicketTiers.Add(tier);
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var originalUpdatedAt = tier.UpdatedAt;

            // Act
            var result = await _repository.DeleteTicketTierAsync(tier.Id);

            // Assert
            Assert.True(result);

            // Verify tier is soft deleted (marked as inactive)
            var softDeleted = await _context.TicketTiers
                .IgnoreQueryFilters() // Include inactive records
                .FirstOrDefaultAsync(t => t.Id == tier.Id);
            
            Assert.NotNull(softDeleted);
            Assert.False(softDeleted.IsActive);
            Assert.True(softDeleted.UpdatedAt > originalUpdatedAt);
        }

        [Fact]
        public async Task DeleteTicketTierAsync_NonExistentTier_ReturnsFalse()
        {
            // Act
            var result = await _repository.DeleteTicketTierAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
