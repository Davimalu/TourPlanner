using Microsoft.EntityFrameworkCore;
using TourPlanner.RestServer.Models;

namespace TourPlanner.RestServer.DAL;

public class AppDbContext : DbContext
{
    // Constructor supporting dependency injection
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    
    // Define the DbSets for the entities
    public DbSet<Tour> Tours { get; set; }
    // We explicitly declare a DBSet for TourLogs so that it can be queried on its own (otherwise the table would still be created as the TourModel requires it,
    // but we couldn't query it directly since there would be no DbSet / Context for it)
    public DbSet<TourLog> TourLogs { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Saving the DateTime to the database in local time throws an error (and could lead to inconsistencies), so we need to convert it to UTC before saving
        modelBuilder.Entity<TourLog>()
            .Property(tl => tl.TimeStamp)
            .HasConversion(
                // Convert the value to UTC before saving:
                v => v.ToUniversalTime(),
                // When reading from the database, mark as UTC:
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

        // Call the base method to ensure any other configurations are applied (our OnModelCreating method overrides the base method of the EntityFrameworkCore DbContext,
        // so if we still want the logic in the original method to execute, we need to call it explicitly)
        base.OnModelCreating(modelBuilder);
    }
}