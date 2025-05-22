using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class OpenRouteService
{
    private const string ApiKey = "DEIN_API_SCHLÜSSEL"; //API-Schlüssel
    private const string BaseUrl = "https://api.openrouteservice.org/v2/directions/";
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<RouteInfo> GetRouteInfo(string start, string end, string transportType)
    {
        //  transportType:  "driving-car", "cycling-regular", "foot-walking" usw.
        string url = $"{BaseUrl}{transportType}?start={start}&end={end}&api_key={ApiKey}";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Wirft eine Ausnahme, wenn der Aufruf nicht erfolgreich ist

            string json = await response.Content.ReadAsStringAsync();
            return ParseRouteResponse(json);
        }
        catch (HttpRequestException ex)
        {
            //  Behandle API-Anfragefehler (logge sie, wirf eine benutzerdefinierte Ausnahme usw.)
            Console.Error.WriteLine($"Fehler beim Abrufen der Route: {ex.Message}");
            return null;  // Oder wirf eine Ausnahme
        }
    }

    private RouteInfo ParseRouteResponse(string json)
    {
        JObject result = JObject.Parse(json);
        //  Extrahiere Distanz, Dauer und die Routengeometrie (Koordinaten) aus dem JSON
        //  Dieser Teil hängt von der Struktur der Open Routes Service-Antwort ab.
        //  Du musst das JSON inspizieren, um die richtigen Pfade zu finden.

        double distance = (double)result["features"][0]["properties"]["summary"]["distance"];
        double duration = (double)result["features"][0]["properties"]["summary"]["duration"];
        string geometry = result["features"][0]["geometry"]["coordinates"].ToString();  //  Beispiel: Passe dies an!

        return new RouteInfo
        {
            Distance = distance,
            Duration = duration,
            Geometry = geometry  //  Die Koordinaten der Route (für Leaflet)
        };
    }
}

public class RouteInfo
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public string Geometry { get; set; }  //  Für Leaflet
}