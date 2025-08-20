namespace Shared.Kernel.Enums
{
    /// <summary>
    /// Defines the available user roles in the system for Role-Based Access Control (RBAC).
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Attendee role - Can view events, purchase tickets, and access their own tickets.
        /// </summary>
        Attendee = 1,

        /// <summary>
        /// Staff role - Can assist with event management, ticket scanning, and basic event operations.
        /// </summary>
        Staff = 2,

        /// <summary>
        /// Organiser role - Can create, manage, and delete their own events and tickets.
        /// </summary>
        Organiser = 3,

        /// <summary>
        /// Admin role - Has full system access and can manage all users, events, and system settings.
        /// </summary>
        Admin = 4
    }
} 