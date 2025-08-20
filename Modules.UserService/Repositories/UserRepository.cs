using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.UserService.Models;

namespace Modules.UserService.Repositories
{
    /// <summary>
    /// In-memory repository implementation for user data access operations.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <returns>The created user with generated ID.</returns>
        public async Task<User> CreateAsync(User user)
        {
            _logger.LogInformation("Creating new user with email: {Email}", user.Email);
            
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
            return user;
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The user if found, null otherwise.</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Retrieving user by email: {Email}", email);
            
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            
            if (user == null)
            {
                _logger.LogWarning("User not found with email: {Email}", email);
            }
            
            return user;
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>The user if found, null otherwise.</returns>
        public async Task<User?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Retrieving user by ID: {UserId}", id);
            
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
            }
            
            return user;
        }

        /// <summary>
        /// Checks if a user with the given email already exists.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if a user with this email exists, false otherwise.</returns>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            _logger.LogInformation("Checking if user exists with email: {Email}", email);
            
            var exists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
            
            _logger.LogInformation("User exists check result: {Exists} for email: {Email}", exists, email);
            return exists;
        }

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>The updated user.</returns>
        public async Task<User> UpdateAsync(User user)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", user.Id);
            
            user.UpdatedAt = DateTime.UtcNow;
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);
            return user;
        }

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>True if the user was deleted, false if not found.</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);
            
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found for deletion with ID: {UserId}", id);
                return false;
            }
            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User deleted successfully with ID: {UserId}", id);
            return true;
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A list of all users.</returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all users");
            
            var users = await _context.Users.ToListAsync();
            
            _logger.LogInformation("Retrieved {Count} users", users.Count);
            return users;
        }
    }
} 