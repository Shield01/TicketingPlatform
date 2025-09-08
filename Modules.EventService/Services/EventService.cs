using Microsoft.Extensions.Logging;
using Modules.EventService.DTOs;
using Modules.EventService.Models;
using Modules.EventService.Repositories;
using Modules.TeamService.Services;
using Modules.TicketService.Services;

namespace Modules.EventService.Services
{
    /// <summary>
    /// Service implementation for Event business logic operations.
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITeamService _teamService;
        private readonly ITicketTierService _ticketTierService;
        private readonly ILogger<EventService> _logger;

        /// <summary>
        /// Initializes a new instance of the EventService.
        /// </summary>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="teamService">The team service.</param>
        /// <param name="ticketTierService">The ticket tier service.</param>
        /// <param name="logger">The logger instance.</param>
        public EventService(IEventRepository eventRepository, ITeamService teamService, ITicketTierService ticketTierService, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
            _ticketTierService = ticketTierService ?? throw new ArgumentNullException(nameof(ticketTierService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new event asynchronously.
        /// </summary>
        /// <param name="request">The event creation request.</param>
        /// <param name="organizerId">The ID of the user creating the event.</param>
        /// <returns>The created event response.</returns>
        public async Task<EventResponse> CreateEventAsync(CreateEventRequest request, Guid organizerId)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException("Request cannot be null.");
                }

                _logger.LogInformation("Creating event: {EventTitle} for organizer: {OrganizerId}", request.Title, organizerId);

                // Validate the request
                var (isValid, errorMessage) = ValidateCreateEventRequest(request);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid event creation request: {ErrorMessage}", errorMessage);
                    throw new ArgumentException(errorMessage);
                }

                // Determine initial publication status
                var isPublished = request.IsPublished;
                var status = !string.IsNullOrWhiteSpace(request.Status) ? request.Status : (isPublished ? "Published" : "Draft");
                
                // Validate status if provided
                if (!string.IsNullOrWhiteSpace(request.Status) && 
                    !new[] { "Draft", "Published" }.Contains(request.Status))
                {
                    throw new ArgumentException("Invalid status. Valid values are: Draft, Published");
                }

                // Create the event entity
                var @event = new Event
                {
                    Title = request.Title,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Location = request.Location,
                    Category = request.Category,
                    IsPublic = request.IsPublic,
                    IsPublished = isPublished,
                    Status = status,
                    OrganizerId = organizerId
                };

                // Validate business rules
                if (!@event.ValidateDates())
                {
                    throw new ArgumentException("End date must be after start date.");
                }

                if (!@event.ValidateEventNotInPast())
                {
                    throw new ArgumentException("Event cannot be created in the past.");
                }

                // Save to database
                var createdEvent = await _eventRepository.CreateEventAsync(@event);

                // Convert to response DTO
                var response = await MapEventToResponseAsync(createdEvent);

                _logger.LogInformation("Event created successfully: {EventId}", createdEvent.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {EventTitle}", request?.Title ?? "Unknown");
                throw;
            }
        }

        /// <summary>
        /// Gets an event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event response if found, null otherwise.</returns>
        public async Task<EventResponse?> GetEventByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Getting event by ID: {EventId}", id);

                var @event = await _eventRepository.GetEventByIdAsync(id);
                if (@event == null)
                {
                    _logger.LogDebug("Event not found: {EventId}", id);
                    return null;
                }

