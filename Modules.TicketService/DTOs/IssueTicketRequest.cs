using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Request model for issuing a ticket after payment confirmation.
    /// </summary>
    public class IssueTicketRequest
    {
        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440000</example>
        [Required(ErrorMessage = "Event ID is required.")]
        public Guid EventId { get; set; }

        /// <summary>
        /// The unique identifier of the user purchasing the ticket.
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440001</example>
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique identifier of the ticket tier.
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440002</example>
        [Required(ErrorMessage = "Ticket tier ID is required.")]
        public Guid TicketTierId { get; set; }

        /// <summary>
        /// The price paid for the ticket.
        /// </summary>
        /// <example>150.00</example>
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        /// <summary>
        /// The currency code for the ticket price.
        /// </summary>
        /// <example>USD</example>
        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, ErrorMessage = "Currency must be a 3-character code.")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The unique identifier of the payment that confirmed the purchase.
        /// If not provided, a GUID will be auto-generated for testing purposes.
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440003</example>
        public Guid? PaymentId { get; set; }

        /// <summary>
        /// The quantity of tickets to issue (for multiple ticket purchases).
        /// </summary>
        /// <example>2</example>
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10 tickets.")]
        public int Quantity { get; set; } = 1;
    }
}
