using Microsoft.Extensions.Logging;
using Modules.TeamService.DTOs;
using Modules.TeamService.Models;
using Modules.TeamService.Repositories;
using Modules.UserService.Repositories;

namespace Modules.TeamService.Services
{
    /// <summary>
    /// Service implementation for team-related business operations.
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TeamService> _logger;

        public TeamService(ITeamRepository teamRepository, IUserRepository userRepository, ILogger<TeamService> logger)
        {
            _teamRepository = teamRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new team.
        /// </summary>
        /// <param name="request">The team creation request.</param>
        /// <returns>The created team response.</returns>
        public async Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request)
        {
            _logger.LogInformation("Creating team: {TeamName}", request.Name);

            // Validate team leader exists
            var teamLeader = await _userRepository.GetByIdAsync(request.TeamLeaderId);
            if (teamLeader == null)
            {
                throw new InvalidOperationException($"User with ID {request.TeamLeaderId} not found.");
            }

            // Validate team leader has appropriate role
            if (!IsValidTeamLeaderRole(teamLeader.Role))
            {
                throw new InvalidOperationException($"User with role {teamLeader.Role} cannot be a team leader.");
            }

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                TeamLeaderId = request.TeamLeaderId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdTeam = await _teamRepository.CreateTeamAsync(team);
            return MapTeamToResponse(createdTeam);
        }

        /// <summary>
        /// Gets a team by its ID.
        /// </summary>
        /// <param name="id">The team ID.</param>
        /// <returns>The team response if found, null otherwise.</returns>
        public async Task<TeamResponse?> GetTeamByIdAsync(Guid id)
        {
            var team = await _teamRepository.GetTeamByIdAsync(id);
            return team != null ? MapTeamToResponse(team) : null;
        }

        /// <summary>
        /// Gets all teams where a user is a member.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of team responses.</returns>
        public async Task<IEnumerable<TeamResponse>> GetUserTeamsAsync(Guid userId)
        {
            var teams = await _teamRepository.GetTeamsByUserIdAsync(userId);
            return teams.Select(MapTeamToResponse);
        }

        /// <summary>
        /// Updates a team.
        /// </summary>
        /// <param name="id">The team ID.</param>
        /// <param name="request">The team update request.</param>
        /// <returns>The updated team response.</returns>
        public async Task<TeamResponse?> UpdateTeamAsync(Guid id, CreateTeamRequest request)
        {
            _logger.LogInformation("Updating team: {TeamId}", id);

            var existingTeam = await _teamRepository.GetTeamByIdAsync(id);
            if (existingTeam == null)
            {
                _logger.LogWarning("Team not found for update: {TeamId}", id);
                return null;
            }

            // Validate team leader exists if changed
            if (existingTeam.TeamLeaderId != request.TeamLeaderId)
            {
                var newTeamLeader = await _userRepository.GetByIdAsync(request.TeamLeaderId);
                if (newTeamLeader == null)
                {
                    throw new InvalidOperationException($"User with ID {request.TeamLeaderId} not found.");
                }

                if (!IsValidTeamLeaderRole(newTeamLeader.Role))
                {
                    throw new InvalidOperationException($"User with role {newTeamLeader.Role} cannot be a team leader.");
                }
            }

            existingTeam.Name = request.Name;
            existingTeam.Description = request.Description;
            existingTeam.TeamLeaderId = request.TeamLeaderId;
            existingTeam.UpdatedAt = DateTime.UtcNow;

            var updatedTeam = await _teamRepository.UpdateTeamAsync(existingTeam);
            return MapTeamToResponse(updatedTeam);
        }

        /// <summary>
        /// Deletes a team.
        /// </summary>
        /// <param name="id">The team ID to delete.</param>
        /// <returns>True if the team was deleted, false otherwise.</returns>
        public async Task<bool> DeleteTeamAsync(Guid id)
        {
            return await _teamRepository.DeleteTeamAsync(id);
        }

        /// <summary>
        /// Adds a member to a team.
        /// </summary>
        /// <param name="request">The add team member request.</param>
        /// <returns>The added team member response.</returns>
        public async Task<TeamMemberResponse> AddTeamMemberAsync(AddTeamMemberRequest request)
        {
            _logger.LogInformation("Adding member {UserId} to team {TeamId}", request.UserId, request.TeamId);

            // Validate team exists
            var team = await _teamRepository.GetTeamByIdAsync(request.TeamId);
            if (team == null)
            {
                throw new InvalidOperationException($"Team with ID {request.TeamId} not found.");
            }

            // Validate user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {request.UserId} not found.");
            }

            // Validate user is not already a member
            var isAlreadyMember = await _teamRepository.IsUserTeamMemberAsync(request.TeamId, request.UserId);
            if (isAlreadyMember)
            {
                throw new InvalidOperationException($"User {request.UserId} is already a member of team {request.TeamId}.");
            }

