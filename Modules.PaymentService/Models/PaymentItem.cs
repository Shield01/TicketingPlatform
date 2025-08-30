using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.Models
{
    /// <summary>
    /// Model representing an individual item in a payment (e.g., tickets purchased).
    /// </summary>
    public class PaymentItem
    {
        /// <summary>
        /// The unique identifier of the payment item.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the parent payment.
        /// </summary>
        [Required]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// The type of item being purchased (ticket, merchandise, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ItemType { get; set; } = "Ticket";

        /// <summary>
        /// The unique identifier of the item (e.g., TicketTier ID).
        /// </summary>
        [Required]
        public Guid ItemId { get; set; }

        /// <summary>
        /// The name or description of the item.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// The quantity of this item purchased.
        /// </summary>
        [Required]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// The unit price of the item.
        /// </summary>
        [Required]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// The total price for this item (Quantity * UnitPrice).
        /// </summary>
        [Required]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// The currency of the prices.
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The date and time when the payment item was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the payment item was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the payment item is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property to the parent payment.
        /// </summary>
        public virtual Payment? Payment { get; set; }
    }
}
