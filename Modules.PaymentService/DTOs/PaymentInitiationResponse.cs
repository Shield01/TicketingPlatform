namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Response model for payment initiation.
    /// </summary>
    public class PaymentInitiationResponse
    {
        /// <summary>
        /// The unique identifier of the transaction.
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// The payment URL where the user should complete the payment.
        /// </summary>
        public string PaymentUrl { get; set; } = string.Empty;

        /// <summary>
        /// The transaction reference for tracking.
        /// </summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>
        /// The amount to be paid.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code for the payment.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the payment.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The payment gateway used.
        /// </summary>
        public string Gateway { get; set; } = string.Empty;

        /// <summary>
        /// The expiration time of the payment URL.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
} 