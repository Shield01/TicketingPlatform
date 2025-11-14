using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Request model for creating a payment session.
    /// </summary>
    public class CreateSessionRequest
    {
        /// <summary>
        /// The unique identifier of the user making the payment.
        /// </summary>
        /// <example>12345678-1234-1234-1234-123456789012</example>
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique identifier of the event being paid for.
        /// </summary>
        /// <example>98765432-1234-1234-1234-123456789012</example>
        [Required(ErrorMessage = "EventId is required")]
        public Guid EventId { get; set; }

        /// <summary>
        /// The unique identifier of the ticket tier being purchased.
        /// </summary>
        /// <example>11111111-2222-3333-4444-555555555555</example>
        [Required(ErrorMessage = "TicketTierId is required")]
        public Guid TicketTierId { get; set; }

        /// <summary>
        /// The number of tickets to purchase.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// The amount to be paid.
        /// </summary>
        /// <example>5000.00</example>
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code for the payment (ISO 4217).
        /// </summary>
        /// <example>NGN</example>
        [Required(ErrorMessage = "Currency is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code")]
        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// The customer's email address.
        /// </summary>
        /// <example>customer@example.com</example>
        [Required(ErrorMessage = "CustomerEmail is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string CustomerEmail { get; set; } = string.Empty;

        /// <summary>
        /// The customer's full name.
        /// </summary>
        /// <example>John Doe</example>
        [Required(ErrorMessage = "CustomerName is required")]
        [StringLength(200, ErrorMessage = "CustomerName cannot exceed 200 characters")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// The customer's phone number.
        /// </summary>
        /// <example>+2348012345678</example>
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// The URL to redirect to after successful payment.
        /// </summary>
        /// <example>https://example.com/payment/success</example>
        [Url(ErrorMessage = "Invalid success URL")]
        public string? SuccessUrl { get; set; }

        /// <summary>
        /// The URL to redirect to after cancelled payment.
        /// </summary>
        /// <example>https://example.com/payment/cancel</example>
        [Url(ErrorMessage = "Invalid cancel URL")]
        public string? CancelUrl { get; set; }
    }
}

