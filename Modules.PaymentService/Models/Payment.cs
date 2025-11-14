using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.Models
{
    /// <summary>
    /// Model representing a payment transaction.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// The unique identifier of the payment.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the user making the payment.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique identifier of the event being paid for.
        /// </summary>
        [Required]
        public Guid EventId { get; set; }

        /// <summary>
        /// The payment reference from the payment gateway.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string PaymentReference { get; set; } = string.Empty;

        /// <summary>
        /// The transaction ID from the payment gateway.
        /// </summary>
        [StringLength(100)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// The payment gateway used (Payaza, Flutterwave, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Gateway { get; set; } = string.Empty;

        /// <summary>
        /// The amount paid.
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency of the payment.
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The current status of the payment.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// The payment method used (card, bank transfer, etc.).
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// The description or purpose of the payment.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Metadata from the payment gateway.
        /// </summary>
        public string? GatewayMetadata { get; set; }

        /// <summary>
        /// The date and time when the payment was initiated.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the payment was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the payment was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Whether the payment record is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The last webhook event ID received for this payment (for idempotency).
        /// </summary>
        [StringLength(100)]
        public string? LastWebhookEventId { get; set; }

        /// <summary>
        /// The timestamp of the last webhook received for this payment.
        /// </summary>
        public DateTime? LastWebhookReceivedAt { get; set; }

        /// <summary>
        /// The number of webhook events received for this payment.
        /// </summary>
        public int WebhookCount { get; set; } = 0;

        /// <summary>
        /// Collection of tickets purchased with this payment.
        /// </summary>
        public virtual ICollection<PaymentItem> PaymentItems { get; set; } = new List<PaymentItem>();
    }
}
