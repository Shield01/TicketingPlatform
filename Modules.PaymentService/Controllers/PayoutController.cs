using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Resources.LocalisedStrings;
using Modules.PaymentService.Services;
using Shared.Kernel.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace Modules.PaymentService.Controllers
{
    /// <summary>
    /// Controller for managing payout operations including payout initiation, account verification, and payout tracking.
    /// </summary>
    [ApiController]
    [Route("api/payments/payouts")]
    [Produces("application/json")]
    [Authorize(Policy = "AdminOnly")] // Admin/Finance roles only
    public class PayoutController : ControllerBase
    {
        private readonly ILogger<PayoutController> _logger;
        private readonly IPayoutService _payoutService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayoutController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="payoutService">The payout service.</param>
        public PayoutController(
            ILogger<PayoutController> logger,
            IPayoutService payoutService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _payoutService = payoutService ?? throw new ArgumentNullException(nameof(payoutService));
        }

        /// <summary>
        /// Initiates a new payout transaction.
        /// </summary>
        /// <param name="request">The payout initiation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Payout transaction details.</returns>
        /// <response code="200">Payout initiated successfully.</response>
        /// <response code="400">Invalid payout data provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized (Admin/Finance role required).</response>
        /// <response code="409">Duplicate transaction reference.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("initiate")]
        [SwaggerOperation(
            Summary = "Initiate a payout",
            Description = @"Initiates a new payout transaction to a recipient bank account via PayAza. 
            
**Features:**
- Validates account details before payout
- Generates unique transaction reference
- Integrates with PayAza payout API
- Tracks payout status (INITIATED → PROCESSING → COMPLETED/FAILED)
- Supports dry-run mode for preview
- Stores payout metadata and gateway fee

**Security:**
- Requires Admin or Finance role
- Validates duplicate transaction references
- Logs all payout attempts

**Example Request:**
```json
{
  ""amount"": 50000.00,
  ""currency"": ""NGN"",
  ""accountNumber"": ""0123456789"",
  ""bankCode"": ""058"",
  ""accountName"": ""John Doe"",
  ""narration"": ""Event payout for EVENT-12345"",
  ""recipientUserId"": ""99887766-5544-3322-1100-998877665544"",
  ""eventId"": ""11223344-5566-7788-9900-112233445566"",
  ""isDryRun"": false
}
```",
            OperationId = "InitiatePayout",
            Tags = new[] { "Payouts" }
        )]
        [SwaggerResponse(200, "Payout initiated successfully", typeof(PayoutResponse))]
        [SwaggerResponse(400, "Invalid payout data or validation error")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized (Admin/Finance role required)")]
        [SwaggerResponse(409, "Duplicate transaction reference")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> InitiatePayout(
            [FromBody] InitiatePayoutRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                if (!userId.HasValue || userId.Value == Guid.Empty)
                {
                    _logger.LogWarning("Payout initiation attempted without valid user ID");
                    return Unauthorized(new { Message = "User not authenticated." });
                }

                _logger.LogInformation(
                    "Initiating payout for user {UserId}, amount {Amount} {Currency}, account {AccountNumber}",
                    userId.Value, request.Amount, request.Currency, request.AccountNumber);

                var response = await _payoutService.InitiatePayoutAsync(request, userId.Value, cancellationToken);

                _logger.LogInformation(
                    "Payout initiated successfully: {PayoutId}, Reference: {Reference}, Status: {Status}",
                    response.PayoutId, response.TransactionReference, response.Status);

                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Duplicate payout reference: {Message}", ex.Message);
                return Conflict(new { Message = PaymentMessages.DuplicatePayoutReference });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid payout data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payout");
                return StatusCode(500, new { Message = PaymentMessages.PayoutInitiationFailed });
            }
        }

        /// <summary>
        /// Verifies a bank account before payout.
        /// </summary>
        /// <param name="request">The account enquiry request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Account verification result.</returns>
        /// <response code="200">Account verified successfully.</response>
        /// <response code="400">Invalid account data provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized (Admin/Finance role required).</response>
        /// <response code="404">Account not found.</response>
        [HttpPost("account-enquiry")]
        [SwaggerOperation(
            Summary = "Verify bank account",
            Description = @"Verifies a bank account before initiating a payout. Returns account name and bank details.

**Features:**
- Validates account number and bank code
- Returns verified account name
- Checks account existence
- Provides bank name

**Usage:**
Always call this endpoint before initiating a payout to verify the recipient's account details.

**Example Request:**
```json
{
  ""accountNumber"": ""0123456789"",
  ""bankCode"": ""058""
}
```

**Example Response:**
```json
{
  ""success"": true,
  ""accountNumber"": ""0123456789"",
  ""accountName"": ""John Doe"",
  ""bankCode"": ""058"",
  ""bankName"": ""GTBank"",
  ""currency"": ""NGN"",
  ""message"": ""Account verified successfully.""
}
```",
            OperationId = "VerifyAccount",
            Tags = new[] { "Payouts" }
        )]
        [SwaggerResponse(200, "Account verified successfully", typeof(AccountEnquiryResponse))]
        [SwaggerResponse(400, "Invalid account data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        [SwaggerResponse(404, "Account not found")]
        public async Task<IActionResult> VerifyAccount(
            [FromBody] AccountEnquiryRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Verifying account {AccountNumber} at bank {BankCode}",
                    request.AccountNumber, request.BankCode);

                var response = await _payoutService.VerifyAccountAsync(request, cancellationToken);

                if (response.Success)
                {
                    _logger.LogInformation(
                        "Account verified successfully: {AccountName}",
                        response.AccountName);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning(
                        "Account verification failed: {Message}",
                        response.ErrorMessage);
                    
                    if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        return NotFound(response);
                    
                    return BadRequest(response);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid account enquiry data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during account verification");
                return StatusCode(500, new { Message = PaymentMessages.AccountEnquiryError });
            }
        }

        /// <summary>
        /// Previews a payout without executing it (dry-run).
        /// </summary>
        /// <param name="request">The payout preview request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Preview of payout details.</returns>
        /// <response code="200">Payout preview generated successfully.</response>
        /// <response code="400">Invalid payout data provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized (Admin/Finance role required).</response>
        [HttpPost("preview")]
        [SwaggerOperation(
            Summary = "Preview a payout (dry-run)",
            Description = @"Previews a payout without executing it. Creates a dry-run payout record for review.

**Features:**
- Validates payout data without execution
- Generates transaction reference
- Stores preview in database with IsDryRun flag
- Does not initiate actual payout with gateway

**Use Case:**
Use this endpoint to preview payout details and fees before initiating the actual payout.

**Example Request:**
```json
{
  ""amount"": 50000.00,
  ""currency"": ""NGN"",
  ""accountNumber"": ""0123456789"",
  ""bankCode"": ""058"",
  ""accountName"": ""John Doe"",
  ""narration"": ""Preview payout"",
  ""isDryRun"": true
}
```",
            OperationId = "PreviewPayout",
            Tags = new[] { "Payouts" }
        )]
        [SwaggerResponse(200, "Payout preview generated successfully", typeof(PayoutResponse))]
        [SwaggerResponse(400, "Invalid payout data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        public async Task<IActionResult> PreviewPayout(
            [FromBody] InitiatePayoutRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                if (!userId.HasValue || userId.Value == Guid.Empty)
                {
                    _logger.LogWarning("Payout preview attempted without valid user ID");
                    return Unauthorized(new { Message = "User not authenticated." });
                }

                _logger.LogInformation(
                    "Previewing payout for user {UserId}, amount {Amount} {Currency}",
                    userId.Value, request.Amount, request.Currency);

                var response = await _payoutService.PreviewPayoutAsync(request, userId.Value, cancellationToken);

                _logger.LogInformation(
                    "Payout preview created: {PayoutId}, Reference: {Reference}",
                    response.PayoutId, response.TransactionReference);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid payout preview data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing payout");
                return StatusCode(500, new { Message = "An error occurred while previewing the payout." });
            }
        }

        /// <summary>
        /// Gets payout transaction details by ID.
        /// </summary>
        /// <param name="payoutId">The payout transaction ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Payout transaction details.</returns>
        /// <response code="200">Payout details retrieved successfully.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized (Admin/Finance role required).</response>
        /// <response code="404">Payout not found.</response>
        [HttpGet("{payoutId}")]
        [SwaggerOperation(
            Summary = "Get payout by ID",
            Description = "Retrieves detailed information about a specific payout transaction.",
            OperationId = "GetPayoutById",
            Tags = new[] { "Payouts" }
        )]
        [SwaggerResponse(200, "Payout details retrieved successfully", typeof(PayoutResponse))]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        [SwaggerResponse(404, "Payout not found")]
        public async Task<IActionResult> GetPayoutById(
            [FromRoute] Guid payoutId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting payout by ID: {PayoutId}", payoutId);

                var response = await _payoutService.GetPayoutByIdAsync(payoutId, cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning("Payout not found: {PayoutId}", payoutId);
                    return NotFound(new { Message = PaymentMessages.PayoutNotFound });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payout {PayoutId}", payoutId);
                return StatusCode(500, new { Message = "An error occurred while retrieving the payout." });
            }
        }

        /// <summary>
        /// Gets all payout transactions with optional filtering.
        /// </summary>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Page size (default: 20, max: 100).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of payout transactions.</returns>
        /// <response code="200">Payouts retrieved successfully.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized (Admin/Finance role required).</response>
        [HttpGet("my-payouts")]
        [SwaggerOperation(
            Summary = "Get user's payout history",
            Description = "Retrieves a paginated list of payout transactions initiated by the current user.",
            OperationId = "GetMyPayouts",
            Tags = new[] { "Payouts" }
        )]
        [SwaggerResponse(200, "Payouts retrieved successfully")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        public async Task<IActionResult> GetMyPayouts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                if (!userId.HasValue || userId.Value == Guid.Empty)
                {
                    _logger.LogWarning("Payout history request without valid user ID");
                    return Unauthorized(new { Message = "User not authenticated." });
                }

                // Enforce max page size
                if (pageSize > 100)
                    pageSize = 100;

                _logger.LogDebug(
                    "Getting payout history for user {UserId}, page {Page}, pageSize {PageSize}",
                    userId.Value, page, pageSize);

                var result = await _payoutService.GetPayoutsByUserIdAsync(
                    userId.Value, page, pageSize, cancellationToken);
                var payouts = result.Payouts;
                var totalCount = result.TotalCount;

                return Ok(new
                {
                    Payouts = payouts,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payout history");
                return StatusCode(500, new { Message = "An error occurred while retrieving payout history." });
            }
        }

        /// <summary>
        /// Gets account details including payout statistics.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Account details with payout statistics.</returns>
        /// <response code="200">Account details retrieved successfully.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized (Admin/Finance role required).</response>
        [HttpGet("account-details")]
        [SwaggerOperation(
            Summary = "Get account details",
            Description = @"Retrieves account details including payout statistics and recent payouts.

**Returns:**
- Total payouts count
- Total payout amount
- Completed/pending/failed payouts counts
- Recent payout transactions (last 10)

**Example Response:**
```json
{
  ""totalPayouts"": 150,
  ""totalAmount"": 7500000.00,
  ""currency"": ""NGN"",
  ""pendingPayoutsCount"": 5,
  ""completedPayoutsCount"": 140,
  ""failedPayoutsCount"": 5,
  ""recentPayouts"": [...]
}
```",
            OperationId = "GetAccountDetails",
            Tags = new[] { "Payouts" }
        )]
        [SwaggerResponse(200, "Account details retrieved successfully", typeof(AccountDetailsResponse))]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        public async Task<IActionResult> GetAccountDetails(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting account details with payout statistics");

                var response = await _payoutService.GetAccountDetailsAsync(cancellationToken);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account details");
                return StatusCode(500, new { Message = "An error occurred while retrieving account details." });
            }
        }
    }
}

