namespace Modules.TeamService.DTOs
{
    /// <summary>
    /// Response DTO for team member information.
    /// </summary>
    public class TeamMemberResponse
    {
        /// <summary>
        /// The unique identifier of the team member.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The ID of the team.
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// The name of the team.
        /// </summary>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The first name of the user.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The last name of the user.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// The email of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The role of the user within the team.
        /// </summary>
        public string TeamRole { get; set; } = string.Empty;

        /// <summary>
        /// The global role of the user in the system.
        /// </summary>
        public string GlobalRole { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the user joined the team.
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// The date and time when the team member was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Indicates whether the team member is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
} 