using System.ComponentModel.DataAnnotations;

namespace Modules.EventService.DTOs
{
    /// <summary>
    /// Request model for creating a new event.
    /// </summary>
    public class CreateEventRequest
    {
        /// <summary>
        /// The title of the event.
        /// </summary>
        /// <example>Tech Conference 2024</example>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The detailed description of the event.
        /// </summary>
        /// <example>Join us for an exciting day of technology talks and networking.</example>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The start date and time of the event.
        /// </summary>
        /// <example>2024-06-15T09:00:00Z</example>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date and time of the event.
        /// </summary>
        /// <example>2024-06-15T17:00:00Z</example>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The venue or location of the event.
        /// </summary>
        /// <example>Convention Center, 123 Main Street</example>
        [Required]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// The category of the event.
        /// </summary>
        /// <example>Technology</example>
        public string? Category { get; set; }

        /// <summary>
        /// Whether the event is public or private.
        /// </summary>
        /// <example>true</example>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Whether the event should be published immediately upon creation. 
        /// If false, the event will be created as a draft.
        /// </summary>
        /// <example>false</example>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// The initial status of the event. Valid values: "Draft", "Published".
        /// If not specified, defaults to "Draft" unless IsPublished is true.
        /// </summary>
        /// <example>Draft</example>
        public string? Status { get; set; }

        /// <summary>
        /// The URL of the event's thumbnail image.
        /// </summary>
        /// <example>https://example.com/images/event-thumbnail.jpg</example>
        [StringLength(2000)]
        public string? ImageUrl { get; set; }
    }
} 