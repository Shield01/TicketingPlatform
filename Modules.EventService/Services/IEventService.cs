using Modules.EventService.DTOs;
using Modules.EventService.Models;

namespace Modules.EventService.Services
{
    /// <summary>
    /// Service interface for Event business logic operations.
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Creates a new event asynchronously.
        /// </summary>
        /// <param name="request">The event creation request.</param>
        /// <param name="organizerId">The ID of the user creating the event.</param>
        /// <returns>The created event response.</returns>
        Task<EventResponse> CreateEventAsync(CreateEventRequest request, Guid organizerId);

        /// <summary>
        /// Gets an event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event response if found, null otherwise.</returns>
        Task<EventResponse?> GetEventByIdAsync(Guid id);

        /// <summary>
        /// Gets a public event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event response if found and published, null otherwise.</returns>
        Task<EventResponse?> GetPublicEventByIdAsync(Guid id);

        /// <summary>
        /// Gets all events for a specific organizer asynchronously.
        /// </summary>
        /// <param name="organizerId">The unique identifier of the organizer.</param>
        /// <returns>A list of event responses created by the organizer.</returns>
        Task<IEnumerable<EventResponse>> GetEventsByOrganizerAsync(Guid organizerId);

        /// <summary>
        /// Gets all public events asynchronously.
        /// </summary>
        /// <returns>A list of all public event responses.</returns>
        Task<IEnumerable<EventResponse>> GetPublicEventsAsync();

        /// <summary>
        /// Gets filtered public events asynchronously with pagination.
        /// </summary>
        /// <param name="filter">The filter criteria for events.</param>
        /// <returns>A paginated response of filtered public events.</returns>
        Task<PaginatedEventViewResponse> GetFilteredPublicEventsAsync(EventFilterRequest filter);

        /// <summary>
        /// Updates an existing event asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event to update.</param>
        /// <param name="request">The event update request.</param>
        /// <param name="userId">The ID of the user updating the event.</param>
        /// <param name="isAdmin">Whether the user has Admin privileges for ownership override.</param>
        /// <returns>The updated event response.</returns>
        Task<EventResponse> UpdateEventAsync(Guid id, UpdateEventRequest request, Guid userId, bool isAdmin = false);

        /// <summary>
        /// Deletes an event asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event to delete.</param>
        /// <param name="userId">The ID of the user deleting the event.</param>
        /// <param name="isAdmin">Whether the user has Admin privileges for ownership override.</param>
        /// <returns>True if the event was deleted, false if not found or unauthorized.</returns>
        Task<bool> DeleteEventAsync(Guid id, Guid userId, bool isAdmin = false);

        /// <summary>
        /// Validates event creation request data.
        /// </summary>
        /// <param name="request">The event creation request to validate.</param>
        /// <returns>A tuple containing validation result and error message if any.</returns>
        (bool IsValid, string? ErrorMessage) ValidateCreateEventRequest(CreateEventRequest request);

        /// <summary>
        /// Gets events created by the user's team, regardless of status.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="filter">Optional filter parameters.</param>
        /// <returns>Paginated response of team events.</returns>
        Task<PaginatedEventsResponse> GetTeamEventsAsync(Guid userId, EventFilterRequest? filter = null);

        /// <summary>
        /// Gets a specific event by ID if the user is a member of the team that created it.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The event response if found and accessible, null otherwise.</returns>
        Task<EventResponse?> GetTeamEventByIdAsync(Guid eventId, Guid userId);

        /// <summary>
        /// Updates an event if the user is a member of the team that created it.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="request">The update request.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The updated event response if successful, null otherwise.</returns>
        Task<EventResponse?> UpdateTeamEventAsync(Guid eventId, UpdateEventRequest request, Guid userId);

        /// <summary>
        /// Deletes an event if the user has appropriate permissions.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the event was deleted, false otherwise.</returns>
        Task<bool> DeleteTeamEventAsync(Guid eventId, Guid userId);

        /// <summary>
        /// Publishes an event, making it visible to the public.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to publish.</param>
        /// <param name="userId">The ID of the user publishing the event.</param>
        /// <returns>The updated event response if successful, null if not found or unauthorized.</returns>
        Task<EventResponse?> PublishEventAsync(Guid eventId, Guid userId);

        /// <summary>
        /// Unpublishes an event, making it a draft and hiding it from public view.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to unpublish.</param>
        /// <param name="userId">The ID of the user unpublishing the event.</param>
        /// <returns>The updated event response if successful, null if not found or unauthorized.</returns>
        Task<EventResponse?> UnpublishEventAsync(Guid eventId, Guid userId);
    }
} 