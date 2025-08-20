namespace Shared.Kernel.Constants
{
    /// <summary>
    /// Common API routes used across all services
    /// </summary>
    public static class ApiRoutes
    {
        // Base API prefix
        public const string ApiPrefix = "api";

        // User Service Routes
        public const string Users = "users";
        public const string Register = "register";
        public const string Login = "login";
        public const string Me = "me";
        public const string AssignRole = "assign-role";

        // Event Service Routes
        public const string Events = "events";

        // Ticket Service Routes
        public const string Tickets = "tickets";
        public const string EventTickets = "event";
        public const string Verify = "verify";

        // Payment Service Routes
        public const string Payments = "payments";
        public const string Initiate = "initiate";
        public const string Webhook = "webhook";
        public const string UserHistory = "user-history";

        // Health Check
        public const string Health = "health";
    }
} 