namespace SampleAuthJWT.Shared.Models;

public record class RegisterRequest(string FirstName, string LastName, string Email, string Password);
public record class LoginRequest(string UserName, string Password, string? Scopes);
public record class LoginQrCodeRequest(string Email, string Password);
public record class ValidationQrCodeRequest(string Token, string Code);