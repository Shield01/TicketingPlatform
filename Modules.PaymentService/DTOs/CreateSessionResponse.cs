namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Response model for payment session creation.
    /// </summary>
    public class CreateSessionResponse
    {
        /// <summary>
        /// The unique identifier of the payment transaction.
        /// </summary>
        /// <example>99887766-5544-3322-1100-998877665544</example>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// The unique transaction reference for tracking.
        /// </summary>
        /// <example>PAY-20240115-A1B2C3D4E5F6</example>
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The payment page redirect URL where the user should complete the payment.
        /// </summary>
        /// <example>https://checkout-test.payaza.africa?transaction_reference=PAY-20240115-A1B2C3D4E5F6&amp;amount=5000.00&amp;currency=NGN</example>
        public string RedirectUrl { get; set; } = string.Empty;

        /// <summary>
        /// The payment amount.
        /// </summary>
        /// <example>5000.00</example>
        public decimal Amount { get; set; }

        /// <summary>
        /// The payment currency.
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the payment.
        /// </summary>
        /// <example>PENDING_REDIRECT</example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The payment gateway being used.
        /// </summary>
        /// <example>PayAza</example>
        public string Gateway { get; set; } = "PayAza";

        /// <summary>
        /// The expiration time of the payment session.
        /// </summary>
        /// <example>2024-01-15T15:45:00Z</example>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// The date and time when the payment was created.
        /// </summary>
        /// <example>2024-01-15T15:15:00Z</example>
        public DateTime CreatedAt { get; set; }
    }
}

