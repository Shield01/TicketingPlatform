namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Response model for ticket tier information within event responses.
    /// This is a simplified version to avoid circular dependencies with TicketService.
    /// </summary>
    public class EventTicketTierResponse
    {
        /// <summary>
        /// The unique identifier of the ticket tier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the ticket tier.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket tier.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The price of the ticket tier.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The currency of the ticket price.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The maximum quantity of tickets available for this tier.
        /// </summary>
        public int MaxQuantity { get; set; }

        /// <summary>
        /// The number of tickets sold in this tier.
        /// </summary>
        public int SoldQuantity { get; set; }

        /// <summary>
        /// The number of tickets still available for this tier.
        /// </summary>
        public int AvailableQuantity => MaxQuantity - SoldQuantity;

        /// <summary>
        /// Whether this tier is currently available for purchase.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// The date and time when the ticket tier was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
