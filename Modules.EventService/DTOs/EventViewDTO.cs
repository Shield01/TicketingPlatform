namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Response model for public event viewing with essential information only.
    /// </summary>
    public class EventViewDTO
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
        /// The category of the event.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// The name of the event organizer.
        /// </summary>
        public string OrganizerName { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the event was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Indicates if the event is upcoming (future date).
        /// </summary>
        public bool IsUpcoming { get; set; }

        /// <summary>
        /// The number of days until the event starts.
        /// </summary>
        public int DaysUntilEvent { get; set; }

        /// <summary>
        /// The minimum ticket price for the event (if available).
        /// </summary>
        public decimal? MinimumPrice { get; set; }

        /// <summary>
        /// The currency of the minimum ticket price (e.g., "USD", "NGN", "EUR").
        /// </summary>
        public string? MinimumPriceCurrency { get; set; }

        /// <summary>
        /// Formatted minimum price with currency for display (e.g., "50 USD", "5000 NGN").
        /// </summary>
        public string? MinimumPriceFormatted => MinimumPrice.HasValue && !string.IsNullOrEmpty(MinimumPriceCurrency) 
            ? $"{MinimumPrice:N2} {MinimumPriceCurrency}" 
            : null;

        /// <summary>
        /// The URL of the event's thumbnail image.
        /// </summary>
        public string? ImageUrl { get; set; }
    }
} 