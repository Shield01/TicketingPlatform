using Shared.Kernel.Interfaces;
using Modules.EventService.DTOs;

namespace Modules.EventService.Services
{
    /// <summary>
    /// Implementation of IEventInfoService for cross-module event information access.
    /// </summary>
    public class EventInfoService : IEventInfoService
    {
        private readonly IEventService _eventService;

        /// <summary>
        /// Initializes a new instance of the EventInfoService.
        /// </summary>
        /// <param name="eventService">The event service.</param>
        public EventInfoService(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Gets event information by event ID.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>The event information if found, null otherwise.</returns>
        public async Task<EventInfo?> GetEventInfoAsync(Guid eventId)
        {
            var eventResponse = await _eventService.GetEventByIdAsync(eventId);
            if (eventResponse == null)
            {
                return null;
            }

            return new EventInfo
            {
                Id = eventResponse.Id,
                Title = eventResponse.Title,
                Description = eventResponse.Description,
                StartDate = eventResponse.StartDate,
                EndDate = eventResponse.EndDate,
                Location = eventResponse.Location,
                Status = eventResponse.Status,
                IsPublished = eventResponse.IsPublished
            };
        }
    }
}
