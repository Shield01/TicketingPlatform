using System.ComponentModel.DataAnnotations;

namespace Modules.TeamService.DTOs
{
    /// <summary>
    /// Request DTO for creating a new team.
    /// </summary>
    public class CreateTeamRequest
    {
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
        /// The ID of the team leader.
        /// </summary>
        [Required]
        public Guid TeamLeaderId { get; set; }
    }
} 