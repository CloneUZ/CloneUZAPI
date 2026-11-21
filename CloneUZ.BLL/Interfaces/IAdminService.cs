using DTOs.Admin;

namespace CloneUZ.BLL.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserAdminDto>> GetAllUsersAsync();
    Task<UserAdminDto?> GetUserByIdAsync(Guid id);
    Task<UserAdminDto?> SetUserBanStatusAsync(Guid id, BanUserRequestDto request);
    Task<bool> DeleteUserAsync(Guid id);
}