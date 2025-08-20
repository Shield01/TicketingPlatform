using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.EventService.Data;
using Modules.EventService.Models;
using Modules.EventService.DTOs;

namespace Modules.EventService.Repositories
{
    /// <summary>
    /// Repository implementation for Event entity operations.
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly EventServiceDbContext _context;
        private readonly ILogger<EventRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the EventRepository.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public EventRepository(EventServiceDbContext context, ILogger<EventRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new event asynchronously.
        /// </summary>
        /// <param name="event">The event to create.</param>
        /// <returns>The created event with generated ID.</returns>
        public async Task<Event> CreateEventAsync(Event @event)
        {
            try
            {
                _logger.LogInformation("Creating new event: {EventTitle}", @event.Title);
                
                @event.Id = Guid.NewGuid();
                @event.CreatedAt = DateTime.UtcNow;
                @event.UpdatedAt = DateTime.UtcNow;
                
                _context.Events.Add(@event);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Event created successfully with ID: {EventId}", @event.Id);
                return @event;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {EventTitle}", @event.Title);
                throw;
            }
        }

        /// <summary>
        /// Gets an event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event if found, null otherwise.</returns>
        public async Task<Event?> GetEventByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Getting event by ID: {EventId}", id);
                
                return await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event by ID: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets a public event by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event if found and published, null otherwise.</returns>
        public async Task<Event?> GetPublicEventByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Getting public event by ID: {EventId}", id);
                
                return await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == id && e.IsPublic && e.IsPublished && e.Status == "Published" && e.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public event by ID: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all events for a specific organizer asynchronously.
        /// </summary>
        /// <param name="organizerId">The unique identifier of the organizer.</param>
        /// <returns>A list of events created by the organizer.</returns>
        public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(Guid organizerId)
        {
            try
            {
                _logger.LogDebug("Getting events for organizer: {OrganizerId}", organizerId);
                
                return await _context.Events
                    .Where(e => e.OrganizerId == organizerId)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for organizer: {OrganizerId}", organizerId);
                throw;
            }
        }

        /// <summary>
        /// Gets all public events asynchronously.
        /// </summary>
        /// <returns>A list of all public events.</returns>
        public async Task<IEnumerable<Event>> GetPublicEventsAsync()
        {
            try
            {
                _logger.LogDebug("Getting public events");
                
                return await _context.Events
                    .Where(e => e.IsPublic && e.IsPublished && e.Status == "Published" && e.IsActive)
                    .OrderBy(e => e.StartDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public events");
                throw;
            }
        }

        /// <summary>
        /// Gets filtered public events asynchronously with pagination.
        /// </summary>
        /// <param name="filter">The filter criteria for events.</param>
        /// <returns>A tuple containing the filtered events and total count.</returns>
        public async Task<(IEnumerable<Event> Events, int TotalCount)> GetFilteredPublicEventsAsync(EventFilterRequest filter)
        {
            try
            {
                _logger.LogDebug("Getting filtered public events with filter: {@Filter}", filter);
                
                // Build the base query
                var baseQuery = _context.Events.AsQueryable();

                // Apply base filters - only show published public events
                baseQuery = baseQuery.Where(e => e.IsPublic && e.IsPublished && e.Status == "Published" && e.IsActive);

                // Apply additional filters
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    baseQuery = baseQuery.Where(e => e.Status == filter.Status);
                }

                if (!string.IsNullOrWhiteSpace(filter.Category))
                {
                    baseQuery = baseQuery.Where(e => e.Category == filter.Category);
                }

                if (!string.IsNullOrWhiteSpace(filter.EventType))
                {
                    var now = DateTime.UtcNow;
                    switch (filter.EventType.ToLower())
                    {
                        case "upcoming":
                            baseQuery = baseQuery.Where(e => e.StartDate > now);
                            break;
                        case "past":
                            baseQuery = baseQuery.Where(e => e.EndDate < now);
                            break;
                        case "all":
                        default:
                            // No additional filtering
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                {
                    var keyword = filter.SearchKeyword.ToLower();
                    baseQuery = baseQuery.Where(e => 
                        e.Title.ToLower().Contains(keyword) ||
                        e.Description.ToLower().Contains(keyword) ||
                        e.Location.ToLower().Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(filter.Location))
                {
                    var location = filter.Location.ToLower();
                    baseQuery = baseQuery.Where(e => e.Location.ToLower().Contains(location));
                }

                if (filter.StartDateFrom.HasValue)
                {
                    baseQuery = baseQuery.Where(e => e.StartDate >= filter.StartDateFrom.Value);
                }

                if (filter.StartDateTo.HasValue)
                {
                    baseQuery = baseQuery.Where(e => e.StartDate <= filter.StartDateTo.Value);
                }

                // Get total count before sorting and pagination
                var totalCount = await baseQuery.CountAsync();

                // Apply sorting
                IQueryable<Event> sortedQuery;
                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                {
                    sortedQuery = filter.SortBy.ToLower() switch
                    {
                        "title" => filter.SortDirection?.ToLower() == "desc" 
                            ? baseQuery.OrderByDescending(e => e.Title)
                            : baseQuery.OrderBy(e => e.Title),
                        "startdate" => filter.SortDirection?.ToLower() == "desc"
                            ? baseQuery.OrderByDescending(e => e.StartDate)
                            : baseQuery.OrderBy(e => e.StartDate),
                        "createdat" => filter.SortDirection?.ToLower() == "desc"
                            ? baseQuery.OrderByDescending(e => e.CreatedAt)
                            : baseQuery.OrderBy(e => e.CreatedAt),
                        _ => baseQuery.OrderBy(e => e.StartDate)
                    };
                }
                else
                {
                    sortedQuery = baseQuery.OrderBy(e => e.StartDate);
                }

                // Apply pagination
                var skip = (filter.Page - 1) * filter.PageSize;
                var events = await sortedQuery
                    .Skip(skip)
                    .Take(filter.PageSize)
                    .ToListAsync();

                _logger.LogDebug("Filtered public events retrieved. Count: {Count}, Total: {TotalCount}", events.Count, totalCount);
                return (events, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered public events");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing event asynchronously.
        /// </summary>
        /// <param name="event">The event to update.</param>
        /// <returns>The updated event.</returns>
        public async Task<Event> UpdateEventAsync(Event @event)
        {
            try
            {
                _logger.LogInformation("Updating event: {EventId}", @event.Id);
                
                @event.UpdatedAt = DateTime.UtcNow;
                
                _context.Events.Update(@event);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Event updated successfully: {EventId}", @event.Id);
                return @event;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event: {EventId}", @event.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes an event asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event to delete.</param>
        /// <returns>True if the event was deleted, false if not found.</returns>
        public async Task<bool> DeleteEventAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting event: {EventId}", id);
                
                var @event = await _context.Events.FindAsync(id);
                if (@event == null)
                {
                    _logger.LogWarning("Event not found for deletion: {EventId}", id);
                    return false;
                }
                
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Event deleted successfully: {EventId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Checks if an event exists by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>True if the event exists, false otherwise.</returns>
        public async Task<bool> EventExistsAsync(Guid id)
        {
            try
            {
                return await _context.Events.AnyAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if event exists: {EventId}", id);
                throw;
            }
        }

        /// <summary>
        /// Checks if a user is the organizer of an event asynchronously.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>True if the user is the organizer, false otherwise.</returns>
        public async Task<bool> IsUserOrganizerAsync(Guid eventId, Guid userId)
        {
            try
            {
                return await _context.Events
                    .AnyAsync(e => e.Id == eventId && e.OrganizerId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is organizer: EventId={EventId}, UserId={UserId}", eventId, userId);
                throw;
            }
        }

        /// <summary>
        /// Gets events created by teams where the user is a member.
        /// </summary>
        /// <param name="userTeamIds">The team IDs where the user is a member.</param>
        /// <param name="filter">Optional filter parameters.</param>
        /// <returns>A tuple containing events and total count.</returns>
        public async Task<(IEnumerable<Event> Events, int TotalCount)> GetTeamEventsAsync(IEnumerable<Guid> userTeamIds, EventFilterRequest? filter = null)
        {
            try
            {
                _logger.LogInformation("Getting team events for user teams: {TeamIds}", string.Join(", ", userTeamIds));

                var baseQuery = _context.Events
                    .Include(e => e.Team)
                    .Where(e => e.IsActive && userTeamIds.Contains(e.TeamId ?? Guid.Empty));

                // Apply filters if provided
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        baseQuery = baseQuery.Where(e => e.Status == filter.Status);
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Category))
                    {
                        baseQuery = baseQuery.Where(e => e.Category == filter.Category);
                    }

                    if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                    {
                        var keyword = filter.SearchKeyword.ToLower();
                        baseQuery = baseQuery.Where(e => 
                            e.Title.ToLower().Contains(keyword) || 
                            e.Description.ToLower().Contains(keyword) ||
                            e.Location.ToLower().Contains(keyword));
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Location))
                    {
                        baseQuery = baseQuery.Where(e => e.Location.ToLower().Contains(filter.Location.ToLower()));
                    }

                    if (filter.StartDateFrom.HasValue)
                    {
                        baseQuery = baseQuery.Where(e => e.StartDate >= filter.StartDateFrom.Value);
                    }

                    if (filter.StartDateTo.HasValue)
                    {
                        baseQuery = baseQuery.Where(e => e.StartDate <= filter.StartDateTo.Value);
                    }
                }

                // Get total count
                var totalCount = await baseQuery.CountAsync();

                // Apply sorting
                IQueryable<Event> sortedQuery;
                if (filter?.SortBy?.ToLower() == "startdate")
                {
                    sortedQuery = filter.SortDirection?.ToLower() == "desc" 
                        ? baseQuery.OrderByDescending(e => e.StartDate)
                        : baseQuery.OrderBy(e => e.StartDate);
                }
                else if (filter?.SortBy?.ToLower() == "title")
                {
                    sortedQuery = filter.SortDirection?.ToLower() == "desc"
                        ? baseQuery.OrderByDescending(e => e.Title)
                        : baseQuery.OrderBy(e => e.Title);
                }
                else
                {
                    sortedQuery = baseQuery.OrderByDescending(e => e.CreatedAt);
                }

                // Apply pagination
                var skip = ((filter?.Page ?? 1) - 1) * (filter?.PageSize ?? 10);
                var events = await sortedQuery
                    .Skip(skip)
                    .Take(filter?.PageSize ?? 10)
                    .ToListAsync();

                _logger.LogDebug("Team events retrieved. Count: {Count}, Total: {TotalCount}", events.Count, totalCount);
                return (events, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team events");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific event by ID if it belongs to one of the user's teams.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userTeamIds">The team IDs where the user is a member.</param>
        /// <returns>The event if found and accessible, null otherwise.</returns>
        public async Task<Event?> GetTeamEventByIdAsync(Guid eventId, IEnumerable<Guid> userTeamIds)
        {
            try
            {
                _logger.LogInformation("Getting team event by ID: {EventId} for teams: {TeamIds}", eventId, string.Join(", ", userTeamIds));

                var @event = await _context.Events
                    .Include(e => e.Team)
                    .FirstOrDefaultAsync(e => e.Id == eventId && 
                                            e.IsActive && 
                                            userTeamIds.Contains(e.TeamId ?? Guid.Empty));

                if (@event == null)
                {
                    _logger.LogWarning("Team event not found or not accessible: {EventId}", eventId);
                }

                return @event;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team event by ID: {EventId}", eventId);
                throw;
            }
        }

        /// <summary>
        /// Checks if a user is a member of the team that created an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userTeamIds">The team IDs where the user is a member.</param>
        /// <returns>True if the user is a member of the team that created the event, false otherwise.</returns>
        public async Task<bool> IsUserTeamMemberAsync(Guid eventId, IEnumerable<Guid> userTeamIds)
        {
            try
            {
                return await _context.Events
                    .AnyAsync(e => e.Id == eventId && 
                                  e.IsActive && 
                                  userTeamIds.Contains(e.TeamId ?? Guid.Empty));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is team member: EventId={EventId}", eventId);
                throw;
            }
        }
    }
} 