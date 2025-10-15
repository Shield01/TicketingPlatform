using Microsoft.Extensions.Logging;
using Modules.TicketService.DTOs;
using Modules.TicketService.Models;
using Modules.TicketService.Repositories;
using Shared.Kernel.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.Services
{
    /// <summary>
    /// Service implementation for ticket issuance operations.
    /// </summary>
    public class TicketIssueService : ITicketIssueService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IEmailService _emailService;
        private readonly IUserInfoService _userInfoService;
        private readonly IEventInfoService _eventInfoService;
        private readonly IEventMinimumPriceService _eventMinimumPriceService;
        private readonly ILogger<TicketIssueService> _logger;

        /// <summary>
        /// Initializes a new instance of the TicketIssueService.
        /// </summary>
        /// <param name="ticketRepository">The ticket repository.</param>
        /// <param name="qrCodeService">The QR code service.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="userInfoService">The user info service.</param>
        /// <param name="eventInfoService">The event info service.</param>
        /// <param name="eventMinimumPriceService">The event minimum price service.</param>
        /// <param name="logger">The logger instance.</param>
        public TicketIssueService(
            ITicketRepository ticketRepository, 
            IQRCodeService qrCodeService, 
            IEmailService emailService, 
            IUserInfoService userInfoService, 
            IEventInfoService eventInfoService, 
            IEventMinimumPriceService eventMinimumPriceService,
            ILogger<TicketIssueService> logger)
        {
            _ticketRepository = ticketRepository;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
            _userInfoService = userInfoService;
            _eventInfoService = eventInfoService;
            _eventMinimumPriceService = eventMinimumPriceService;
            _logger = logger;
        }

        /// <summary>
        /// Issues tickets after payment confirmation.
        /// </summary>
        /// <param name="request">The ticket issuance request.</param>
        /// <returns>The ticket issuance response with issued tickets.</returns>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated.</exception>
        public async Task<IssueTicketResponse> IssueTicketsAsync(IssueTicketRequest request)
        {
            _logger.LogInformation("Starting ticket issuance for user {UserId}, event {EventId}, tier {TicketTierId}, quantity {Quantity}", 
                request.UserId, request.EventId, request.TicketTierId, request.Quantity);

            // Auto-generate PaymentId if not provided (for testing purposes while PaymentService is not implemented)
            if (!request.PaymentId.HasValue || request.PaymentId.Value == Guid.Empty)
            {
                request.PaymentId = Guid.NewGuid();
                _logger.LogInformation("Auto-generated PaymentId {PaymentId} for testing purposes", request.PaymentId.Value);
            }

            // Validate request
            await ValidateTicketIssuanceRequestAsync(request);

            // Validate payment
            var isPaymentValid = await _ticketRepository.ValidatePaymentForTicketIssuanceAsync(request.PaymentId.Value);
            if (!isPaymentValid)
            {
                throw new InvalidOperationException($"Payment {request.PaymentId.Value} is not valid for ticket issuance.");
            }

            // Check ticket tier capacity
            var hasCapacity = await _ticketRepository.ValidateTicketTierCapacityAsync(request.TicketTierId, request.Quantity);
            if (!hasCapacity)
            {
                throw new InvalidOperationException("Insufficient ticket capacity for the requested quantity.");
            }

            // Get ticket tier information
            var ticketTier = await _ticketRepository.GetTicketTierAsync(request.TicketTierId);
            if (ticketTier == null)
            {
                throw new InvalidOperationException($"Ticket tier {request.TicketTierId} not found.");
            }

            try
            {
                // Create tickets
                var tickets = new List<Ticket>();
                for (int i = 0; i < request.Quantity; i++)
                {
                    var ticket = await CreateTicketAsync(request, ticketTier);
                    tickets.Add(ticket);
                }

                // Issue tickets
                var issuedTickets = await _ticketRepository.IssueMultipleTicketsAsync(tickets);

                // Update ticket tier sold quantity
                await _ticketRepository.UpdateTicketTierSoldQuantityAsync(request.TicketTierId, request.Quantity);

                // Check if tier is now sold out and recalculate minimum price
                var updatedTicketTier = await _ticketRepository.GetTicketTierAsync(request.TicketTierId);
                if (updatedTicketTier != null && updatedTicketTier.SoldQuantity >= updatedTicketTier.MaxQuantity)
                {
                    _logger.LogInformation("Ticket tier {TierId} is now sold out, recalculating minimum price for event {EventId}", 
                        request.TicketTierId, request.EventId);
                    
                    try
                    {
                        await _eventMinimumPriceService.RecalculateAndUpdateMinimumPriceAsync(request.EventId);
                        _logger.LogDebug("Recalculated minimum price for event {EventId} after tier {TierId} sold out", 
                            request.EventId, request.TicketTierId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to recalculate minimum price for event {EventId}, but tickets were issued successfully", request.EventId);
                        // Don't throw - ticket issuance succeeded, minimum price update is secondary
                    }
                }

                _logger.LogInformation("Successfully issued {Count} tickets for user {UserId}, payment {PaymentId}", 
                    request.Quantity, request.UserId, request.PaymentId);

                // Convert to response
                var ticketResponses = new List<TicketResponse>();
                foreach (var ticket in issuedTickets)
                {
                    var response = await ConvertToTicketResponseAsync(ticket, ticketTier);
                    ticketResponses.Add(response);
                }

                // Send email confirmation (fire and forget - don't block ticket issuance)
                // Capture user and event info within the request scope before background task
                var userInfo = await _userInfoService.GetUserInfoAsync(request.UserId);
                var eventInfo = await _eventInfoService.GetEventInfoAsync(request.EventId);
                _ = Task.Run(async () => await SendTicketConfirmationEmailAsync(ticketResponses, userInfo, eventInfo, ticketTier.Name));

                return new IssueTicketResponse
                {
                    Tickets = ticketResponses,
                    TicketsIssued = request.Quantity,
                    TotalPrice = request.Price * request.Quantity,
                    Currency = request.Currency,
                    PaymentId = request.PaymentId.Value,
                    IssuedAt = DateTime.UtcNow,
                    Message = $"Successfully issued {request.Quantity} ticket(s) for {ticketTier.Name}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing tickets for user {UserId}, payment {PaymentId}", request.UserId, request.PaymentId);
                throw new InvalidOperationException("Failed to issue tickets. Please contact support.", ex);
            }
        }

        /// <summary>
        /// Gets all tickets for a specific user with pagination and filtering.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="status">Optional status filter.</param>
        /// <returns>The user's tickets with pagination information.</returns>
        public async Task<UserTicketsResponse> GetUserTicketsAsync(Guid userId, int page = 1, int pageSize = 10, string? status = null)
        {
            _logger.LogInformation("Getting tickets for user {UserId}, page {Page}, pageSize {PageSize}, status {Status}", 
                userId, page, pageSize, status);

            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            try
            {
                var (tickets, totalCount) = await _ticketRepository.GetUserTicketsAsync(userId, page, pageSize, status);
                var statusCounts = await _ticketRepository.GetUserTicketStatusCountsAsync(userId);

                var ticketResponses = new List<TicketResponse>();
                foreach (var ticket in tickets)
                {
                    var ticketTier = ticket.TicketTier ?? await _ticketRepository.GetTicketTierAsync(ticket.TicketTierId);
                    var response = await ConvertToTicketResponseAsync(ticket, ticketTier);
                    ticketResponses.Add(response);
                }

                return new UserTicketsResponse
                {
                    UserId = userId,
                    Tickets = ticketResponses,
                    TotalTickets = totalCount,
                    UnusedTickets = statusCounts.GetValueOrDefault(Ticket.TicketStatus.Unused, 0),
                    UsedTickets = statusCounts.GetValueOrDefault(Ticket.TicketStatus.Used, 0),
                    CancelledTickets = statusCounts.GetValueOrDefault(Ticket.TicketStatus.Cancelled, 0),
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets a specific ticket by ID.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="userId">The user ID (for authorization).</param>
        /// <returns>The ticket response if found and authorized, null otherwise.</returns>
        public async Task<TicketResponse?> GetTicketByIdAsync(Guid ticketId, Guid userId)
        {
            _logger.LogDebug("Getting ticket {TicketId} for user {UserId}", ticketId, userId);

            try
            {
                var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
                if (ticket == null || ticket.UserId != userId)
                {
                    _logger.LogWarning("Ticket {TicketId} not found or not owned by user {UserId}", ticketId, userId);
                    return null;
                }

                var ticketTier = ticket.TicketTier ?? await _ticketRepository.GetTicketTierAsync(ticket.TicketTierId);
                return await ConvertToTicketResponseAsync(ticket, ticketTier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketId} for user {UserId}", ticketId, userId);
                throw;
            }
        }

        /// <summary>
        /// Verifies a ticket using its code or QR data.
        /// </summary>
        /// <param name="request">The ticket verification request.</param>
        /// <returns>The ticket verification response.</returns>
        public async Task<TicketVerificationResponse> VerifyTicketAsync(TicketVerificationRequest request)
        {
            _logger.LogInformation("Verifying ticket with code: {TicketCode}", request.TicketCode);

            try
            {
                var ticket = await _ticketRepository.GetTicketByCodeAsync(request.TicketCode);
                
                if (ticket == null)
                {
                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        Message = "Ticket not found."
                    };
                }

                var isValidForUse = ticket.IsValidForUse();
                
                // Get event and user information
                var eventInfo = await _eventInfoService.GetEventInfoAsync(ticket.EventId);
                var userInfo = await _userInfoService.GetUserInfoAsync(ticket.UserId);
                
                var eventName = eventInfo?.Title ?? "Event Name";
                var attendeeName = userInfo?.FullName ?? "User Name";

                if (isValidForUse)
                {
                    // Mark ticket as used
                    await _ticketRepository.MarkTicketAsUsedAsync(ticket.Id);

                    return new TicketVerificationResponse
                    {
                        IsValid = true,
                        TicketId = ticket.Id,
                        EventId = ticket.EventId,
                        EventName = eventName,
                        TicketTier = ticket.TicketTier?.Name ?? "Unknown",
                        AttendeeName = attendeeName,
                        VerifiedAt = DateTime.UtcNow,
                        Message = "Ticket verified successfully and marked as used."
                    };
                }
                else
                {
                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        TicketId = ticket.Id,
                        EventId = ticket.EventId,
                        EventName = eventName,
                        TicketTier = ticket.TicketTier?.Name ?? "Unknown",
                        AttendeeName = attendeeName,
                        VerifiedAt = DateTime.UtcNow,
                        Message = $"Ticket is not valid for use. Status: {ticket.Status}, Used: {ticket.IsUsed}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying ticket {TicketCode}", request.TicketCode);
                return new TicketVerificationResponse
                {
                    IsValid = false,
                    Message = "An error occurred during ticket verification."
                };
            }
        }

        /// <summary>
        /// Cancels a ticket if it hasn't been used.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="userId">The user ID (for authorization).</param>
        /// <returns>True if cancelled successfully, false otherwise.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authorized.</exception>
        /// <exception cref="InvalidOperationException">Thrown when ticket cannot be cancelled.</exception>
        public async Task<bool> CancelTicketAsync(Guid ticketId, Guid userId)
        {
            _logger.LogInformation("Cancelling ticket {TicketId} for user {UserId}", ticketId, userId);

            try
            {
                var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    throw new InvalidOperationException("Ticket not found.");
                }

                if (ticket.UserId != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to cancel this ticket.");
                }

                if (ticket.IsUsed || ticket.Status == Ticket.TicketStatus.Used)
                {
                    throw new InvalidOperationException("Cannot cancel a ticket that has already been used.");
                }

                if (ticket.Status == Ticket.TicketStatus.Cancelled)
                {
                    throw new InvalidOperationException("Ticket is already cancelled.");
                }

                // Cancel the ticket
                var success = await _ticketRepository.CancelTicketAsync(ticketId);
                if (success)
                {
                    // Update ticket tier sold quantity (decrease by 1)
                    await _ticketRepository.UpdateTicketTierSoldQuantityAsync(ticket.TicketTierId, -1);
                    _logger.LogInformation("Ticket {TicketId} cancelled successfully", ticketId);
                }

                return success;
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);
                throw new InvalidOperationException("Failed to cancel ticket. Please contact support.", ex);
            }
        }

        /// <summary>
        /// Validates that a ticket issuance request is valid.
        /// </summary>
        /// <param name="request">The ticket issuance request.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public async Task<bool> ValidateTicketIssuanceRequestAsync(IssueTicketRequest request)
        {
            _logger.LogDebug("Validating ticket issuance request for user {UserId}", request.UserId);

            // Validate data annotations
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, context, validationResults, true))
            {
                var errors = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                throw new ArgumentException($"Invalid ticket issuance request: {errors}");
            }

            // Additional business validations
            if (request.EventId == Guid.Empty)
            {
                throw new ArgumentException("Event ID cannot be empty.");
            }

            if (request.UserId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.");
            }

            if (request.TicketTierId == Guid.Empty)
            {
                throw new ArgumentException("Ticket tier ID cannot be empty.");
            }

            // PaymentId validation is now handled in IssueTicketsAsync where it's auto-generated if not provided
            if (request.PaymentId.HasValue && request.PaymentId.Value == Guid.Empty)
            {
                throw new ArgumentException("Payment ID cannot be empty if provided.");
            }

            if (request.Quantity <= 0 || request.Quantity > 10)
            {
                throw new ArgumentException("Quantity must be between 1 and 10.");
            }

            return true;
        }

        /// <summary>
        /// Creates a single ticket from the issuance request.
        /// </summary>
        /// <param name="request">The ticket issuance request.</param>
        /// <param name="ticketTier">The ticket tier.</param>
        /// <returns>A new ticket instance.</returns>
        private async Task<Ticket> CreateTicketAsync(IssueTicketRequest request, TicketTier ticketTier)
        {
            // Generate unique ticket code
            string ticketCode;
            do
            {
                ticketCode = Ticket.GenerateTicketCode();
            } while (await _ticketRepository.TicketCodeExistsAsync(ticketCode));

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                UserId = request.UserId,
                TicketTierId = request.TicketTierId,
                Price = request.Price,
                Currency = request.Currency,
                TicketCode = ticketCode,
                Status = Ticket.TicketStatus.Unused,
                PaymentId = request.PaymentId!.Value,
                IsUsed = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Generate QR code data using the new JWT-like format
            ticket.QRCodeData = _qrCodeService.GenerateJWTLikeQRData(ticket);

            return ticket;
        }

        /// <summary>
        /// Converts a ticket entity to a ticket response DTO.
        /// </summary>
        /// <param name="ticket">The ticket entity.</param>
        /// <param name="ticketTier">The ticket tier (optional).</param>
        /// <returns>A ticket response DTO.</returns>
        private async Task<TicketResponse> ConvertToTicketResponseAsync(Ticket ticket, TicketTier? ticketTier)
        {
            try
            {
                // Generate QR code image if QR data exists
                string? qrCodeImage = null;
                if (!string.IsNullOrEmpty(ticket.QRCodeData))
                {
                    qrCodeImage = _qrCodeService.GenerateQRCodeImage(ticket.QRCodeData);
                }

                // Get event name from EventInfoService
                var eventInfo = await _eventInfoService.GetEventInfoAsync(ticket.EventId);
                var eventName = eventInfo?.Title ?? "Event Name";

                return new TicketResponse
                {
                    Id = ticket.Id,
                    EventId = ticket.EventId,
                    EventName = eventName,
                    UserId = ticket.UserId,
                    TicketTierId = ticket.TicketTierId,
                    TierName = ticketTier?.Name ?? "Unknown",
                    TierDescription = ticketTier?.Description,
                    Price = ticket.Price,
                    Currency = ticket.Currency,
                    TicketCode = ticket.TicketCode,
                    QRCodeData = ticket.QRCodeData,
                    QRCodeImage = qrCodeImage,
                    IsUsed = ticket.IsUsed,
                    UsedAt = ticket.UsedAt,
                    Status = ticket.Status,
                    PaymentId = ticket.PaymentId,
                    IssuedAt = ticket.CreatedAt,
                    IsActive = ticket.IsActive,
                    IsValidForUse = ticket.IsValidForUse()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image for ticket {TicketId}", ticket.Id);
                
                // Get event name from EventInfoService (with fallback)
                var eventInfo = await _eventInfoService.GetEventInfoAsync(ticket.EventId);
                var eventName = eventInfo?.Title ?? "Event Name";
                
                // Return response without QR code image if generation fails
                return new TicketResponse
                {
                    Id = ticket.Id,
                    EventId = ticket.EventId,
                    EventName = eventName,
                    UserId = ticket.UserId,
                    TicketTierId = ticket.TicketTierId,
                    TierName = ticketTier?.Name ?? "Unknown",
                    TierDescription = ticketTier?.Description,
                    Price = ticket.Price,
                    Currency = ticket.Currency,
                    TicketCode = ticket.TicketCode,
                    QRCodeData = ticket.QRCodeData,
                    QRCodeImage = null, // QR code generation failed
                    IsUsed = ticket.IsUsed,
                    UsedAt = ticket.UsedAt,
                    Status = ticket.Status,
                    PaymentId = ticket.PaymentId,
                    IssuedAt = ticket.CreatedAt,
                    IsActive = ticket.IsActive,
                    IsValidForUse = ticket.IsValidForUse()
                };
            }
        }

        /// <summary>
        /// Sends ticket confirmation email to the user.
        /// </summary>
        /// <param name="ticketResponses">The list of ticket responses.</param>
        /// <param name="userInfo">The user information.</param>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="eventName">The event name (fallback).</param>
        private async Task SendTicketConfirmationEmailAsync(IEnumerable<TicketResponse> ticketResponses, Shared.Kernel.Interfaces.UserInfo? userInfo, Shared.Kernel.Interfaces.EventInfo? eventInfo, string eventName)
        {
            try
            {
                var tickets = ticketResponses.ToList();
                if (!tickets.Any())
                {
                    _logger.LogWarning("No tickets provided for email confirmation");
                    return;
                }

                // Check if user info is available
                if (userInfo == null)
                {
                    _logger.LogWarning("User information not available, skipping email confirmation");
                    return;
                }

                var userEmail = userInfo.Email;
                var userName = userInfo.FullName;
                var userId = userInfo.Id;
                
                // Use provided event info or fallback
                var actualEventName = eventInfo?.Title ?? eventName ?? "Event";

                _logger.LogInformation("Sending ticket confirmation email for {TicketCount} tickets to user {UserId} ({UserEmail}) for event {EventName}", 
                    tickets.Count, userId, userEmail, actualEventName);

                bool emailSent;
                if (tickets.Count == 1)
                {
                    emailSent = await _emailService.SendTicketConfirmationEmailAsync(tickets.First(), userEmail, userName, actualEventName);
                }
                else
                {
                    emailSent = await _emailService.SendMultipleTicketConfirmationEmailsAsync(tickets, userEmail, userName, actualEventName);
                }

                if (emailSent)
                {
                    _logger.LogInformation("Successfully sent ticket confirmation email for {TicketCount} tickets to user {UserId} ({UserEmail})", tickets.Count, userId, userEmail);
                }
                else
                {
                    _logger.LogWarning("Failed to send ticket confirmation email for {TicketCount} tickets to user {UserId} ({UserEmail})", tickets.Count, userId, userEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket confirmation email for user {UserId}", userInfo?.Id);
                // Don't rethrow - email failure shouldn't affect ticket issuance
            }
        }

        /// <summary>
        /// Validates a QR code and marks the associated ticket as used.
        /// </summary>
        /// <param name="request">The QR code validation request.</param>
        /// <returns>The ticket verification response with comprehensive ticket details.</returns>
        public async Task<TicketVerificationResponse> ValidateQRCodeAsync(QRCodeValidationRequest request)
        {
            _logger.LogInformation("QR code validation attempt");

            try
            {
                // Step 1: Validate QR code data structure and extract ticket information
                var extractedData = _qrCodeService.ValidateAndExtractQRData(request.QRCodeData);
                
                if (extractedData == null)
                {
                    _logger.LogWarning("Invalid QR code data provided");
                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        Message = "Invalid QR code data. The QR code may be corrupted, expired, or tampered with."
                    };
                }

                // Step 2: Extract ticket information from QR data
                if (!extractedData.TryGetValue("ticketId", out var ticketIdString) || 
                    !Guid.TryParse(ticketIdString, out var ticketId))
                {
                    _logger.LogWarning("QR code does not contain valid ticket ID");
                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        Message = "QR code does not contain valid ticket information."
                    };
                }

                if (!extractedData.TryGetValue("ticketCode", out var ticketCode) || string.IsNullOrEmpty(ticketCode))
                {
                    _logger.LogWarning("QR code does not contain valid ticket code");
                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        Message = "QR code does not contain valid ticket code."
                    };
                }

                // Step 3: Get the ticket from database
                var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found in database", ticketId);
                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        Message = "Ticket not found. This ticket may have been deleted or the QR code is invalid."
                    };
                }

                // Step 4: Verify ticket code matches
                if (ticket.TicketCode != ticketCode)
                {
                    _logger.LogWarning("Ticket code mismatch for ticket {TicketId}. Expected: {ExpectedCode}, Got: {ActualCode}", 
                        ticketId, ticket.TicketCode, ticketCode);
                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        Message = "QR code data does not match ticket information. This may be a fraudulent QR code."
                    };
                }

                // Step 5: Check if ticket is valid for use
                if (!ticket.IsValidForUse())
                {
                    _logger.LogWarning("Ticket {TicketId} is not valid for use. Status: {Status}, Used: {IsUsed}, Active: {IsActive}", 
                        ticketId, ticket.Status, ticket.IsUsed, ticket.IsActive);
                    
                    var reason = ticket.IsUsed ? "already used" : 
                                ticket.Status == Ticket.TicketStatus.Cancelled ? "cancelled" :
                                ticket.Status == Ticket.TicketStatus.Expired ? "expired" :
                                !ticket.IsActive ? "inactive" : "invalid status";

                    // Get event and user information for error response
                    var errorEventInfo = await _eventInfoService.GetEventInfoAsync(ticket.EventId);
                    var errorUserInfo = await _userInfoService.GetUserInfoAsync(ticket.UserId);
                    
                    var errorEventName = errorEventInfo?.Title ?? "Event Name";
                    var errorAttendeeName = errorUserInfo?.FullName ?? "User Name";

                    return new TicketVerificationResponse
                    {
                        IsValid = false,
                        TicketId = ticket.Id,
                        EventId = ticket.EventId,
                        EventName = errorEventName,
                        TicketTier = ticket.TicketTier?.Name ?? "Unknown",
                        AttendeeName = errorAttendeeName,
                        VerifiedAt = DateTime.UtcNow,
                        Message = $"Ticket cannot be used because it is {reason}."
                    };
                }

                // Step 6: Mark ticket as used
                await _ticketRepository.MarkTicketAsUsedAsync(ticket.Id);

                _logger.LogInformation("QR code validated successfully for ticket {TicketId}", ticketId);

                // Step 7: Get event and user information
                var eventInfo = await _eventInfoService.GetEventInfoAsync(ticket.EventId);
                var userInfo = await _userInfoService.GetUserInfoAsync(ticket.UserId);
                
                var eventName = eventInfo?.Title ?? "Event Name";
                var attendeeName = userInfo?.FullName ?? "User Name";

                // Step 8: Return comprehensive success response
                return new TicketVerificationResponse
                {
                    IsValid = true,
                    TicketId = ticket.Id,
                    EventId = ticket.EventId,
                    EventName = eventName,
                    TicketTier = ticket.TicketTier?.Name ?? "Unknown",
                    AttendeeName = attendeeName,
                    VerifiedAt = DateTime.UtcNow,
                    Message = "QR code validated successfully and ticket marked as used."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating QR code");
                return new TicketVerificationResponse
                {
                    IsValid = false,
                    Message = "An error occurred during QR code validation. Please try again or contact support."
                };
            }
        }
    }
}
