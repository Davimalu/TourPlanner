using TourPlanner.Model;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Structs;
using TourPlanner.Models;

namespace TourPlanner.DAL.Interfaces;

public interface IOrsService {
    string GetOrsProfileForTransportType(Transport transportType);
    Task<Route?> GetRouteAsync(Transport transportMode, GeoCoordinate start, GeoCoordinate end);
    Task<GeoCode?> GetGeoCodeFromAddressAsync(string address);
}