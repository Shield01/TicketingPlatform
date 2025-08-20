using System.ComponentModel.DataAnnotations;
using Modules.UserService.Models;

namespace Modules.TeamService.Models
{
    /// <summary>
    /// Model representing a team in the system for event management.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// The unique identifier of the team.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the team.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the team.
        /// </summary>
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the team leader (Organiser or Admin).
        /// </summary>
        [Required]
        public Guid TeamLeaderId { get; set; }

        /// <summary>
        /// Navigation property for the team leader.
        /// </summary>
        public virtual User? TeamLeader { get; set; }

        /// <summary>
        /// Collection of team members.
        /// </summary>
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

        /// <summary>
        /// The date and time when the team was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the team was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the team is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
} 