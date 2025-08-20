using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Request model for a single ticket tier.
    /// </summary>
    public class TicketTierRequest
    {
        /// <summary>
        /// The name of the ticket tier.
        /// </summary>
        /// <example>VIP</example>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket tier.
        /// </summary>
        /// <example>Premium access with exclusive benefits</example>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The price of the ticket tier.
        /// </summary>
        /// <example>150.00</example>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        /// <summary>
        /// The total quantity of tickets available for this tier.
        /// </summary>
        /// <example>50</example>
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
} 