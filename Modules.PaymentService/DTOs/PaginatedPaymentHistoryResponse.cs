namespace Modules.PaymentService.DTOs
{
    /// <summary>
    /// Response model for paginated payment history.
    /// </summary>
    public class PaginatedPaymentHistoryResponse
    {
        /// <summary>
        /// The list of payment transactions for the current page.
        /// </summary>
        public List<PaymentTransactionResponse> Transactions { get; set; } = new();

        /// <summary>
        /// The total number of transactions matching the criteria.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages { get; set; }
    }
} 