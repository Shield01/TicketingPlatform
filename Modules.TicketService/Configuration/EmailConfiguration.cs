namespace Modules.TicketService.Configuration
{
    /// <summary>
    /// Configuration settings for email functionality.
    /// </summary>
    public class EmailConfiguration
    {
        /// <summary>
        /// The SMTP server host.
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// The SMTP server port.
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// Whether to use SSL/TLS for SMTP connection.
        /// </summary>
        public bool UseSsl { get; set; } = true;

        /// <summary>
        /// The SMTP username for authentication.
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;

        /// <summary>
        /// The SMTP password for authentication.
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// The sender email address.
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// The sender display name.
        /// </summary>
        public string FromName { get; set; } = "Ticketing Platform";

        /// <summary>
        /// Whether email functionality is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The timeout for SMTP operations in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
}
