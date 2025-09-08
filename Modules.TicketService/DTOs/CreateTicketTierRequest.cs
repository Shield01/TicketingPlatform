using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Request model for creating a single ticket tier for an event.
    /// </summary>
    public class CreateTicketTierRequest
    {
        /// <summary>
        /// The name of the ticket tier (must be unique per event).
        /// </summary>
        /// <example>VIP</example>
        [Required(ErrorMessage = "Tier name is required.")]
        [StringLength(100, ErrorMessage = "Tier name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket tier.
        /// </summary>
        /// <example>Premium access with exclusive benefits</example>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// The price of the ticket tier (must be greater than 0).
        /// </summary>
        /// <example>150.00</example>
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        /// <summary>
        /// The currency code for the ticket price.
        /// </summary>
        /// <example>USD</example>
        [StringLength(3, ErrorMessage = "Currency must be a 3-character code.")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The maximum quantity of tickets available for this tier (must be 0 or greater).
        /// </summary>
        /// <example>50</example>
        [Required(ErrorMessage = "Maximum quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Maximum quantity must be 0 or greater.")]
        public int MaxQuantity { get; set; }

        /// <summary>
        /// Whether this tier is available for purchase.
        /// </summary>
        /// <example>true</example>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// The date and time when sales for this tier start (optional).
        /// </summary>
        /// <example>2024-12-01T00:00:00Z</example>
        public DateTime? SaleStartDate { get; set; }

        /// <summary>
        /// The date and time when sales for this tier end (optional).
        /// </summary>
        /// <example>2024-12-31T23:59:59Z</example>
        public DateTime? SaleEndDate { get; set; }
    }
}
