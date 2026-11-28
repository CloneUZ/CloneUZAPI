using System.Security.Claims;
using CloneUZ.Api.Contracts.Auth;
using CloneUZ.Api.Data;
using CloneUZ.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, AppDbContext db) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var (response, errors, statusCode) = await authService.RegisterAsync(request, ct);
        if (errors.Count > 0)
            return StatusCode(statusCode, new { errors });

        return StatusCode(statusCode, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var (response, errors, statusCode) = await authService.LoginAsync(request, ct);
        if (errors.Count > 0)
            return StatusCode(statusCode, new { errors });

        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT є stateless: клієнт (фронтенд) видаляє токен зі свого сховища.
        return Ok(new { message = "Ви успішно вийшли з системи." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(rawId, out var userId))
            return Unauthorized();

        var user = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.IsBanned,
                u.PhoneNumber,
                u.BirthDate,
                u.Citizenship,
                u.DocumentNumber,
                u.CreatedAtUtc
            })
            .FirstOrDefaultAsync(ct);

        return user is null ? NotFound() : Ok(user);
    }
}