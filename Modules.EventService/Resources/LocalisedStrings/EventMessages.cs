namespace Modules.EventService.Resources.LocalisedStrings
{
    /// <summary>
    /// Event service specific messages and string constants
    /// </summary>
    public static class EventMessages
    {
        // Event Creation Messages
        public const string EventCreated = "Event created successfully.";
        public const string EventCreationFailed = "Event creation failed.";
        public const string EventTitleRequired = "Event title is required.";
        public const string EventDescriptionRequired = "Event description is required.";
        public const string EventDateRequired = "Event date is required.";
        public const string EventLocationRequired = "Event location is required.";
        public const string InvalidEventDate = "Event date must be in the future.";

        // Event Update Messages
        public const string EventUpdated = "Event updated successfully.";
        public const string EventUpdateFailed = "Event update failed.";
        public const string EventNotFound = "Event not found.";
        public const string EventCannotBeModified = "Event cannot be modified as it has been published.";

        // Event Publishing Messages
        public const string EventPublished = "Event published successfully.";
        public const string EventPublishFailed = "Event publishing failed.";
        public const string EventUnpublished = "Event unpublished successfully.";
        public const string EventAlreadyPublished = "Event is already published.";
        public const string EventNotPublished = "Event is not published.";

        // Event Retrieval Messages
        public const string EventsRetrieved = "Events retrieved successfully.";
        public const string EventRetrieved = "Event retrieved successfully.";
        public const string NoEventsFound = "No events found.";
        public const string EventDeleted = "Event deleted successfully.";
        public const string EventDeletionFailed = "Event deletion failed.";

        // Event Visibility Messages
        public const string EventVisibilityUpdated = "Event visibility updated successfully.";
        public const string InvalidVisibility = "Invalid visibility setting. Must be 'public' or 'private'.";

        // Event Categories and Tags
        public const string CategoryAdded = "Category added successfully.";
        public const string TagAdded = "Tag added successfully.";
        public const string InvalidCategory = "Invalid category specified.";
        public const string InvalidTag = "Invalid tag specified.";

        // Validation Messages
        public const string TitleTooLong = "Event title cannot exceed 200 characters.";
        public const string DescriptionTooLong = "Event description cannot exceed 2000 characters.";
        public const string LocationTooLong = "Event location cannot exceed 500 characters.";
        public const string InvalidCapacity = "Event capacity must be a positive number.";
        public const string InvalidPrice = "Event price must be a non-negative number.";

        // Log Messages
        public const string EventCreationAttempt = "Event creation attempt for title: {0}";
        public const string EventUpdateAttempt = "Event update attempt for event ID: {0}";
        public const string EventPublishAttempt = "Event publish attempt for event ID: {0}";
        public const string EventRetrievalAttempt = "Event retrieval attempt for event ID: {0}";
    }
} 