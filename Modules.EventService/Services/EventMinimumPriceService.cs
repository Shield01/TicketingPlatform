using Microsoft.Extensions.Logging;
using Modules.EventService.Repositories;
using Modules.TicketService.Repositories;
using Shared.Kernel.Interfaces;

namespace Modules.EventService.Services
{
    /// <summary>
    /// Service implementation for updating event minimum prices based on ticket tier information.
    /// </summary>
    public class EventMinimumPriceService : IEventMinimumPriceService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITicketTierRepository _ticketTierRepository;
        private readonly ILogger<EventMinimumPriceService> _logger;

        /// <summary>
        /// Initializes a new instance of the EventMinimumPriceService.
        /// </summary>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="ticketTierRepository">The ticket tier repository.</param>
        /// <param name="logger">The logger instance.</param>
        public EventMinimumPriceService(
            IEventRepository eventRepository, 
            ITicketTierRepository ticketTierRepository, 
            ILogger<EventMinimumPriceService> logger)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _ticketTierRepository = ticketTierRepository ?? throw new ArgumentNullException(nameof(ticketTierRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Recalculates and updates the minimum price for an event based on its available ticket tiers.
        /// This method queries all ticket tiers for the event, finds the cheapest available tier,
        /// and updates the Event.MinimumPrice property.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>The updated minimum price, or null if no available tiers exist.</returns>
        public async Task<decimal?> RecalculateAndUpdateMinimumPriceAsync(Guid eventId)
        {
            try
            {
                _logger.LogInformation("Recalculating minimum price for event {EventId}", eventId);

                // Get the event
                var @event = await _eventRepository.GetEventByIdAsync(eventId);
                if (@event == null)
                {
                    _logger.LogWarning("Event {EventId} not found, cannot update minimum price", eventId);
                    return null;
                }

                // Get all ticket tiers for the event directly from repository
                var ticketTiers = await _ticketTierRepository.GetTicketTiersByEventIdAsync(eventId);
                
                // Filter to available tiers with capacity remaining
                var availableTiers = ticketTiers
                    .Where(t => t.IsAvailable && 
                               t.IsActive && 
                               t.SoldQuantity < t.MaxQuantity &&
                               (!t.SaleStartDate.HasValue || t.SaleStartDate.Value <= DateTime.UtcNow) &&
                               (!t.SaleEndDate.HasValue || t.SaleEndDate.Value >= DateTime.UtcNow))
                    .ToList();

                // Find the minimum price among available tiers
                decimal? newMinimumPrice = null;
                string? newMinimumPriceCurrency = null;
                
                if (availableTiers.Any())
                {
                    var cheapestTier = availableTiers.OrderBy(t => t.Price).First();
                    newMinimumPrice = cheapestTier.Price;
                    newMinimumPriceCurrency = cheapestTier.Currency;
                    
                    _logger.LogInformation("Found minimum price {MinPrice} {Currency} for event {EventId} from {TierCount} available tiers", 
                        newMinimumPrice, newMinimumPriceCurrency, eventId, availableTiers.Count);
                }
                else
                {
                    _logger.LogInformation("No available ticket tiers found for event {EventId}, setting minimum price to null", eventId);
                }

                // Update the event's minimum price and currency
                @event.MinimumPrice = newMinimumPrice;
                @event.MinimumPriceCurrency = newMinimumPriceCurrency;
                @event.UpdatedAt = DateTime.UtcNow;
                await _eventRepository.UpdateEventAsync(@event);

                _logger.LogInformation("Successfully updated minimum price for event {EventId} to {MinPrice} {Currency}", 
                    eventId, newMinimumPrice, newMinimumPriceCurrency ?? "N/A");
                
                return newMinimumPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating minimum price for event {EventId}", eventId);
                throw;
            }
        }

        /// <summary>
        /// Updates the event's minimum price if the provided price is lower than the current minimum.
        /// This is an optimized method for when a new tier is created - it avoids querying all tiers
        /// if the new tier price is not lower than the existing minimum.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="newTierPrice">The price of the newly created tier.</param>
        /// <param name="currency">The currency of the new tier.</param>
        /// <returns>The updated minimum price.</returns>
        public async Task<decimal?> UpdateMinimumPriceIfLowerAsync(Guid eventId, decimal newTierPrice, string currency)
        {
            try
            {
                _logger.LogDebug("Checking if new tier price {Price} {Currency} is lower than current minimum for event {EventId}", 
                    newTierPrice, currency, eventId);

                // Get the event
                var @event = await _eventRepository.GetEventByIdAsync(eventId);
                if (@event == null)
                {
                    _logger.LogWarning("Event {EventId} not found, cannot update minimum price", eventId);
                    return null;
                }

                // Check if we need to update
                bool shouldUpdate = false;
                
                if (@event.MinimumPrice == null)
                {
                    // No existing minimum, set it to the new tier price
                    shouldUpdate = true;
                    _logger.LogInformation("Event {EventId} has no minimum price, setting to {Price} {Currency}", 
                        eventId, newTierPrice, currency);
                }
                else if (newTierPrice < @event.MinimumPrice.Value)
                {
                    // New tier is cheaper, update
                    shouldUpdate = true;
                    _logger.LogInformation("New tier price {NewPrice} {NewCurrency} is lower than current minimum {OldPrice} {OldCurrency} for event {EventId}", 
                        newTierPrice, currency, @event.MinimumPrice.Value, @event.MinimumPriceCurrency ?? "N/A", eventId);
                }
                else
                {
                    _logger.LogDebug("New tier price {NewPrice} {NewCurrency} is not lower than current minimum {OldPrice} {OldCurrency} for event {EventId}, no update needed", 
                        newTierPrice, currency, @event.MinimumPrice.Value, @event.MinimumPriceCurrency ?? "N/A", eventId);
                }

                if (shouldUpdate)
                {
                    @event.MinimumPrice = newTierPrice;
                    @event.MinimumPriceCurrency = currency;
                    @event.UpdatedAt = DateTime.UtcNow;
                    await _eventRepository.UpdateEventAsync(@event);
                    _logger.LogInformation("Updated minimum price for event {EventId} to {Price} {Currency}", 
                        eventId, newTierPrice, currency);
                }

                return @event.MinimumPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating minimum price for event {EventId} with new tier price {Price} {Currency}", 
                    eventId, newTierPrice, currency);
                throw;
            }
        }
    }
}

