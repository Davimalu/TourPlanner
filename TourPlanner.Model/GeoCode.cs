using TourPlanner.Model.Structs;

namespace TourPlanner.Model;

public class GeoCode
{
    public string Label { get; set; }
    public GeoCoordinate Coordinates { get; set; }
    
    public GeoCode(string label, GeoCoordinate coordinates)
    {
        this.Label = label;
        Coordinates = coordinates;
    }
}