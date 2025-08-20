namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Response model for ticket tier information.
    /// </summary>
    public class TicketTierResponse
    {
        /// <summary>
        /// The unique identifier of the ticket tier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// The name of the ticket tier.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket tier.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The price of the ticket tier.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The total quantity of tickets available for this tier.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The number of tickets still available for this tier.
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// The date and time when the ticket tier was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 