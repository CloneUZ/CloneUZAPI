using System.ComponentModel.DataAnnotations;

namespace CloneUZ.Api.Services;

public sealed class SeatHoldOptions
{
    public const string Section = "SeatHold";

    [Range(typeof(TimeSpan), "00:01:00", "01:00:00")]
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(15);

    [Range(1, 10)]
    public int MaxSeatsPerHold { get; set; } = 4;
}
