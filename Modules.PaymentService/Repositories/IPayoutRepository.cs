using Modules.PaymentService.Models;

namespace Modules.PaymentService.Repositories
{
    /// <summary>
    /// Repository interface for payout transaction operations.
    /// </summary>
    public interface IPayoutRepository
    {
        /// <summary>
        /// Creates a new payout transaction.
        /// </summary>
        /// <param name="payout">The payout transaction to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created payout transaction.</returns>
        Task<PayoutTransaction> CreateAsync(PayoutTransaction payout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a payout transaction by ID.
        /// </summary>
        /// <param name="id">The payout ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payout transaction if found, null otherwise.</returns>
        Task<PayoutTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a payout transaction by transaction reference.
        /// </summary>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payout transaction if found, null otherwise.</returns>
        Task<PayoutTransaction?> GetByReferenceAsync(string transactionReference, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing payout transaction.
        /// </summary>
        /// <param name="payout">The payout transaction to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated payout transaction.</returns>
        Task<PayoutTransaction> UpdateAsync(PayoutTransaction payout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a transaction reference already exists.
        /// </summary>
        /// <param name="transactionReference">The transaction reference to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the reference exists, false otherwise.</returns>
        Task<bool> ReferenceExistsAsync(string transactionReference, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all payout transactions initiated by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of payout transactions.</returns>
        Task<(List<PayoutTransaction> Payouts, int TotalCount)> GetByUserIdAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 20, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all payout transactions for a specific recipient user.
        /// </summary>
        /// <param name="recipientUserId">The recipient user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of payout transactions.</returns>
        Task<(List<PayoutTransaction> Payouts, int TotalCount)> GetByRecipientUserIdAsync(
            Guid recipientUserId, 
            int page = 1, 
            int pageSize = 20, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all payout transactions for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of payout transactions.</returns>
        Task<List<PayoutTransaction>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all payout transactions with a specific status.
        /// </summary>
        /// <param name="statuses">The payout statuses to filter by.</param>
        /// <param name="since">Optional filter to get payouts created since a specific date.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of payout transactions.</returns>
        Task<List<PayoutTransaction>> GetByStatusAsync(
            string[] statuses, 
            DateTime? since = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payout statistics for admin dashboard.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Payout statistics.</returns>
        Task<PayoutStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Payout statistics DTO.
    /// </summary>
    public class PayoutStatistics
    {
        public int TotalPayouts { get; set; }
        public int CompletedPayouts { get; set; }
        public int FailedPayouts { get; set; }
        public int PendingPayouts { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalFees { get; set; }
        public string Currency { get; set; } = "NGN";
    }
}

