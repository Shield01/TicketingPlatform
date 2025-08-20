using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.UserService.Models;
using Modules.UserService.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Tests.UserService.Tests
{
    /// <summary>
    /// Unit tests for JwtService functionality.
    /// </summary>
    public class JwtServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<JwtService>> _mockLogger;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<JwtService>>();

            // Setup default JWT configuration
            _mockConfiguration.Setup(x => x["Jwt:SecretKey"]).Returns("YourSuperSecretKeyHereThatShouldBeAtLeast32CharactersLong");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("TicketingPlatform");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("TicketingPlatform");
            _mockConfiguration.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("1440");

            _jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ValidConfiguration_InitializesCorrectly()
        {
            // Arrange & Act
            var jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(jwtService);
        }

        [Fact]
        public void Constructor_MissingSecretKey_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Jwt:SecretKey"]).Returns((string?)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new JwtService(_mockConfiguration.Object, _mockLogger.Object));
        }

        [Fact]
        public void GenerateToken_ValidUser_ReturnsValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // Verify token structure
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            Assert.Equal("TicketingPlatform", jwtToken.Issuer);
            Assert.Equal("TicketingPlatform", jwtToken.Audiences.First());
            Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_ValidUser_ContainsCorrectClaims()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal(userId.ToString(), jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            Assert.Equal("test@example.com", jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value);
            Assert.Equal("Attendee", jwtToken.Claims.FirstOrDefault(c => c.Type == "Role")?.Value);
            
            // Check for name claim - it uses "unique_name" in the JWT
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name");
            Assert.NotNull(nameClaim);
            Assert.Equal("John Doe", nameClaim.Value);
        }

        [Fact]
        public void ValidateToken_ValidToken_ReturnsUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            var token = _jwtService.GenerateToken(user);

            // Act
            var validatedUserId = _jwtService.ValidateToken(token);

            // Assert
            Assert.NotNull(validatedUserId);
            Assert.Equal(userId, validatedUserId);
        }

        [Fact]
        public void ValidateToken_InvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var result = _jwtService.ValidateToken(invalidToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ValidateToken_EmptyToken_ReturnsNull()
        {
            // Arrange
            var emptyToken = "";

            // Act
            var result = _jwtService.ValidateToken(emptyToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ValidateToken_NullToken_ReturnsNull()
        {
            // Arrange
            string? nullToken = null;

            // Act
            var result = _jwtService.ValidateToken(nullToken!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ValidateToken_ExpiredToken_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            // Create a service with very short expiration
            _mockConfiguration.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("0");
            var shortExpirationService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);

            var token = shortExpirationService.GenerateToken(user);

            // Wait a moment to ensure token expires
            Thread.Sleep(100);

            // Act
            var result = shortExpirationService.ValidateToken(token);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetTokenExpirationMinutes_ReturnsConfiguredValue()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("60");

            // Act
            var expirationMinutes = _jwtService.GetTokenExpirationMinutes();

            // Assert
            Assert.Equal(1440, expirationMinutes); // Should return the value from constructor
        }

        [Fact]
        public void GetTokenExpirationMinutes_InvalidConfiguration_ReturnsDefault()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("invalid");
            var jwtService = new JwtService(_mockConfiguration.Object, _mockLogger.Object);

            // Act
            var expirationMinutes = jwtService.GetTokenExpirationMinutes();

            // Assert
            Assert.Equal(1440, expirationMinutes); // Default value
        }

        [Fact]
        public void GenerateToken_DifferentUsers_GeneratesDifferentTokens()
        {
            // Arrange
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Role = "Host"
            };

            // Act
            var token1 = _jwtService.GenerateToken(user1);
            var token2 = _jwtService.GenerateToken(user2);

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateToken_SameUser_GeneratesValidTokens()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            // Act
            var token1 = _jwtService.GenerateToken(user);
            var token2 = _jwtService.GenerateToken(user);

            // Assert
            Assert.NotNull(token1);
            Assert.NotNull(token2);
            Assert.NotEmpty(token1);
            Assert.NotEmpty(token2);
        }
    }
} 