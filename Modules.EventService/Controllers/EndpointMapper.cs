using Microsoft.AspNetCore.Routing;

namespace Modules.EventService.Controllers
{
    /// <summary>
    /// Extension methods for mapping EventService endpoints.
    /// </summary>
    public static class EndpointMapper
    {
        /// <summary>
        /// Maps EventService endpoints to the application.
        /// </summary>
        /// <param name="endpoints">The IEndpointRouteBuilder instance.</param>
        /// <returns>The IEndpointRouteBuilder instance.</returns>
        public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Map EventService endpoints here
            // The EventController will be automatically mapped by ASP.NET Core
            // when using the [ApiController] attribute and [Route] attribute
            return endpoints;
        }
    }
} 