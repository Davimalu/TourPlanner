using TourPlanner.Model;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Structs;

namespace TourPlanner.DAL.Interfaces;

public interface IOrsService {
    Task<Route?> GetRouteAsync(Transport transportMode, GeoCoordinate start, GeoCoordinate end);
    Task<GeoCode?> GetGeoCodeFromAddressAsync(string address);
}