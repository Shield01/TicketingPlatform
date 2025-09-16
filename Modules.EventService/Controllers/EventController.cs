using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Modules.EventService.DTOs;
using Modules.EventService.Services;
using Shared.Kernel.Extensions;
using Shared.Kernel.Constants;
using System.Security.Claims;

namespace Modules.EventService.Controllers
{
    /// <summary>
    /// Controller for managing event operations including creation, updates, and retrieval.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    // [SwaggerTag("Event management operations including creation, updates, publishing, and retrieval")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;

        public EventController(ILogger<EventController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        /// <summary>
        /// Creates a new event.
        /// </summary>
        /// <param name="request">The event creation request containing all event details.</param>
        /// <returns>Created event with generated ID.</returns>
        /// <response code="201">Event created successfully.</response>
        /// <response code="400">Invalid event data provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to create events.</response>
        [HttpPost]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Create a new event",
            Description = "Creates a new event with the provided information. The event will be created as a draft by default.",
            OperationId = "CreateEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(201, "Event created successfully", typeof(EventResponse))]
        [SwaggerResponse(400, "Invalid event data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to create events")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            try
            {
                _logger.LogInformation("Event creation attempt: {EventTitle}", request.Title);
                
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized("User not authenticated.");
                }

                // Create the event using the service
                var response = await _eventService.CreateEventAsync(request, userId);

                _logger.LogInformation("Event created successfully: {EventId}", response.Id);
                return CreatedAtAction(nameof(GetPublicEvent), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid event creation request: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {EventTitle}", request.Title);
                return StatusCode(500, new { error = "An error occurred while creating the event." });
            }
        }

        /// <summary>
        /// Updates an existing event.
        /// </summary>
        /// <param name="id">The unique identifier of the event to update.</param>
        /// <param name="request">The updated event information.</param>
        /// <returns>Updated event information.</returns>
        /// <response code="200">Event updated successfully.</response>
        /// <response code="400">Invalid event data provided.</response>
        /// <response code="404">Event not found.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to update this event.</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Update an existing event",
            Description = "Updates an existing event with new information. Only the event organizer or Admin can update the event.",
            OperationId = "UpdateEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Event updated successfully", typeof(EventResponse))]
        [SwaggerResponse(400, "Invalid event data")]
        [SwaggerResponse(404, "Event not found")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to update this event")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
        {
            try
            {
                _logger.LogInformation("Event update attempt for event {EventId}: {EventTitle}", id, request.Title);
                
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized("User not authenticated.");
                }

                // Get user role for Admin override check
                var userRole = HttpContext.GetUserRole();
                var isAdmin = userRole == RbacConstants.Roles.Admin;

                // Update the event using the service
                var response = await _eventService.UpdateEventAsync(id, request, userId, isAdmin);

                _logger.LogInformation("Event updated successfully: {EventId}", id);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid event update request: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized event update attempt: {ErrorMessage}", ex.Message);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Event not found: {ErrorMessage}", ex.Message);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event: {EventId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the event." });
            }
        }

        /// <summary>
        /// Retrieves a paginated list of public events with advanced filtering options.
        /// </summary>
        /// <param name="status">Filter by event status (Draft, Published, Cancelled).</param>
        /// <param name="category">Filter by event category.</param>
        /// <param name="eventType">Filter by event type (upcoming, past, all).</param>
        /// <param name="searchKeyword">Search by keyword in title, description, or location.</param>
        /// <param name="location">Filter by location.</param>
        /// <param name="startDateFrom">Filter by start date from.</param>
        /// <param name="startDateTo">Filter by start date to.</param>
        /// <param name="page">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100).</param>
        /// <param name="sortBy">Sort field (Title, StartDate, CreatedAt).</param>
        /// <param name="sortDirection">Sort direction (asc, desc).</param>
        /// <returns>Paginated list of public events.</returns>
        /// <response code="200">Events retrieved successfully.</response>
        /// <response code="400">Invalid filter parameters provided.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get public events with advanced filtering",
            Description = "Retrieves a paginated list of public events with advanced filtering, search, and sorting options.",
            OperationId = "GetPublicEvents",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Events retrieved successfully", typeof(PaginatedEventViewResponse))]
        [SwaggerResponse(400, "Invalid filter parameters")]
        public async Task<IActionResult> GetPublicEvents(
            [FromQuery] string? status = null,
            [FromQuery] string? category = null,
            [FromQuery] string? eventType = null,
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? location = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc")
        {
            _logger.LogInformation("Public events retrieval attempt with filters: Status={Status}, Category={Category}, EventType={EventType}, Page={Page}", 
                status, category, eventType, page);
            
            try
            {
                var filter = new EventFilterRequest
                {
                    Status = status,
                    Category = category,
                    EventType = eventType,
                    SearchKeyword = searchKeyword,
                    Location = location,
                    StartDateFrom = startDateFrom,
                    StartDateTo = startDateTo,
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                };

                var response = await _eventService.GetFilteredPublicEventsAsync(filter);

                _logger.LogInformation("Public events retrieved successfully. Count: {Count}, Total: {TotalCount}", 
                    response.Events.Count, response.TotalCount);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid filter parameters: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public events");
                return StatusCode(500, new { error = "An error occurred while retrieving events." });
            }
        }

        /// <summary>
        /// Retrieves a specific public event by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>Event details.</returns>
        /// <response code="200">Event retrieved successfully.</response>
        /// <response code="404">Event not found or not published.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get public event by ID",
            Description = "Retrieves detailed information about a specific public event by its unique identifier. Only published events are accessible.",
            OperationId = "GetPublicEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Event retrieved successfully", typeof(EventResponse))]
        [SwaggerResponse(404, "Event not found or not published")]
        public async Task<IActionResult> GetPublicEvent(Guid id)
        {
            _logger.LogInformation("Public event retrieval attempt for event {EventId}", id);
            
            try
            {
                var response = await _eventService.GetPublicEventByIdAsync(id);
                
                if (response == null)
                {
                    _logger.LogWarning("Public event not found or not published: {EventId}", id);
                    return NotFound(new { error = "Event not found or not published." });
                }

                _logger.LogInformation("Public event retrieved successfully: {EventId}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public event: {EventId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the event." });
            }
        }

        /// <summary>
        /// Gets all events created by the authenticated user (organizer) with filtering and pagination.
        /// </summary>
        /// <param name="status">Filter by event status (Draft, Published, Cancelled) - can be comma-separated.</param>
        /// <param name="category">Filter by event category.</param>
        /// <param name="eventType">Filter by event type (upcoming, past, all).</param>
        /// <param name="q">Search by keyword in title, description, or location.</param>
        /// <param name="location">Filter by location.</param>
        /// <param name="from">Filter by start date from (ISO date).</param>
        /// <param name="to">Filter by start date to (ISO date).</param>
        /// <param name="page">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 20, max: 100).</param>
        /// <param name="sortBy">Sort field (Title, StartDate, CreatedAt, Name).</param>
        /// <param name="sortDir">Sort direction (asc, desc).</param>
        /// <returns>Paginated list of events created by the current user.</returns>
        /// <response code="200">Events retrieved successfully.</response>
        /// <response code="400">Invalid filter parameters provided.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized (should not occur for owner).</response>
        [HttpGet("mine")]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Get my events (all statuses)",
            Description = "Retrieves all events created by the authenticated user (organizer), including Draft, Published, Cancelled, and other statuses. Supports advanced filtering, search, and sorting options. Only the event owner or Admin can access this endpoint.",
            OperationId = "GetMyEvents",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "My events retrieved successfully", typeof(PaginatedEventsResponse))]
        [SwaggerResponse(400, "Invalid filter parameters")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        public async Task<IActionResult> GetMyEvents(
            [FromQuery] string? status = null,
            [FromQuery] string? category = null,
            [FromQuery] string? eventType = null,
            [FromQuery] string? q = null,
            [FromQuery] string? location = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = "desc")
        {
            try
            {
                // Get the current user ID from JWT claims
                var userId = HttpContext.GetUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("User ID not found in claims for my events retrieval");
                    return Unauthorized("User not authenticated.");
                }

                _logger.LogInformation("My events retrieval attempt for user {UserId} with filters: Status={Status}, Category={Category}, EventType={EventType}, Page={Page}", 
                    userId.Value, status, category, eventType, page);

                // Build the filter request
                var filter = new EventFilterRequest
                {
                    Status = status,
                    Category = category,
                    EventType = eventType,
                    SearchKeyword = q, // Map 'q' parameter to SearchKeyword
                    Location = location,
                    StartDateFrom = from, // Map 'from' parameter to StartDateFrom
                    StartDateTo = to, // Map 'to' parameter to StartDateTo
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDirection = sortDir // Map 'sortDir' parameter to SortDirection
                };

                // Get the user's events
                var response = await _eventService.GetMyEventsAsync(userId.Value, filter);

                _logger.LogInformation("My events retrieved successfully for user {UserId}. Count: {Count}, Total: {TotalCount}, Page: {Page}/{TotalPages}", 
                    userId.Value, response.Events.Count, response.TotalCount, response.Page, response.TotalPages);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid filter parameters for my events: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving my events");
                return StatusCode(500, new { error = "An error occurred while retrieving your events." });
            }
        }

        /// <summary>
        /// Deletes an event.
        /// </summary>
        /// <param name="id">The unique identifier of the event to delete.</param>
        /// <returns>Deletion confirmation.</returns>
        /// <response code="204">Event deleted successfully.</response>
        /// <response code="404">Event not found.</response>
        /// <response code="409">Cannot delete event - tickets have been issued.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to delete this event.</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Delete an event",
            Description = "Deletes an event. Only the event organizer or Admin can delete the event. Cannot delete events with issued tickets.",
            OperationId = "DeleteEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(204, "Event deleted successfully")]
        [SwaggerResponse(404, "Event not found")]
        [SwaggerResponse(409, "Cannot delete event - tickets have been issued")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to delete this event")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                _logger.LogInformation("Event deletion attempt for event {EventId}", id);
                
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized("User not authenticated.");
                }

                // Get user role for Admin override check
                var userRole = HttpContext.GetUserRole();
                var isAdmin = userRole == RbacConstants.Roles.Admin;

                // Delete the event using the service
                var result = await _eventService.DeleteEventAsync(id, userId, isAdmin);

                if (result)
                {
                    _logger.LogInformation("Event deleted successfully: {EventId}", id);
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning("Event deletion failed: {EventId}", id);
                    return NotFound(new { error = "Event not found or you don't have permission to delete this event." });
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("tickets"))
            {
                _logger.LogWarning("Cannot delete event with tickets: {EventId} - {ErrorMessage}", id, ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized event deletion attempt: {ErrorMessage}", ex.Message);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Event deletion failed: {ErrorMessage}", ex.Message);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event: {EventId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the event." });
            }
        }

        /// <summary>
        /// Gets events created by the user's team, regardless of status.
        /// </summary>
        /// <param name="filter">Optional filter parameters.</param>
        /// <returns>Paginated list of team events.</returns>
        /// <response code="200">Team events retrieved successfully.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpGet("team")]
        [Authorize(Policy = "StaffOrHigher")]
        [SwaggerOperation(
            Summary = "Get team events",
            Description = "Gets events created by the user's team, regardless of status. Available to Staff, Organiser, and Admin roles.",
            OperationId = "GetTeamEvents",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Team events retrieved successfully", typeof(PaginatedEventsResponse))]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> GetTeamEvents([FromQuery] EventFilterRequest? filter = null)
        {
            _logger.LogInformation("Get team events request received");

            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.GetTeamEventsAsync(userId, filter);

                _logger.LogInformation("Team events retrieved successfully. Count: {Count}", result.Events.Count());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team events");
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Gets a specific event by ID if the user is a member of the team that created it.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>The event details if accessible.</returns>
        /// <response code="200">Event details retrieved successfully.</response>
        /// <response code="404">Event not found or not accessible.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpGet("team/{id}")]
        [Authorize(Policy = "StaffOrHigher")]
        [SwaggerOperation(
            Summary = "Get team event by ID",
            Description = "Gets a specific event by ID if the user is a member of the team that created it. Available to Staff, Organiser, and Admin roles.",
            OperationId = "GetTeamEventById",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Event details retrieved successfully", typeof(EventResponse))]
        [SwaggerResponse(404, "Event not found or not accessible")]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> GetTeamEventById(Guid id)
        {
            _logger.LogInformation("Get team event by ID request received for event ID: {EventId}", id);

            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.GetTeamEventByIdAsync(id, userId);

                if (result == null)
                {
                    _logger.LogWarning("Team event not found or not accessible: {EventId}", id);
                    return NotFound(new { Message = "Event not found or you don't have access to this event." });
                }

                _logger.LogInformation("Team event retrieved successfully: {EventId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team event by ID: {EventId}", id);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Updates an event if the user is a member of the team that created it.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <param name="request">The update request.</param>
        /// <returns>The updated event details.</returns>
        /// <response code="200">Event updated successfully.</response>
        /// <response code="404">Event not found or not accessible.</response>
        /// <response code="400">Invalid update request.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpPut("team/{id}")]
        [Authorize(Policy = "StaffOrHigher")]
        [SwaggerOperation(
            Summary = "Update team event",
            Description = "Updates an event if the user is a member of the team that created it. Available to Staff, Organiser, and Admin roles.",
            OperationId = "UpdateTeamEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Event updated successfully", typeof(EventResponse))]
        [SwaggerResponse(404, "Event not found or not accessible")]
        [SwaggerResponse(400, "Invalid update request")]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> UpdateTeamEvent(Guid id, [FromBody] UpdateEventRequest request)
        {
            _logger.LogInformation("Update team event request received for event ID: {EventId}", id);

            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.UpdateTeamEventAsync(id, request, userId);

                if (result == null)
                {
                    _logger.LogWarning("Team event update failed for event ID: {EventId}", id);
                    return NotFound(new { Message = "Event not found or you don't have access to update this event." });
                }

                _logger.LogInformation("Team event updated successfully: {EventId}", id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid team event update request: {ErrorMessage}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team event: {EventId}", id);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Deletes an event if the user has appropriate permissions.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">Event deleted successfully.</response>
        /// <response code="404">Event not found or not accessible.</response>
        /// <response code="403">User not authorized to delete this event.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpDelete("team/{id}")]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Delete team event",
            Description = "Deletes an event if the user has appropriate permissions. Only Organiser and Admin can delete events.",
            OperationId = "DeleteTeamEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(204, "Event deleted successfully")]
        [SwaggerResponse(404, "Event not found or not accessible")]
        [SwaggerResponse(403, "User not authorized to delete this event")]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> DeleteTeamEvent(Guid id)
        {
            _logger.LogInformation("Delete team event request received for event ID: {EventId}", id);

            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.DeleteTeamEventAsync(id, userId);

                if (!result)
                {
                    _logger.LogWarning("Team event deletion failed for event ID: {EventId}", id);
                    return NotFound(new { Message = "Event not found, not accessible, or you don't have permission to delete this event." });
                }

                _logger.LogInformation("Team event deleted successfully: {EventId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team event: {EventId}", id);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Publishes an event, making it visible to the public.
        /// </summary>
        /// <param name="id">The unique identifier of the event to publish.</param>
        /// <returns>The published event details.</returns>
        /// <response code="200">Event published successfully.</response>
        /// <response code="400">Event cannot be published due to validation errors.</response>
        /// <response code="404">Event not found or not accessible.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to publish this event.</response>
        [HttpPost("{id}/publish")]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Publish an event",
            Description = "Publishes an event, making it visible in public event listings. Only event organizers or team members can publish events.",
            OperationId = "PublishEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Event published successfully", typeof(EventResponse))]
        [SwaggerResponse(400, "Event cannot be published due to validation errors")]
        [SwaggerResponse(404, "Event not found or not accessible")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to publish this event")]
        public async Task<IActionResult> PublishEvent(Guid id)
        {
            _logger.LogInformation("Publish event request received for event ID: {EventId}", id);

            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.PublishEventAsync(id, userId);

                if (result == null)
                {
                    _logger.LogWarning("Event publish failed for event ID: {EventId}", id);
                    return NotFound(new { Message = "Event not found or you don't have permission to publish this event." });
                }

                _logger.LogInformation("Event published successfully: {EventId}", id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid event publish request: {ErrorMessage}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event: {EventId}", id);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Unpublishes an event, making it a draft and hiding it from public view.
        /// </summary>
        /// <param name="id">The unique identifier of the event to unpublish.</param>
        /// <returns>The unpublished event details.</returns>
        /// <response code="200">Event unpublished successfully.</response>
        /// <response code="404">Event not found or not accessible.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="403">User not authorized to unpublish this event.</response>
        [HttpPost("{id}/unpublish")]
        [Authorize(Policy = "OrganiserOrAdmin")]
        [SwaggerOperation(
            Summary = "Unpublish an event",
            Description = "Unpublishes an event, making it a draft and hiding it from public view. Only event organizers or team members can unpublish events.",
            OperationId = "UnpublishEvent",
            Tags = new[] { "Events" }
        )]
        [SwaggerResponse(200, "Event unpublished successfully", typeof(EventResponse))]
        [SwaggerResponse(404, "Event not found or not accessible")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized to unpublish this event")]
        public async Task<IActionResult> UnpublishEvent(Guid id)
        {
            _logger.LogInformation("Unpublish event request received for event ID: {EventId}", id);

            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.UnpublishEventAsync(id, userId);

                if (result == null)
                {
                    _logger.LogWarning("Event unpublish failed for event ID: {EventId}", id);
                    return NotFound(new { Message = "Event not found or you don't have permission to unpublish this event." });
                }

                _logger.LogInformation("Event unpublished successfully: {EventId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing event: {EventId}", id);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Gets the current user ID from the JWT claims.
        /// </summary>
        /// <returns>The current user ID.</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }
            return userId;
        }
    }
} 