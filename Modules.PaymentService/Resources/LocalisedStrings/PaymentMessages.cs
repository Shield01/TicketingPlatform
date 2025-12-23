namespace Modules.PaymentService.Resources.LocalisedStrings
{
    /// <summary>
    /// Payment service specific messages and string constants
    /// </summary>
    public static class PaymentMessages
    {
        // Payment Initiation Messages
        public const string PaymentInitiated = "Payment initiated successfully.";
        public const string PaymentInitiationFailed = "Payment initiation failed.";
        public const string PaymentGatewayUnavailable = "Payment gateway is currently unavailable.";
        public const string InvalidPaymentAmount = "Invalid payment amount.";
        public const string PaymentAmountRequired = "Payment amount is required.";
        public const string PaymentCurrencyRequired = "Payment currency is required.";

        // Payment Confirmation Messages
        public const string PaymentConfirmed = "Payment confirmed successfully.";
        public const string PaymentConfirmationFailed = "Payment confirmation failed.";
        public const string PaymentAlreadyConfirmed = "Payment has already been confirmed.";
        public const string PaymentNotFound = "Payment not found.";
        public const string InvalidPaymentReference = "Invalid payment reference.";

        // Webhook Messages
        public const string WebhookReceived = "Payment webhook received successfully.";
        public const string WebhookProcessingFailed = "Webhook processing failed.";
        public const string InvalidWebhookSignature = "Invalid webhook signature.";
        public const string WebhookSignatureRequired = "Webhook signature is required.";

        // Transaction Messages
        public const string TransactionRecorded = "Transaction recorded successfully.";
        public const string TransactionRecordingFailed = "Failed to record transaction.";
        public const string TransactionRetrieved = "Transaction retrieved successfully.";
        public const string NoTransactionsFound = "No transactions found.";

        // Payment Gateway Messages
        public const string PayazaIntegrationError = "Payaza integration error occurred.";
        public const string FlutterwaveIntegrationError = "Flutterwave integration error occurred.";
        public const string GatewayTimeout = "Payment gateway timeout.";
        public const string GatewayUnavailable = "Payment gateway unavailable.";

        // Payment Status Messages
        public const string PaymentPending = "Payment is pending.";
        public const string PaymentSuccessful = "Payment was successful.";
        public const string PaymentFailed = "Payment failed.";
        public const string PaymentCancelled = "Payment was cancelled.";
        public const string PaymentRefunded = "Payment was refunded.";

        // Validation Messages
        public const string InvalidCurrency = "Invalid currency code.";
        public const string InvalidPaymentMethod = "Invalid payment method.";
        public const string PaymentMethodRequired = "Payment method is required.";
        public const string UserIdRequired = "User ID is required.";
        public const string EventIdRequired = "Event ID is required.";
        public const string TicketIdRequired = "Ticket ID is required.";

        // Error Messages
        public const string InsufficientFunds = "Insufficient funds for payment.";
        public const string CardDeclined = "Payment card was declined.";
        public const string NetworkError = "Network error during payment processing.";
        public const string TimeoutError = "Payment processing timed out.";

        // Log Messages
        public const string PaymentInitiationAttempt = "Payment initiation attempt for user ID: {0}, amount: {1}";
        public const string PaymentConfirmationAttempt = "Payment confirmation attempt for reference: {0}";
        public const string WebhookProcessingAttempt = "Webhook processing attempt for reference: {0}";
        public const string TransactionRetrievalAttempt = "Transaction retrieval attempt for user ID: {0}";

        // Payout Messages
        public const string PayoutInitiated = "Payout initiated successfully.";
        public const string PayoutInitiationFailed = "Payout initiation failed.";
        public const string PayoutCompleted = "Payout completed successfully.";
        public const string PayoutFailed = "Payout failed.";
        public const string PayoutCancelled = "Payout was cancelled.";
        public const string PayoutNotFound = "Payout transaction not found.";
        public const string PayoutAlreadyProcessed = "Payout has already been processed.";
        public const string InvalidPayoutAmount = "Invalid payout amount. Amount must be greater than zero.";
        public const string InvalidAccountNumber = "Invalid account number.";
        public const string InvalidBankCode = "Invalid bank code.";
        public const string AccountNumberRequired = "Account number is required.";
        public const string BankCodeRequired = "Bank code is required.";
        public const string AccountNameRequired = "Account name is required.";
        public const string NarrationTooLong = "Narration cannot exceed 500 characters.";
        public const string DuplicatePayoutReference = "A payout with this transaction reference already exists.";

        // Account Enquiry Messages
        public const string AccountVerified = "Account verified successfully.";
        public const string AccountVerificationFailed = "Account verification failed.";
        public const string AccountNotFound = "Account not found.";
        public const string AccountEnquiryError = "Error occurred during account enquiry.";

        // Authorization Messages
        public const string UnauthorizedPayoutAccess = "You are not authorized to initiate payouts.";
        public const string AdminOrFinanceRoleRequired = "Admin or Finance role is required for this operation.";
    }
} 