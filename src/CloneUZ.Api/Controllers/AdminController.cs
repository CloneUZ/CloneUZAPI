using CloneUZ.Api.Contracts.Admin;
using CloneUZ.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneUZ.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var users = await adminService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var user = await adminService.GetUserByIdAsync(id, ct);
        return user is null ? NotFound(new { error = "Користувача не знайдено." }) : Ok(user);
    }

    [HttpPatch("users/{id:guid}/ban")]
    public async Task<IActionResult> UpdateBanStatus(Guid id, [FromBody] BanUserRequestDto request, CancellationToken ct)
    {
        var (success, error, statusCode) = await adminService.UpdateBanStatusAsync(id, request, ct);
        if (!success)
            return StatusCode(statusCode, new { error });

        return Ok(new { message = request.IsBanned ? "Користувача заблоковано." : "Користувача розблоковано." });
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        var (success, error, statusCode) = await adminService.DeleteUserAsync(id, ct);
        if (!success)
            return StatusCode(statusCode, new { error });

        return NoContent();
    }
}