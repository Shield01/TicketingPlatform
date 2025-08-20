using Microsoft.AspNetCore.Routing;

namespace Modules.TicketService.Controllers
{
    /// <summary>
    /// Extension methods for mapping TicketService endpoints.
    /// </summary>
    public static class EndpointMapper
    {
        /// <summary>
        /// Maps TicketService endpoints to the application.
        /// </summary>
        /// <param name="endpoints">The IEndpointRouteBuilder instance.</param>
        /// <returns>The IEndpointRouteBuilder instance.</returns>
        public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Map TicketService endpoints here
            // The TicketController will be automatically mapped by ASP.NET Core
            // when using the [ApiController] attribute and [Route] attribute
            return endpoints;
        }
    }
} 