namespace Modules.PaymentService.Services
{
    /// <summary>
    /// Service interface for webhook signature validation.
    /// </summary>
    public interface IWebhookValidationService
    {
        /// <summary>
        /// Validates webhook signature using HMAC SHA512.
        /// </summary>
        /// <param name="payload">The raw webhook payload.</param>
        /// <param name="signature">The signature from the webhook header.</param>
        /// <returns>True if the signature is valid, false otherwise.</returns>
        bool ValidateSignature(string payload, string signature);

        /// <summary>
        /// Computes HMAC SHA512 signature for a payload.
        /// </summary>
        /// <param name="payload">The payload to sign.</param>
        /// <returns>The Base64-encoded signature.</returns>
        string ComputeSignature(string payload);
    }
}

