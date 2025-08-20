namespace Modules.UserService.DTOs
{
    /// <summary>
    /// Response model for user login.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// The JWT token for API authentication.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the authenticated user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The email address of the authenticated user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The role of the authenticated user.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The expiration time of the JWT token.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
} 