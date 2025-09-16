using Modules.TicketService.DTOs;
using Modules.TicketService.Models;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service interface for ticket override operations by admin/staff.
    /// </summary>
    public interface ITicketOverrideService
    {
        /// <summary>
        /// Overrides the status of a ticket with audit logging.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="request">
        /// </param>
        /// <param name="operatorUserId">The ID of the user performing the override.</param>
        /// <param name="ipAddress">The IP address of the request.</param>
        /// <param name="userAgent">The user agent of the request.</param>
        /// <returns>The updated ticket if successful, null otherwise.</returns>
        Task<TicketResponse?> OverrideTicketStatusAsync(
            Guid ticketId, 
            TicketOverrideRequest request, 
            Guid operatorUserId,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>
        /// Gets the audit log for a specific ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>List of audit log entries for the ticket.</returns>
        Task<List<TicketAuditLogResponse>> GetTicketAuditLogAsync(Guid ticketId);

        /// <summary>
        /// Gets audit logs for tickets managed by a specific user.
        /// </summary>
        /// <param name="operatorUserId">The operator user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>List of audit log entries performed by the user.</returns>
        Task<List<TicketAuditLogResponse>> GetOperatorAuditLogsAsync(Guid operatorUserId, int page = 1, int pageSize = 50);
    }
}
