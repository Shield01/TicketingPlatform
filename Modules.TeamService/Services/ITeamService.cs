using Modules.TeamService.DTOs;

namespace Modules.TeamService.Services
{
    /// <summary>
    /// Service interface for team-related business operations.
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// Creates a new team.
        /// </summary>
        /// <param name="request">The team creation request.</param>
        /// <returns>The created team response.</returns>
        Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request);

        /// <summary>
        /// Gets a team by its ID.
        /// </summary>
        /// <param name="id">The team ID.</param>
        /// <returns>The team response if found, null otherwise.</returns>
        Task<TeamResponse?> GetTeamByIdAsync(Guid id);

        /// <summary>
        /// Gets all teams where a user is a member.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of team responses.</returns>
        Task<IEnumerable<TeamResponse>> GetUserTeamsAsync(Guid userId);

        /// <summary>
        /// Updates a team.
        /// </summary>
        /// <param name="id">The team ID.</param>
        /// <param name="request">The team update request.</param>
        /// <returns>The updated team response.</returns>
        Task<TeamResponse?> UpdateTeamAsync(Guid id, CreateTeamRequest request);

        /// <summary>
        /// Deletes a team.
        /// </summary>
        /// <param name="id">The team ID to delete.</param>
        /// <returns>True if the team was deleted, false otherwise.</returns>
        Task<bool> DeleteTeamAsync(Guid id);

        /// <summary>
        /// Adds a member to a team.
        /// </summary>
        /// <param name="request">The add team member request.</param>
        /// <returns>The added team member response.</returns>
        Task<TeamMemberResponse> AddTeamMemberAsync(AddTeamMemberRequest request);

        /// <summary>
        /// Removes a member from a team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the member was removed, false otherwise.</returns>
        Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId);

        /// <summary>
        /// Gets team members for a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <returns>A collection of team member responses.</returns>
        Task<IEnumerable<TeamMemberResponse>> GetTeamMembersAsync(Guid teamId);

        /// <summary>
        /// Gets all team IDs where a user is a member.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of team IDs.</returns>
        Task<IEnumerable<Guid>> GetUserTeamIdsAsync(Guid userId);

        /// <summary>
        /// Checks if a user is a member of a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the user is a member, false otherwise.</returns>
        Task<bool> IsUserTeamMemberAsync(Guid teamId, Guid userId);

        /// <summary>
        /// Gets the team role of a user in a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The team role if the user is a member, null otherwise.</returns>
        Task<string?> GetUserTeamRoleAsync(Guid teamId, Guid userId);
    }
} 