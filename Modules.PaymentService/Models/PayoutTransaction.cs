using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.Models
{
    /// <summary>
    /// Model representing a payout transaction (transfer to recipient account).
    /// </summary>
    public class PayoutTransaction
    {
        /// <summary>
        /// The unique identifier of the payout transaction.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the user initiating the payout (admin/finance).
        /// </summary>
        [Required]
        public Guid InitiatedByUserId { get; set; }

        /// <summary>
        /// The unique identifier of the recipient user (if internal).
        /// </summary>
        public Guid? RecipientUserId { get; set; }

        /// <summary>
        /// The unique identifier of the event related to this payout (optional).
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// The unique transaction reference for this payout.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The payout amount.
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency of the payout.
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// The recipient account number.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// The recipient bank code.
        /// </summary>
        [Required]
        [StringLength(10)]
        public string BankCode { get; set; } = string.Empty;

        /// <summary>
        /// The recipient bank name.
        /// </summary>
        [StringLength(200)]
        public string? BankName { get; set; }

        /// <summary>
        /// The recipient account name (verified via account enquiry).
        /// </summary>
        [Required]
        [StringLength(200)]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// The payout narration/description.
        /// </summary>
        [StringLength(500)]
        public string? Narration { get; set; }

        /// <summary>
        /// The current status of the payout.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = PayoutStatus.INITIATED;

        /// <summary>
        /// The payment gateway used (e.g., "PayAza").
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Gateway { get; set; } = "PayAza";

        /// <summary>
        /// The gateway transaction ID (if provided by gateway).
        /// </summary>
        [StringLength(100)]
        public string? GatewayTransactionId { get; set; }

        /// <summary>
        /// The gateway fee charged for this payout.
        /// </summary>
        public decimal? GatewayFee { get; set; }

        /// <summary>
        /// Metadata from the payment gateway (stored as JSON).
        /// </summary>
        public string? GatewayMetadata { get; set; }

        /// <summary>
        /// Whether this is a dry-run/preview payout (not executed).
        /// </summary>
        public bool IsDryRun { get; set; } = false;

        /// <summary>
        /// The date and time when the payout was initiated.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the payout was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the payout was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Whether the payout record is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Error message if the payout failed.
        /// </summary>
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Error code if the payout failed.
        /// </summary>
        [StringLength(50)]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Validates the payout transaction data.
        /// </summary>
        /// <returns>A tuple indicating validation result and error message.</returns>
        public (bool IsValid, string? ErrorMessage) Validate()
        {
            if (Amount <= 0)
                return (false, "Amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(TransactionReference))
                return (false, "Transaction reference is required.");

            if (string.IsNullOrWhiteSpace(AccountNumber))
                return (false, "Account number is required.");

            if (string.IsNullOrWhiteSpace(BankCode))
                return (false, "Bank code is required.");

            if (string.IsNullOrWhiteSpace(AccountName))
                return (false, "Account name is required.");

            if (string.IsNullOrWhiteSpace(Currency) || Currency.Length != 3)
                return (false, "Valid 3-letter currency code is required.");

            if (!string.IsNullOrWhiteSpace(Narration) && Narration.Length > 500)
                return (false, "Narration cannot exceed 500 characters.");

            return (true, null);
        }

        /// <summary>
        /// Checks if the payout is in a final state (completed/failed/cancelled).
        /// </summary>
        public bool IsFinalState()
        {
            return Status == PayoutStatus.COMPLETED ||
                   Status == PayoutStatus.FAILED ||
                   Status == PayoutStatus.CANCELLED ||
                   Status == PayoutStatus.REVERSED;
        }

        /// <summary>
        /// Marks the payout as completed.
        /// </summary>
        public void MarkAsCompleted(string? gatewayTransactionId = null, decimal? gatewayFee = null)
        {
            Status = PayoutStatus.COMPLETED;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(gatewayTransactionId))
                GatewayTransactionId = gatewayTransactionId;

            if (gatewayFee.HasValue)
                GatewayFee = gatewayFee.Value;
        }

        /// <summary>
        /// Marks the payout as failed.
        /// </summary>
        public void MarkAsFailed(string errorMessage, string? errorCode = null)
        {
            Status = PayoutStatus.FAILED;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the payout as processing.
        /// </summary>
        public void MarkAsProcessing()
        {
            Status = PayoutStatus.PROCESSING;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Constants for payout transaction statuses.
    /// </summary>
    public static class PayoutStatus
    {
        /// <summary>
        /// Payout has been initiated and awaiting processing.
        /// </summary>
        public const string INITIATED = "INITIATED";

        /// <summary>
        /// Payout is being processed by the gateway.
        /// </summary>
        public const string PROCESSING = "PROCESSING";

        /// <summary>
        /// Payout was completed successfully.
        /// </summary>
        public const string COMPLETED = "COMPLETED";

        /// <summary>
        /// Payout failed.
        /// </summary>
        public const string FAILED = "FAILED";

        /// <summary>
        /// Payout was cancelled.
        /// </summary>
        public const string CANCELLED = "CANCELLED";

        /// <summary>
        /// Payout was reversed.
        /// </summary>
        public const string REVERSED = "REVERSED";

        /// <summary>
        /// Payout is pending approval (for multi-level approval systems).
        /// </summary>
        public const string PENDING_APPROVAL = "PENDING_APPROVAL";
    }
}

