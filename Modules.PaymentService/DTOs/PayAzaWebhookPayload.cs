using System.Text.Json.Serialization;

namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Represents the webhook payload from PayAza payment gateway.
    /// This DTO supports both Collections and Transfers webhook events.
    /// </summary>
    public class PayAzaWebhookPayload
    {
        /// <summary>
        /// The type of webhook event (e.g., "collection.success", "transfer.completed").
        /// </summary>
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        /// <summary>
        /// The unique transaction reference.
        /// </summary>
        [JsonPropertyName("transaction_reference")]
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The gateway transaction ID.
        /// </summary>
        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        /// <summary>
        /// The status of the transaction (e.g., "success", "failed", "pending").
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The transaction amount.
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code (e.g., "NGN", "USD").
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The payment method used (e.g., "card", "bank_transfer", "wallet").
        /// </summary>
        [JsonPropertyName("payment_method")]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// The gateway fee charged for the transaction.
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal? Fee { get; set; }

        /// <summary>
        /// The timestamp when the transaction was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the transaction was completed.
        /// </summary>
        [JsonPropertyName("completed_at")]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Customer email address.
        /// </summary>
        [JsonPropertyName("customer_email")]
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Customer name.
        /// </summary>
        [JsonPropertyName("customer_name")]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Additional metadata from the payment gateway.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Error message if the transaction failed.
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Error code if the transaction failed.
        /// </summary>
        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }
    }
}

