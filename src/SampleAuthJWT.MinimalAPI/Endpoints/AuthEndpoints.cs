using System.Net.Mime;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using MinimalHelpers.Routing;
using QRCoder;
using SampleAuthJWT.DataAccessLayer.Entities;
using SampleAuthJWT.Shared.Models;
using SimpleAuthentication.JwtBearer;

namespace SampleAuthJWT.MinimalAPI.Endpoints;

public class AuthEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/register", async Task<Results<Created, BadRequest<IEnumerable<IdentityError>>>> (RegisterRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email.ToLowerInvariant(),
                Email = request.Email.ToLowerInvariant(),
                TwoFactorEnabled = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, request.Password);

            return result.Succeeded ? TypedResults.Created() : TypedResults.BadRequest(result.Errors);

        }).WithOpenApi();

        endpoints.MapPost("/login", async Task<Results<Ok<LoginResponse>, BadRequest>> (LoginRequest request,
            SignInManager<ApplicationUser> signInManager, IJwtBearerService jwtBearerService, ITimeLimitedDataProtector dataProtector) =>
        {
            var user = await signInManager.UserManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return TypedResults.BadRequest();
            }

            var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

            if (!result.Succeeded && !result.RequiresTwoFactor)
            {
                return TypedResults.BadRequest();
            }

            var token = dataProtector.Protect(user.Id.ToString(), TimeSpan.FromMinutes(5));

            return TypedResults.Ok(new LoginResponse(token));

        }).WithOpenApi();

        endpoints.MapGet("/qrcode", async Task<Results<FileContentHttpResult, BadRequest>> (string token,
            ITimeLimitedDataProtector dataProtector, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment) =>
        {
            ApplicationUser? user = null;

            try
            {
                var userId = dataProtector.Unprotect(token);
                user = await userManager.FindByIdAsync(userId);
            }
            catch
            {
                return TypedResults.BadRequest();
            }

            if (user is null || await userManager.GetAuthenticatorKeyAsync(user) is not null)
            {
                return TypedResults.BadRequest();
            }

            await userManager.ResetAuthenticatorKeyAsync(user);

            var secret = await userManager.GetAuthenticatorKeyAsync(user);
            var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(environment.ApplicationName)}:{user.Email}?secret={secret}&issuer={Uri.EscapeDataString(environment.ApplicationName)}";

            using var qrCodeGenerator = new QRCodeGenerator();
            using var qrCodeData = qrCodeGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(3);

            return TypedResults.File(qrCodeBytes, MediaTypeNames.Image.Png);

        }).WithOpenApi();

        endpoints.MapPost("/validate", async Task<Results<Ok<LoginResponse>, BadRequest>> (ValidationRequest request,
            ITimeLimitedDataProtector dataProtector, UserManager<ApplicationUser> userManager, IJwtBearerService jwtBearerService) =>
        {
            ApplicationUser? user = null;

            try
            {
                var userId = dataProtector.Unprotect(request.Token);
                user = await userManager.FindByIdAsync(userId);
            }
            catch
            {
                return TypedResults.BadRequest();
            }

            if (user is null)
            {
                return TypedResults.BadRequest();
            }

            var isValidTotpCode = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);

            // Example of verification using Otp.NET library:
            // https://github.com/kspearrin/Otp.NET
            //var secret = await userManager.GetAuthenticatorKeyAsync(user);
            //var totp = new Totp(Base32Encoding.ToBytes(secret));
            //var isValidTotpCode = totp.VerifyTotp(request.Code, out var timeStepUsed, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!isValidTotpCode)
            {
                return TypedResults.BadRequest();
            }

            var token = await jwtBearerService.CreateTokenAsync(user.Email!);

            return TypedResults.Ok(new LoginResponse(token));

        }).WithOpenApi();
    }
}
