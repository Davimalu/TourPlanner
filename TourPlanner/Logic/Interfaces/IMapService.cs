using TourPlanner.Model;
using TourPlanner.Model.Structs;

namespace TourPlanner.Logic.Interfaces;

public interface IMapService
{
    Task InitializeAsync();
    Task<bool> DrawRouteAsync(string geoJsonRoute);
    Task<bool> AddMarkerAsync(MapMarker marker);
    Task<bool> ClearMapAsync();
    
    event EventHandler<GeoCoordinate>? MapClicked;
}