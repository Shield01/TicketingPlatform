using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Request DTO for account enquiry/verification.
    /// </summary>
    public class AccountEnquiryRequest
    {
        /// <summary>
        /// The account number to verify.
        /// </summary>
        [Required(ErrorMessage = "Account number is required.")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Account number must be between 10 and 50 characters.")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// The bank code.
        /// </summary>
        [Required(ErrorMessage = "Bank code is required.")]
        [StringLength(10, ErrorMessage = "Bank code cannot exceed 10 characters.")]
        public string BankCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for account enquiry.
    /// </summary>
    public class AccountEnquiryResponse
    {
        /// <summary>
        /// Whether the account verification was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The verified account number.
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// The verified account name.
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// The bank code.
        /// </summary>
        public string BankCode { get; set; } = string.Empty;

        /// <summary>
        /// The bank name.
        /// </summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// The account currency (if available).
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// The account balance (if available and authorized).
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// Message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error message if verification failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Response DTO for account details retrieval.
    /// </summary>
    public class AccountDetailsResponse
    {
        /// <summary>
        /// List of recent payout transactions.
        /// </summary>
        public List<PayoutResponse> RecentPayouts { get; set; } = new();

        /// <summary>
        /// Total number of payouts.
        /// </summary>
        public int TotalPayouts { get; set; }

        /// <summary>
        /// Total amount paid out.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The currency code.
        /// </summary>
        public string Currency { get; set; } = "NGN";

        /// <summary>
        /// Total pending payouts count.
        /// </summary>
        public int PendingPayoutsCount { get; set; }

        /// <summary>
        /// Total completed payouts count.
        /// </summary>
        public int CompletedPayoutsCount { get; set; }

        /// <summary>
        /// Total failed payouts count.
        /// </summary>
        public int FailedPayoutsCount { get; set; }
    }
}

