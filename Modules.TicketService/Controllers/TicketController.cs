using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Modules.TicketService.DTOs;
using Modules.TicketService.Services;
using Shared.Kernel.Extensions;
using Shared.Kernel.Constants;

namespace Modules.TicketService.Controllers
{
    /// <summary>
    /// Controller for managing ticket operations including creation, verification, and retrieval.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    // [SwaggerTag("Ticket management operations including creation, verification, and retrieval")]
    public class TicketController : ControllerBase
    {
        private readonly ILogger<TicketController> _logger;
        private readonly ITicketTierService _ticketTierService;
        private readonly ITicketIssueService _ticketIssueService;
        private readonly IQRCodeService _qrCodeService;

        public TicketController(ILogger<TicketController> logger, ITicketTierService ticketTierService, ITicketIssueService ticketIssueService, IQRCodeService qrCodeService)
        {
            _logger = logger;
            _ticketTierService = ticketTierService;
            _ticketIssueService = ticketIssueService;
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Creates a ticket tier for an event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="request">The ticket tier creation request.</param>
        /// <returns>Created ticket tier with generated ID.</returns>
        /// <response code="201">Ticket tier created successfully.</response>
        /// <response code="400">Invalid ticket tier data provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to create ticket tiers for this event.</response>
        /// <response code="409">Ticket tier name already exists for this event.</response>
        [HttpPost("/api/events/{eventId:guid}/ticket-tiers")]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Create a ticket tier for an event",
            Description = "Creates a single ticket tier (VIP, Regular, Early Bird, etc.) for a specific event. Only event organizers or admins can create ticket tiers.",
            OperationId = "CreateEventTicketTier",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(201, "Ticket tier created successfully", typeof(TicketTierResponse))]
        [SwaggerResponse(400, "Invalid ticket tier data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to create ticket tiers for this event")]
        [SwaggerResponse(409, "Ticket tier name already exists for this event")]
        public async Task<IActionResult> CreateEventTicketTier(Guid eventId, [FromBody] CreateTicketTierRequest request)
        {
            _logger.LogInformation("Ticket tier creation attempt for event {EventId}", eventId);

            try
            {
                // Get the current user ID from JWT claims
                var userId = HttpContext.GetUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("User ID not found in claims for ticket tier creation");
                    return Unauthorized("User not authenticated.");
                }

                // TODO: Add authorization check to verify user can manage this event
                // This should integrate with EventService to verify event ownership
                // For now, we rely on the OrganiserOrAdmin policy

                var response = await _ticketTierService.CreateTicketTierAsync(eventId, request, userId.Value);

                _logger.LogInformation("Ticket tier created successfully: {TierId} for event {EventId}", response.Id, eventId);
                return CreatedAtAction(nameof(GetEventTickets), new { eventId = eventId }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid ticket tier creation request: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                _logger.LogWarning("Duplicate ticket tier name for event {EventId}: {ErrorMessage}", eventId, ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized ticket tier creation attempt: {ErrorMessage}", ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket tier for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while creating the ticket tier." });
            }
        }

