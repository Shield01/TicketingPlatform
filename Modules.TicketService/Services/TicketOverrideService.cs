using Microsoft.Extensions.Logging;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Shared.Kernel.Interfaces;
using System.Text.Json;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service implementation for ticket override operations by admin/staff.
    /// </summary>
    public class TicketOverrideService : ITicketOverrideService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketAuditLogRepository _auditLogRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IEventInfoService _eventInfoService;
        private readonly ILogger<TicketOverrideService> _logger;

        /// <summary>
        /// Initializes a new instance of the TicketOverrideService.
        /// </summary>
        /// <param name="ticketRepository">The ticket repository.</param>
        /// <param name="auditLogRepository">The audit log repository.</param>
        /// <param name="userInfoService">The user info service.</param>
        /// <param name="eventInfoService">The event info service.</param>
        /// <param name="logger">The logger instance.</param>
        public TicketOverrideService(
            ITicketRepository ticketRepository,
            ITicketAuditLogRepository auditLogRepository,
            IUserInfoService userInfoService,
            IEventInfoService eventInfoService,
            ILogger<TicketOverrideService> logger)
        {
            _ticketRepository = ticketRepository;
            _auditLogRepository = auditLogRepository;
            _userInfoService = userInfoService;
            _eventInfoService = eventInfoService;
            _logger = logger;
        }

        /// <summary>
        /// Overrides the status of a ticket with audit logging.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="request">The override request containing new status and reason.</param>
        /// <param name="operatorUserId">The ID of the user performing the override.</param>
        /// <param name="ipAddress">The IP address of the request.</param>
        /// <param name="userAgent">The user agent of the request.</param>
        /// <returns>The updated ticket if successful, null otherwise.</returns>
        public async Task<TicketResponse?> OverrideTicketStatusAsync(
            Guid ticketId, 
            TicketOverrideRequest request, 
            Guid operatorUserId,
            string? ipAddress = null,
            string? userAgent = null)
        {
            _logger.LogInformation("Ticket override requested by user {OperatorUserId} for ticket {TicketId} to status {NewStatus}", 
                operatorUserId, ticketId, request.NewStatus);

            try
            {
                // Validate the new status
                if (!IsValidTicketStatus(request.NewStatus))
                {
                    _logger.LogWarning("Invalid ticket status {NewStatus} provided for override", request.NewStatus);
                    return null;
                }

                // Get the current ticket
                var currentTicket = await _ticketRepository.GetTicketByIdAsync(ticketId);
                if (currentTicket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found for override", ticketId);
                    return null;
                }

                var previousStatus = currentTicket.Status;

                // Log the override attempt
                _logger.LogInformation("Attempting to override ticket {TicketId} from {PreviousStatus} to {NewStatus} by user {OperatorUserId}. Reason: {Reason}", 
                    ticketId, previousStatus, request.NewStatus, operatorUserId, request.Reason);

                // Perform the override
                var updatedTicket = await _ticketRepository.OverrideTicketStatusAsync(ticketId, request.NewStatus, request.ForceOverride);
                if (updatedTicket == null)
                {
                    _logger.LogWarning("Failed to override ticket {TicketId} status to {NewStatus}", ticketId, request.NewStatus);
                    return null;
                }

                // Create audit log entry
                var auditLog = new TicketAuditLog
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    PerformedByUserId = operatorUserId,
                    ActionType = DetermineActionType(previousStatus, request.NewStatus),
                    PreviousStatus = previousStatus,
                    NewStatus = request.NewStatus,
                    Reason = request.Reason,
                    WasForced = request.ForceOverride,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalDetails = JsonSerializer.Serialize(new
                    {
                        OriginalRequest = request,
                        TicketCode = updatedTicket.TicketCode,
                        EventId = updatedTicket.EventId,
                        UserId = updatedTicket.UserId
                    }),
                    PerformedAt = DateTime.UtcNow
                };

                await _auditLogRepository.CreateAuditLogAsync(auditLog);

                _logger.LogInformation("Successfully overrode ticket {TicketId} status from {PreviousStatus} to {NewStatus} by user {OperatorUserId}", 
                    ticketId, previousStatus, request.NewStatus, operatorUserId);

                // Convert to response
                return await ConvertToTicketResponseAsync(updatedTicket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error overriding ticket {TicketId} status to {NewStatus} by user {OperatorUserId}", 
                    ticketId, request.NewStatus, operatorUserId);
                return null;
            }
        }

        /// <summary>
        /// Gets the audit log for a specific ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>List of audit log entries for the ticket.</returns>
        public async Task<List<TicketAuditLogResponse>> GetTicketAuditLogAsync(Guid ticketId)
        {
            _logger.LogInformation("Getting audit log for ticket {TicketId}", ticketId);

            try
            {
                var auditLogs = await _auditLogRepository.GetTicketAuditLogsAsync(ticketId);
                return auditLogs.Select(ConvertToAuditLogResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit log for ticket {TicketId}", ticketId);
                return new List<TicketAuditLogResponse>();
            }
        }

        /// <summary>
        /// Gets audit logs for tickets managed by a specific user.
        /// </summary>
        /// <param name="operatorUserId">The operator user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>List of audit log entries performed by the user.</returns>
        public async Task<List<TicketAuditLogResponse>> GetOperatorAuditLogsAsync(Guid operatorUserId, int page = 1, int pageSize = 50)
        {
            _logger.LogInformation("Getting audit logs for operator {OperatorUserId}, page {Page}, pageSize {PageSize}", 
                operatorUserId, page, pageSize);

            try
            {
                var auditLogs = await _auditLogRepository.GetUserAuditLogsAsync(operatorUserId, page, pageSize);
                return auditLogs.Select(ConvertToAuditLogResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for operator {OperatorUserId}", operatorUserId);
                return new List<TicketAuditLogResponse>();
            }
        }

        /// <summary>
        /// Validates if the provided status is a valid ticket status.
        /// </summary>
        /// <param name="status">The status to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        private static bool IsValidTicketStatus(string status)
        {
            var validStatuses = new[] 
            { 
                Ticket.TicketStatus.Unused, 
                Ticket.TicketStatus.Used, 
                Ticket.TicketStatus.Cancelled, 
                Ticket.TicketStatus.Expired 
            };

            return validStatuses.Contains(status.ToUpper());
        }

        /// <summary>
        /// Determines the action type based on status transition.
        /// </summary>
        /// <param name="previousStatus">The previous status.</param>
        /// <param name="newStatus">The new status.</param>
        /// <returns>The action type.</returns>
        private static string DetermineActionType(string previousStatus, string newStatus)
        {
            return (previousStatus.ToUpper(), newStatus.ToUpper()) switch
            {
                (_, "USED") => TicketAuditLog.ActionTypes.ForceRedeem,
                (_, "UNUSED") => TicketAuditLog.ActionTypes.Reset,
                (_, "CANCELLED") => TicketAuditLog.ActionTypes.AdminCancel,
                _ => TicketAuditLog.ActionTypes.StatusOverride
            };
        }

        /// <summary>
        /// Converts a Ticket to a TicketResponse.
        /// </summary>
        /// <param name="ticket">The ticket to convert.</param>
        /// <returns>The ticket response.</returns>
        private async Task<TicketResponse> ConvertToTicketResponseAsync(Ticket ticket)
        {
            // Get event and user information
            var eventInfo = await _eventInfoService.GetEventInfoAsync(ticket.EventId);
            var userInfo = await _userInfoService.GetUserInfoAsync(ticket.UserId);

            return new TicketResponse
            {
                Id = ticket.Id,
                EventId = ticket.EventId,
                EventName = eventInfo?.Title ?? "Event",
                UserId = ticket.UserId,
                TicketTierId = ticket.TicketTierId,
                TierName = ticket.TicketTier?.Name ?? "Unknown",
                TierDescription = ticket.TicketTier?.Description,
                Price = ticket.Price,
                Currency = ticket.Currency,
                TicketCode = ticket.TicketCode,
                QRCodeData = ticket.QRCodeData,
                QRCodeImage = string.Empty, // This would be generated on demand
                IsUsed = ticket.IsUsed,
                UsedAt = ticket.UsedAt,
                Status = ticket.Status,
                IsValidForUse = ticket.IsValidForUse(),
                PaymentId = ticket.PaymentId,
                IssuedAt = ticket.CreatedAt,
                IsActive = ticket.IsActive
            };
        }

        /// <summary>
        /// Converts a TicketAuditLog to a TicketAuditLogResponse.
        /// </summary>
        /// <param name="auditLog">The audit log to convert.</param>
        /// <returns>The audit log response.</returns>
        private static TicketAuditLogResponse ConvertToAuditLogResponse(TicketAuditLog auditLog)
        {
            return new TicketAuditLogResponse
            {
                Id = auditLog.Id,
                TicketId = auditLog.TicketId,
                TicketCode = auditLog.Ticket?.TicketCode ?? "Unknown",
                PerformedByUserId = auditLog.PerformedByUserId,
                ActionType = auditLog.ActionType,
                PreviousStatus = auditLog.PreviousStatus,
                NewStatus = auditLog.NewStatus,
                Reason = auditLog.Reason,
                AdditionalDetails = auditLog.AdditionalDetails,
                WasForced = auditLog.WasForced,
                IpAddress = auditLog.IpAddress,
                PerformedAt = auditLog.PerformedAt
            };
        }
    }
}
