using Microsoft.Extensions.Logging;
using Modules.PaymentService.Constants;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Repositories;
using Modules.PaymentService.Resources.LocalisedStrings;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service implementation for processing payment webhook events.
    /// </summary>
    public class WebhookProcessingService : IWebhookProcessingService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<WebhookProcessingService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookProcessingService"/> class.
        /// </summary>
        /// <param name="paymentRepository">The payment repository.</param>
        /// <param name="logger">The logger instance.</param>
        public WebhookProcessingService(
            IPaymentRepository paymentRepository,
            ILogger<WebhookProcessingService> logger)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<WebhookProcessingResult> ProcessWebhookAsync(
            PayAzaWebhookPayload payload,
            CancellationToken cancellationToken = default)
        {
            if (payload == null)
            {
                _logger.LogWarning("Webhook processing failed: Payload is null");
                return WebhookProcessingResult.FailureResult("Webhook payload is null");
            }

            if (string.IsNullOrWhiteSpace(payload.TransactionReference))
            {
                _logger.LogWarning("Webhook processing failed: Transaction reference is missing");
                return WebhookProcessingResult.FailureResult("Transaction reference is required");
            }

            _logger.LogInformation(
                "Processing webhook for transaction {TransactionReference}, event {Event}, status {Status}",
                payload.TransactionReference, payload.Event, payload.Status);

            try
            {
                // Get payment by reference
                var payment = await _paymentRepository.GetByReferenceAsync(
                    payload.TransactionReference, 
                    cancellationToken);

                if (payment == null)
                {
                    _logger.LogWarning(
                        "Payment not found for transaction reference {TransactionReference}",
                        payload.TransactionReference);
                    return WebhookProcessingResult.FailureResult(
                        PaymentMessages.PaymentNotFound, 
                        payload.TransactionReference);
                }

                // Check for duplicate webhook
                var webhookEventId = GenerateWebhookEventId(payload);
                if (await IsDuplicateWebhookAsync(payload.TransactionReference, webhookEventId, cancellationToken))
                {
                    _logger.LogInformation(
                        "Duplicate webhook detected for transaction {TransactionReference}, event ID {EventId}",
                        payload.TransactionReference, webhookEventId);
                    return WebhookProcessingResult.DuplicateResult(
                        payload.TransactionReference, 
                        "Webhook already processed (duplicate detected)");
                }

                // Map webhook status to internal payment status
                var previousStatus = payment.Status;
                var newStatus = MapWebhookStatusToPaymentStatus(payload.Status, payload.Event);

                // Update payment details
                payment.Status = newStatus;
                payment.TransactionId = payload.TransactionId ?? payment.TransactionId;
                payment.PaymentMethod = payload.PaymentMethod ?? payment.PaymentMethod;
                payment.UpdatedAt = DateTime.UtcNow;

                // Set completed timestamp for successful payments
                if (newStatus == PaymentStatus.Completed || newStatus == PaymentStatus.Confirmed)
                {
                    payment.CompletedAt = payload.CompletedAt ?? DateTime.UtcNow;
                }

                // Store webhook metadata
                if (payload.Metadata != null || !string.IsNullOrWhiteSpace(payload.ErrorMessage))
                {
                    var metadata = new Dictionary<string, object>();
                    
                    if (payment.GatewayMetadata != null)
                    {
                        try
                        {
                            var existing = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(payment.GatewayMetadata);
                            if (existing != null)
                            {
                                foreach (var kvp in existing)
                                {
                                    metadata[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                        catch
                        {
                            // Ignore deserialization errors for existing metadata
                        }
                    }

                    // Add webhook-specific metadata
                    metadata["webhook_event"] = payload.Event;
                    metadata["webhook_received_at"] = DateTime.UtcNow;
                    
                    if (payload.Fee.HasValue)
                        metadata["gateway_fee"] = payload.Fee.Value;
                    
                    if (!string.IsNullOrWhiteSpace(payload.ErrorMessage))
                        metadata["error_message"] = payload.ErrorMessage;
                    
                    if (!string.IsNullOrWhiteSpace(payload.ErrorCode))
                        metadata["error_code"] = payload.ErrorCode;

                    // Merge with payload metadata
                    if (payload.Metadata != null)
                    {
                        foreach (var kvp in payload.Metadata)
                        {
                            metadata[$"webhook_{kvp.Key}"] = kvp.Value;
                        }
                    }

                    payment.GatewayMetadata = System.Text.Json.JsonSerializer.Serialize(metadata);
                }

                // Update idempotency tracking
                payment.LastWebhookEventId = webhookEventId;
                payment.LastWebhookReceivedAt = DateTime.UtcNow;
                payment.WebhookCount += 1;

                // Save changes
                await _paymentRepository.UpdateAsync(payment, cancellationToken);

                _logger.LogInformation(
                    "Webhook processed successfully for payment {PaymentId}. Status changed from {PreviousStatus} to {NewStatus}",
                    payment.Id, previousStatus, newStatus);

                return WebhookProcessingResult.SuccessResult(
                    payment.Id,
                    payment.PaymentReference,
                    newStatus,
                    $"Payment status updated to {newStatus}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing webhook for transaction {TransactionReference}",
                    payload.TransactionReference);
                return WebhookProcessingResult.FailureResult(
                    $"Error processing webhook: {ex.Message}",
                    payload.TransactionReference);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsDuplicateWebhookAsync(
            string transactionReference,
            string webhookEventId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transactionReference) || string.IsNullOrWhiteSpace(webhookEventId))
                return false;

            var payment = await _paymentRepository.GetByReferenceAsync(transactionReference, cancellationToken);

            if (payment == null)
                return false;

            // Check if the same webhook event ID was already processed
            return payment.LastWebhookEventId == webhookEventId;
        }

        /// <summary>
        /// Generates a unique event ID for the webhook based on its content.
        /// </summary>
        /// <param name="payload">The webhook payload.</param>
        /// <returns>A unique event ID.</returns>
        private static string GenerateWebhookEventId(PayAzaWebhookPayload payload)
        {
            // Combine key fields to create a unique identifier
            var uniqueString = $"{payload.TransactionReference}_{payload.Event}_{payload.Status}_{payload.TransactionId ?? "null"}";
            
            // Generate a hash for the event
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(uniqueString));
            return Convert.ToHexString(hashBytes).ToLower()[..16]; // Use first 16 characters
        }

        /// <summary>
        /// Maps PayAza webhook status to internal payment status.
        /// </summary>
        /// <param name="webhookStatus">The status from the webhook.</param>
        /// <param name="webhookEvent">The webhook event type.</param>
        /// <returns>The internal payment status.</returns>
        private static string MapWebhookStatusToPaymentStatus(string webhookStatus, string webhookEvent)
        {
            // Normalize status for comparison
            var normalizedStatus = webhookStatus?.ToLower() ?? string.Empty;
            var normalizedEvent = webhookEvent?.ToLower() ?? string.Empty;

            // Check event type first for more specific mapping
            if (normalizedEvent.Contains("success") || normalizedEvent.Contains("completed"))
                return PaymentStatus.Completed;

            if (normalizedEvent.Contains("failed") || normalizedEvent.Contains("failure"))
                return PaymentStatus.Failed;

            if (normalizedEvent.Contains("cancelled") || normalizedEvent.Contains("canceled"))
                return PaymentStatus.Cancelled;

            // Fall back to status mapping
            return normalizedStatus switch
            {
                "success" or "successful" or "completed" => PaymentStatus.Completed,
                "confirmed" => PaymentStatus.Confirmed,
                "pending" => PaymentStatus.Pending,
                "failed" or "failure" => PaymentStatus.Failed,
                "cancelled" or "canceled" => PaymentStatus.Cancelled,
                "expired" => PaymentStatus.Expired,
                _ => PaymentStatus.Failed // Default to failed for unknown statuses
            };
        }
    }
}

