using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Modules.UserService.Models;
using Modules.TeamService.Models;

namespace Modules.EventService.Models
{
    /// <summary>
    /// Model representing an event in the ticketing platform.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The title of the event.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The detailed description of the event.
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The start date and time of the event.
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date and time of the event.
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The venue or location of the event.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// The category of the event.
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Whether the event is public or private.
        /// </summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// The current status of the event (Draft, Published, Cancelled, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Whether the event is published and visible to the public.
        /// </summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// The unique identifier of the user who created the event.
        /// </summary>
        [Required]
        public Guid OrganizerId { get; set; }

        /// <summary>
        /// The unique identifier of the team that owns the event (optional).
        /// </summary>
        public Guid? TeamId { get; set; }

        /// <summary>
        /// The date and time when the event was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the event was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the event is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The minimum ticket price for the event (cached from ticket tiers for performance).
        /// This value is automatically updated when ticket tiers are created, updated, or sold out.
        /// </summary>
        public decimal? MinimumPrice { get; set; }

        /// <summary>
        /// The URL of the event's thumbnail image.
        /// </summary>
        [StringLength(2000)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Navigation property for the organizer of the event.
        /// </summary>
        [ForeignKey(nameof(OrganizerId))]
        public virtual User? Organizer { get; set; }

        /// <summary>
        /// Navigation property for the team that owns the event.
        /// </summary>
        [ForeignKey(nameof(TeamId))]
        public virtual Team? Team { get; set; }

        /// <summary>
        /// Validates that the end date is after the start date.
        /// </summary>
        /// <returns>True if the event dates are valid, false otherwise.</returns>
        public bool ValidateDates()
        {
            return EndDate > StartDate;
        }

        /// <summary>
        /// Validates that the event is not in the past.
        /// </summary>
        /// <returns>True if the event is in the future, false otherwise.</returns>
        public bool ValidateEventNotInPast()
        {
            return StartDate > DateTime.UtcNow;
        }
    }
} 