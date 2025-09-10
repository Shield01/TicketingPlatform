namespace Shared.Kernel.Interfaces
{
    /// <summary>
    /// Interface for retrieving event information across modules.
    /// </summary>
    public interface IEventInfoService
    {
        /// <summary>
        /// Gets event information by event ID.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>The event information if found, null otherwise.</returns>
        Task<EventInfo?> GetEventInfoAsync(Guid eventId);
    }

    /// <summary>
    /// Event information model for cross-module communication.
    /// </summary>
    public class EventInfo
    {
        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The title of the event.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The description of the event.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The start date and time of the event.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date and time of the event.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The venue or location of the event.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the event.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Whether the event is published and visible to the public.
        /// </summary>
        public bool IsPublished { get; set; }
    }
}
