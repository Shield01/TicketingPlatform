namespace Shared.Kernel.Constants
{
    /// <summary>
    /// Constants for Role-Based Access Control (RBAC) implementation.
    /// </summary>
    public static class RbacConstants
    {
        /// <summary>
        /// Role names as strings for JWT claims and database storage.
        /// </summary>
        public static class Roles
        {
            public const string Attendee = "Attendee";
            public const string Staff = "Staff";
            public const string Organiser = "Organiser";
            public const string Admin = "Admin";
        }

        /// <summary>
        /// Permission constants for different operations.
        /// </summary>
        public static class Permissions
        {
            // User management permissions
            public const string ViewUsers = "users.view";
            public const string CreateUsers = "users.create";
            public const string UpdateUsers = "users.update";
            public const string DeleteUsers = "users.delete";
            public const string AssignRoles = "users.assign_roles";

            // Event management permissions
            public const string ViewEvents = "events.view";
            public const string CreateEvents = "events.create";
            public const string UpdateEvents = "events.update";
            public const string DeleteEvents = "events.delete";
            public const string ManageOwnEvents = "events.manage_own";

            // Ticket management permissions
            public const string ViewTickets = "tickets.view";
            public const string CreateTickets = "tickets.create";
            public const string UpdateTickets = "tickets.update";
            public const string DeleteTickets = "tickets.delete";
            public const string ScanTickets = "tickets.scan";
            public const string ManageOwnTickets = "tickets.manage_own";

            // Payment management permissions
            public const string ViewPayments = "payments.view";
            public const string ProcessPayments = "payments.process";
            public const string ViewOwnPayments = "payments.view_own";
        }

        /// <summary>
        /// Default role assigned to new users during registration.
        /// </summary>
        public const string DefaultRole = Roles.Admin;

        /// <summary>
        /// JWT claim types for role-based authentication.
        /// </summary>
        public static class Claims
        {
            public const string UserId = "UserId";
            public const string Email = "Email";
            public const string Role = "Role";
            public const string Name = "Name";
        }
    }
} 