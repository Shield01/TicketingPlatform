using Modules.TeamService.Models;
using Modules.TeamService.DTOs;

namespace Modules.TeamService.Repositories
{
    /// <summary>
    /// Repository interface for team-related data operations.
    /// </summary>
    public interface ITeamRepository
    {
        /// <summary>
        /// Creates a new team.
        /// </summary>
        /// <param name="team">The team to create.</param>
        /// <returns>The created team.</returns>
        Task<Team> CreateTeamAsync(Team team);

        /// <summary>
        /// Gets a team by its ID.
        /// </summary>
        /// <param name="id">The team ID.</param>
        /// <returns>The team if found, null otherwise.</returns>
        Task<Team?> GetTeamByIdAsync(Guid id);

        /// <summary>
        /// Gets all teams.
        /// </summary>
        /// <returns>A collection of all teams.</returns>
        Task<IEnumerable<Team>> GetAllTeamsAsync();

        /// <summary>
        /// Gets teams where a user is a member.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of teams where the user is a member.</returns>
        Task<IEnumerable<Team>> GetTeamsByUserIdAsync(Guid userId);

        /// <summary>
        /// Gets teams where a user is the team leader.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of teams where the user is the team leader.</returns>
        Task<IEnumerable<Team>> GetTeamsByLeaderIdAsync(Guid userId);

        /// <summary>
        /// Updates a team.
        /// </summary>
        /// <param name="team">The team to update.</param>
        /// <returns>The updated team.</returns>
        Task<Team> UpdateTeamAsync(Team team);

        /// <summary>
        /// Deletes a team.
        /// </summary>
        /// <param name="id">The team ID to delete.</param>
        /// <returns>True if the team was deleted, false otherwise.</returns>
        Task<bool> DeleteTeamAsync(Guid id);

        /// <summary>
        /// Adds a member to a team.
        /// </summary>
        /// <param name="teamMember">The team member to add.</param>
        /// <returns>The added team member.</returns>
        Task<TeamMember> AddTeamMemberAsync(TeamMember teamMember);

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
        /// <returns>A collection of team members.</returns>
        Task<IEnumerable<TeamMember>> GetTeamMembersAsync(Guid teamId);

        /// <summary>
        /// Gets all teams where a user is a member (including as team leader).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of team IDs where the user is a member.</returns>
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