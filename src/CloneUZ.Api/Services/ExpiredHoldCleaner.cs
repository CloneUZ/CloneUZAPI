using System.ComponentModel.DataAnnotations;
using CloneUZ.Api.Data;
using Microsoft.Extensions.Options;

namespace CloneUZ.Api.Services;

public sealed class ExpiredHoldSweeperOptions
{
    public const string Section = "ExpiredHoldSweeper";

    [Range(typeof(TimeSpan), "00:00:05", "00:10:00")]
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);

    [Range(10, 10_000)]
    public int BatchSize { get; set; } = 500;

    [Range(1, 1_000)]
    public int MaxBatchesPerRun { get; set; } = 20;
}

public interface IExpiredHoldCleaner
{
    Task<int> CleanAsync(CancellationToken ct);
}

public sealed class ExpiredHoldCleaner(ISeatRepository seats, IOptions<ExpiredHoldSweeperOptions> options, TimeProvider clock)
    : IExpiredHoldCleaner
{
    private readonly ExpiredHoldSweeperOptions _options = options.Value;

    public async Task<int> CleanAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var total = 0;

        for (var i = 0; i < _options.MaxBatchesPerRun; i++)
        {
            var released = await seats.ReleaseExpiredBatchAsync(now, _options.BatchSize, ct);
            total += released;
            if (released < _options.BatchSize) break;
        }

        return total;
    }
}
