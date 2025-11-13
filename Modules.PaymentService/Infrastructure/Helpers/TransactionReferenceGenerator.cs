using System.Security.Cryptography;
using System.Text;

namespace Modules.PaymentService.Infrastructure.Helpers
{
    /// <summary>
    /// Helper class for generating unique transaction references for PayAza operations.
    /// </summary>
    public static class TransactionReferenceGenerator
    {
        private static readonly object _lock = new object();
        private static long _counter = 0;

        /// <summary>
        /// Generates a unique transaction reference with the format: PREFIX-YYYYMMDD-UUID.
        /// </summary>
        /// <param name="prefix">The prefix for the transaction reference (e.g., "PAY", "REF").</param>
        /// <returns>A unique transaction reference string.</returns>
        public static string Generate(string prefix = "TXN")
        {
            var dateString = DateTime.UtcNow.ToString("yyyyMMdd");
            var uniqueId = Guid.NewGuid().ToString("N")[..12].ToUpper();
            return $"{prefix}-{dateString}-{uniqueId}";
        }

        /// <summary>
        /// Generates a transaction reference for a specific event.
        /// Format: EVT-{EventId}-YYYYMMDD-UUID
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <returns>A unique transaction reference string.</returns>
        public static string GenerateForEvent(Guid eventId)
        {
            var dateString = DateTime.UtcNow.ToString("yyyyMMdd");
            var eventIdShort = eventId.ToString("N")[..8].ToUpper();
            var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            return $"EVT-{eventIdShort}-{dateString}-{uniqueId}";
        }

        /// <summary>
        /// Generates a transaction reference with a timestamp component.
        /// Format: PREFIX-YYYYMMDDHHMMSS-UUID
        /// </summary>
        /// <param name="prefix">The prefix for the transaction reference.</param>
        /// <returns>A unique transaction reference string with timestamp.</returns>
        public static string GenerateWithTimestamp(string prefix = "TXN")
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            return $"{prefix}-{timestamp}-{uniqueId}";
        }

        /// <summary>
        /// Generates a sequential transaction reference for testing purposes.
        /// Format: PREFIX-YYYYMMDD-SEQ{counter}
        /// </summary>
        /// <param name="prefix">The prefix for the transaction reference.</param>
        /// <returns>A sequential transaction reference string.</returns>
        public static string GenerateSequential(string prefix = "TEST")
        {
            lock (_lock)
            {
                _counter++;
                var dateString = DateTime.UtcNow.ToString("yyyyMMdd");
                return $"{prefix}-{dateString}-SEQ{_counter:D8}";
            }
        }

        /// <summary>
        /// Generates an idempotency key for PayAza operations.
        /// Format: SHA256 hash of the transaction data.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="amount">The transaction amount.</param>
        /// <param name="currency">The transaction currency.</param>
        /// <param name="timestamp">The timestamp of the transaction.</param>
        /// <returns>An idempotency key string.</returns>
        public static string GenerateIdempotencyKey(Guid userId, decimal amount, string currency, DateTime timestamp)
        {
            var data = $"{userId}|{amount}|{currency}|{timestamp:O}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        /// <summary>
        /// Validates a transaction reference format.
        /// </summary>
        /// <param name="reference">The transaction reference to validate.</param>
        /// <returns>True if the reference format is valid, false otherwise.</returns>
        public static bool IsValid(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                return false;

            var parts = reference.Split('-');
            
            // Minimum format: PREFIX-DATE-UUID (3 parts) or EVT-EVENTID-DATE-UUID (4 parts)
            if (parts.Length < 3)
                return false;

            // For event-specific references (4 parts): EVT-EVENTID-DATE-UUID
            if (parts.Length == 4 && parts[0] == "EVT")
            {
                // Check if the third part is a valid date (YYYYMMDD)
                if (parts[2].Length == 8 && int.TryParse(parts[2], out _))
                    return true;
            }

            // For standard references (3 parts): PREFIX-DATE-UUID
            if (parts.Length >= 3)
            {
                // Check if the second part is a valid date (YYYYMMDD)
                if (parts[1].Length == 8 && int.TryParse(parts[1], out _))
                    return true;

                // Check if the second part is a timestamp (YYYYMMDDHHMMSS - 14 chars)
                if (parts[1].Length == 14 && long.TryParse(parts[1], out _))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Extracts the date from a transaction reference.
        /// </summary>
        /// <param name="reference">The transaction reference.</param>
        /// <returns>The date if extraction succeeds, null otherwise.</returns>
        public static DateTime? ExtractDate(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                return null;

            var parts = reference.Split('-');
            if (parts.Length < 3)
                return null;

            var datePart = parts[1];
            
            // Try to parse as date (YYYYMMDD)
            if (datePart.Length == 8 && DateTime.TryParseExact(datePart, "yyyyMMdd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var date))
            {
                return date;
            }

            // Try to parse as timestamp (YYYYMMDDHHMMSS)
            if (datePart.Length == 14 && DateTime.TryParseExact(datePart, "yyyyMMddHHmmss", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var timestamp))
            {
                return timestamp;
            }

            return null;
        }

    }
}

