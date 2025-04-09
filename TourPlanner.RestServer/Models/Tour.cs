using System.Collections.ObjectModel;
using TourPlanner.Enums;

namespace TourPlanner.RestServer.Models
{
    public class Tour
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = String.Empty;
        public string TourDescription { get; set; } = String.Empty;
        public string StartLocation { get; set; } = String.Empty;
        public string EndLocation { get; set; } = String.Empty;
        public Transport TransportationType { get; set; }
        public float Distance { get; set; }
        public float EstimatedTime { get; set; }
        public List<TourLog> Logs { get; set; }
    }
}
