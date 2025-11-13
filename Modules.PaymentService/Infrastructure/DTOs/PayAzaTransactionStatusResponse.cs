using System.Text.Json.Serialization;

namespace Modules.PaymentService.Infrastructure.DTOs
{
    /// <summary>
    /// Response DTO for PayAza transaction status.
    /// </summary>
    public class PayAzaTransactionStatusResponse
    {
        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// The response message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The transaction data.
        /// </summary>
        [JsonPropertyName("data")]
        public PayAzaTransactionData? Data { get; set; }

        /// <summary>
        /// The error details if the request failed.
        /// </summary>
        [JsonPropertyName("error")]
        public PayAzaErrorDetails? Error { get; set; }
    }

    /// <summary>
    /// PayAza transaction data.
    /// </summary>
    public class PayAzaTransactionData
    {
        /// <summary>
        /// The transaction reference.
        /// </summary>
        [JsonPropertyName("transaction_reference")]
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The transaction status (e.g., "pending", "successful", "failed").
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The transaction amount.
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The transaction currency.
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// The transaction fee.
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }

        /// <summary>
        /// The transaction type (e.g., "payout", "payment").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the transaction was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the transaction was updated.
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// The timestamp when the transaction was completed.
        /// </summary>
        [JsonPropertyName("completed_at")]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Additional metadata for the transaction.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// PayAza error details.
    /// </summary>
    public class PayAzaErrorDetails
    {
        /// <summary>
        /// The error code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// The error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Additional error details.
        /// </summary>
        [JsonPropertyName("details")]
        public Dictionary<string, string[]>? Details { get; set; }
    }
}

