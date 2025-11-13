using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Modules.PaymentService.Infrastructure.DTOs
{
    /// <summary>
    /// Request DTO for initiating a PayAza payout.
    /// </summary>
    public class PayAzaPayoutRequest
    {
        /// <summary>
        /// The transaction reference (unique identifier).
        /// </summary>
        [Required]
        [JsonPropertyName("transaction_reference")]
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The payout amount.
        /// </summary>
        [Required]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code (e.g., "NGN", "USD").
        /// </summary>
        [Required]
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// The recipient account number.
        /// </summary>
        [Required]
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// The recipient bank code.
        /// </summary>
        [Required]
        [JsonPropertyName("bank_code")]
        public string BankCode { get; set; } = string.Empty;

        /// <summary>
        /// The recipient account name.
        /// </summary>
        [JsonPropertyName("account_name")]
        public string? AccountName { get; set; }

        /// <summary>
        /// The payout description/narration.
        /// </summary>
        [JsonPropertyName("narration")]
        public string? Narration { get; set; }

        /// <summary>
        /// The merchant key.
        /// </summary>
        [JsonPropertyName("merchant_key")]
        public string? MerchantKey { get; set; }

        /// <summary>
        /// Metadata for the payout.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// Response DTO for PayAza payout initiation.
    /// </summary>
    public class PayAzaPayoutResponse
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
        /// The payout data.
        /// </summary>
        [JsonPropertyName("data")]
        public PayAzaPayoutData? Data { get; set; }

        /// <summary>
        /// The error details if the request failed.
        /// </summary>
        [JsonPropertyName("error")]
        public PayAzaErrorDetails? Error { get; set; }
    }

    /// <summary>
    /// PayAza payout data.
    /// </summary>
    public class PayAzaPayoutData
    {
        /// <summary>
        /// The transaction reference.
        /// </summary>
        [JsonPropertyName("transaction_reference")]
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The payout status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The amount paid out.
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The transaction fee.
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }

        /// <summary>
        /// The timestamp when the payout was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}

