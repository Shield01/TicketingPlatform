using Modules.UserService.Models;

namespace Modules.UserService.Services
{
    /// <summary>
    /// Service interface for JWT token operations.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>The generated JWT token.</returns>
        string GenerateToken(User user);

        /// <summary>
        /// Validates a JWT token and extracts user information.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>The user ID if token is valid, null otherwise.</returns>
        Guid? ValidateToken(string token);

        /// <summary>
        /// Gets the expiration time for JWT tokens.
        /// </summary>
        /// <returns>The expiration time in minutes.</returns>
        int GetTokenExpirationMinutes();
    }
} 