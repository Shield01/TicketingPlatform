using Shared.Kernel.Interfaces;
using Modules.UserService.Models;

namespace Modules.UserService.Services
{
    /// <summary>
    /// Implementation of IUserInfoService for cross-module user information access.
    /// </summary>
    public class UserInfoService : IUserInfoService
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the UserInfoService.
        /// </summary>
        /// <param name="userService">The user service.</param>
        public UserInfoService(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets user profile information by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user profile information if found, null otherwise.</returns>
        public async Task<UserInfo?> GetUserInfoAsync(Guid userId)
        {
            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                return null;
            }

            return new UserInfo
            {
                Id = userProfile.Id,
                Email = userProfile.Email,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName
            };
        }
    }
}
