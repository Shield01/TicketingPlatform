using Modules.TicketService.DTOs;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Interface for email template service operations.
    /// </summary>
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Generates HTML content for a single ticket confirmation email.
        /// </summary>
        /// <param name="ticketResponse">The ticket response containing ticket details.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>HTML content for the email.</returns>
        string GenerateTicketConfirmationHtml(TicketResponse ticketResponse, string userName, string eventName);

        /// <summary>
        /// Generates HTML content for multiple ticket confirmation emails.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>HTML content for the email.</returns>
        string GenerateMultipleTicketConfirmationHtml(IEnumerable<TicketResponse> ticketResponses, string userName, string eventName);

        /// <summary>
        /// Generates plain text content for a single ticket confirmation email.
        /// </summary>
        /// <param name="ticketResponse">The ticket response containing ticket details.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>Plain text content for the email.</returns>
        string GenerateTicketConfirmationText(TicketResponse ticketResponse, string userName, string eventName);

        /// <summary>
        /// Generates plain text content for multiple ticket confirmation emails.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>Plain text content for the email.</returns>
        string GenerateMultipleTicketConfirmationText(IEnumerable<TicketResponse> ticketResponses, string userName, string eventName);
    }
}
