namespace SampleAuthJWT.MinimalAPI.Endpoints;

public class AuthEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var identityApi = endpoints
            .MapGroup("/api/auth")
            .WithTags("Identity API")
            .WithOpenApi();

        identityApi.MapPost("/register", APIExtension.RegisterAsync);

        identityApi.MapPost("/login-qrcode", APIExtension.LoginQrCodeAsync);

        identityApi.MapGet("/generate-qrcode", APIExtension.GenerateQrCodeAsync);

        identityApi.MapPost("/validate-qrcode", APIExtension.ValidateQrCodeAsync);
    }
}