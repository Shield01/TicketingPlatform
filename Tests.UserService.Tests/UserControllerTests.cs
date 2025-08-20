using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.UserService.DTOs;
using Modules.UserService.Models;
using Modules.UserService.Services;
using Modules.UserService.Resources.LocalisedStrings;
using Modules.UserService.Controllers;
using Shared.Kernel.Constants;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Tests.UserService.Tests
{
    /// <summary>
    /// Unit tests for UserController functionality.
    /// </summary>
    public class UserControllerTests
    {
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockLogger.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                Email = "test@example.com",
                Password = "SecurePassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var expectedResponse = new UserRegistrationResponse
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                Message = UserMessages.RegistrationSuccess
            };

            _mockUserService.Setup(x => x.RegisterUserAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(expectedResponse, createdResult.Value);
            Assert.Equal(nameof(UserController.GetProfile), createdResult.ActionName);
            Assert.Equal(expectedResponse.UserId, createdResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Register_ValidationException_ReturnsBadRequest()
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                Email = "invalid-email",
                Password = "short",
                FirstName = "",
                LastName = ""
            };

            _mockUserService.Setup(x => x.RegisterUserAsync(request))
                .ThrowsAsync(new ValidationException("Invalid registration data provided."));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);
            var messageProperty = response.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Invalid registration data provided.", messageProperty.GetValue(response));
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                Email = "duplicate@example.com",
                Password = "SecurePassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockUserService.Setup(x => x.RegisterUserAsync(request))
                .ThrowsAsync(new InvalidOperationException(UserMessages.EmailAlreadyExists));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = conflictResult.Value;
            Assert.NotNull(response);
            var messageProperty = response.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty);
            Assert.Equal(UserMessages.EmailAlreadyExists, messageProperty.GetValue(response));
        }

        [Fact]
        public async Task Register_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                Email = "test@example.com",
                Password = "SecurePassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockUserService.Setup(x => x.RegisterUserAsync(request))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value;
            Assert.NotNull(response);
            var messageProperty = response.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty);
            Assert.Equal(CommonMessages.InternalServerError, messageProperty.GetValue(response));
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "SecurePassword123!"
            };

            var expectedResponse = new LoginResponse
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                Role = "Attendee",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _mockUserService.Setup(x => x.LoginUserAsync(request.Email, request.Password))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal(expectedResponse.UserId, response.UserId);
            Assert.Equal(expectedResponse.Email, response.Email);
            Assert.Equal(expectedResponse.Role, response.Role);
            Assert.Equal(expectedResponse.Token, response.Token);
            Assert.Equal(expectedResponse.ExpiresAt, response.ExpiresAt);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _mockUserService.Setup(x => x.LoginUserAsync(request.Email, request.Password))
                .ReturnsAsync((LoginResponse?)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value;
            Assert.NotNull(response);
            var messageProperty = response.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty);
            Assert.Equal(CommonMessages.InvalidCredentials, messageProperty.GetValue(response));
        }

        [Fact]
        public async Task Login_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "SecurePassword123!"
            };

            _mockUserService.Setup(x => x.LoginUserAsync(request.Email, request.Password))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value;
            Assert.NotNull(response);
            var messageProperty = response.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty);
            Assert.Equal(CommonMessages.InternalServerError, messageProperty.GetValue(response));
        }

        [Fact]
        public async Task GetProfile_ExistingUser_ReturnsOk()
        {
            // Arrange
            var userProfile = new UserProfile
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee",
                CreatedAt = DateTime.UtcNow
            };

            _mockUserService.Setup(x => x.GetUserProfileAsync(It.IsAny<Guid>()))
                .ReturnsAsync(userProfile);

            // Setup authenticated user context
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, userProfile.Id.ToString()),
                new Claim(RbacConstants.Claims.Email, userProfile.Email),
                new Claim(RbacConstants.Claims.Role, userProfile.Role),
                new Claim(ClaimTypes.Role, userProfile.Role)
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UserProfile>(okResult.Value);
            Assert.Equal(userProfile.Id, response.Id);
            Assert.Equal(userProfile.Email, response.Email);
        }

        [Fact]
        public async Task GetProfile_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync((UserProfile?)null);

            // Setup authenticated user context
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, userId.ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, "Attendee"),
                new Claim(ClaimTypes.Role, "Attendee")
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value;
            Assert.NotNull(response);
            var messageProperty = response.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty);
            Assert.Equal(UserMessages.ProfileNotFound, messageProperty.GetValue(response));
        }

        [Fact]
        public async Task GetProfile_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(x => x.GetUserProfileAsync(userId))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Setup authenticated user context
            var claims = new List<Claim>
            {
                new Claim(RbacConstants.Claims.UserId, userId.ToString()),
                new Claim(RbacConstants.Claims.Email, "test@example.com"),
                new Claim(RbacConstants.Claims.Role, "Attendee"),
                new Claim(ClaimTypes.Role, "Attendee")
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value;
            Assert.NotNull(response);
            var messageProperty = response.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty);
            Assert.Equal(CommonMessages.InternalServerError, messageProperty.GetValue(response));
        }
    }
} 