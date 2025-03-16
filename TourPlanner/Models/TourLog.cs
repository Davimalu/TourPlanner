using System;
using TourPlanner.Enums;

namespace TourPlanner.Models
{
    public class TourLog
    {
        public DateTime TimeStamp { get; set; }
        public string Comment { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; }
        public float DistanceTraveled { get; set; }
        public DateTime TimeTaken { get; set; }
        public Rating Rating { get; set; }
    }
}