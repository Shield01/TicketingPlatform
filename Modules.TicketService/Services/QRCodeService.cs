using Microsoft.Extensions.Logging;
using Modules.TicketService.Models;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service for generating and validating QR codes for tickets.
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        private readonly ILogger<QRCodeService> _logger;
        private const int QRCodeSize = 512;
        private const int QRCodeBorder = 4;

        /// <summary>
        /// Initializes a new instance of the QRCodeService.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public QRCodeService(ILogger<QRCodeService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generates a QR code image as a base64 string for the given ticket.
        /// </summary>
        /// <param name="ticket">The ticket to generate QR code for.</param>
        /// <returns>A base64 encoded QR code image string.</returns>
        public string GenerateQRCodeImage(Ticket ticket)
        {
            try
            {
                var qrData = GenerateJWTLikeQRData(ticket);
                return GenerateQRCodeImage(qrData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image for ticket {TicketId}", ticket.Id);
                throw;
            }
        }

        /// <summary>
        /// Generates a QR code image as a base64 string for the given QR data.
        /// </summary>
        /// <param name="qrData">The QR code data to encode.</param>
        /// <returns>A base64 encoded QR code image string.</returns>
        public string GenerateQRCodeImage(string qrData)
        {
            try
            {
                var qrCodeBytes = GenerateQRCodeBytes(qrData);
                return Convert.ToBase64String(qrCodeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image for data");
                throw;
            }
        }

        /// <summary>
        /// Generates QR code data for a ticket with JWT-like structure.
        /// </summary>
        /// <param name="ticket">The ticket to generate QR data for.</param>
        /// <returns>QR code data string with JWT-like structure.</returns>
        public string GenerateJWTLikeQRData(Ticket ticket)
        {
            try
            {
                // Create JWT-like structure: header.payload.signature
                var header = new
                {
                    alg = "HS256",
                    typ = "TICKET"
                };

                var payload = new
                {
                    ticketId = ticket.Id.ToString(),
                    ticketCode = ticket.TicketCode,
                    eventId = ticket.EventId.ToString(),
                    userId = ticket.UserId.ToString(),
                    ticketTierId = ticket.TicketTierId.ToString(),
                    status = ticket.Status,
                    issuedAt = ticket.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    exp = ticket.CreatedAt.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ssZ") // 1 year expiration
                };

                var headerJson = JsonSerializer.Serialize(header);
                var payloadJson = JsonSerializer.Serialize(payload);

                var headerBase64 = Base64UrlEncode(headerJson);
                var payloadBase64 = Base64UrlEncode(payloadJson);

                // Create signature using ticket-specific secret
                var signature = GenerateSignature($"{headerBase64}.{payloadBase64}", ticket.Id.ToString());

                return $"{headerBase64}.{payloadBase64}.{signature}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT-like QR data for ticket {TicketId}", ticket.Id);
                throw;
            }
        }

        /// <summary>
        /// Validates QR code data and extracts ticket information.
        /// </summary>
        /// <param name="qrData">The QR code data to validate.</param>
        /// <returns>A dictionary containing extracted ticket information, or null if invalid.</returns>
        public Dictionary<string, string>? ValidateAndExtractQRData(string qrData)
        {
            try
            {
                if (string.IsNullOrEmpty(qrData))
                    return null;

                var parts = qrData.Split('.');
                if (parts.Length != 3)
                    return null;

                var headerBase64 = parts[0];
                var payloadBase64 = parts[1];
                var signature = parts[2];

                // Decode header and payload
                var headerJson = Base64UrlDecode(headerBase64);
                var payloadJson = Base64UrlDecode(payloadBase64);

                var header = JsonSerializer.Deserialize<Dictionary<string, object>>(headerJson);
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);

                if (header == null || payload == null)
                    return null;

                // Validate header
                if (!header.ContainsKey("typ") || header["typ"]?.ToString() != "TICKET")
                    return null;

                // Validate signature
                var expectedSignature = GenerateSignature($"{headerBase64}.{payloadBase64}", payload["ticketId"]?.ToString() ?? "");
                if (signature != expectedSignature)
                    return null;

                // Check expiration
                if (payload.ContainsKey("exp"))
                {
                    if (DateTime.TryParse(payload["exp"]?.ToString(), out var expDate) && expDate < DateTime.UtcNow)
                        return null;
                }

                // Convert payload to string dictionary
                var result = new Dictionary<string, string>();
                foreach (var kvp in payload)
                {
                    result[kvp.Key] = kvp.Value?.ToString() ?? "";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating QR code data");
                return null;
            }
        }

        /// <summary>
        /// Generates a QR code image as byte array for the given ticket.
        /// </summary>
        /// <param name="ticket">The ticket to generate QR code for.</param>
        /// <returns>A byte array containing the QR code image.</returns>
        public byte[] GenerateQRCodeBytes(Ticket ticket)
        {
            try
            {
                var qrData = GenerateJWTLikeQRData(ticket);
                return GenerateQRCodeBytes(qrData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code bytes for ticket {TicketId}", ticket.Id);
                throw;
            }
        }

        /// <summary>
        /// Generates a QR code image as byte array for the given QR data.
        /// </summary>
        /// <param name="qrData">The QR code data to encode.</param>
        /// <returns>A byte array containing the QR code image.</returns>
        public byte[] GenerateQRCodeBytes(string qrData)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                
                return qrCode.GetGraphic(QRCodeBorder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code bytes for data");
                throw;
            }
        }

        /// <summary>
        /// Base64 URL encodes a string.
        /// </summary>
        /// <param name="input">The string to encode.</param>
        /// <returns>Base64 URL encoded string.</returns>
        private static string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var base64 = Convert.ToBase64String(bytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        /// <summary>
        /// Base64 URL decodes a string.
        /// </summary>
        /// <param name="input">The string to decode.</param>
        /// <returns>Decoded string.</returns>
        private static string Base64UrlDecode(string input)
        {
            var base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Generates a signature for the given data using HMAC-SHA256.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <param name="ticketId">The ticket ID to use as part of the secret.</param>
        /// <returns>A base64 URL encoded signature.</returns>
        private static string GenerateSignature(string data, string ticketId)
        {
            // Use a combination of ticket ID and a secret key for signing
            var secret = $"TICKET_SECRET_{ticketId}_QR_CODE";
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return Base64UrlEncode(Convert.ToBase64String(hashBytes));
        }
    }
}
