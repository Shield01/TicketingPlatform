using Modules.PaymentService.DTOs;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service interface for payout operations.
    /// </summary>
    public interface IPayoutService
    {
        /// <summary>
        /// Initiates a new payout transaction.
        /// </summary>
        /// <param name="request">The payout request details.</param>
        /// <param name="initiatedByUserId">The user ID initiating the payout.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payout response.</returns>
        Task<PayoutResponse> InitiatePayoutAsync(
            InitiatePayoutRequest request, 
            Guid initiatedByUserId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies an account before payout.
        /// </summary>
        /// <param name="request">The account enquiry request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The account verification response.</returns>
        Task<AccountEnquiryResponse> VerifyAccountAsync(
            AccountEnquiryRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payout transaction details by ID.
        /// </summary>
        /// <param name="payoutId">The payout ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payout response if found, null otherwise.</returns>
        Task<PayoutResponse?> GetPayoutByIdAsync(
            Guid payoutId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payout transaction details by reference.
        /// </summary>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payout response if found, null otherwise.</returns>
        Task<PayoutResponse?> GetPayoutByReferenceAsync(
            string transactionReference, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payout transactions initiated by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of payout responses and total count.</returns>
        Task<(List<PayoutResponse> Payouts, int TotalCount)> GetPayoutsByUserIdAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 20, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets account details including payout statistics.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Account details response.</returns>
        Task<AccountDetailsResponse> GetAccountDetailsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Previews a payout without executing it (dry-run).
        /// </summary>
        /// <param name="request">The payout request details.</param>
        /// <param name="initiatedByUserId">The user ID initiating the preview.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The preview payout response.</returns>
        Task<PayoutResponse> PreviewPayoutAsync(
            InitiatePayoutRequest request, 
            Guid initiatedByUserId, 
            CancellationToken cancellationToken = default);
    }
}

