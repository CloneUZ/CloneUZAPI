using CloneUZ.Api.Common;
using CloneUZ.Api.Data;
using CloneUZ.Api.Domain;
using CloneUZ.Api.Realtime;
using Microsoft.Extensions.Options;

namespace CloneUZ.Api.Services;

public sealed record HoldSeatsCommand(Guid UserId, Guid TripId, int WagonNumber, IReadOnlyList<int> Seats);

public sealed record HoldDto(Guid TripId, int WagonNumber, IReadOnlyList<int> Seats, DateTimeOffset ExpiresAt);

public interface IHoldService
{
    Task<Result<HoldDto>> HoldAsync(HoldSeatsCommand command, CancellationToken ct);
}

public sealed class HoldService(
    ISeatRepository seats,
    ISeatLockService locks,
    ISeatMapNotifier notifier,
    IOptions<SeatHoldOptions> options,
    TimeProvider clock,
    ILogger<HoldService> logger) : IHoldService
{
    private readonly SeatHoldOptions _options = options.Value;

    public async Task<Result<HoldDto>> HoldAsync(HoldSeatsCommand command, CancellationToken ct)
    {
        var seatNumbers = command.Seats.Distinct().Order().ToArray();

        var error = await ValidateAsync(command, seatNumbers, ct);
        if (error is not null) return error;

        var now = clock.GetUtcNow();
        var expiresAt = now + _options.Ttl;

        var redisLocked = false;
        try
        {
            var conflicts = await locks.TryAcquireAsync(command.TripId, command.WagonNumber, seatNumbers, command.UserId, _options.Ttl);
            if (conflicts.Length > 0)
                return Error.Conflict("seats_unavailable", "Some of the selected seats are no longer available.",
                    new Dictionary<string, object?> { ["seats"] = conflicts });
            redisLocked = true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          
            logger.LogWarning(ex, "Redis seat lock unavailable, falling back to database only");
        }

        var reserved = await seats.TryReserveAsync(command.TripId, command.WagonNumber, seatNumbers, command.UserId, now, expiresAt, ct);
        if (!reserved)
        {
            if (redisLocked) await TryUnlockAsync(command, seatNumbers);
            return Error.Conflict("seats_unavailable", "Some of the selected seats are no longer available.",
                new Dictionary<string, object?> { ["seats"] = seatNumbers });
        }

        await notifier.SeatsChangedAsync(command.TripId, command.WagonNumber, seatNumbers, SeatStatus.Reserved);
        return new HoldDto(command.TripId, command.WagonNumber, seatNumbers, expiresAt);
    }

    private async Task TryUnlockAsync(HoldSeatsCommand command, int[] seatNumbers)
    {
        try { await locks.ReleaseAsync(command.TripId, command.WagonNumber, seatNumbers, command.UserId); }
        catch (Exception ex) { logger.LogWarning(ex, "Failed to roll back Redis seat lock"); }
    }

    private async Task<Error?> ValidateAsync(HoldSeatsCommand command, int[] seatNumbers, CancellationToken ct)
    {
        if (seatNumbers.Length == 0)
            return Error.Validation("seats_required", "At least one seat must be selected.");
        if (seatNumbers.Length > _options.MaxSeatsPerHold)
            return Error.Validation("too_many_seats", $"At most {_options.MaxSeatsPerHold} seats per hold.");

        var wagonType = await seats.GetWagonTypeAsync(command.TripId, command.WagonNumber, ct);
        if (wagonType is null)
            return Error.NotFound("wagon_not_found", $"Wagon {command.WagonNumber} not found on trip {command.TripId}.");

        var existing = WagonLayouts.For(wagonType.Value).Select(l => l.Number).ToHashSet();
        var invalid = seatNumbers.Where(s => !existing.Contains(s)).ToArray();
        return invalid.Length > 0
            ? Error.Validation("invalid_seat_number", $"Seats {string.Join(", ", invalid)} do not exist in this wagon.")
            : null;
    }
}
