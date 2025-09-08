using Modules.TicketService.DTOs;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service interface for ticket tier operations.
    /// </summary>
    public interface ITicketTierService
    {
        /// <summary>
        /// Creates a new ticket tier for an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="request">The ticket tier creation request.</param>
        /// <param name="userId">The ID of the user creating the tier (for authorization).</param>
        /// <returns>The created ticket tier response.</returns>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authorized.</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated.</exception>
        Task<TicketTierResponse> CreateTicketTierAsync(Guid eventId, CreateTicketTierRequest request, Guid userId);

        /// <summary>
        /// Updates an existing ticket tier.
        /// </summary>
        /// <param name="tierID">The ticket tier ID.</param>
        /// <param name="request">The ticket tier update request.</param>
        /// <param name="userId">The ID of the user updating the tier (for authorization).</param>
        /// <returns>The updated ticket tier response.</returns>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authorized.</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated.</exception>
        Task<TicketTierResponse> UpdateTicketTierAsync(Guid tierID, CreateTicketTierRequest request, Guid userId);

        /// <summary>
        /// Gets all ticket tiers for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>A list of ticket tier responses.</returns>
        Task<IEnumerable<TicketTierResponse>> GetEventTicketTiersAsync(Guid eventId);

        /// <summary>
        /// Gets a specific ticket tier by ID.
        /// </summary>
        /// <param name="tierId">The ticket tier ID.</param>
        /// <returns>The ticket tier response if found, null otherwise.</returns>
        Task<TicketTierResponse?> GetTicketTierByIdAsync(Guid tierId);

        /// <summary>
        /// Deletes a ticket tier.
        /// </summary>
        /// <param name="tierId">The ticket tier ID.</param>
        /// <param name="userId">The ID of the user deleting the tier (for authorization).</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authorized.</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated.</exception>
        Task<bool> DeleteTicketTierAsync(Guid tierId, Guid userId);

        /// <summary>
        /// Validates that a user can manage ticket tiers for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the user can manage tiers, false otherwise.</returns>
        Task<bool> CanUserManageEventTicketsAsync(Guid eventId, Guid userId);
    }
}
