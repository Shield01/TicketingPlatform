using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Shared.Kernel.Constants;

namespace Shared.Kernel.Middlewares
{
    /// <summary>
    /// Middleware for JWT authentication that extracts and validates JWT tokens.
    /// </summary>
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        /// <summary>
        /// Initializes a new instance of the JwtAuthenticationMiddleware class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            
            // Get JWT configuration from appsettings
            _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _issuer = configuration["Jwt:Issuer"] ?? "TicketingPlatform";
            _audience = configuration["Jwt:Audience"] ?? "TicketingPlatform";
        }

        /// <summary>
        /// Processes the HTTP request and validates JWT tokens.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = ExtractTokenFromHeader(context);
                
                if (!string.IsNullOrEmpty(token))
                {
                    var principal = ValidateToken(token);
                    if (principal != null)
                    {
                        context.User = principal;
                        _logger.LogDebug("JWT token validated successfully for user: {UserId}", 
                            principal.FindFirst(RbacConstants.Claims.UserId)?.Value);
                    }
                    else
                    {
                        _logger.LogWarning("JWT token validation failed");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during JWT authentication");
            }

            await _next(context);
        }

        /// <summary>
        /// Extracts the JWT token from the Authorization header.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The JWT token if found, null otherwise.</returns>
        private string? ExtractTokenFromHeader(HttpContext context)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorizationHeader.Substring("Bearer ".Length).Trim();
        }

        /// <summary>
        /// Validates a JWT token and returns the claims principal.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>The claims principal if token is valid, null otherwise.</returns>
        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                // Ensure the token has the required claims
                if (principal.FindFirst(RbacConstants.Claims.UserId) == null ||
                    principal.FindFirst(RbacConstants.Claims.Role) == null)
                {
                    _logger.LogWarning("JWT token missing required claims");
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("JWT token has invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT token validation failed with unexpected error");
                return null;
            }
        }
    }
} 