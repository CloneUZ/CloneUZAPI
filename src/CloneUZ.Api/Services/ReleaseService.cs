using CloneUZ.Api.Data;
using CloneUZ.Api.Domain;

namespace CloneUZ.Api.Services;

public sealed record ReleaseSeatsCommand(Guid UserId, Guid TripId, int WagonNumber, IReadOnlyList<int>? Seats);

public interface IReleaseService
{
    Task<int> ReleaseAsync(ReleaseSeatsCommand command, CancellationToken ct);
}

public sealed class ReleaseService(ISeatRepository seats, ISeatLockService locks, ILogger<ReleaseService> logger) : IReleaseService
{
    public async Task<int> ReleaseAsync(ReleaseSeatsCommand command, CancellationToken ct)
    {
        var seatNumbers = command.Seats?.Distinct().ToArray();

        var released = await seats.ReleaseAsync(command.TripId, command.WagonNumber, seatNumbers, command.UserId, ct);

        try
        {
            var redisSeats = seatNumbers ?? await AllSeatsAsync(command, ct);
            if (redisSeats.Length > 0)
                await locks.ReleaseAsync(command.TripId, command.WagonNumber, redisSeats, command.UserId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to release Redis seat locks");
        }

        return released;
    }
    
    private async Task<int[]> AllSeatsAsync(ReleaseSeatsCommand command, CancellationToken ct)
    {
        var type = await seats.GetWagonTypeAsync(command.TripId, command.WagonNumber, ct);
        return type is null ? [] : WagonLayouts.For(type.Value).Select(l => l.Number).ToArray();
    }
}
