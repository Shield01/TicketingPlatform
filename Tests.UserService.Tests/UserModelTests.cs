using Modules.UserService.Models;

namespace Tests.UserService.Tests
{
    /// <summary>
    /// Unit tests for User model functionality.
    /// </summary>
    public class UserModelTests
    {
        [Fact]
        public void HashPassword_ValidPassword_ReturnsHashedPassword()
        {
            // Arrange
            var password = "SecurePassword123!";

            // Act
            var hashedPassword = User.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
            Assert.True(hashedPassword.Length > 20); // BCrypt hashes are typically 60+ characters
        }

        [Fact]
        public void HashPassword_DifferentPasswords_ReturnsDifferentHashes()
        {
            // Arrange
            var password1 = "Password123!";
            var password2 = "Password456!";

            // Act
            var hash1 = User.HashPassword(password1);
            var hash2 = User.HashPassword(password2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashPassword_SamePassword_ReturnsDifferentHashes()
        {
            // Arrange
            var password = "SamePassword123!";

            // Act
            var hash1 = User.HashPassword(password);
            var hash2 = User.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // BCrypt generates different salts each time
        }

        [Fact]
        public void VerifyPassword_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var password = "SecurePassword123!";
            var hashedPassword = User.HashPassword(password);

            // Act
            var isValid = User.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void VerifyPassword_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "SecurePassword123!";
            var wrongPassword = "WrongPassword123!";
            var hashedPassword = User.HashPassword(correctPassword);

            // Act
            var isValid = User.VerifyPassword(wrongPassword, hashedPassword);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void SetPassword_ValidPassword_SetsHashedPassword()
        {
            // Arrange
            var user = new User();
            var password = "SecurePassword123!";

            // Act
            user.SetPassword(password);

            // Assert
            Assert.NotNull(user.PasswordHash);
            Assert.NotEqual(password, user.PasswordHash);
            Assert.True(user.VerifyPassword(password));
        }

        [Fact]
        public void VerifyPassword_InstanceMethod_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var user = new User();
            var password = "SecurePassword123!";
            user.SetPassword(password);

            // Act
            var isValid = user.VerifyPassword(password);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void VerifyPassword_InstanceMethod_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var user = new User();
            var correctPassword = "SecurePassword123!";
            var wrongPassword = "WrongPassword123!";
            user.SetPassword(correctPassword);

            // Act
            var isValid = user.VerifyPassword(wrongPassword);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ToUserProfile_ValidUser_ReturnsCorrectProfile()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var profile = user.ToUserProfile();

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(user.Id, profile.Id);
            Assert.Equal(user.Email, profile.Email);
            Assert.Equal(user.FirstName, profile.FirstName);
            Assert.Equal(user.LastName, profile.LastName);
            Assert.Equal(user.Role, profile.Role);
            Assert.Equal(user.CreatedAt, profile.CreatedAt);
        }

        [Fact]
        public void User_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Equal(Guid.Empty, user.Id);
            Assert.Equal(string.Empty, user.Email);
            Assert.Equal(string.Empty, user.PasswordHash);
            Assert.Equal(string.Empty, user.FirstName);
            Assert.Equal(string.Empty, user.LastName);
            Assert.Equal("Admin", user.Role);
            Assert.False(user.EmailVerified);
            Assert.True(user.IsActive);
            Assert.True(user.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
            Assert.True(user.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void User_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var role = "Admin";
            var createdAt = DateTime.UtcNow.AddDays(-1);
            var updatedAt = DateTime.UtcNow;

            // Act
            var user = new User
            {
                Id = userId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                EmailVerified = true,
                IsActive = false,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            Assert.Equal(userId, user.Id);
            Assert.Equal(email, user.Email);
            Assert.Equal(firstName, user.FirstName);
            Assert.Equal(lastName, user.LastName);
            Assert.Equal(role, user.Role);
            Assert.True(user.EmailVerified);
            Assert.False(user.IsActive);
            Assert.Equal(createdAt, user.CreatedAt);
            Assert.Equal(updatedAt, user.UpdatedAt);
        }
    }
} 