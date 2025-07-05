using Microsoft.EntityFrameworkCore;
using TourPlanner.Model;
using TourPlanner.Model.Structs;

namespace TourPlanner.RestServer.DAL;

public class AppDbContext : DbContext
{
    // Constructor supporting dependency injection
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    
    // Define the DbSets for the entities
    public DbSet<Tour> Tours { get; set; }
    // We explicitly declare a DBSet for TourLogs so that it can be queried on its own (otherwise the table would still be created since the Tour Model requires it,
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
        
        // The TourModel stores the Start and End coordinates as a GeoCoordinates struct - EF Core doesn't support structs, so we need to explictly define how to map it to the database
        modelBuilder.Entity<Tour>()
            .Property(t => t.StartCoordinates)
            .HasConversion(
                // Convert the GeoCoordinate struct to a string for saving (e.g. "latitude,longitude")
                v => v.HasValue ? $"{v.Value.Latitude}|{v.Value.Longitude}" : null,
                // When reading from the database, convert the string back to a GeoCoordinate struct
                v => string.IsNullOrEmpty(v) ? null : ParseGeoCoordinate(v));

        modelBuilder.Entity<Tour>()
            .Property(t => t.EndCoordinates)
            .HasConversion(
                // Convert the GeoCoordinate struct to a string for saving (e.g. "latitude,longitude")
                v => v.HasValue ? $"{v.Value.Latitude}|{v.Value.Longitude}" : null,
                // When reading from the database, convert the string back to a GeoCoordinate struct
                v => string.IsNullOrEmpty(v) ? null : ParseGeoCoordinate(v));

        // Call the base method to ensure any other configurations are applied (our OnModelCreating method overrides the base method of the EntityFrameworkCore DbContext,
        // so if we still want the logic in the original method to execute, we need to call it explicitly)
        base.OnModelCreating(modelBuilder);
    }
    
    
    
    private GeoCoordinate? ParseGeoCoordinate(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        
        // The GeoCoordinate is stored as a string in the format "latitude,longitude"
        var parts = value.Split('|');

        if (parts.Length != 2)
        {
            return null;
        }
        
        if (double.TryParse(parts[0], out var latitude) && double.TryParse(parts[1], out var longitude))
        {
            return new GeoCoordinate(latitude, longitude);
        }
    
        return null;
    }
}