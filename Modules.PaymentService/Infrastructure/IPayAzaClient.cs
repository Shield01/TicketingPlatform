using Modules.PaymentService.Infrastructure.DTOs;

namespace Modules.PaymentService.Infrastructure
{
    /// <summary>
    /// Interface for PayAza payment gateway client operations.
    /// </summary>
    public interface IPayAzaClient
    {
        /// <summary>
        /// Gets account details from PayAza.
        /// </summary>
        /// <param name="accountNumber">The account number to query.</param>
        /// <param name="bankCode">The bank code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The account details response.</returns>
        Task<PayAzaAccountDetailsResponse> GetAccountDetailsAsync(
            string accountNumber, 
            string bankCode, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Initiates a payout transaction through PayAza.
        /// </summary>
        /// <param name="request">The payout request details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The payout response.</returns>
        Task<PayAzaPayoutResponse> InitiatePayoutAsync(
            PayAzaPayoutRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the status of a transaction from PayAza.
        /// </summary>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transaction status response.</returns>
        Task<PayAzaTransactionStatusResponse> GetTransactionStatusAsync(
            string transactionReference, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates PayAza webhook signature.
        /// </summary>
        /// <param name="payload">The webhook payload.</param>
        /// <param name="signature">The webhook signature.</param>
        /// <returns>True if the signature is valid, false otherwise.</returns>
        bool ValidateWebhookSignature(string payload, string signature);

        /// <summary>
        /// Gets the current configuration mode (test or live).
        /// </summary>
        /// <returns>The current mode.</returns>
        string GetCurrentMode();

        /// <summary>
        /// Checks if the client is configured and ready to use.
        /// </summary>
        /// <returns>True if the client is ready, false otherwise.</returns>
        bool IsConfigured();
    }
}

