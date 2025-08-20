namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Response model for paginated events list.
    /// </summary>
    public class PaginatedEventsResponse
    {
        /// <summary>
        /// The list of events for the current page.
        /// </summary>
        public List<EventResponse> Events { get; set; } = new();

        /// <summary>
        /// The total number of events matching the filter criteria.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Indicates whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
} 