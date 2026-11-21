namespace DTOs.Admin;

public class BanUserRequestDto
{
    public bool IsBanned { get; set; }
    public string? Reason { get; set; }
}