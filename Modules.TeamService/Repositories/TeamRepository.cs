using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.TeamService.Data;
using Modules.TeamService.Models;

namespace Modules.TeamService.Repositories
{
    /// <summary>
    /// Repository implementation for team-related data operations.
    /// </summary>
    public class TeamRepository : ITeamRepository
    {
        private readonly TeamServiceDbContext _context;
        private readonly ILogger<TeamRepository> _logger;

        public TeamRepository(TeamServiceDbContext context, ILogger<TeamRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new team.
        /// </summary>
        /// <param name="team">The team to create.</param>
        /// <returns>The created team.</returns>
        public async Task<Team> CreateTeamAsync(Team team)
        {
            _logger.LogInformation("Creating team: {TeamName}", team.Name);
            
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Team created successfully: {TeamId}", team.Id);
            return team;
        }

        /// <summary>
        /// Gets a team by its ID.
        /// </summary>
        /// <param name="id">The team ID.</param>
        /// <returns>The team if found, null otherwise.</returns>
        public async Task<Team?> GetTeamByIdAsync(Guid id)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        /// <summary>
        /// Gets all teams.
        /// </summary>
        /// <returns>A collection of all teams.</returns>
        public async Task<IEnumerable<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                .Where(t => t.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets teams where a user is a member.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of teams where the user is a member.</returns>
        public async Task<IEnumerable<Team>> GetTeamsByUserIdAsync(Guid userId)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                .Where(t => t.IsActive && 
                           (t.TeamLeaderId == userId || 
                            t.TeamMembers.Any(tm => tm.UserId == userId && tm.IsActive)))
                .ToListAsync();
        }

        /// <summary>
        /// Gets teams where a user is the team leader.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of teams where the user is the team leader.</returns>
        public async Task<IEnumerable<Team>> GetTeamsByLeaderIdAsync(Guid userId)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                .Where(t => t.IsActive && t.TeamLeaderId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Updates a team.
        /// </summary>
        /// <param name="team">The team to update.</param>
        /// <returns>The updated team.</returns>
        public async Task<Team> UpdateTeamAsync(Team team)
        {
            _logger.LogInformation("Updating team: {TeamId}", team.Id);
            
            team.UpdatedAt = DateTime.UtcNow;
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Team updated successfully: {TeamId}", team.Id);
            return team;
        }

        /// <summary>
        /// Deletes a team.
        /// </summary>
        /// <param name="id">The team ID to delete.</param>
        /// <returns>True if the team was deleted, false otherwise.</returns>
        public async Task<bool> DeleteTeamAsync(Guid id)
        {
            _logger.LogInformation("Deleting team: {TeamId}", id);
            
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                _logger.LogWarning("Team not found for deletion: {TeamId}", id);
                return false;
            }

            team.IsActive = false;
            team.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Team deleted successfully: {TeamId}", id);
            return true;
        }

        /// <summary>
        /// Adds a member to a team.
        /// </summary>
        /// <param name="teamMember">The team member to add.</param>
        /// <returns>The added team member.</returns>
        public async Task<TeamMember> AddTeamMemberAsync(TeamMember teamMember)
        {
            _logger.LogInformation("Adding member {UserId} to team {TeamId}", teamMember.UserId, teamMember.TeamId);
            
            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Member added successfully to team: {TeamId}", teamMember.TeamId);
            return teamMember;
        }

        /// <summary>
        /// Removes a member from a team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the member was removed, false otherwise.</returns>
        public async Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId)
        {
            _logger.LogInformation("Removing member {UserId} from team {TeamId}", userId, teamId);
            
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId && tm.IsActive);
            
            if (teamMember == null)
            {
                _logger.LogWarning("Team member not found: TeamId={TeamId}, UserId={UserId}", teamId, userId);
                return false;
            }

            teamMember.IsActive = false;
            teamMember.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Member removed successfully from team: {TeamId}", teamId);
            return true;
        }

        /// <summary>
        /// Gets team members for a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <returns>A collection of team members.</returns>
        public async Task<IEnumerable<TeamMember>> GetTeamMembersAsync(Guid teamId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.Team)
                .Where(tm => tm.TeamId == teamId && tm.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all teams where a user is a member (including as team leader).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of team IDs where the user is a member.</returns>
        public async Task<IEnumerable<Guid>> GetUserTeamIdsAsync(Guid userId)
        {
            var teamIds = await _context.Teams
                .Where(t => t.IsActive && t.TeamLeaderId == userId)
                .Select(t => t.Id)
                .ToListAsync();

            var memberTeamIds = await _context.TeamMembers
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            return teamIds.Union(memberTeamIds).Distinct();
        }

        /// <summary>
        /// Checks if a user is a member of a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the user is a member, false otherwise.</returns>
        public async Task<bool> IsUserTeamMemberAsync(Guid teamId, Guid userId)
        {
            var isLeader = await _context.Teams
                .AnyAsync(t => t.Id == teamId && t.IsActive && t.TeamLeaderId == userId);

            if (isLeader) return true;

            return await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId && tm.IsActive);
        }

        /// <summary>
        /// Gets the team role of a user in a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The team role if the user is a member, null otherwise.</returns>
        public async Task<string?> GetUserTeamRoleAsync(Guid teamId, Guid userId)
        {
            // Check if user is team leader
            var isLeader = await _context.Teams
                .AnyAsync(t => t.Id == teamId && t.IsActive && t.TeamLeaderId == userId);

            if (isLeader) return "TeamLeader";

            // Check team member role
            var teamMember = await _context.TeamMembers
                .Where(tm => tm.TeamId == teamId && tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.TeamRole)
                .FirstOrDefaultAsync();

            return teamMember;
        }
    }
} 