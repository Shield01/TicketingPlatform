using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

namespace Modules.UserService.Models
{
    /// <summary>
    /// Model representing a user in the system with authentication capabilities.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user's email address. Must be unique.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's hashed password.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// The user's first name.
        /// </summary>
        [Required]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The user's last name.
        /// </summary>
        [Required]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// The user's role in the system. Defaults to "Attendee".
        /// </summary>
        public string Role { get; set; } = Shared.Kernel.Constants.RbacConstants.DefaultRole;

        /// <summary>
        /// Indicates whether the user's email has been verified.
        /// </summary>
        public bool EmailVerified { get; set; } = false;

        /// <summary>
        /// The date and time when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the user account was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Hashes a plain text password using BCrypt.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>The hashed password.</returns>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// Verifies a plain text password against the stored hash.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="hash">The stored password hash.</param>
        /// <returns>True if the password matches the hash, false otherwise.</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        /// <summary>
        /// Sets the password hash for this user.
        /// </summary>
        /// <param name="password">The plain text password to hash and store.</param>
        public void SetPassword(string password)
        {
            PasswordHash = HashPassword(password);
        }

        /// <summary>
        /// Verifies the provided password against this user's stored hash.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <returns>True if the password matches, false otherwise.</returns>
        public bool VerifyPassword(string password)
        {
            return VerifyPassword(password, PasswordHash);
        }

        /// <summary>
        /// Converts this User to a UserProfile for API responses.
        /// </summary>
        /// <returns>A UserProfile object with the user's public information.</returns>
        public UserProfile ToUserProfile()
        {
            return new UserProfile
            {
                Id = Id,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                Role = Role,
                CreatedAt = CreatedAt
            };
        }
    }
} 