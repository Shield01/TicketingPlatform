using Modules.TicketService.Models;

namespace Modules.TicketService.Repositories
{
    /// <summary>
    /// Repository interface for ticket tier operations.
    /// </summary>
    public interface ITicketTierRepository
    {
        /// <summary>
        /// Creates a new ticket tier.
        /// </summary>
        /// <param name="ticketTier">The ticket tier to create.</param>
        /// <returns>The created ticket tier.</returns>
        Task<TicketTier> CreateTicketTierAsync(TicketTier ticketTier);

        /// <summary>
        /// Updates an existing ticket tier.
        /// </summary>
        /// <param name="ticketTier">The ticket tier to update.</param>
        /// <returns>The updated ticket tier.</returns>
        Task<TicketTier> UpdateTicketTierAsync(TicketTier ticketTier);

        /// <summary>
        /// Gets a ticket tier by its ID.
        /// </summary>
        /// <param name="id">The ticket tier ID.</param>
        /// <returns>The ticket tier if found, null otherwise.</returns>
        Task<TicketTier?> GetTicketTierByIdAsync(Guid id);

        /// <summary>
        /// Gets all ticket tiers for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>A list of ticket tiers for the event.</returns>
        Task<IEnumerable<TicketTier>> GetTicketTiersByEventIdAsync(Guid eventId);

        /// <summary>
        /// Checks if a ticket tier name already exists for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="name">The tier name to check.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name exists, false otherwise.</returns>
        Task<bool> TierNameExistsForEventAsync(Guid eventId, string name, Guid? excludeId = null);

        /// <summary>
        /// Deletes a ticket tier.
        /// </summary>
        /// <param name="id">The ticket tier ID.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        Task<bool> DeleteTicketTierAsync(Guid id);

        /// <summary>
        /// Checks if the event exists in the system.
        /// </summary>
        /// <param name="eventId">The event ID to check.</param>
        /// <returns>True if the event exists, false otherwise.</returns>
        Task<bool> EventExistsAsync(Guid eventId);

        /// <summary>
        /// Checks if the user is the organizer of the specified event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the user is the organizer, false otherwise.</returns>
        Task<bool> IsUserEventOrganizerAsync(Guid eventId, Guid userId);
    }
}
