using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.PaymentService.Data;
using Modules.PaymentService.Models;

namespace Modules.PaymentService.Repositories
{
    /// <summary>
    /// Repository implementation for payout transaction operations.
    /// </summary>
    public class PayoutRepository : IPayoutRepository
    {
        private readonly PaymentServiceDbContext _context;
        private readonly ILogger<PayoutRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayoutRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger.</param>
        public PayoutRepository(PaymentServiceDbContext context, ILogger<PayoutRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new payout transaction.
        /// </summary>
        public async Task<PayoutTransaction> CreateAsync(PayoutTransaction payout, CancellationToken cancellationToken = default)
        {
            if (payout == null)
                throw new ArgumentNullException(nameof(payout));

            _logger.LogInformation("Creating payout transaction with reference {Reference}", payout.TransactionReference);

            payout.CreatedAt = DateTime.UtcNow;
            payout.UpdatedAt = DateTime.UtcNow;

            await _context.PayoutTransactions.AddAsync(payout, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payout transaction created with ID {Id}", payout.Id);

            return payout;
        }

        /// <summary>
        /// Gets a payout transaction by ID.
        /// </summary>
        public async Task<PayoutTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payout transaction by ID {Id}", id);

            return await _context.PayoutTransactions
                .Where(p => p.Id == id && p.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets a payout transaction by transaction reference.
        /// </summary>
        public async Task<PayoutTransaction?> GetByReferenceAsync(string transactionReference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transactionReference))
                throw new ArgumentException("Transaction reference cannot be null or empty.", nameof(transactionReference));

            _logger.LogDebug("Getting payout transaction by reference {Reference}", transactionReference);

            return await _context.PayoutTransactions
                .Where(p => p.TransactionReference == transactionReference && p.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an existing payout transaction.
        /// </summary>
        public async Task<PayoutTransaction> UpdateAsync(PayoutTransaction payout, CancellationToken cancellationToken = default)
        {
            if (payout == null)
                throw new ArgumentNullException(nameof(payout));

            _logger.LogInformation("Updating payout transaction with ID {Id}", payout.Id);

            payout.UpdatedAt = DateTime.UtcNow;

            _context.PayoutTransactions.Update(payout);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payout transaction updated with ID {Id}", payout.Id);

            return payout;
        }

        /// <summary>
        /// Checks if a transaction reference already exists.
        /// </summary>
        public async Task<bool> ReferenceExistsAsync(string transactionReference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transactionReference))
                throw new ArgumentException("Transaction reference cannot be null or empty.", nameof(transactionReference));

            return await _context.PayoutTransactions
                .AnyAsync(p => p.TransactionReference == transactionReference, cancellationToken);
        }

        /// <summary>
        /// Gets all payout transactions initiated by a specific user.
        /// </summary>
        public async Task<(List<PayoutTransaction> Payouts, int TotalCount)> GetByUserIdAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 20, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payout transactions for user {UserId}, page {Page}, pageSize {PageSize}", 
                userId, page, pageSize);

            var query = _context.PayoutTransactions
                .Where(p => p.InitiatedByUserId == userId && p.IsActive);

            var totalCount = await query.CountAsync(cancellationToken);

            var payouts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (payouts, totalCount);
        }

        /// <summary>
        /// Gets all payout transactions for a specific recipient user.
        /// </summary>
        public async Task<(List<PayoutTransaction> Payouts, int TotalCount)> GetByRecipientUserIdAsync(
            Guid recipientUserId, 
            int page = 1, 
            int pageSize = 20, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payout transactions for recipient {RecipientUserId}, page {Page}, pageSize {PageSize}", 
                recipientUserId, page, pageSize);

            var query = _context.PayoutTransactions
                .Where(p => p.RecipientUserId == recipientUserId && p.IsActive);

            var totalCount = await query.CountAsync(cancellationToken);

            var payouts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (payouts, totalCount);
        }

        /// <summary>
        /// Gets all payout transactions for a specific event.
        /// </summary>
        public async Task<List<PayoutTransaction>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payout transactions for event {EventId}", eventId);

            return await _context.PayoutTransactions
                .Where(p => p.EventId == eventId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all payout transactions with a specific status.
        /// </summary>
        public async Task<List<PayoutTransaction>> GetByStatusAsync(
            string[] statuses, 
            DateTime? since = null, 
            CancellationToken cancellationToken = default)
        {
            if (statuses == null || statuses.Length == 0)
                throw new ArgumentException("At least one status is required.", nameof(statuses));

            _logger.LogDebug("Getting payout transactions with statuses {Statuses}, since {Since}", 
                string.Join(", ", statuses), since);

            var query = _context.PayoutTransactions
                .Where(p => statuses.Contains(p.Status) && p.IsActive);

            if (since.HasValue)
                query = query.Where(p => p.CreatedAt >= since.Value);

            return await query
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets payout statistics for admin dashboard.
        /// </summary>
        public async Task<PayoutStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payout statistics");

            var payouts = await _context.PayoutTransactions
                .Where(p => p.IsActive && !p.IsDryRun)
                .ToListAsync(cancellationToken);

            var statistics = new PayoutStatistics
            {
                TotalPayouts = payouts.Count,
                CompletedPayouts = payouts.Count(p => p.Status == PayoutStatus.COMPLETED),
                FailedPayouts = payouts.Count(p => p.Status == PayoutStatus.FAILED),
                PendingPayouts = payouts.Count(p => p.Status == PayoutStatus.INITIATED || 
                                                     p.Status == PayoutStatus.PROCESSING),
                TotalAmount = payouts.Where(p => p.Status == PayoutStatus.COMPLETED).Sum(p => p.Amount),
                TotalFees = payouts.Where(p => p.Status == PayoutStatus.COMPLETED && p.GatewayFee.HasValue)
                                   .Sum(p => p.GatewayFee!.Value),
                Currency = "NGN"
            };

            _logger.LogInformation("Payout statistics: Total={Total}, Completed={Completed}, Failed={Failed}, Pending={Pending}", 
                statistics.TotalPayouts, statistics.CompletedPayouts, statistics.FailedPayouts, statistics.PendingPayouts);

            return statistics;
        }
    }
}

