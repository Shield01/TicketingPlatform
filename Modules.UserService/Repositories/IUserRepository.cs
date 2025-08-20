using Modules.UserService.Models;

namespace Modules.UserService.Repositories
{
    /// <summary>
    /// Repository interface for user data access operations.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <returns>The created user with generated ID.</returns>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The user if found, null otherwise.</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>The user if found, null otherwise.</returns>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>
        /// Checks if a user with the given email already exists.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if a user with this email exists, false otherwise.</returns>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>The updated user.</returns>
        Task<User> UpdateAsync(User user);

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>True if the user was deleted, false if not found.</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A list of all users.</returns>
        Task<IEnumerable<User>> GetAllAsync();
    }
} 