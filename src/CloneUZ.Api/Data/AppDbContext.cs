using CloneUZ.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<TripWagon> TripWagons => Set<TripWagon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Seat>(seat =>
        {
            seat.HasKey(s => s.Id);

            // Одно место в вагоне рейса существует один раз — защита от дублей на уровне БД.
            // Этот же индекс ускоряет главный запрос «все места вагона».
            seat.HasIndex(s => new { s.TripId, s.WagonNumber, s.SeatNumber }).IsUnique();

            // Оптимистичная блокировка: uint + IsRowVersion → Npgsql использует системную колонку xmin.
            seat.Property(s => s.Version).IsRowVersion();

            // Для фоновой очистки просроченных резервов (шаг 7).
            seat.HasIndex(s => s.ReservedUntil);
        });

        modelBuilder.Entity<TripWagon>(wagon =>
        {
            wagon.HasKey(w => new { w.TripId, w.Number });
        });
    }
}
