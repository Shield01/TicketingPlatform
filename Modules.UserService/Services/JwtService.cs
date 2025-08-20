using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Modules.UserService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Modules.UserService.Services
{
    /// <summary>
    /// Service implementation for JWT token operations.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        /// <summary>
        /// Initializes a new instance of the JwtService class.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="logger">The logger instance.</param>
        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Get JWT configuration from appsettings
            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _issuer = _configuration["Jwt:Issuer"] ?? "TicketingPlatform";
            _audience = _configuration["Jwt:Audience"] ?? "TicketingPlatform";
            _expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 1440; // Default 24 hours
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>The generated JWT token.</returns>
        public string GenerateToken(User user)
        {
            _logger.LogInformation("Generating JWT token for user ID: {UserId}", user.Id);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(Shared.Kernel.Constants.RbacConstants.Claims.UserId, user.Id.ToString()),
                new Claim(Shared.Kernel.Constants.RbacConstants.Claims.Email, user.Email),
                new Claim(Shared.Kernel.Constants.RbacConstants.Claims.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("JWT token generated successfully for user ID: {UserId}", user.Id);
            return tokenString;
        }

        /// <summary>
        /// Validates a JWT token and extracts user information.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>The user ID if token is valid, null otherwise.</returns>
        public Guid? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token validation failed: Empty token provided");
                return null;
            }

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
                var userIdClaim = principal.FindFirst(Shared.Kernel.Constants.RbacConstants.Claims.UserId)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Token validation failed: Invalid user ID in token");
                    return null;
                }

                _logger.LogInformation("Token validated successfully for user ID: {UserId}", userId);
                return userId;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token validation failed: Token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Token validation failed: Invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed with unexpected error");
                return null;
            }
        }

        /// <summary>
        /// Gets the expiration time for JWT tokens.
        /// </summary>
        /// <returns>The expiration time in minutes.</returns>
        public int GetTokenExpirationMinutes()
        {
            return _expirationMinutes;
        }
    }
} 