using TourPlanner.Enums;

namespace TourPlanner.RestServer.Models
{
    public class TourLog
    {
        public int LogId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Comment { get; set; } = String.Empty;
        public int Difficulty { get; set; }
        public float DistanceTraveled { get; set; }
        public float TimeTaken { get; set; }
        public float Rating { get; set; }
    }
}