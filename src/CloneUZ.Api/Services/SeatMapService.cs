using CloneUZ.Api.Data;
using CloneUZ.Api.Domain;

namespace CloneUZ.Api.Services;

public sealed record SeatDto(
    int Number, int Compartment, BerthLevel Berth, SeatFeatures Features, int X, int Y,
    SeatStatus Status, bool IsMine, DateTimeOffset? ReservedUntil);

public sealed record SeatMapDto(Guid TripId, int WagonNumber, WagonType WagonType, int AvailableCount, List<SeatDto> Seats);

public interface ISeatMapService
{
    Task<SeatMapDto?> GetSeatMapAsync(Guid tripId, int wagonNumber, Guid? currentUserId, CancellationToken ct);
}

/// <summary>
/// Схема вагона = геометрия (из памяти, WagonLayouts) + текущие статусы мест (из БД).
/// </summary>
public sealed class SeatMapService(ISeatRepository seats, TimeProvider clock) : ISeatMapService
{
    public async Task<SeatMapDto?> GetSeatMapAsync(Guid tripId, int wagonNumber, Guid? currentUserId, CancellationToken ct)
    {
        var wagonType = await seats.GetWagonTypeAsync(tripId, wagonNumber, ct);
        if (wagonType is null) return null;

        var seatsByNumber = (await seats.GetSeatsAsync(tripId, wagonNumber, ct)).ToDictionary(s => s.SeatNumber);
        var now = clock.GetUtcNow();

        var result = new List<SeatDto>();
        foreach (var layout in WagonLayouts.For(wagonType.Value))
        {
            seatsByNumber.TryGetValue(layout.Number, out var seat);
            var (status, isMine, reservedUntil) = GetState(seat, currentUserId, now);
            result.Add(new SeatDto(layout.Number, layout.Compartment, layout.Berth, layout.Features,
                layout.X, layout.Y, status, isMine, reservedUntil));
        }

        var available = result.Count(s => s.Status == SeatStatus.Available);
        return new SeatMapDto(tripId, wagonNumber, wagonType.Value, available, result);
    }

    private static (SeatStatus Status, bool IsMine, DateTimeOffset? ReservedUntil) GetState(Seat? seat, Guid? userId, DateTimeOffset now)
    {
        if (seat is null) return (SeatStatus.Sold, false, null);

        if (seat.IsAvailable(now)) return (SeatStatus.Available, false, null);

        var isMine = userId is not null && seat.Status == SeatStatus.Reserved && seat.ReservedBy == userId;

        return (seat.Status, isMine, isMine ? seat.ReservedUntil : null);
    }
}
