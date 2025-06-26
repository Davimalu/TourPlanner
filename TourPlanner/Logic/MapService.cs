using System.IO;
using TourPlanner.config.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;
using TourPlanner.Model.Structs;

namespace TourPlanner.Logic;

public class MapService : IMapService
{
    private readonly IWebViewService _webViewService;
    private readonly IEventAggregator _eventAggregator;
    private readonly ITourPlannerConfig _tourPlannerConfig;
    private readonly ILogger<MapService> _logger;
    
    private const string TemporaryImageName = "MapImage.png";
    
    public MapService(IWebViewService webViewService, ITourPlannerConfig tourPlannerConfig, IEventAggregator eventAggregator, ILogger<MapService> logger)
    {
        _webViewService = webViewService ?? throw new ArgumentNullException(nameof(webViewService));
        _tourPlannerConfig = tourPlannerConfig ?? throw new ArgumentNullException(nameof(tourPlannerConfig));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    
    /// <summary>
    /// Adds a marker to the map at the specified coordinates with an optional description
    /// </summary>
    /// <param name="marker">The marker to add, containing coordinates, name, and optional description</param>
    /// <returns></returns>
    public async Task<bool> AddMarkerAsync(MapMarker marker)
    {
        if (!_webViewService.IsReady)
        {
            _logger.Warn("Failed to add marker: WebView is not ready.");
            return false;
        }

        try
        {
            await _webViewService.CallFunctionAsync("addMarker", marker.Coordinates.Latitude, marker.Coordinates.Longitude, marker.Name, marker.Description);
            _logger.Debug($"Added marker at {marker.Coordinates.Latitude}, {marker.Coordinates.Longitude} with name '{marker.Name}' and description '{marker.Description}'");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to add marker: {ex.Message}", ex);
            return false;
        }
    }


    /// <summary>
    /// Removes a marker from the map by its title
    /// </summary>
    /// <param name="markerTitle">The title of the marker to remove</param>
    /// <returns></returns>
    public async Task<bool> RemoveMarkerByTitleAsync(string markerTitle)
    {
        if (!_webViewService.IsReady)
        {
            _logger.Warn("Failed to add marker: WebView is not ready.");
            return false;
        }
        
        try
        {
            await _webViewService.CallFunctionAsync("removeMarkerByTitle", markerTitle);
            _logger.Debug($"Removed marker with title '{markerTitle}'");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to remove marker: {ex.Message}", ex);
            return false;
        }
    }


    /// <summary>
    /// Sets the map view to the specified coordinates and zoom level
    /// </summary>
    /// <param name="coordinates">Coordinates to center the map on</param>
    /// <param name="zoomLevel">Zoom level to set the map to (defaults to 15)</param>
    /// <returns></returns>
    public async Task<bool> SetViewToCoordinatesAsync(GeoCoordinate coordinates, int zoomLevel = 15)
    {
        if (!_webViewService.IsReady)
        {
            _logger.Warn("Failed to add marker: WebView is not ready.");
            return false;
        }
        
        try 
        {
            await _webViewService.CallFunctionAsync("flyToLocation", coordinates.Latitude, coordinates.Longitude, zoomLevel);
            _logger.Debug($"Set map view to coordinates ({coordinates.Latitude}, {coordinates.Longitude})");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to set view to coordinates: {ex.Message}", ex);
            return false;
        }
    }
    
    
    /// <summary>
    /// Draws a route on the map using a GeoJSON string representation of the route
    /// </summary>
    /// <param name="geoJsonRoute">The GeoJSON string representing the route to draw</param>
    /// <returns></returns>
    public async Task<bool> DrawRouteAsync(string geoJsonRoute)
    {
        if (!_webViewService.IsReady)
        {
            _logger.Warn("Failed to draw route: WebView is not ready.");
            return false;
        }

        try
        {
            await _webViewService.CallFunctionAsync("drawRoute", geoJsonRoute);
            _logger.Debug("Route drawn successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to draw route: {ex.Message}", ex);
            return false;
        }
    }
    
    
    /// <summary>
    /// Clears the map by removing all markers and routes
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ClearMapAsync()
    {
        if (!_webViewService.IsReady)
        {
            _logger.Warn("Failed to clear map: WebView is not ready.");
            return false;
        }

        try
        {
            await _webViewService.CallFunctionAsync("clearMap");
            _logger.Debug("Map cleared successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to clear map: {ex.Message}", ex);
            return false;
        }
    }


    /// <summary>
    /// Switches the control back to the main map in the MainWindow's WebView
    /// </summary>
    /// <returns>>true if the switch was successful, false otherwise</returns>
    public async Task<bool> SwitchControlToMainMapAsync()
    {
        _logger.Debug("Switching control back to the main map in the MainWindow's WebView...");
        return await _webViewService.RevertToMainWindowWebViewAsync();
    }
    
    
    public async Task<string> CaptureMapImageAsync(Tour tour)
    {
        if (!_webViewService.IsReady)
        {
            _logger.Warn("Failed to capture map image: WebView is not ready.");
            return string.Empty;
        }
        
        if (tour.StartCoordinates == null || tour.EndCoordinates == null || string.IsNullOrEmpty(tour.GeoJsonString))
        {
            _logger.Warn("Failed to capture map image: Tour does not have valid start / end coordinates or route data.");
            return string.Empty;
        }
        
        // Inform others that a screenshot of the map is about to be taken
        _eventAggregator.Publish(new MapScreenshotRequestedEvent());

        // Ensure the map displays the tour's route before capturing the image
        await ClearMapAsync();
        await AddMarkerAsync(new MapMarker((GeoCoordinate)tour.StartCoordinates, "Start", tour.StartLocation));
        await AddMarkerAsync(new MapMarker((GeoCoordinate)tour.EndCoordinates, "End", tour.EndLocation));
        await DrawRouteAsync(tour.GeoJsonString);

        try
        {
            var success = await _webViewService.TakeScreenshotAsync(Path.Combine(_tourPlannerConfig.TmpFolder, TemporaryImageName));
            
            if (!success)
            {
                _logger.Error("Failed to capture map image: Screenshot operation was not successful.");
                return string.Empty;
            }
            
            _logger.Debug($"Captured map image for tour '{tour.TourName}'");
            return _tourPlannerConfig.TmpFolder + TemporaryImageName;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to capture map image: {ex.Message}", ex);
            return string.Empty;
        }
    }
}