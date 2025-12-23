using Microsoft.Extensions.Logging;
using Modules.PaymentService.DTOs;
using Modules.PaymentService.Infrastructure;
using Modules.PaymentService.Infrastructure.DTOs;
using Modules.PaymentService.Infrastructure.Exceptions;
using Modules.PaymentService.Infrastructure.Helpers;
using Modules.PaymentService.Models;
using Modules.PaymentService.Repositories;
using Modules.PaymentService.Resources.LocalisedStrings;
using System.Text.Json;

namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service implementation for payout operations.
    /// </summary>
    public class PayoutService : IPayoutService
    {
        private readonly IPayoutRepository _payoutRepository;
        private readonly IPayAzaClient _payAzaClient;
        private readonly ILogger<PayoutService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayoutService"/> class.
        /// </summary>
        /// <param name="payoutRepository">The payout repository.</param>
        /// <param name="payAzaClient">The PayAza client.</param>
        /// <param name="logger">The logger.</param>
        public PayoutService(
            IPayoutRepository payoutRepository,
            IPayAzaClient payAzaClient,
            ILogger<PayoutService> logger)
        {
            _payoutRepository = payoutRepository ?? throw new ArgumentNullException(nameof(payoutRepository));
            _payAzaClient = payAzaClient ?? throw new ArgumentNullException(nameof(payAzaClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initiates a new payout transaction.
        /// </summary>
        public async Task<PayoutResponse> InitiatePayoutAsync(
            InitiatePayoutRequest request, 
            Guid initiatedByUserId, 
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogInformation("Initiating payout for user {UserId}, amount {Amount} {Currency}",
                initiatedByUserId, request.Amount, request.Currency);

            // Generate transaction reference if not provided
            var transactionReference = !string.IsNullOrWhiteSpace(request.TransactionReference)
                ? request.TransactionReference
                : TransactionReferenceGenerator.Generate("PAYOUT");

            // Check for duplicate transaction reference
            if (await _payoutRepository.ReferenceExistsAsync(transactionReference, cancellationToken))
            {
                _logger.LogWarning("Duplicate payout reference detected: {Reference}", transactionReference);
                throw new InvalidOperationException(PaymentMessages.DuplicatePayoutReference);
            }

            // Create payout transaction record
            var payout = new PayoutTransaction
            {
                Id = Guid.NewGuid(),
                InitiatedByUserId = initiatedByUserId,
                RecipientUserId = request.RecipientUserId,
                EventId = request.EventId,
                TransactionReference = transactionReference,
                Amount = request.Amount,
                Currency = request.Currency.ToUpperInvariant(),
                AccountNumber = request.AccountNumber,
                BankCode = request.BankCode,
                AccountName = request.AccountName,
                Narration = request.Narration,
                Status = PayoutStatus.INITIATED,
                Gateway = "PayAza",
                IsDryRun = request.IsDryRun,
                GatewayMetadata = request.Metadata != null 
                    ? JsonSerializer.Serialize(request.Metadata) 
                    : null
            };

            // Validate payout data
            var (isValid, errorMessage) = payout.Validate();
            if (!isValid)
            {
                _logger.LogWarning("Payout validation failed: {ErrorMessage}", errorMessage);
                throw new ArgumentException(errorMessage);
            }

            // Save payout to database
            payout = await _payoutRepository.CreateAsync(payout, cancellationToken);

            // If not a dry-run, initiate payout via PayAza
            if (!request.IsDryRun)
            {
                try
                {
                    var payAzaRequest = new PayAzaPayoutRequest
                    {
                        TransactionReference = transactionReference,
                        Amount = request.Amount,
                        Currency = request.Currency.ToUpperInvariant(),
                        AccountNumber = request.AccountNumber,
                        BankCode = request.BankCode,
                        AccountName = request.AccountName,
                        Narration = request.Narration,
                        Metadata = request.Metadata
                    };

                    var payAzaResponse = await _payAzaClient.InitiatePayoutAsync(payAzaRequest, cancellationToken);

                    if (payAzaResponse.Success && payAzaResponse.Data != null)
                    {
                        // Update payout with gateway response
                        payout.GatewayTransactionId = payAzaResponse.Data.TransactionReference;
                        payout.Status = MapPayAzaStatus(payAzaResponse.Data.Status);
                        payout.GatewayFee = payAzaResponse.Data.Fee;
                        
                        if (payout.Status == PayoutStatus.COMPLETED)
                        {
                            payout.MarkAsCompleted(payAzaResponse.Data.TransactionReference, payAzaResponse.Data.Fee);
                        }
                        else
                        {
                            payout.MarkAsProcessing();
                        }

                        payout = await _payoutRepository.UpdateAsync(payout, cancellationToken);

                        _logger.LogInformation("Payout initiated successfully: {Reference}, Status: {Status}",
                            transactionReference, payout.Status);
                    }
                    else
                    {
                        // Payout failed at gateway
                        var errorMsg = payAzaResponse.Error?.Message ?? payAzaResponse.Message;
                        payout.MarkAsFailed(errorMsg, payAzaResponse.Error?.Code);
                        payout = await _payoutRepository.UpdateAsync(payout, cancellationToken);

                        _logger.LogError("PayAza payout failed: {Message}", errorMsg);
                    }
                }
                catch (PayAzaException ex)
                {
                    _logger.LogError(ex, "PayAza exception during payout initiation: {Reference}", transactionReference);
                    
                    payout.MarkAsFailed(ex.Message, ex.ErrorCode);
                    payout = await _payoutRepository.UpdateAsync(payout, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during payout initiation: {Reference}", transactionReference);
                    
                    payout.MarkAsFailed($"Payout initiation failed: {ex.Message}");
                    payout = await _payoutRepository.UpdateAsync(payout, cancellationToken);
                }
            }
            else
            {
                _logger.LogInformation("Dry-run payout created: {Reference}", transactionReference);
            }

            return MapToPayoutResponse(payout);
        }

        /// <summary>
        /// Verifies an account before payout.
        /// </summary>
        public async Task<AccountEnquiryResponse> VerifyAccountAsync(
            AccountEnquiryRequest request, 
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogInformation("Verifying account {AccountNumber} at bank {BankCode}",
                request.AccountNumber, request.BankCode);

            try
            {
                var payAzaResponse = await _payAzaClient.GetAccountDetailsAsync(
                    request.AccountNumber, 
                    request.BankCode, 
                    cancellationToken);

                if (payAzaResponse.Success && payAzaResponse.Data != null)
                {
                    _logger.LogInformation("Account verified successfully: {AccountName}",
                        payAzaResponse.Data.AccountName);

                    return new AccountEnquiryResponse
                    {
                        Success = true,
                        AccountNumber = payAzaResponse.Data.AccountNumber,
                        AccountName = payAzaResponse.Data.AccountName,
                        BankCode = payAzaResponse.Data.BankCode,
                        BankName = payAzaResponse.Data.BankName,
                        Currency = payAzaResponse.Data.Currency,
                        Balance = payAzaResponse.Data.Balance,
                        Message = PaymentMessages.AccountVerified
                    };
                }
                else
                {
                    var errorMsg = payAzaResponse.Error?.Message ?? payAzaResponse.Message;
                    _logger.LogWarning("Account verification failed: {Message}", errorMsg);

                    return new AccountEnquiryResponse
                    {
                        Success = false,
                        AccountNumber = request.AccountNumber,
                        BankCode = request.BankCode,
                        Message = PaymentMessages.AccountVerificationFailed,
                        ErrorMessage = errorMsg
                    };
                }
            }
            catch (PayAzaNotFoundException ex)
            {
                _logger.LogWarning(ex, "Account not found: {AccountNumber}", request.AccountNumber);
                
                return new AccountEnquiryResponse
                {
                    Success = false,
                    AccountNumber = request.AccountNumber,
                    BankCode = request.BankCode,
                    Message = PaymentMessages.AccountNotFound,
                    ErrorMessage = ex.Message
                };
            }
            catch (PayAzaException ex)
            {
                _logger.LogError(ex, "PayAza exception during account verification");
                
                return new AccountEnquiryResponse
                {
                    Success = false,
                    AccountNumber = request.AccountNumber,
                    BankCode = request.BankCode,
                    Message = PaymentMessages.AccountVerificationFailed,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during account verification");
                
                return new AccountEnquiryResponse
                {
                    Success = false,
                    AccountNumber = request.AccountNumber,
                    BankCode = request.BankCode,
                    Message = PaymentMessages.AccountEnquiryError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets payout transaction details by ID.
        /// </summary>
        public async Task<PayoutResponse?> GetPayoutByIdAsync(
            Guid payoutId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payout by ID: {PayoutId}", payoutId);

            var payout = await _payoutRepository.GetByIdAsync(payoutId, cancellationToken);
            
            return payout != null ? MapToPayoutResponse(payout) : null;
        }

        /// <summary>
        /// Gets payout transaction details by reference.
        /// </summary>
        public async Task<PayoutResponse?> GetPayoutByReferenceAsync(
            string transactionReference, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transactionReference))
                throw new ArgumentException("Transaction reference cannot be null or empty.", nameof(transactionReference));

            _logger.LogDebug("Getting payout by reference: {Reference}", transactionReference);

            var payout = await _payoutRepository.GetByReferenceAsync(transactionReference, cancellationToken);
            
            return payout != null ? MapToPayoutResponse(payout) : null;
        }

        /// <summary>
        /// Gets payout transactions initiated by a specific user.
        /// </summary>
        public async Task<(List<PayoutResponse> Payouts, int TotalCount)> GetPayoutsByUserIdAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 20, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payouts for user {UserId}, page {Page}, pageSize {PageSize}",
                userId, page, pageSize);

            var (payouts, totalCount) = await _payoutRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);

            var payoutResponses = payouts.Select(MapToPayoutResponse).ToList();

            return (payoutResponses, totalCount);
        }

        /// <summary>
        /// Gets account details including payout statistics.
        /// </summary>
        public async Task<AccountDetailsResponse> GetAccountDetailsAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting account details with payout statistics");

            var statistics = await _payoutRepository.GetStatisticsAsync(cancellationToken);

            // Get recent payouts (last 10)
            var (payouts, _) = await _payoutRepository.GetByUserIdAsync(Guid.Empty, 1, 10, cancellationToken);

            var response = new AccountDetailsResponse
            {
                TotalPayouts = statistics.TotalPayouts,
                TotalAmount = statistics.TotalAmount,
                Currency = statistics.Currency,
                PendingPayoutsCount = statistics.PendingPayouts,
                CompletedPayoutsCount = statistics.CompletedPayouts,
                FailedPayoutsCount = statistics.FailedPayouts,
                RecentPayouts = payouts.Select(MapToPayoutResponse).ToList()
            };

            _logger.LogInformation("Account details retrieved: Total={Total}, Completed={Completed}, Pending={Pending}",
                response.TotalPayouts, response.CompletedPayoutsCount, response.PendingPayoutsCount);

            return response;
        }

        /// <summary>
        /// Previews a payout without executing it (dry-run).
        /// </summary>
        public async Task<PayoutResponse> PreviewPayoutAsync(
            InitiatePayoutRequest request, 
            Guid initiatedByUserId, 
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger.LogInformation("Previewing payout for user {UserId}, amount {Amount} {Currency}",
                initiatedByUserId, request.Amount, request.Currency);

            // Force dry-run mode
            request.IsDryRun = true;

            // Initiate as dry-run
            return await InitiatePayoutAsync(request, initiatedByUserId, cancellationToken);
        }

        /// <summary>
        /// Maps a PayoutTransaction entity to a PayoutResponse DTO.
        /// </summary>
        private PayoutResponse MapToPayoutResponse(PayoutTransaction payout)
        {
            return new PayoutResponse
            {
                PayoutId = payout.Id,
                TransactionReference = payout.TransactionReference,
                Amount = payout.Amount,
                Currency = payout.Currency,
                Status = payout.Status,
                AccountNumber = payout.AccountNumber,
                AccountName = payout.AccountName,
                BankCode = payout.BankCode,
                BankName = payout.BankName,
                GatewayTransactionId = payout.GatewayTransactionId,
                GatewayFee = payout.GatewayFee,
                Narration = payout.Narration,
                IsDryRun = payout.IsDryRun,
                CreatedAt = payout.CreatedAt,
                CompletedAt = payout.CompletedAt,
                ErrorMessage = payout.ErrorMessage,
                Message = GetStatusMessage(payout.Status, payout.IsDryRun)
            };
        }

        /// <summary>
        /// Maps PayAza payout status to internal payout status.
        /// </summary>
        private string MapPayAzaStatus(string payAzaStatus)
        {
            return payAzaStatus?.ToLowerInvariant() switch
            {
                "success" or "successful" or "completed" => PayoutStatus.COMPLETED,
                "processing" or "pending" => PayoutStatus.PROCESSING,
                "failed" or "failure" => PayoutStatus.FAILED,
                "cancelled" or "canceled" => PayoutStatus.CANCELLED,
                "reversed" => PayoutStatus.REVERSED,
                _ => PayoutStatus.PROCESSING
            };
        }

        /// <summary>
        /// Gets a user-friendly message based on payout status.
        /// </summary>
        private string GetStatusMessage(string status, bool isDryRun)
        {
            if (isDryRun)
                return "This is a preview/dry-run payout (not executed).";

            return status switch
            {
                PayoutStatus.COMPLETED => PaymentMessages.PayoutCompleted,
                PayoutStatus.FAILED => PaymentMessages.PayoutFailed,
                PayoutStatus.CANCELLED => PaymentMessages.PayoutCancelled,
                PayoutStatus.PROCESSING => "Payout is being processed.",
                PayoutStatus.INITIATED => PaymentMessages.PayoutInitiated,
                _ => "Payout status unknown."
            };
        }
    }
}

