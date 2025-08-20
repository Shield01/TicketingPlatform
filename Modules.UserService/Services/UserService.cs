using Microsoft.Extensions.Logging;
using Modules.UserService.DTOs;
using Modules.UserService.Models;
using Modules.UserService.Repositories;
using Modules.UserService.Resources.LocalisedStrings;
using System.ComponentModel.DataAnnotations;

namespace Modules.UserService.Services
{
    /// <summary>
    /// Service implementation for user business logic operations.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IJwtService jwtService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The user registration request.</param>
        /// <returns>The registration response with user details.</returns>
        public async Task<UserRegistrationResponse> RegisterUserAsync(UserRegistrationRequest request)
        {
            _logger.LogInformation("Starting user registration for email: {Email}", request.Email);

            // Validate request
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, context, validationResults, true))
            {
                _logger.LogWarning("Validation failed for user registration: {Errors}", 
                    string.Join(", ", validationResults.Select(v => v.ErrorMessage)));
                throw new ValidationException("Invalid registration data provided.");
            }

            // Check if user already exists
            var userExists = await _userRepository.ExistsByEmailAsync(request.Email);
            if (userExists)
            {
                _logger.LogWarning("Registration failed: Email already exists: {Email}", request.Email);
                throw new InvalidOperationException(UserMessages.EmailAlreadyExists);
            }

            // Create new user
            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Role = Shared.Kernel.Constants.RbacConstants.DefaultRole, // Default role
                EmailVerified = false,
                IsActive = true
            };

            // Hash password
            user.SetPassword(request.Password);

            // Save to database
            var createdUser = await _userRepository.CreateAsync(user);

            _logger.LogInformation("User registered successfully with ID: {UserId}", createdUser.Id);

            return new UserRegistrationResponse
            {
                UserId = createdUser.Id,
                Email = createdUser.Email,
                Message = UserMessages.RegistrationSuccess
            };
        }

        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The authenticated user if successful, null otherwise.</returns>
        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            _logger.LogInformation("Attempting authentication for email: {Email}", email);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Authentication failed: Empty email or password");
                return null;
            }

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for email: {Email}", email);
                return null;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed: Inactive user for email: {Email}", email);
                return null;
            }

            // Verify password
            if (!user.VerifyPassword(password))
            {
                _logger.LogWarning("Authentication failed: Invalid password for email: {Email}", email);
                return null;
            }

            _logger.LogInformation("Authentication successful for user ID: {UserId}", user.Id);
            return user;
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>The user profile if found, null otherwise.</returns>
        public async Task<UserProfile?> GetUserProfileAsync(Guid id)
        {
            _logger.LogInformation("Retrieving user profile for ID: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User profile not found for ID: {UserId}", id);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("User profile not available for inactive user ID: {UserId}", id);
                return null;
            }

            _logger.LogInformation("User profile retrieved successfully for ID: {UserId}", id);
            return user.ToUserProfile();
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The login response with JWT token if successful, null otherwise.</returns>
        public async Task<LoginResponse?> LoginUserAsync(string email, string password)
        {
            _logger.LogInformation("Attempting login with JWT token generation for email: {Email}", email);

            var user = await AuthenticateUserAsync(email, password);
            if (user == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for email: {Email}", email);
                return null;
            }

            var token = _jwtService.GenerateToken(user);
            var expirationMinutes = _jwtService.GetTokenExpirationMinutes();

            _logger.LogInformation("Login successful with JWT token generated for user ID: {UserId}", user.Id);

            return new LoginResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }

        /// <summary>
        /// Checks if a user with the given email already exists.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if a user with this email exists, false otherwise.</returns>
        public async Task<bool> UserExistsAsync(string email)
        {
            _logger.LogInformation("Checking if user exists with email: {Email}", email);

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("User exists check failed: Empty email provided");
                return false;
            }

            var exists = await _userRepository.ExistsByEmailAsync(email);
            _logger.LogInformation("User exists check result: {Exists} for email: {Email}", exists, email);
            return exists;
        }

        /// <summary>
        /// Assigns a role to a user (Admin only).
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="role">The role to assign.</param>
        /// <returns>True if role was assigned successfully, false otherwise.</returns>
        public async Task<bool> AssignRoleAsync(Guid userId, string role)
        {
            _logger.LogInformation("Attempting to assign role {Role} to user {UserId}", role, userId);

            // Validate role
            var validRoles = new[] { 
                Shared.Kernel.Constants.RbacConstants.Roles.Admin,
                Shared.Kernel.Constants.RbacConstants.Roles.Organiser,
                Shared.Kernel.Constants.RbacConstants.Roles.Staff,
                Shared.Kernel.Constants.RbacConstants.Roles.Attendee
            };

            if (!validRoles.Contains(role))
            {
                _logger.LogWarning("Role assignment failed: Invalid role {Role}", role);
                return false;
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Role assignment failed: User not found for ID {UserId}", userId);
                return false;
            }

            // Update user role
            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;

            // Save changes
            var updatedUser = await _userRepository.UpdateAsync(user);
            if (updatedUser == null)
            {
                _logger.LogError("Role assignment failed: Could not update user {UserId}", userId);
                return false;
            }

            _logger.LogInformation("Role {Role} assigned successfully to user {UserId}", role, userId);
            return true;
        }
    }
} 