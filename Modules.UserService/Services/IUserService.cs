using Modules.UserService.DTOs;
using Modules.UserService.Models;

namespace Modules.UserService.Services
{
    /// <summary>
    /// Service interface for user business logic operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The user registration request.</param>
        /// <returns>The registration response with user details.</returns>
        Task<UserRegistrationResponse> RegisterUserAsync(UserRegistrationRequest request);

        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The authenticated user if successful, null otherwise.</returns>
        Task<User?> AuthenticateUserAsync(string email, string password);

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The login response with JWT token if successful, null otherwise.</returns>
        Task<LoginResponse?> LoginUserAsync(string email, string password);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>The user profile if found, null otherwise.</returns>
        Task<UserProfile?> GetUserProfileAsync(Guid id);

        /// <summary>
        /// Checks if a user with the given email already exists.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if a user with this email exists, false otherwise.</returns>
        Task<bool> UserExistsAsync(string email);

        /// <summary>
        /// Assigns a role to a user (Admin only).
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="role">The role to assign.</param>
        /// <returns>True if role was assigned successfully, false otherwise.</returns>
        Task<bool> AssignRoleAsync(Guid userId, string role);
    }
} 