namespace SampleAuthJWT.MinimalAPI.Endpoints;

public class AuthEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/register", APIExtension.RegisterAsync)
            .WithOpenApi();

        endpoints.MapPost("/login-qrcode", APIExtension.LoginQrCodeAsync)
            .WithOpenApi();

        endpoints.MapGet("/generate-qrcode", APIExtension.GenerateQrCodeAsync)
            .WithOpenApi();

        endpoints.MapPost("/validate-qrcode", APIExtension.ValidateQrCodeAsync)
            .WithOpenApi();
    }
}