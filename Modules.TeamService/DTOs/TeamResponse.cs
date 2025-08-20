namespace Modules.TeamService.DTOs
{
    /// <summary>
    /// Response DTO for team information.
    /// </summary>
    public class TeamResponse
    {
        /// <summary>
        /// The unique identifier of the team.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the team.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the team.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the team leader.
        /// </summary>
        public Guid TeamLeaderId { get; set; }

        /// <summary>
        /// The name of the team leader.
        /// </summary>
        public string TeamLeaderName { get; set; } = string.Empty;

        /// <summary>
        /// The email of the team leader.
        /// </summary>
        public string TeamLeaderEmail { get; set; } = string.Empty;

        /// <summary>
        /// The number of active team members.
        /// </summary>
        public int MemberCount { get; set; }



        /// <summary>
        /// The date and time when the team was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the team was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Indicates whether the team is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
} 