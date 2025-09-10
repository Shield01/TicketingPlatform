using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Modules.TicketService.Configuration;
using Modules.TicketService.DTOs;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service implementation for email operations using MailKit.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly IEmailTemplateService _templateService;
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// Initializes a new instance of the EmailService.
        /// </summary>
        /// <param name="emailConfig">The email configuration options.</param>
        /// <param name="templateService">The email template service.</param>
        /// <param name="logger">The logger instance.</param>
        public EmailService(IOptions<EmailConfiguration> emailConfig, IEmailTemplateService templateService, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig.Value;
            _templateService = templateService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a ticket confirmation email to the user.
        /// </summary>
        /// <param name="ticketResponse">The ticket response containing ticket details.</param>
        /// <param name="userEmail">The user's email address.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>True if email was sent successfully, false otherwise.</returns>
        public async Task<bool> SendTicketConfirmationEmailAsync(TicketResponse ticketResponse, string userEmail, string userName, string eventName)
        {
            if (!_emailConfig.IsEnabled)
            {
                _logger.LogInformation("Email functionality is disabled. Skipping email send for ticket {TicketId}", ticketResponse.Id);
                return true; // Return true since this is expected behavior when disabled
            }

            try
            {
                _logger.LogInformation("Sending ticket confirmation email for ticket {TicketId} to {UserEmail}", ticketResponse.Id, userEmail);

                var message = CreateTicketConfirmationMessage(ticketResponse, userEmail, userName, eventName);
                var success = await SendEmailAsync(message);

                if (success)
                {
                    _logger.LogInformation("Successfully sent ticket confirmation email for ticket {TicketId} to {UserEmail}", ticketResponse.Id, userEmail);
                }
                else
                {
                    _logger.LogError("Failed to send ticket confirmation email for ticket {TicketId} to {UserEmail}", ticketResponse.Id, userEmail);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket confirmation email for ticket {TicketId} to {UserEmail}", ticketResponse.Id, userEmail);
                return false;
            }
        }

        /// <summary>
        /// Sends multiple ticket confirmation emails to the user.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userEmail">The user's email address.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>True if all emails were sent successfully, false otherwise.</returns>
        public async Task<bool> SendMultipleTicketConfirmationEmailsAsync(IEnumerable<TicketResponse> ticketResponses, string userEmail, string userName, string eventName)
        {
            if (!_emailConfig.IsEnabled)
            {
                _logger.LogInformation("Email functionality is disabled. Skipping multiple ticket email send to {UserEmail}", userEmail);
                return true; // Return true since this is expected behavior when disabled
            }

            var tickets = ticketResponses.ToList();
            if (!tickets.Any())
            {
                _logger.LogWarning("No tickets provided for email sending to {UserEmail}", userEmail);
                return false;
            }

            try
            {
                _logger.LogInformation("Sending multiple ticket confirmation email for {TicketCount} tickets to {UserEmail}", tickets.Count, userEmail);

                var message = CreateMultipleTicketConfirmationMessage(tickets, userEmail, userName, eventName);
                var success = await SendEmailAsync(message);

                if (success)
                {
                    _logger.LogInformation("Successfully sent multiple ticket confirmation email for {TicketCount} tickets to {UserEmail}", tickets.Count, userEmail);
                }
                else
                {
                    _logger.LogError("Failed to send multiple ticket confirmation email for {TicketCount} tickets to {UserEmail}", tickets.Count, userEmail);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending multiple ticket confirmation email for {TicketCount} tickets to {UserEmail}", tickets.Count, userEmail);
                return false;
            }
        }

        /// <summary>
        /// Tests the email configuration by sending a test email.
        /// </summary>
        /// <param name="testEmail">The email address to send the test to.</param>
        /// <returns>True if test email was sent successfully, false otherwise.</returns>
        public async Task<bool> SendTestEmailAsync(string testEmail)
        {
            if (!_emailConfig.IsEnabled)
            {
                _logger.LogInformation("Email functionality is disabled. Cannot send test email to {TestEmail}", testEmail);
                return false;
            }

            try
            {
                _logger.LogInformation("Sending test email to {TestEmail}", testEmail);

                var message = CreateTestMessage(testEmail);
                var success = await SendEmailAsync(message);

                if (success)
                {
                    _logger.LogInformation("Successfully sent test email to {TestEmail}", testEmail);
                }
                else
                {
                    _logger.LogError("Failed to send test email to {TestEmail}", testEmail);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email to {TestEmail}", testEmail);
                return false;
            }
        }

        /// <summary>
        /// Creates a ticket confirmation email message.
        /// </summary>
        /// <param name="ticketResponse">The ticket response.</param>
        /// <param name="userEmail">The user's email address.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>A MimeMessage for the ticket confirmation.</returns>
        private MimeMessage CreateTicketConfirmationMessage(TicketResponse ticketResponse, string userEmail, string userName, string eventName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmail));
            message.To.Add(new MailboxAddress(userName, userEmail));
            message.Subject = $"🎫 Ticket Confirmation - {eventName}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = _templateService.GenerateTicketConfirmationHtml(ticketResponse, userName, eventName),
                TextBody = _templateService.GenerateTicketConfirmationText(ticketResponse, userName, eventName)
            };

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        /// <summary>
        /// Creates a multiple ticket confirmation email message.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userEmail">The user's email address.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>A MimeMessage for the multiple ticket confirmation.</returns>
        private MimeMessage CreateMultipleTicketConfirmationMessage(IEnumerable<TicketResponse> ticketResponses, string userEmail, string userName, string eventName)
        {
            var tickets = ticketResponses.ToList();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmail));
            message.To.Add(new MailboxAddress(userName, userEmail));
            message.Subject = $"🎫 {tickets.Count} Tickets Confirmed - {eventName}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = _templateService.GenerateMultipleTicketConfirmationHtml(tickets, userName, eventName),
                TextBody = _templateService.GenerateMultipleTicketConfirmationText(tickets, userName, eventName)
            };

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        /// <summary>
        /// Creates a test email message.
        /// </summary>
        /// <param name="testEmail">The test email address.</param>
        /// <returns>A MimeMessage for the test email.</returns>
        private MimeMessage CreateTestMessage(string testEmail)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmail));
            message.To.Add(new MailboxAddress("Test User", testEmail));
            message.Subject = "Test Email - Ticketing Platform";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = @"
                    <html>
                    <body>
                        <h2>Test Email</h2>
                        <p>This is a test email from the Ticketing Platform.</p>
                        <p>If you received this email, the email configuration is working correctly.</p>
                        <p>Sent at: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + @"</p>
                    </body>
                    </html>",
                TextBody = $@"
