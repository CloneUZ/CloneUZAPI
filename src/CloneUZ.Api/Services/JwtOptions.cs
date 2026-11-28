using System.ComponentModel.DataAnnotations;

namespace CloneUZ.Api.Services;

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    [Required]
    public string Issuer { get; init; } = "CloneUZ.Api";

    [Required]
    public string Audience { get; init; } = "CloneUZ.Client";

    [Required, MinLength(32)]
    public string SecretKey { get; init; } = string.Empty;

    [Range(5, 10080)]
    public int ExpiresMinutes { get; init; } = 120;
}