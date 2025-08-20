using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Modules.TicketService.DTOs;
using Shared.Kernel.Extensions;

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

        public TicketController(ILogger<TicketController> logger)
        {
            _logger = logger;
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
                Quantity = tier.Quantity,
                AvailableQuantity = tier.Quantity,
                CreatedAt = DateTime.UtcNow
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
            
            // TODO: Implement actual event tickets retrieval logic
            var response = new List<TicketTierResponse>
            {
                new TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "VIP",
                    Description = "Premium access with exclusive benefits",
                    Price = 150.00m,
                    Quantity = 50,
                    AvailableQuantity = 35,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Regular",
                    Description = "Standard access to the event",
                    Price = 75.00m,
                    Quantity = 200,
                    AvailableQuantity = 120,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new TicketTierResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Name = "Early Bird",
                    Description = "Discounted early access tickets",
                    Price = 50.00m,
                    Quantity = 100,
                    AvailableQuantity = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            return Ok(response);
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