using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.TeamService.Data;
using Modules.TeamService.Models;
using Modules.TeamService.Repositories;
using Modules.TeamService.Services;
using Modules.TeamService.DTOs;
using Modules.UserService.Models;
using Modules.UserService.Repositories;
using Shared.Kernel.Constants;
using Xunit;
using TeamService = Modules.TeamService.Services.TeamService;

namespace Tests.TeamService.Tests
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> _mockTeamRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<Modules.TeamService.Services.TeamService>> _mockLogger;
        private readonly Modules.TeamService.Services.TeamService _teamService;

        public TeamServiceTests()
        {
            _mockTeamRepository = new Mock<ITeamRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<Modules.TeamService.Services.TeamService>>();
            _teamService = new Modules.TeamService.Services.TeamService(_mockTeamRepository.Object, _mockUserRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateTeamAsync_ValidRequest_ReturnsTeamResponse()
        {
            // Arrange
            var request = new CreateTeamRequest
            {
                Name = "Test Team",
                Description = "Test Description",
                TeamLeaderId = Guid.NewGuid()
            };

            var user = new User
            {
                Id = request.TeamLeaderId,
                Role = RbacConstants.Roles.Organiser
            };

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                TeamLeaderId = request.TeamLeaderId,
                TeamLeader = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(request.TeamLeaderId))
                .ReturnsAsync(user);
            _mockTeamRepository.Setup(x => x.CreateTeamAsync(It.IsAny<Team>()))
                .ReturnsAsync(team);

            // Act
            var result = await _teamService.CreateTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(team.Id, result.Id);
            Assert.Equal(team.Name, result.Name);
            Assert.Equal(team.Description, result.Description);
            Assert.Equal(team.TeamLeaderId, result.TeamLeaderId);
            Assert.Equal(user.Email, result.TeamLeaderEmail);
            Assert.Equal(user.FirstName + " " + user.LastName, result.TeamLeaderName);
        }

        [Fact]
        public async Task CreateTeamAsync_UserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateTeamRequest
            {
                Name = "Test Team",
                Description = "Test Description",
                TeamLeaderId = Guid.NewGuid()
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(request.TeamLeaderId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _teamService.CreateTeamAsync(request));
        }

        [Fact]
        public async Task CreateTeamAsync_UserNotOrganiserOrAdmin_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateTeamRequest
            {
                Name = "Test Team",
                Description = "Test Description",
                TeamLeaderId = Guid.NewGuid()
            };

            var user = new User
            {
                Id = request.TeamLeaderId,
                Role = RbacConstants.Roles.Attendee
            };

            _mockUserRepository.Setup(x => x.GetByIdAsync(request.TeamLeaderId))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _teamService.CreateTeamAsync(request));
        }

        [Fact]
        public async Task GetTeamByIdAsync_ValidId_ReturnsTeamResponse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "leader@test.com",
                FirstName = "John",
                LastName = "Doe",
                Role = RbacConstants.Roles.Organiser
            };

            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                Description = "Test Description",
                TeamLeaderId = user.Id,
                TeamLeader = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);

            // Act
            var result = await _teamService.GetTeamByIdAsync(teamId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(team.Id, result.Id);
            Assert.Equal(team.Name, result.Name);
            Assert.Equal(team.Description, result.Description);
            Assert.Equal(team.TeamLeaderId, result.TeamLeaderId);
            Assert.Equal(user.Email, result.TeamLeaderEmail);
            Assert.Equal(user.FirstName + " " + user.LastName, result.TeamLeaderName);
        }

        [Fact]
        public async Task GetTeamByIdAsync_TeamNotFound_ReturnsNull()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.GetTeamByIdAsync(teamId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTeamByIdAsync_InactiveTeam_ReturnsTeamResponse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsActive = false
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);

            // Act
            var result = await _teamService.GetTeamByIdAsync(teamId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teamId, result.Id);
            Assert.False(result.IsActive);
        }

        [Fact]
        public async Task GetUserTeamsAsync_ValidUserId_ReturnsTeams()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var teams = new List<Team>
            {
                new Team
                {
                    Id = Guid.NewGuid(),
                    Name = "Team 1",
                    Description = "Description 1",
                    TeamLeaderId = Guid.NewGuid(),
                    TeamLeader = new User { Email = "leader1@test.com", FirstName = "John", LastName = "Doe" },
                    IsActive = true
                },
                new Team
                {
                    Id = Guid.NewGuid(),
                    Name = "Team 2",
                    Description = "Description 2",
                    TeamLeaderId = Guid.NewGuid(),
                    TeamLeader = new User { Email = "leader2@test.com", FirstName = "Jane", LastName = "Smith" },
                    IsActive = true
                }
            };

            _mockTeamRepository.Setup(x => x.GetTeamsByUserIdAsync(userId))
                .ReturnsAsync(teams);

            // Act
            var result = await _teamService.GetUserTeamsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateTeamAsync_ValidRequest_ReturnsUpdatedTeamResponse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "leader@test.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var existingTeam = new Team
            {
                Id = teamId,
                Name = "Old Name",
                Description = "Old Description",
                TeamLeaderId = user.Id,
                TeamLeader = user,
                IsActive = true
            };

            var updatedTeam = new Team
            {
                Id = teamId,
                Name = "New Name",
                Description = "New Description",
                TeamLeaderId = user.Id,
                TeamLeader = user,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var updateRequest = new CreateTeamRequest
            {
                Name = "New Name",
                Description = "New Description",
                TeamLeaderId = user.Id
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(existingTeam);
            _mockTeamRepository.Setup(x => x.UpdateTeamAsync(It.IsAny<Team>()))
                .ReturnsAsync(updatedTeam);

            // Act
            var result = await _teamService.UpdateTeamAsync(teamId, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedTeam.Id, result.Id);
            Assert.Equal("New Name", result.Name);
            Assert.Equal("New Description", result.Description);
        }

        [Fact]
        public async Task UpdateTeamAsync_TeamNotFound_ReturnsNull()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var updateRequest = new CreateTeamRequest
            {
                Name = "New Name",
                Description = "New Description",
                TeamLeaderId = Guid.NewGuid()
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.UpdateTeamAsync(teamId, updateRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTeamAsync_InactiveTeam_ThrowsInvalidOperationException()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsActive = false
            };

            var updateRequest = new CreateTeamRequest
            {
                Name = "New Name",
                Description = "New Description",
                TeamLeaderId = Guid.NewGuid()
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);
            _mockUserRepository.Setup(x => x.GetByIdAsync(updateRequest.TeamLeaderId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _teamService.UpdateTeamAsync(teamId, updateRequest));
        }

        [Fact]
        public async Task DeleteTeamAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsActive = true
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);
            _mockTeamRepository.Setup(x => x.DeleteTeamAsync(teamId))
                .ReturnsAsync(true);

            // Act
            var result = await _teamService.DeleteTeamAsync(teamId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteTeamAsync_TeamNotFound_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.DeleteTeamAsync(teamId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTeamAsync_InactiveTeam_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsActive = false
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);

            // Act
            var result = await _teamService.DeleteTeamAsync(teamId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddTeamMemberAsync_ValidRequest_ReturnsTeamMemberResponse()
        {
            // Arrange
            var request = new AddTeamMemberRequest
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TeamRole = "Staff"
            };

            var team = new Team
            {
                Id = request.TeamId,
                Name = "Test Team",
                IsActive = true
            };

            var user = new User
            {
                Id = request.UserId,
                Email = "member@test.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = request.TeamId,
                UserId = request.UserId,
                TeamRole = request.TeamRole,
                Team = team,
                User = user,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(request.TeamId))
                .ReturnsAsync(team);
            _mockUserRepository.Setup(x => x.GetByIdAsync(request.UserId))
                .ReturnsAsync(user);
            _mockTeamRepository.Setup(x => x.IsUserTeamMemberAsync(request.TeamId, request.UserId))
                .ReturnsAsync(false);
            _mockTeamRepository.Setup(x => x.AddTeamMemberAsync(It.IsAny<TeamMember>()))
                .ReturnsAsync(teamMember);

            // Act
            var result = await _teamService.AddTeamMemberAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teamMember.Id, result.Id);
            Assert.Equal(request.TeamId, result.TeamId);
            Assert.Equal(request.UserId, result.UserId);
            Assert.Equal(request.TeamRole, result.TeamRole);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FirstName + " " + user.LastName, result.FirstName + " " + result.LastName);
        }

        [Fact]
        public async Task AddTeamMemberAsync_TeamNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new AddTeamMemberRequest
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TeamRole = "Staff"
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(request.TeamId))
                .ReturnsAsync((Team?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _teamService.AddTeamMemberAsync(request));
        }

        [Fact]
        public async Task AddTeamMemberAsync_UserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new AddTeamMemberRequest
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TeamRole = "Staff"
            };

            var team = new Team
            {
                Id = request.TeamId,
                Name = "Test Team",
                IsActive = true
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(request.TeamId))
                .ReturnsAsync(team);
            _mockUserRepository.Setup(x => x.GetByIdAsync(request.UserId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _teamService.AddTeamMemberAsync(request));
        }

        [Fact]
        public async Task AddTeamMemberAsync_UserAlreadyMember_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new AddTeamMemberRequest
            {
                TeamId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TeamRole = "Staff"
            };

            var team = new Team
            {
                Id = request.TeamId,
                Name = "Test Team",
                IsActive = true
            };

            var user = new User
            {
                Id = request.UserId,
                Email = "member@test.com"
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(request.TeamId))
                .ReturnsAsync(team);
            _mockUserRepository.Setup(x => x.GetByIdAsync(request.UserId))
                .ReturnsAsync(user);
            _mockTeamRepository.Setup(x => x.IsUserTeamMemberAsync(request.TeamId, request.UserId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _teamService.AddTeamMemberAsync(request));
        }

        [Fact]
        public async Task RemoveTeamMemberAsync_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsActive = true
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);
            _mockTeamRepository.Setup(x => x.IsUserTeamMemberAsync(teamId, userId))
                .ReturnsAsync(true);
            _mockTeamRepository.Setup(x => x.RemoveTeamMemberAsync(teamId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _teamService.RemoveTeamMemberAsync(teamId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RemoveTeamMemberAsync_TeamNotFound_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.RemoveTeamMemberAsync(teamId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveTeamMemberAsync_UserNotMember_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsActive = true
            };

            _mockTeamRepository.Setup(x => x.GetTeamByIdAsync(teamId))
                .ReturnsAsync(team);
            _mockTeamRepository.Setup(x => x.IsUserTeamMemberAsync(teamId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _teamService.RemoveTeamMemberAsync(teamId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetTeamMembersAsync_ValidTeamId_ReturnsTeamMembers()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var teamMembers = new List<TeamMember>
            {
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamId,
                    UserId = Guid.NewGuid(),
                    TeamRole = "Member",
                    User = new User { Email = "member1@test.com", FirstName = "John", LastName = "Doe" },
                    IsActive = true
                },
                new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamId,
                    UserId = Guid.NewGuid(),
                    TeamRole = "Admin",
                    User = new User { Email = "member2@test.com", FirstName = "Jane", LastName = "Smith" },
                    IsActive = true
                }
            };

            _mockTeamRepository.Setup(x => x.GetTeamMembersAsync(teamId))
                .ReturnsAsync(teamMembers);

            // Act
            var result = await _teamService.GetTeamMembersAsync(teamId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUserTeamIdsAsync_ValidUserId_ReturnsTeamIds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var teamIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            _mockTeamRepository.Setup(x => x.GetUserTeamIdsAsync(userId))
                .ReturnsAsync(teamIds);

            // Act
            var result = await _teamService.GetUserTeamIdsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(teamIds, result);
        }

        [Fact]
        public async Task IsUserTeamMemberAsync_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockTeamRepository.Setup(x => x.IsUserTeamMemberAsync(teamId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _teamService.IsUserTeamMemberAsync(teamId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUserTeamMemberAsync_UserNotMember_ReturnsFalse()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockTeamRepository.Setup(x => x.IsUserTeamMemberAsync(teamId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _teamService.IsUserTeamMemberAsync(teamId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserTeamRoleAsync_ValidRequest_ReturnsRole()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var role = "Admin";

            _mockTeamRepository.Setup(x => x.GetUserTeamRoleAsync(teamId, userId))
                .ReturnsAsync(role);

            // Act
            var result = await _teamService.GetUserTeamRoleAsync(teamId, userId);

            // Assert
            Assert.Equal(role, result);
        }

        [Fact]
        public async Task GetUserTeamRoleAsync_UserNotMember_ReturnsNull()
        {
            // Arrange
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockTeamRepository.Setup(x => x.GetUserTeamRoleAsync(teamId, userId))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _teamService.GetUserTeamRoleAsync(teamId, userId);

            // Assert
            Assert.Null(result);
        }
    }
} 