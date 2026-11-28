using CloneUZ.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<TripWagon> TripWagons => Set<TripWagon>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Seat>(seat =>
        {
            seat.HasKey(s => s.Id);
            seat.HasIndex(s => new { s.TripId, s.WagonNumber, s.SeatNumber }).IsUnique();
            seat.Property(s => s.Version).IsRowVersion();
            seat.HasIndex(s => s.ReservedUntil);
        });

        modelBuilder.Entity<TripWagon>(wagon =>
        {
            wagon.HasKey(w => new { w.TripId, w.Number });
        });

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.FirstName).HasMaxLength(100);
            user.Property(u => u.LastName).HasMaxLength(100);
            user.Property(u => u.Email).HasMaxLength(255);
        });
    }
}