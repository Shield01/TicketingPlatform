using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.UserService.Models;
using Modules.UserService.Repositories;

namespace Tests.UserService.Tests
{
    /// <summary>
    /// Unit tests for UserRepository functionality.
    /// </summary>
    public class UserRepositoryTests
    {
        private readonly UserDbContext _context;
        private readonly Mock<ILogger<UserRepository>> _mockLogger;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new UserDbContext(options);

            // Setup mocks
            _mockLogger = new Mock<ILogger<UserRepository>>();

            // Setup repository
            _repository = new UserRepository(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidUser_ReturnsCreatedUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };

            // Act
            var result = await _repository.CreateAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FirstName, result.FirstName);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(user.Role, result.Role);
            Assert.True(result.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
            Assert.True(result.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            await _repository.CreateAsync(user);

            // Act
            var result = await _repository.GetByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_NonExistentUser_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            await _repository.CreateAsync(user);

            // Act
            var result = await _repository.GetByEmailAsync("TEST@EXAMPLE.COM");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            var createdUser = await _repository.CreateAsync(user);

            // Act
            var result = await _repository.GetByIdAsync(createdUser.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdUser.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsByEmailAsync_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            await _repository.CreateAsync(user);

            // Act
            var exists = await _repository.ExistsByEmailAsync("test@example.com");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByEmailAsync_NonExistentUser_ReturnsFalse()
        {
            // Act
            var exists = await _repository.ExistsByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByEmailAsync_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            await _repository.CreateAsync(user);

            // Act
            var exists = await _repository.ExistsByEmailAsync("TEST@EXAMPLE.COM");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            var createdUser = await _repository.CreateAsync(user);
            var originalUpdatedAt = createdUser.UpdatedAt;

            // Wait a moment to ensure timestamp difference
            await Task.Delay(10);

            // Act
            createdUser.FirstName = "Jane";
            var result = await _repository.UpdateAsync(createdUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jane", result.FirstName);
            Assert.True(result.UpdatedAt > originalUpdatedAt);
        }

        [Fact]
        public async Task DeleteAsync_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            var createdUser = await _repository.CreateAsync(user);

            // Act
            var deleted = await _repository.DeleteAsync(createdUser.Id);

            // Assert
            Assert.True(deleted);

            // Verify user is no longer in database
            var retrievedUser = await _repository.GetByIdAsync(createdUser.Id);
            Assert.Null(retrievedUser);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentUser_ReturnsFalse()
        {
            // Act
            var deleted = await _repository.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.False(deleted);
        }

        [Fact]
        public async Task GetAllAsync_NoUsers_ReturnsEmptyList()
        {
            // Act
            var users = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(users);
            Assert.Empty(users);
        }

        [Fact]
        public async Task GetAllAsync_MultipleUsers_ReturnsAllUsers()
        {
            // Arrange
            var user1 = new User
            {
                Email = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Attendee"
            };
            var user2 = new User
            {
                Email = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Role = "Host"
            };
            await _repository.CreateAsync(user1);
            await _repository.CreateAsync(user2);

            // Act
            var users = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(users);
            Assert.Equal(2, users.Count());
            Assert.Contains(users, u => u.Email == "user1@example.com");
            Assert.Contains(users, u => u.Email == "user2@example.com");
        }

        private void Dispose()
        {
            _context?.Dispose();
        }
    }
} 