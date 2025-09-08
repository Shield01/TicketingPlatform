using Modules.TicketService.DTOs;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service interface for ticket issuance operations.
    /// </summary>
    public interface ITicketIssueService
    {
        /// <summary>
        /// Issues tickets after payment confirmation.
        /// </summary>
        /// <param name="request">The ticket issuance request.</param>
        /// <returns>The ticket issuance response with issued tickets.</returns>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated.</exception>
        Task<IssueTicketResponse> IssueTicketsAsync(IssueTicketRequest request);

        /// <summary>
        /// Gets all tickets for a specific user with pagination and filtering.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="status">Optional status filter.</param>
        /// <returns>The user's tickets with pagination information.</returns>
        Task<UserTicketsResponse> GetUserTicketsAsync(Guid userId, int page = 1, int pageSize = 10, string? status = null);

        /// <summary>
        /// Gets a specific ticket by ID.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="userId">The user ID (for authorization).</param>
        /// <returns>The ticket response if found and authorized, null otherwise.</returns>
        Task<TicketResponse?> GetTicketByIdAsync(Guid ticketId, Guid userId);

        /// <summary>
        /// Verifies a ticket using its code or QR data.
        /// </summary>
        /// <param name="request">The ticket verification request.</param>
        /// <returns>The ticket verification response.</returns>
        Task<TicketVerificationResponse> VerifyTicketAsync(TicketVerificationRequest request);

        /// <summary>
        /// Cancels a ticket if it hasn't been used.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="userId">The user ID (for authorization).</param>
        /// <returns>True if cancelled successfully, false otherwise.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authorized.</exception>
        /// <exception cref="InvalidOperationException">Thrown when ticket cannot be cancelled.</exception>
        Task<bool> CancelTicketAsync(Guid ticketId, Guid userId);

        /// <summary>
        /// Validates that a ticket issuance request is valid.
        /// </summary>
        /// <param name="request">The ticket issuance request.</param>
        /// <returns>True if valid, false otherwise.</returns>
        Task<bool> ValidateTicketIssuanceRequestAsync(IssueTicketRequest request);
    }
}
