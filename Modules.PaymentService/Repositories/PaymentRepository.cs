using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.PaymentService.Data;
using Modules.PaymentService.Models;

namespace Modules.PaymentService.Repositories
{
    /// <summary>
    /// Repository implementation for payment operations.
    /// </summary>
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentServiceDbContext _context;
        private readonly ILogger<PaymentRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public PaymentRepository(PaymentServiceDbContext context, ILogger<PaymentRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            _logger.LogInformation("Creating payment with reference {Reference}", payment.PaymentReference);

            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.Payments.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment created successfully with ID {PaymentId}", payment.Id);

            return payment;
        }

        /// <inheritdoc/>
        public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving payment with ID {PaymentId}", id);

            return await _context.Payments
                .Include(p => p.PaymentItems)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException("Reference cannot be null or empty", nameof(reference));

            _logger.LogDebug("Retrieving payment with reference {Reference}", reference);

            return await _context.Payments
                .Include(p => p.PaymentItems)
                .FirstOrDefaultAsync(p => p.PaymentReference == reference && p.IsActive, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            _logger.LogInformation("Updating payment with ID {PaymentId}", payment.Id);

            payment.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment updated successfully with ID {PaymentId}", payment.Id);

            return payment;
        }

        /// <inheritdoc/>
        public async Task<bool> ReferenceExistsAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException("Reference cannot be null or empty", nameof(reference));

            _logger.LogDebug("Checking if payment reference exists: {Reference}", reference);

            return await _context.Payments
                .AnyAsync(p => p.PaymentReference == reference && p.IsActive, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetByUserIdAsync(
            Guid userId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving payments for user {UserId}, page {Page}, pageSize {PageSize}", 
                userId, page, pageSize);

            var query = _context.Payments
                .Include(p => p.PaymentItems)
                .Where(p => p.UserId == userId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (payments, totalCount);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Payment>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving payments for event {EventId}", eventId);

            return await _context.Payments
                .Include(p => p.PaymentItems)
                .Where(p => p.EventId == eventId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}

