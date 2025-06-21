using TourPlanner.Model;
using TourPlanner.Model.Structs;

namespace TourPlanner.Logic.Interfaces;

public interface IMapService
{
    Task<bool> DrawRouteAsync(string geoJsonRoute);
    Task<bool> AddMarkerAsync(MapMarker marker);
    Task<bool> RemoveMarkerByTitleAsync(string markerTitle);
    Task<bool> SetViewToCoordinatesAsync(GeoCoordinate coordinates, int zoomLevel = 15);
    Task<bool> ClearMapAsync();
    Task<bool> SwitchControlToMainMapAsync();
    
    event EventHandler<GeoCoordinate>? MapClicked;
}