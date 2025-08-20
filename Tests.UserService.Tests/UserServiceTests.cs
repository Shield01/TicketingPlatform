using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.UserService.DTOs;
using Modules.UserService.Models;
using Modules.UserService.Repositories;
using Modules.UserService.Services;
using Modules.UserService.Resources.LocalisedStrings;
using System.ComponentModel.DataAnnotations;
using UserService = Modules.UserService.Services.UserService;

namespace Tests.UserService.Tests
{
    /// <summary>
    /// Unit tests for UserService functionality.
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<ILogger<Modules.UserService.Services.UserService>> _mockLogger;
        private readonly Modules.UserService.Services.UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockLogger = new Mock<ILogger<Modules.UserService.Services.UserService>>();
            _userService = new Modules.UserService.Services.UserService(_mockUserRepository.Object, _mockJwtService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task LoginUserAsync_ValidCredentials_ReturnsLoginResponse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "SecurePassword123!";
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            // Set password hash for the user
            user.SetPassword(password);

            var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
            var expectedExpirationMinutes = 1440;

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mockJwtService.Setup(x => x.GenerateToken(user))
                .Returns(expectedToken);

            _mockJwtService.Setup(x => x.GetTokenExpirationMinutes())
                .Returns(expectedExpirationMinutes);

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(email, result.Email);
            Assert.Equal("Attendee", result.Role);
            Assert.Equal(expectedToken, result.Token);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);

            _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockJwtService.Verify(x => x.GenerateToken(user), Times.Once);
            _mockJwtService.Verify(x => x.GetTokenExpirationMinutes(), Times.Once);
        }

        [Fact]
        public async Task LoginUserAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "SecurePassword123!";

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserAsync_InactiveUser_ReturnsNull()
        {
            // Arrange
            var email = "inactive@example.com";
            var password = "SecurePassword123!";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee",
                IsActive = false
            };

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "WrongPassword";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee",
                IsActive = true
            };

            // Set up password verification to fail
            user.SetPassword("CorrectPassword");

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserAsync_EmptyEmail_ReturnsNull()
        {
            // Arrange
            var email = "";
            var password = "SecurePassword123!";

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserAsync_EmptyPassword_ReturnsNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "";

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserAsync_WhitespaceCredentials_ReturnsNull()
        {
            // Arrange
            var email = "   ";
            var password = "   ";

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserAsync_ValidCredentialsWithDifferentRole_ReturnsCorrectRole()
        {
            // Arrange
            var email = "host@example.com";
            var password = "SecurePassword123!";
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = email,
                FirstName = "Jane",
                LastName = "Smith",
                Role = "Host"
            };

            // Set password hash for the user
            user.SetPassword(password);

            var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
            var expectedExpirationMinutes = 1440;

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mockJwtService.Setup(x => x.GenerateToken(user))
                .Returns(expectedToken);

            _mockJwtService.Setup(x => x.GetTokenExpirationMinutes())
                .Returns(expectedExpirationMinutes);

            // Act
            var result = await _userService.LoginUserAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Host", result.Role);
            Assert.Equal(expectedToken, result.Token);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "SecurePassword123!";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee",
                IsActive = true
            };

            user.SetPassword(password);

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.AuthenticateUserAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task AuthenticateUserAsync_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "WrongPassword";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee",
                IsActive = true
            };

            user.SetPassword("CorrectPassword");

            _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.AuthenticateUserAsync(email, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidRequest_ReturnsRegistrationResponse()
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                Email = "newuser@example.com",
                Password = "SecurePassword123!",
                FirstName = "New",
                LastName = "User"
            };

            var createdUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "Attendee"
            };

            _mockUserRepository.Setup(x => x.ExistsByEmailAsync(request.Email))
                .ReturnsAsync(false);

            _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _userService.RegisterUserAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdUser.Id, result.UserId);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(UserMessages.RegistrationSuccess, result.Message);

            _mockUserRepository.Verify(x => x.ExistsByEmailAsync(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_DuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                Email = "existing@example.com",
                Password = "SecurePassword123!",
                FirstName = "New",
                LastName = "User"
            };

            _mockUserRepository.Setup(x => x.ExistsByEmailAsync(request.Email))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.RegisterUserAsync(request));

            Assert.Equal(UserMessages.EmailAlreadyExists, exception.Message);

            _mockUserRepository.Verify(x => x.ExistsByEmailAsync(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_InvalidRequest_ThrowsValidationException()
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                Email = "invalid-email",
                Password = "",
                FirstName = "",
                LastName = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _userService.RegisterUserAsync(request));

            _mockUserRepository.Verify(x => x.ExistsByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        }
    }
} 