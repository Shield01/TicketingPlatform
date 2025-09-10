using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.TicketService.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Modules.TicketService.Controllers
{
    /// <summary>
    /// Controller for testing email functionality.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Require authentication for email testing
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;

        /// <summary>
        /// Initializes a new instance of the EmailTestController.
        /// </summary>
        /// <param name="emailService">The email service.</param>
        /// <param name="logger">The logger instance.</param>
        public EmailTestController(IEmailService emailService, ILogger<EmailTestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a test email to verify email configuration.
        /// </summary>
        /// <param name="request">The test email request containing the email address.</param>
        /// <returns>The result of the test email sending operation.</returns>
        [HttpPost("send-test")]
        [SwaggerOperation(
            Summary = "Send test email",
            Description = "Sends a test email to verify email configuration and SMTP connectivity."
        )]
        [SwaggerResponse(200, "Test email sent successfully", typeof(object))]
        [SwaggerResponse(400, "Invalid email address", typeof(object))]
        [SwaggerResponse(401, "Unauthorized", typeof(object))]
        [SwaggerResponse(500, "Internal server error", typeof(object))]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Test email request received for {TestEmail}", request.TestEmail);

                if (string.IsNullOrWhiteSpace(request.TestEmail))
                {
                    return BadRequest(new { message = "Test email address is required." });
                }

                if (!IsValidEmail(request.TestEmail))
                {
                    return BadRequest(new { message = "Invalid email address format." });
                }

                var result = await _emailService.SendTestEmailAsync(request.TestEmail);

                if (result)
                {
                    _logger.LogInformation("Test email sent successfully to {TestEmail}", request.TestEmail);
                    return Ok(new 
                    { 
                        message = "Test email sent successfully.",
                        testEmail = request.TestEmail,
                        sentAt = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to send test email to {TestEmail}", request.TestEmail);
                    return StatusCode(500, new 
                    { 
                        message = "Failed to send test email. Please check email configuration.",
                        testEmail = request.TestEmail
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email to {TestEmail}", request.TestEmail);
                return StatusCode(500, new 
                { 
                    message = "An error occurred while sending the test email.",
                    testEmail = request.TestEmail
                });
            }
        }

        /// <summary>
        /// Validates if the provided string is a valid email address.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Request model for test email endpoint.
    /// </summary>
    public class TestEmailRequest
    {
        /// <summary>
        /// The email address to send the test email to.
        /// </summary>
        public string TestEmail { get; set; } = string.Empty;
    }
}