Test Email
==========

This is a test email from the Ticketing Platform.

If you received this email, the email configuration is working correctly.

Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}"
            };

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        /// <summary>
        /// Sends an email message using SMTP.
        /// </summary>
        /// <param name="message">The email message to send.</param>
        /// <returns>True if sent successfully, false otherwise.</returns>
        private async Task<bool> SendEmailAsync(MimeMessage message)
        {
            try
            {
                using var client = new SmtpClient();
                
                // Configure timeout
                client.Timeout = _emailConfig.TimeoutSeconds * 1000; // Convert to milliseconds
                
                // Connect to SMTP server
                await client.ConnectAsync(_emailConfig.SmtpHost, _emailConfig.SmtpPort, _emailConfig.UseSsl);
                
                // Authenticate if credentials are provided
                if (!string.IsNullOrEmpty(_emailConfig.SmtpUsername) && !string.IsNullOrEmpty(_emailConfig.SmtpPassword))
                {
                    await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                }
                
                // Send the email
                await client.SendAsync(message);
                
                // Disconnect
                await client.DisconnectAsync(true);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP error while sending email. Host: {SmtpHost}, Port: {SmtpPort}, UseSsl: {UseSsl}", 
                    _emailConfig.SmtpHost, _emailConfig.SmtpPort, _emailConfig.UseSsl);
                return false;
            }
        }
    }
}
