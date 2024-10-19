using System.Security.Claims;
using MinimalHelpers.Routing;

namespace SampleAuthJWT.MinimalAPI.Endpoints;

public class MeEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/me", (ClaimsPrincipal user) =>
        {
            return TypedResults.Ok(new
            {
                user.Identity!.Name
            });
        }).RequireAuthorization()
        .WithOpenApi();
    }
}
