namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Response model for ticket audit log entries.
    /// </summary>
    public class TicketAuditLogResponse
    {
        /// <summary>
        /// The unique identifier of the audit log entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the ticket that was modified.
        /// </summary>
        public Guid TicketId { get; set; }

        /// <summary>
        /// The ticket code for reference.
        /// </summary>
        public string TicketCode { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the user who performed the action.
        /// </summary>
        public Guid PerformedByUserId { get; set; }

        /// <summary>
        /// The type of action performed.
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// The previous status of the ticket before the action.
        /// </summary>
        public string PreviousStatus { get; set; } = string.Empty;

        /// <summary>
        /// The new status of the ticket after the action.
        /// </summary>
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// The reason provided for the action.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Additional details about the action.
        /// </summary>
        public string? AdditionalDetails { get; set; }

        /// <summary>
        /// Whether this was a forced override action.
        /// </summary>
        public bool WasForced { get; set; }

        /// <summary>
        /// The IP address of the user who performed the action.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// The date and time when the action was performed.
        /// </summary>
        public DateTime PerformedAt { get; set; }
    }
}
