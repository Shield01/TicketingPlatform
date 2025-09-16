using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.TicketService.Data;
using Modules.TicketService.Models;

namespace Modules.TicketService.Repositories
{
    /// <summary>
    /// Repository implementation for ticket operations.
    /// </summary>
    public class TicketRepository : ITicketRepository
    {
        private readonly TicketServiceDbContext _context;
        private readonly ILogger<TicketRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the TicketRepository.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public TicketRepository(TicketServiceDbContext context, ILogger<TicketRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Issues a new ticket for a user.
        /// </summary>
        /// <param name="ticket">The ticket to create.</param>
        /// <returns>The created ticket.</returns>
        public async Task<Ticket> IssueTicketAsync(Ticket ticket)
        {
            _logger.LogInformation("Issuing ticket for user {UserId} for event {EventId}", ticket.UserId, ticket.EventId);

            try
            {
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ticket {TicketId} issued successfully with code {TicketCode}", ticket.Id, ticket.TicketCode);
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing ticket for user {UserId} for event {EventId}", ticket.UserId, ticket.EventId);
                throw;
            }
        }

        /// <summary>
        /// Issues multiple tickets for a user.
        /// </summary>
        /// <param name="tickets">The tickets to create.</param>
        /// <returns>The created tickets.</returns>
        public async Task<IEnumerable<Ticket>> IssueMultipleTicketsAsync(IEnumerable<Ticket> tickets)
        {
            var ticketList = tickets.ToList();
            _logger.LogInformation("Issuing {Count} tickets", ticketList.Count);

            try
            {
                _context.Tickets.AddRange(ticketList);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully issued {Count} tickets", ticketList.Count);
                return ticketList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing {Count} tickets", ticketList.Count);
                throw;
            }
        }

        /// <summary>
        /// Gets a ticket by its ID.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>The ticket if found, null otherwise.</returns>
        public async Task<Ticket?> GetTicketByIdAsync(Guid ticketId)
        {
            _logger.LogDebug("Getting ticket by ID: {TicketId}", ticketId);

            try
            {
                return await _context.Tickets
                    .Include(t => t.TicketTier)
                    .FirstOrDefaultAsync(t => t.Id == ticketId && t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket by ID: {TicketId}", ticketId);
                throw;
            }
        }

        /// <summary>
        /// Gets a ticket by its unique ticket code.
        /// </summary>
        /// <param name="ticketCode">The ticket code.</param>
        /// <returns>The ticket if found, null otherwise.</returns>
        public async Task<Ticket?> GetTicketByCodeAsync(string ticketCode)
        {
            _logger.LogDebug("Getting ticket by code: {TicketCode}", ticketCode);

            try
            {
                return await _context.Tickets
                    .Include(t => t.TicketTier)
                    .FirstOrDefaultAsync(t => t.TicketCode == ticketCode && t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket by code: {TicketCode}", ticketCode);
                throw;
            }
        }

        /// <summary>
        /// Gets all tickets for a specific user with pagination.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="status">Optional status filter.</param>
        /// <returns>A tuple containing the tickets and total count.</returns>
        public async Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetUserTicketsAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 10, 
            string? status = null)
        {
            _logger.LogDebug("Getting tickets for user {UserId}, page {Page}, pageSize {PageSize}, status {Status}", 
                userId, page, pageSize, status);

            try
            {
                var query = _context.Tickets
                    .Include(t => t.TicketTier)
                    .Where(t => t.UserId == userId && t.IsActive);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                }

                var totalCount = await query.CountAsync();

                var tickets = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (tickets, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets all tickets for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>A list of tickets for the event.</returns>
        public async Task<IEnumerable<Ticket>> GetEventTicketsAsync(Guid eventId)
        {
            _logger.LogDebug("Getting tickets for event: {EventId}", eventId);

            try
            {
                return await _context.Tickets
                    .Include(t => t.TicketTier)
                    .Where(t => t.EventId == eventId && t.IsActive)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for event {EventId}", eventId);
                throw;
            }
        }

        /// <summary>
        /// Gets all tickets for a specific payment.
        /// </summary>
        /// <param name="paymentId">The payment ID.</param>
        /// <returns>A list of tickets for the payment.</returns>
        public async Task<IEnumerable<Ticket>> GetTicketsByPaymentIdAsync(Guid paymentId)
        {
            _logger.LogDebug("Getting tickets for payment: {PaymentId}", paymentId);

            try
            {
                return await _context.Tickets
                    .Include(t => t.TicketTier)
                    .Where(t => t.PaymentId == paymentId && t.IsActive)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for payment {PaymentId}", paymentId);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing ticket.
        /// </summary>
        /// <param name="ticket">The ticket to update.</param>
        /// <returns>The updated ticket.</returns>
        public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
        {
            _logger.LogInformation("Updating ticket {TicketId}", ticket.Id);

            try
            {
                ticket.UpdatedAt = DateTime.UtcNow;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ticket {TicketId} updated successfully", ticket.Id);
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", ticket.Id);
                throw;
            }
        }

        /// <summary>
        /// Marks a ticket as used.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public async Task<bool> MarkTicketAsUsedAsync(Guid ticketId)
        {
            _logger.LogInformation("Marking ticket {TicketId} as used", ticketId);

            try
            {
                var ticket = await GetTicketByIdAsync(ticketId);
                if (ticket == null || !ticket.IsValidForUse())
                {
                    _logger.LogWarning("Ticket {TicketId} not found or not valid for use", ticketId);
                    return false;
                }

                ticket.MarkAsUsed();
                await UpdateTicketAsync(ticket);

                _logger.LogInformation("Ticket {TicketId} marked as used successfully", ticketId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking ticket {TicketId} as used", ticketId);
                return false;
            }
        }

        /// <summary>
        /// Cancels a ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public async Task<bool> CancelTicketAsync(Guid ticketId)
        {
            _logger.LogInformation("Cancelling ticket {TicketId}", ticketId);

            try
            {
                var ticket = await GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found", ticketId);
                    return false;
                }

                ticket.Cancel();
                await UpdateTicketAsync(ticket);

                _logger.LogInformation("Ticket {TicketId} cancelled successfully", ticketId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);
                return false;
            }
        }

        /// <summary>
        /// Checks if a ticket code already exists.
        /// </summary>
        /// <param name="ticketCode">The ticket code to check.</param>
        /// <returns>True if the code exists, false otherwise.</returns>
        public async Task<bool> TicketCodeExistsAsync(string ticketCode)
        {
            try
            {
                return await _context.Tickets
                    .AnyAsync(t => t.TicketCode == ticketCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if ticket code exists: {TicketCode}", ticketCode);
                throw;
            }
        }

        /// <summary>
        /// Gets the count of tickets by status for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A dictionary with status counts.</returns>
        public async Task<Dictionary<string, int>> GetUserTicketStatusCountsAsync(Guid userId)
        {
            _logger.LogDebug("Getting ticket status counts for user {UserId}", userId);

            try
            {
                var counts = await _context.Tickets
                    .Where(t => t.UserId == userId && t.IsActive)
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return counts.ToDictionary(c => c.Status, c => c.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket status counts for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Validates that a ticket tier has available capacity.
        /// </summary>
        /// <param name="ticketTierId">The ticket tier ID.</param>
        /// <param name="quantity">The number of tickets requested.</param>
        /// <returns>True if there's enough capacity, false otherwise.</returns>
        public async Task<bool> ValidateTicketTierCapacityAsync(Guid ticketTierId, int quantity)
        {
            _logger.LogDebug("Validating capacity for ticket tier {TicketTierId}, quantity {Quantity}", ticketTierId, quantity);

            try
            {
                var ticketTier = await _context.TicketTiers
                    .FirstOrDefaultAsync(tt => tt.Id == ticketTierId && tt.IsActive);

                if (ticketTier == null)
                {
                    _logger.LogWarning("Ticket tier {TicketTierId} not found", ticketTierId);
                    return false;
                }

                var availableCapacity = ticketTier.MaxQuantity - ticketTier.SoldQuantity;
                var hasCapacity = availableCapacity >= quantity;

                _logger.LogDebug("Ticket tier {TicketTierId} has {AvailableCapacity} available, requested {Quantity}, valid: {HasCapacity}", 
                    ticketTierId, availableCapacity, quantity, hasCapacity);

                return hasCapacity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ticket tier capacity for {TicketTierId}", ticketTierId);
                throw;
            }
        }

        /// <summary>
        /// Gets the ticket tier by ID with availability information.
        /// </summary>
        /// <param name="ticketTierId">The ticket tier ID.</param>
        /// <returns>The ticket tier if found, null otherwise.</returns>
        public async Task<TicketTier?> GetTicketTierAsync(Guid ticketTierId)
        {
            _logger.LogDebug("Getting ticket tier by ID: {TicketTierId}", ticketTierId);

            try
            {
                return await _context.TicketTiers
                    .FirstOrDefaultAsync(tt => tt.Id == ticketTierId && tt.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket tier by ID: {TicketTierId}", ticketTierId);
                throw;
            }
        }

        /// <summary>
        /// Updates the sold quantity for a ticket tier.
        /// </summary>
        /// <param name="ticketTierId">The ticket tier ID.</param>
        /// <param name="quantityChange">The change in sold quantity (positive or negative).</param>
        /// <returns>True if successful, false otherwise.</returns>
        public async Task<bool> UpdateTicketTierSoldQuantityAsync(Guid ticketTierId, int quantityChange)
        {
            _logger.LogInformation("Updating sold quantity for ticket tier {TicketTierId} by {QuantityChange}", ticketTierId, quantityChange);

            try
            {
                var ticketTier = await GetTicketTierAsync(ticketTierId);
                if (ticketTier == null)
                {
                    _logger.LogWarning("Ticket tier {TicketTierId} not found", ticketTierId);
                    return false;
                }

                ticketTier.SoldQuantity += quantityChange;
                ticketTier.UpdatedAt = DateTime.UtcNow;

                // Ensure sold quantity doesn't go below 0
                if (ticketTier.SoldQuantity < 0)
                {
                    ticketTier.SoldQuantity = 0;
                }

                _context.TicketTiers.Update(ticketTier);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated sold quantity for ticket tier {TicketTierId} to {SoldQuantity}", 
                    ticketTierId, ticketTier.SoldQuantity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sold quantity for ticket tier {TicketTierId}", ticketTierId);
                return false;
            }
        }

        /// <summary>
        /// Validates that a payment is confirmed and can be used for ticket issuance.
        /// </summary>
        /// <param name="paymentId">The payment ID.</param>
        /// <returns>True if payment is valid for ticket issuance, false otherwise.</returns>
        public async Task<bool> ValidatePaymentForTicketIssuanceAsync(Guid paymentId)
        {
            _logger.LogDebug("Validating payment {PaymentId} for ticket issuance", paymentId);

            try
            {
                // TODO: This should integrate with the PaymentService to validate payment status
                // For now, we'll assume any payment ID is valid since PaymentService is not fully implemented
                // In a real implementation, this would check:
                // 1. Payment exists
                // 2. Payment is in "CONFIRMED" or "COMPLETED" status
                // 3. Payment amount matches the ticket price
                // 4. Payment hasn't already been used for ticket issuance

                // Placeholder implementation - in real scenario, call PaymentService
                _logger.LogWarning("Payment validation is a placeholder - PaymentService integration needed");
                return paymentId != Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment {PaymentId} for ticket issuance", paymentId);
                return false;
            }
        }

        /// <summary>
        /// Overrides the status of a ticket (admin/staff action).
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="newStatus">The new status to set.</param>
        /// <param name="forceOverride">Whether to force the override even if the ticket is in an invalid state.</param>
        /// <returns>The updated ticket if successful, null otherwise.</returns>
        public async Task<Ticket?> OverrideTicketStatusAsync(Guid ticketId, string newStatus, bool forceOverride = false)
        {
            _logger.LogInformation("Overriding ticket {TicketId} status to {NewStatus}, forceOverride: {ForceOverride}", 
                ticketId, newStatus, forceOverride);

            try
            {
                var ticket = await GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found for status override", ticketId);
                    return null;
                }

                var previousStatus = ticket.Status;

                // Validate status transition if not forcing
                if (!forceOverride && !IsValidStatusTransition(previousStatus, newStatus))
                {
                    _logger.LogWarning("Invalid status transition from {PreviousStatus} to {NewStatus} for ticket {TicketId}", 
                        previousStatus, newStatus, ticketId);
                    return null;
                }

                // Update the ticket status
                ticket.Status = newStatus;
                ticket.UpdatedAt = DateTime.UtcNow;

                // Handle specific status changes
                switch (newStatus.ToUpper())
                {
                    case "USED":
                        ticket.IsUsed = true;
                        ticket.UsedAt = DateTime.UtcNow;
                        break;
                    case "UNUSED":
                        ticket.IsUsed = false;
                        ticket.UsedAt = null;
                        break;
                    case "CANCELLED":
                        // No additional changes needed
                        break;
                    case "EXPIRED":
                        // No additional changes needed
                        break;
                }

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully overrode ticket {TicketId} status from {PreviousStatus} to {NewStatus}", 
                    ticketId, previousStatus, newStatus);

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error overriding ticket {TicketId} status to {NewStatus}", ticketId, newStatus);
                return null;
            }
        }

        /// <summary>
        /// Validates if a status transition is allowed under normal circumstances.
        /// </summary>
        /// <param name="currentStatus">The current ticket status.</param>
        /// <param name="newStatus">The desired new status.</param>
        /// <returns>True if the transition is valid, false otherwise.</returns>
        private static bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            return (currentStatus.ToUpper(), newStatus.ToUpper()) switch
            {
                // From UNUSED
                ("UNUSED", "USED") => true,
                ("UNUSED", "CANCELLED") => true,
                ("UNUSED", "EXPIRED") => true,
                
                // From USED (only to UNUSED with admin override)
                ("USED", "UNUSED") => true, // Allow admin to reset
                ("USED", "CANCELLED") => true, // Allow admin to cancel used ticket
                
                // From CANCELLED
                ("CANCELLED", "UNUSED") => true, // Allow admin to reactivate
                ("CANCELLED", "EXPIRED") => true,
                
                // From EXPIRED
                ("EXPIRED", "UNUSED") => true, // Allow admin to reactivate
                ("EXPIRED", "CANCELLED") => true,
                
                // Same status (no-op)
                var (current, new_) when current == new_ => true,
                
                // All other transitions are invalid
                _ => false
            };
        }
    }
}
