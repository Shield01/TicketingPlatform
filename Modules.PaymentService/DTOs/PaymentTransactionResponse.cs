namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Response model for payment transaction information.
    /// </summary>
    public class PaymentTransactionResponse
    {
        /// <summary>
        /// The unique identifier of the transaction.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// The amount that was paid.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency of the payment.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// The status of the payment.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The payment gateway used.
        /// </summary>
        public string Gateway { get; set; } = string.Empty;

        /// <summary>
        /// The transaction reference.
        /// </summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the transaction was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the transaction was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }
} 