using System.Text.Json.Serialization;

namespace Modules.PaymentService.Infrastructure.DTOs
{
    /// <summary>
    /// Response DTO for PayAza account details.
    /// </summary>
    public class PayAzaAccountDetailsResponse
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
        /// The account data.
        /// </summary>
        [JsonPropertyName("data")]
        public PayAzaAccountData? Data { get; set; }

        /// <summary>
        /// The error details if the request failed.
        /// </summary>
        [JsonPropertyName("error")]
        public PayAzaErrorDetails? Error { get; set; }
    }

    /// <summary>
    /// PayAza account data.
    /// </summary>
    public class PayAzaAccountData
    {
        /// <summary>
        /// The account number.
        /// </summary>
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// The account name.
        /// </summary>
        [JsonPropertyName("account_name")]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// The bank name.
        /// </summary>
        [JsonPropertyName("bank_name")]
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// The bank code.
        /// </summary>
        [JsonPropertyName("bank_code")]
        public string BankCode { get; set; } = string.Empty;

        /// <summary>
        /// The account balance.
        /// </summary>
        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }

        /// <summary>
        /// The account currency.
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "NGN";
    }
}

