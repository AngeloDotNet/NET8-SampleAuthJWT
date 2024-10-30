namespace SampleAuthJWT.Shared.Models;

public record class User(string? UserName);
public record class LoginResponse(string Token);
public record class LoginQrCodeResponse(string Token);
public record class AuthResponse(string? AccessToken, string? RefreshToken, IEnumerable<string> Errors);