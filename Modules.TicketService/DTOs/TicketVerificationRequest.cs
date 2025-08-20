using System.ComponentModel.DataAnnotations;

namespace Modules.TicketService.DTOs
{
    /// <summary>
    /// Request model for ticket verification.
    /// </summary>
    public class TicketVerificationRequest
    {
        /// <summary>
        /// The encrypted ticket code to verify.
        /// </summary>
        /// <example>eyJ0aWNrZXRJZCI6IjEyMzQ1Njc4LTEyMzQtMTIzNC0xMjM0LTEyMzQ1Njc4OTAxMiIsImV2ZW50SWQiOiI5ODc2NTQzMi0xMjM0LTEyMzQtMTIzNC0xMjM0NTY3ODkwMTIiLCJ0aW1lc3RhbXAiOiIyMDI0LTAxLTE1VDEwOjAwOjAwWiJ9</example>
        [Required]
        public string TicketCode { get; set; } = string.Empty;
    }
} 