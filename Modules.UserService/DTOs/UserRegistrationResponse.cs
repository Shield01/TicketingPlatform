namespace Modules.UserService.DTOs
{
    /// <summary>
    /// Response model for user registration.
    /// </summary>
    public class UserRegistrationResponse
    {
        /// <summary>
        /// The unique identifier for the newly created user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The email address of the registered user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// A message confirming the registration status.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
} 