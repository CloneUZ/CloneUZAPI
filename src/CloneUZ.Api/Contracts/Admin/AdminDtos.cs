using CloneUZ.Api.Domain;

namespace CloneUZ.Api.Contracts.Admin;

public sealed record UserAdminDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsBanned,
    string? BanReason,
    string? PhoneNumber,
    DateOnly? BirthDate,
    string? Citizenship,
    string? DocumentNumber,
    DateTime CreatedAtUtc);

public sealed record BanUserRequestDto(
    bool IsBanned,
    string? BanReason);