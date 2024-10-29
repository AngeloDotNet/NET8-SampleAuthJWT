namespace SampleAuthJWT.MinimalAPI.Endpoints;

public class MeEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/me", APIExtension.GetMe)
            .RequireAuthorization()
            .WithOpenApi();
    }
}
