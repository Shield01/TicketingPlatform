namespace Shared.Kernel.Interfaces
{
    /// <summary>
    /// Interface for retrieving user information across modules.
    /// </summary>
    public interface IUserInfoService
    {
        /// <summary>
        /// Gets user profile information by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user profile information if found, null otherwise.</returns>
        Task<UserInfo?> GetUserInfoAsync(Guid userId);
    }

    /// <summary>
    /// User information model for cross-module communication.
    /// </summary>
    public class UserInfo
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
        /// The user's full name.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
