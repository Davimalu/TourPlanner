using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces;

public interface IMapService
{
    Task<IEnumerable<MapLocation>> GetMarkersAsync();
    Task<bool> AddMarkerAsync(MapLocation location);
    Task<bool> RemoveMarkerAsync(MapLocation location);
}