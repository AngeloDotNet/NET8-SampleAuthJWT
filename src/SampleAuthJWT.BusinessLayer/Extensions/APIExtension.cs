namespace SampleAuthJWT.BusinessLayer.Extensions;

public static class APIExtension
{
    public static IResult GetMe(ClaimsPrincipal user)
        => TypedResults.Ok(new { user.Identity!.Name });

    public static async Task<IResult> RegisterAsync(RegisterRequest request, UserManager<ApplicationUser> userManager)
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
    }

    public static async Task<IResult> LoginQrCodeAsync(LoginRequest request, SignInManager<ApplicationUser> signInManager,
        IJwtBearerService jwtBearerService, ITimeLimitedDataProtector dataProtector)
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
    }

    public static async Task<IResult> GenerateQrCodeAsync(string token, ITimeLimitedDataProtector dataProtector,
        UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
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
    }

    public static async Task<IResult> ValidateQrCodeAsync(ValidationRequest request, ITimeLimitedDataProtector dataProtector,
        UserManager<ApplicationUser> userManager, IJwtBearerService jwtBearerService)
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

        if (!isValidTotpCode)
        {
            return TypedResults.BadRequest();
        }

        var token = await jwtBearerService.CreateTokenAsync(user.Email!);

        return TypedResults.Ok(new LoginResponse(token));
    }
}