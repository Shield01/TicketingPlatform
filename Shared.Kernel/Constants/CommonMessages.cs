namespace Shared.Kernel.Constants
{
    /// <summary>
    /// Common system-wide messages used across all services
    /// </summary>
    public static class CommonMessages
    {
        // Authentication & Authorization
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string Forbidden = "Access denied. Insufficient permissions.";
        public const string InvalidCredentials = "Invalid email or password.";
        public const string TokenExpired = "Authentication token has expired.";
        public const string TokenInvalid = "Invalid authentication token.";

        // Common API Responses
        public const string Success = "Operation completed successfully.";
        public const string NotFound = "The requested resource was not found.";
        public const string BadRequest = "Invalid request data provided.";
        public const string InternalServerError = "An internal server error occurred.";
        public const string ValidationError = "One or more validation errors occurred.";

        // Common Status Messages
        public const string Created = "Resource created successfully.";
        public const string Updated = "Resource updated successfully.";
        public const string Deleted = "Resource deleted successfully.";
        public const string Retrieved = "Resource retrieved successfully.";

        // Common Error Messages
        public const string DatabaseError = "A database error occurred.";
        public const string NetworkError = "A network error occurred.";
        public const string TimeoutError = "The operation timed out.";
        public const string RateLimitExceeded = "Rate limit exceeded. Please try again later.";

        // Common Validation Messages
        public const string RequiredField = "This field is required.";
        public const string InvalidFormat = "Invalid format provided.";
        public const string InvalidLength = "Invalid length provided.";
        public const string InvalidEmail = "Invalid email format.";
        public const string InvalidPassword = "Password must be at least 8 characters long.";
    }
} 