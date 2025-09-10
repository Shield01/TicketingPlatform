using Modules.TicketService.DTOs;
using System.Text;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service implementation for email template generation.
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        /// <summary>
        /// Generates HTML content for a single ticket confirmation email.
        /// </summary>
        /// <param name="ticketResponse">The ticket response containing ticket details.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>HTML content for the email.</returns>
        public string GenerateTicketConfirmationHtml(TicketResponse ticketResponse, string userName, string eventName)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <title>Ticket Confirmation</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }");
            html.AppendLine("        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }");
            html.AppendLine("        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }");
            html.AppendLine("        .ticket-card { background: white; border: 2px solid #e0e0e0; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine("        .ticket-header { border-bottom: 2px solid #667eea; padding-bottom: 15px; margin-bottom: 20px; }");
            html.AppendLine("        .ticket-code { font-size: 24px; font-weight: bold; color: #667eea; text-align: center; margin: 15px 0; }");
            html.AppendLine("        .qr-code { text-align: center; margin: 20px 0; }");
            html.AppendLine("        .qr-code img { max-width: 200px; border: 1px solid #ddd; border-radius: 5px; }");
            html.AppendLine("        .ticket-details { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 20px 0; }");
            html.AppendLine("        .detail-item { background: #f5f5f5; padding: 10px; border-radius: 5px; }");
            html.AppendLine("        .detail-label { font-weight: bold; color: #666; font-size: 12px; text-transform: uppercase; }");
            html.AppendLine("        .detail-value { font-size: 16px; margin-top: 5px; }");
            html.AppendLine("        .price { font-size: 28px; font-weight: bold; color: #2ecc71; text-align: center; margin: 20px 0; }");
            html.AppendLine("        .footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 14px; }");
            html.AppendLine("        .important { background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0; }");
            html.AppendLine("        .important h3 { color: #856404; margin-top: 0; }");
            html.AppendLine("        @media (max-width: 600px) { .ticket-details { grid-template-columns: 1fr; } }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine("    <div class=\"header\">");
            html.AppendLine("        <h1>🎫 Ticket Confirmation</h1>");
            html.AppendLine("        <p>Your ticket has been successfully issued!</p>");
            html.AppendLine("    </div>");
            
            // Content
            html.AppendLine("    <div class=\"content\">");
            html.AppendLine($"        <p>Dear {userName},</p>");
            html.AppendLine("        <p>Thank you for your purchase! Your ticket has been successfully issued and is ready for use.</p>");
            
            // Ticket Card
            html.AppendLine("        <div class=\"ticket-card\">");
            html.AppendLine("            <div class=\"ticket-header\">");
            html.AppendLine($"                <h2>{eventName}</h2>");
            html.AppendLine($"                <h3>{ticketResponse.TierName}</h3>");
            html.AppendLine("            </div>");
            
            html.AppendLine($"            <div class=\"ticket-code\">{ticketResponse.TicketCode}</div>");
            
            // QR Code
            if (!string.IsNullOrEmpty(ticketResponse.QRCodeImage))
            {
                html.AppendLine("            <div class=\"qr-code\">");
                html.AppendLine($"                <img src=\"data:image/png;base64,{ticketResponse.QRCodeImage}\" alt=\"QR Code\" />");
                html.AppendLine("                <p><strong>Present this QR code at the event entrance</strong></p>");
                html.AppendLine("            </div>");
            }
            
            // Ticket Details
            html.AppendLine("            <div class=\"ticket-details\">");
            html.AppendLine("                <div class=\"detail-item\">");
            html.AppendLine("                    <div class=\"detail-label\">Event</div>");
            html.AppendLine($"                    <div class=\"detail-value\">{eventName}</div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"detail-item\">");
            html.AppendLine("                    <div class=\"detail-label\">Ticket Type</div>");
            html.AppendLine($"                    <div class=\"detail-value\">{ticketResponse.TierName}</div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"detail-item\">");
            html.AppendLine("                    <div class=\"detail-label\">Issued Date</div>");
            html.AppendLine($"                    <div class=\"detail-value\">{ticketResponse.IssuedAt:MMMM dd, yyyy 'at' h:mm tt}</div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"detail-item\">");
            html.AppendLine("                    <div class=\"detail-label\">Status</div>");
            html.AppendLine($"                    <div class=\"detail-value\">{ticketResponse.Status}</div>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
            
            html.AppendLine($"            <div class=\"price\">{ticketResponse.Currency} {ticketResponse.Price:F2}</div>");
            
            if (!string.IsNullOrEmpty(ticketResponse.TierDescription))
            {
                html.AppendLine($"            <p><strong>Description:</strong> {ticketResponse.TierDescription}</p>");
            }
            
            html.AppendLine("        </div>");
            
            // Important Information
            html.AppendLine("        <div class=\"important\">");
            html.AppendLine("            <h3>📱 Important Information</h3>");
            html.AppendLine("            <ul>");
            html.AppendLine("                <li>Please arrive at the event venue at least 30 minutes before the start time</li>");
            html.AppendLine("                <li>Present this email or the QR code on your mobile device at the entrance</li>");
            html.AppendLine("                <li>Keep this email safe as it serves as your ticket confirmation</li>");
            html.AppendLine("                <li>Contact the event organizer if you have any questions</li>");
            html.AppendLine("            </ul>");
            html.AppendLine("        </div>");
            
            html.AppendLine("        <p>We look forward to seeing you at the event!</p>");
            html.AppendLine("        <p>Best regards,<br>The Ticketing Platform Team</p>");
            
            // Footer
            html.AppendLine("        <div class=\"footer\">");
            html.AppendLine("            <p>This is an automated email. Please do not reply to this message.</p>");
            html.AppendLine("            <p>If you have any questions, please contact our support team.</p>");
            html.AppendLine("        </div>");
            
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates HTML content for multiple ticket confirmation emails.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>HTML content for the email.</returns>
        public string GenerateMultipleTicketConfirmationHtml(IEnumerable<TicketResponse> ticketResponses, string userName, string eventName)
        {
            var tickets = ticketResponses.ToList();
            var totalPrice = tickets.Sum(t => t.Price);
            var currency = tickets.FirstOrDefault()?.Currency ?? "USD";
            
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <title>Multiple Tickets Confirmation</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }");
            html.AppendLine("        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }");
            html.AppendLine("        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }");
            html.AppendLine("        .ticket-card { background: white; border: 2px solid #e0e0e0; border-radius: 10px; padding: 20px; margin: 20px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine("        .ticket-header { border-bottom: 2px solid #667eea; padding-bottom: 15px; margin-bottom: 20px; }");
            html.AppendLine("        .ticket-code { font-size: 20px; font-weight: bold; color: #667eea; text-align: center; margin: 15px 0; }");
            html.AppendLine("        .qr-code { text-align: center; margin: 20px 0; }");
            html.AppendLine("        .qr-code img { max-width: 150px; border: 1px solid #ddd; border-radius: 5px; }");
            html.AppendLine("        .ticket-details { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 20px 0; }");
            html.AppendLine("        .detail-item { background: #f5f5f5; padding: 10px; border-radius: 5px; }");
            html.AppendLine("        .detail-label { font-weight: bold; color: #666; font-size: 12px; text-transform: uppercase; }");
            html.AppendLine("        .detail-value { font-size: 14px; margin-top: 5px; }");
            html.AppendLine("        .price { font-size: 18px; font-weight: bold; color: #2ecc71; text-align: center; margin: 15px 0; }");
            html.AppendLine("        .total-price { font-size: 32px; font-weight: bold; color: #2ecc71; text-align: center; margin: 30px 0; padding: 20px; background: #f0f8f0; border-radius: 10px; }");
            html.AppendLine("        .footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 14px; }");
            html.AppendLine("        .important { background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0; }");
            html.AppendLine("        .important h3 { color: #856404; margin-top: 0; }");
            html.AppendLine("        @media (max-width: 600px) { .ticket-details { grid-template-columns: 1fr; } }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine("    <div class=\"header\">");
            html.AppendLine($"        <h1>🎫 {tickets.Count} Tickets Confirmed</h1>");
            html.AppendLine("        <p>Your tickets have been successfully issued!</p>");
            html.AppendLine("    </div>");
            
            // Content
            html.AppendLine("    <div class=\"content\">");
            html.AppendLine($"        <p>Dear {userName},</p>");
            html.AppendLine($"        <p>Thank you for your purchase! Your {tickets.Count} ticket(s) have been successfully issued and are ready for use.</p>");
            
            // Individual Tickets
            for (int i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                html.AppendLine($"        <div class=\"ticket-card\">");
                html.AppendLine($"            <div class=\"ticket-header\">");
                html.AppendLine($"                <h3>Ticket #{i + 1} - {ticket.TierName}</h3>");
                html.AppendLine("            </div>");
                
                html.AppendLine($"            <div class=\"ticket-code\">{ticket.TicketCode}</div>");
                
                // QR Code
                if (!string.IsNullOrEmpty(ticket.QRCodeImage))
                {
                    html.AppendLine("            <div class=\"qr-code\">");
                    html.AppendLine($"                <img src=\"data:image/png;base64,{ticket.QRCodeImage}\" alt=\"QR Code\" />");
                    html.AppendLine("            </div>");
                }
                
                // Ticket Details
                html.AppendLine("            <div class=\"ticket-details\">");
                html.AppendLine("                <div class=\"detail-item\">");
                html.AppendLine("                    <div class=\"detail-label\">Event</div>");
                html.AppendLine($"                    <div class=\"detail-value\">{eventName}</div>");
                html.AppendLine("                </div>");
                html.AppendLine("                <div class=\"detail-item\">");
                html.AppendLine("                    <div class=\"detail-label\">Ticket Type</div>");
                html.AppendLine($"                    <div class=\"detail-value\">{ticket.TierName}</div>");
                html.AppendLine("                </div>");
                html.AppendLine("                <div class=\"detail-item\">");
                html.AppendLine("                    <div class=\"detail-label\">Issued Date</div>");
                html.AppendLine($"                    <div class=\"detail-value\">{ticket.IssuedAt:MMMM dd, yyyy 'at' h:mm tt}</div>");
                html.AppendLine("                </div>");
                html.AppendLine("                <div class=\"detail-item\">");
                html.AppendLine("                    <div class=\"detail-label\">Status</div>");
                html.AppendLine($"                    <div class=\"detail-value\">{ticket.Status}</div>");
                html.AppendLine("                </div>");
                html.AppendLine("            </div>");
                
                html.AppendLine($"            <div class=\"price\">{ticket.Currency} {ticket.Price:F2}</div>");
                
                if (!string.IsNullOrEmpty(ticket.TierDescription))
                {
                    html.AppendLine($"            <p><strong>Description:</strong> {ticket.TierDescription}</p>");
                }
                
                html.AppendLine("        </div>");
            }
            
            // Total Price
            html.AppendLine($"        <div class=\"total-price\">");
            html.AppendLine($"            Total Amount: {currency} {totalPrice:F2}");
            html.AppendLine("        </div>");
            
            // Important Information
            html.AppendLine("        <div class=\"important\">");
            html.AppendLine("            <h3>📱 Important Information</h3>");
            html.AppendLine("            <ul>");
            html.AppendLine("                <li>Please arrive at the event venue at least 30 minutes before the start time</li>");
            html.AppendLine("                <li>Present this email or the QR codes on your mobile device at the entrance</li>");
            html.AppendLine("                <li>Each ticket has a unique QR code - make sure to present the correct one</li>");
            html.AppendLine("                <li>Keep this email safe as it serves as your ticket confirmation</li>");
            html.AppendLine("                <li>Contact the event organizer if you have any questions</li>");
            html.AppendLine("            </ul>");
            html.AppendLine("        </div>");
            
            html.AppendLine("        <p>We look forward to seeing you at the event!</p>");
            html.AppendLine("        <p>Best regards,<br>The Ticketing Platform Team</p>");
            
            // Footer
            html.AppendLine("        <div class=\"footer\">");
            html.AppendLine("            <p>This is an automated email. Please do not reply to this message.</p>");
            html.AppendLine("            <p>If you have any questions, please contact our support team.</p>");
            html.AppendLine("        </div>");
            
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates plain text content for a single ticket confirmation email.
        /// </summary>
        /// <param name="ticketResponse">The ticket response containing ticket details.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>Plain text content for the email.</returns>
        public string GenerateTicketConfirmationText(TicketResponse ticketResponse, string userName, string eventName)
        {
            var text = new StringBuilder();
            
            text.AppendLine("TICKET CONFIRMATION");
            text.AppendLine("==================");
            text.AppendLine();
            text.AppendLine($"Dear {userName},");
            text.AppendLine();
            text.AppendLine("Thank you for your purchase! Your ticket has been successfully issued and is ready for use.");
            text.AppendLine();
            text.AppendLine("TICKET DETAILS:");
            text.AppendLine("---------------");
            text.AppendLine($"Event: {eventName}");
            text.AppendLine($"Ticket Type: {ticketResponse.TierName}");
            text.AppendLine($"Ticket Code: {ticketResponse.TicketCode}");
            text.AppendLine($"Price: {ticketResponse.Currency} {ticketResponse.Price:F2}");
            text.AppendLine($"Issued Date: {ticketResponse.IssuedAt:MMMM dd, yyyy 'at' h:mm tt}");
            text.AppendLine($"Status: {ticketResponse.Status}");
            
            if (!string.IsNullOrEmpty(ticketResponse.TierDescription))
            {
                text.AppendLine($"Description: {ticketResponse.TierDescription}");
            }
            
            text.AppendLine();
            text.AppendLine("IMPORTANT INFORMATION:");
            text.AppendLine("---------------------");
            text.AppendLine("- Please arrive at the event venue at least 30 minutes before the start time");
            text.AppendLine("- Present this email or the QR code on your mobile device at the entrance");
            text.AppendLine("- Keep this email safe as it serves as your ticket confirmation");
            text.AppendLine("- Contact the event organizer if you have any questions");
            text.AppendLine();
            text.AppendLine("We look forward to seeing you at the event!");
            text.AppendLine();
            text.AppendLine("Best regards,");
            text.AppendLine("The Ticketing Platform Team");
            text.AppendLine();
            text.AppendLine("---");
            text.AppendLine("This is an automated email. Please do not reply to this message.");
            text.AppendLine("If you have any questions, please contact our support team.");
            
            return text.ToString();
        }

        /// <summary>
        /// Generates plain text content for multiple ticket confirmation emails.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userName">The user's name.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>Plain text content for the email.</returns>
        public string GenerateMultipleTicketConfirmationText(IEnumerable<TicketResponse> ticketResponses, string userName, string eventName)
        {
            var tickets = ticketResponses.ToList();
            var totalPrice = tickets.Sum(t => t.Price);
            var currency = tickets.FirstOrDefault()?.Currency ?? "USD";
            
            var text = new StringBuilder();
            
            text.AppendLine("MULTIPLE TICKETS CONFIRMATION");
            text.AppendLine("=============================");
            text.AppendLine();
            text.AppendLine($"Dear {userName},");
            text.AppendLine();
            text.AppendLine($"Thank you for your purchase! Your {tickets.Count} ticket(s) have been successfully issued and are ready for use.");
            text.AppendLine();
            
            for (int i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                text.AppendLine($"TICKET #{i + 1}:");
                text.AppendLine("---------------");
                text.AppendLine($"Event: {eventName}");
                text.AppendLine($"Ticket Type: {ticket.TierName}");
                text.AppendLine($"Ticket Code: {ticket.TicketCode}");
                text.AppendLine($"Price: {ticket.Currency} {ticket.Price:F2}");
                text.AppendLine($"Issued Date: {ticket.IssuedAt:MMMM dd, yyyy 'at' h:mm tt}");
                text.AppendLine($"Status: {ticket.Status}");
                
                if (!string.IsNullOrEmpty(ticket.TierDescription))
                {
                    text.AppendLine($"Description: {ticket.TierDescription}");
                }
                
                text.AppendLine();
            }
            
            text.AppendLine($"TOTAL AMOUNT: {currency} {totalPrice:F2}");
            text.AppendLine();
            text.AppendLine("IMPORTANT INFORMATION:");
            text.AppendLine("---------------------");
            text.AppendLine("- Please arrive at the event venue at least 30 minutes before the start time");
            text.AppendLine("- Present this email or the QR codes on your mobile device at the entrance");
            text.AppendLine("- Each ticket has a unique QR code - make sure to present the correct one");
            text.AppendLine("- Keep this email safe as it serves as your ticket confirmation");
            text.AppendLine("- Contact the event organizer if you have any questions");
            text.AppendLine();
            text.AppendLine("We look forward to seeing you at the event!");
            text.AppendLine();
            text.AppendLine("Best regards,");
            text.AppendLine("The Ticketing Platform Team");
            text.AppendLine();
            text.AppendLine("---");
            text.AppendLine("This is an automated email. Please do not reply to this message.");
            text.AppendLine("If you have any questions, please contact our support team.");
            
            return text.ToString();
        }
    }
}
