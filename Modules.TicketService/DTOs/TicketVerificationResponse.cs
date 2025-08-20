namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Response model for ticket verification.
    /// </summary>
    public class TicketVerificationResponse
    {
        /// <summary>
        /// Whether the ticket is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// The unique identifier of the ticket.
        /// </summary>
        public Guid TicketId { get; set; }

        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// The ticket tier name.
        /// </summary>
        public string TicketTier { get; set; } = string.Empty;

        /// <summary>
        /// The name of the ticket holder.
        /// </summary>
        public string AttendeeName { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the ticket was verified.
        /// </summary>
        public DateTime VerifiedAt { get; set; }

        /// <summary>
        /// A message describing the verification result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
} 