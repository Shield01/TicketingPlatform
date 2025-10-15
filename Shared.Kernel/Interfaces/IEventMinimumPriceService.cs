namespace Shared.Kernel.Interfaces
{
    /// <summary>
    /// Interface for updating event minimum prices across modules.
    /// This service allows TicketService to update the cached MinimumPrice on Event entities.
    /// </summary>
    public interface IEventMinimumPriceService
    {
        /// <summary>
        /// Recalculates and updates the minimum price for an event based on its available ticket tiers.
        /// This should be called whenever:
        /// - A ticket tier is created
        /// - A ticket tier is updated (price or availability changes)
        /// - A ticket tier sells out
        /// - A ticket tier is deleted
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>The updated minimum price, or null if no available tiers exist.</returns>
        Task<decimal?> RecalculateAndUpdateMinimumPriceAsync(Guid eventId);

        /// <summary>
        /// Updates the event's minimum price if the provided price is lower than the current minimum.
        /// This is an optimized method for when a new tier is created.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="newTierPrice">The price of the newly created tier.</param>
        /// <returns>The updated minimum price.</returns>
        Task<decimal?> UpdateMinimumPriceIfLowerAsync(Guid eventId, decimal newTierPrice);
    }
}

