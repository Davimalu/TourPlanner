using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TourPlanner.Infrastructure;

namespace TourPlanner.DAL.ServiceAgents;

public class MapService
{
    private readonly HttpClient _http   = new();
    private readonly string     _apiKey;
    private readonly string     _profile;

    public MapService()
    {
        var cfg = new ConfigurationBuilder()
            //.AddJsonFile("config/appsettings.json", optional:false)
            .Build()
            .GetSection("OpenRouteService");

        _apiKey  = cfg["ApiKey"]  ?? throw new Exception("ORS key missing");
        _profile = cfg["Profile"] ?? "driving-car";
    }

    /// returns raw GeoJSON line-string
    public async Task<string> GetRouteAsync(string from, string to)
    {
        // 1) geocode plain text → Lon/Lat
        async Task<(double lon,double lat)> Geocode(string query)
        {
            var url=$"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(query)}";
            var doc = JsonDocument.Parse(await _http.GetStringAsync(url));
            var coords = doc.RootElement.GetProperty("features")[0]
                .GetProperty("geometry").GetProperty("coordinates");
            return (coords[0].GetDouble(), coords[1].GetDouble());
        }

        var (lon1,lat1) = await Geocode(from);
        var (lon2,lat2) = await Geocode(to);

        // 2) directions
        var dirUrl = $"https://api.openrouteservice.org/v2/directions/{_profile}" +
                     $"?api_key={_apiKey}&start={lon1},{lat1}&end={lon2},{lat2}";
        return await _http.GetStringAsync(dirUrl);   // GeoJSON string
    }
}