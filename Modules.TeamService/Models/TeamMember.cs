using System.ComponentModel.DataAnnotations;
using Modules.UserService.Models;

namespace Modules.TeamService.Models
{
    /// <summary>
    /// Model representing a team member in the system.
    /// </summary>
    public class TeamMember
    {
        /// <summary>
        /// The unique identifier of the team member.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The ID of the team.
        /// </summary>
        [Required]
        public Guid TeamId { get; set; }

        /// <summary>
        /// Navigation property for the team.
        /// </summary>
        public virtual Team? Team { get; set; }

        /// <summary>
        /// The ID of the user who is a member of the team.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Navigation property for the user.
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// The role of the user within the team (Staff, Organiser, Admin).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TeamRole { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the user joined the team.
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the team member is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The date and time when the team member was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 