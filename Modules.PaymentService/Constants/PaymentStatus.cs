namespace Modules.PaymentService.Constants
{
    /// <summary>
    /// Constants for payment transaction statuses.
    /// </summary>
    public static class PaymentStatus
    {
        /// <summary>
        /// Payment has been initiated and user is being redirected to payment page.
        /// </summary>
        public const string PendingRedirect = "PENDING_REDIRECT";

        /// <summary>
        /// Payment is being processed by the gateway.
        /// </summary>
        public const string Pending = "PENDING";

        /// <summary>
        /// Payment has been successfully completed.
        /// </summary>
        public const string Completed = "COMPLETED";

        /// <summary>
        /// Payment has been confirmed by the gateway.
        /// </summary>
        public const string Confirmed = "CONFIRMED";

        /// <summary>
        /// Payment has failed.
        /// </summary>
        public const string Failed = "FAILED";

        /// <summary>
        /// Payment has been cancelled by the user.
        /// </summary>
        public const string Cancelled = "CANCELLED";

        /// <summary>
        /// Payment has been refunded.
        /// </summary>
        public const string Refunded = "REFUNDED";

        /// <summary>
        /// Payment session has expired.
        /// </summary>
        public const string Expired = "EXPIRED";
    }
}

