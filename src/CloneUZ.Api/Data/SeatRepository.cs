using System.Data;
using CloneUZ.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.Api.Data;

/// <summary>
/// Interface needed for mock unit test 
/// </summary>
public interface ISeatRepository
{
    Task<WagonType?> GetWagonTypeAsync(Guid tripId, int wagonNumber, CancellationToken ct);
    Task<List<Seat>> GetSeatsAsync(Guid tripId, int wagonNumber, CancellationToken ct);

    Task<bool> TryReserveAsync(Guid tripId, int wagonNumber, IReadOnlyCollection<int> seatNumbers, Guid userId,
        DateTimeOffset now, DateTimeOffset reservedUntil, CancellationToken ct);

    Task<int> ReleaseAsync(Guid tripId, int wagonNumber, IReadOnlyCollection<int>? seatNumbers, Guid userId, CancellationToken ct);
}

public sealed class SeatRepository(AppDbContext db) : ISeatRepository
{
    public Task<WagonType?> GetWagonTypeAsync(Guid tripId, int wagonNumber, CancellationToken ct) =>
        db.TripWagons
            .Where(w => w.TripId == tripId && w.Number == wagonNumber)
            .Select(w => (WagonType?)w.Type)
            .FirstOrDefaultAsync(ct);

    public Task<List<Seat>> GetSeatsAsync(Guid tripId, int wagonNumber, CancellationToken ct) =>
        db.Seats
            .AsNoTracking() // AsNoTracking!!
            .Where(s => s.TripId == tripId && s.WagonNumber == wagonNumber)
            .ToListAsync(ct);

    public async Task<bool> TryReserveAsync(Guid tripId, int wagonNumber, IReadOnlyCollection<int> seatNumbers, Guid userId,
        DateTimeOffset now, DateTimeOffset reservedUntil, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        
        var numbers = seatNumbers.ToArray();
        await db.Database
            .SqlQuery<int>($"""
                SELECT "Id" AS "Value" FROM "Seats"
                WHERE "TripId" = {tripId} AND "WagonNumber" = {wagonNumber} AND "SeatNumber" = ANY({numbers})
                ORDER BY "SeatNumber"
                FOR UPDATE
                """)
            .ToListAsync(ct);

        var updated = await db.Seats
            .Where(s => s.TripId == tripId && s.WagonNumber == wagonNumber && seatNumbers.Contains(s.SeatNumber)
                        && (s.Status == SeatStatus.Available
                            || (s.Status == SeatStatus.Reserved && s.ReservedUntil <= now)))
            .ExecuteUpdateAsync(u => u
                .SetProperty(s => s.Status, SeatStatus.Reserved)
                .SetProperty(s => s.ReservedBy, (Guid?)userId)
                .SetProperty(s => s.ReservedUntil, (DateTimeOffset?)reservedUntil), ct);

        if (updated != seatNumbers.Count)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        await tx.CommitAsync(ct);
        return true;
    }

    public Task<int> ReleaseAsync(Guid tripId, int wagonNumber, IReadOnlyCollection<int>? seatNumbers, Guid userId, CancellationToken ct)
    {
        var query = db.Seats.Where(s => s.TripId == tripId && s.WagonNumber == wagonNumber
                                        && s.Status == SeatStatus.Reserved && s.ReservedBy == userId);
        if (seatNumbers is { Count: > 0 })
            query = query.Where(s => seatNumbers.Contains(s.SeatNumber));

        return query.ExecuteUpdateAsync(u => u
            .SetProperty(s => s.Status, SeatStatus.Available)
            .SetProperty(s => s.ReservedBy, (Guid?)null)
            .SetProperty(s => s.ReservedUntil, (DateTimeOffset?)null), ct);
    }
}
