using CloneUZ.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.Api.Data;

/// <summary>
/// Доступ к местам в БД. Interface needed for mock unit test 
/// </summary>
public interface ISeatRepository
{
    Task<WagonType?> GetWagonTypeAsync(Guid tripId, int wagonNumber, CancellationToken ct);
    Task<List<Seat>> GetSeatsAsync(Guid tripId, int wagonNumber, CancellationToken ct);
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
}
