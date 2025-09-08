namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Response model for user tickets with pagination.
    /// </summary>
    public class UserTicketsResponse
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The list of tickets owned by the user.
        /// </summary>
        public IEnumerable<TicketResponse> Tickets { get; set; } = new List<TicketResponse>();

        /// <summary>
        /// The total number of tickets for the user.
        /// </summary>
        public int TotalTickets { get; set; }

        /// <summary>
        /// The number of unused tickets.
        /// </summary>
        public int UnusedTickets { get; set; }

        /// <summary>
        /// The number of used tickets.
        /// </summary>
        public int UsedTickets { get; set; }

        /// <summary>
        /// The number of cancelled tickets.
        /// </summary>
        public int CancelledTickets { get; set; }

        /// <summary>
        /// The current page number (for pagination).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Whether there are more tickets available.
        /// </summary>
        public bool HasNextPage => (Page * PageSize) < TotalTickets;

        /// <summary>
        /// Whether there are previous tickets.
        /// </summary>
        public bool HasPreviousPage => Page > 1;
    }
}
