using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Request model for overriding ticket status by admin/staff.
    /// </summary>
    public class TicketOverrideRequest
    {
        /// <summary>
        /// The new status to set for the ticket.
        /// Valid values: UNUSED, USED, CANCELLED, EXPIRED
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// The reason for the override action.
        /// This will be logged for audit purposes.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Whether to force the status change even if the ticket is in an invalid state.
        /// Default is false for safety.
        /// </summary>
        public bool ForceOverride { get; set; } = false;
    }
}
