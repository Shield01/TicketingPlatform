namespace Modules.TicketService.Resources.LocalisedStrings
{
    /// <summary>
    /// Ticket service specific messages and string constants
    /// </summary>
    public static class TicketMessages
    {
        // Ticket Creation Messages
        public const string TicketTiersCreated = "Ticket tiers created successfully.";
        public const string TicketCreationFailed = "Ticket creation failed.";
        public const string EventIdRequired = "Event ID is required.";
        public const string TicketTiersRequired = "At least one ticket tier is required.";
        public const string InvalidTicketTier = "Invalid ticket tier configuration.";

        // Ticket Tier Messages
        public const string VipTier = "VIP";
        public const string RegularTier = "Regular";
        public const string EarlyBirdTier = "Early Bird";
        public const string TierNameRequired = "Ticket tier name is required.";
        public const string TierPriceRequired = "Ticket tier price is required.";
        public const string TierQuantityRequired = "Ticket tier quantity is required.";
        public const string InvalidTierPrice = "Ticket tier price must be a non-negative number.";
        public const string InvalidTierQuantity = "Ticket tier quantity must be a positive number.";

        // Ticket Verification Messages
        public const string TicketVerified = "Ticket verified successfully.";
        public const string TicketVerificationFailed = "Ticket verification failed.";
        public const string TicketNotFound = "Ticket not found.";
        public const string TicketAlreadyUsed = "Ticket has already been used.";
        public const string TicketExpired = "Ticket has expired.";
        public const string InvalidTicketCode = "Invalid ticket code.";
        public const string TicketCodeRequired = "Ticket code is required.";

        // Ticket Retrieval Messages
        public const string TicketsRetrieved = "Tickets retrieved successfully.";
        public const string TicketRetrieved = "Ticket retrieved successfully.";
        public const string NoTicketsFound = "No tickets found for this event.";
        public const string EventTicketsRetrieved = "Event tickets retrieved successfully.";

        // Ticket Encryption Messages
        public const string TicketEncryptionFailed = "Ticket encryption failed.";
        public const string TicketDecryptionFailed = "Ticket decryption failed.";
        public const string InvalidEncryptionKey = "Invalid encryption key.";

        // QR Code Messages
        public const string QrCodeGenerationFailed = "QR code generation failed.";
        public const string QrCodeInvalid = "Invalid QR code.";

        // Ticket Usage Tracking
        public const string TicketUsageRecorded = "Ticket usage recorded successfully.";
        public const string TicketUsageFailed = "Failed to record ticket usage.";
        public const string TicketRedemptionRecorded = "Ticket redemption recorded successfully.";

        // Validation Messages
        public const string InvalidEventId = "Invalid event ID provided.";
        public const string EventNotFound = "Event not found.";
        public const string TicketLimitExceeded = "Ticket purchase limit exceeded.";
        public const string InsufficientTickets = "Insufficient tickets available.";

        // Log Messages
        public const string TicketCreationAttempt = "Ticket creation attempt for event ID: {0}";
        public const string TicketVerificationAttempt = "Ticket verification attempt for code: {0}";
        public const string TicketRetrievalAttempt = "Ticket retrieval attempt for event ID: {0}";
        public const string QrCodeGenerationAttempt = "QR code generation attempt for ticket ID: {0}";
    }
} 