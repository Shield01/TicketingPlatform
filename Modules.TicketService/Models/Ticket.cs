using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Modules.TicketService.Models
{
    /// <summary>
    /// Model representing a ticket issued for an event.
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Constants for ticket status values.
        /// </summary>
        public static class TicketStatus
        {
            public const string Unused = "UNUSED";
            public const string Used = "USED";
            public const string Cancelled = "CANCELLED";
            public const string Expired = "EXPIRED";
        }

        /// <summary>
        /// The unique identifier of the ticket.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the event this ticket is for.
        /// </summary>
        [Required]
        public Guid EventId { get; set; }

        /// <summary>
        /// The unique identifier of the user who purchased this ticket.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique identifier of the ticket tier this ticket belongs to.
        /// </summary>
        [Required]
        public Guid TicketTierId { get; set; }

        /// <summary>
        /// Navigation property for the ticket tier.
        /// </summary>
        public virtual TicketTier? TicketTier { get; set; }

        /// <summary>
        /// The price of the ticket at the time of purchase.
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// The currency of the ticket price.
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The unique ticket code/number for verification.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TicketCode { get; set; } = string.Empty;

        /// <summary>
        /// The QR code data for ticket scanning.
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// Whether the ticket has been used/scanned.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// The date and time when the ticket was used/scanned.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// The current status of the ticket.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = TicketStatus.Unused;

        /// <summary>
        /// The unique identifier of the payment that purchased this ticket.
        /// </summary>
        public Guid? PaymentId { get; set; }

        /// <summary>
        /// The date and time when the ticket was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the ticket was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the ticket is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Generates a unique ticket code for this ticket.
        /// </summary>
        /// <returns>A unique ticket code string.</returns>
        public static string GenerateTicketCode()
        {
            // Generate a ticket code in format: TKT-YYYYMMDD-XXXXXXXX
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomSuffix = GenerateRandomString(8);
            return $"TKT-{datePrefix}-{randomSuffix}";
        }

        /// <summary>
        /// Generates QR code data for this ticket using the legacy format.
        /// This method is kept for backward compatibility.
        /// </summary>
        /// <returns>QR code data string.</returns>
        public string GenerateQRCodeData()
        {
            // Generate QR code data containing essential ticket information
            var qrData = $"TICKET:{TicketCode}|EVENT:{EventId}|USER:{UserId}|TIER:{TicketTierId}|STATUS:{Status}|ISSUED:{CreatedAt:yyyy-MM-ddTHH:mm:ssZ}";
            
            // Add a hash for verification
            var hash = GenerateVerificationHash(qrData);
            return $"{qrData}|HASH:{hash}";
        }

        /// <summary>
        /// Sets the QR code data for this ticket.
        /// </summary>
        /// <param name="qrCodeData">The QR code data to set.</param>
        public void SetQRCodeData(string qrCodeData)
        {
            QRCodeData = qrCodeData;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the ticket as used.
        /// </summary>
        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
            Status = TicketStatus.Used;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cancels the ticket.
        /// </summary>
        public void Cancel()
        {
            Status = TicketStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the ticket is valid for use.
        /// </summary>
        /// <returns>True if the ticket can be used, false otherwise.</returns>
        public bool IsValidForUse()
        {
            return IsActive && 
                   Status == TicketStatus.Unused && 
                   !IsUsed;
        }

        /// <summary>
        /// Validates the QR code data for this ticket.
        /// </summary>
        /// <param name="qrCodeData">The QR code data to validate.</param>
        /// <returns>True if the QR code is valid, false otherwise.</returns>
        public bool ValidateQRCode(string qrCodeData)
        {
            if (string.IsNullOrEmpty(qrCodeData) || string.IsNullOrEmpty(QRCodeData))
                return false;

            // Extract hash from QR code data
            var parts = qrCodeData.Split('|');
            var hashPart = parts.LastOrDefault(p => p.StartsWith("HASH:"));
            
            if (hashPart == null)
                return false;

            var providedHash = hashPart.Substring(5); // Remove "HASH:" prefix
            var dataWithoutHash = string.Join("|", parts.Where(p => !p.StartsWith("HASH:")));
            var expectedHash = GenerateVerificationHash(dataWithoutHash);

            return providedHash == expectedHash;
        }

        /// <summary>
        /// Generates a random string of specified length.
        /// </summary>
        /// <param name="length">The length of the random string.</param>
        /// <returns>A random string.</returns>
        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a verification hash for the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>A hash string.</returns>
        private static string GenerateVerificationHash(string data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes)[..8]; // Take first 8 characters
        }
    }
}
