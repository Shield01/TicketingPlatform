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

        public TicketController(ILogger<TicketController> logger, ITicketTierService ticketTierService)
        {
            _logger = logger;
            _ticketTierService = ticketTierService;
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
            Description = "Creates multiple ticket tiers (VIP, Regular, Early Bird) for a specific event.",
            OperationId = "CreateTicketTiers",
            Tags = new[] { "Tickets" }
        )]
        [SwaggerResponse(201, "Ticket tiers created successfully", typeof(List<TicketTierResponse>))]
        [SwaggerResponse(400, "Invalid ticket data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to create tickets")]
        public async Task<IActionResult> CreateTicketTiers([FromBody] CreateTicketTiersRequest request)
        {
            _logger.LogInformation("Ticket tiers creation attempt for event {EventId}", request.EventId);
            
            // TODO: Implement actual ticket tier creation logic
            var response = request.Tiers.Select(tier => new TicketTierResponse
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                Name = tier.Name,
                Description = tier.Description,
                Price = tier.Price,
                Currency = "USD",
                MaxQuantity = tier.Quantity,
                SoldQuantity = 0,
                IsAvailable = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            return CreatedAtAction(nameof(GetEventTickets), new { eventId = request.EventId }, response);
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
            
            // TODO: Implement actual ticket verification logic
            var response = new TicketVerificationResponse
            {
                IsValid = true,
                TicketId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventName = "Sample Event",
                TicketTier = "VIP",
                AttendeeName = "John Doe",
                VerifiedAt = DateTime.UtcNow,
                Message = "Ticket verified successfully"
            };

            return Ok(response);
        }
    }
} 