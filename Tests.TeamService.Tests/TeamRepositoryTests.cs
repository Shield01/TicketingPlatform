using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.TeamService.Data;
using Modules.TeamService.Models;
using Modules.TeamService.Repositories;
using Modules.UserService.Models;
using Xunit;
using Moq;

namespace Tests.TeamService.Tests
{
    public class TeamRepositoryTests : IDisposable
    {
        private readonly TeamServiceDbContext _context;
        private readonly TeamRepository _repository;
        private readonly Mock<ILogger<TeamRepository>> _mockLogger;

        public TeamRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TeamServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TeamServiceDbContext(options);
            _mockLogger = new Mock<ILogger<TeamRepository>>();
            _repository = new TeamRepository(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreateTeamAsync_ValidTeam_ReturnsCreatedTeam()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team",
                Description = "Test Description",
                TeamLeaderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Act
            var result = await _repository.CreateTeamAsync(team);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(team.Name, result.Name);
            Assert.Equal(team.Description, result.Description);
            Assert.Equal(team.TeamLeaderId, result.TeamLeaderId);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetTeamByIdAsync_ExistingTeam_ReturnsTeam()
        {
            // Arrange
            var teamLeaderId = Guid.NewGuid();
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                Description = "Test Description",
                TeamLeaderId = teamLeaderId,
                IsActive = true
            };

            // Add a user for the team leader
            var teamLeader = new User
            {
                Id = teamLeaderId,
                Email = "leader@test.com",
                FirstName = "Team",
                LastName = "Leader",
                Role = "Organiser"
            };

            _context.Users.Add(teamLeader);
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Verify the team was actually saved
            var savedTeam = await _context.Teams.FindAsync(team.Id);
            Assert.NotNull(savedTeam);

            // Act
            var result = await _repository.GetTeamByIdAsync(team.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(team.Id, result.Id);
            Assert.Equal(team.Name, result.Name);
        }

        [Fact]
        public async Task GetTeamByIdAsync_NonExistentTeam_ReturnsNull()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            // Act
            var result = await _repository.GetTeamByIdAsync(teamId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllTeamsAsync_ReturnsOnlyActiveTeams()
        {
            // Arrange
            var teamLeader1Id = Guid.NewGuid();
            var teamLeader2Id = Guid.NewGuid();
            var teamLeader3Id = Guid.NewGuid();

            var users = new List<User>
            {
                new User { Id = teamLeader1Id, Email = "leader1@test.com", FirstName = "Leader", LastName = "1", Role = "Organiser" },
                new User { Id = teamLeader2Id, Email = "leader2@test.com", FirstName = "Leader", LastName = "2", Role = "Organiser" },
                new User { Id = teamLeader3Id, Email = "leader3@test.com", FirstName = "Leader", LastName = "3", Role = "Organiser" }
            };

            var teams = new List<Team>
            {
                new Team { Id = Guid.NewGuid(), Name = "Team 1", TeamLeaderId = teamLeader1Id, IsActive = true },
                new Team { Id = Guid.NewGuid(), Name = "Team 2", TeamLeaderId = teamLeader2Id, IsActive = true },
                new Team { Id = Guid.NewGuid(), Name = "Team 3", TeamLeaderId = teamLeader3Id, IsActive = false }
            };

            _context.Users.AddRange(users);
            _context.Teams.AddRange(teams);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllTeamsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count()); // Only active teams
            Assert.All(result, team => Assert.True(team.IsActive));
        }

        [Fact]
        public async Task UpdateTeamAsync_ExistingTeam_ReturnsUpdatedTeam()
        {
            // Arrange
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Description = "Original Description",
                TeamLeaderId = Guid.NewGuid(),
                IsActive = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Act
            team.Name = "Updated Name";
            team.Description = "Updated Description";
            team.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateTeamAsync(team);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Description", result.Description);
        }

        [Fact]
        public async Task DeleteTeamAsync_ExistingTeam_ReturnsTrue()
        {
            // Arrange
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                IsActive = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteTeamAsync(team.Id);

            // Assert
            Assert.True(result);
            var deletedTeam = await _context.Teams.FindAsync(team.Id);
            Assert.False(deletedTeam!.IsActive);
        }

        [Fact]
        public async Task DeleteTeamAsync_NonExistentTeam_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteTeamAsync(teamId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddTeamMemberAsync_ValidTeamMember_ReturnsCreatedTeamMember()
        {
            // Arrange
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                IsActive = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = Guid.NewGuid(),
                TeamRole = "Member",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Act
            var result = await _repository.AddTeamMemberAsync(teamMember);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(team.Id, result.TeamId);
            Assert.Equal(teamMember.UserId, result.UserId);
            Assert.Equal(teamMember.TeamRole, result.TeamRole);
        }

        [Fact]
        public async Task RemoveTeamMemberAsync_ExistingTeamMember_ReturnsTrue()
        {
            // Arrange
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                IsActive = true
            };

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = Guid.NewGuid(),
                TeamRole = "Member",
                IsActive = true
            };

            _context.Teams.Add(team);
            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.RemoveTeamMemberAsync(team.Id, teamMember.UserId);

            // Assert
            Assert.True(result);
            var removedMember = await _context.TeamMembers.FindAsync(teamMember.Id);
            Assert.False(removedMember!.IsActive);
        }

