using CloneUZ.Api.Data;

namespace CloneUZ.Api.Services;

public sealed record ReleaseSeatsCommand(Guid UserId, Guid TripId, int WagonNumber, IReadOnlyList<int>? Seats);

public interface IReleaseService
{
    Task<int> ReleaseAsync(ReleaseSeatsCommand command, CancellationToken ct);
}

public sealed class ReleaseService(ISeatRepository seats) : IReleaseService
{
    public Task<int> ReleaseAsync(ReleaseSeatsCommand command, CancellationToken ct)
    {
        var seatNumbers = command.Seats?.Distinct().ToArray();
        return seats.ReleaseAsync(command.TripId, command.WagonNumber, seatNumbers, command.UserId, ct);
    }
}
