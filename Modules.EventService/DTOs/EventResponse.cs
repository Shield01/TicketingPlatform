namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Response model for event information.
    /// </summary>
    public class EventResponse
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
        /// The detailed description of the event.
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
        /// The current status of the event (Draft, Live, Cancelled).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The category of the event.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// The URL of the event's thumbnail image.
        /// </summary>
        public string? ImageURL { get; set; }

        /// <summary>
        /// Whether the event is public or private.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Whether the event is published and visible to the public.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// The unique identifier of the event organizer.
        /// </summary>
        public Guid OrganizerId { get; set; }

        /// <summary>
        /// The date and time when the event was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the event was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// The name of the event organizer.
        /// </summary>
        public string OrganizerName { get; set; } = string.Empty;

        /// <summary>
        /// The ticket tiers available for this event.
        /// </summary>
        public IEnumerable<EventTicketTierResponse> TicketTiers { get; set; } = new List<EventTicketTierResponse>();
    }
} 