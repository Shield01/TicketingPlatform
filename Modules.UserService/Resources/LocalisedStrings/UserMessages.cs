namespace Modules.UserService.Resources.LocalisedStrings
{
    /// <summary>
    /// User service specific messages and string constants
    /// </summary>
    public static class UserMessages
    {
        // Registration Messages
        public const string RegistrationSuccess = "User registered successfully.";
        public const string RegistrationFailed = "User registration failed.";
        public const string EmailAlreadyExists = "An account with this email already exists.";
        public const string EmailRequired = "Email address is required.";
        public const string PasswordRequired = "Password is required.";
        public const string FirstNameRequired = "First name is required.";
        public const string LastNameRequired = "Last name is required.";
        public const string PasswordTooShort = "Password must be at least 8 characters long.";
        public const string InvalidEmailFormat = "Please provide a valid email address.";

        // Login Messages
        public const string LoginSuccess = "Login successful.";
        public const string LoginFailed = "Login failed. Please check your credentials.";
        public const string UserNotFound = "User not found.";
        public const string InvalidPassword = "Invalid password.";

        // Profile Messages
        public const string ProfileRetrieved = "User profile retrieved successfully.";
        public const string ProfileUpdated = "User profile updated successfully.";
        public const string ProfileNotFound = "User profile not found.";

        // Role Assignment Messages
        public const string RoleAssigned = "Role assigned successfully.";
        public const string RoleAssignmentFailed = "Role assignment failed.";
        public const string InvalidRole = "Invalid role specified.";
        public const string InsufficientPermissions = "Insufficient permissions to assign roles.";

        // Validation Messages
        public const string EmailValidationError = "Please provide a valid email address.";
        public const string PasswordValidationError = "Password must be at least 8 characters and contain at least one uppercase letter, one lowercase letter, and one number.";
        public const string NameValidationError = "Name must contain only letters and spaces.";
        public const string RoleValidationError = "Role must be one of: Admin, Organiser, Staff, Attendee.";

        // Log Messages
        public const string UserRegistrationAttempt = "User registration attempt for email: {0}";
        public const string UserLoginAttempt = "User login attempt for email: {0}";
        public const string UserProfileRetrieved = "User profile retrieved for user ID: {0}";
        public const string RoleAssignmentAttempt = "Role assignment attempt for user ID: {0}, role: {1}";
    }
} 