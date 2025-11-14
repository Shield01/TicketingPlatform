namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Represents the result of webhook processing.
    /// </summary>
    public class WebhookProcessingResult
    {
        /// <summary>
        /// Indicates whether the webhook was processed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The payment ID that was updated.
        /// </summary>
        public Guid? PaymentId { get; set; }

        /// <summary>
        /// The transaction reference from the webhook.
        /// </summary>
        public string? TransactionReference { get; set; }

        /// <summary>
        /// The updated payment status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Indicates whether this was a duplicate webhook that was already processed.
        /// </summary>
        public bool IsDuplicate { get; set; }

        /// <summary>
        /// Creates a success result.
        /// </summary>
        public static WebhookProcessingResult SuccessResult(Guid paymentId, string transactionReference, string status, string message = "Webhook processed successfully")
        {
            return new WebhookProcessingResult
            {
                Success = true,
                Message = message,
                PaymentId = paymentId,
                TransactionReference = transactionReference,
                Status = status,
                IsDuplicate = false
            };
        }

        /// <summary>
        /// Creates a duplicate result.
        /// </summary>
        public static WebhookProcessingResult DuplicateResult(string transactionReference, string message = "Webhook already processed")
        {
            return new WebhookProcessingResult
            {
                Success = true,
                Message = message,
                TransactionReference = transactionReference,
                IsDuplicate = true
            };
        }

        /// <summary>
        /// Creates a failure result.
        /// </summary>
        public static WebhookProcessingResult FailureResult(string message, string? transactionReference = null)
        {
            return new WebhookProcessingResult
            {
                Success = false,
                Message = message,
                TransactionReference = transactionReference,
                IsDuplicate = false
            };
        }
    }
}

