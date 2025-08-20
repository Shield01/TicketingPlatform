using System.ComponentModel.DataAnnotations;

namespace Modules.UserService.DTOs
{
    /// <summary>
    /// Request model for user registration.
    /// </summary>
    public class UserRegistrationRequest
    {
        /// <summary>
        /// The user's email address. Must be unique.
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's password. Must be at least 8 characters long.
        /// </summary>
        /// <example>SecurePassword123!</example>
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The user's first name.
        /// </summary>
        /// <example>John</example>
        [Required]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The user's last name.
        /// </summary>
        /// <example>Doe</example>
        [Required]
        public string LastName { get; set; } = string.Empty;
    }
} 