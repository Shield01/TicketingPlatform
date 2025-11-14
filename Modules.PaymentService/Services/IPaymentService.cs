using Modules.PaymentService.DTOs;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service interface for payment operations.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a new payment session and generates a redirect URL for the payment page.
        /// </summary>
        /// <param name="request">The payment session creation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payment session response with redirect URL.</returns>
        Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles the web redirect callback from the payment gateway.
        /// </summary>
        /// <param name="request">The callback request from the payment gateway.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The callback processing result.</returns>
        Task<WebRedirectCallbackResponse> HandleWebRedirectCallbackAsync(WebRedirectCallbackRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the payment status for a transaction reference.
        /// </summary>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payment transaction response.</returns>
        Task<PaymentTransactionResponse?> GetPaymentStatusAsync(string transactionReference, CancellationToken cancellationToken = default);
    }
}

