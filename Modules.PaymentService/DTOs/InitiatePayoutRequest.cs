using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Request DTO for initiating a payout.
    /// </summary>
    public class InitiatePayoutRequest
    {
        /// <summary>
        /// The payout amount.
        /// </summary>
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code (e.g., "NGN", "USD").
        /// </summary>
        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code.")]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be uppercase 3-letter code.")]
        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// The recipient account number.
        /// </summary>
        [Required(ErrorMessage = "Account number is required.")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Account number must be between 10 and 50 characters.")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// The recipient bank code.
        /// </summary>
        [Required(ErrorMessage = "Bank code is required.")]
        [StringLength(10, ErrorMessage = "Bank code cannot exceed 10 characters.")]
        public string BankCode { get; set; } = string.Empty;

        /// <summary>
        /// The recipient account name (should be verified first via account enquiry).
        /// </summary>
        [Required(ErrorMessage = "Account name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Account name must be between 2 and 200 characters.")]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// The payout narration/description.
        /// </summary>
        [StringLength(500, ErrorMessage = "Narration cannot exceed 500 characters.")]
        public string? Narration { get; set; }

        /// <summary>
        /// The recipient user ID (optional, for internal tracking).
        /// </summary>
        public Guid? RecipientUserId { get; set; }

        /// <summary>
        /// The event ID related to this payout (optional).
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Whether this is a dry-run/preview (does not execute the payout).
        /// </summary>
        public bool IsDryRun { get; set; } = false;

        /// <summary>
        /// Additional metadata for the payout.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Unique transaction reference (optional, will be generated if not provided).
        /// </summary>
        [StringLength(100, ErrorMessage = "Transaction reference cannot exceed 100 characters.")]
        public string? TransactionReference { get; set; }
    }

    /// <summary>
    /// Response DTO for payout initiation.
    /// </summary>
    public class PayoutResponse
    {
        /// <summary>
        /// The unique payout transaction ID.
        /// </summary>
        public Guid PayoutId { get; set; }

        /// <summary>
        /// The transaction reference.
        /// </summary>
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The payout amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the payout.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The recipient account number.
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// The recipient account name.
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// The recipient bank code.
        /// </summary>
        public string BankCode { get; set; } = string.Empty;

        /// <summary>
        /// The recipient bank name.
        /// </summary>
        public string? BankName { get; set; }

        /// <summary>
        /// The gateway transaction ID (if available).
        /// </summary>
        public string? GatewayTransactionId { get; set; }

        /// <summary>
        /// The gateway fee (if available).
        /// </summary>
        public decimal? GatewayFee { get; set; }

        /// <summary>
        /// The narration/description.
        /// </summary>
        public string? Narration { get; set; }

        /// <summary>
        /// Whether this is a dry-run/preview.
        /// </summary>
        public bool IsDryRun { get; set; }

        /// <summary>
        /// The date and time when the payout was initiated.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the payout was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Error message if the payout failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

