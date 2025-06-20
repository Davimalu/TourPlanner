using System.Text.Json;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Structs;

namespace TourPlanner.Logic;

public class MapService : IMapService
{
    private readonly IWebViewService _webViewService;
    private readonly ILoggerWrapper _logger;
    
    private bool _isReady = false;
    public event EventHandler<GeoCoordinate>? MapClicked;
    
    public MapService(IWebViewService webViewService)
    {
        _webViewService = webViewService ?? throw new ArgumentNullException(nameof(webViewService));
        _logger = LoggerFactory.GetLogger<MapService>();
    }
    
    
    /// <summary>
    /// Initializes the map service by ensuring the WebView is ready and setting up necessary event handlers
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isReady)
        {
            _logger.Warn("Map service is already initialized.");
            return;
        }
        
        try
        {
            await _webViewService.InitializeAsync();
            _webViewService.MessageReceived += OnWebViewMessageReceived;
            
            _isReady = true;
            _logger.Info("Map service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to initialize map service: {ex.Message}", ex);
            throw;
        }
    }
    
    
    /// <summary>
    /// Adds a marker to the map at the specified coordinates with an optional description
    /// </summary>
    /// <param name="marker">The marker to add, containing coordinates, name, and optional description</param>
    /// <returns></returns>
    public async Task<bool> AddMarkerAsync(MapMarker marker)
    {
        if (!_isReady)
        {
            _logger.Warn("Failed to add marker: Map service is not ready.");
            return false;
        }

        try
        {
            var result = await _webViewService.CallFunctionAsync("addMarker", marker.Coordinates.Latitude, marker.Coordinates.Longitude, marker.Name, marker.Description ?? "");
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
    /// Draws a route on the map using a GeoJSON string representation of the route
    /// </summary>
    /// <param name="geoJsonRoute">The GeoJSON string representing the route to draw</param>
    /// <returns></returns>
    public async Task<bool> DrawRouteAsync(string geoJsonRoute)
    {
        if (!_isReady)
        {
            _logger.Warn("Failed to draw route: Map service is not ready.");
            return false;
        }

        try
        {
            var result = await _webViewService.CallFunctionAsync("drawRoute", geoJsonRoute);
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
        if (!_isReady)
        {
            _logger.Warn("Failed to clear map: Map service is not ready.");
            return false;
        }

        try
        {
            var result = await _webViewService.CallFunctionAsync("clearMap");
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
    /// Handles messages received from the WebView, specifically looking for map click events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    private void OnWebViewMessageReceived(object? sender, string message)
    {
        try
        {
            var mapMessage = JsonSerializer.Deserialize<MapMessage>(message);
            if (mapMessage?.Type == "MapClick")
            {
                MapClicked?.Invoke(this, new GeoCoordinate(mapMessage.Lat, mapMessage.Lon));
                _logger.Debug($"Map clicked at ({mapMessage.Lat}, {mapMessage.Lon})");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to parse map message: {ex.Message}", ex);
        }
    }
    
    
    /// <summary>
    /// used to deserialize messages from the WebView
    /// </summary>
    private class MapMessage
    {
        public string Type { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}