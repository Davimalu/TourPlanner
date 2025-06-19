using TourPlanner.Enums;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents;

public interface IOSRService
{
    string GetProfileForTransportType(Transport transportType);

    /// returns structured route information including GeoJSON, distance, and duration
    Task<RouteInfo?> GetRouteAsync(
        Transport mode, (double lon, double lat) start, (double lon, double lat) end);

    Task<(double lon, double lat)?> GetCoordinatesFromAddressAsync(string address);
}