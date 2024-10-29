namespace SampleAuthJWT.MinimalAPI.Endpoints;

public class MeEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var testApi = endpoints
            .MapGroup("/api/profile")
            .RequireAuthorization()
            .WithTags("Profile API")
            .WithOpenApi();

        testApi.MapGet("/me", APIExtension.GetMe);
    }
}
