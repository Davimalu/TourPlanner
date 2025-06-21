using TourPlanner.Model.Structs;

namespace TourPlanner.Model;

public class MapMarker
{
    public GeoCoordinate Coordinates { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public MapMarker(GeoCoordinate coordinates, string name, string description)
    {
        Coordinates = coordinates;
        Name = name;
        Description = description;
    }
}