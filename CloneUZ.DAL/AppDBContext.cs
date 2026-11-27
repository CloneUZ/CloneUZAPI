using CloneUZ.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.DAL;

public class AppDBContext:DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }

    public DbSet<Station> Stations { get; set; }
    public DbSet<RouteStation> RouteStations { get; set; }
    public DbSet<Route> Routes { get; set; }
    
    public DbSet<Train> Trains { get; set; }
    
    public DbSet<TrainRun> TrainRuns { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TrainRun>().HasOne(tr => tr.Train).WithMany(t => t.TrainsRun)
            .HasForeignKey(tr => tr.TrainId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TrainRun>().HasOne(tr => tr.Route).WithMany(t => t.TrainRun)
            .HasForeignKey(tr => tr.RouteId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RouteStation>().HasOne(rs=>rs.Routes).WithMany(r=>r.Sations)
            .HasForeignKey(rs=>rs.RoutesId).OnDelete(DeleteBehavior.Cascade);
    }
}
