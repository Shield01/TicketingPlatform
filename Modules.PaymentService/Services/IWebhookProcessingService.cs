using Modules.PaymentService.DTOs;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service interface for processing payment webhook events.
    /// </summary>
    public interface IWebhookProcessingService
    {
        /// <summary>
        /// Processes a webhook payload from PayAza.
        /// </summary>
        /// <param name="payload">The webhook payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of webhook processing.</returns>
        Task<WebhookProcessingResult> ProcessWebhookAsync(
            PayAzaWebhookPayload payload, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a webhook has already been processed (idempotency check).
        /// </summary>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="webhookEventId">The webhook event ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the webhook has already been processed, false otherwise.</returns>
        Task<bool> IsDuplicateWebhookAsync(
            string transactionReference, 
            string webhookEventId,
            CancellationToken cancellationToken = default);
    }
}

