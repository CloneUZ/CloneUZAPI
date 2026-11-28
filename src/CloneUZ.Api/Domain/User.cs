namespace CloneUZ.Api.Domain;

public sealed class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;

    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }

    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Citizenship { get; set; }
    public string? DocumentNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}