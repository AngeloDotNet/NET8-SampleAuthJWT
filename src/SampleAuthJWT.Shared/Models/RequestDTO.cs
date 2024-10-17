namespace SampleAuthJWT.Shared.Models;

public record class LoginRequest(string Email, string Password);
public record class RegisterRequest(string FirstName, string LastName, string Email, string Password);
public record class ValidationRequest(string Token, string Code);