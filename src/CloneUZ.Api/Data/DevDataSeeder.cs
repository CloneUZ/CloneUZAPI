using CloneUZ.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.Api.Data;

/// <summary>
/// Demo record db
/// </summary>
public static class DevDataSeeder
{
    public static readonly Guid DemoTripId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.TripWagons.AnyAsync(w => w.TripId == DemoTripId)) return;

        var wagons = new[]
        {
            new TripWagon { TripId = DemoTripId, Number = 1, Type = WagonType.Lux },
            new TripWagon { TripId = DemoTripId, Number = 2, Type = WagonType.Compartment },
            new TripWagon { TripId = DemoTripId, Number = 3, Type = WagonType.Platzkart },
        };
        db.TripWagons.AddRange(wagons);

        foreach (var wagon in wagons)
            foreach (var layout in WagonLayouts.For(wagon.Type))
                db.Seats.Add(new Seat(DemoTripId, wagon.Number, layout.Number));

        await db.SaveChangesAsync();
    }
}
