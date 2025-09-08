namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Response model for ticket information.
    /// </summary>
    public class TicketResponse
    {
        /// <summary>
        /// The unique identifier of the ticket.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the user who owns the ticket.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique identifier of the ticket tier.
        /// </summary>
        public Guid TicketTierId { get; set; }

        /// <summary>
        /// The name of the ticket tier.
        /// </summary>
        public string TierName { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket tier.
        /// </summary>
        public string? TierDescription { get; set; }

        /// <summary>
        /// The price paid for the ticket.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The currency of the ticket price.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The unique ticket code for verification.
        /// </summary>
        public string TicketCode { get; set; } = string.Empty;

        /// <summary>
        /// The QR code data for scanning.
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// Whether the ticket has been used.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// The date and time when the ticket was used.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// The current status of the ticket.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the payment.
        /// </summary>
        public Guid? PaymentId { get; set; }

        /// <summary>
        /// The date and time when the ticket was issued.
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// Whether the ticket is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether the ticket is valid for use.
        /// </summary>
        public bool IsValidForUse { get; set; }
    }
}
