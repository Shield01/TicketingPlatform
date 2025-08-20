using Modules.EventService.Models;
using Modules.EventService.DTOs;

namespace Modules.EventService.Repositories
{
    /// <summary>
    /// Repository interface for Event entity operations.
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Creates a new event asynchronously.
        /// </summary>
        /// <param name="event">The event to create.</param>
        /// <returns>The created event with generated ID.</returns>
        Task<Event> CreateEventAsync(Event @event);

        /// <summary>
        /// Gets an event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event if found, null otherwise.</returns>
        Task<Event?> GetEventByIdAsync(Guid id);

        /// <summary>
        /// Gets a public event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event if found and published, null otherwise.</returns>
        Task<Event?> GetPublicEventByIdAsync(Guid id);

        /// <summary>
        /// Gets all events for a specific organizer asynchronously.
        /// </summary>
        /// <param name="organizerId">The unique identifier of the organizer.</param>
        /// <returns>A list of events created by the organizer.</returns>
        Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId);

        /// <summary>
        /// Gets all public events asynchronously.
        /// </summary>
        /// <returns>A list of all public events.</returns>
        Task<IEnumerable<Event>> GetPublicEventsAsync();

        /// <summary>
        /// Gets filtered public events asynchronously with pagination.
        /// </summary>
        /// <param name="filter">The filter criteria for events.</param>
        /// <returns>A tuple containing the filtered events and total count.</returns>
        Task<(IEnumerable<Event> Events, int TotalCount)> GetFilteredPublicEventsAsync(EventFilterRequest filter);

        /// <summary>
        /// Updates an existing event asynchronously.
        /// </summary>
        /// <param name="event">The event to update.</param>
        /// <returns>The updated event.</returns>
        Task<Event> UpdateEventAsync(Event @event);

        /// <summary>
        /// Deletes an event asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event to delete.</param>
        /// <returns>True if the event was deleted, false if not found.</returns>
        Task<bool> DeleteEventAsync(Guid id);

        /// <summary>
        /// Checks if an event exists by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>True if the event exists, false otherwise.</returns>
        Task<bool> EventExistsAsync(Guid id);

        /// <summary>
        /// Checks if a user is the organizer of an event asynchronously.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>True if the user is the organizer, false otherwise.</returns>
        Task<bool> IsUserOrganizerAsync(Guid eventId, Guid userId);

        /// <summary>
        /// Gets events created by teams where the user is a member.
        /// </summary>
        /// <param name="userTeamIds">The team IDs where the user is a member.</param>
        /// <param name="filter">Optional filter parameters.</param>
        /// <returns>A tuple containing events and total count.</returns>
        Task<(IEnumerable<Event> Events, int TotalCount)> GetTeamEventsAsync(IEnumerable<Guid> userTeamIds, EventFilterRequest? filter = null);

        /// <summary>
        /// Gets a specific event by ID if it belongs to one of the user's teams.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userTeamIds">The team IDs where the user is a member.</param>
        /// <returns>The event if found and accessible, null otherwise.</returns>
        Task<Event?> GetTeamEventByIdAsync(Guid eventId, IEnumerable<Guid> userTeamIds);

        /// <summary>
        /// Checks if a user is a member of the team that created an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userTeamIds">The team IDs where the user is a member.</param>
        /// <returns>True if the user is a member of the team that created the event, false otherwise.</returns>
        Task<bool> IsUserTeamMemberAsync(Guid eventId, IEnumerable<Guid> userTeamIds);
    }
} 