using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Request model for payment initiation.
    /// </summary>
    public class PaymentInitiationRequest
    {
        /// <summary>
        /// The unique identifier of the user making the payment.
        /// </summary>
        /// <example>12345678-1234-1234-1234-123456789012</example>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique identifier of the event being paid for.
        /// </summary>
        /// <example>98765432-1234-1234-1234-123456789012</example>
        [Required]
        public Guid EventId { get; set; }

        /// <summary>
        /// The unique identifier of the ticket tier being purchased.
        /// </summary>
        /// <example>11111111-2222-3333-4444-555555555555</example>
        [Required]
        public Guid TicketTierId { get; set; }

        /// <summary>
        /// The amount to be paid.
        /// </summary>
        /// <example>150.00</example>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code for the payment.
        /// </summary>
        /// <example>NGN</example>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// The payment gateway to use (Paystack or Flutterwave).
        /// </summary>
        /// <example>Paystack</example>
        [Required]
        public string Gateway { get; set; } = "Paystack";

        /// <summary>
        /// The customer's email address.
        /// </summary>
        /// <example>customer@example.com</example>
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        /// <summary>
        /// The customer's phone number.
        /// </summary>
        /// <example>+2348012345678</example>
        public string? CustomerPhone { get; set; }
    }
} 