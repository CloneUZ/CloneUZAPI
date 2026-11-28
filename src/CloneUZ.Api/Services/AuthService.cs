using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CloneUZ.Api.Contracts.Auth;
using CloneUZ.Api.Data;
using CloneUZ.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CloneUZ.Api.Services;

public interface IAuthService
{
    Task<(AuthResponse? Response, IReadOnlyList<string> Errors, int StatusCode)> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<(AuthResponse? Response, IReadOnlyList<string> Errors, int StatusCode)> LoginAsync(LoginRequest request, CancellationToken ct);
}

public sealed class AuthService(
    AppDbContext db,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<(AuthResponse? Response, IReadOnlyList<string> Errors, int StatusCode)> RegisterAsync(
        RegisterRequest request, CancellationToken ct)
    {
        var validationErrors = AuthValidator.ValidateRegister(request);
        if (validationErrors.Count > 0)
            return (null, validationErrors, StatusCodes.Status400BadRequest);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (exists)
            return (null, ["Користувач із такою поштою вже зареєстрований."], StatusCodes.Status409Conflict);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            PhoneNumber = request.PhoneNumber?.Trim(),
            BirthDate = request.BirthDate,
            Citizenship = request.Citizenship?.Trim(),
            DocumentNumber = request.DocumentNumber?.Trim(),
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return (GenerateTokenResponse(user), [], StatusCodes.Status201Created);
    }

    public async Task<(AuthResponse? Response, IReadOnlyList<string> Errors, int StatusCode)> LoginAsync(
        LoginRequest request, CancellationToken ct)
    {
        var validationErrors = AuthValidator.ValidateLogin(request);
        if (validationErrors.Count > 0)
            return (null, validationErrors, StatusCodes.Status400BadRequest);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (null, ["Невірний email або пароль."], StatusCodes.Status401Unauthorized);

        if (user.IsBanned)
            return (null, [$"Акаунт заблоковано. Причина: {user.BanReason ?? "не вказана"}"], StatusCodes.Status403Forbidden);

        return (GenerateTokenResponse(user), [], StatusCodes.Status200OK);
    }

    private AuthResponse GenerateTokenResponse(User user)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_jwt.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse(
            AccessToken: tokenString,
            ExpiresAtUtc: expires,
            UserId: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: user.Role);
    }
}