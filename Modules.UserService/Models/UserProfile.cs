namespace Modules.UserService.Models
{
    /// <summary>
    /// Model representing a user's profile information.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// The user's role in the system.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 