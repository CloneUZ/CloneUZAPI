using CloneUZ.BLL.Interfaces;
using DTOs.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CloneUZ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")] -> включен одразу після підключення jwt в AuthController
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Користувача не знайдено." });
        }

        return Ok(user);
    }

    [HttpPatch("users/{id:guid}/ban")]
    public async Task<IActionResult> SetBanStatus(Guid id, [FromBody] BanUserRequestDto request)
    {
        try
        {
            var updatedUser = await _adminService.SetUserBanStatusAsync(id, request);
            if (updatedUser == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var deleted = await _adminService.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            return Ok(new { message = "Користувача успішно видалено." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}