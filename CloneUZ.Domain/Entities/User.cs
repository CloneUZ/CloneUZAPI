using CloneUZ.Domain.Enums;

namespace CloneUZ.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // додаткові поля з макета
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Citizenship { get; set; }
    public string? DocumentNumber { get; set; }
}