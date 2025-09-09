using Modules.TicketService.Models;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Interface for QR code generation and validation services.
    /// </summary>
    public interface IQRCodeService
    {
        /// <summary>
        /// Generates a QR code image as a base64 string for the given ticket.
        /// </summary>
        /// <param name="ticket">The ticket to generate QR code for.</param>
        /// <returns>A base64 encoded QR code image string.</returns>
        string GenerateQRCodeImage(Ticket ticket);

        /// <summary>
        /// Generates a QR code image as a base64 string for the given QR data.
        /// </summary>
        /// <param name="qrData">The QR code data to encode.</param>
        /// <returns>A base64 encoded QR code image string.</returns>
        string GenerateQRCodeImage(string qrData);

        /// <summary>
        /// Generates QR code data for a ticket with JWT-like structure.
        /// </summary>
        /// <param name="ticket">The ticket to generate QR data for.</param>
        /// <returns>QR code data string with JWT-like structure.</returns>
        string GenerateJWTLikeQRData(Ticket ticket);

        /// <summary>
        /// Validates QR code data and extracts ticket information.
        /// </summary>
        /// <param name="qrData">The QR code data to validate.</param>
        /// <returns>A dictionary containing extracted ticket information, or null if invalid.</returns>
        Dictionary<string, string>? ValidateAndExtractQRData(string qrData);

        /// <summary>
        /// Generates a QR code image as byte array for the given ticket.
        /// </summary>
        /// <param name="ticket">The ticket to generate QR code for.</param>
        /// <returns>A byte array containing the QR code image.</returns>
        byte[] GenerateQRCodeBytes(Ticket ticket);

        /// <summary>
        /// Generates a QR code image as byte array for the given QR data.
        /// </summary>
        /// <param name="qrData">The QR code data to encode.</param>
        /// <returns>A byte array containing the QR code image.</returns>
        byte[] GenerateQRCodeBytes(string qrData);
    }
}
