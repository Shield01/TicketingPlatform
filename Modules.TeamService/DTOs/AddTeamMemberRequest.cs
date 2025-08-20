using System.ComponentModel.DataAnnotations;

namespace Modules.TeamService.DTOs
{
    /// <summary>
    /// Request DTO for adding a member to a team.
    /// </summary>
    public class AddTeamMemberRequest
    {
        /// <summary>
        /// The ID of the team.
        /// </summary>
        [Required]
        public Guid TeamId { get; set; }

        /// <summary>
        /// The ID of the user to add to the team.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The role of the user within the team.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TeamRole { get; set; } = string.Empty;
    }
} 