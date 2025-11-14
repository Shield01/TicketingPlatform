using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Services;
using Shared.Kernel.Extensions;

namespace Modules.PaymentService.Controllers
{
    /// <summary>
    /// Controller for managing payment operations including payment initiation and webhook handling.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    // [SwaggerTag("Payment management operations including payment initiation, webhook handling, and transaction history")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IWebhookValidationService _webhookValidationService;
        private readonly IWebhookProcessingService _webhookProcessingService;

        public PaymentController(
            ILogger<PaymentController> logger, 
            IPaymentService paymentService,
            IWebhookValidationService webhookValidationService,
            IWebhookProcessingService webhookProcessingService)
        {
            _logger = logger;
            _paymentService = paymentService;
            _webhookValidationService = webhookValidationService;
            _webhookProcessingService = webhookProcessingService;
        }

        /// <summary>
        /// Creates a new payment session and returns a redirect URL to the PayAza payment page.
        /// </summary>
        /// <param name="request">The payment session creation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Payment session details with redirect URL.</returns>
        /// <response code="200">Payment session created successfully.</response>
        /// <response code="400">Invalid payment data provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="409">Duplicate transaction reference.</response>
        [HttpPost("create-session")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Create a payment session",
            Description = "Creates a new payment session and generates a redirect URL to PayAza payment page. The transaction is stored with PENDING_REDIRECT status.",
            OperationId = "CreatePaymentSession",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(200, "Payment session created successfully", typeof(CreateSessionResponse))]
        [SwaggerResponse(400, "Invalid payment data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(409, "Duplicate transaction reference")]
        public async Task<IActionResult> CreateSession(
            [FromBody] CreateSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Creating payment session for user {UserId}, event {EventId}, amount {Amount}",
                    request.UserId, request.EventId, request.Amount);

                var response = await _paymentService.CreateSessionAsync(request, cancellationToken);

                _logger.LogInformation(
                    "Payment session created successfully: {PaymentId}, Reference: {Reference}",
                    response.PaymentId, response.TransactionReference);

                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Duplicate transaction reference"))
            {
                _logger.LogWarning("Duplicate transaction reference: {Message}", ex.Message);
                return Conflict(new { Message = "Duplicate transaction reference. Please try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment session");
                return StatusCode(500, new { Message = "An error occurred while creating the payment session." });
            }
        }

        /// <summary>
        /// Handles web redirect callback from PayAza payment page.
        /// </summary>
        /// <param name="request">The callback data from the payment gateway.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Callback processing result.</returns>
        /// <response code="200">Callback processed successfully.</response>
        /// <response code="400">Invalid callback data.</response>
        /// <response code="404">Payment not found.</response>
        [HttpPost("web-redirect-callback")]
        [AllowAnonymous] // Allow anonymous as this is called by payment gateway
        [SwaggerOperation(
            Summary = "Handle web redirect callback",
            Description = "Processes the callback when user is redirected back from PayAza payment page. Updates the payment status based on the payment outcome.",
            OperationId = "HandleWebRedirectCallback",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(200, "Callback processed successfully", typeof(WebRedirectCallbackResponse))]
        [SwaggerResponse(400, "Invalid callback data")]
        [SwaggerResponse(404, "Payment not found")]
        public async Task<IActionResult> HandleWebRedirectCallback(
            [FromBody] WebRedirectCallbackRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Handling web redirect callback for reference {Reference}, status {Status}",
                    request.TransactionReference, request.Status);

                var response = await _paymentService.HandleWebRedirectCallbackAsync(request, cancellationToken);

                if (!response.Success && response.PaymentId == Guid.Empty)
                {
                    return NotFound(response);
                }

                _logger.LogInformation(
                    "Web redirect callback processed successfully: {PaymentId}, Status: {Status}",
                    response.PaymentId, response.Status);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing web redirect callback");
                return StatusCode(500, new { Message = "An error occurred while processing the callback." });
            }
        }

        /// <summary>
        /// Handles payment webhook notifications from PayAza payment gateway.
        /// This endpoint validates HMAC SHA512 signatures, handles idempotency, and updates payment transaction status.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Webhook processing result.</returns>
        /// <response code="200">Webhook processed successfully or already processed (duplicate).</response>
        /// <response code="400">Invalid webhook data or missing required fields.</response>
        /// <response code="401">Invalid or missing webhook signature.</response>
        [HttpPost("webhook")]
        [AllowAnonymous] // Allow anonymous as this is called by payment gateway
        [SwaggerOperation(
            Summary = "Handle PayAza payment webhook",
            Description = @"Processes webhook notifications from PayAza payment gateway to update payment transaction status.
            
**Security:** Validates webhook authenticity using HMAC SHA512 signature from x-payaza-signature header.

**Idempotency:** Automatically detects and ignores duplicate webhook events based on transaction reference and event fingerprint.

**Status Mapping:** Maps PayAza webhook events to internal payment statuses (COMPLETED, FAILED, PENDING, CANCELLED).

**Example Webhook Events:**
- collection.success - Payment successfully completed
- collection.failed - Payment failed
- transfer.completed - Payout completed
- transfer.failed - Payout failed

**Required Headers:**
- x-payaza-signature: Base64-encoded HMAC SHA512 signature of the request body",
            OperationId = "ProcessPaymentWebhook",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(200, "Webhook processed successfully", typeof(WebhookProcessingResult))]
        [SwaggerResponse(400, "Invalid webhook data")]
        [SwaggerResponse(401, "Invalid or missing webhook signature")]
        public async Task<IActionResult> ProcessWebhook(CancellationToken cancellationToken = default)
        {
            try
            {
                // Read raw request body
                string rawPayload;
                using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    rawPayload = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(rawPayload))
                {
                    _logger.LogWarning("Webhook received with empty payload");
                    return BadRequest(new { Message = "Webhook payload is empty" });
                }

                _logger.LogInformation("Webhook received, payload length: {Length} bytes", rawPayload.Length);

                // Get signature from header
                var signature = Request.Headers["x-payaza-signature"].ToString();

                if (string.IsNullOrWhiteSpace(signature))
                {
                    _logger.LogWarning("Webhook received without signature header");
                    return Unauthorized(new { Message = "Missing x-payaza-signature header" });
                }

                // Validate signature using HMAC SHA512
                if (!_webhookValidationService.ValidateSignature(rawPayload, signature))
                {
                    _logger.LogWarning("Webhook signature validation failed");
                    return Unauthorized(new { Message = "Invalid webhook signature" });
                }

                _logger.LogInformation("Webhook signature validated successfully");

                // Parse payload
                PayAzaWebhookPayload? payload;
                try
                {
                    payload = System.Text.Json.JsonSerializer.Deserialize<PayAzaWebhookPayload>(
                        rawPayload, 
                        new System.Text.Json.JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse webhook payload");
                    return BadRequest(new { Message = "Invalid JSON payload" });
                }

                if (payload == null)
                {
                    _logger.LogWarning("Webhook payload deserialized to null");
                    return BadRequest(new { Message = "Failed to parse webhook payload" });
                }

                _logger.LogInformation(
                    "Processing webhook for transaction {TransactionReference}, event {Event}, status {Status}",
                    payload.TransactionReference, payload.Event, payload.Status);

                // Process webhook
                var result = await _webhookProcessingService.ProcessWebhookAsync(payload, cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Webhook processing failed for transaction {TransactionReference}: {Message}",
                        payload.TransactionReference, result.Message);
                    return BadRequest(result);
                }

                if (result.IsDuplicate)
                {
                    _logger.LogInformation(
                        "Duplicate webhook detected for transaction {TransactionReference}",
                        payload.TransactionReference);
                }
                else
                {
                    _logger.LogInformation(
                        "Webhook processed successfully for payment {PaymentId}, status: {Status}",
                        result.PaymentId, result.Status);
                }

                // Always return 200 OK for processed or duplicate webhooks
                // This prevents PayAza from retrying successfully processed webhooks
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing webhook");
                
                // Return 200 OK to prevent retries for unexpected errors
                // The payment can be reconciled via TSQ (Transaction Status Query)
                return Ok(new { 
                    Success = false, 
                    Message = "Webhook received but encountered processing error. Transaction will be reconciled via status query." 
                });
            }
        }

        /// <summary>
        /// Retrieves payment history for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="page">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100).</param>
        /// <returns>Paginated list of user's payment transactions.</returns>
        /// <response code="200">Payment history retrieved successfully.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to view this payment history.</response>
        [HttpGet("user-history")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Get user payment history",
            Description = "Retrieves a paginated list of payment transactions for the authenticated user.",
            OperationId = "GetUserPaymentHistory",
            Tags = new[] { "Payments" }
        )]
        [SwaggerResponse(200, "Payment history retrieved successfully", typeof(PaginatedPaymentHistoryResponse))]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to view this payment history")]
        public async Task<IActionResult> GetUserPaymentHistory(
            [FromQuery] Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Payment history retrieval attempt for user {UserId}, page {Page}", userId, page);
            
            // TODO: Implement actual payment history retrieval logic
            var transactions = new List<PaymentTransactionResponse>
            {
                new PaymentTransactionResponse
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EventId = Guid.NewGuid(),
                    EventName = "Tech Conference 2024",
                    Amount = 150.00m,
                    Currency = "NGN",
                    Status = "Completed",
                    Gateway = "Payaza",
                    Reference = "TXN_20240115120000",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CompletedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(5)
                },
                new PaymentTransactionResponse
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EventId = Guid.NewGuid(),
                    EventName = "Music Festival 2024",
                    Amount = 75.00m,
                    Currency = "NGN",
                    Status = "Pending",
                    Gateway = "Flutterwave",
                    Reference = "TXN_20240115130000",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            var response = new PaginatedPaymentHistoryResponse
            {
                Transactions = transactions,
                TotalCount = 2,
                Page = page,
                PageSize = pageSize,
                TotalPages = 1
            };

            return Ok(response);
        }
    }
} 