                return await MapEventToResponseAsync(@event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event by ID: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets a public event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event response if found and published, null otherwise.</returns>
        public async Task<EventResponse?> GetPublicEventByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Getting public event by ID: {EventId}", id);

                var @event = await _eventRepository.GetPublicEventByIdAsync(id);
                if (@event == null)
                {
                    _logger.LogDebug("Public event not found or not published: {EventId}", id);
                    return null;
                }

                return await MapEventToResponseAsync(@event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public event by ID: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all events for a specific organizer asynchronously.
        /// </summary>
        /// <param name="organizerId">The unique identifier of the organizer.</param>
        /// <returns>A list of event responses created by the organizer.</returns>
        public async Task<IEnumerable<EventResponse>> GetEventsByOrganizerAsync(Guid organizerId)
        {
            try
            {
                _logger.LogDebug("Getting events for organizer: {OrganizerId}", organizerId);

                var events = await _eventRepository.GetEventsByOrganizerAsync(organizerId);
                var tasks = events.Select(async e => await MapEventToResponseAsync(e));
                return await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for organizer: {OrganizerId}", organizerId);
                throw;
            }
        }

        /// <summary>
        /// Gets all public events asynchronously.
        /// </summary>
        /// <returns>A list of all public event responses.</returns>
        public async Task<IEnumerable<EventResponse>> GetPublicEventsAsync()
        {
            try
            {
                _logger.LogDebug("Getting public events");

                var events = await _eventRepository.GetPublicEventsAsync();
                var tasks = events.Select(async e => await MapEventToResponseAsync(e));
                return await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public events");
                throw;
            }
        }

        /// <summary>
        /// Gets filtered public events asynchronously with pagination.
        /// </summary>
        /// <param name="filter">The filter criteria for events.</param>
        /// <returns>A paginated response of filtered public events.</returns>
        public async Task<PaginatedEventViewResponse> GetFilteredPublicEventsAsync(EventFilterRequest filter)
        {
            try
            {
                _logger.LogDebug("Getting filtered public events with filter: {@Filter}", filter);

                // Validate and normalize filter parameters
                NormalizeFilterParameters(filter);

                var (events, totalCount) = await _eventRepository.GetFilteredPublicEventsAsync(filter);
                var eventViews = events.Select(MapEventToViewDTO);

                var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);
                var hasNextPage = filter.Page < totalPages;
                var hasPreviousPage = filter.Page > 1;

                var response = new PaginatedEventViewResponse
                {
                    Events = eventViews.ToList(),
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage
                };

                _logger.LogDebug("Filtered public events retrieved. Count: {Count}, Total: {TotalCount}", eventViews.Count(), totalCount);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered public events");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing event asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event to update.</param>
        /// <param name="request">The event update request.</param>
        /// <param name="userId">The ID of the user updating the event.</param>
        /// <param name="isAdmin">Whether the user has Admin privileges for ownership override.</param>
        /// <returns>The updated event response.</returns>
        public async Task<EventResponse> UpdateEventAsync(Guid id, UpdateEventRequest request, Guid userId, bool isAdmin = false)
        {
            try
            {
                _logger.LogInformation("Updating event: {EventId} by user: {UserId} (Admin: {IsAdmin})", id, userId, isAdmin);

                // Get the existing event first
                var existingEvent = await _eventRepository.GetEventByIdAsync(id);
                if (existingEvent == null)
                {
                    throw new InvalidOperationException($"Event with ID {id} not found.");
                }

                // Check if user is authorized to update this event (organizer or admin override)
                var isOrganizer = await _eventRepository.IsUserOrganizerAsync(id, userId);
                if (!isOrganizer && !isAdmin)
                {
                    _logger.LogWarning("User {UserId} is not authorized to update event {EventId} (Not organizer, not admin)", userId, id);
                    throw new UnauthorizedAccessException("You are not authorized to update this event.");
                }

                // Update the event properties
                existingEvent.Title = request.Title;
                existingEvent.Description = request.Description;
                existingEvent.StartDate = request.StartDate;
                existingEvent.EndDate = request.EndDate;
                existingEvent.Location = request.Location;
                existingEvent.Category = request.Category;
                existingEvent.IsPublic = request.IsPublic;
                existingEvent.Status = request.Status;
                existingEvent.UpdatedAt = DateTime.UtcNow;

                // Validate business rules
                if (!existingEvent.ValidateDates())
                {
                    throw new ArgumentException("End date must be after start date.");
                }

                if (!existingEvent.ValidateEventNotInPast())
                {
                    throw new ArgumentException("Event start date cannot be in the past.");
                }

                // Save changes
                var updatedEvent = await _eventRepository.UpdateEventAsync(existingEvent);

                _logger.LogInformation("Event updated successfully: {EventId}", id);
                return await MapEventToResponseAsync(updatedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Deletes an event asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event to delete.</param>
        /// <param name="userId">The ID of the user deleting the event.</param>
        /// <param name="isAdmin">Whether the user has Admin privileges for ownership override.</param>
        /// <returns>True if the event was deleted, false if not found or unauthorized.</returns>
        public async Task<bool> DeleteEventAsync(Guid id, Guid userId, bool isAdmin = false)
        {
            try
            {
                _logger.LogInformation("Deleting event: {EventId} by user: {UserId} (Admin: {IsAdmin})", id, userId, isAdmin);

                // Check if event exists first
                var existingEvent = await _eventRepository.GetEventByIdAsync(id);
                if (existingEvent == null)
                {
                    _logger.LogWarning("Event not found for deletion: {EventId}", id);
                    return false;
                }

                // Check if user is authorized to delete this event (organizer or admin override)
                var isOrganizer = await _eventRepository.IsUserOrganizerAsync(id, userId);
                if (!isOrganizer && !isAdmin)
                {
                    _logger.LogWarning("User {UserId} is not authorized to delete event {EventId} (Not organizer, not admin)", userId, id);
                    return false;
                }

                // Check if any tickets exist for this event
                // Note: Since TicketService is currently stub implementation, this is a placeholder
                // TODO: Replace with actual ticket service integration once TicketService is implemented
                var hasTickets = await CheckIfEventHasTicketsAsync(id);
                if (hasTickets)
                {
                    _logger.LogWarning("Cannot delete event {EventId} - tickets have been issued", id);
                    throw new InvalidOperationException("Cannot delete event - tickets have been issued for this event.");
                }

                var deleted = await _eventRepository.DeleteEventAsync(id);
                if (deleted)
                {
                    _logger.LogInformation("Event deleted successfully: {EventId}", id);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Validates event creation request data.
        /// </summary>
        /// <param name="request">The event creation request to validate.</param>
        /// <returns>A tuple containing validation result and error message if any.</returns>
        public (bool IsValid, string? ErrorMessage) ValidateCreateEventRequest(CreateEventRequest request)
        {
            if (request == null)
            {
                return (false, "Request cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return (false, "Title is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return (false, "Description is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Location))
            {
                return (false, "Location is required.");
            }

            if (request.StartDate >= request.EndDate)
            {
                return (false, "End date must be after start date.");
            }

            if (request.StartDate <= DateTime.UtcNow)
            {
                return (false, "Event cannot be created in the past.");
            }

            return (true, null);
        }

        /// <summary>
        /// Maps an Event entity to EventResponse DTO.
        /// </summary>
        /// <param name="event">The event entity to map.</param>
        /// <returns>The mapped EventResponse.</returns>
        private async Task<EventResponse> MapEventToResponseAsync(Event @event)
        {
            // Get ticket tiers for this event
            var ticketTiers = new List<EventTicketTierResponse>();
            try
            {
                var tiers = await _ticketTierService.GetEventTicketTiersAsync(@event.Id);
                ticketTiers = tiers.Select(t => new EventTicketTierResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Price = t.Price,
                    Currency = t.Currency,
                    MaxQuantity = t.MaxQuantity,
                    SoldQuantity = t.SoldQuantity,
                    IsAvailable = t.IsAvailable,
                    CreatedAt = t.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load ticket tiers for event {EventId}", @event.Id);
                // Continue without ticket tiers if there's an error
            }

            return new EventResponse
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                Location = @event.Location,
                Category = @event.Category,
                IsPublic = @event.IsPublic,
                IsPublished = @event.IsPublished,
                Status = @event.Status,
                OrganizerId = @event.OrganizerId,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt,
                OrganizerName = @event.Organizer != null 
                    ? $"{@event.Organizer.FirstName} {@event.Organizer.LastName}".Trim()
                    : string.Empty,
                TicketTiers = ticketTiers
            };
        }

        /// <summary>
        /// Maps an Event entity to EventViewDTO for public viewing.
        /// </summary>
        /// <param name="event">The event entity to map.</param>
        /// <returns>The mapped EventViewDTO.</returns>
        private static EventViewDTO MapEventToViewDTO(Event @event)
        {
            var now = DateTime.UtcNow;
            var isUpcoming = @event.StartDate > now;
            var daysUntilEvent = isUpcoming ? (int)(@event.StartDate - now).TotalDays : 0;

            return new EventViewDTO
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                Location = @event.Location,
                Category = @event.Category,
                OrganizerName = @event.Organizer != null 
                    ? $"{@event.Organizer.FirstName} {@event.Organizer.LastName}".Trim()
                    : string.Empty,
                CreatedAt = @event.CreatedAt,
                IsUpcoming = isUpcoming,
                DaysUntilEvent = daysUntilEvent
            };
        }

        /// <summary>
        /// Normalizes and validates filter parameters.
        /// </summary>
        /// <param name="filter">The filter to normalize.</param>
        private static void NormalizeFilterParameters(EventFilterRequest filter)
        {
            // Ensure page and pageSize are within valid ranges
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.PageSize > 100) filter.PageSize = 100;

            // Normalize sort direction
            if (!string.IsNullOrWhiteSpace(filter.SortDirection))
            {
                filter.SortDirection = filter.SortDirection.ToLower();
                if (filter.SortDirection != "asc" && filter.SortDirection != "desc")
                {
                    filter.SortDirection = "asc";
                }
            }

            // Normalize sort field
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                filter.SortBy = filter.SortBy.ToLower();
                if (filter.SortBy != "title" && filter.SortBy != "startdate" && filter.SortBy != "createdat")
                {
                    filter.SortBy = "startdate";
                }
            }
        }

        /// <summary>
        /// Gets events created by the user's team, regardless of status.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="filter">Optional filter parameters.</param>
        /// <returns>Paginated response of team events.</returns>
        public async Task<PaginatedEventsResponse> GetTeamEventsAsync(Guid userId, EventFilterRequest? filter = null)
        {
            _logger.LogInformation("Getting team events for user: {UserId}", userId);

            // Get user's team IDs
            var userTeamIds = await _teamService.GetUserTeamIdsAsync(userId);
            if (!userTeamIds.Any())
            {
                _logger.LogWarning("User {UserId} is not a member of any teams", userId);
                return new PaginatedEventsResponse
                {
                    Events = new List<EventResponse>(),
                    TotalCount = 0,
                    Page = filter?.Page ?? 1,
                    PageSize = filter?.PageSize ?? 10,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };
            }

            // Normalize filter parameters
            var normalizedFilter = filter ?? new EventFilterRequest();
            NormalizeFilterParameters(normalizedFilter);

            // Get team events from repository
            var (events, totalCount) = await _eventRepository.GetTeamEventsAsync(userTeamIds, normalizedFilter);

            // Map to response DTOs
            var tasks = events.Select(async e => await MapEventToResponseAsync(e));
            var eventResponses = (await Task.WhenAll(tasks)).ToList();

            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling((double)totalCount / normalizedFilter.PageSize);
            var hasNextPage = normalizedFilter.Page < totalPages;
            var hasPreviousPage = normalizedFilter.Page > 1;

            return new PaginatedEventsResponse
            {
                Events = eventResponses,
                TotalCount = totalCount,
                Page = normalizedFilter.Page,
                PageSize = normalizedFilter.PageSize,
                TotalPages = totalPages,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };
        }

        /// <summary>
        /// Gets a specific event by ID if the user is a member of the team that created it.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The event response if found and accessible, null otherwise.</returns>
        public async Task<EventResponse?> GetTeamEventByIdAsync(Guid eventId, Guid userId)
        {
            _logger.LogInformation("Getting team event by ID: {EventId} for user: {UserId}", eventId, userId);

            // Get user's team IDs
            var userTeamIds = await _teamService.GetUserTeamIdsAsync(userId);
            if (!userTeamIds.Any())
            {
                _logger.LogWarning("User {UserId} is not a member of any teams", userId);
                return null;
            }

            // Get team event from repository
            var @event = await _eventRepository.GetTeamEventByIdAsync(eventId, userTeamIds);
            if (@event == null)
            {
                _logger.LogWarning("Team event not found or not accessible: {EventId} for user: {UserId}", eventId, userId);
                return null;
            }

            return await MapEventToResponseAsync(@event);
        }

        /// <summary>
        /// Updates an event if the user is a member of the team that created it.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="request">The update request.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The updated event response if successful, null otherwise.</returns>
        public async Task<EventResponse?> UpdateTeamEventAsync(Guid eventId, UpdateEventRequest request, Guid userId)
        {
            _logger.LogInformation("Updating team event: {EventId} for user: {UserId}", eventId, userId);

            // Get user's team IDs
            var userTeamIds = await _teamService.GetUserTeamIdsAsync(userId);
            if (!userTeamIds.Any())
            {
                _logger.LogWarning("User {UserId} is not a member of any teams", userId);
                return null;
            }

            // Check if user is a member of the team that created the event
            var isTeamMember = await _eventRepository.IsUserTeamMemberAsync(eventId, userTeamIds);
            if (!isTeamMember)
            {
                _logger.LogWarning("User {UserId} is not a member of the team that created event {EventId}", userId, eventId);
                return null;
            }

            // Get the event
            var @event = await _eventRepository.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                _logger.LogWarning("Event not found: {EventId}", eventId);
                return null;
            }

            // Update event properties
            @event.Title = request.Title;
            @event.Description = request.Description;
            @event.StartDate = request.StartDate;
            @event.EndDate = request.EndDate;
            @event.Location = request.Location;
            @event.Category = request.Category;
            @event.IsPublic = request.IsPublic;
            @event.Status = request.Status;
            @event.UpdatedAt = DateTime.UtcNow;

            // Validate the updated event
            var (isValid, errorMessage) = ValidateCreateEventRequest(new CreateEventRequest
            {
                Title = @event.Title,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                Location = @event.Location,
                Category = @event.Category,
                IsPublic = @event.IsPublic,
                IsPublished = @event.IsPublished,
                Status = @event.Status
            });

            if (!isValid)
            {
                _logger.LogWarning("Invalid event update request: {ErrorMessage}", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Update the event
            var updatedEvent = await _eventRepository.UpdateEventAsync(@event);
            return await MapEventToResponseAsync(updatedEvent);
        }

        /// <summary>
        /// Deletes an event if the user has appropriate permissions.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the event was deleted, false otherwise.</returns>
        public async Task<bool> DeleteTeamEventAsync(Guid eventId, Guid userId)
        {
            _logger.LogInformation("Deleting team event: {EventId} for user: {UserId}", eventId, userId);

            // Get user's team IDs
            var userTeamIds = await _teamService.GetUserTeamIdsAsync(userId);
            if (!userTeamIds.Any())
            {
                _logger.LogWarning("User {UserId} is not a member of any teams", userId);
                return false;
            }

            // Check if user is a member of the team that created the event
            var isTeamMember = await _eventRepository.IsUserTeamMemberAsync(eventId, userTeamIds);
            if (!isTeamMember)
            {
                _logger.LogWarning("User {UserId} is not a member of the team that created event {EventId}", userId, eventId);
                return false;
            }

            // Get the event to check user's role
            var @event = await _eventRepository.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                _logger.LogWarning("Event not found: {EventId}", eventId);
                return false;
            }

            // Get user's role in the team
            var teamId = @event.TeamId;
            if (teamId.HasValue)
            {
                var userTeamRole = await _teamService.GetUserTeamRoleAsync(teamId.Value, userId);
                
                // Only Organiser and Admin can delete events
                if (userTeamRole != "TeamLeader" && userTeamRole != "Organiser" && userTeamRole != "Admin")
                {
                    _logger.LogWarning("User {UserId} with role {Role} cannot delete event {EventId}", userId, userTeamRole, eventId);
                    return false;
                }
            }

            // Delete the event
            return await _eventRepository.DeleteEventAsync(eventId);
        }

        /// <summary>
        /// Publishes an event, making it visible to the public.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to publish.</param>
        /// <param name="userId">The ID of the user publishing the event.</param>
        /// <returns>The updated event response if successful, null if not found or unauthorized.</returns>
        public async Task<EventResponse?> PublishEventAsync(Guid eventId, Guid userId)
        {
            _logger.LogInformation("Publishing event: {EventId} for user: {UserId}", eventId, userId);

            try
            {
                // Check if user is authorized to publish this event (must be organizer or team member)
                var isOrganizer = await _eventRepository.IsUserOrganizerAsync(eventId, userId);
                if (!isOrganizer)
                {
                    // Check if user is a team member
                    var userTeamIds = await _teamService.GetUserTeamIdsAsync(userId);
                    var isTeamMember = await _eventRepository.IsUserTeamMemberAsync(eventId, userTeamIds);
                    
                    if (!isTeamMember)
                    {
                        _logger.LogWarning("User {UserId} is not authorized to publish event {EventId}", userId, eventId);
                        return null;
                    }
                }

                // Get the event
                var @event = await _eventRepository.GetEventByIdAsync(eventId);
                if (@event == null)
                {
                    _logger.LogWarning("Event not found for publishing: {EventId}", eventId);
                    return null;
                }

                // Validate the event before publishing
                var (isValid, errorMessage) = ValidateCreateEventRequest(new CreateEventRequest
                {
                    Title = @event.Title,
                    Description = @event.Description,
                    StartDate = @event.StartDate,
                    EndDate = @event.EndDate,
                    Location = @event.Location,
                    Category = @event.Category,
                    IsPublic = @event.IsPublic,
                    IsPublished = true, // For validation purposes
                    Status = "Published"
                });

                if (!isValid)
                {
                    _logger.LogWarning("Cannot publish invalid event {EventId}: {ErrorMessage}", eventId, errorMessage);
                    throw new InvalidOperationException($"Cannot publish event: {errorMessage}");
                }

                // Update event to published status
                @event.IsPublished = true;
                @event.Status = "Published";
                @event.UpdatedAt = DateTime.UtcNow;

                // Save changes
                var updatedEvent = await _eventRepository.UpdateEventAsync(@event);

                _logger.LogInformation("Event published successfully: {EventId}", eventId);
                return await MapEventToResponseAsync(updatedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event: {EventId}", eventId);
                throw;
            }
        }

        /// <summary>
        /// Unpublishes an event, making it a draft and hiding it from public view.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to unpublish.</param>
        /// <param name="userId">The ID of the user unpublishing the event.</param>
        /// <returns>The updated event response if successful, null if not found or unauthorized.</returns>
        public async Task<EventResponse?> UnpublishEventAsync(Guid eventId, Guid userId)
        {
            _logger.LogInformation("Unpublishing event: {EventId} for user: {UserId}", eventId, userId);

            try
            {
                // Check if user is authorized to unpublish this event (must be organizer or team member)
                var isOrganizer = await _eventRepository.IsUserOrganizerAsync(eventId, userId);
                if (!isOrganizer)
                {
                    // Check if user is a team member
                    var userTeamIds = await _teamService.GetUserTeamIdsAsync(userId);
                    var isTeamMember = await _eventRepository.IsUserTeamMemberAsync(eventId, userTeamIds);
                    
                    if (!isTeamMember)
                    {
                        _logger.LogWarning("User {UserId} is not authorized to unpublish event {EventId}", userId, eventId);
                        return null;
                    }
                }

                // Get the event
                var @event = await _eventRepository.GetEventByIdAsync(eventId);
                if (@event == null)
                {
                    _logger.LogWarning("Event not found for unpublishing: {EventId}", eventId);
                    return null;
                }

                // Update event to draft status
                @event.IsPublished = false;
                @event.Status = "Draft";
                @event.UpdatedAt = DateTime.UtcNow;

                // Save changes
                var updatedEvent = await _eventRepository.UpdateEventAsync(@event);

                _logger.LogInformation("Event unpublished successfully: {EventId}", eventId);
                return await MapEventToResponseAsync(updatedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing event: {EventId}", eventId);
                throw;
            }
        }

        /// <summary>
        /// Checks if an event has any issued tickets.
        /// This is a placeholder implementation until TicketService is fully implemented.
        /// </summary>
        /// <param name="eventId">The event ID to check.</param>
        /// <returns>True if tickets exist, false otherwise.</returns>
        private async Task<bool> CheckIfEventHasTicketsAsync(Guid eventId)
        {
            // TODO: Replace this with actual TicketService integration once implemented
            // For now, we'll simulate checking based on event age (events older than 1 day might have tickets)
            try
            {
                var @event = await _eventRepository.GetEventByIdAsync(eventId);
                if (@event == null) return false;

                // Simple placeholder logic: assume events that are published and created more than 1 day ago might have tickets
                var hasTickets = @event.IsPublished && 
                                @event.CreatedAt < DateTime.UtcNow.AddDays(-1) && 
                                @event.StartDate > DateTime.UtcNow.AddDays(-1);

                _logger.LogDebug("Ticket check for event {EventId}: HasTickets={HasTickets} (placeholder logic)", eventId, hasTickets);
                return hasTickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tickets for event: {EventId}", eventId);
                // In case of error, be conservative and assume tickets might exist
                return true;
            }
        }
    }
} 