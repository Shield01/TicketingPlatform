namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Response model for QR code information.
    /// </summary>
    public class QRCodeResponse
    {
        /// <summary>
        /// The unique identifier of the ticket.
        /// </summary>
        public Guid TicketId { get; set; }

        /// <summary>
        /// The unique ticket code.
        /// </summary>
        public string TicketCode { get; set; } = string.Empty;

        /// <summary>
        /// The QR code data for scanning.
        /// </summary>
        public string QRCodeData { get; set; } = string.Empty;

        /// <summary>
        /// The QR code image as a base64 encoded string.
        /// </summary>
        public string QRCodeImage { get; set; } = string.Empty;

        /// <summary>
        /// The MIME type of the QR code image.
        /// </summary>
        public string ImageMimeType { get; set; } = "image/png";

        /// <summary>
        /// The size of the QR code image in pixels.
        /// </summary>
        public int ImageSize { get; set; } = 512;

        /// <summary>
        /// The date and time when the QR code was generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the ticket is valid for use.
        /// </summary>
        public bool IsValidForUse { get; set; }

        /// <summary>
        /// The current status of the ticket.
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
