namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Request model for filtering events with various parameters.
    /// </summary>
    public class EventFilterRequest
    {
        /// <summary>
        /// Filter by event status (Draft, Published, Cancelled).
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Filter by event category.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Filter by event type (upcoming, past, all).
        /// </summary>
        public string? EventType { get; set; }

        /// <summary>
        /// Search by keyword in title, description, or location.
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// Filter by location.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Filter by start date (events starting from this date).
        /// </summary>
        public DateTime? StartDateFrom { get; set; }

        /// <summary>
        /// Filter by start date (events starting before this date).
        /// </summary>
        public DateTime? StartDateTo { get; set; }

        /// <summary>
        /// Page number for pagination (default: 1).
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page (default: 10, max: 100).
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Sort field (Title, StartDate, CreatedAt).
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc, desc).
        /// </summary>
        public string? SortDirection { get; set; } = "asc";
    }
} 