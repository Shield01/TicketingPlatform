using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Request model for QR code validation.
    /// </summary>
    public class QRCodeValidationRequest
    {
        /// <summary>
        /// The QR code data to validate (JWT-like format).
        /// </summary>
        [Required(ErrorMessage = "QR code data is required.")]
        [StringLength(2000, ErrorMessage = "QR code data cannot exceed 2000 characters.")]
        public string QRCodeData { get; set; } = string.Empty;
    }
}
