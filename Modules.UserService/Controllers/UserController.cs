using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Modules.UserService.DTOs;
using Modules.UserService.Resources.LocalisedStrings;
using Shared.Kernel.Constants;
using Shared.Kernel.Extensions;
using Modules.UserService.Models;
using Modules.UserService.Services;

namespace Modules.UserService.Controllers
{
    /// <summary>
    /// Controller for managing user operations including registration, login, and profile management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    // [SwaggerTag("User management operations including registration, authentication, and profile management")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The user registration request containing email, password, and basic information.</param>
        /// <returns>Registration result with user ID and confirmation message.</returns>
        /// <response code="201">User successfully registered.</response>
        /// <response code="400">Invalid registration data provided.</response>
        /// <response code="409">User with this email already exists.</response>
        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user account with the provided information. Email verification will be sent.",
            OperationId = "RegisterUser",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(201, "User registered successfully", typeof(UserRegistrationResponse))]
        [SwaggerResponse(400, "Invalid registration data")]
        [SwaggerResponse(409, "User already exists")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            _logger.LogInformation(UserMessages.UserRegistrationAttempt, request.Email);
            
            try
            {
                var response = await _userService.RegisterUserAsync(request);
                return CreatedAtAction(nameof(GetProfile), new { id = response.UserId }, response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error during user registration: {Error}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == UserMessages.EmailAlreadyExists)
            {
                _logger.LogWarning("Registration failed: Email already exists: {Email}", request.Email);
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user registration for email: {Email}", request.Email);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="request">The login credentials.</param>
        /// <returns>Authentication result with JWT token and user information.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Authenticate user",
            Description = "Authenticates user credentials and returns a JWT token for API access.",
            OperationId = "LoginUser",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "Login successful", typeof(LoginResponse))]
        [SwaggerResponse(401, "Invalid credentials")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation(UserMessages.UserLoginAttempt, request.Email);
            
            try
            {
                var response = await _userService.LoginUserAsync(request.Email, request.Password);
                if (response == null)
                {
                    return Unauthorized(new { Message = CommonMessages.InvalidCredentials });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user login for email: {Email}", request.Email);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Retrieves the current user's profile information.
        /// </summary>
        /// <returns>User profile information.</returns>
        /// <response code="200">User profile retrieved successfully.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpGet("me")]
        [Authorize(Policy = "AuthenticatedUser")]
        [SwaggerOperation(
            Summary = "Get current user profile",
            Description = "Retrieves the profile information of the currently authenticated user.",
            OperationId = "GetCurrentUserProfile",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "User profile retrieved", typeof(UserProfile))]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<IActionResult> GetProfile()
        {
            // Get user ID from JWT token
            var userId = HttpContext.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Message = "Invalid or missing authentication token" });
            }
            
            try
            {
                var profile = await _userService.GetUserProfileAsync(userId.Value);
                if (profile == null)
                {
                    return NotFound(new { Message = UserMessages.ProfileNotFound });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user profile for ID: {UserId}", userId);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Assigns a role to a user (Admin only).
        /// </summary>
        /// <param name="request">The role assignment request.</param>
        /// <returns>Role assignment result.</returns>
        /// <response code="200">Role assigned successfully.</response>
        /// <response code="400">Invalid role assignment data.</response>
        /// <response code="403">Insufficient permissions.</response>
        /// <response code="404">User not found.</response>
        [HttpPost("assign-role")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Assign role to user",
            Description = "Assigns a specific role to a user. This operation requires admin privileges.",
            OperationId = "AssignUserRole",
            Tags = new[] { "Users" }
        )]
        [SwaggerResponse(200, "Role assigned successfully")]
        [SwaggerResponse(400, "Invalid role assignment data")]
        [SwaggerResponse(403, "Insufficient permissions")]
        [SwaggerResponse(404, "User not found")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentRequest request)
        {
            _logger.LogInformation("Role assignment attempt for user {UserId} to role {Role}", request.UserId, request.Role);
            
            try
            {
                // Validate request
                if (request == null || request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Role))
                {
                    return BadRequest(new { Message = "Invalid role assignment request" });
                }

                // Validate role
                var validRoles = new[] { 
                    Shared.Kernel.Constants.RbacConstants.Roles.Admin,
                    Shared.Kernel.Constants.RbacConstants.Roles.Organiser,
                    Shared.Kernel.Constants.RbacConstants.Roles.Staff,
                    Shared.Kernel.Constants.RbacConstants.Roles.Attendee
                };

                if (!validRoles.Contains(request.Role))
                {
                    return BadRequest(new { Message = "Invalid role specified" });
                }

                // Assign role
                var success = await _userService.AssignRoleAsync(request.UserId, request.Role);
                if (!success)
                {
                    return NotFound(new { Message = "User not found or role assignment failed" });
                }

                return Ok(new { Message = "Role assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during role assignment for user {UserId}", request.UserId);
                return StatusCode(500, new { Message = CommonMessages.InternalServerError });
            }
        }
    }
} 