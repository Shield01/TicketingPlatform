using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.Models
{
    /// <summary>
    /// Model representing a ticket issued for an event.
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// The unique identifier of the ticket.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the event this ticket is for.
        /// </summary>
        [Required]
        public Guid EventId { get; set; }

        /// <summary>
        /// The unique identifier of the user who purchased this ticket.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The tier of the ticket (VIP, Regular, Early Bird, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TicketTier { get; set; } = string.Empty;

        /// <summary>
        /// The price of the ticket.
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// The currency of the ticket price.
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The unique ticket code/number for verification.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TicketCode { get; set; } = string.Empty;

        /// <summary>
        /// The QR code data for ticket scanning.
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// Whether the ticket has been used/scanned.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// The date and time when the ticket was used/scanned.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// The current status of the ticket.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// The date and time when the ticket was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the ticket was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the ticket is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
