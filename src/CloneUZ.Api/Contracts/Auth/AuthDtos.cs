using CloneUZ.Api.Domain;

namespace CloneUZ.Api.Contracts.Auth;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber = null,
    DateOnly? BirthDate = null,
    string? Citizenship = null,
    string? DocumentNumber = null);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role);