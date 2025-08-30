using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.Models
{
    /// <summary>
    /// Model representing a ticket tier configuration for an event.
    /// </summary>
    public class TicketTier
    {
        /// <summary>
        /// The unique identifier of the ticket tier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the event this tier belongs to.
        /// </summary>
        [Required]
        public Guid EventId { get; set; }

        /// <summary>
        /// The name of the ticket tier.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket tier.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// The price of tickets in this tier.
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// The currency of the ticket price.
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The maximum number of tickets available in this tier.
        /// </summary>
        [Required]
        public int MaxQuantity { get; set; }

        /// <summary>
        /// The number of tickets sold in this tier.
        /// </summary>
        public int SoldQuantity { get; set; } = 0;

        /// <summary>
        /// Whether this tier is currently available for purchase.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// The date and time when sales for this tier start.
        /// </summary>
        public DateTime? SaleStartDate { get; set; }

        /// <summary>
        /// The date and time when sales for this tier end.
        /// </summary>
        public DateTime? SaleEndDate { get; set; }

        /// <summary>
        /// The date and time when the ticket tier was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the ticket tier was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the ticket tier is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Collection of tickets sold in this tier.
        /// </summary>
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
