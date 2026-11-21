using System.Collections.Concurrent;
using CloneUZ.BLL.Interfaces;
using CloneUZ.Domain.Entities;
using CloneUZ.Domain.Enums;
using DTOs.Admin;

namespace CloneUZ.BLL.Services;

public class AdminService : IAdminService
{
    private static readonly ConcurrentDictionary<Guid, User> _users = SeedUsers();

    private static ConcurrentDictionary<Guid, User> SeedUsers()
    {
        var dict = new ConcurrentDictionary<Guid, User>();

        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FirstName = "Дмитро",
            LastName = "Кучинський",
            Email = "admin@uz.gov.ua",
            Role = UserRole.Admin
        };

        var user1 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FirstName = "Олена",
            LastName = "Коваль",
            Email = "olena.koval@example.com",
            Role = UserRole.User
        };

        var user2 = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FirstName = "Іван",
            LastName = "Петренко",
            Email = "ivan.petrenko@example.com",
            Role = UserRole.User
        };

        dict[admin.Id] = admin;
        dict[user1.Id] = user1;
        dict[user2.Id] = user2;

        return dict;
    }

    public Task<IEnumerable<UserAdminDto>> GetAllUsersAsync()
    {
        var result = _users.Values
            .OrderBy(u => u.CreatedAt)
            .Select(MapToDto);

        return Task.FromResult(result);
    }

    public Task<UserAdminDto?> GetUserByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user == null ? null : MapToDto(user));
    }

    public Task<UserAdminDto?> SetUserBanStatusAsync(Guid id, BanUserRequestDto request)
    {
        if (!_users.TryGetValue(id, out var user))
        {
            return Task.FromResult<UserAdminDto?>(null);
        }

        if (user.Role == UserRole.Admin)
        {
            throw new InvalidOperationException("Неможливо заблокувати адміністратора.");
        }

        user.IsBanned = request.IsBanned;
        user.BanReason = request.IsBanned ? (request.Reason ?? "Порушення правил сервісу") : null;

        return Task.FromResult<UserAdminDto?>(MapToDto(user));
    }

    public Task<bool> DeleteUserAsync(Guid id)
    {
        if (!_users.TryGetValue(id, out var user))
        {
            return Task.FromResult(false);
        }

        if (user.Role == UserRole.Admin)
        {
            throw new InvalidOperationException("Неможливо видалити адміністратора.");
        }

        return Task.FromResult(_users.TryRemove(id, out _));
    }

    private static UserAdminDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role.ToString(),
        IsBanned = user.IsBanned,
        BanReason = user.BanReason,
        CreatedAt = user.CreatedAt
    };
}