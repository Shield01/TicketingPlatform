using System.ComponentModel.DataAnnotations;

namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Request model for handling web redirect callback from payment gateway.
    /// </summary>
    public class WebRedirectCallbackRequest
    {
        /// <summary>
        /// The transaction reference from the payment gateway.
        /// </summary>
        /// <example>PAY-20240115-A1B2C3D4E5F6</example>
        [Required(ErrorMessage = "TransactionReference is required")]
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// The status of the payment from the gateway.
        /// </summary>
        /// <example>success</example>
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The transaction ID from the payment gateway.
        /// </summary>
        /// <example>PAYAZA_TXN_123456789</example>
        public string? GatewayTransactionId { get; set; }

        /// <summary>
        /// The payment method used.
        /// </summary>
        /// <example>card</example>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Additional metadata from the gateway.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}

