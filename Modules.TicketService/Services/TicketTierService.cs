using Microsoft.Extensions.Logging;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Shared.Kernel.Interfaces;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service implementation for ticket tier operations.
    /// </summary>
    public class TicketTierService : ITicketTierService
    {
        private readonly ITicketTierRepository _ticketTierRepository;
        private readonly IEventMinimumPriceService _eventMinimumPriceService;
        private readonly ILogger<TicketTierService> _logger;

        /// <summary>
        /// Initializes a new instance of the TicketTierService class.
        /// </summary>
        /// <param name="ticketTierRepository">The ticket tier repository.</param>
        /// <param name="eventMinimumPriceService">The event minimum price service.</param>
        /// <param name="logger">The logger.</param>
        public TicketTierService(
            ITicketTierRepository ticketTierRepository, 
            IEventMinimumPriceService eventMinimumPriceService,
            ILogger<TicketTierService> logger)
        {
            _ticketTierRepository = ticketTierRepository;
            _eventMinimumPriceService = eventMinimumPriceService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<TicketTierResponse> CreateTicketTierAsync(Guid eventId, CreateTicketTierRequest request, Guid userId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (eventId == Guid.Empty)
                throw new ArgumentException("Event ID cannot be empty.", nameof(eventId));

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            _logger.LogInformation("Creating ticket tier '{TierName}' for event {EventId} by user {UserId}", 
                request.Name, eventId, userId);

            // Validate request
            ValidateCreateTicketTierRequest(request);

            // Check for duplicate tier name in the same event
            var nameExists = await _ticketTierRepository.TierNameExistsForEventAsync(eventId, request.Name);
            if (nameExists)
            {
                throw new InvalidOperationException($"A ticket tier with the name '{request.Name}' already exists for this event.");
            }

            // Validate sale dates
            ValidateSaleDates(request.SaleStartDate, request.SaleEndDate);

            // Create the ticket tier entity
            var ticketTier = new TicketTier
            {
                EventId = eventId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Price = request.Price,
                Currency = request.Currency,
                MaxQuantity = request.MaxQuantity,
                SoldQuantity = 0,
                IsAvailable = request.IsAvailable,
                SaleStartDate = request.SaleStartDate,
                SaleEndDate = request.SaleEndDate,
                IsActive = true
            };

            try
            {
                var createdTier = await _ticketTierRepository.CreateTicketTierAsync(ticketTier);
                
                _logger.LogInformation("Successfully created ticket tier {TierId} for event {EventId}", 
                    createdTier.Id, eventId);

                // Update event minimum price if this tier is available and cheaper
                if (createdTier.IsAvailable)
                {
                    try
                    {
                        await _eventMinimumPriceService.UpdateMinimumPriceIfLowerAsync(eventId, createdTier.Price);
                        _logger.LogDebug("Updated minimum price for event {EventId} after creating tier {TierId}", 
                            eventId, createdTier.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update minimum price for event {EventId}, but tier was created successfully", eventId);
                        // Don't throw - tier creation succeeded, minimum price update is secondary
                    }
                }

                return MapToTicketTierResponse(createdTier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ticket tier '{TierName}' for event {EventId}", 
                    request.Name, eventId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<TicketTierResponse> UpdateTicketTierAsync(Guid tierId, CreateTicketTierRequest request, Guid userId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (tierId == Guid.Empty)
                throw new ArgumentException("Tier ID cannot be empty.", nameof(tierId));

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            _logger.LogInformation("Updating ticket tier {TierId} by user {UserId}", tierId, userId);

            // Get existing tier
            var existingTier = await _ticketTierRepository.GetTicketTierByIdAsync(tierId);
            if (existingTier == null)
            {
                throw new InvalidOperationException("Ticket tier not found.");
            }

            // Validate request
            ValidateCreateTicketTierRequest(request);

            // Check for duplicate tier name in the same event (excluding current tier)
            var nameExists = await _ticketTierRepository.TierNameExistsForEventAsync(existingTier.EventId, request.Name, tierId);
            if (nameExists)
            {
                throw new InvalidOperationException($"A ticket tier with the name '{request.Name}' already exists for this event.");
            }

            // Validate sale dates
            ValidateSaleDates(request.SaleStartDate, request.SaleEndDate);

            // Validate that MaxQuantity is not less than SoldQuantity
            if (request.MaxQuantity < existingTier.SoldQuantity)
            {
                throw new InvalidOperationException($"Maximum quantity ({request.MaxQuantity}) cannot be less than already sold quantity ({existingTier.SoldQuantity}).");
            }

            // Update the tier
            existingTier.Name = request.Name.Trim();
            existingTier.Description = request.Description?.Trim();
            existingTier.Price = request.Price;
            existingTier.Currency = request.Currency;
            existingTier.MaxQuantity = request.MaxQuantity;
            existingTier.IsAvailable = request.IsAvailable;
            existingTier.SaleStartDate = request.SaleStartDate;
            existingTier.SaleEndDate = request.SaleEndDate;

            try
            {
                var updatedTier = await _ticketTierRepository.UpdateTicketTierAsync(existingTier);
                
                _logger.LogInformation("Successfully updated ticket tier {TierId}", tierId);

                // Recalculate event minimum price as tier details changed
                try
                {
                    await _eventMinimumPriceService.RecalculateAndUpdateMinimumPriceAsync(existingTier.EventId);
                    _logger.LogDebug("Recalculated minimum price for event {EventId} after updating tier {TierId}", 
                        existingTier.EventId, tierId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to recalculate minimum price for event {EventId}, but tier was updated successfully", existingTier.EventId);
                    // Don't throw - tier update succeeded, minimum price update is secondary
                }

                return MapToTicketTierResponse(updatedTier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update ticket tier {TierId}", tierId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TicketTierResponse>> GetEventTicketTiersAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Event ID cannot be empty.", nameof(eventId));

            _logger.LogInformation("Getting ticket tiers for event {EventId}", eventId);

            var tiers = await _ticketTierRepository.GetTicketTiersByEventIdAsync(eventId);
            return tiers.Select(MapToTicketTierResponse);
        }

        /// <inheritdoc />
        public async Task<TicketTierResponse?> GetTicketTierByIdAsync(Guid tierId)
        {
            if (tierId == Guid.Empty)
                return null;

            var tier = await _ticketTierRepository.GetTicketTierByIdAsync(tierId);
            return tier != null ? MapToTicketTierResponse(tier) : null;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTicketTierAsync(Guid tierId, Guid userId)
        {
            if (tierId == Guid.Empty)
                throw new ArgumentException("Tier ID cannot be empty.", nameof(tierId));

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            _logger.LogInformation("Deleting ticket tier {TierId} by user {UserId}", tierId, userId);

            try
            {
                // Get the tier before deletion to know which event to update
                var tier = await _ticketTierRepository.GetTicketTierByIdAsync(tierId);
                Guid? eventId = tier?.EventId;

                var result = await _ticketTierRepository.DeleteTicketTierAsync(tierId);
                
                if (result)
                {
                    _logger.LogInformation("Successfully deleted ticket tier {TierId}", tierId);

                    // Recalculate event minimum price after deletion
                    if (eventId.HasValue)
                    {
                        try
                        {
                            await _eventMinimumPriceService.RecalculateAndUpdateMinimumPriceAsync(eventId.Value);
                            _logger.LogDebug("Recalculated minimum price for event {EventId} after deleting tier {TierId}", 
                                eventId.Value, tierId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to recalculate minimum price for event {EventId}, but tier was deleted successfully", eventId.Value);
                            // Don't throw - tier deletion succeeded, minimum price update is secondary
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to delete ticket tier {TierId} - tier not found", tierId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket tier {TierId}", tierId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> CanUserManageEventTicketsAsync(Guid eventId, Guid userId)
        {
            if (eventId == Guid.Empty || userId == Guid.Empty)
                return false;

            // Note: In a production system, this would integrate with EventService
            // to verify event ownership. For now, we delegate this to the controller
            // layer where RBAC and event ownership can be validated.
            return await _ticketTierRepository.IsUserEventOrganizerAsync(eventId, userId);
        }

        /// <summary>
        /// Validates the create ticket tier request.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        private static void ValidateCreateTicketTierRequest(CreateTicketTierRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Tier name is required.", nameof(request));

            if (request.Name.Length > 100)
                throw new ArgumentException("Tier name cannot exceed 100 characters.", nameof(request));

            if (request.Price <= 0)
                throw new ArgumentException("Price must be greater than 0.", nameof(request));

            if (request.MaxQuantity < 0)
                throw new ArgumentException("Maximum quantity must be 0 or greater.", nameof(request));

            if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Currency))
                throw new ArgumentException("Currency is required.", nameof(request));

            if (request.Currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-character code.", nameof(request));
        }

        /// <summary>
        /// Validates sale start and end dates.
        /// </summary>
        /// <param name="saleStartDate">The sale start date.</param>
        /// <param name="saleEndDate">The sale end date.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        private static void ValidateSaleDates(DateTime? saleStartDate, DateTime? saleEndDate)
        {
            if (saleStartDate.HasValue && saleEndDate.HasValue)
            {
                if (saleEndDate <= saleStartDate)
                {
                    throw new ArgumentException("Sale end date must be after sale start date.");
                }
            }
        }

        /// <summary>
        /// Maps a TicketTier entity to a TicketTierResponse DTO.
        /// </summary>
        /// <param name="ticketTier">The ticket tier entity.</param>
        /// <returns>The ticket tier response DTO.</returns>
        private static TicketTierResponse MapToTicketTierResponse(TicketTier ticketTier)
        {
            return new TicketTierResponse
            {
                Id = ticketTier.Id,
                EventId = ticketTier.EventId,
                Name = ticketTier.Name,
                Description = ticketTier.Description,
                Price = ticketTier.Price,
                Currency = ticketTier.Currency,
                MaxQuantity = ticketTier.MaxQuantity,
                SoldQuantity = ticketTier.SoldQuantity,
                IsAvailable = ticketTier.IsAvailable,
                SaleStartDate = ticketTier.SaleStartDate,
                SaleEndDate = ticketTier.SaleEndDate,
                IsActive = ticketTier.IsActive,
                CreatedAt = ticketTier.CreatedAt,
                UpdatedAt = ticketTier.UpdatedAt
            };
        }
    }
}
