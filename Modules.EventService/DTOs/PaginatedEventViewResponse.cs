namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Response model for paginated public events list.
    /// </summary>
    public class PaginatedEventViewResponse
    {
        /// <summary>
        /// The list of events for the current page.
        /// </summary>
        public List<EventViewDTO> Events { get; set; } = new();

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
        /// Indicates if there are more pages available.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Indicates if there are previous pages available.
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
} 