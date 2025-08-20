using Microsoft.AspNetCore.Routing;

namespace Modules.UserService.Controllers
{
    /// <summary>
    /// Extension methods for mapping UserService endpoints.
    /// </summary>
    public static class EndpointMapper
    {
        /// <summary>
        /// Maps UserService endpoints to the application.
        /// </summary>
        /// <param name="endpoints">The IEndpointRouteBuilder instance.</param>
        /// <returns>The IEndpointRouteBuilder instance.</returns>
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Map UserService endpoints here
            // The UserController will be automatically mapped by ASP.NET Core
            // when using the [ApiController] attribute and [Route] attribute
            return endpoints;
        }
    }
} 