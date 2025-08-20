using Microsoft.AspNetCore.Routing;

namespace Modules.PaymentService.Controllers
{
    /// <summary>
    /// Extension methods for mapping PaymentService endpoints.
    /// </summary>
    public static class EndpointMapper
    {
        /// <summary>
        /// Maps PaymentService endpoints to the application.
        /// </summary>
        /// <param name="endpoints">The IEndpointRouteBuilder instance.</param>
        /// <returns>The IEndpointRouteBuilder instance.</returns>
        public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Map PaymentService endpoints here
            // The PaymentController will be automatically mapped by ASP.NET Core
            // when using the [ApiController] attribute and [Route] attribute
            return endpoints;
        }
    }
} 