using System.Collections.ObjectModel;
using TourPlanner.Enums;

namespace TourPlanner.Models
{
    public class Tour
    {
        public string TourName { get; set; } = String.Empty;
        public string TourDescription { get; set; } = String.Empty;
        public string StartLocation { get; set; } = String.Empty;
        public string EndLocation { get; set; } = String.Empty;
        public Transport TransportationType { get; set; }
        public float Distance { get; set; }
        public DateTime EstimatedTime { get; set; }
        public string RouteInformation { get; set; } = String.Empty;

        public ObservableCollection<TourLog> Logs { get; set; } = new ObservableCollection<TourLog>();
    }
}
