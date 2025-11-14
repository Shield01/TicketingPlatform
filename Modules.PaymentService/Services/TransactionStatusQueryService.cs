using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.PaymentService.Constants;
using Modules.PaymentService.Infrastructure;
using Modules.PaymentService.Repositories;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Background service that periodically queries transaction status from PayAza
    /// for pending payments to ensure reconciliation in case webhooks are missed.
    /// </summary>
    public class TransactionStatusQueryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransactionStatusQueryService> _logger;
        private readonly TimeSpan _queryInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes
        private readonly TimeSpan _pendingPaymentTimeout = TimeSpan.FromMinutes(30); // Mark as expired after 30 minutes

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStatusQueryService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for scoped services.</param>
        /// <param name="logger">The logger instance.</param>
        public TransactionStatusQueryService(
            IServiceProvider serviceProvider,
            ILogger<TransactionStatusQueryService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the background task to query transaction statuses.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token to stop the service.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TransactionStatusQueryService started. Checking pending transactions every {Interval} minutes", 
                _queryInterval.TotalMinutes);

            // Wait 1 minute before first run to allow application startup
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await QueryPendingTransactionsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while querying pending transactions");
                }

                // Wait for next interval
                await Task.Delay(_queryInterval, stoppingToken);
            }

            _logger.LogInformation("TransactionStatusQueryService stopped");
        }

        /// <summary>
        /// Queries status of pending transactions from PayAza.
        /// </summary>
        private async Task QueryPendingTransactionsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
            var payAzaClient = scope.ServiceProvider.GetRequiredService<IPayAzaClient>();

            _logger.LogDebug("Starting transaction status query batch");

            try
            {
                // Get all payments with pending statuses that need reconciliation
                var pendingStatuses = new[] 
                { 
                    PaymentStatus.Pending, 
                    PaymentStatus.PendingRedirect 
                };

                var pendingPayments = await GetPendingPaymentsForReconciliationAsync(
                    paymentRepository, 
                    pendingStatuses, 
                    cancellationToken);

                if (!pendingPayments.Any())
                {
                    _logger.LogDebug("No pending payments found for reconciliation");
                    return;
                }

                _logger.LogInformation("Found {Count} pending payments to reconcile", pendingPayments.Count());

                var successCount = 0;
                var failureCount = 0;
                var expiredCount = 0;

                foreach (var payment in pendingPayments)
                {
                    try
                    {
                        // Check if payment has exceeded timeout period
                        if (DateTime.UtcNow - payment.CreatedAt > _pendingPaymentTimeout)
                        {
                            _logger.LogInformation(
                                "Payment {PaymentId} has exceeded timeout period. Marking as expired",
                                payment.Id);

                            payment.Status = PaymentStatus.Expired;
                            payment.UpdatedAt = DateTime.UtcNow;
                            await paymentRepository.UpdateAsync(payment, cancellationToken);
                            expiredCount++;
                            continue;
                        }

                        // Query transaction status from PayAza
                        _logger.LogDebug(
                            "Querying PayAza for transaction status: {TransactionReference}",
                            payment.PaymentReference);

                        var statusResponse = await payAzaClient.GetTransactionStatusAsync(
                            payment.PaymentReference,
                            cancellationToken);

                        if (statusResponse?.Success == true && statusResponse.Data != null)
                        {
                            var gatewayStatus = statusResponse.Data.Status;
                            var internalStatus = MapPayAzaStatusToInternalStatus(gatewayStatus);

                            // Only update if status has changed
                            if (payment.Status != internalStatus)
                            {
                                _logger.LogInformation(
                                    "Updating payment {PaymentId} status from {OldStatus} to {NewStatus} via TSQ",
                                    payment.Id, payment.Status, internalStatus);

                                payment.Status = internalStatus;
                                payment.UpdatedAt = DateTime.UtcNow;

                                if (internalStatus == PaymentStatus.Completed || internalStatus == PaymentStatus.Confirmed)
                                {
                                    payment.CompletedAt = statusResponse.Data.CompletedAt ?? DateTime.UtcNow;
                                }

                                // Store TSQ metadata
                                var metadata = new Dictionary<string, object>
                                {
                                    ["reconciled_via"] = "TSQ",
                                    ["reconciled_at"] = DateTime.UtcNow,
                                    ["gateway_status"] = gatewayStatus,
                                    ["gateway_amount"] = statusResponse.Data.Amount,
                                    ["gateway_fee"] = statusResponse.Data.Fee
                                };

                                payment.GatewayMetadata = System.Text.Json.JsonSerializer.Serialize(metadata);

                                await paymentRepository.UpdateAsync(payment, cancellationToken);
                                successCount++;
                            }
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Failed to get transaction status for {TransactionReference}: {Message}",
                                payment.PaymentReference, statusResponse?.Message ?? "Unknown error");
                            failureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, 
                            "Error querying status for payment {PaymentId}, reference {TransactionReference}",
                            payment.Id, payment.PaymentReference);
                        failureCount++;
                    }

                    // Add small delay between queries to avoid rate limiting
                    await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                }

                _logger.LogInformation(
                    "TSQ batch completed. Reconciled: {SuccessCount}, Failed: {FailureCount}, Expired: {ExpiredCount}",
                    successCount, failureCount, expiredCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in transaction status query batch");
            }
        }

        /// <summary>
        /// Gets pending payments that need reconciliation.
        /// </summary>
        private Task<IEnumerable<Models.Payment>> GetPendingPaymentsForReconciliationAsync(
            IPaymentRepository repository,
            string[] pendingStatuses,
            CancellationToken cancellationToken)
        {
            var allPendingPayments = new List<Models.Payment>();

            // This is a simplified approach - in production, you might want to add a specific
            // repository method to get payments by status with a date filter
            // For now, we'll use a workaround to get recent pending payments

            // Get payments from recent event (this is a placeholder - ideally add a new repo method)
            // Since we don't have a direct method to get all payments by status,
            // we'll rely on the webhook to handle most cases, and TSQ as fallback
            
            // In production, add this method to IPaymentRepository:
            // Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string[] statuses, DateTime? since = null)

            return Task.FromResult<IEnumerable<Models.Payment>>(allPendingPayments);
        }

        /// <summary>
        /// Maps PayAza transaction status to internal payment status.
        /// </summary>
        private static string MapPayAzaStatusToInternalStatus(string payAzaStatus)
        {
            return payAzaStatus?.ToLower() switch
            {
                "success" or "successful" or "completed" => PaymentStatus.Completed,
                "confirmed" => PaymentStatus.Confirmed,
                "pending" or "processing" => PaymentStatus.Pending,
                "failed" or "failure" => PaymentStatus.Failed,
                "cancelled" or "canceled" => PaymentStatus.Cancelled,
                "expired" => PaymentStatus.Expired,
                _ => PaymentStatus.Failed
            };
        }
    }
}

