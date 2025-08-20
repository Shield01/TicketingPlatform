using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Request model for creating ticket tiers.
    /// </summary>
    public class CreateTicketTiersRequest
    {
        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        /// <example>12345678-1234-1234-1234-123456789012</example>
        [Required]
        public Guid EventId { get; set; }

        /// <summary>
        /// The list of ticket tiers to create.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<TicketTierRequest> Tiers { get; set; } = new();
    }
} 