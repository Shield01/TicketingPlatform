using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.UserService.Services;
using Modules.UserService.Repositories;
using Modules.UserService.Models;
using Shared.Kernel.Constants;
using Shared.Kernel.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using System.Security.Principal;

namespace Tests.UserService.Tests
{
    /// <summary>
    /// Unit tests for Role-Based Access Control (RBAC) functionality.
    /// </summary>
    public class RbacTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<ILogger<Modules.UserService.Services.UserService>> _mockLogger;
        private readonly Modules.UserService.Services.UserService _userService;

        public RbacTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockLogger = new Mock<ILogger<Modules.UserService.Services.UserService>>();
            _userService = new Modules.UserService.Services.UserService(_mockUserRepository.Object, _mockJwtService.Object, _mockLogger.Object);
        }

        [Fact]
        public void DefaultRole_ShouldBeAttendee()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            // Assert
            Assert.Equal(RbacConstants.DefaultRole, user.Role);
        }

        [Fact]
        public void RegisterUser_ShouldAssignDefaultRole()
        {
            // Arrange
            var request = new Modules.UserService.DTOs.UserRegistrationRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            _mockUserRepository.Setup(r => r.ExistsByEmailAsync(request.Email))
                .ReturnsAsync(false);

            _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => user);

            // Act
            var result = _userService.RegisterUserAsync(request).Result;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Email, result.Email);
            _mockUserRepository.Verify(r => r.CreateAsync(It.Is<User>(u => u.Role == RbacConstants.DefaultRole)), Times.Once);
        }

        [Theory]
        [InlineData(RbacConstants.Roles.Admin)]
        [InlineData(RbacConstants.Roles.Organiser)]
        [InlineData(RbacConstants.Roles.Staff)]
        [InlineData(RbacConstants.Roles.Attendee)]
        public async Task AssignRoleAsync_WithValidRole_ShouldSucceed(string role)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = RbacConstants.DefaultRole
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _userService.AssignRoleAsync(userId, role);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Role == role)), Times.Once);
        }

        [Fact]
        public async Task AssignRoleAsync_WithInvalidRole_ShouldFail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidRole = "InvalidRole";

            // Act
            var result = await _userService.AssignRoleAsync(userId, invalidRole);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleAsync_WithNonExistentUser_ShouldFail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = RbacConstants.Roles.Admin;

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.AssignRoleAsync(userId, role);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void JwtToken_ShouldIncludeRoleClaim()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = RbacConstants.Roles.Admin
            };

            var mockJwtService = new Mock<IJwtService>();
            mockJwtService.Setup(j => j.GenerateToken(It.IsAny<User>()))
                .Returns("mock.jwt.token");

            // Act
            var token = mockJwtService.Object.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            mockJwtService.Verify(j => j.GenerateToken(It.Is<User>(u => u.Role == RbacConstants.Roles.Admin)), Times.Once);
        }

        [Theory]
        [InlineData(RbacConstants.Roles.Admin)]
        [InlineData(RbacConstants.Roles.Organiser)]
        [InlineData(RbacConstants.Roles.Staff)]
        [InlineData(RbacConstants.Roles.Attendee)]
        public void HttpContextExtensions_IsInRole_ShouldWork(string role)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, role),
                new Claim(ClaimTypes.Role, role) // Add standard role claim for IsInRole to work
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.IsInRole(role);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HttpContextExtensions_IsInRole_WithWrongRole_ShouldReturnFalse()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Attendee),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Attendee) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.IsInRole(RbacConstants.Roles.Admin);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HttpContextExtensions_IsInAnyRole_ShouldWork()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Organiser),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Organiser) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.IsInAnyRole(RbacConstants.Roles.Admin, RbacConstants.Roles.Organiser);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HttpContextExtensions_GetUserRole_ShouldReturnCorrectRole()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var expectedRole = RbacConstants.Roles.Admin;
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, expectedRole),
                new Claim(ClaimTypes.Role, expectedRole) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.GetUserRole();

            // Assert
            Assert.Equal(expectedRole, result);
        }

        [Fact]
        public void HttpContextExtensions_GetUserId_ShouldReturnCorrectId()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var expectedUserId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, expectedUserId.ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Admin),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Admin) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.GetUserId();

            // Assert
            Assert.Equal(expectedUserId, result);
        }

        [Fact]
        public void HttpContextExtensions_IsAdmin_ShouldReturnTrueForAdmin()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Admin),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Admin) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.IsAdmin();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HttpContextExtensions_IsAdmin_ShouldReturnFalseForNonAdmin()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Attendee),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Attendee) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.IsAdmin();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HttpContextExtensions_CanManageEvents_ShouldReturnTrueForAdminAndOrganiser()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Organiser),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Organiser) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.CanManageEvents();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HttpContextExtensions_CanManageEvents_ShouldReturnFalseForAttendee()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Attendee),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Attendee) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.CanManageEvents();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HttpContextExtensions_CanScanTickets_ShouldReturnTrueForStaffAndHigher()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Staff),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Staff) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.CanScanTickets();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HttpContextExtensions_CanScanTickets_ShouldReturnFalseForAttendee()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Attendee),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Attendee) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.CanScanTickets();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HttpContextExtensions_CanManageUsers_ShouldReturnTrueOnlyForAdmin()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, RbacConstants.Roles.Admin),
                new Claim(ClaimTypes.Role, RbacConstants.Roles.Admin) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.CanManageUsers();

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(RbacConstants.Roles.Organiser)]
        [InlineData(RbacConstants.Roles.Staff)]
        [InlineData(RbacConstants.Roles.Attendee)]
        public void HttpContextExtensions_CanManageUsers_ShouldReturnFalseForNonAdmin(string role)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, Guid.NewGuid().ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, role),
                new Claim(ClaimTypes.Role, role) // Add standard role claim
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = httpContext.CanManageUsers();

            // Assert
            Assert.False(result);
        }
    }
} 