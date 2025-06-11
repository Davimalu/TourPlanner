using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using TourPlanner.Enums;
using TourPlanner.Infrastructure;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents;

public class MapService
{
    private readonly HttpClient _http = new();
    private readonly string _apiKey;
    
    // Add a model for parsing the full response from ORS
    private class OrsResponse
    {
        [JsonPropertyName("features")]
        public List<OrsFeature> Features { get; set; }
    }
    private class OrsFeature
    {
        [JsonPropertyName("geometry")]
        public JsonElement Geometry { get; set; }
        [JsonPropertyName("properties")]
        public OrsProperties Properties { get; set; }
    }
    private class OrsProperties
    {
        [JsonPropertyName("summary")]
        public OrsSummary Summary { get; set; }
    }
    private class OrsSummary
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; } // in meters
        [JsonPropertyName("duration")]
        public double Duration { get; set; } // in seconds
    }


// FILE: DAL/ServiceAgents/MapService.cs

    public MapService()
    {
        try
        {
            // Get the directory where the .exe is running
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // Combine it with the relative path to the config file
            string configPath = Path.Combine(baseDirectory, "config", "appsettings.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file not found at the expected path: {configPath}");
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

            var section = configuration.GetSection("OpenRouteService");
            _apiKey = section["ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("OpenRouteService API key is missing or empty in config/appsettings.json.");
            }
        }
        catch (Exception ex)
        {
            // This will now log the exact reason for the failure.
            System.Diagnostics.Debug.WriteLine($"FATAL ERROR initializing MapService: {ex.ToString()}");
            // Re-throw the exception to make it clear that the service is unusable.
            throw; 
        }
    }

    private string GetProfileForTransportType(Transport transportType)
    {
        return transportType switch
        {
            Transport.Car => "driving-car",
            Transport.Bicycle => "cycling-road",
            Transport.Foot => "foot-walking",
            _ => "driving-car" // Default case
        };
    }

    /// returns structured route information including GeoJSON, distance, and duration
    public async Task<RouteInfo?> GetRouteAsync(
        Transport mode, (double lon, double lat) start, (double lon, double lat) end)
    {
        var url = $"https://api.openrouteservice.org/v2/directions/{GetProfileForTransportType(mode)}/geojson";
        var body = JsonSerializer.Serialize(new
        {
            coordinates = new[] {
                new[] { start.lon, start.lat },
                new[] { end.lon,   end.lat   }
            }
        });

        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        req.Headers.Add("Authorization", _apiKey);     // preferred over query string
        var res = await _http.SendAsync(req);

        if (!res.IsSuccessStatusCode) return null;     // log + return

        var json = await res.Content.ReadAsStringAsync();
        var ors = JsonSerializer.Deserialize<OrsResponse>(json);

        var feat = ors?.Features?.FirstOrDefault();
        if (feat == null) return null;

        return new RouteInfo
        {
            RouteGeometry = feat.Geometry.GetRawText(),
            Distance      = feat.Properties.Summary.Distance,
            Duration      = feat.Properties.Summary.Duration
        };
    }

    
    
    public async Task<(double lon, double lat)?> GetCoordinatesFromAddressAsync(string address)
    {
        // URL-encode the address to handle spaces and special characters
        var encodedAddress = System.Web.HttpUtility.UrlEncode(address);
        var geocodeUrl = $"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={encodedAddress}&size=1";

        try
        {
            var response = await _http.GetAsync(geocodeUrl);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"ORS Geocoding Error: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            // We can reuse the OrsResponse model as the structure is similar for geocoding
            var orsResponse = JsonSerializer.Deserialize<OrsResponse>(jsonString);

            if (orsResponse?.Features != null && orsResponse.Features.Count > 0)
            {
                var geometry = orsResponse.Features[0].Geometry;
                // The coordinates are in a [lon, lat] array
                if (geometry.ValueKind == JsonValueKind.Object && geometry.TryGetProperty("coordinates", out var coords) && coords.GetArrayLength() == 2)
                {
                    var lon = coords[0].GetDouble();
                    var lat = coords[1].GetDouble();
                    return (lon, lat);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Geocoding exception: {ex.Message}");
        }

        return null;
    }
    
    
}