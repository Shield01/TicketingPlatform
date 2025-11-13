namespace Modules.PaymentService.Infrastructure.Exceptions
{
    /// <summary>
    /// Base exception for PayAza-related errors.
    /// </summary>
    public class PayAzaException : Exception
    {
        /// <summary>
        /// Gets or sets the PayAza error code.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PayAzaException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorCode">The PayAza error code.</param>
        public PayAzaException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorCode">The PayAza error code.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        public PayAzaException(string message, string errorCode, int statusCode) : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PayAzaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when PayAza authentication fails.
    /// </summary>
    public class PayAzaAuthenticationException : PayAzaException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaAuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PayAzaAuthenticationException(string message) : base(message, "AUTH_ERROR", 401)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaAuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PayAzaAuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "AUTH_ERROR";
            StatusCode = 401;
        }
    }

    /// <summary>
    /// Exception thrown when a PayAza validation error occurs.
    /// </summary>
    public class PayAzaValidationException : PayAzaException
    {
        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PayAzaValidationException(string message) : base(message, "VALIDATION_ERROR", 400)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="validationErrors">The validation errors.</param>
        public PayAzaValidationException(string message, Dictionary<string, string[]> validationErrors) 
            : base(message, "VALIDATION_ERROR", 400)
        {
            ValidationErrors = validationErrors;
        }
    }

    /// <summary>
    /// Exception thrown when a PayAza resource is not found.
    /// </summary>
    public class PayAzaNotFoundException : PayAzaException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PayAzaNotFoundException(string message) : base(message, "NOT_FOUND", 404)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a PayAza rate limit is exceeded.
    /// </summary>
    public class PayAzaRateLimitException : PayAzaException
    {
        /// <summary>
        /// Gets the time when the rate limit will reset.
        /// </summary>
        public DateTime? ResetTime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaRateLimitException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="resetTime">The time when the rate limit will reset.</param>
        public PayAzaRateLimitException(string message, DateTime? resetTime = null) 
            : base(message, "RATE_LIMIT_EXCEEDED", 429)
        {
            ResetTime = resetTime;
        }
    }

    /// <summary>
    /// Exception thrown when a PayAza server error occurs.
    /// </summary>
    public class PayAzaServerException : PayAzaException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaServerException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        public PayAzaServerException(string message, int statusCode) : base(message, "SERVER_ERROR", statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayAzaServerException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PayAzaServerException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "SERVER_ERROR";
            StatusCode = 500;
        }
    }
}

