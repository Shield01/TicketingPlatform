using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.Models
{
    /// <summary>
    /// Model representing an audit log entry for ticket operations.
    /// </summary>
    public class TicketAuditLog
    {
        /// <summary>
        /// Constants for audit action types.
        /// </summary>
        public static class ActionTypes
        {
            public const string StatusOverride = "STATUS_OVERRIDE";
            public const string ForceRedeem = "FORCE_REDEEM";
            public const string Reset = "RESET";
            public const string AdminCancel = "ADMIN_CANCEL";
        }

        /// <summary>
        /// The unique identifier of the audit log entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the ticket that was modified.
        /// </summary>
        [Required]
        public Guid TicketId { get; set; }

        /// <summary>
        /// Navigation property for the ticket.
        /// </summary>
        public virtual Ticket? Ticket { get; set; }

        /// <summary>
        /// The unique identifier of the user who performed the action.
        /// </summary>
        [Required]
        public Guid PerformedByUserId { get; set; }

        /// <summary>
        /// The type of action performed.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// The previous status of the ticket before the action.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PreviousStatus { get; set; } = string.Empty;

        /// <summary>
        /// The new status of the ticket after the action.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// The reason provided for the action.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Additional details about the action in JSON format.
        /// </summary>
        public string? AdditionalDetails { get; set; }

        /// <summary>
        /// Whether this was a forced override action.
        /// </summary>
        public bool WasForced { get; set; } = false;

        /// <summary>
        /// The IP address of the user who performed the action.
        /// </summary>
        [StringLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        /// <summary>
        /// The user agent of the request that performed the action.
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// The date and time when the action was performed.
        /// </summary>
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the audit log is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
