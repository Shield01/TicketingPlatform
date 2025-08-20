using System.ComponentModel.DataAnnotations;

namespace Modules.UserService.DTOs
{
    /// <summary>
    /// Request model for user login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The user's email address.
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's password.
        /// </summary>
        /// <example>SecurePassword123!</example>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
} 