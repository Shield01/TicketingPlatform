namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Request model for payment webhook.
    /// </summary>
    public class PaymentWebhookRequest
    {
        /// <summary>
        /// The unique identifier of the transaction.
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// The transaction reference.
        /// </summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>
        /// The status of the payment.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The amount that was paid.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency of the payment.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The payment gateway that processed the payment.
        /// </summary>
        public string Gateway { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the payment was processed.
        /// </summary>
        public DateTime ProcessedAt { get; set; }

        /// <summary>
        /// Additional metadata from the payment gateway.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
} 