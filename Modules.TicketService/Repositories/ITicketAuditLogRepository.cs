using Modules.TicketService.Models;

namespace Modules.TicketService.Repositories
{
    /// <summary>
    /// Repository interface for ticket audit log operations.
    /// </summary>
    public interface ITicketAuditLogRepository
    {
        /// <summary>
        /// Creates a new audit log entry.
        /// </summary>
        /// <param name="auditLog">The audit log entry to create.</param>
        /// <returns>The created audit log entry.</returns>
        Task<TicketAuditLog> CreateAuditLogAsync(TicketAuditLog auditLog);

        /// <summary>
        /// Gets all audit log entries for a specific ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>List of audit log entries for the ticket.</returns>
        Task<List<TicketAuditLog>> GetTicketAuditLogsAsync(Guid ticketId);

        /// <summary>
        /// Gets all audit log entries performed by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>List of audit log entries performed by the user.</returns>
        Task<List<TicketAuditLog>> GetUserAuditLogsAsync(Guid userId, int page = 1, int pageSize = 50);

        /// <summary>
        /// Gets audit log entries within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>List of audit log entries within the date range.</returns>
        Task<List<TicketAuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50);

        /// <summary>
        /// Gets the total count of audit log entries for a specific ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>The total count of audit log entries.</returns>
        Task<int> GetTicketAuditLogCountAsync(Guid ticketId);
    }
}
