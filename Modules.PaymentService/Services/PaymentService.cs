using Microsoft.Extensions.Logging;
using Modules.PaymentService.Configuration;
using Modules.PaymentService.Constants;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Infrastructure.Helpers;
using Modules.PaymentService.Models;
using Modules.PaymentService.Repositories;
using Modules.PaymentService.Resources.LocalisedStrings;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service implementation for payment operations.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly PayAzaConfiguration _payAzaConfiguration;
        private readonly ILogger<PaymentService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentService"/> class.
        /// </summary>
        /// <param name="paymentRepository">The payment repository.</param>
        /// <param name="payAzaConfiguration">The PayAza configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public PaymentService(
            IPaymentRepository paymentRepository,
            PayAzaConfiguration payAzaConfiguration,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _payAzaConfiguration = payAzaConfiguration ?? throw new ArgumentNullException(nameof(payAzaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CreateSessionResponse> CreateSessionAsync(
            CreateSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogInformation(
                "Creating payment session for user {UserId}, event {EventId}, amount {Amount} {Currency}",
                request.UserId, request.EventId, request.Amount, request.Currency);

            // Generate unique transaction reference
            var transactionReference = TransactionReferenceGenerator.GenerateForEvent(request.EventId);

            // Check if transaction reference already exists (very unlikely, but validate for safety)
            if (await _paymentRepository.ReferenceExistsAsync(transactionReference, cancellationToken))
            {
                _logger.LogWarning("Duplicate transaction reference generated: {Reference}", transactionReference);
                throw new InvalidOperationException($"Duplicate transaction reference: {transactionReference}");
            }

            // Create payment entity
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                EventId = request.EventId,
                PaymentReference = transactionReference,
                Gateway = "PayAza",
                Amount = request.Amount,
                Currency = request.Currency,
                Status = PaymentStatus.PendingRedirect,
                Description = $"Ticket purchase for event {request.EventId}",
                IsActive = true
            };

            // Create payment item for the ticket
            var paymentItem = new PaymentItem
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                ItemType = "Ticket",
                ItemId = request.TicketTierId,
                ItemName = $"Event Ticket - {request.EventId}",
                Quantity = request.Quantity,
                UnitPrice = request.Amount / request.Quantity,
                TotalPrice = request.Amount,
                Currency = request.Currency,
                IsActive = true
            };

            payment.PaymentItems.Add(paymentItem);

            // Save to database
            var createdPayment = await _paymentRepository.CreateAsync(payment, cancellationToken);

            // Build PayAza payment page URL
            var redirectUrl = BuildPayAzaRedirectUrl(
                transactionReference,
                request.Amount,
                request.Currency,
                request.CustomerEmail,
                request.CustomerName,
                request.CustomerPhone,
                request.SuccessUrl,
                request.CancelUrl);

            _logger.LogInformation(
                "Payment session created successfully: {PaymentId}, Reference: {Reference}",
                createdPayment.Id, transactionReference);

            // Return response
            return new CreateSessionResponse
            {
                PaymentId = createdPayment.Id,
                TransactionReference = transactionReference,
                RedirectUrl = redirectUrl,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = PaymentStatus.PendingRedirect,
                Gateway = "PayAza",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30), // 30-minute expiration
                CreatedAt = createdPayment.CreatedAt
            };
        }

        /// <inheritdoc/>
        public async Task<WebRedirectCallbackResponse> HandleWebRedirectCallbackAsync(
            WebRedirectCallbackRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogInformation(
                "Handling web redirect callback for reference {Reference}, status {Status}",
                request.TransactionReference, request.Status);

            // Get payment by reference
            var payment = await _paymentRepository.GetByReferenceAsync(request.TransactionReference, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for reference {Reference}", request.TransactionReference);
                return new WebRedirectCallbackResponse
                {
                    PaymentId = Guid.Empty,
                    TransactionReference = request.TransactionReference,
                    Status = PaymentStatus.Failed,
                    Message = PaymentMessages.PaymentNotFound,
                    Success = false
                };
            }

            // Map gateway status to internal status
            var internalStatus = MapGatewayStatus(request.Status);

            // Update payment status
            payment.Status = internalStatus;
            payment.TransactionId = request.GatewayTransactionId;
            payment.PaymentMethod = request.PaymentMethod;
            payment.UpdatedAt = DateTime.UtcNow;

            if (internalStatus == PaymentStatus.Completed || internalStatus == PaymentStatus.Confirmed)
            {
                payment.CompletedAt = DateTime.UtcNow;
            }

            // Store metadata if provided
            if (request.Metadata != null && request.Metadata.Any())
            {
                payment.GatewayMetadata = System.Text.Json.JsonSerializer.Serialize(request.Metadata);
            }

            // Save updates
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            _logger.LogInformation(
                "Payment callback processed: {PaymentId}, Status: {Status}",
                payment.Id, internalStatus);

            return new WebRedirectCallbackResponse
            {
                PaymentId = payment.Id,
                TransactionReference = payment.PaymentReference,
                Status = internalStatus,
                Message = GetStatusMessage(internalStatus),
                Success = internalStatus == PaymentStatus.Completed || internalStatus == PaymentStatus.Confirmed,
                RedirectUrl = null // Can be populated with custom redirect URL if needed
            };
        }

        /// <inheritdoc/>
        public async Task<PaymentTransactionResponse?> GetPaymentStatusAsync(
            string transactionReference,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transactionReference))
                throw new ArgumentException("Transaction reference cannot be null or empty", nameof(transactionReference));

            _logger.LogDebug("Getting payment status for reference {Reference}", transactionReference);

            var payment = await _paymentRepository.GetByReferenceAsync(transactionReference, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for reference {Reference}", transactionReference);
                return null;
            }

            return new PaymentTransactionResponse
            {
                Id = payment.Id,
                UserId = payment.UserId,
                EventId = payment.EventId,
                EventName = $"Event {payment.EventId}", // TODO: Fetch actual event name from EventService
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                Gateway = payment.Gateway,
                Reference = payment.PaymentReference,
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }

        /// <summary>
        /// Builds the PayAza payment page redirect URL.
        /// </summary>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="amount">The payment amount.</param>
        /// <param name="currency">The payment currency.</param>
        /// <param name="customerEmail">The customer's email address.</param>
        /// <param name="customerName">The customer's name.</param>
        /// <param name="customerPhone">The customer's phone number.</param>
        /// <param name="successUrl">The success redirect URL.</param>
        /// <param name="cancelUrl">The cancel redirect URL.</param>
        /// <returns>The PayAza payment page URL.</returns>
        private string BuildPayAzaRedirectUrl(
            string transactionReference,
            decimal amount,
            string currency,
            string customerEmail,
            string customerName,
            string? customerPhone,
            string? successUrl,
            string? cancelUrl)
        {
            // PayAza Payment Page URL format
            // https://checkout-test.payaza.africa or https://checkout.payaza.africa
            var baseUrl = _payAzaConfiguration.IsTestMode
                ? "https://checkout-test.payaza.africa"
                : "https://checkout.payaza.africa";

            // Build query parameters
            var queryParams = new List<string>
            {
                $"transaction_reference={Uri.EscapeDataString(transactionReference)}",
                $"amount={amount:F2}",
                $"currency={Uri.EscapeDataString(currency)}",
                $"merchant_key={Uri.EscapeDataString(_payAzaConfiguration.MerchantKey)}",
                $"email={Uri.EscapeDataString(customerEmail)}",
                $"name={Uri.EscapeDataString(customerName)}"
            };

            if (!string.IsNullOrWhiteSpace(customerPhone))
            {
                queryParams.Add($"phone={Uri.EscapeDataString(customerPhone)}");
            }

            if (!string.IsNullOrWhiteSpace(successUrl))
            {
                queryParams.Add($"success_url={Uri.EscapeDataString(successUrl)}");
            }

            if (!string.IsNullOrWhiteSpace(cancelUrl))
            {
                queryParams.Add($"cancel_url={Uri.EscapeDataString(cancelUrl)}");
            }

            var redirectUrl = $"{baseUrl}?{string.Join("&", queryParams)}";

            _logger.LogDebug("Built PayAza redirect URL for reference {Reference}", transactionReference);

            return redirectUrl;
        }

        /// <summary>
        /// Maps gateway status to internal payment status.
        /// </summary>
        /// <param name="gatewayStatus">The status from the payment gateway.</param>
        /// <returns>The internal payment status.</returns>
        private static string MapGatewayStatus(string gatewayStatus)
        {
            return gatewayStatus?.ToLower() switch
            {
                "success" or "successful" or "completed" => PaymentStatus.Completed,
                "confirmed" => PaymentStatus.Confirmed,
                "pending" => PaymentStatus.Pending,
                "failed" or "failure" => PaymentStatus.Failed,
                "cancelled" or "canceled" => PaymentStatus.Cancelled,
                "expired" => PaymentStatus.Expired,
                _ => PaymentStatus.Failed
            };
        }

        /// <summary>
        /// Gets a user-friendly message for a payment status.
        /// </summary>
        /// <param name="status">The payment status.</param>
        /// <returns>A user-friendly status message.</returns>
        private static string GetStatusMessage(string status)
        {
            return status switch
            {
                PaymentStatus.Completed => PaymentMessages.PaymentSuccessful,
                PaymentStatus.Confirmed => PaymentMessages.PaymentConfirmed,
                PaymentStatus.Pending => PaymentMessages.PaymentPending,
                PaymentStatus.Failed => PaymentMessages.PaymentFailed,
                PaymentStatus.Cancelled => PaymentMessages.PaymentCancelled,
                PaymentStatus.Expired => "Payment session has expired.",
                _ => "Payment status unknown."
            };
        }
    }
}

