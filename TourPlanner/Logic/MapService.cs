using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Structs;

namespace TourPlanner.Logic;

public class MapService : IMapService
{
    private readonly List<MapLocation> _locations = new();
    
    public MapService()
    {
        // Sample data for testing
        // TODO: Remove
        _locations.AddRange(new[]
        {
            new MapLocation(new GeoCoordinate(48.2082, 16.3738), "Vienna", "Capital of Austria"),
            new MapLocation(new GeoCoordinate(52.5200, 13.4049), "Germany", "Capital of Germany"),
            new MapLocation(new GeoCoordinate(41.9027, 12.4963), "Rome", "Capital of Italy"),
            new MapLocation(new GeoCoordinate(51.5098, -0.1180), "London", "Capital of the United Kingdom"),
        });
    }
    
    // TODO: There's not really a reason to use async here
    
    public Task<IEnumerable<MapLocation>> GetMarkersAsync()
    {
        return Task.FromResult<IEnumerable<MapLocation>>(_locations);
    }
    
    public Task<bool> AddMarkerAsync(MapLocation location)
    {
        if (location == null || _locations.Contains(location))
        {
            return Task.FromResult(false);
        }
        
        _locations.Add(location);
        return Task.FromResult(true);
    }
    
    public Task<bool> RemoveMarkerAsync(MapLocation location)
    {
        if (location == null || !_locations.Contains(location))
        {
            return Task.FromResult(false);
        }
        
        _locations.Remove(location);
        return Task.FromResult(true);
    }
}