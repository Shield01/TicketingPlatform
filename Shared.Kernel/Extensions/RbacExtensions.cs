using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Shared.Kernel.Constants;
using Shared.Kernel.Enums;

namespace Shared.Kernel.Extensions
{
    /// <summary>
    /// Extension methods for Role-Based Access Control (RBAC) functionality.
    /// </summary>
    public static class RbacExtensions
    {
        /// <summary>
        /// Checks if the current user has the specified role.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="role">The role to check.</param>
        /// <returns>True if the user has the role, false otherwise.</returns>
        public static bool IsInRole(this HttpContext context, string role)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return false;

            return context.User.IsInRole(role);
        }

        /// <summary>
        /// Checks if the current user has the specified role using the UserRole enum.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="role">The role to check.</param>
        /// <returns>True if the user has the role, false otherwise.</returns>
        public static bool IsInRole(this HttpContext context, UserRole role)
        {
            return context.IsInRole(role.ToString());
        }

        /// <summary>
        /// Checks if the current user has any of the specified roles.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="roles">The roles to check.</param>
        /// <returns>True if the user has any of the roles, false otherwise.</returns>
        public static bool IsInAnyRole(this HttpContext context, params string[] roles)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return false;

            return roles.Any(role => context.User.IsInRole(role));
        }

        /// <summary>
        /// Checks if the current user has any of the specified roles using the UserRole enum.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="roles">The roles to check.</param>
        /// <returns>True if the user has any of the roles, false otherwise.</returns>
        public static bool IsInAnyRole(this HttpContext context, params UserRole[] roles)
        {
            return context.IsInAnyRole(roles.Select(r => r.ToString()).ToArray());
        }

        /// <summary>
        /// Gets the current user's role from the JWT claims.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The user's role, or null if not found.</returns>
        public static string? GetUserRole(this HttpContext context)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return null;

            return context.User.FindFirst(RbacConstants.Claims.Role)?.Value;
        }

        /// <summary>
        /// Gets the current user's ID from the JWT claims.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The user's ID, or null if not found.</returns>
        public static Guid? GetUserId(this HttpContext context)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = context.User.FindFirst(RbacConstants.Claims.UserId)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }

        /// <summary>
        /// Gets the current user's email from the JWT claims.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The user's email, or null if not found.</returns>
        public static string? GetUserEmail(this HttpContext context)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return null;

            return context.User.FindFirst(RbacConstants.Claims.Email)?.Value;
        }

        /// <summary>
        /// Checks if the current user is an admin.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if the user is an admin, false otherwise.</returns>
        public static bool IsAdmin(this HttpContext context)
        {
            return context.IsInRole(RbacConstants.Roles.Admin);
        }

        /// <summary>
        /// Checks if the current user is an organiser.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if the user is an organiser, false otherwise.</returns>
        public static bool IsOrganiser(this HttpContext context)
        {
            return context.IsInRole(RbacConstants.Roles.Organiser);
        }

        /// <summary>
        /// Checks if the current user is staff.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if the user is staff, false otherwise.</returns>
        public static bool IsStaff(this HttpContext context)
        {
            return context.IsInRole(RbacConstants.Roles.Staff);
        }

        /// <summary>
        /// Checks if the current user is an attendee.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if the user is an attendee, false otherwise.</returns>
        public static bool IsAttendee(this HttpContext context)
        {
            return context.IsInRole(RbacConstants.Roles.Attendee);
        }

        /// <summary>
        /// Checks if the current user can manage events (Admin or Organiser).
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if the user can manage events, false otherwise.</returns>
        public static bool CanManageEvents(this HttpContext context)
        {
            return context.IsInAnyRole(RbacConstants.Roles.Admin, RbacConstants.Roles.Organiser);
        }

        /// <summary>
        /// Checks if the current user can scan tickets (Admin, Organiser, or Staff).
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if the user can scan tickets, false otherwise.</returns>
        public static bool CanScanTickets(this HttpContext context)
        {
            return context.IsInAnyRole(RbacConstants.Roles.Admin, RbacConstants.Roles.Organiser, RbacConstants.Roles.Staff);
        }

        /// <summary>
        /// Checks if the current user can manage users (Admin only).
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>True if the user can manage users, false otherwise.</returns>
        public static bool CanManageUsers(this HttpContext context)
        {
            return context.IsAdmin();
        }
    }
} 