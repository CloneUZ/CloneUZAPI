using CloneUZ.Api.Contracts.Admin;
using CloneUZ.Api.Data;
using CloneUZ.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.Api.Services;

public interface IAdminService
{
    Task<List<UserAdminDto>> GetAllUsersAsync(CancellationToken ct);
    Task<UserAdminDto?> GetUserByIdAsync(Guid id, CancellationToken ct);
    Task<(bool Success, string? Error, int StatusCode)> UpdateBanStatusAsync(Guid id, BanUserRequestDto request, CancellationToken ct);
    Task<(bool Success, string? Error, int StatusCode)> DeleteUserAsync(Guid id, CancellationToken ct);
}

public sealed class AdminService(AppDbContext db) : IAdminService
{
    public async Task<List<UserAdminDto>> GetAllUsersAsync(CancellationToken ct)
    {
        return await db.Users
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new UserAdminDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.IsBanned,
                u.BanReason,
                u.PhoneNumber,
                u.BirthDate,
                u.Citizenship,
                u.DocumentNumber,
                u.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<UserAdminDto?> GetUserByIdAsync(Guid id, CancellationToken ct)
    {
        return await db.Users
            .Where(u => u.Id == id)
            .Select(u => new UserAdminDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.IsBanned,
                u.BanReason,
                u.PhoneNumber,
                u.BirthDate,
                u.Citizenship,
                u.DocumentNumber,
                u.CreatedAtUtc))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string? Error, int StatusCode)> UpdateBanStatusAsync(
        Guid id, BanUserRequestDto request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return (false, "Користувача не знайдено.", StatusCodes.Status404NotFound);

        if (user.Role == UserRole.Admin)
            return (false, "Неможливо заблокувати адміністратора.", StatusCodes.Status403Forbidden);

        user.IsBanned = request.IsBanned;
        user.BanReason = request.IsBanned ? request.BanReason?.Trim() : null;

        await db.SaveChangesAsync(ct);
        return (true, null, StatusCodes.Status200OK);
    }

    public async Task<(bool Success, string? Error, int StatusCode)> DeleteUserAsync(Guid id, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return (false, "Користувача не знайдено.", StatusCodes.Status404NotFound);

        if (user.Role == UserRole.Admin)
            return (false, "Неможливо видалити адміністратора.", StatusCodes.Status403Forbidden);

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
        return (true, null, StatusCodes.Status204NoContent);
    }
}