        /// <summary>
        /// Creates ticket tiers for an event.
        /// </summary>
        /// <param name="request">The ticket creation request containing tier information.</param>
        /// <returns>Created ticket tiers with generated IDs.</returns>
        /// <response code="201">Ticket tiers created successfully.</response>
        /// <response code="400">Invalid ticket data provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to create tickets.</response>
        [HttpPost]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Create ticket tiers for an event",
            Description = "Creates multiple ticket tiers (VIP, Regular, Early Bird) for a specific event. Can handle partial success where some tiers are created successfully while others fail due to validation or duplicate names.",
            OperationId = "CreateTicketTiers",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(201, "All ticket tiers created successfully", typeof(List<TicketTierResponse>))]
        [SwaggerResponse(200, "Some ticket tiers created successfully (partial success)")]
        [SwaggerResponse(400, "Invalid ticket data or all tier creations failed")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to create tickets")]
        [SwaggerResponse(500, "Unexpected server error")]
        public async Task<IActionResult> CreateTicketTiers([FromBody] CreateTicketTiersRequest request)
        {
            _logger.LogInformation("Ticket tiers creation attempt for event {EventId} with {TierCount} tiers", 
                request.EventId, request.Tiers.Count);
            
            try
            {
                // Get the current user ID from JWT claims
                var userId = HttpContext.GetUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("User ID not found in claims for ticket tier creation");
                    return Unauthorized("User not authenticated.");
                }

                var createdTiers = new List<TicketTierResponse>();
                var errors = new List<string>();

                // Create each tier individually
                foreach (var tierRequest in request.Tiers)
                {
                    try
                    {
                        // Convert TicketTierRequest to CreateTicketTierRequest
                        var createTierRequest = new CreateTicketTierRequest
                        {
                            Name = tierRequest.Name,
                            Description = tierRequest.Description,
                            Price = tierRequest.Price,
                            Currency = "USD", // Default currency, could be made configurable
                            MaxQuantity = tierRequest.Quantity,
                            IsAvailable = true
                        };

                        var createdTier = await _ticketTierService.CreateTicketTierAsync(
                            request.EventId, createTierRequest, userId.Value);
                        
                        createdTiers.Add(createdTier);
                        
                        _logger.LogInformation("Created ticket tier {TierName} with ID {TierId} for event {EventId}", 
                            createdTier.Name, createdTier.Id, request.EventId);
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning("Invalid data for tier {TierName}: {ErrorMessage}", tierRequest.Name, ex.Message);
                        errors.Add($"Tier '{tierRequest.Name}': {ex.Message}");
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
                    {
                        _logger.LogWarning("Duplicate tier name {TierName} for event {EventId}: {ErrorMessage}", 
                            tierRequest.Name, request.EventId, ex.Message);
                        errors.Add($"Tier '{tierRequest.Name}': {ex.Message}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.LogWarning("Unauthorized tier creation attempt for {TierName}: {ErrorMessage}", 
                            tierRequest.Name, ex.Message);
                        errors.Add($"Tier '{tierRequest.Name}': Unauthorized to create tier for this event");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating tier {TierName} for event {EventId}", 
                            tierRequest.Name, request.EventId);
                        errors.Add($"Tier '{tierRequest.Name}': Failed to create tier");
                    }
                }

                // If we have some successful creations and some errors, return partial success
                if (createdTiers.Any() && errors.Any())
                {
                    _logger.LogWarning("Partial success: Created {SuccessCount} tiers, failed {FailureCount} tiers for event {EventId}", 
                        createdTiers.Count, errors.Count, request.EventId);
                    
                    return Ok(new
                    {
                        message = $"Partially created {createdTiers.Count} out of {request.Tiers.Count} ticket tiers",
                        createdTiers = createdTiers,
                        errors = errors
                    });
                }

                // If all failed
                if (!createdTiers.Any() && errors.Any())
                {
                    _logger.LogWarning("All tier creations failed for event {EventId}", request.EventId);
                    return BadRequest(new { 
                        error = "Failed to create any ticket tiers", 
                        details = errors 
                    });
                }

                // If all succeeded
                _logger.LogInformation("Successfully created {TierCount} ticket tiers for event {EventId}", 
                    createdTiers.Count, request.EventId);
                
                return CreatedAtAction(nameof(GetEventTickets), new { eventId = request.EventId }, createdTiers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating ticket tiers for event {EventId}", request.EventId);
                return StatusCode(500, new { error = "An unexpected error occurred while creating ticket tiers." });
            }
        }

        /// <summary>
        /// Retrieves all tickets for a specific event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>List of ticket tiers for the event.</returns>
        /// <response code="200">Event tickets retrieved successfully.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("event/{eventId}")]
        [SwaggerOperation(
            Summary = "Get tickets for an event",
            Description = "Retrieves all ticket tiers available for a specific event.",
            OperationId = "GetEventTickets",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(200, "Event tickets retrieved successfully", typeof(List<TicketTierResponse>))]
        [SwaggerResponse(404, "Event not found")]
        public async Task<IActionResult> GetEventTickets(Guid eventId)
        {
            _logger.LogInformation("Event tickets retrieval attempt for event {EventId}", eventId);
            
            try
            {
                var ticketTiers = await _ticketTierService.GetEventTicketTiersAsync(eventId);
                return Ok(ticketTiers);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid event ID for ticket retrieval: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket tiers for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving ticket tiers." });
            }
        }

        /// <summary>
        /// Verifies a ticket for event entry.
        /// </summary>
        /// <param name="request">The ticket verification request containing the ticket code.</param>
        /// <returns>Ticket verification result.</returns>
        /// <response code="200">Ticket verified successfully.</response>
        /// <response code="400">Invalid ticket code.</response>
        /// <response code="404">Ticket not found.</response>
        /// <response code="409">Ticket already used.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to verify tickets.</response>
        [HttpPost("verify")]
        [Authorize(Policy = "StaffOrHigher")]
        [SwaggerOperation(
            Summary = "Verify a ticket for entry",
            Description = "Verifies a ticket code and marks it as used for event entry.",
            OperationId = "VerifyTicket",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(200, "Ticket verified successfully", typeof(TicketVerificationResponse))]
        [SwaggerResponse(400, "Invalid ticket code")]
        [SwaggerResponse(404, "Ticket not found")]
        [SwaggerResponse(409, "Ticket already used")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to verify tickets")]
        public async Task<IActionResult> VerifyTicket([FromBody] TicketVerificationRequest request)
        {
            _logger.LogInformation("Ticket verification attempt for ticket {TicketCode}", request.TicketCode);
            
            try
            {
                var response = await _ticketIssueService.VerifyTicketAsync(request);
                
                if (response.IsValid)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying ticket {TicketCode}", request.TicketCode);
                return StatusCode(500, new { error = "An error occurred while verifying the ticket." });
            }
        }

        /// <summary>
        /// Issues tickets after payment confirmation.
        /// </summary>
        /// <param name="request">The ticket issuance request.</param>
        /// <returns>The issued tickets information.</returns>
        /// <response code="201">Tickets issued successfully.</response>
        /// <response code="400">Invalid ticket issuance data.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to issue tickets.</response>
        /// <response code="409">Insufficient ticket capacity or invalid payment.</response>
        [HttpPost("issue")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Issue tickets after payment confirmation",
            Description = "Issues one or more tickets for a user after payment has been confirmed. This endpoint should typically be called by the payment webhook. If PaymentId is not provided, a GUID will be auto-generated for testing purposes.",
            OperationId = "IssueTickets",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(201, "Tickets issued successfully", typeof(IssueTicketResponse))]
        [SwaggerResponse(400, "Invalid ticket issuance data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to issue tickets")]
        [SwaggerResponse(409, "Insufficient ticket capacity or invalid payment")]
        public async Task<IActionResult> IssueTickets([FromBody] IssueTicketRequest request)
        {
            _logger.LogInformation("Ticket issuance attempt for user {UserId}, event {EventId}, payment {PaymentId}", 
                request.UserId, request.EventId, request.PaymentId);

            try
            {
                var response = await _ticketIssueService.IssueTicketsAsync(request);
                
                _logger.LogInformation("Successfully issued {Count} tickets for user {UserId}", 
                    response.TicketsIssued, request.UserId);
                
                return CreatedAtAction(nameof(GetUserTickets), new { }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid ticket issuance request: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Ticket issuance failed: {ErrorMessage}", ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing tickets for user {UserId}", request.UserId);
                return StatusCode(500, new { error = "An error occurred while issuing tickets." });
            }
        }

        /// <summary>
        /// Gets all tickets for the authenticated user.
        /// </summary>
        /// <param name="page">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <param name="status">Optional status filter (UNUSED, USED, CANCELLED).</param>
        /// <returns>User's tickets with pagination information.</returns>
        /// <response code="200">User tickets retrieved successfully.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpGet("user")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Get user's tickets",
            Description = "Retrieves all tickets owned by the authenticated user with pagination and optional status filtering.",
            OperationId = "GetUserTickets",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(200, "User tickets retrieved successfully", typeof(UserTicketsResponse))]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> GetUserTickets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            // Get the current user ID from JWT claims
            var userId = HttpContext.GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("User ID not found in claims for ticket retrieval");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Getting tickets for user {UserId}, page {Page}, pageSize {PageSize}, status {Status}", 
                userId.Value, page, pageSize, status);

            try
            {
                var response = await _ticketIssueService.GetUserTicketsAsync(userId.Value, page, pageSize, status);
                
                _logger.LogInformation("Retrieved {Count} tickets for user {UserId}", 
                    response.Tickets.Count(), userId.Value);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for user {UserId}", userId.Value);
                return StatusCode(500, new { error = "An error occurred while retrieving tickets." });
            }
        }

        /// <summary>
        /// Gets a specific ticket by ID for the authenticated user.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket.</param>
        /// <returns>Ticket details if found and owned by the user.</returns>
        /// <response code="200">Ticket retrieved successfully.</response>
        /// <response code="404">Ticket not found or not owned by user.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpGet("{ticketId}")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Get ticket by ID",
            Description = "Retrieves a specific ticket by its ID. Users can only access their own tickets.",
            OperationId = "GetTicketById",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(200, "Ticket retrieved successfully", typeof(TicketResponse))]
        [SwaggerResponse(404, "Ticket not found or not owned by user")]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> GetTicketById(Guid ticketId)
        {
            // Get the current user ID from JWT claims
            var userId = HttpContext.GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("User ID not found in claims for ticket retrieval");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Getting ticket {TicketId} for user {UserId}", ticketId, userId.Value);

            try
            {
                var ticket = await _ticketIssueService.GetTicketByIdAsync(ticketId, userId.Value);
                
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found or not owned by user {UserId}", ticketId, userId.Value);
                    return NotFound(new { error = "Ticket not found or you don't have access to this ticket." });
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId} for user {UserId}", ticketId, userId.Value);
                return StatusCode(500, new { error = "An error occurred while retrieving the ticket." });
            }
        }

        /// <summary>
        /// Cancels a ticket if it hasn't been used.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket to cancel.</param>
        /// <returns>Cancellation confirmation.</returns>
        /// <response code="200">Ticket cancelled successfully.</response>
        /// <response code="400">Ticket cannot be cancelled (already used or invalid state).</response>
        /// <response code="404">Ticket not found or not owned by user.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpPost("{ticketId}/cancel")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Cancel a ticket",
            Description = "Cancels a ticket if it hasn't been used yet. Users can only cancel their own tickets.",
            OperationId = "CancelTicket",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(200, "Ticket cancelled successfully")]
        [SwaggerResponse(400, "Ticket cannot be cancelled")]
        [SwaggerResponse(404, "Ticket not found or not owned by user")]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> CancelTicket(Guid ticketId)
        {
            // Get the current user ID from JWT claims
            var userId = HttpContext.GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("User ID not found in claims for ticket cancellation");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Cancelling ticket {TicketId} for user {UserId}", ticketId, userId.Value);

            try
            {
                var success = await _ticketIssueService.CancelTicketAsync(ticketId, userId.Value);
                
                if (success)
                {
                    _logger.LogInformation("Ticket {TicketId} cancelled successfully for user {UserId}", ticketId, userId.Value);
                    return Ok(new { message = "Ticket cancelled successfully." });
                }
                else
                {
                    return BadRequest(new { error = "Failed to cancel ticket." });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized ticket cancellation attempt: {ErrorMessage}", ex.Message);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid ticket cancellation request: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId} for user {UserId}", ticketId, userId.Value);
                return StatusCode(500, new { error = "An error occurred while cancelling the ticket." });
            }
        }

        /// <summary>
        /// Gets the QR code for a specific ticket.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket.</param>
        /// <returns>QR code information including image and data.</returns>
        /// <response code="200">QR code retrieved successfully.</response>
        /// <response code="404">Ticket not found or not owned by user.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpGet("{ticketId}/qr")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Get QR code for a ticket",
            Description = "Retrieves the QR code information for a specific ticket including the QR code image and data. Users can only access QR codes for their own tickets.",
            OperationId = "GetTicketQRCode",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(200, "QR code retrieved successfully", typeof(QRCodeResponse))]
        [SwaggerResponse(404, "Ticket not found or not owned by user")]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> GetTicketQRCode(Guid ticketId)
        {
            // Get the current user ID from JWT claims
            var userId = HttpContext.GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("User ID not found in claims for QR code retrieval");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Getting QR code for ticket {TicketId} for user {UserId}", ticketId, userId.Value);

            try
            {
                var ticket = await _ticketIssueService.GetTicketByIdAsync(ticketId, userId.Value);
                
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found or not owned by user {UserId}", ticketId, userId.Value);
                    return NotFound(new { error = "Ticket not found or you don't have access to this ticket." });
                }

                // Generate QR code response
                var qrResponse = new QRCodeResponse
                {
                    TicketId = ticket.Id,
                    TicketCode = ticket.TicketCode,
                    QRCodeData = ticket.QRCodeData ?? string.Empty,
                    QRCodeImage = ticket.QRCodeImage ?? string.Empty,
                    ImageMimeType = "image/png",
                    ImageSize = 512,
                    GeneratedAt = DateTime.UtcNow,
                    IsValidForUse = ticket.IsValidForUse,
                    Status = ticket.Status
                };

                _logger.LogInformation("Successfully retrieved QR code for ticket {TicketId}", ticketId);
                return Ok(qrResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving QR code for ticket {TicketId} for user {UserId}", ticketId, userId.Value);
                return StatusCode(500, new { error = "An error occurred while retrieving the QR code." });
            }
        }

        /// <summary>
        /// Validates a QR code for event entry.
        /// </summary>
        /// <param name="request">The QR code validation request containing the QR code data.</param>
        /// <returns>QR code validation result with comprehensive ticket details.</returns>
        /// <response code="200">QR code validated successfully and ticket marked as used.</response>
        /// <response code="400">Invalid QR code data or ticket cannot be used.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to validate QR codes.</response>
        [HttpPost("validate-qr")]
        [Authorize(Policy = "StaffOrHigher")]
        [SwaggerOperation(
            Summary = "Validate a QR code for entry",
            Description = "Validates a QR code and marks the associated ticket as used for event entry. This endpoint is designed for event organizers to scan QR codes at the entrance. No need to extract ticket codes - just send the raw QR code data.",
            OperationId = "ValidateQRCode",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(200, "QR code validated successfully and ticket marked as used", typeof(TicketVerificationResponse))]
        [SwaggerResponse(400, "Invalid QR code data or ticket cannot be used", typeof(TicketVerificationResponse))]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to validate QR codes")]
        public async Task<IActionResult> ValidateQRCode([FromBody] QRCodeValidationRequest request)
        {
            _logger.LogInformation("QR code validation attempt");

            try
            {
                var response = await _ticketIssueService.ValidateQRCodeAsync(request);
                
                if (response.IsValid)
                {
                    _logger.LogInformation("QR code validated successfully for ticket {TicketId}", response.TicketId);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("QR code validation failed: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating QR code");
                return StatusCode(500, new { error = "An error occurred while validating the QR code." });
            }
        }
    }
} 