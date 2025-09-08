using Modules.TicketService.Models;

namespace Modules.TicketService.Repositories
{
    /// <summary>
    /// Repository interface for ticket operations.
    /// </summary>
    public interface ITicketRepository
    {
        /// <summary>
        /// Issues a new ticket for a user.
        /// </summary>
        /// <param name="ticket">The ticket to create.</param>
        /// <returns>The created ticket.</returns>
        Task<Ticket> IssueTicketAsync(Ticket ticket);

        /// <summary>
        /// Issues multiple tickets for a user.
        /// </summary>
        /// <param name="tickets">The tickets to create.</param>
        /// <returns>The created tickets.</returns>
        Task<IEnumerable<Ticket>> IssueMultipleTicketsAsync(IEnumerable<Ticket> tickets);

        /// <summary>
        /// Gets a ticket by its ID.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>The ticket if found, null otherwise.</returns>
        Task<Ticket?> GetTicketByIdAsync(Guid ticketId);

        /// <summary>
        /// Gets a ticket by its unique ticket code.
        /// </summary>
        /// <param name="ticketCode">The ticket code.</param>
        /// <returns>The ticket if found, null otherwise.</returns>
        Task<Ticket?> GetTicketByCodeAsync(string ticketCode);

        /// <summary>
        /// Gets all tickets for a specific user with pagination.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="status">Optional status filter.</param>
        /// <returns>A tuple containing the tickets and total count.</returns>
        Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetUserTicketsAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 10, 
            string? status = null);

        /// <summary>
        /// Gets all tickets for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>A list of tickets for the event.</returns>
        Task<IEnumerable<Ticket>> GetEventTicketsAsync(Guid eventId);

        /// <summary>
        /// Gets all tickets for a specific payment.
        /// </summary>
        /// <param name="paymentId">The payment ID.</param>
        /// <returns>A list of tickets for the payment.</returns>
        Task<IEnumerable<Ticket>> GetTicketsByPaymentIdAsync(Guid paymentId);

        /// <summary>
        /// Updates an existing ticket.
        /// </summary>
        /// <param name="ticket">The ticket to update.</param>
        /// <returns>The updated ticket.</returns>
        Task<Ticket> UpdateTicketAsync(Ticket ticket);

        /// <summary>
        /// Marks a ticket as used.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> MarkTicketAsUsedAsync(Guid ticketId);

        /// <summary>
        /// Cancels a ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> CancelTicketAsync(Guid ticketId);

        /// <summary>
        /// Checks if a ticket code already exists.
        /// </summary>
        /// <param name="ticketCode">The ticket code to check.</param>
        /// <returns>True if the code exists, false otherwise.</returns>
        Task<bool> TicketCodeExistsAsync(string ticketCode);

        /// <summary>
        /// Gets the count of tickets by status for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A dictionary with status counts.</returns>
        Task<Dictionary<string, int>> GetUserTicketStatusCountsAsync(Guid userId);

        /// <summary>
        /// Validates that a ticket tier has available capacity.
        /// </summary>
        /// <param name="ticketTierId">The ticket tier ID.</param>
        /// <param name="quantity">The number of tickets requested.</param>
        /// <returns>True if there's enough capacity, false otherwise.</returns>
        Task<bool> ValidateTicketTierCapacityAsync(Guid ticketTierId, int quantity);

        /// <summary>
        /// Gets the ticket tier by ID with availability information.
        /// </summary>
        /// <param name="ticketTierId">The ticket tier ID.</param>
        /// <returns>The ticket tier if found, null otherwise.</returns>
        Task<TicketTier?> GetTicketTierAsync(Guid ticketTierId);

        /// <summary>
        /// Updates the sold quantity for a ticket tier.
        /// </summary>
        /// <param name="ticketTierId">The ticket tier ID.</param>
        /// <param name="quantityChange">The change in sold quantity (positive or negative).</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> UpdateTicketTierSoldQuantityAsync(Guid ticketTierId, int quantityChange);

        /// <summary>
        /// Validates that a payment is confirmed and can be used for ticket issuance.
        /// </summary>
        /// <param name="paymentId">The payment ID.</param>
        /// <returns>True if payment is valid for ticket issuance, false otherwise.</returns>
        Task<bool> ValidatePaymentForTicketIssuanceAsync(Guid paymentId);
    }
}
