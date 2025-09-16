using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.TicketService.Data;
using Modules.TicketService.Models;

namespace Modules.TicketService.Repositories
{
    /// <summary>
    /// Repository implementation for ticket audit log operations.
    /// </summary>
    public class TicketAuditLogRepository : ITicketAuditLogRepository
    {
        private readonly TicketServiceDbContext _context;
        private readonly ILogger<TicketAuditLogRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the TicketAuditLogRepository.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public TicketAuditLogRepository(TicketServiceDbContext context, ILogger<TicketAuditLogRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new audit log entry.
        /// </summary>
        /// <param name="auditLog">The audit log entry to create.</param>
        /// <returns>The created audit log entry.</returns>
        public async Task<TicketAuditLog> CreateAuditLogAsync(TicketAuditLog auditLog)
        {
            _logger.LogInformation("Creating audit log for ticket {TicketId}, action {ActionType}", 
                auditLog.TicketId, auditLog.ActionType);

            _context.TicketAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created with ID {AuditLogId}", auditLog.Id);
            return auditLog;
        }

        /// <summary>
        /// Gets all audit log entries for a specific ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>List of audit log entries for the ticket.</returns>
        public async Task<List<TicketAuditLog>> GetTicketAuditLogsAsync(Guid ticketId)
        {
            _logger.LogInformation("Getting audit logs for ticket {TicketId}", ticketId);

            return await _context.TicketAuditLogs
                .Where(tal => tal.TicketId == ticketId && tal.IsActive)
                .OrderByDescending(tal => tal.PerformedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all audit log entries performed by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>List of audit log entries performed by the user.</returns>
        public async Task<List<TicketAuditLog>> GetUserAuditLogsAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            _logger.LogInformation("Getting audit logs for user {UserId}, page {Page}, pageSize {PageSize}", 
                userId, page, pageSize);

            var skip = (page - 1) * pageSize;

            return await _context.TicketAuditLogs
                .Where(tal => tal.PerformedByUserId == userId && tal.IsActive)
                .OrderByDescending(tal => tal.PerformedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(tal => tal.Ticket)
                .ToListAsync();
        }

        /// <summary>
        /// Gets audit log entries within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>List of audit log entries within the date range.</returns>
        public async Task<List<TicketAuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50)
        {
            _logger.LogInformation("Getting audit logs from {StartDate} to {EndDate}, page {Page}, pageSize {PageSize}", 
                startDate, endDate, page, pageSize);

            var skip = (page - 1) * pageSize;

            return await _context.TicketAuditLogs
                .Where(tal => tal.PerformedAt >= startDate && tal.PerformedAt <= endDate && tal.IsActive)
                .OrderByDescending(tal => tal.PerformedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(tal => tal.Ticket)
                .ToListAsync();
        }

        /// <summary>
        /// Gets the total count of audit log entries for a specific ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>The total count of audit log entries.</returns>
        public async Task<int> GetTicketAuditLogCountAsync(Guid ticketId)
        {
            _logger.LogInformation("Getting audit log count for ticket {TicketId}", ticketId);

            return await _context.TicketAuditLogs
                .Where(tal => tal.TicketId == ticketId && tal.IsActive)
                .CountAsync();
        }
    }
}