            // Validate team role
            if (!IsValidTeamRole(request.TeamRole))
            {
                throw new InvalidOperationException($"Invalid team role: {request.TeamRole}");
            }

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = request.TeamId,
                UserId = request.UserId,
                TeamRole = request.TeamRole,
                JoinedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var addedTeamMember = await _teamRepository.AddTeamMemberAsync(teamMember);
            return MapTeamMemberToResponse(addedTeamMember);
        }

        /// <summary>
        /// Removes a member from a team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the member was removed, false otherwise.</returns>
        public async Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId)
        {
            return await _teamRepository.RemoveTeamMemberAsync(teamId, userId);
        }

        /// <summary>
        /// Gets team members for a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <returns>A collection of team member responses.</returns>
        public async Task<IEnumerable<TeamMemberResponse>> GetTeamMembersAsync(Guid teamId)
        {
            var teamMembers = await _teamRepository.GetTeamMembersAsync(teamId);
            return teamMembers.Select(MapTeamMemberToResponse);
        }

        /// <summary>
        /// Gets all team IDs where a user is a member.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A collection of team IDs.</returns>
        public async Task<IEnumerable<Guid>> GetUserTeamIdsAsync(Guid userId)
        {
            return await _teamRepository.GetUserTeamIdsAsync(userId);
        }

        /// <summary>
        /// Checks if a user is a member of a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the user is a member, false otherwise.</returns>
        public async Task<bool> IsUserTeamMemberAsync(Guid teamId, Guid userId)
        {
            return await _teamRepository.IsUserTeamMemberAsync(teamId, userId);
        }

        /// <summary>
        /// Gets the team role of a user in a specific team.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The team role if the user is a member, null otherwise.</returns>
        public async Task<string?> GetUserTeamRoleAsync(Guid teamId, Guid userId)
        {
            return await _teamRepository.GetUserTeamRoleAsync(teamId, userId);
        }

        /// <summary>
        /// Maps a Team entity to TeamResponse DTO.
        /// </summary>
        /// <param name="team">The team entity.</param>
        /// <returns>The team response DTO.</returns>
        private TeamResponse MapTeamToResponse(Team team)
        {
            return new TeamResponse
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                TeamLeaderId = team.TeamLeaderId,
                TeamLeaderName = team.TeamLeader != null ? $"{team.TeamLeader.FirstName} {team.TeamLeader.LastName}" : string.Empty,
                TeamLeaderEmail = team.TeamLeader?.Email ?? string.Empty,
                MemberCount = team.TeamMembers?.Count(tm => tm.IsActive) ?? 0,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                IsActive = team.IsActive
            };
        }

        /// <summary>
        /// Maps a TeamMember entity to TeamMemberResponse DTO.
        /// </summary>
        /// <param name="teamMember">The team member entity.</param>
        /// <returns>The team member response DTO.</returns>
        private TeamMemberResponse MapTeamMemberToResponse(TeamMember teamMember)
        {
            return new TeamMemberResponse
            {
                Id = teamMember.Id,
                TeamId = teamMember.TeamId,
                TeamName = teamMember.Team?.Name ?? string.Empty,
                UserId = teamMember.UserId,
                FirstName = teamMember.User?.FirstName ?? string.Empty,
                LastName = teamMember.User?.LastName ?? string.Empty,
                Email = teamMember.User?.Email ?? string.Empty,
                TeamRole = teamMember.TeamRole,
                GlobalRole = teamMember.User?.Role ?? string.Empty,
                JoinedAt = teamMember.JoinedAt,
                UpdatedAt = teamMember.UpdatedAt,
                IsActive = teamMember.IsActive
            };
        }

        /// <summary>
        /// Validates if a role is valid for being a team leader.
        /// </summary>
        /// <param name="role">The role to validate.</param>
        /// <returns>True if the role is valid for team leader, false otherwise.</returns>
        private bool IsValidTeamLeaderRole(string role)
        {
            return role == Shared.Kernel.Constants.RbacConstants.Roles.Organiser || 
                   role == Shared.Kernel.Constants.RbacConstants.Roles.Admin;
        }

        /// <summary>
        /// Validates if a team role is valid.
        /// </summary>
        /// <param name="teamRole">The team role to validate.</param>
        /// <returns>True if the team role is valid, false otherwise.</returns>
        private bool IsValidTeamRole(string teamRole)
        {
            return teamRole == Shared.Kernel.Constants.RbacConstants.Roles.Staff ||
                   teamRole == Shared.Kernel.Constants.RbacConstants.Roles.Organiser ||
                   teamRole == Shared.Kernel.Constants.RbacConstants.Roles.Admin;
        }
    }
} 