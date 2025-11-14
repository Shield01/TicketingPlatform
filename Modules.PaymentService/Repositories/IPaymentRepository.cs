using Modules.PaymentService.Models;

namespace Modules.PaymentService.Repositories
{
    /// <summary>
    /// Repository interface for payment operations.
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Creates a new payment transaction in the database.
        /// </summary>
        /// <param name="payment">The payment entity to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created payment entity.</returns>
        Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a payment by its unique identifier.
        /// </summary>
        /// <param name="id">The payment ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payment entity if found, null otherwise.</returns>
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a payment by its transaction reference.
        /// </summary>
        /// <param name="reference">The transaction reference.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payment entity if found, null otherwise.</returns>
        Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing payment transaction.
        /// </summary>
        /// <param name="payment">The payment entity to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated payment entity.</returns>
        Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a payment reference already exists.
        /// </summary>
        /// <param name="reference">The transaction reference to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the reference exists, false otherwise.</returns>
        Task<bool> ReferenceExistsAsync(string reference, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all payments for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Page size (default: 10).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of payment entities.</returns>
        Task<(IEnumerable<Payment> Payments, int TotalCount)> GetByUserIdAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 10, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all payments for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of payment entities.</returns>
        Task<IEnumerable<Payment>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}