        [Fact]
        public async Task RemoveTeamMemberAsync_NonExistentTeamMember_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.RemoveTeamMemberAsync(teamId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetTeamMembersAsync_ValidTeamId_ReturnsTeamMembers()
        {
            // Arrange
            var teamLeaderId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var users = new List<User>
            {
                new User { Id = teamLeaderId, Email = "leader@test.com", FirstName = "Team", LastName = "Leader", Role = "Organiser" },
                new User { Id = userId1, Email = "member1@test.com", FirstName = "Member", LastName = "1", Role = "Staff" },
                new User { Id = userId2, Email = "member2@test.com", FirstName = "Member", LastName = "2", Role = "Staff" }
            };

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                TeamLeaderId = teamLeaderId,
                IsActive = true
            };

            var teamMembers = new List<TeamMember>
            {
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = team.Id,
                    UserId = userId1,
                    TeamRole = "Member",
                    IsActive = true
                },
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = team.Id,
                    UserId = userId2,
                    TeamRole = "Admin",
                    IsActive = true
                }
            };

            _context.Users.AddRange(users);
            _context.Teams.Add(team);
            _context.TeamMembers.AddRange(teamMembers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTeamMembersAsync(team.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, member => Assert.True(member.IsActive));
        }

        [Fact]
        public async Task GetTeamMembersAsync_NonExistentTeam_ReturnsEmptyList()
        {
            // Arrange
            var teamId = Guid.NewGuid();

            // Act
            var result = await _repository.GetTeamMembersAsync(teamId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserTeamIdsAsync_ValidUserId_ReturnsTeamIds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var teams = new List<Team>
            {
                new Team { Id = Guid.NewGuid(), Name = "Team 1", IsActive = true },
                new Team { Id = Guid.NewGuid(), Name = "Team 2", IsActive = true }
            };

            var teamMembers = new List<TeamMember>
            {
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = teams[0].Id,
                    UserId = userId,
                    TeamRole = "Member",
                    IsActive = true
                },
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = teams[1].Id,
                    UserId = userId,
                    TeamRole = "Admin",
                    IsActive = true
                }
            };

            _context.Teams.AddRange(teams);
            _context.TeamMembers.AddRange(teamMembers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserTeamIdsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(teams[0].Id, result);
            Assert.Contains(teams[1].Id, result);
        }

        [Fact]
        public async Task GetUserTeamIdsAsync_UserNotInAnyTeam_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetUserTeamIdsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task IsUserTeamMemberAsync_UserIsMember_ReturnsTrue()
        {
            // Arrange
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                IsActive = true
            };

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = Guid.NewGuid(),
                TeamRole = "Member",
                IsActive = true
            };

            _context.Teams.Add(team);
            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsUserTeamMemberAsync(team.Id, teamMember.UserId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUserTeamMemberAsync_UserNotMember_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.IsUserTeamMemberAsync(teamId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserTeamRoleAsync_UserIsMember_ReturnsRole()
        {
            // Arrange
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                IsActive = true
            };

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = Guid.NewGuid(),
                TeamRole = "Admin",
                IsActive = true
            };

            _context.Teams.Add(team);
            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserTeamRoleAsync(team.Id, teamMember.UserId);

            // Assert
            Assert.Equal("Admin", result);
        }

        [Fact]
        public async Task GetUserTeamRoleAsync_UserIsTeamLeader_ReturnsTeamLeader()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                TeamLeaderId = userId,
                IsActive = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserTeamRoleAsync(team.Id, userId);

            // Assert
            Assert.Equal("TeamLeader", result);
        }

        [Fact]
        public async Task GetUserTeamRoleAsync_UserNotMember_ReturnsNull()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetUserTeamRoleAsync(teamId, userId);

            // Assert
            Assert.Null(result);
        }
    }
} 