using System.ComponentModel.DataAnnotations;

namespace Modules.UserService.DTOs
{
    /// <summary>
    /// Request model for role assignment.
    /// </summary>
    public class RoleAssignmentRequest
    {
        /// <summary>
        /// The unique identifier of the user to assign the role to.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The role to assign to the user.
        /// </summary>
        /// <example>Admin</example>
        [Required]
        public string Role { get; set; } = string.Empty;
    }
} 