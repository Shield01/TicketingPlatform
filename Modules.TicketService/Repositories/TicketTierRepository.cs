using Microsoft.EntityFrameworkCore;
using Modules.TicketService.Data;
using Modules.TicketService.Models;

namespace Modules.TicketService.Repositories
{
    /// <summary>
    /// Repository implementation for ticket tier operations.
    /// </summary>
    public class TicketTierRepository : ITicketTierRepository
    {
        private readonly TicketServiceDbContext _context;

        /// <summary>
        /// Initializes a new instance of the TicketTierRepository class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TicketTierRepository(TicketServiceDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<TicketTier> CreateTicketTierAsync(TicketTier ticketTier)
        {
            if (ticketTier == null)
                throw new ArgumentNullException(nameof(ticketTier));

            ticketTier.Id = Guid.NewGuid();
            ticketTier.CreatedAt = DateTime.UtcNow;
            ticketTier.UpdatedAt = DateTime.UtcNow;

            _context.TicketTiers.Add(ticketTier);
            await _context.SaveChangesAsync();

            return ticketTier;
        }

        /// <inheritdoc />
        public async Task<TicketTier> UpdateTicketTierAsync(TicketTier ticketTier)
        {
            if (ticketTier == null)
                throw new ArgumentNullException(nameof(ticketTier));

            ticketTier.UpdatedAt = DateTime.UtcNow;

            _context.TicketTiers.Update(ticketTier);
            await _context.SaveChangesAsync();

            return ticketTier;
        }

        /// <inheritdoc />
        public async Task<TicketTier?> GetTicketTierByIdAsync(Guid id)
        {
            return await _context.TicketTiers
                .FirstOrDefaultAsync(tt => tt.Id == id && tt.IsActive);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TicketTier>> GetTicketTiersByEventIdAsync(Guid eventId)
        {
            return await _context.TicketTiers
                .Where(tt => tt.EventId == eventId && tt.IsActive)
                .OrderBy(tt => tt.Price)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> TierNameExistsForEventAsync(Guid eventId, string name, Guid? excludeId = null)
        {
            var query = _context.TicketTiers
                .Where(tt => tt.EventId == eventId && tt.Name == name && tt.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(tt => tt.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTicketTierAsync(Guid id)
        {
            var ticketTier = await _context.TicketTiers.FindAsync(id);
            if (ticketTier == null)
                return false;

            // Check if there are any tickets sold for this tier
            var hasTickets = await _context.Tickets
                .AnyAsync(t => t.TicketTierId == id && t.IsActive);

            if (hasTickets)
            {
                // Soft delete - mark as inactive instead of removing
                ticketTier.IsActive = false;
                ticketTier.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Hard delete if no tickets sold
                _context.TicketTiers.Remove(ticketTier);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> EventExistsAsync(Guid eventId)
        {
            // Note: This is a placeholder implementation
            // In a production system, this would need to check the EventService
            // For now, we'll assume the event exists if called by an authorized user
            // This could be implemented via:
            // 1. HTTP call to EventService API
            // 2. Shared database with cross-schema queries
            // 3. Message bus/event-driven validation
            
            // For the current implementation, we'll validate this at the service layer
            // where the caller (EventService or authorized controller) provides validation
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> IsUserEventOrganizerAsync(Guid eventId, Guid userId)
        {
            // Note: This is a placeholder implementation
            // In a production system, this would need to check the EventService
            // For now, we'll delegate this validation to the service layer
            // where the caller provides authorization context
            
            // This could be implemented via:
            // 1. HTTP call to EventService API
            // 2. Shared database with cross-schema queries
            // 3. JWT claims validation
            // 4. Authorization service integration
            
            // For the current implementation, we'll validate this at the service layer
            return true;
        }
    }
}
