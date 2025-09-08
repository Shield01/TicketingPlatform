namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Response model for ticket issuance operation.
    /// </summary>
    public class IssueTicketResponse
    {
        /// <summary>
        /// The list of tickets that were issued.
        /// </summary>
        public IEnumerable<TicketResponse> Tickets { get; set; } = new List<TicketResponse>();

        /// <summary>
        /// The number of tickets successfully issued.
        /// </summary>
        public int TicketsIssued { get; set; }

        /// <summary>
        /// The total price for all issued tickets.
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// The currency of the total price.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the payment.
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// The date and time when the tickets were issued.
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// A message indicating the result of the ticket issuance.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
