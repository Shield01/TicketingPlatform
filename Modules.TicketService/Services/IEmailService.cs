using Modules.TicketService.DTOs;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Interface for email service operations.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a ticket confirmation email to the user.
        /// </summary>
        /// <param name="ticketResponse">The ticket response containing ticket details.</param>
        /// <param name="userEmail">The user's email address.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>True if email was sent successfully, false otherwise.</returns>
        Task<bool> SendTicketConfirmationEmailAsync(TicketResponse ticketResponse, string userEmail, string userName, string eventName);

        /// <summary>
        /// Sends multiple ticket confirmation emails to the user.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userEmail">The user's email address.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>True if all emails were sent successfully, false otherwise.</returns>
        Task<bool> SendMultipleTicketConfirmationEmailsAsync(IEnumerable<TicketResponse> ticketResponses, string userEmail, string userName, string eventName);

        /// <summary>
        /// Tests the email configuration by sending a test email.
        /// </summary>
        /// <param name="testEmail">The email address to send the test to.</param>
        /// <returns>True if test email was sent successfully, false otherwise.</returns>
        Task<bool> SendTestEmailAsync(string testEmail);
    }
}
