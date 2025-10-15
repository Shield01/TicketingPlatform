using System.ComponentModel.DataAnnotations;

namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Request model for updating an existing event.
    /// </summary>
    public class UpdateEventRequest
    {
        /// <summary>
        /// The updated title of the event.
        /// </summary>
        /// <example>Updated Tech Conference 2024</example>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The updated description of the event.
        /// </summary>
        /// <example>Updated description with new details.</example>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The updated start date and time of the event.
        /// </summary>
        /// <example>2024-06-15T09:00:00Z</example>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The updated end date and time of the event.
        /// </summary>
        /// <example>2024-06-15T17:00:00Z</example>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The updated venue or location of the event.
        /// </summary>
        /// <example>Updated Convention Center, 456 New Street</example>
        [Required]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// The updated category of the event.
        /// </summary>
        /// <example>Technology</example>
        public string? Category { get; set; }

        /// <summary>
        /// Whether the event is public or private.
        /// </summary>
        /// <example>true</example>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// The updated status of the event.
        /// </summary>
        /// <example>Published</example>
        [StringLength(50)]
        public string Status { get; set; } = "Draft";
    }
} 