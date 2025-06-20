using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Structs;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents;

public class OrsService : IOrsService
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerWrapper _logger;
    
    private readonly string _apiKey;
    private readonly string _baseUrl;
    

    public OrsService(HttpClient http, ITourPlannerConfig tourPlannerConfig)
    {
        _httpClient = http ?? throw new ArgumentNullException(nameof(http));
        
        _logger = LoggerFactory.GetLogger<OrsService>();
        
        // Get the API key from the configuration file
        _apiKey = tourPlannerConfig.OpenRouteServiceApiKey ?? throw new ArgumentNullException(nameof(tourPlannerConfig.OpenRouteServiceApiKey), "OpenRouteService API key is not configured.");
        
        // Get the base URL from the configuration file
        _baseUrl = tourPlannerConfig.OpenRouteServiceBaseUrl ?? throw new ArgumentNullException(nameof(tourPlannerConfig.OpenRouteServiceBaseUrl), "OpenRouteService base URL is not configured.");
    }

    
    /// <summary>
    /// <para>maps the <see cref="Transport"/> enum to the OpenRouteService profile string</para>
    /// <para>(e.g. Transport.Car -> "driving-car")</para>
    /// </summary>
    /// <param name="transportType">The type of <see cref="Transport"/> to map</param>
    /// <returns>The corresponding OpenRouteService profile string</returns>
    public string GetOrsProfileForTransportType(Transport transportType)
    {
        // Mappings are available at https://giscience.github.io/openrouteservice-r/reference/ors_profile.html
        
        return transportType switch
        {
            Transport.Car => "driving-car",
            Transport.Truck => "driving-hgv",
            Transport.Bicycle => "cycling-regular",
            Transport.Roadbike => "cycling-road",
            Transport.Mountainbike => "cycling-mountain",
            Transport.ElectricBicycle => "cycling-electric",
            Transport.Walking => "foot-walking",
            Transport.Hiking => "foot-hiking",
            Transport.Wheelchair => "wheelchair",
            _ => "driving-car"
        };
    }

    
    /// <summary>
    /// Calculates the route between two geographical points (provided as <see cref="GeoCoordinate"/>s using the OpenRouteService API
    /// </summary>
    /// <param name="transportMode">The mode of transport to use for the route calculation</param>
    /// <param name="start">The starting geographical coordinate</param>
    /// <param name="end"> The ending geographical coordinate</param>
    /// <returns></returns>
    public async Task<Route?> GetRouteAsync(Transport transportMode, GeoCoordinate start, GeoCoordinate end)
    {
        string profile = GetOrsProfileForTransportType(transportMode);
        string url = $"{_baseUrl}/v2/directions/{profile}/geojson";
        
        // Create the request body
        var requestBody = JsonSerializer.Serialize(new
        {
            coordinates = new[] {
                new[] { start.Latitude, start.Longitude },
                new[] { end.Latitude, end.Longitude }
            }
        });
        
        // Create the complete HTTP request
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };
        
        request.Headers.Add("Authorization", _apiKey);
        
        // Send the request to the OpenRouteService API
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.Error($"ORS Route calculation failed with status code '{response.StatusCode}': {errorContent}");
            return null;
        }
        
        var jsonContent = await response.Content.ReadAsStringAsync();
        return ParseRouteResponse(jsonContent);
    }

    
    private Route? ParseRouteResponse(string json)
    {
        try
        {
            var orsResponse = JsonSerializer.Deserialize<OrsRouteResponse>(json);
            var feature = orsResponse?.Features?.FirstOrDefault();
            
            if (feature == null)
            {
                _logger.Warn("No features found in ORS route response");
                return null;
            }
            
            return new Route
            {
                RouteGeometry = feature.Geometry.GetRawText(),
                Distance = feature.Properties.RouteSummary.Distance,
                Duration = feature.Properties.RouteSummary.Duration
            };
        }
        catch (JsonException ex)
        {
            _logger.Error($"Failed to parse ORS route response: {ex.Message}", ex);
            return null;
        }
    }

    
    /// <summary>
    /// Asynchronously retrieves the GeoCode (geographical coordinates, latitude and longitude, and the place's label) for a given address using the OpenRouteService Geocoding API.
    /// </summary>
    /// <param name="address">The address to geocode</param>
    /// <returns>The best matching place as a <see cref="GeoCode"/> class, or null if the address is invalid or not found</returns>
    public async Task<GeoCode?> GetGeoCodeFromAddressAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.Warn("ORS Geocoding failed: Address is null or empty.");
            return null;
        }

        try
        {
            // UrlEncode converts a string into a format that can be safely included in a URL (handles spaces, special characters, etc.)
            var encodedAddress = System.Web.HttpUtility.UrlEncode(address);
            var geocodeUrl = $"{_baseUrl}/geocode/search?api_key={_apiKey}&text={encodedAddress}&size=1";
            
            // Make the API call to ORS Geocoding API
            var response = await _httpClient.GetAsync(geocodeUrl);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.Error($"ORS Geocoding failed with status code {response.StatusCode}: {errorContent}");
                return null;
            }
            
            // Get the response content as a JSON string
            var jsonString = await response.Content.ReadAsStringAsync();
            var orsResponse = JsonSerializer.Deserialize<OrsGeocodeResponse>(jsonString);
            
            return ExtractGeoCodeFromOrsResponse(orsResponse);
        }
        catch (Exception ex)
        {
            _logger.Error($"ORS Geocoding failed: {ex.Message}", ex);
            return null;
        }
    }
    
    
    /// <summary>
    /// Extracts the geographical coordinates + the location's label from the ORS geocoding response
    /// </summary>
    /// <param name="orsResponse">The deserialized ORS geocoding response containing features with geometry and properties</param>
    /// <returns>The location as a <see cref="GeoCode"/> class, or null if the response is invalid</returns>
    private GeoCode? ExtractGeoCodeFromOrsResponse(OrsGeocodeResponse? orsResponse)
    {
        if ( orsResponse?.Features == null || !orsResponse.Features.Any())
        {
            _logger.Warn("No features found in geocoding response");
            return null;
        }

        // Extract the coordinates from the first feature's geometry
        var geometry = orsResponse.Features[0].Geometry;
            
        if (geometry.ValueKind != JsonValueKind.Object || !geometry.TryGetProperty("coordinates", out var coords) || coords.GetArrayLength() != 2)
        {
            _logger.Warn("Invalid geometry structure in geocoding response");
            return null;
        }

        var coordsStruct = new GeoCoordinate(
            coords[0].GetDouble(),  // Longitude
            coords[1].GetDouble() // Latitude
        );
        
        return new GeoCode(orsResponse.Features[0].Properties.Label, coordsStruct);
    }
    
    
    // --- Models to communicate with the ORS API ---
    // Theoretically, these could be moved to a separate file for better organization, but since they are only really needed here, we keep them here for simplicity.
    
    /// <summary>
    /// <para>used to deserialize the GeoJSON response from ORS when geocoding an address</para>
    /// <list type="bullet">
    /// <item><description>One response from the ORS API contains a list of features</description></item>
    /// </list>
    /// </summary>
    private class OrsGeocodeResponse
    {
        [JsonPropertyName("features")]
        public List<OrsGeocodeFeature> Features { get; set; }
    }
    
    /// <summary>
    /// used to deserialize the features of a geocoded location from ORS
    /// <list type="bullet">
    /// <item><description>Each feature consists of a geometry and properties</description></item>
    /// <item><description>The geometry is a GeoJSON object containing the coordinates of the location</description></item>
    /// <item><description>The properties contain metadata about the location, such as Name, street, housenumber, postal code, city, ...</description></item>
    /// </list>
    /// </summary>
    private class OrsGeocodeFeature
    {
        [JsonPropertyName("geometry")]
        public JsonElement Geometry { get; set; }
        [JsonPropertyName("properties")]
        public OrsGeocodeProperties Properties { get; set; }
    }
    
    /// <summary>
    /// used to deserialize the properties of a geocoded location from ORS
    /// </summary>
    private class OrsGeocodeProperties
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("country")]
        public string Country { get; set; }
        [JsonPropertyName("region")]
        public string Region { get; set; }
        [JsonPropertyName("street")]
        public string Street { get; set; }
        [JsonPropertyName("housenumber")]
        public string HouseNumber { get; set; }
        [JsonPropertyName("postalcode")]
        public string PostalCode { get; set; }
    }

    /// <summary>
    /// used to deserialize the response from ORS when calculating a route
    /// </summary>
    private class OrsRouteResponse
    {
        [JsonPropertyName("features")]
        public List<OrsRouteFeature> Features { get; set; }
        [JsonPropertyName("bbox")]
        public List<double> BoundingBox { get; set; }
    }
    
    /// <summary>
    /// used to deserialize the features of a route from ORS
    /// <list type="bullet">
    /// <item><description>Each feature consists of a geometry and properties</description></item>
    /// <item><description>The geometry is a GeoJSON object containing the coordinates of the route</description></item>
    /// <item><description>The properties contain metadata about the route, such as distance and duration</description></item>
    /// </list>
    /// </summary>
    private class OrsRouteFeature
    {
        [JsonPropertyName("geometry")]
        public JsonElement Geometry { get; set; }
        [JsonPropertyName("properties")]
        public OrsRouteProperties Properties { get; set; }
    }
    
    /// <summary>
    /// used to deserialize the properties of a route from ORS
    /// </summary>
    private class OrsRouteProperties
    {
        [JsonPropertyName("summary")]
        public OrsRouteSummary RouteSummary { get; set; }
    }
    
    /// <summary>
    /// used to deserialize the summary of a route from ORS
    /// </summary>
    private class OrsRouteSummary
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; } // in meters
        [JsonPropertyName("duration")]
        public double Duration { get; set; } // in seconds
    }
    
}