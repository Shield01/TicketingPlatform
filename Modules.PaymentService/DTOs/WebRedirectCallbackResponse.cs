namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Response model for web redirect callback handling.
    /// </summary>
    public class WebRedirectCallbackResponse
    {
        /// <summary>
        /// The unique identifier of the payment transaction.
        /// </summary>
        /// <example>99887766-5544-3322-1100-998877665544</example>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// The transaction reference.
        /// </summary>
        /// <example>PAY-20240115-A1B2C3D4E5F6</example>
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The updated payment status.
        /// </summary>
        /// <example>COMPLETED</example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// A message describing the callback processing result.
        /// </summary>
        /// <example>Payment completed successfully</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Whether the payment was successful.
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// The URL to redirect the user to (if applicable).
        /// </summary>
        /// <example>https://example.com/payment/success?ref=PAY-20240115-A1B2C3D4E5F6</example>
        public string? RedirectUrl { get; set; }
    }
}

